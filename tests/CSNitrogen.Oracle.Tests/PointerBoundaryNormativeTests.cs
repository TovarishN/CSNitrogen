using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class PointerBoundaryNormativeTests
{
    private const string Definitions = "struct Leaf{public int X;}struct Nested{public Leaf Value;}enum E{A}";
    private static readonly Lazy<Language> Full = new(() => CSharp1Composition.CreateBuilder().Build());
    private static readonly Lazy<Language> Safe = new(() => CSharp1SafeComposition.CreateBuilder().Build());
    public static IEnumerable<object[]> PointerTypes()
    {
        string[] referents = ["sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "bool", "E", "Nested", "void"];
        foreach (string type in referents)
            foreach (int depth in new[] { 1, 2, 3 })
                foreach (string ranks in new[] { "", "[]", "[,][]" }) yield return [type + new string('*', depth) + ranks];
        yield return ["int /*a*/ **[/*rank*/,][]"];
        yield return [@"N\u0065sted /*p*/ * /*q*/ *"];
    }

    [Theory, MemberData(nameof(PointerTypes))]
    public void Historical_pointer_categories_preserve_declaration_cast_and_fragment_shapes(string type)
    {
        string source = Definitions + "unsafe class C{" + type + " f;" + type + " M(" + type + " p){return (" + type + ")p;}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var cls = Assert.Single(reference.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());
        var field = Assert.Single(cls.Members.OfType<FieldDeclarationSyntax>());
        var method = Assert.Single(cls.Members.OfType<MethodDeclarationSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pointer-categories.cs", [])); Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        foreach (TypeSyntax expected in new[] { field.Declaration.Type, method.ReturnType, method.ParameterList.Parameters[0].Type! })
        {
            int n = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => TypeSyntaxTypeReference.Is(ours.Tree, n) && ours.Tree.Span(n).Start == expected.Span.Start);
            CompareType(expected, SyntaxView.Cast<TypeSyntaxTypeReference>(ours.Tree, n));
        }
        var cast = Assert.Single(method.DescendantNodes().OfType<CastExpressionSyntax>());
        int castNode = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => CastExpressionExpression.Is(ours.Tree, n));
        ExpressionShapeAssert.Compare(cast, new ExpressionNode(ours.Tree, castNode));
        var fragmentReference = SyntaxFactory.ParseTypeName(type, options: new CSharpParseOptions(LanguageVersion.CSharp1), consumeFullText: true);
        Assert.False(fragmentReference.ContainsDiagnostics);
        using var fullFragment = Full.Value.Parse(type, NamesTypesModule.TypeFragment); Assert.True(fullFragment.Success && !fullFragment.HasErrors);
        var fragment = SyntaxView.Cast<TypeFragmentNode>(fullFragment.Tree, fullFragment.Tree.Root);
        CompareType(fragmentReference, SyntaxView.Cast<TypeSyntaxTypeReference>(fullFragment.Tree, fragment.Value.Index));
        using var pointerCast = Full.Value.Parse(type, UnsafeModule.PointerCastType); Assert.True(pointerCast.Success && !pointerCast.HasErrors);
        UnsafeShapeAssert.PointerCast(fragmentReference, SyntaxView.Cast<PointerCastTypeNode>(pointerCast.Tree, pointerCast.Tree.Root));
        using var safe = Safe.Value.Parse(type, NamesTypesModule.TypeFragment); Assert.False(safe.Success && !safe.HasErrors);
        Assert.DoesNotContain(Safe.Value.Modules, m => m.Name == "CSharp.Unsafe");
        Assert.DoesNotContain(Enumerable.Range(0, safe.Tree.NodeCount), n => SyntaxKinds.ModuleOf(safe.Tree.Kind(n)) == UnsafeModule.Instance.Id);
    }

    private static void CompareType(TypeSyntax expected, TypeSyntaxTypeReference actual)
    {
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        var ranks = expected is ArrayTypeSyntax a ? a.RankSpecifiers.ToArray() : [];
        UnsafeShapeAssert.PointerBase(expected is ArrayTypeSyntax array ? array.ElementType : expected, actual.Element);
        Assert.Equal(ranks.Length, actual.Ranks.Count);
        for (int i = 0; i < ranks.Length; i++)
        {
            ParameterShapeAssert.Span(ranks[i].Span, actual.Ranks[i].Span);
            var commas = ranks[i].Sizes.GetSeparators().ToArray(); Assert.Equal(commas.Length, actual.Ranks[i].Commas.Count);
            for (int c = 0; c < commas.Length; c++) ParameterShapeAssert.Span(commas[c].Span, actual.Ranks[i].Commas[c].Span);
        }
    }

    public static IEnumerable<object[]> AllocationReferents() => new[] { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "bool", "E", "Nested", "int*", "void*" }.Select(t => new object[] { t });

    [Theory, MemberData(nameof(AllocationReferents))]
    public void Historical_unmanaged_categories_are_recognized_as_allocation_elements(string type)
    {
        string source = Definitions + "unsafe class C{void M(){" + type + "* p=stackalloc " + type + "[4];}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.Tree.GetRoot().DescendantNodes().OfType<StackAllocArrayCreationExpressionSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "allocation-category.cs", [])); Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        int n = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => StackAllocExpressionExpression.Is(ours.Tree, n));
        var actual = SyntaxView.Cast<StackAllocExpressionExpression>(ours.Tree, n);
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        var array = Assert.IsType<ArrayTypeSyntax>(expected.Type); UnsafeShapeAssert.PointerBase(array.ElementType, actual.Type);
        ExpressionShapeAssert.Compare(Assert.Single(Assert.Single(array.RankSpecifiers).Sizes), actual.Size);
    }

    [Theory]
    [InlineData("", "string")]
    [InlineData("", "object")]
    [InlineData("class Managed{}", "Managed")]
    [InlineData("struct Managed{public object Value;}", "Managed")]
    public void Managed_referent_legality_is_not_inferred_by_the_parser(string definition, string type)
    {
        string source = definition + "unsafe class C{" + type + "* f;}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8500" && d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "managed-referent.cs", [])); Assert.True(ours.Accepted);
    }

    [Theory]
    [InlineData("class C{int* f;}")]
    [InlineData("class C{int* M(int* p){return p;}}")]
    public void Unsafe_context_legality_remains_a_compiler_observation(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS0214");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "unsafe-context.cs", [])); Assert.True(ours.Accepted);
    }

    [Theory]
    [InlineData("int**?")]
    [InlineData("void")]
    [InlineData("*int")]
    [InlineData("N.T.*")]
    [InlineData("int*[,1]")]
    public void Malformed_pointer_type_fragments_require_full_consumption(string type)
    {
        using var ours = Full.Value.Parse(type, NamesTypesModule.TypeFragment); Assert.False(ours.Success && !ours.HasErrors);
    }
}
