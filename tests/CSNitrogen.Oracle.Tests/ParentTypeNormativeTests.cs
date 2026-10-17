using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class ParentTypeNormativeTests
{
    [Theory]
    [InlineData("int[]*")]
    [InlineData("N.T[,]*")]
    [InlineData("int*[]*")]
    [InlineData("void[]")]
    public void Historical_invalid_array_pointer_boundary_retains_its_oracle_phase_difference(string type)
    {
        string source = "unsafe class C{" + type + " f;}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "array-pointer.cs", []));
        if (type == "void[]") Assert.Contains(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        else
        {
            Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
            Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8500" && d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
        }
        Assert.False(ours.Accepted);
    }

    private static readonly Lazy<Language> Safe = new(() => CSharp1SafeComposition.CreateBuilder().Build());

    public static IEnumerable<object[]> TypeOwners()
    {
        string[] builtin = ["bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "string", "object"];
        var types = builtin.Concat(new[] { "N.T", "@int.@void", "var.dynamic" }.SelectMany(t => new[] { t, t + "[]", t + "[,][]" }))
            .Concat(new[] { "int*", "N.T**", "void*", "void**" }.SelectMany(t => new[] { t, t + "[]", t + "[,][]" }));
        foreach (string type in types)
            foreach (string owner in new[] { "field", "local", "parameter", "return", "property", "indexer", "delegate", "interface", "typeof" }) yield return [owner, type];
    }

    private static (string Prefix, string Suffix) Owner(string owner) => owner switch
    {
        "local" => ("class C{void M(){", " v;}}"),
        "parameter" => ("class C{void M(", " p){}}"),
        "return" => ("class C{", " M(){return null;}}"),
        "property" => ("class C{", " P{get{return null;}}}"),
        "indexer" => ("class C{", " this[int i]{get{return null;}}}"),
        "delegate" => ("delegate ", " D();"),
        "interface" => ("interface I{", " M();}"),
        "typeof" => ("class C{object M(){return typeof(", ");}}"),
        _ => ("class C{", " f;}")
    };

    [Theory, MemberData(nameof(TypeOwners))]
    public void Type_base_and_rank_children_match_in_each_declaration_and_typeof_owner(string owner, string type)
    {
        var (prefix, suffix) = Owner(owner); string source = prefix + type + suffix;
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<TypeSyntax>(), t => t.Span.Start == prefix.Length && t.Span.Length == type.Length);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "type-owner.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => TypeSyntaxTypeReference.Is(ours.Tree, n) && ours.Tree.Span(n).Start == prefix.Length);
        var actual = SyntaxView.Cast<TypeSyntaxTypeReference>(ours.Tree, node);
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        var ranks = expected is ArrayTypeSyntax array ? array.RankSpecifiers.ToArray() : [];
        TypeSyntax element = expected is ArrayTypeSyntax a ? a.ElementType : expected;
        UnsafeShapeAssert.PointerBase(element, actual.Element);
        Assert.Equal(ranks.Length, actual.Ranks.Count);
        for (int r = 0; r < ranks.Length; r++)
        {
            ParameterShapeAssert.Span(ranks[r].Span, actual.Ranks[r].Span);
            var commas = ranks[r].Sizes.GetSeparators().ToArray(); Assert.Equal(commas.Length, actual.Ranks[r].Commas.Count);
            for (int c = 0; c < commas.Length; c++) ParameterShapeAssert.Span(commas[c].Span, actual.Ranks[r].Commas[c].Span);
        }
        using var safe = Safe.Value.Parse(source, CSharp1SafeComposition.Start);
        bool safeAccepted = safe.Success && !safe.HasErrors && !CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0)).Any(i => i.Severity == Nitrogen.DiagnosticSeverity.Error);
        Assert.Equal(!type.Contains('*'), safeAccepted);
        Assert.DoesNotContain(Enumerable.Range(0, safe.Tree.NodeCount), n => SyntaxKinds.ModuleOf(safe.Tree.Kind(n)) == UnsafeModule.Instance.Id);
    }

    [Theory]
    [InlineData("class Named{}", "Named")]
    [InlineData("struct Named{}", "Named")]
    [InlineData("enum Named{A}", "Named")]
    [InlineData("interface Named{}", "Named")]
    [InlineData("delegate void Named();", "Named")]
    public void Named_value_reference_enum_interface_and_delegate_types_share_syntax(string declaration, string name)
    {
        string source = declaration + "class C{" + name + " f;}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "named-kind.cs", []));
        Assert.True(ours.Accepted);
        int start = declaration.Length + "class C{".Length;
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => TypeSyntaxTypeReference.Is(ours.Tree, n) && ours.Tree.Span(n).Start == start);
        var actual = SyntaxView.Cast<TypeSyntaxTypeReference>(ours.Tree, node);
        var named = SyntaxView.Cast<NamedTypeNameTypeBase>(ours.Tree, actual.Element.Index);
        Assert.Equal(1, named.Name.Parts.Count);
        Assert.Equal(name, named.Name.Parts[0].ToString());
    }

    [Theory]
    [InlineData("class C{", " M(){}}")]
    [InlineData("delegate ", " D();")]
    [InlineData("interface I{", " M();}")]
    [InlineData("class C{object M(){return typeof(", ");}}")]
    public void Void_uses_the_dedicated_return_type_alternative(string prefix, string suffix)
    {
        string source = prefix + "void" + suffix;
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "void-owner.cs", [])); Assert.True(ours.Accepted);
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => ReturnTypeNode.Is(ours.Tree, n) && ours.Tree.Span(n).Start == prefix.Length);
        Assert.Equal(new Nitrogen.TextSpan(prefix.Length, 4), ours.Tree.Span(node));
        Assert.DoesNotContain(Enumerable.Range(0, ours.Tree.NodeCount), n => TypeSyntaxTypeReference.Is(ours.Tree, n) && ours.Tree.Span(n).Start == prefix.Length);
    }

    [Theory]
    [InlineData("int?", "CS8022")]
    [InlineData("N<T>", "CS8022")]
    [InlineData("global::N.T", "CS8022")]
    [InlineData("void", "CS0670")]
    [InlineData("int[1]", "CS0270")]
    public void Later_or_compiler_deferred_type_forms_keep_their_phase_classification(string type, string diagnostic)
    {
        string source = "class C{" + type + " f;}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "type-boundary.cs", [])); Assert.False(ours.Accepted);
    }
}
