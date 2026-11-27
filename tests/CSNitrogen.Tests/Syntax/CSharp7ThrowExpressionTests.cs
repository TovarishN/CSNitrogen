using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CSharp7ThrowExpressionTests
{
    [Theory]
    [InlineData("=> throw e;")]
    [InlineData("=> x ?? throw e;")]
    [InlineData("=> b ? x : throw e;")]
    [InlineData("=> b ? throw e : x;")]
    [InlineData("=> N(() => throw e);")]
    [InlineData("=> throw (b ? e : f);")]
    [InlineData("=> throw (e = f);")]
    [InlineData("=> throw (from a in x select a);")]
    public void Throw_expressions_start_at_csharp7(string body)
    {
        using var old = Parse(body, "CSharp6");
        using var current = Parse(body, "CSharp7");
        Assert.False(old.Accepted);
        Assert.True(current.Accepted, string.Join(" | ", current.Issues));
        using var later = Parse(body, "CSharp14");
        Assert.True(later.Accepted, string.Join(" | ", later.Issues));
    }

    [Theory]
    [InlineData("=> x ?? (throw e);")]
    [InlineData("=> (throw e) ?? x;")]
    [InlineData("=> throw e ? x : y;")]
    [InlineData("=> N(throw e);")]
    [InlineData("=> x + throw e;")]
    [InlineData("=> throw throw e;")]
    [InlineData("{ return throw e; }")]
    [InlineData("{ x = throw e; return x; }")]
    public void Throw_expressions_require_an_allowed_direct_parent(string body)
    {
        using var parsed = Parse(body, "CSharp7");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2070");
    }

    [Theory]
    [InlineData("=> throw () => x;")]
    [InlineData("=> throw from a in x select a;")]
    public void Direct_lambda_and_query_are_outside_the_throw_operand_category(string body)
    {
        using var parsed = Parse(body, "CSharp7");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2071");
    }

    [Theory]
    [InlineData("=> throw e ?? f;")]
    [InlineData("=> x ?? throw e ?? f;")]
    public void Throw_operand_includes_coalescing_and_preserves_its_keyword(string body)
    {
        using var parsed = Parse(body, "CSharp7");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => ThrowExpressionExpression.Is(tree, i));
        var expression = ThrowExpressionExpression.Create(tree, index);
        Token keyword = expression.Keyword;
        Assert.Equal("throw", keyword.ToString());
        Assert.Equal("throw e ?? f", tree.GetText(index).ToString());
        Assert.True(NullCoalescingExpressionExpression.Is(tree, expression.Operand.Index));
    }

    [Fact]
    public void Historical_throw_statement_is_unchanged()
    {
        foreach (string profile in new[] { "CSharp1", "CSharp6", "CSharp7" })
        {
            using var parsed = Parse("{ throw e; }", profile);
            Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
            var tree = parsed.Tree;
            int jump = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => JumpStatementNode.Is(tree, i));
            Assert.Equal("throw e;", tree.GetText(jump).ToString());
            Assert.DoesNotContain(Enumerable.Range(0, tree.NodeCount), i => ThrowExpressionExpression.Is(tree, i));
        }
    }

    [Fact]
    public void Custom_composition_can_select_the_grammar_and_placement_policy()
    {
        var custom = CSharp6Composition.CreateBuilder().Add(ThrowExpressionsModule.Instance).Build();
        using var parsed = custom.Parse("throw e", ExpressionsModule.ExpressionFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        Assert.Contains(CSharpSyntaxPolicy.Check(parsed.Tree, new Version(7, 0)), issue => issue.Code == "CSN2070");
        Assert.DoesNotContain("Grammars/ThrowExpressions.ngr", ProfileResolver.Resolve("CSharp6").Profile!.Grammars);
        Assert.Contains("csharp7.throw-expression-contexts", ProfileResolver.Resolve("CSharp7").Profile!.Policies);
    }

    private static ParseDocument Parse(string body, string profile) =>
        CSharpParser.Parse(new($"class C {{ object M(bool b, object x, object y, E e, E f) {body} }}", profile, "throw.cs", []));
}
