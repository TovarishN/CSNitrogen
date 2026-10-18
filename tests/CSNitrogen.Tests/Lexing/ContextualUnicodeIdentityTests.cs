using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
public class ContextualUnicodeIdentityTests
{
 [Theory][InlineData("\u180b")][InlineData("\u180c")][InlineData("\u180d")]
 [InlineData(@"\u180b")][InlineData(@"\U0000180C")]
 public void Category_changes_do_not_erase_combining_marks_from_contextual_words(string spelling)
 {
  string source="class C{int P{get"+spelling+"{return 1;}set{}}}";
  using var first=CSharpParser.Parse(new(source,"CSharp1","identity.cs",[]));Assert.True(first.Accepted,string.Join(";",first.Issues));
  var accepted=new List<string>();foreach(string profile in new[]{"CSharp2","CSharp3"})
  {using var p=CSharpParser.Parse(new(source,profile,"identity.cs",[]));if(p.Accepted)accepted.Add(profile);}Assert.Empty(accepted);
 }
 [Theory][InlineData("partial\u180b class C{}")][InlineData("class C<T> where\u180c T:class{}")]
 [InlineData("class C{int P{get{return 1;}set\u180d{}}}")]
 [InlineData("extern alias\u180b A;class C{}")]
 [InlineData("class C{event E Ev{add\u180c{}remove{}}}")]
 [InlineData("class C{event E Ev{add{}remove\u180d{}}}")]
 [InlineData("class C{object M(){yield\u180b break;}}")]
 public void Other_contextual_tokens_use_selected_identifier_identity(string source)
 {var accepted=new List<string>();foreach(string profile in new[]{"CSharp2","CSharp3"}){using var p=CSharpParser.Parse(new(source,profile,"contextual.cs",[]));if(p.Accepted)accepted.Add(profile);}Assert.Empty(accepted);}
 [Theory][InlineData("CSharp1")][InlineData("CSharp2")][InlineData("CSharp3")]
 public void Ordinary_identifier_marks_and_actual_format_characters_remain_available(string profile)
 {
  using var ordinary=CSharpParser.Parse(new("class C{int get\u180b;}",profile,"ordinary.cs",[]));Assert.True(ordinary.Accepted,string.Join(";",ordinary.Issues));
  using var typeName=CSharpParser.Parse(new("class partial\u180b{} class C{partial\u180b field;partial\u180b M(){return null;}}",profile,"type-name.cs",[]));Assert.True(typeName.Accepted,string.Join(";",typeName.Issues));
  using var contextual=CSharpParser.Parse(new("class C{int P{get\u200c{return 1;}set{}}}",profile,"contextual.cs",[]));Assert.True(contextual.Accepted,string.Join(";",contextual.Issues));
 }
 [Theory][InlineData("\u17b4")][InlineData("\u17b5")]
 public void Newly_format_characters_follow_the_selected_version(string spelling)
 {
  string source="class C{int P{get"+spelling+"{return 1;}set{}}}";
  using var first=CSharpParser.Parse(new(source,"CSharp1","identity.cs",[]));Assert.False(first.Accepted);Assert.Contains(first.Issues,i=>i.Code=="CSN2023");
  foreach(string profile in new[]{"CSharp2","CSharp3"}){using var p=CSharpParser.Parse(new(source,profile,"identity.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 }
 [Fact]public void Identity_mismatch_diagnostic_anchors_the_whole_contextual_spelling()
 {
  const string source="class C{int P{get\\u180b{return 1;}set{}}}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","identity.cs",[]));var issue=Assert.Single(p.Issues,i=>i.Code=="CSN2023");
  Assert.Equal(IssueCategory.Syntax,issue.Category);Assert.Equal("get\\u180b",source.Substring(issue.Span.Start,issue.Span.Length));
 }
}
