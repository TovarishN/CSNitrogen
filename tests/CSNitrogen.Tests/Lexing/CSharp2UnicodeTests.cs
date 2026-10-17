using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
public class CSharp2UnicodeTests
{
 [Theory]
 [InlineData("class 𐐀{}")]
 [InlineData(@"class \U00010400{}")]
 [InlineData("class C𐐀{}")]
 [InlineData("\u180eclass C{}")]
 [InlineData("#define 𐐀\n#if 𐐀\nclass C{}\n#endif")]
 public void Unicode4_identifiers_and_whitespace_are_supported(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp2","unicode4.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
 [Fact]public void Supplementary_identifier_spans_preserve_authored_utf16_offsets()
 {
  const string source="class 𐐀{int 𐐁;}";
  using var p=CSharpParser.Parse(new(source,"CSharp2","unicode4.cs",[]));Assert.True(p.Accepted);
  Assert.Equal(2,p.Classes[0].Name.Span.Length);Assert.Equal(source.IndexOf("𐐀",StringComparison.Ordinal),p.Classes[0].Name.Span.Start);
  Assert.Equal(source,p.OriginalText);Assert.Equal(source.Length,p.MaskedText.Length);
 }
 [Theory][InlineData("[assem\\U000E0001bly:A] class C{}")][InlineData("[assem\U000E0001bly:A] class C{}")]
 public void Unicode4_formatting_selects_global_attribute_ownership(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","target.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(1,p.Root.Attributes.Count);Assert.Equal(0,p.Classes[0].Attributes.Count);}
 [Theory]
 [InlineData("class \u1c90{}")]
 [InlineData(@"class \u005f{}")]
 [InlineData("class \ud800{}")]
 [InlineData(@"class \U00110000{}")]
 public void Characters_outside_the_historical_identifier_contract_are_rejected(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp2","unicode4.cs",[]));Assert.False(parsed.Accepted);}
}
