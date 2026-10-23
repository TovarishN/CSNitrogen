using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Replay of admitted CSharp3 witnesses on the actual CSharp4 factory; source identities remain CSharp3, not a CSharp4 occurrence mapping.</summary>
public class CSharp4InheritedProductionTests
{
 public static IEnumerable<object[]> Cases()
 {
  foreach(string domain in new[]{"expression","statement","unsafe","declaration"})
  {
   using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,$"Corpus/CSharp3/{domain}-production-cases.json")));
   foreach(var c in j.RootElement.GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetRawText()];
  }
 }
 [Theory][MemberData(nameof(Cases))]
 public void Production_alternatives_have_owning_context_positive_contrast_and_original_range_observations(string id,string json)
 {
  using var j=JsonDocument.Parse(json);var c=j.RootElement;const string prefix="/*𐐀*/";
  string source=prefix+c.GetProperty("source").GetString();
  using var parsed=CSharpParser.Parse(new(source,"CSharp4",id,[]));
  Assert.True(parsed.Accepted,id+" positive :: "+source+" :: "+string.Join(";",parsed.Issues));Assert.True(parsed.ProfileComplete);Assert.Empty(parsed.Issues);
  string focus=c.GetProperty("focus").GetString()!;int start=prefix.Length+c.GetProperty("focusStart").GetInt32();
  Assert.Equal(focus,source.Substring(start,focus.Length));
  string anchor=c.GetProperty("anchorText").GetString()!;int anchorStart=prefix.Length+c.GetProperty("anchorStart").GetInt32();
  Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Span(n).Start==anchorStart&&parsed.Tree.Span(n).Length==anchor.Length);
  if(c.TryGetProperty("acceptedNeighbor",out var neighbor)){using var accepted=CSharpParser.Parse(new(prefix+neighbor.GetString(),"CSharp4",id,[]));Assert.True(accepted.Accepted);Assert.Empty(accepted.Issues);}
  // A focus can be a grammar fragment (modifier/list/production body), not a generated node.
  // Its authored coordinates are exact; all generated spans below are bounds-checked.
  Assert.Equal(source,parsed.OriginalText);Assert.Equal(source.Length,parsed.MaskedText.Length);
  foreach(int n in Enumerable.Range(0,parsed.Tree.NodeCount)){var span=parsed.Tree.Span(n);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
  string negative=prefix+c.GetProperty("negative").GetString();
  using var contrast=CSharpParser.Parse(new(negative,"CSharp4",id,[]));Assert.False(contrast.Accepted,id+" contrast :: "+negative);
  foreach(var issue in contrast.Issues){Assert.InRange(issue.Span.Start,0,negative.Length);Assert.InRange(issue.Span.Length,0,negative.Length-issue.Span.Start);}
 }
}
