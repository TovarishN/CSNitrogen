using System.Text.Json.Nodes;
using System.Text;
using CSNitrogen.Profiles;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Profiles;
public class CSharp3CoverageContractTests
{
 static byte[] Read(string suffix){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+suffix)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static Dictionary<string,byte[]> Inputs()=>typeof(FeatureCatalog).Assembly.GetManifestResourceNames().ToDictionary(n=>n,n=>Read(n["CSNitrogen.".Length..]));
 static bool Evaluate(byte[]? gate=null,byte[]? index=null,byte[]? map=null,byte[]? reference=null,byte[]? composition=null,Dictionary<string,byte[]>? inputs=null,bool predecessor=true)
  =>CSharp3CoverageContract.Reviewed(gate??Read("csharp3-coverage-gate.json"),index??Read("csharp3-standard-index.json"),map??Read("csharp3-standard-crosswalk.json"),reference??Read("csharp3-reference-policy.json"),composition??Read("composition.json"),inputs??Inputs(),predecessor);
 [Fact]public void Only_the_reviewed_third_snapshot_with_admitted_predecessor_is_accepted()
 {Assert.True(Evaluate());Assert.False(Evaluate(predecessor:false));Assert.True(FeatureCatalog.ThirdSourceReviewed);}
 [Fact]public void Actual_third_factory_has_exact_reviewed_modules_and_compilation_unit_root()
 {Assert.True(CSharpParser.MatchesThirdFactory(FeatureCatalog.ThirdModules));Assert.False(CSharpParser.MatchesThirdFactory(FeatureCatalog.ThirdModules.Skip(1).ToArray()));Assert.False(CSharpParser.MatchesThirdFactory(FeatureCatalog.ThirdModules.Append(FeatureCatalog.ThirdModules[0]).ToArray()));Assert.False(CSharpParser.MatchesThirdFactory(FeatureCatalog.ThirdModules.Skip(1).Append("Invented.Module").ToArray()));}
 [Theory]
 [InlineData("ledger")][InlineData("index")][InlineData("map")][InlineData("authority")][InlineData("unicode")][InlineData("composition-start")][InlineData("composition-namespace")][InlineData("composition-modules")]
 [InlineData("source-added")][InlineData("source-removed")][InlineData("grammar-added")][InlineData("report-added")][InlineData("evidence-added")]
 [InlineData("source-code")][InlineData("grammar-code")][InlineData("report-code")][InlineData("evidence-code")]
 [InlineData("alternative")][InlineData("row-review")][InlineData("inherited-target")][InlineData("production-link")][InlineData("unknown-domain")][InlineData("correction")]
 public void Reviewed_snapshot_and_input_drift_fail_closed(string change)
 {
  byte[] gate=Read("csharp3-coverage-gate.json"),index=Read("csharp3-standard-index.json"),map=Read("csharp3-standard-crosswalk.json"),reference=Read("csharp3-reference-policy.json"),composition=Read("composition.json");var inputs=Inputs();
  byte[] Edit(byte[] b,Action<JsonNode> edit){var n=JsonNode.Parse(b)!;edit(n);return Encoding.UTF8.GetBytes(n.ToJsonString());}
  switch(change)
  {
   case "ledger":gate=Edit(gate,n=>n["status"]="pending");break;
   case "index":index=Edit(index,n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "map":map=Edit(map,n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "authority":reference=Edit(reference,n=>n["primary"]!["sha256"]="invented");break;
   case "unicode":reference=Edit(reference,n=>n["unicode"]!["policy"]="unicode-current-bmp");break;
   case "composition-start":case "composition-namespace":case "composition-modules":
    composition=Edit(composition,n=>{var f=n["languages"]!.AsArray().Single(l=>l!["name"]!.GetValue<string>()=="CSharp3")!;if(change=="composition-start")f["start"]="CSharp.Core.LegacyCompilationUnit";else if(change=="composition-namespace")f["namespace"]="Invented.Syntax";else f["grammars"]!.AsArray().RemoveAt(0);});break;
   case "source-added":inputs["CSNitrogen.source.Parsing/Unreviewed.cs"]=[32];break;case "source-removed":inputs.Remove("CSNitrogen.source.Parsing/UnicodePolicy.cs");break;
   case "grammar-added":inputs["CSNitrogen.grammar.Unreviewed.ngr"]=[32];break;case "report-added":inputs["CSNitrogen.csharp3-review.unreviewed.md"]=[32];break;case "evidence-added":inputs["CSNitrogen.csharp3-evidence.unreviewed.json"]=[32];break;
   case "source-code":case "grammar-code":case "report-code":case "evidence-code":
    string key=change switch{"source-code"=>"CSNitrogen.source.Parsing/CSharpSyntaxPolicy.cs","grammar-code"=>"CSNitrogen.grammar.Queries.ngr","report-code"=>"CSNitrogen.csharp3-review.csharp3-expression-source-review.md",_=>"CSNitrogen.csharp3-evidence.declaration-production-cases.json"};inputs[key]=inputs[key].Concat(new byte[]{32}).ToArray();break;
   case "alternative":index=Edit(index,n=>n["productions"]![0]!["alternatives"]!.AsArray().RemoveAt(0));break;
   case "row-review":map=Edit(map,n=>n["productions"]![0]!["reviews"]!["normative-prose"]="pending");break;
   case "inherited-target":map=Edit(map,n=>n["productions"]!.AsArray().First(r=>r!["inheritedSourceAlternatives"] is not null)!["inheritedSourceAlternatives"]![0]!["sourceAlternativeIds"]!.AsArray().Add("invented"));break;
   case "production-link":case "unknown-domain":map=Edit(map,n=>{var link=n["productions"]!.AsArray().First(r=>r!["productionAlternativeWitnesses"] is not null)!["productionAlternativeWitnesses"]![0]!;link[change=="production-link"?"caseId":"domain"]="invented";});break;
   case "correction":reference=Edit(reference,n=>n["sourceCorrections"]!["status"]="pending");break;
  }
  Assert.False(Evaluate(gate,index,map,reference,composition,inputs));
 }
}
