using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Lexing;
public class CSharp3UnicodeContractTests
{
 [Theory]
 [InlineData("class 𐐀{}")][InlineData(@"class \U00010400{}")][InlineData("class C𐐀{}")]
 [InlineData("\u180eclass C{}")][InlineData("\u205fclass C{}")]
 [InlineData("#define 𐐀\n#if 𐐀\nclass C{}\n#endif")]
 [InlineData("#define \\U00010400\n#if 𐐀\nclass C{}\n#endif")]
 [InlineData("class C{}\u001a")]
 public void Approved_frozen_repertoire_is_used_by_source_and_directives(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","unicode3.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Theory]
 [InlineData("class \u1c90{}")][InlineData(@"class \u005f{}")][InlineData("class \ud800{}")]
 [InlineData(@"class \U00110000{}")][InlineData("#define \u1c90\nclass C{}")]
 [InlineData("#if \u1c90\nclass C{}\n#endif")]
 public void Current_host_characters_and_invalid_spelling_do_not_leak_into_the_contract(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","bad.cs",[]));Assert.False(p.Accepted);}
 [Fact]public void Supplementary_spans_and_conditional_identity_remain_UTF16_based()
 {
  const string source="#if 𐐀\nclass 𐐁{int 𐐂;}\n#endif";
  using var p=CSharpParser.Parse(new(source,"CSharp3","span.cs",[@"\U00010400"]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Equal(2,p.Classes[0].Name.Span.Length);Assert.Equal(source.IndexOf("𐐁",StringComparison.Ordinal),p.Classes[0].Name.Span.Start);Assert.Equal(source,p.OriginalText);Assert.Equal(source.Length,p.MaskedText.Length);
 }
 [Fact]public void Metadata_exposes_the_approved_policy_without_promoting_completeness()
 {var p=ProfileResolver.Resolve("CSharp3").Profile!;Assert.Contains("unicode-4.0-scalar",p.Policies);Assert.DoesNotContain("unicode-current-bmp",p.Policies);Assert.False(p.Complete);}
}
