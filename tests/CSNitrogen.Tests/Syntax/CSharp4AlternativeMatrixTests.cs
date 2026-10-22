using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Tests.Lexing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4AlternativeMatrixTests
{
 static JsonElement Read(string path){using var j=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,path)));return j.RootElement.Clone();}
 static readonly Lazy<Dictionary<string,JsonElement>> Witnesses=new(()=>
 {
  var result=new Dictionary<string,JsonElement>(StringComparer.Ordinal);
  foreach(string domain in new[]{"lexical","expression","statement","unsafe","declaration","finite","source-specific"})
  {
   string path=domain=="source-specific"?"Corpus/CSharp4/source-specific-cases.json":domain=="finite"?"Corpus/CSharp3/finite-alternative-cases.json":$"Corpus/CSharp3/{domain}-production-cases.json";
   foreach(var c in Read(path).GetProperty("cases").EnumerateArray())result.Add(domain+"/"+c.GetProperty("id").GetString(),c.Clone());
  }
  return result;
 });
 public static IEnumerable<object[]> Cases()
 {foreach(var c in Read("Corpus/CSharp4/alternative-witness-matrix.json").GetProperty("cases").EnumerateArray())yield return [c.GetProperty("id").GetString()!,c.GetProperty("domain").GetString()!,c.GetProperty("witnessId").GetString()!];}
 [Theory][MemberData(nameof(Cases))]
 public void Every_fourth_nonplaceholder_alternative_executes_its_explicit_witness(string id,string domain,string witness)
 {
  var c=Witnesses.Value[domain+"/"+witness];
  if(domain=="lexical")new CSharp4InheritedLexicalTests().Lexical_production_alternatives_retain_context_effects_and_original_coordinates(id,c.GetRawText());
  else if(domain=="finite")new CSharp4InheritedFiniteTests().Every_finite_one_of_alternative_has_positive_negative_and_original_range_witnesses(id,c.GetProperty("positive").GetString()!,c.GetProperty("negative").GetString()!,c.GetProperty("spanText").GetString()!,c.GetProperty("spanStart").GetInt32());
  else
  {
   new CSharp4InheritedProductionTests().Production_alternatives_have_owning_context_positive_contrast_and_original_range_observations(id,c.GetRawText());
   if(domain=="source-specific")
   {
    using var p=CSharpParser.Parse(new("/*𐐀*/"+c.GetProperty("source").GetString(),"CSharp4",id,[]));
    string kind=c.GetProperty("kind").GetString()!;Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==kind);
   }
  }
 }
}
