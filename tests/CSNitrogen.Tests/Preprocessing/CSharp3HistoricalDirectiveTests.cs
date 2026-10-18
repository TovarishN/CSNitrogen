using CSNitrogen.Parsing;
using System.Numerics;
namespace CSNitrogen.Tests.Preprocessing;
public class CSharp3HistoricalDirectiveTests
{
 [Theory]
 [InlineData("#if tr\\u0075e\nnot C sharp\n#endif\nclass C{}")]
 [InlineData("#define tr\\u0075e\n#undef tr\\u0075e\n#if tr\\u0075e\nnot C sharp\n#endif\nclass C{}")]
 [InlineData("#if tr\u200cue\nnot C sharp\n#endif\nclass C{}")]
 public void Escaped_and_formatting_boolean_spellings_are_symbol_names(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","symbols.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal("C",Assert.Single(p.Classes).Name.ToString());}
 [Theory][InlineData("#if(true)\nclass C{}\n#endif")][InlineData("#warning!\nclass C{}")]
 [InlineData("#define @class\nclass C{}")][InlineData("#line de\\u0066ault\nclass C{}")]
 public void Source_spelling_and_directive_separators_remain_version_specific(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","bad.cs",[]));Assert.False(p.Accepted);}
 [Theory][InlineData("#line -1")][InlineData("#line 10 \"\"")][InlineData("#nullable disable")]
 public void Inactive_directives_still_have_the_historical_source_form(string directive)
 {using var p=CSharpParser.Parse(new("#if false\n"+directive+"\n#endif\nclass C{}","CSharp3","inactive.cs",[]));Assert.False(p.Accepted);}
 [Theory][InlineData("0")][InlineData("99999999999999999999999999999999999")]
 public void Source_decimal_digits_keep_exact_unbounded_line_mapping(string digits)
 {string source="#line "+digits+"\nclass C{}";using var p=CSharpParser.Parse(new(source,"CSharp3","line.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(BigInteger.Parse(digits)-1,p.MapLocationExact(source.IndexOf("class",StringComparison.Ordinal)).LogicalLine);}
 [Fact]public void Inactive_valid_line_directives_do_not_change_mapping()
 {using var p=CSharpParser.Parse(new("#if false\n#line 20 \"other.cs\"\n#endif\nclass C{}","CSharp3","inactive.cs",[]));Assert.True(p.Accepted);Assert.Empty(p.ExactLineMappings);}
}
