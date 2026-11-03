using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp5CoverageContractTests
{
 static byte[] Read(string name){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static Dictionary<string,byte[]> Inputs()=>typeof(FeatureCatalog).Assembly.GetManifestResourceNames().ToDictionary(n=>n,n=>Read(n["CSNitrogen.".Length..]));
 static bool Evaluate(byte[]? gate=null,byte[]? composition=null,Dictionary<string,byte[]>? inputs=null,bool predecessor=true)
  =>CSharp5CoverageContract.Reviewed(gate??Read("csharp5-coverage-gate.json"),composition??Read("composition.json"),inputs??Inputs(),predecessor);
 [Fact]public void Only_the_reviewed_fifth_snapshot_with_admitted_fourth_predecessor_is_accepted()
 {Assert.True(Evaluate());Assert.False(Evaluate(predecessor:false));Assert.True(FeatureCatalog.FifthSourceReviewed);Assert.True(ProfileResolver.Resolve("CSharp5").Profile!.Complete);}
 [Fact]public void Actual_fifth_factory_requires_exact_modules_and_compilation_unit_root()
 {
  Assert.Equal(32,FeatureCatalog.FifthModules.Count);Assert.True(CSharpParser.MatchesFifthFactory(FeatureCatalog.FifthModules));
  Assert.Contains("CSharp.AsyncFunctions",FeatureCatalog.FifthModules);Assert.Contains("CSharp.AwaitExpressions",FeatureCatalog.FifthModules);
  Assert.False(CSharpParser.MatchesFifthFactory(FeatureCatalog.FourthModules));
  Assert.False(CSharpParser.MatchesFifthFactory(FeatureCatalog.FifthModules.Skip(1).ToArray()));Assert.False(CSharpParser.MatchesFifthFactory(FeatureCatalog.FifthModules.Append(FeatureCatalog.FifthModules[0]).ToArray()));Assert.False(CSharpParser.MatchesFifthFactory(FeatureCatalog.FifthModules.Skip(1).Append("Invented.Module").ToArray()));
 }
 [Fact]public void Later_minor_and_custom_selections_remain_partial()
 {
  foreach(string name in Enumerable.Range(6,9).Select(n=>$"CSharp{n}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))Assert.False(ProfileResolver.Resolve(name).Profile!.Complete);
  Assert.False(ProfileResolver.ResolveCustom("CSharp5",FeatureCatalog.Implemented.Where(f=>f.Introduced<=new Version(5,0)).Select(f=>f.Id)).Profile!.Complete);
  using var good=CSharpParser.Parse(new("class C{async void M(){await(M());}}","CSharp5","good5.cs",[]));using var bad=CSharpParser.Parse(new("class C{void M(){await M();}}","CSharp5","bad5.cs",[]));
  Assert.True(good.Accepted);Assert.False(bad.Accepted);Assert.True(good.ProfileComplete);Assert.True(bad.ProfileComplete);Assert.Equal(good.CoverageIdentity,bad.CoverageIdentity);
 }
 [Theory]
 [InlineData("ledger")][InlineData("composition-start")][InlineData("composition-namespace")][InlineData("composition-modules")]
 [InlineData("source-added")][InlineData("source-removed")][InlineData("grammar-added")][InlineData("artifact-added")][InlineData("review-added")][InlineData("evidence-added")]
 [InlineData("fourth-artifact-added")][InlineData("fourth-evidence-added")][InlineData("third-evidence-added")]
 [InlineData("source-code")][InlineData("grammar-code")][InlineData("dependency-code")][InlineData("inventory-code")][InlineData("context-report")]
 [InlineData("source-authority")][InlineData("unicode")][InlineData("modifier-boundary")][InlineData("async-semantic-boundary")]
 [InlineData("source-index")][InlineData("source-body")][InlineData("carried-owner")][InlineData("unknown-policy")]
 [InlineData("occurrence-review")][InlineData("occurrence-basis")][InlineData("occurrence-domain")][InlineData("occurrence-reference")]
 [InlineData("missing-addition")][InlineData("addition-policy")][InlineData("addition-appendix")][InlineData("addition-review")][InlineData("addition-line")]
 [InlineData("witness-link")][InlineData("witness-source")][InlineData("specific-case")][InlineData("fourth-matrix")][InlineData("predecessor-case")]
 [InlineData("main-disposition")][InlineData("main-index")][InlineData("predecessor-overlay")][InlineData("predecessor-crosswalk")]
 public void Source_factory_policy_and_evidence_drift_fail_closed(string change)
 {
  var inputs=Inputs();byte[] gate=Read("csharp5-coverage-gate.json"),composition=Read("composition.json");
  byte[] Edit(byte[] b,Action<JsonNode> edit){var n=JsonNode.Parse(b)!;edit(n);return Encoding.UTF8.GetBytes(n.ToJsonString());}
  void Artifact(string name,Action<JsonNode> edit){string key="CSNitrogen.csharp5-artifact.csharp5-"+name+".json";inputs[key]=Edit(inputs[key],edit);}
  void Append(string key){inputs[key]=inputs[key].Concat(new byte[]{32}).ToArray();}
  JsonNode Addition(JsonNode n,string name)=>n["mainTextAdditions"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>().EndsWith(":"+name,StringComparison.Ordinal))!;
  switch(change)
  {
   case "ledger":gate=Edit(gate,n=>n["status"]="pending");break;
   case "composition-start":case "composition-namespace":case "composition-modules":
    composition=Edit(composition,n=>{var f=n["languages"]!.AsArray().Single(l=>l!["name"]!.GetValue<string>()=="CSharp5")!;if(change=="composition-start")f["start"]="CSharp.Core.LegacyCompilationUnit";else if(change=="composition-namespace")f["namespace"]="Invented.Syntax";else f["grammars"]!.AsArray().RemoveAt(f["grammars"]!.AsArray().Count-1);});break;
   case "source-added":inputs["CSNitrogen.source.Parsing/Unreviewed.cs"]=[32];break;
   case "source-removed":inputs.Remove("CSNitrogen.source.Parsing/CSharp5ContextPolicy.cs");break;
   case "grammar-added":inputs["CSNitrogen.grammar.Unreviewed.ngr"]=[32];break;
   case "artifact-added":inputs["CSNitrogen.csharp5-artifact.unreviewed.json"]=[32];break;
   case "review-added":inputs["CSNitrogen.csharp5-review.unreviewed.md"]=[32];break;
   case "evidence-added":inputs["CSNitrogen.csharp5-evidence.unreviewed.json"]=[32];break;
   case "fourth-artifact-added":inputs["CSNitrogen.csharp4-artifact.unreviewed.json"]=[32];break;
   case "fourth-evidence-added":inputs["CSNitrogen.csharp4-evidence.unreviewed.json"]=[32];break;
   case "third-evidence-added":inputs["CSNitrogen.csharp3-evidence.unreviewed.json"]=[32];break;
   case "source-code":Append("CSNitrogen.source.Parsing/CSharp5ContextPolicy.cs");break;
   case "grammar-code":Append("CSNitrogen.grammar.AwaitExpressions.ngr");break;
   case "dependency-code":Append("CSNitrogen.dependencies.json");break;
   case "inventory-code":Append("CSNitrogen.coverage.json");break;
   case "context-report":Append("CSNitrogen.csharp5-review.csharp5-normative-context-review.md");break;
   case "source-authority":Artifact("reference-policy",n=>n["primary"]!["sha256"]="invented");break;
   case "unicode":Artifact("reference-policy",n=>n["unicode"]!["policy"]="unicode-current-bmp");break;
   case "modifier-boundary":Artifact("reference-policy",n=>{var a=n["normalizedExclusions"]!.AsArray();a.Remove(a.Single(x=>x!.GetValue<string>()=="general-modifier-compatibility-and-production-finite-sets"));});break;
   case "async-semantic-boundary":Artifact("reference-policy",n=>{var a=n["normalizedExclusions"]!.AsArray();a.Remove(a.Single(x=>x!.GetValue<string>()=="async-awaitability-task-types-and-return-types"));});break;
   case "source-index":Artifact("standard-index",n=>n["productions"]!.AsArray().RemoveAt(0));break;
   case "source-body":Artifact("standard-crosswalk",n=>n["productions"]![0]!["sourceBodySha256"]="changed");break;
   case "carried-owner":Artifact("standard-crosswalk",n=>n["productions"]![0]!["targets"]!.AsArray().Add("CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.AwaitExpression"));break;
   case "unknown-policy":Artifact("standard-crosswalk",n=>n["productions"]![0]!["policies"]!.AsArray().Add("invented"));break;
   case "occurrence-review":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["reviews"]!["normative-prose"]="pending");break;
   case "occurrence-basis":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["basis"]="version5-source-review");break;
   case "occurrence-domain":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["sourceReview"]!["domain"]="invented");break;
   case "occurrence-reference":Artifact("reviewed-occurrences",n=>n["productions"]![0]!["sourceReview"]!["occurrenceId"]="invented");break;
   case "missing-addition":Artifact("standard-crosswalk",n=>n["mainTextAdditions"]!.AsArray().Remove(Addition(n,"await-expression")));break;
   case "addition-policy":Artifact("standard-crosswalk",n=>Addition(n,"await-expression")["policies"]=new JsonArray());break;
   case "addition-appendix":Artifact("standard-crosswalk",n=>Addition(n,"unary-expression")["appendixOccurrence"]="B:9841:lambda-expression");break;
   case "addition-review":Artifact("reviewed-occurrences",n=>Addition(n,"method-modifier")["reviews"]!["spans-diagnostics"]="pending");break;
   case "addition-line":Artifact("reviewed-occurrences",n=>Addition(n,"await-expression")["sourceLineSha256"]="changed");break;
   case "main-disposition":Artifact("main-text-dispositions",n=>n["regions"]!.AsArray().RemoveAt(0));break;
   case "main-index":Artifact("main-text-differences",n=>n["regions"]!.AsArray().RemoveAt(0));break;
   case "predecessor-overlay":Append("CSNitrogen.csharp4-artifact.csharp4-reviewed-occurrences.json");break;
   case "predecessor-crosswalk":Append("CSNitrogen.csharp4-artifact.csharp4-standard-crosswalk.json");break;
   case "witness-link":case "witness-source":case "specific-case":case "fourth-matrix":case "predecessor-case":
    string fixture=change switch{"specific-case"=>"CSNitrogen.csharp5-evidence.source-specific-cases.json","fourth-matrix"=>"CSNitrogen.csharp4-evidence.alternative-witness-matrix.json","predecessor-case"=>"CSNitrogen.csharp3-evidence.expression-production-cases.json",_=>"CSNitrogen.csharp5-evidence.alternative-witness-matrix.json"};
    inputs[fixture]=Edit(inputs[fixture],n=>{if(change=="witness-link")n["cases"]![0]!["witnessId"]="invented";else if(change=="witness-source")n["sourceSha256"]="changed";else if(change=="fourth-matrix")n["cases"]![0]!["witnessId"]="invented";else n["cases"]![0]!["source"]="class Changed{}";});break;
  }
  Assert.False(Evaluate(gate,composition,inputs));
 }
 [Fact]public void Directed_contract_pins_require_real_predecessor_admission_when_fifth_contract_code_changes()
 {
  var inputs=Inputs();Assert.False(CSharp5CoverageContract.Eligible("CSNitrogen.source.Profiles/CSharp5CoverageContract.cs"));Assert.True(CSharp4CoverageContract.Eligible("CSNitrogen.source.Profiles/CSharp5CoverageContract.cs"));
  string key="CSNitrogen.source.Profiles/CSharp5CoverageContract.cs";inputs[key]=inputs[key].Concat(new byte[]{32}).ToArray();
  bool third=CSharp3CoverageContract.Reviewed(Read("csharp3-coverage-gate.json"),Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json"),Read("csharp3-reference-policy.json"),Read("composition.json"),inputs,true);
  bool predecessor=CSharp4CoverageContract.Reviewed(Read("csharp4-coverage-gate.json"),Read("composition.json"),inputs,third);
  Assert.False(predecessor);Assert.False(Evaluate(inputs:inputs,predecessor:predecessor));
 }
}
