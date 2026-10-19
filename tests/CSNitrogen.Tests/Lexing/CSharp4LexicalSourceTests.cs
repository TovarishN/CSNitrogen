using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Lexing;
public class CSharp4LexicalSourceTests
{
 [Fact]public void Escaped_boolean_spellings_are_symbols_under_the_fourth_source_grammar()
 {
  const string source=@"#if \u0074rue
class Wrong{}
#else
class Right{}
#endif";
  using var p=CSharpParser.Parse(new(source,"CSharp4","symbol.cs",[]));Assert.True(p.Accepted);Assert.Equal("Right",Assert.Single(p.Classes).Name.ToString());
 }
 [Theory]
 [InlineData("#if false\n#nullable enable\n#endif\nclass C{}")]
 [InlineData("#if false\n#:package x\n#endif\nclass C{}")]
 public void Disabled_sections_still_reject_directives_outside_the_fourth_source(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","inactive.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code=="CSN1205");}
 [Fact]public void Terminal_control_Z_reparse_matches_full_normalization_and_original_coordinates()
 {
  const string before="class 𐐀{}",after=before+"\u001a";
  using var old=CSharpParser.ParseIncremental(new(before,"CSharp4","eof.cs",[]));Assert.True(old.Accepted);
  using var edit=CSharpParser.Reparse(old,new(after,"CSharp4","eof.cs",[]),new[]{TextChange.Between(before,after)});
  using var full=CSharpParser.Parse(new(after,"CSharp4","eof.cs",[]));
  Assert.True(edit.Accepted);Assert.True(full.Accepted);Assert.Equal(full.MaskedText,edit.MaskedText);Assert.Equal(after,edit.OriginalText);
  Assert.Equal(2,Assert.Single(edit.Classes).Name.Span.Length);Assert.Equal(before.Length+1,edit.MaskedText.Length);
 }
}
