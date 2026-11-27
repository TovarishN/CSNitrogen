using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CSharp71DefaultLiteralTests
{
    [Theory]
    [InlineData("class C { int F = default; }")]
    [InlineData("class C { int M() { return default; } }")]
    [InlineData("class C { void M(int value = default) {} }")]
    [InlineData("class C { int M(bool b) => b ? default : 1; }")]
    [InlineData("class C { int M() => (int)default; }")]
    [InlineData("class C { T M<T>() => (T)default; }")]
    [InlineData("class C { int M() => N(default); int N(int a) => a; }")]
    public void Untyped_default_starts_at_csharp71(string source)
    {
        using var before = Parse(source, "CSharp7");
        Assert.False(before.Accepted);
        foreach (string profile in new[] { "CSharp7_1", "CSharp7_2", "CSharp7_3", "CSharp14" })
        {
            using var after = Parse(source, profile);
            Assert.True(after.Accepted, string.Join(" | ", after.Issues));
            var tree = after.Tree;
            int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => DefaultLiteralExpressionExpression.Is(tree, i));
            Token keyword = DefaultLiteralExpressionExpression.Create(tree, index).Keyword;
            Assert.Equal("default", keyword.ToString());
            Assert.Equal(new TextSpan(source.IndexOf("default", StringComparison.Ordinal), 7), keyword.Span);
        }
    }

    [Theory]
    [InlineData("default()")]
    [InlineData("default(1)")]
    [InlineData("default /* trivia */ (1)")]
    public void Invalid_typed_default_cannot_fall_back_to_invocation(string expression)
    {
        using var document = Parse($"class C {{ object F = {expression}; }}", "CSharp7_1");
        Assert.False(document.Accepted);
    }

    [Fact]
    public void Typed_default_and_switch_label_keep_their_existing_nodes()
    {
        const string source = "class C { int M(int x) { switch (x) { default: return default /* trivia */ (int); } } }";
        foreach (string profile in new[] { "CSharp2", "CSharp7", "CSharp7_1" })
        {
            using var parsed = Parse(source, profile);
            Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
            var tree = parsed.Tree;
            Assert.Single(Enumerable.Range(0, tree.NodeCount), i => DefaultValueExpressionExpression.Is(tree, i));
            Assert.DoesNotContain(Enumerable.Range(0, tree.NodeCount), i => DefaultLiteralExpressionExpression.Is(tree, i));
            int label = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => SwitchLabelNode.Is(tree, i));
            Assert.Equal("default:", tree.GetText(label).ToString());
        }
    }

    [Fact]
    public void Default_is_a_primary_expression_and_keyword_prefixes_stay_identifiers()
    {
        const string source = "class C { object M(object defaulted) => default.Member ?? defaulted; }";
        using var parsed = Parse(source, "CSharp7_1");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => MemberExpressionExpression.Is(tree, i));
        var member = MemberExpressionExpression.Create(tree, index);
        Assert.True(DefaultLiteralExpressionExpression.Is(tree, member.Target.Index));
        Assert.Equal("Member", member.Name.ToString());
        Assert.Contains(Enumerable.Range(0, tree.NodeCount), i => NameExpressionExpression.Is(tree, i)
            && NameExpressionExpression.Create(tree, i).Name.ToString() == "defaulted");
    }

    [Fact]
    public void Default_literal_can_be_selected_independently_in_a_composition()
    {
        var baseline = CSharp7Composition.CreateBuilder().Build();
        var custom = CSharp7Composition.CreateBuilder().Add(DefaultLiteralsModule.Instance).Build();
        using var before = baseline.Parse("default", ExpressionsModule.ExpressionFragment);
        using var after = custom.Parse("default", ExpressionsModule.ExpressionFragment);
        Assert.False(before.Success && !before.HasErrors);
        Assert.True(after.Success && !after.HasErrors);
        Assert.DoesNotContain("Grammars/DefaultLiterals.ngr", ProfileResolver.Resolve("CSharp7").Profile!.Grammars);
        Assert.Contains("Grammars/DefaultLiterals.ngr", ProfileResolver.Resolve("CSharp7_1").Profile!.Grammars);
    }

    private static ParseDocument Parse(string source, string profile) =>
        CSharpParser.Parse(new(source, profile, "default.cs", []));
}
