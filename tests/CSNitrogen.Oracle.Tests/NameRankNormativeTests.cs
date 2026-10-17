using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class NameRankNormativeTests
{
    private static string Wrap(string context, string name) => context switch
    {
        "using" => "using " + name + "; class C{}",
        "alias" => "using Alias = " + name + "; class C{}",
        "namespace" => "namespace " + name + "{}",
        "base" => "class C:" + name + "{}",
        _ => "class C{" + name + " f;}"
    };

    public static IEnumerable<object[]> Names()
    {
        foreach (string context in new[] { "using", "alias", "namespace", "base", "field" })
            foreach (string name in new[] { "N", "N.Outer.Inner", "@int.@void", @"N.cl\u0061ss", "var.dynamic.async", "N/*a*/ . @T" })
                yield return [context, name];
    }

    [Theory, MemberData(nameof(Names))]
    public void Qualified_name_tokens_and_spans_match_in_each_owning_context(string context, string name)
    {
        string source = Wrap(context, name);
        var oracle = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(oracle.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "name-context.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => QualifiedNameNode.Is(ours.Tree, n));
        var actual = SyntaxView.Cast<QualifiedNameNode>(ours.Tree, node);
        int start = source.IndexOf(name, StringComparison.Ordinal);
        Assert.Equal(new Nitrogen.TextSpan(start, name.Length), actual.Span);
        var expected = oracle.GetRoot().DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken) && t.Span.Start >= start && t.Span.End <= start + name.Length).ToArray();
        Assert.Equal(expected.Length, actual.Parts.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Text, actual.Parts[i].ToString());
            Assert.Equal(new Nitrogen.TextSpan(expected[i].Span.Start, expected[i].Span.Length), actual.Parts[i].Span);
        }
    }

    public static IEnumerable<object[]> InvalidNames()
    {
        foreach (string context in new[] { "using", "alias", "namespace", "base", "field" })
            foreach (string name in new[] { ".N", "N.", "N..T", "N.int", "int.X" })
                yield return [context, name];
    }

    [Theory, MemberData(nameof(InvalidNames))]
    public void Broken_name_syntax_is_rejected_in_each_owning_context(string context, string name)
    {
        string source = Wrap(context, name);
        var oracle = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.Contains(oracle.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "bad-name.cs", []));
        Assert.False(ours.Accepted);
        int start = source.IndexOf(name, StringComparison.Ordinal);
        if (context == "field" && name == "N.")
        {
            // Trivia joins N. and the following f into a valid type name N.f;
            // the missing field identifier is diagnosed at the semicolon.
            Assert.Contains(ours.Issues, issue => issue.Code == "NG1" && issue.Span.Start == source.IndexOf(';'));
        }
        else Assert.Contains(ours.Issues, issue => issue.Category == IssueCategory.Syntax && issue.Severity == Nitrogen.DiagnosticSeverity.Error && issue.Span.Start >= start && issue.Span.Start <= start + name.Length);
    }

    public static IEnumerable<object[]> PredefinedTypes() => new[] { "bool", "byte", "char", "decimal", "double", "float", "int", "long", "object", "sbyte", "short", "string", "uint", "ulong", "ushort" }.Select(t => new object[] { t });

    [Theory, MemberData(nameof(PredefinedTypes))]
    public void Every_normative_predefined_type_has_its_member_access_alternative(string type)
    {
        string source = "class C{object M(){return " + type + ".Member;}}";
        var oracle = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(oracle.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(oracle.GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>());
        Assert.Equal(type, Assert.IsType<PredefinedTypeSyntax>(expected.Expression).Keyword.Text);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "predefined.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => PredefinedMemberExpressionExpression.Is(ours.Tree, n));
        var actual = SyntaxView.Cast<PredefinedMemberExpressionExpression>(ours.Tree, node);
        Assert.Equal(new Nitrogen.TextSpan(expected.Expression.Span.Start, type.Length), actual.Type.Span);
        Assert.Equal(new Nitrogen.TextSpan(expected.Name.Span.Start, expected.Name.Span.Length), actual.Name.Span);
    }

    [Theory]
    [InlineData("void")][InlineData("Void")][InlineData("@int")]
    public void Void_is_excluded_and_named_spellings_do_not_become_predefined_types(string type)
    {
        string source = "class C{object M(){return " + type + ".Member;}}";
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "predefined-boundary.cs", []));
        Assert.Equal(type != "void", ours.Accepted);
        Assert.DoesNotContain(Enumerable.Range(0, ours.Tree.NodeCount), n => PredefinedMemberExpressionExpression.Is(ours.Tree, n));
    }

    [Theory]
    [InlineData("N.T[1]")][InlineData("N.T[,1]")][InlineData("N.T[,")]
    public void Malformed_rank_has_a_physical_syntax_diagnostic(string type)
    {
        string source = "#line 90 \"mapped.cs\"\nclass C{\n " + type + " f;}";
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "rank.cs", []));
        Assert.False(ours.Accepted);
        int start = source.IndexOf('[', StringComparison.Ordinal);
        Assert.Contains(ours.Issues, issue => issue.Category == IssueCategory.Syntax && issue.Span.Start >= start && issue.Span.Start <= source.IndexOf(" f;", StringComparison.Ordinal) + 1);
    }

    public static IEnumerable<object[]> RankedOwners()
    {
        foreach (string owner in new[] { "field", "parameter", "return", "typeof" })
            foreach (string type in new[] { "N.T[,][][,,]", "int[]", "int[,,,,,,,]" }) yield return [owner, type];
    }

    [Theory, MemberData(nameof(RankedOwners))]
    public void Rank_helpers_preserve_order_and_comma_spans_across_owning_contexts(string owner, string type)
    {
        string source = owner switch
        {
            "parameter" => "class C{void M(" + type + " p){}}",
            "return" => "class C{" + type + " M(){return null;}}",
            "typeof" => "class C{object M(){return typeof(" + type + ");}}",
            _ => "class C{" + type + " f;}"
        };
        var oracle = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(oracle.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(oracle.GetRoot().DescendantNodes().OfType<ArrayTypeSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "rank-owner.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => TypeSyntaxTypeReference.Is(ours.Tree, n) && ours.Tree.Span(n).Start == expected.Span.Start);
        var actual = SyntaxView.Cast<TypeSyntaxTypeReference>(ours.Tree, node);
        Assert.Equal(new Nitrogen.TextSpan(expected.Span.Start, expected.Span.Length), actual.Span);
        Assert.Equal(expected.RankSpecifiers.Count, actual.Ranks.Count);
        for (int r = 0; r < actual.Ranks.Count; r++)
        {
            var rank = expected.RankSpecifiers[r];
            Assert.Equal(new Nitrogen.TextSpan(rank.Span.Start, rank.Span.Length), actual.Ranks[r].Span);
            var commas = rank.Sizes.GetSeparators().ToArray();
            Assert.Equal(commas.Length, actual.Ranks[r].Commas.Count);
            for (int c = 0; c < commas.Length; c++) Assert.Equal(new Nitrogen.TextSpan(commas[c].Span.Start, commas[c].Span.Length), actual.Ranks[r].Commas[c].Span);
        }
    }
}
