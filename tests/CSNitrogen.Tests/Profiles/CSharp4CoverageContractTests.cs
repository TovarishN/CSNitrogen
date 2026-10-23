using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp4CoverageContractTests
{
 static byte[] Read(string name){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static Dictionary<string,byte[]> Inputs()=>typeof(FeatureCatalog).Assembly.GetManifestResourceNames().ToDictionary(n=>n,n=>Read(n["CSNitrogen.".Length..]));
 static bool Evaluate(byte[]? gate=null,byte[]? composition=null,Dictionary<string,byte[]>? inputs=null,bool predecessor=true)
  =>CSharp4CoverageContract.Reviewed(gate??Read("csharp4-coverage-gate.json"),composition??Read("composition.json"),inputs??Inputs(),predecessor);
 [Fact]public void Only_the_reviewed_fourth_snapshot_with_admitted_third_predecessor_is_accepted()
 {Assert.True(Evaluate());Assert.False(Evaluate(predecessor:false));Assert.True(FeatureCatalog.FourthSourceReviewed);}
 [Fact]public void Actual_fourth_factory_requires_exact_modules_and_compilation_unit_root()
 {
  Assert.Equal(30,FeatureCatalog.FourthModules.Count);Assert.True(CSharpParser.MatchesFourthFactory(FeatureCatalog.FourthModules));
  Assert.False(CSharpParser.MatchesFourthFactory(FeatureCatalog.FourthModules.Skip(1).ToArray()));Assert.False(CSharpParser.MatchesFourthFactory(FeatureCatalog.FourthModules.Append(FeatureCatalog.FourthModules[0]).ToArray()));Assert.False(CSharpParser.MatchesFourthFactory(FeatureCatalog.FourthModules.Skip(1).Append("Invented.Module").ToArray()));
 }
 [Theory]
 [InlineData("ledger")][InlineData("composition-start")][InlineData("composition-namespace")][InlineData("composition-modules")]
 [InlineData("source-added")][InlineData("source-removed")][InlineData("grammar-added")][InlineData("artifact-added")][InlineData("review-added")][InlineData("evidence-added")][InlineData("predecessor-evidence-added")]
 [InlineData("source-code")][InlineData("grammar-code")][InlineData("dependency-code")][InlineData("inventory-code")]
 [InlineData("source-authority")][InlineData("unicode")][InlineData("explicit-pointer-boundary")]
 [InlineData("source-index")][InlineData("source-body")][InlineData("unknown-owner")][InlineData("unknown-policy")]
 [InlineData("occurrence-review")][InlineData("occurrence-domain")][InlineData("occurrence-reference")]
 [InlineData("witness-link")][InlineData("witness-source")][InlineData("specific-case")][InlineData("predecessor-case")]
 [InlineData("main-disposition")][InlineData("main-index")][InlineData("context-report")][InlineData("correction")]
 public void Source_factory_policy_and_evidence_drift_fail_closed(string change)
 {
  var inputs=Inputs();byte[] gate=Read("csharp4-coverage-gate.json"),composition=Read("composition.json");
  byte[] Edit(byte[] b,Action<JsonNode> edit){var n=JsonNode.Parse(b)!;edit(n);return Encoding.UTF8.GetBytes(n.ToJsonString());}
  void Artifact(string name,Action<JsonNode> edit){string key="CSNitrogen.csharp4-artifact.csharp4-"+name+".json";inputs[key]=Edit(inputs[key],edit);}
  switch(change)
  {
   case "ledger":gate=Edit(gate,n=>n["status"]="pending");break;
   case "composition-start":case "composition-namespace":case "composition-modules":
    composition=Edit(composition,n=>{var f=n["languages"]!.AsArray().Single(l=>l!["name"]!.GetValue<string>()=="CSharp4")!;if(change=="composition-start")f["start"]="CSharp.Core.LegacyCompilationUnit";else if(change=="composition-namespace")f["namespace"]="Invented.Syntax";else f["grammars"]!.AsArray().RemoveAt(0);});break;
   case "source-added":inputs["CSNitrogen.source.Parsing/Unreviewed.cs"]=[32];break;
   case "source-removed":inputs.Remove("CSNitrogen.source.Parsing/UnicodePolicy.cs");break;
   case "grammar-added":inputs["CSNitrogen.grammar.Unreviewed.ngr"]=[32];break;
   case "artifact-added":inputs["CSNitrogen.csharp4-artifact.unreviewed.json"]=[32];break;
   case "review-added":inputs["CSNitrogen.csharp4-review.unreviewed.md"]=[32];break;
   case "evidence-added":inputs["CSNitrogen.csharp4-evidence.unreviewed.json"]=[32];break;
   case "predecessor-evidence-added":inputs["CSNitrogen.csharp3-evidence.unreviewed.json"]=[32];break;
   case "source-code":case "grammar-code":case "dependency-code":case "inventory-code":case "context-report":
    string key=change switch{"source-code"=>"CSNitrogen.source.Parsing/CSharp4ContextPolicy.cs","grammar-code"=>"CSNitrogen.grammar.GenericFollowersFourth.ngr","dependency-code"=>"CSNitrogen.dependencies.json","inventory-code"=>"CSNitrogen.coverage.json",_=>"CSNitrogen.csharp4-review.csharp4-normative-context-review.md"};inputs[key]=inputs[key].Concat(new byte[]{32}).ToArray();break;
   case "source-authority":Artifact("reference-policy",n=>n["primary"]!["sha256"]="invented");break;
   case "unicode":Artifact("reference-policy",n=>n["unicode"]!["policy"]="unicode-current-bmp");break;
   case "explicit-pointer-boundary":Artifact("reference-policy",n=>n["normalizedExclusions"]!.AsArray().Clear());break;
   case "source-index":Artifact("standard-index",n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "source-body":Artifact("standard-crosswalk",n=>n["productions"]![0]!["sourceBodySha256"]="changed");break;
   case "unknown-owner":Artifact("standard-crosswalk",n=>n["productions"]![0]!["targets"]!.AsArray().Add("Invented.Module/rule:Missing"));break;
   case "unknown-policy":Artifact("standard-crosswalk",n=>n["productions"]![0]!["policies"]!.AsArray().Add("invented"));break;
   case "occurrence-review":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["reviews"]!["normative-prose"]="pending");break;
   case "occurrence-domain":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["sourceReview"]!["domain"]="invented");break;
   case "occurrence-reference":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["sourceReview"]!["occurrenceId"]="invented");break;
   case "correction":Artifact("standard-crosswalk",n=>n["sourceCorrections"]!["approved-generic-member-suffix"]!["status"]="pending");break;
   case "main-disposition":Artifact("main-text-dispositions",n=>n["regions"]!.AsArray().RemoveAt(0));break;
   case "main-index":Artifact("main-text-differences",n=>n["regions"]!.AsArray().RemoveAt(0));break;
   case "witness-link":case "witness-source":case "specific-case":case "predecessor-case":
    string fixture=change=="specific-case"?"CSNitrogen.csharp4-evidence.source-specific-cases.json":change=="predecessor-case"?"CSNitrogen.csharp3-evidence.expression-production-cases.json":"CSNitrogen.csharp4-evidence.alternative-witness-matrix.json";
    inputs[fixture]=Edit(inputs[fixture],n=>{if(change=="witness-link")n["cases"]![0]!["witnessId"]="invented";else if(change=="witness-source")n["sourceSha256"]="changed";else n["cases"]![0]!["source"]="class Changed{}";});break;
  }
  Assert.False(Evaluate(gate,composition,inputs));
 }
 [Fact]public void Directed_contract_pins_require_real_predecessor_admission_when_fourth_contract_code_changes()
 {
  var inputs=Inputs();Assert.False(CSharp4CoverageContract.Eligible("CSNitrogen.source.Profiles/CSharp4CoverageContract.cs"));Assert.True(CSharp3CoverageContract.Eligible("CSNitrogen.source.Profiles/CSharp4CoverageContract.cs"));
  string key="CSNitrogen.source.Profiles/CSharp4CoverageContract.cs";inputs[key]=inputs[key].Concat(new byte[]{32}).ToArray();
  bool predecessor=CSharp3CoverageContract.Reviewed(Read("csharp3-coverage-gate.json"),Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json"),Read("csharp3-reference-policy.json"),Read("composition.json"),inputs,true);
  Assert.False(predecessor);Assert.False(Evaluate(inputs:inputs,predecessor:predecessor));
 }
}
