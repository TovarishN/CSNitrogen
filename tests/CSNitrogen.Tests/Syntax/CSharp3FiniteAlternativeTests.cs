using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3FiniteAlternativeTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/CSharp3/finite-alternative-cases.json")));
  foreach(var c in j.RootElement.GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetProperty("positive").GetString()!,c.GetProperty("negative").GetString()!,c.GetProperty("spanText").GetString()!,c.GetProperty("spanStart").GetInt32()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Every_finite_one_of_alternative_has_positive_negative_and_original_range_witnesses(string id,string positive,string negative,string spanText,int spanStart)
 {
  const string prefix="/*𐐀*/";string source=prefix+positive;
  using var good=CSharpParser.Parse(new(source,"CSharp3",id,[]));Assert.True(good.Accepted,id+" :: "+positive+" :: "+string.Join(";",good.Issues));Assert.True(good.ProfileComplete);
  int start=prefix.Length+spanStart;Assert.Equal(spanText,source.Substring(start,spanText.Length));
  Assert.Contains(Enumerable.Range(0,good.Tree.NodeCount),n=>good.Tree.Span(n).Start==start&&good.Tree.Span(n).Length==spanText.Length);
  using var bad=CSharpParser.Parse(new(prefix+negative,"CSharp3",id,[]));Assert.False(bad.Accepted,id+" :: "+negative);
  foreach(var issue in bad.Issues){Assert.InRange(issue.Span.Start,0,prefix.Length+negative.Length);Assert.InRange(issue.Span.Length,0,prefix.Length+negative.Length-issue.Span.Start);}
 }
}
