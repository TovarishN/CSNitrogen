using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CSharp7NumericLiteralTests
{
    [Theory]
    [InlineData("0b1010UL")]
    [InlineData("0B1__01")]
    [InlineData("0xAC_BdLu")]
    [InlineData("1_2__3")]
    [InlineData("1_2.3_4F")]
    [InlineData("1_2.34e+5_6")]
    [InlineData(".3e-5_6F")]
    [InlineData("1_2e3")]
    [InlineData("12e3_4")]
    [InlineData("1_2m")]
    public void New_spellings_start_in_csharp70(string literal)
    {
        AssertLiteral(literal, "CSharp6", false);
        AssertLiteral(literal, "CSharp7", true);
        AssertLiteral(literal, "CSharp7_1", true);
        AssertLiteral(literal, "CSharp14", true);
    }

    [Theory]
    [InlineData("0x__1_2UL")]
    [InlineData("0b_1__0LU")]
    public void Separators_after_the_prefix_start_in_csharp72(string literal)
    {
        AssertLiteral(literal, "CSharp7_1", false);
        AssertLiteral(literal, "CSharp7_2", true);
        AssertLiteral(literal, "CSharp7_3", true);
        AssertLiteral(literal, "CSharp8", true);
    }

    [Theory]
    [InlineData("0b")]
    [InlineData("0b_")]
    [InlineData("0b12")]
    [InlineData("0x_")]
    [InlineData("0b_2")]
    [InlineData("1_")]
    [InlineData("1_2_")]
    [InlineData("1_.2")]
    [InlineData("1.2_")]
    [InlineData("1.2e_3")]
    [InlineData("1.2e3_")]
    [InlineData("1_2ulL")]
    [InlineData("1_ 2")]
    public void Rejects_invalid_numeric_spellings(string literal) =>
        AssertLiteral(literal, "CSharp7_3", false);

    [Theory]
    [InlineData("12")]
    [InlineData("12.34")]
    [InlineData("0xAC")]
    public void Ordinary_literals_keep_their_token_api(string literal)
    {
        using var parsed = CSharpParser.Parse(new($"class C {{ object F = {literal}; }}", "CSharp7_3", "numbers.cs", []));
        Assert.True(parsed.Accepted);
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => LiteralExpressionExpression.Is(tree, i));
        Token value = LiteralExpressionExpression.Create(tree, index).Value;
        Assert.Equal(literal, value.ToString());
    }

    [Theory]
    [InlineData("0b1_0", false)]
    [InlineData("12.3_4e5_6F", false)]
    [InlineData("0x__F", true)]
    public void New_spellings_have_their_own_token_valued_nodes(string literal, bool leadingSeparator)
    {
        string source = $"class C {{ object F = (T){literal}; }}";
        using var parsed = CSharpParser.Parse(new(source, "CSharp7_3", "numbers.cs", []));
        Assert.True(parsed.Accepted);
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => leadingSeparator
            ? LeadingSeparatorNumericLiteralExpressionExpression.Is(tree, i)
            : NumericLiteralExpressionExpression.Is(tree, i));
        Token value = leadingSeparator
            ? LeadingSeparatorNumericLiteralExpressionExpression.Create(tree, index).Value
            : NumericLiteralExpressionExpression.Create(tree, index).Value;
        Assert.Equal(literal, value.ToString());
        Assert.Equal(new TextSpan(source.IndexOf(literal, StringComparison.Ordinal), literal.Length), value.Span);
        Assert.Contains(Enumerable.Range(0, tree.NodeCount), i => CastExpressionExpression.Is(tree, i));
    }

    [Fact]
    public void Modules_compose_and_keep_the_existing_literal_fragment()
    {
        var first = CSharp6Composition.CreateBuilder().Add(NumericLiteralsModule.Instance).Build();
        var second = CSharp6Composition.CreateBuilder().Add(NumericLiteralsModule.Instance)
            .Add(LeadingDigitSeparatorsModule.Instance).Build();
        using var numeric = first.Parse("1_2", ExpressionsModule.ExpressionFragment);
        using var leadingRejected = first.Parse("0x_F", ExpressionsModule.ExpressionFragment);
        using var leadingAccepted = second.Parse("0x_F", ExpressionsModule.ExpressionFragment);
        using var legacy = second.Parse("1_2", ExpressionsModule.Literal);
        Assert.True(numeric.Success && !numeric.HasErrors);
        Assert.False(leadingRejected.Success && !leadingRejected.HasErrors);
        Assert.True(leadingAccepted.Success && !leadingAccepted.HasErrors);
        Assert.False(legacy.Success && !legacy.HasErrors);

        var holes = new Dictionary<string, QuoteHole>();
        using var expressionQuote = second.CompileQuote("CSharp.Expressions.Expression", "0x_F", holes);
        using var literalQuote = second.CompileQuote("CSharp.Expressions.Literal", "0x_F", holes);
        using var historicalLiteralQuote = second.CompileQuote("CSharp.Expressions.Literal", "0xF", holes);
        Assert.True(expressionQuote.Success);
        Assert.False(literalQuote.Success);
        Assert.True(historicalLiteralQuote.Success);

        var firstProfile = ProfileResolver.Resolve("CSharp7_1").Profile!;
        Assert.Contains("Grammars/NumericLiterals.ngr", firstProfile.Grammars);
        Assert.DoesNotContain("Grammars/LeadingDigitSeparators.ngr", firstProfile.Grammars);
        Assert.Contains("Grammars/LeadingDigitSeparators.ngr", ProfileResolver.Resolve("CSharp7_2").Profile!.Grammars);
    }

    [Theory]
    [InlineData("1_2")]
    [InlineData(".3_4")]
    [InlineData("0x_F")]
    public void A_dot_followed_by_an_identifier_stays_in_member_access(string literal)
    {
        var source = $"class C {{ object F = {literal}.F; }}";
        using var parsed = CSharpParser.Parse(new(source, "CSharp7_2", "numbers.cs", []));
        Assert.True(parsed.Accepted);
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => MemberExpressionExpression.Is(tree, i));
        var member = MemberExpressionExpression.Create(tree, index);
        Assert.Equal(literal, tree.GetText(member.Target.Index).ToString());
        Assert.Equal("F", member.Name.ToString());
    }

    private static void AssertLiteral(string literal, string profile, bool accepted)
    {
        string source = $"class C {{ object F = {literal}; }}";
        using var parsed = CSharpParser.Parse(new(source, profile, "numbers.cs", []));
        Assert.True(accepted == parsed.Accepted, $"{profile} parsing {literal}: {string.Join(" | ", parsed.Issues)}");
    }
}
