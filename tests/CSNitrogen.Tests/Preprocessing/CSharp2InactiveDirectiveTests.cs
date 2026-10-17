using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Preprocessing;
public class CSharp2InactiveDirectiveTests
{
 [Theory][InlineData("#line -1","CSN1207")][InlineData("#line 10 \"\"","CSN1207")][InlineData("#nullable disable","CSN1205")]
 public void Inactive_directives_still_require_historical_lexical_form(string directive,string code)
 {using var p=CSharpParser.Parse(new("#if false\n"+directive+"\n#endif\nclass C{}","CSharp2","inactive.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code==code);}
 [Fact] public void Inactive_valid_line_does_not_change_the_source_map()
 {using var p=CSharpParser.Parse(new("#if false\n#line 20 \"other.cs\"\n#endif\nclass C{}","CSharp2","inactive.cs",[]));Assert.True(p.Accepted);Assert.Empty(p.ExactLineMappings);}
}
