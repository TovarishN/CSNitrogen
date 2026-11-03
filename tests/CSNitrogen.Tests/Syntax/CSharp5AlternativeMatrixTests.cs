using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Tests.Lexing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Executes every Version5 alternative's witness on the actual CSharp5 factory, which stays partial.</summary>
public class CSharp5AlternativeMatrixTests
{
 const string Profile="CSharp5";
 static JsonElement Read(string path){using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,path)));return j.RootElement.Clone();}
 static readonly Lazy<Dictionary<string,JsonElement>> Witnesses=new(()=>
 {
  var result=new Dictionary<string,JsonElement>(StringComparer.Ordinal);
  foreach(string domain in new[]{"lexical","expression","statement","unsafe","declaration","finite","fourth-source-specific","source-specific"})
  {
   string path=domain switch
   {
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
 {foreach(var c in Read("Corpus/CSharp5/alternative-witness-matrix.json").GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetProperty("domain").GetString()!,c.GetProperty("witnessId").GetString()!];}
 [Theory][MemberData(nameof(Cases))]
 public void Every_fifth_nonplaceholder_alternative_executes_its_explicit_witness(string id,string domain,string witness)
 {
  var c=Witnesses.Value[domain+"/"+witness];
  if(domain=="lexical")CSharp4InheritedLexicalTests.Run(id,c.GetRawText(),Profile);
  else if(domain=="finite")CSharp4InheritedFiniteTests.Run(id,c.GetProperty("positive").GetString()!,c.GetProperty("negative").GetString()!,c.GetProperty("spanText").GetString()!,c.GetProperty("spanStart").GetInt32(),Profile);
  else
  {
   CSharp4InheritedProductionTests.Run(id,c.GetRawText(),Profile);
   if(domain is "source-specific" or "fourth-source-specific")
   {
    using var p=CSharpParser.Parse(new("/*𐐀*/"+c.GetProperty("source").GetString(),Profile,id,[]));
    string kind=c.GetProperty("kind").GetString()!;Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==kind);
   }
  }
 }
}
