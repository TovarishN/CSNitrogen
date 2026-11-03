using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Tests.Lexing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Executes every sixth-edition alternative's witness on the actual CSharp6 factory, which stays partial.</summary>
public class CSharp6AlternativeMatrixTests
{
 const string Profile="CSharp6";
 static JsonElement Read(string path){using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,path)));return j.RootElement.Clone();}
 static readonly Lazy<Dictionary<string,JsonElement>> Witnesses=new(()=>
 {
  var result=new Dictionary<string,JsonElement>(StringComparer.Ordinal);
  foreach(string domain in new[]{"lexical","expression","statement","unsafe","declaration","finite","fourth-source-specific","source-specific","sixth-specific"})
  {
   string path=domain switch
   {
    "sixth-specific"=>"Corpus/CSharp6/source-specific-cases.json",
    "source-specific"=>"Corpus/CSharp5/source-specific-cases.json",
    "fourth-source-specific"=>"Corpus/CSharp4/source-specific-cases.json",
    "finite"=>"Corpus/CSharp3/finite-alternative-cases.json",
    _=>$"Corpus/CSharp3/{domain}-production-cases.json",
   };
   foreach(var c in Read(path).GetProperty("cases").EnumerateArray())result.Add(domain+"/"+c.GetProperty("id").GetString(),c.Clone());
  }
  return result;
 });
 public static IEnumerable<object[]> Cases()
 {foreach(var c in Read("Corpus/CSharp6/alternative-witness-matrix.json").GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetProperty("domain").GetString()!,c.GetProperty("witnessId").GetString()!];}
 [Fact]
 public void Every_sixth_alternative_is_linked_once()
 {
  var matrix=Read("Corpus/CSharp6/alternative-witness-matrix.json");
  var ids=matrix.GetProperty("cases").EnumerateArray().Select(c=>c.GetProperty("id").GetString()!).ToArray();
  Assert.Equal(1114,ids.Length);Assert.Equal(ids.Length,ids.Distinct(StringComparer.Ordinal).Count());
 }
 [Theory][MemberData(nameof(Cases))]
 public void Every_sixth_alternative_executes_its_explicit_witness(string id,string domain,string witness)
 {
  var c=Witnesses.Value[domain+"/"+witness];
  if(domain=="sixth-specific")
  {
   string prefix="/*\U00010400*/\n";string source=prefix+c.GetProperty("source").GetString()!;
   using var p=CSharpParser.Parse(new(source,Profile,id,[]));
   Assert.True(p.Accepted,id+": "+string.Join(";",p.Issues));
   string focus=c.GetProperty("focus").GetString()!;int start=c.GetProperty("focusStart").GetInt32()+prefix.Length;
   Assert.Equal(focus,source.Substring(start,focus.Length));
   // A node covers the authored focus; trailing trivia (line terminators) belongs to no node.
   string text=focus.TrimEnd();
   // Directive text is masked from the tree, so a directive record (with its leading line whitespace) covers it instead.
   bool node=Enumerable.Range(0,p.Tree.NodeCount).Any(n=>p.Tree.Span(n).Start<=start&&p.Tree.Span(n).End>=start+text.Length);
   bool directive=p.Directives.Any(d=>d.Span.Start+d.Span.Length>=start+text.Length&&(d.Span.Start<=start||string.IsNullOrWhiteSpace(source[start..d.Span.Start])));
   if(text.Length>0)Assert.True(node||directive,id+": focus not covered");
  }
  else if(domain=="lexical")CSharp4InheritedLexicalTests.Run(id,c.GetRawText(),Profile);
  else if(domain=="finite")CSharp4InheritedFiniteTests.Run(id,c.GetProperty("positive").GetString()!,c.GetProperty("negative").GetString()!,c.GetProperty("spanText").GetString()!,c.GetProperty("spanStart").GetInt32(),Profile);
  else
  {
   CSharp4InheritedProductionTests.Run(id,c.GetRawText(),Profile);
   if(domain is "source-specific" or "fourth-source-specific")
   {
    using var p=CSharpParser.Parse(new("/*\U00010400*/"+c.GetProperty("source").GetString(),Profile,id,[]));
    string kind=c.GetProperty("kind").GetString()!;Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==kind);
   }
  }
 }
}
