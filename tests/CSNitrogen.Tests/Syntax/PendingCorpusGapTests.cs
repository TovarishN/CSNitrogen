using CSNitrogen.Parsing;
using Xunit;
namespace CSNitrogen.Tests.Syntax;
public class PendingCorpusGapTests
{
    [Theory]
    [InlineData("a+b=c")][InlineData("a*b+=c")][InlineData("a<b=c")][InlineData("a||b=c")]
    public void Assignment_target_must_have_a_unary_expression_root(string expression)
    {
        string source="class C{void M(){"+expression+";}}";
        using var result=CSharpParser.Parse(new(source,"CSharp1","assignment.cs",[]));
        Assert.False(result.Accepted);
        Assert.Contains(result.Issues,i=>i.Code=="CSN2013"&&i.Span.Start==source.IndexOf(expression,StringComparison.Ordinal));
    }
    [Theory]
    [InlineData("(a+b)=c")][InlineData("F()=c")][InlineData("++a=c")][InlineData("a=b=c")]
    [InlineData("++a")][InlineData("--a")][InlineData("F()")]
    public void Unary_syntax_is_not_a_writability_or_statement_semantics_check(string expression)
    {using var result=CSharpParser.Parse(new("class C{void M(){"+expression+";}}","CSharp1","unary.cs",[]));Assert.True(result.Accepted,string.Join(";",result.Issues));}
}
