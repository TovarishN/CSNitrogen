using System.Text.Json.Nodes;
using System.Text;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp2CoverageContractTests
{
 static byte[] Read(string suffix)
 {using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+suffix)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static IReadOnlyDictionary<string,byte[]> Inputs()=>typeof(FeatureCatalog).Assembly.GetManifestResourceNames().ToDictionary(n=>n,n=>Read(n["CSNitrogen.".Length..]));
 [Fact]public void Only_the_pinned_reviewed_snapshot_is_admitted()
 {Assert.True(CSharp2CoverageContract.Reviewed(Read("csharp2-coverage-gate.json"),Read("csharp2-standard-index.json"),Read("csharp2-standard-crosswalk.json"),Read("csharp2-reference-policy.json"),Read("composition.json"),Inputs()));}
 [Theory]
 [InlineData("ledger-status")][InlineData("index-source")][InlineData("index-heading")][InlineData("crosswalk-row")][InlineData("crosswalk-review")]
 [InlineData("composition-start")][InlineData("composition-namespace")]
 [InlineData("source-added")][InlineData("source-removed")][InlineData("grammar-added")][InlineData("review-added")]
 [InlineData("reference-unicode")][InlineData("composition-membership")][InlineData("source-code")][InlineData("grammar-code")][InlineData("review-record")]
 public void Reviewed_input_drift_fails_closed(string mutation)
 {
  var gate=Read("csharp2-coverage-gate.json");var index=Read("csharp2-standard-index.json");var map=Read("csharp2-standard-crosswalk.json");var reference=Read("csharp2-reference-policy.json");var composition=Read("composition.json");var inputs=Inputs().ToDictionary(x=>x.Key,x=>x.Value);
  byte[] Change(byte[] bytes,Action<JsonNode> edit){var node=JsonNode.Parse(bytes)!;edit(node);return Encoding.UTF8.GetBytes(node.ToJsonString());}
  switch(mutation)
  {
   case "ledger-status":gate=Change(gate,n=>n["status"]="pending");break;
   case "index-source":index=Change(index,n=>n["sourceSha256"]=new string('0',64));break;
   case "index-heading":index=Change(index,n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "crosswalk-row":map=Change(map,n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "crosswalk-review":map=Change(map,n=>n["productions"]![0]!["reviews"]!["normative-prose"]="pending");break;
   case "reference-unicode":reference=Change(reference,n=>n["unicode"]!["policy"]="unicode-current-bmp");break;
   case "composition-start":composition=Change(composition,n=>n["languages"]!.AsArray().Single(x=>x!["name"]!.GetValue<string>()=="CSharp2")!["start"]="CSharp.Core.LegacyCompilationUnit");break;
   case "composition-namespace":composition=Change(composition,n=>n["languages"]!.AsArray().Single(x=>x!["name"]!.GetValue<string>()=="CSharp2")!["namespace"]="Invented.Syntax");break;
   case "source-added":inputs["CSNitrogen.source.Parsing/UnreviewedPolicy.cs"]=[32];break;
   case "source-removed":inputs.Remove("CSNitrogen.source.Parsing/UnicodePolicy.cs");break;
   case "grammar-added":inputs["CSNitrogen.grammar.Unreviewed.ngr"]=[32];break;
   case "review-added":inputs["CSNitrogen.review.unreviewed.md"]=[32];break;
   case "composition-membership":composition=Change(composition,n=>n["languages"]!.AsArray().Single(x=>x!["name"]!.GetValue<string>()=="CSharp2")!["grammars"]!.AsArray().RemoveAt(0));break;
   default:
    string name=mutation switch {"source-code"=>"CSNitrogen.source.Parsing/CSharpSyntaxPolicy.cs","grammar-code"=>"CSNitrogen.grammar.Generics.ngr",_=>"CSNitrogen.review.csharp2-conformance-review.md"};
    inputs[name]=inputs[name].Concat(new byte[]{32}).ToArray();break;
  }
  Assert.False(CSharp2CoverageContract.Reviewed(gate,index,map,reference,composition,inputs));
 }
}
