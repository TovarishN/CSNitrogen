using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
/// <summary>Replay of admitted CSharp3 witnesses on the actual CSharp4 factory; source identities remain CSharp3, not a CSharp4 occurrence mapping.</summary>
public class CSharp4InheritedLexicalTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/CSharp3/lexical-production-cases.json")));
  foreach(var c in j.RootElement.GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetRawText()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Lexical_production_alternatives_retain_context_effects_and_original_coordinates(string id,string json)
 {
  using var j=JsonDocument.Parse(json);var c=j.RootElement;
  const string prefix="/*𐐀*/\n";
  foreach(var p in c.GetProperty("positives").EnumerateArray())
  {
   string source=prefix+p.GetProperty("source").GetString();
   using var parsed=CSharpParser.Parse(new(source,"CSharp4",id,[]));
   Assert.True(parsed.Accepted==p.GetProperty("accepted").GetBoolean(),id+" :: "+source+" :: "+string.Join(";",parsed.Issues));
   Assert.True(parsed.ProfileComplete);
   Assert.Equal(p.GetProperty("classNames").EnumerateArray().Select(x=>x.GetString()),parsed.Classes.Select(x=>x.Name.ToString()));
   Assert.Equal(p.GetProperty("directiveKinds").EnumerateArray().Select(x=>x.GetString()),parsed.Directives.Select(x=>x.Kind));
   Assert.Equal(p.GetProperty("issueCodes").EnumerateArray().Select(x=>x.GetString()).Order(),parsed.Issues.Select(x=>x.Code).Order());
   foreach(var anchor in p.GetProperty("anchors").EnumerateArray())
   {
    int start=prefix.Length+anchor.GetProperty("start").GetInt32();string text=anchor.GetProperty("text").GetString()!;
    Assert.Equal(text,source.Substring(start,text.Length));
    Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Span(n).Start==start&&parsed.Tree.Span(n).Length==text.Length);
   }
   foreach(var arg in p.GetProperty("arguments").EnumerateObject())Assert.Contains(parsed.Directives,d=>d.Kind==arg.Name&&d.Argument==arg.Value.GetString());
   Assert.Equal(source,parsed.OriginalText);Assert.Equal(source.Length,parsed.MaskedText.Length);
   foreach(int n in Enumerable.Range(0,parsed.Tree.NodeCount)){var span=parsed.Tree.Span(n);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
   foreach(var span in parsed.InactiveSpans.Concat(parsed.Directives.Select(d=>d.Span))){Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
   foreach(var issue in parsed.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
  }
  var contrast=c.GetProperty("contrast");string negative=prefix+contrast.GetProperty("source").GetString();
  using var other=CSharpParser.Parse(new(negative,"CSharp4",id,[]));
  Assert.True(other.Accepted==contrast.GetProperty("accepted").GetBoolean(),id+" contrast :: "+negative+" :: "+string.Join(";",other.Issues));
  foreach(var code in contrast.GetProperty("requiredCodes").EnumerateArray())Assert.Contains(other.Issues,i=>i.Code==code.GetString());
  foreach(var issue in other.Issues){Assert.InRange(issue.Span.Start,0,negative.Length);Assert.InRange(issue.Span.Length,0,negative.Length-issue.Span.Start);}
 }
}
