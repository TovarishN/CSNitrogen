using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;

public class ArrayArgumentConformanceTests
{
    [Theory]
    [InlineData("new int[1][2]")][InlineData("new int[1][][2]")]
    [InlineData("new int[]{1}[0]")][InlineData("new int[1]{1}[0]")]
    public void Direct_array_creation_cannot_be_an_element_access_target(string expression)
    {
        const string prefix = "class C{object M(){return ";
        using var parsed = CSharpParser.Parse(new(prefix + expression + ";}}", "CSharp1", "direct-array.cs", []));
        Assert.False(parsed.Accepted);
        var issue = Assert.Single(parsed.Issues, issue => issue.Code == "CSN2005");
        Assert.Equal(IssueCategory.Syntax, issue.Category);
        Assert.Equal(new TextSpan(prefix.Length, expression.Length), issue.Span);
    }

    [Theory]
    [InlineData("CSharp1")][InlineData("CSharp14")]
    public void Direct_array_target_rule_is_part_of_each_selected_baseline(string profile)
    {
        using var parsed = CSharpParser.Parse(new("class C{object M(){return new int[]{1}[0];}}", profile, "baseline-array.cs", []));
        Assert.False(parsed.Accepted); Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2005");
    }

    [Theory]
    [InlineData("(new int[1])[0]")][InlineData("(new int[]{1})[0]")]
    [InlineData("new int[1].P[0]")][InlineData("new int[]{1}.P[0]")]
    public void Parentheses_or_member_access_form_a_non_array_creation_target(string expression)
    {
        using var parsed = CSharpParser.Parse(new("class C{object M(){return " + expression + ";}}", "CSharp1", "array-target.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
    }

    [Theory]
    [InlineData("new int[]")][InlineData("new int[,]")][InlineData("new int[][1]")]
    [InlineData("new int[1,]")][InlineData("new int[,1]")][InlineData("new int[1")]
    [InlineData("new void[1]")][InlineData("new int[]{,}")][InlineData("new int[]{1,,2}")]
    [InlineData("new int[]{1,,}")][InlineData("new int[]{1;")]
    [InlineData("F(,)")][InlineData("F(x,)")][InlineData("F(ref)")][InlineData("F(out)")]
    [InlineData("F(ref out x)")][InlineData("F(ref x out y)")][InlineData("F({1,2})")]
    public void Malformed_arrays_initializers_and_arguments_are_rejected(string expression)
    {
        using var parsed = CSharpParser.Parse(new("class C{object M(){return " + expression + ";}}", "CSharp1", "bad-array.cs", []));
        Assert.False(parsed.Accepted);
    }

    [Theory]
    [InlineData("class C{int[] x={};}")][InlineData("class C{void M(){int[] x={1,};}}")]
    [InlineData("class C{int M(){return F(ref (x),out a[0]);}}")]
    [InlineData("class C{object M(){return new int[1]{1,2};}}")]
    public void Acceptance_does_not_assert_rank_size_or_ref_writability(string source)
    {
        using var parsed = CSharpParser.Parse(new(source, "CSharp1", "syntax-only.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
    }
}
