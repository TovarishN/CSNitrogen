using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2InheritedSourceTests
{
 public static IEnumerable<object[]> Cases()
 {
  foreach(string file in new[]{"source-grammar-cases.json","source-review-cases.json","context-conformance-cases.json","attribute-conformance-cases.json"})
  {
   using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus",file)));
   foreach(var row in json.RootElement.EnumerateArray())
   {
    string source=row.GetProperty("source").GetString()!;
    if(file=="attribute-conformance-cases.json" && !source.StartsWith("class",StringComparison.Ordinal) && !source.Contains(" using ",StringComparison.Ordinal))source+=" class C{}";
    string mode=row.TryGetProperty("mode",out var value)?value.GetString()!:"source";
    bool expected=row.GetProperty("expected").GetBoolean();
    if(source is "a??b" or "partial class C {}" or "class C<T> {}" or "A<T>")expected=true;
    string wrapped=mode switch
    {
     "type"=>"class C{"+source+" field;}",
     "expression"=>"class C{object M(){return "+source+";}}",
     "statement"=>"class C{void M(){"+source+"}}",
     "attribute"=>source+" class C{}",
     "parameter"=>"class C{void M"+source+"{}}",
     _=>source
    };
    yield return [file,wrapped,expected];
   }
  }
 }
 [Theory][MemberData(nameof(Cases))]
 public void Inherited_source_forms_are_executed_in_the_CSharp2_factory(string fixture,string source,bool expected)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp2",fixture,[]));Assert.True(expected==p.Accepted,source+" :: "+string.Join(";",p.Issues));
  Assert.Equal(source.Length,p.MaskedText.Length);
  for(int node=0;node<p.Tree.NodeCount;node++)
  {var span=p.Tree.Span(node);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
  foreach(var issue in p.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
 }
}
