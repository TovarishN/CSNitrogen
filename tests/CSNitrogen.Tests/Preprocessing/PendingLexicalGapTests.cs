using CSNitrogen.Parsing;
using CSNitrogen.Preprocessing;
namespace CSNitrogen.Tests.Preprocessing;
public class PendingLexicalGapTests
{
    [Theory]
    [InlineData("class C{}\u001a",true)]
    [InlineData("class C{}\u0085",false)]
    [InlineData("class \\u005fC{}",false)]
    [InlineData("class _C{}",true)]
    [InlineData("class C{string s=\"\u0085\";}",true)]
    [InlineData("class C{char c='\u0085';}",true)]
    [InlineData("#line hidden\nclass C{}",false)]
    [InlineData("#line 1 \"\"\nclass C{}",false)]
    [InlineData("#if(true)\nclass C{}\n#endif",false)]
    [InlineData("#warning\"text\"\nclass C{}",false)]
    [InlineData("#if false\n#line nonsense\n#endif\nclass C{}",false)]
    public void Historical_lexical_and_directive_rules_follow_the_pinned_standard(string source,bool accepted)
    {using var result=CSharpParser.Parse(new(source,"CSharp1","historical.cs",[]));Assert.Equal(accepted,result.Accepted);}
    [Fact]
    public void NEL_is_not_a_historical_comment_terminator_or_physical_line_break()
    {
        const string source="// text\u0085class Hidden{}";
        using var result=CSharpParser.Parse(new(source,"CSharp1","comment.cs",[]));
        Assert.True(result.Accepted);Assert.Empty(result.Classes);
        Assert.Equal(0,result.Preprocessing.MapLocation(source.Length).PhysicalLine);
    }
    [Theory][InlineData("#line 2147483648\nclass C{}")][InlineData("#line 16707566\nclass C{}")]
    public void Historical_large_line_mapping_is_preserved_after_API_approval(string source)
    {
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));
        Assert.Empty(result.Issues);
        Assert.Single(result.ExactLineMappings);
    }
}
