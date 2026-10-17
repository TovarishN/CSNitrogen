using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Preprocessing;
public class HistoricalConditionalIdentityTests
{
    [Theory]
    [InlineData("#if \\u0074rue\nclass C{}\n#endif",false)]
    [InlineData("#define \\u0066alse\n#if \\u0066alse\nclass C{}\n#endif",true)]
    [InlineData("#define \\u0074rue\n#if \\u0074rue\nclass C{}\n#endif",true)]
    [InlineData("#if tr\u200Cue\nclass C{}\n#endif",false)]
    public void Historical_escaped_boolean_spellings_are_conditional_symbols(string source,bool included)
    {
        using var doc=CSharpParser.Parse(new(source,"CSharp1","symbols.cs",[]));
        Assert.True(doc.Accepted,string.Join(";",doc.Issues));
        Assert.Equal(included?1:0,doc.Classes.Count);
    }
    [Theory][InlineData("#nullable enable")][InlineData("#pragma warning disable")]
    public void Inactive_later_directives_are_still_outside_historical_lexical_grammar(string directive)
    {
        using var doc=CSharpParser.Parse(new("#if false\n"+directive+"\n#endif","CSharp1","later.cs",[]));
        Assert.False(doc.Accepted);Assert.Contains(doc.Issues,i=>i.Category==IssueCategory.UnavailableFeature);
    }
}
