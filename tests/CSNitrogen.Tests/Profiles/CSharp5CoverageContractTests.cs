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
 static string Hash(byte[] b)=>Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(b));
 /// <summary>Applies a mutation, rebinds every cross-artifact hash and re-pins the ledger, so only structural checks can reject it.</summary>
 static bool Repinned(Action<Func<string,JsonNode>,JsonNode>? mutate=null)
 {
  var inputs=Inputs();var nodes=new Dictionary<string,JsonNode>(StringComparer.Ordinal);
  string A(string name)=>"CSNitrogen.csharp5-artifact.csharp5-"+name+".json";
  const string Matrix="CSNitrogen.csharp5-evidence.alternative-witness-matrix.json",Specific="CSNitrogen.csharp5-evidence.source-specific-cases.json",Report="CSNitrogen.csharp5-review.csharp5-normative-context-review.md";
  JsonNode Node(string key)=>nodes.TryGetValue(key,out var n)?n:nodes[key]=JsonNode.Parse(inputs[key])!;
  byte[] Bytes(string key)=>nodes.TryGetValue(key,out var n)?inputs[key]=Encoding.UTF8.GetBytes(n.ToJsonString()):inputs[key];
  var gate=JsonNode.Parse(Read("csharp5-coverage-gate.json"))!;
  mutate?.Invoke(Node,gate);
  var map=Node(A("standard-crosswalk"));map["indexSha256"]=Hash(Bytes(A("standard-index")));map["deltaSha256"]=Hash(Bytes(A("source-delta-inventory")));
  var disp=Node(A("main-text-dispositions"));disp["differenceIndexSha256"]=Hash(Bytes(A("main-text-differences")));disp["reportSha256"]=Hash(Bytes(Report));
  var matrix=Node(Matrix);matrix["indexSha256"]=Hash(Bytes(A("standard-index")));matrix["deltaSha256"]=Hash(Bytes(A("source-delta-inventory")));matrix["sourceSpecific"]!["sha256"]=Hash(Bytes(Specific));
  var review=Node(A("reviewed-occurrences"));review["ownerCrosswalkSha256"]=Hash(Bytes(A("standard-crosswalk")));review["contextReportSha256"]=Hash(Bytes(Report));
  review["mainTextDispositionsSha256"]=Hash(Bytes(A("main-text-dispositions")));review["alternativeMatrixSha256"]=Hash(Bytes(Matrix));
  foreach(var key in nodes.Keys.ToArray())Bytes(key);
  var pins=new JsonObject();foreach(var key in inputs.Keys.Where(CSharp5CoverageContract.Eligible).Order(StringComparer.Ordinal))pins[key]=Hash(inputs[key]);
  gate["reviewInputs"]=pins;byte[] ledger=Encoding.UTF8.GetBytes(gate.ToJsonString());
  return CSharp5CoverageContract.Reviewed(ledger,Read("composition.json"),inputs,true,Hash(ledger));
 }
 static JsonNode Row(JsonNode map,string id)=>map["productions"]!.AsArray().Single(r=>r!["standardId"]!.GetValue<string>()==id)!;
 static JsonNode Added(JsonNode map,string name)=>map["mainTextAdditions"]!.AsArray().Single(a=>a!["id"]!.GetValue<string>().EndsWith(":"+name,StringComparison.Ordinal))!;
 [Fact]public void Repinned_unmutated_inputs_are_admitted_so_structural_mutations_are_meaningful()
 {Assert.True(Repinned());}
 [Theory]
 [InlineData("carried-owner")][InlineData("carried-policy")][InlineData("duplicate-target")][InlineData("correction-dropped")][InlineData("correction-occurrences")]
 [InlineData("undeclared-owner")][InlineData("gate-owner")][InlineData("occurrence-basis")][InlineData("overlay-status")][InlineData("addition-dimension")]
 [InlineData("addition-appendix")][InlineData("addition-name")][InlineData("witness-link")][InlineData("witness-kind")][InlineData("disposition-category")][InlineData("marker-reference")]
 public void Structural_drift_fails_closed_even_when_every_hash_is_rebound(string change)
 {
  Assert.False(Repinned((node,gate)=>
  {
   var map=node("CSNitrogen.csharp5-artifact.csharp5-standard-crosswalk.json");var review=node("CSNitrogen.csharp5-artifact.csharp5-reviewed-occurrences.json");
   var row=map["productions"]![0]!;
   switch(change)
   {
    case "carried-owner":
     var targets=row["targets"]!.AsArray();var other=gate["owners"]!.AsArray().Select(o=>o!.GetValue<string>()).First(o=>targets.All(t=>t!.GetValue<string>()!=o));targets.Add(other);break;
    case "carried-policy":row["policies"]!.AsArray().Add("csharp5.source-context-checks");break;
    case "duplicate-target":var first=map["productions"]!.AsArray().First(r=>r!["targets"]!.AsArray().Count>0)!;first["targets"]!.AsArray().Add(first["targets"]![0]!.DeepClone());break;
    case "correction-dropped":Row(map,"B:9794:member-access").AsObject().Remove("sourceCorrection");break;
    case "correction-occurrences":map["sourceCorrections"]!["approved-generic-member-suffix"]!["occurrences"]=new JsonArray("B:8860:member-access","B:8865:base-access","B:9178:pointer-member-access");break;
    case "undeclared-owner":
     const string real="CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.AwaitExpression",fake="CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.Invented";
     foreach(var a in map["mainTextAdditions"]!.AsArray()){var t=a!["targets"]!.AsArray();var i=t.Select(x=>x!.GetValue<string>()).ToList().IndexOf(real);if(i>=0)t[i]=fake;}
     var owners=gate["owners"]!.AsArray();owners[owners.Select(x=>x!.GetValue<string>()).ToList().IndexOf(real)]=fake;break;
    case "gate-owner":gate["owners"]!.AsArray().Add("CSharp.Expressions/rule:Expression#extra");break;
    case "occurrence-basis":review["productions"]![0]!["basis"]="version5-source-review";break;
    case "overlay-status":review["status"]="pending";break;
    case "addition-dimension":Added(review,"method-modifier")["reviews"]!["ambiguity-context"]="pending";break;
    case "addition-appendix":Added(map,"unary-expression")["appendixOccurrence"]="B:9841:lambda-expression";break;
    case "addition-name":Added(map,"await-expression")["name"]="awaited-expression";break;
    case "witness-link":node("CSNitrogen.csharp5-evidence.alternative-witness-matrix.json")["cases"]![0]!["witnessId"]="invented";break;
    case "witness-kind":node("CSNitrogen.csharp5-evidence.alternative-witness-matrix.json")["cases"]![0]!["sourceKind"]="source-specific";break;
    case "disposition-category":node("CSNitrogen.csharp5-artifact.csharp5-main-text-dispositions.json")["regions"]![0]!["disposition"]="invented";break;
    case "marker-reference":
     var marker=node("CSNitrogen.csharp5-artifact.csharp5-standard-index.json")["productions"]!.AsArray().SelectMany(p=>p!["alternatives"]!.AsArray()).First(a=>a!["kind"]!.GetValue<string>()=="inherited-placeholder")!;
     marker["earlierSourceAlternativeIds"]!.AsArray().RemoveAt(0);break;
   }
  }));
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
