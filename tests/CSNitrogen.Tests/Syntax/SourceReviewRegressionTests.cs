using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class SourceReviewRegressionTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/source-review-cases.json")));
  foreach(var row in json.RootElement.EnumerateArray())yield return [row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Reviewed_source_contexts(string source,bool expected)
 {using var document=CSharpParser.Parse(new(source,"CSharp1","review.cs",[]));Assert.True(expected==document.Accepted,source+" :: "+string.Join(";",document.Issues)+" :: "+string.Join(",",Enumerable.Range(0,document.Tree.NodeCount).Select(n=>n+":"+document.Tree.Language!.GetKindName(document.Tree.Kind(n))+"/"+document.Tree.Parent(n))));}
}
