using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
public class HistoricalUnicodeTests
{
 [Theory][InlineData("\u1885")][InlineData("\u1886")][InlineData("a\u1369")][InlineData("a\u1371")][InlineData("a\u30fb")][InlineData("a\uff65")]
 public void Historical_identifiers_are_accepted_only_in_the_historical_policy(string name)
 {
  using var old=CSharpParser.Parse(new("class "+name+"{}","CSharp1","old.cs",[]));Assert.True(old.Accepted,string.Join(";",old.Issues));
  using var current=CSharpParser.Parse(new("class "+name+"{}","CSharp14","current.cs",[]));Assert.False(current.Accepted);
 }
 [Theory][InlineData("class \u0220{}",false,true)][InlineData("\u200bclass C{}",true,false)][InlineData("\u205fclass C{}",false,true)]
 public void Changed_categories_respect_profile(string source,bool oldExpected,bool currentExpected)
 {
  using var old=CSharpParser.Parse(new(source,"CSharp1","old.cs",[]));Assert.Equal(oldExpected,old.Accepted);
  using var current=CSharpParser.Parse(new(source,"CSharp14","current.cs",[]));Assert.Equal(currentExpected,current.Accepted);
 }
 [Theory][InlineData("\u180b")][InlineData("\\u180C")][InlineData("\\U0000180D")]
 public void Changed_format_characters_have_profile_correct_target_ownership(string spelling)
 {
  string source="[as"+spelling+"sembly:A] class C{}";
  using var old=CSharpParser.Parse(new(source,"CSharp1","old.cs",[]));Assert.True(old.Accepted);Assert.Equal(1,old.Root.Attributes.Count);
  using var current=CSharpParser.Parse(new(source,"CSharp14","current.cs",[]));Assert.True(current.Accepted);Assert.Equal(0,current.Root.Attributes.Count);
 }
 [Theory][InlineData("#define A\u180B\n#if A\nclass C{}\n#endif")]
 [InlineData("\u200b#define \u1885\u200b\n#if \u1885\nclass C{}\n#endif")]
 [InlineData("#line 20\u200b\"mapped.cs\"\nclass C{}")]
 public void Preprocessor_uses_the_same_historical_categories(string source)
 {
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","old.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));Assert.Single(parsed.Classes);
 }
 [Fact] public void Escaped_keyword_spellings_remain_identifiers()
 {
  using var parsed=CSharpParser.Parse(new("class cl\\u180Bass{}","CSharp1","keyword.cs",[]));Assert.True(parsed.Accepted);
 }
 [Theory]
 [InlineData("class C\u200b{}")]
 [InlineData("class C{int\u200bx;}")]
 [InlineData("class C{int\u1369Type x;}")]
 [InlineData("class C{static\u1369Type x;}")]
 [InlineData("class C{void M(ref\u1369Type x){}}")]
 [InlineData("class C{void M(){F(ref\u1369value);}}")]
 [InlineData("class C{object M(){return true\u1369Value;}}")]
 [InlineData("class C{object M(){return (int\u1369Type[])-x;}}")]
 [InlineData("unsafe class C{void M(int\u1369Type* p){int\u1369Type* q;}}")]
 public void Historical_token_boundaries_do_not_split_identifiers_or_consume_spaces(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","boundary.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
 [Theory][InlineData("CSharp1")][InlineData("CSharp14")]
 public void Current_letter_number_suffixes_are_not_split_from_builtin_prefixes(string profile)
 {using var parsed=CSharpParser.Parse(new("class C{intⅠ x;}",profile,"boundary.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
 [Fact]
 public void Historical_whitespace_normalization_preserves_literals_and_authored_offsets()
 {
  const string source="class C\u200b{string s=\"\u200b\";}";
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","spaces.cs",[]));
  Assert.True(parsed.Accepted);Assert.Equal(source,parsed.OriginalText);Assert.Equal(source.Length,parsed.MaskedText.Length);
  Assert.Equal(' ',parsed.MaskedText[source.IndexOf('\u200b')]);
  Assert.Equal('\u200b',parsed.MaskedText[source.LastIndexOf('\u200b')]);
  Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),parsed.Classes[0].Name.Span.Start);
 }
 [Theory]
 [InlineData("class C{constᢅType x=1;}")]
 [InlineData("class C{eventᢅType x;}")]
 [InlineData("class C{void M(){returnᢅCall();}}")]
 [InlineData("class C{void M(){gotoᢅCall();}}")]
 [InlineData("class C{void M(){throwᢅCall();}}")]
 public void Longer_identifiers_are_not_constant_event_or_jump_keywords(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","prefix.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
 [Theory]
 [InlineData("CSharp1","new\u1885Type")]
 [InlineData("CSharp1","new\u1886Type")]
 [InlineData("CSharp1",@"new\u1885Type")]
 [InlineData("CSharp14","newⅠType")]
 public void Interface_new_prefix_is_part_of_the_complete_return_type(string profile,string name)
 {using var parsed=CSharpParser.Parse(new("interface I{"+name+" M();}",profile,"interface-prefix.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
}
