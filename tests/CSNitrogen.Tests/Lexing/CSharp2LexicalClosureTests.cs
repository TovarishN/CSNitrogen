using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
public class CSharp2LexicalClosureTests
{
 [Theory]
 [InlineData("@partial class C{}")]
 [InlineData(@"part\u0069al class C{}")]
 [InlineData(@"class C<T> wh\u0065re T:class{}")]
 [InlineData("class C{object M(){@yield return 1;}}")]
 [InlineData("extern @alias A; class C{}")]
 [InlineData("class C{int P{@get{return 1;} @set{}}}")]
 [InlineData("class C{}\u001a")]
 [InlineData("#define @class\n#if @class\nclass C{}\n#endif")]
 [InlineData(@"class C\U000E0100{}")]
 public void Historical_lexical_spellings_are_supported(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","lexical.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Theory][InlineData("#if(true)\nclass C{}\n#endif")][InlineData("#warning!")]
 public void Directive_arguments_require_whitespace(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","lexical.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code=="CSN1001");}
 [Fact]public void Escaped_boolean_is_a_conditional_symbol()
 {const string source="#if tr\\u0075e\nclass C{}\n#endif";using var p=CSharpParser.Parse(new(source,"CSharp2","lexical.cs",[]));Assert.True(p.Accepted);Assert.Empty(p.Classes);}
}
