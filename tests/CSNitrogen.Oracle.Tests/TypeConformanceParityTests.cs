using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class TypeConformanceParityTests
{
    public static IEnumerable<object[]> TypeShapes()
    {
        string[] stems = ["bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
            "char", "float", "double", "decimal", "string", "object", "A", "N.T", "N.Outer.Inner",
            "@int.@void", @"N.cl\u0061ss", "var", "dynamic.async"];
        string[] ranks = ["[]", "[,]", "[,,]", "[,,,]"];
        foreach (string stem in stems)
        {
            yield return [stem];
            foreach (string first in ranks)
            {
                yield return [stem + first];
                foreach (string second in ranks) yield return [stem + first + second];
            }
        }
        yield return ["N /*a*/ . @T [/*b*/,] [ ]"];
        yield return ["int [ , /*rank*/ , ]"];
        yield return [@"@void.\u0069nt[]"];
    }

    [Theory, MemberData(nameof(TypeShapes))]
    public void Source_type_children_and_physical_spans_match(string typeText)
    {
        string source = "class C{" + typeText + " field;}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>()).Declaration.Type;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "types.cs", []));
        Assert.True(ours.Accepted, typeText + " :: " + string.Join(";", ours.Issues));
        var tree = ours.Tree;
        int node = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => TypeSyntaxTypeReference.Is(tree, i));
        var actual = SyntaxView.Cast<TypeSyntaxTypeReference>(tree, node);
        SameSpan(expected.Span, actual.Span);
        var expectedRanks = expected is ArrayTypeSyntax array ? array.RankSpecifiers.ToArray() : [];
        Assert.Equal(expectedRanks.Length, actual.Ranks.Count);
        for (int r = 0; r < expectedRanks.Length; r++)
        {
            var rank = actual.Ranks[r]; var oracleRank = expectedRanks[r];
            SameSpan(oracleRank.Span, rank.Span);
            var commas = oracleRank.Sizes.GetSeparators().ToArray();
            Assert.Equal(commas.Length, rank.Commas.Count);
            for (int c = 0; c < commas.Length; c++) SameSpan(commas[c].Span, rank.Commas[c].Span);
        }
        TypeSyntax element = expected is ArrayTypeSyntax a ? a.ElementType : expected;
        if (element is PredefinedTypeSyntax builtin)
        {
            var baseType = SyntaxView.Cast<BuiltinTypeNameTypeBase>(tree, actual.Element.Index);
            SameSpan(builtin.Span, baseType.Value.Span);
            Assert.Equal(builtin.Keyword.Text, source.Substring(baseType.Value.Span.Start, baseType.Value.Span.Length));
        }
        else
        {
            var name = SyntaxView.Cast<NamedTypeNameTypeBase>(tree, actual.Element.Index).Name;
            SameSpan(element.Span, name.Span);
            var identifiers = element.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken)).ToArray();
            Assert.Equal(identifiers.Length, name.Parts.Count);
            for (int i = 0; i < identifiers.Length; i++)
            {
                Assert.Equal(identifiers[i].Text, name.Parts[i].ToString());
                SameSpan(identifiers[i].Span, name.Parts[i].Span);
            }
        }
    }

    [Theory]
    [InlineData("A.")][InlineData(".A")][InlineData("A..B")][InlineData("int.X")]
    [InlineData("A.int")][InlineData("A[")][InlineData("A]")]
    [InlineData("A[,1]")][InlineData("A[][]tail")][InlineData("A B")]
    public void Malformed_type_fragments_are_rejected_by_both(string source)
    {
        var reference = SyntaxFactory.ParseTypeName(source, options: new CSharpParseOptions(LanguageVersion.CSharp1), consumeFullText: true);
        Assert.True(reference.ContainsDiagnostics, source);
        var language = CSharp1SafeComposition.CreateBuilder().Build();
        using var ours = language.Parse(source, NamesTypesModule.TypeFragment);
        Assert.False(ours.Success && !ours.HasErrors);
    }

    [Theory]
    [InlineData("class C{A<T> field;}")][InlineData("class C{global::A field;}")]
    [InlineData("class C{int? field;}")]
    public void Later_type_syntax_has_a_CSharp1_availability_error(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8022");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "later-types.cs", []));
        Assert.False(ours.Accepted);
    }

    [Theory]
    [InlineData("class C{void M(){} delegate void D(); object T(){return typeof(void);}}", true)]
    [InlineData("interface I{void M();N.T[,][] P{get;}}", true)]
    [InlineData("class C{void[] x;}", false)]
    [InlineData("class C{void M(void x){}}", false)]
    [InlineData("class C{object M(){return new void();}}", false)]
    [InlineData("class C{object M(){return typeof(void[]);}}", false)]
    [InlineData("class C{void M(){void x;}}", false)]
    public void Void_contexts_match_syntax_observations(string source, bool expected)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        bool accepted = !reference.SyntaxDiagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.True(expected == accepted, source + " :: " + string.Join(";", reference.CompilationDiagnostics.Select(d => d.Id + ":" + d.GetMessage())));
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "void.cs", []));
        Assert.Equal(expected, ours.Accepted);
    }

    [Theory]
    [InlineData("class C{void x;}", "CS0670")]
    [InlineData("class C{void P{get{return;}}}", "CS0547")]
    public void Compiler_deferred_void_context_errors_are_explicit(string source, string diagnostic)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "void.cs", []));
        Assert.False(ours.Accepted);
    }

    [Fact]
    public void Array_size_is_not_a_declaration_rank_even_when_type_fragment_oracle_accepts_it()
    {
        var fragment = SyntaxFactory.ParseTypeName("A[1]", options: new CSharpParseOptions(LanguageVersion.CSharp1), consumeFullText: true);
        Assert.False(fragment.ContainsDiagnostics);
        var reference = Oracle.Observe("class C{A[1] field;}", LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS0270");
        using var ours = CSharpParser.Parse(new("class C{A[1] field;}", "CSharp1", "rank.cs", []));
        Assert.False(ours.Accepted);
    }

    private static void SameSpan(Microsoft.CodeAnalysis.Text.TextSpan expected, Nitrogen.TextSpan actual)
    { Assert.Equal(expected.Start, actual.Start); Assert.Equal(expected.Length, actual.Length); }
}
