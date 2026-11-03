namespace CSNitrogen.CSharp6.Tests;
public class CSharp6InterpolationTests
{
 static string Field(string expression)=>"class C{object o="+expression+";}";
 static void Has(string source,string kind,params string[] texts)
 {
  foreach(var prefix in new[]{"","/*\U00010400*/"})
  {
   string s=prefix+source;var spans=Probe.Spans("All",s,kind);
   Assert.Equal(texts.Length,spans.Count);
   int from=0;foreach(var (t,i) in texts.Select((t,i)=>(t,i))){int at=s.IndexOf(t,from,StringComparison.Ordinal);Assert.True(at>=0,t);Assert.Equal((at,t.Length),spans[i]);from=at+1;}
  }
 }
 [Fact] public void Holes_have_typed_expressions_and_text_segments()
 {
  string s=Field("$\"a{x}b{y+1}c\"");
  Has(s,"Interpolation","x","y+1");
  Has(s,"InterpolatedRegularString","$\"a{x}b{y+1}c\"");
  Assert.All(Probe.Children("All",s,"Interpolation"),c=>Assert.Contains(c[0],new[]{"NameExpression","AddExpression"}));
 }
 [Fact] public void A_string_without_holes_is_one_interpolated_text()
 {
  Has(Field("$\"a /*x*/ b\""),"InterpolatedRegularText","$\"a /*x*/ b\"");
  Assert.Empty(Probe.Spans("All",Field("$\"a /*x*/ b\""),"Interpolation"));
 }
 [Fact] public void Trivia_inside_a_hole_belongs_to_the_hole() => Has(Field("$\"{ x /*c*/ }\""),"Interpolation","x");
 [Fact] public void Nested_regular_and_verbatim_strings()
 {
  Has(Field("$\"{$\"{x}\"}\""),"Interpolation","$\"{x}\"","x");
  Has(Field("$@\"{$\"{x}\"}\""),"InterpolatedVerbatimString","$@\"{$\"{x}\"}\"");
  Has(Field("$\"{$@\"{x}\"}\""),"InterpolatedVerbatimString","$@\"{x}\"");
 }
 [Fact] public void Width_and_format()
 {
  string s=Field("$\"{x,-5:N2}\"");
  Has(s,"InterpolationWidth",",-5");Has(s,"InterpolationFormat",":N2");Has(s,"Interpolation","x,-5:N2");
  Has(Field("$\"{x,5}\""),"InterpolationWidth",",5");
  Has(Field("$\"{x:yyyy-MM-dd HH:mm}\""),"InterpolationFormat",":yyyy-MM-dd HH:mm");
 }
 [Fact] public void Brace_escapes_are_text()
 {
  Assert.Empty(Probe.Spans("All",Field("$\"{{a}}\""),"Interpolation"));
  Has(Field("$\"{{{x}}}\""),"Interpolation","x");
  Assert.Empty(Probe.Spans("All",Field("$@\"{{\"\"}}\""),"Interpolation"));
 }
 [Theory]
 [InlineData("$\"{(a?b:c)}\"")]
 [InlineData("$\"{F(x:1)}\"")]
 [InlineData("$\"{global::A.B}\"")]
 [InlineData("$\"{a[b?1:2]}\"")]
 public void Nested_colons_are_not_formats(string e)=>Assert.Empty(Probe.Spans("All",Field(e),"InterpolationFormat"));
 [Fact] public void Verbatim_text_may_span_lines()=>Has(Field("$@\"a\r\n{x}\nb\""),"Interpolation","x");
 [Theory]
 [InlineData("$\"a\nb\"")]
 [InlineData("$\"{x}\nb\"")]
 [InlineData("$\"{x\"")]
 [InlineData("$\"{}\"")]
 [InlineData("$\"a}b\"")]
 [InlineData("$\"a{b\"")]
 [InlineData("$\"{x:}\"")]
 [InlineData("@$\"{x}\"")]
 [InlineData("$ \"{x}\"")]
 [InlineData("$\"\\q\"")]
 public void Malformed_interpolated_strings_are_rejected(string e)=>Assert.False(Probe.Accepts("All",Field(e)),e);
 /// <summary>The sixth-edition grammar lexes hole contents as ordinary tokens, so a new-line is whitespace there; only interpolated text and format text exclude it. Roslyn at version 6 rejects these (recorded divergence).</summary>
 [Fact] public void New_lines_inside_regular_holes_are_whitespace()
 {
  Has(Field("$\"{x:N\n}\""),"InterpolationFormat",":N");
  Has(Field("$\"{\nx\n}\""),"Interpolation","x");
 }
 [Fact] public void Escapes_in_regular_text()=>Has(Field("$\"\\n\\u0041\\\"{x}\\\\\""),"Interpolation","x");
 /// <summary>The standard's unnested colon starts a format, leaving the condition incomplete; the grammar reads a conditional, so rejection is a source-local policy obligation (Task 2).</summary>
 [Fact] public void Unparenthesized_conditional_in_a_hole_is_a_policy_obligation()
 {
  string s=Field("$\"{a?b:c}\"");
  Has(s,"ConditionalExpression","a?b:c");
  Assert.Empty(Probe.Spans("All",s,"InterpolationFormat"));
 }
}
