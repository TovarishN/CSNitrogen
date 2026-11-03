using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen.Grammar;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Version5 owners resolve in the actual 32-module factory; owner existence does not approve source contexts.</summary>
public class CSharp5OwnerCrosswalkTests
{
 static byte[] Bytes(string name)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp5",name));
 static JsonObject Read(string name)=>JsonNode.Parse(Bytes(name))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string[] Strings(JsonNode? n)=>n?.AsArray().Select(v=>v!.GetValue<string>()).ToArray()??[];
 static string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
 static readonly string[] AdditionNames=["unary-expression","await-expression","lambda-expression","anonymous-method-expression","statement-expression","method-modifier"];
 [Fact]public void Every_fifth_occurrence_and_main_text_addition_maps_to_actual_owners()
 {Assert.Empty(Evaluate(Read("csharp5-standard-crosswalk.json")));Assert.True(ProfileResolver.Resolve("CSharp5").Profile!.Complete);}
 [Fact]public void Runtime_owner_scanner_matches_the_grammar_parser_for_the_fifth_factory()
 {
  var assembly=typeof(FeatureCatalog).Assembly;
  string Resource(string name){using var s=assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var reader=new StreamReader(s);return reader.ReadToEnd();}
  var grammars=Strings(JsonNode.Parse(Resource("composition.json"))!["languages"]!.AsArray().Single(l=>Text(l!,"name")=="CSharp5")!["grammars"]).Select(g=>Resource("grammar."+g["Grammars/".Length..])).ToArray();
  var expected=new HashSet<string>(StringComparer.Ordinal);
  foreach(var text in grammars)
  {
   var m=GrammarParser.Parse(text).File!.Modules.Single();
   foreach(var r in m.Rules)expected.Add(m.Name+"/rule:"+r.Name);
   foreach(var r in m.Rules.OfType<ExtensibleRule>())foreach(var a in r.Alternatives)expected.Add(m.Name+"/alternative:"+r.Name+"."+a.Name);
   foreach(var e in m.Extends)foreach(var a in e.Alternatives)expected.Add(m.Name+"/extension:"+e.Target+"."+a.Name);
  }
  var scanned=CSharp5CoverageContract.DeclaredOwners(grammars);
  Assert.Equal(expected.Order(StringComparer.Ordinal),scanned.Order(StringComparer.Ordinal));
 }
 [Theory]
 [InlineData("missing","OccurrenceSet")][InlineData("duplicate","OccurrenceSet")][InlineData("invented","OccurrenceSet")]
 [InlineData("body","SourceBody")][InlineData("main","MainReference")][InlineData("domain","SourceDomain")]
 [InlineData("source","SourceIdentity")][InlineData("index","SourceIdentity")][InlineData("predecessor-hash","SourceIdentity")]
 [InlineData("predecessor","PredecessorLink")][InlineData("relation","PredecessorLink")]
 [InlineData("carried-owner","CarriedOwner")][InlineData("carried-policy","CarriedOwner")][InlineData("carried-mapping","CarriedOwner")]
 [InlineData("unknown-owner","Owner")][InlineData("factory","Factory")][InlineData("factory-start","Factory")]
 [InlineData("promoted","Status")][InlineData("row-promoted","Status")]
 [InlineData("missing-addition","Addition")][InlineData("addition-owner","Owner")][InlineData("addition-policy","Addition")]
 [InlineData("addition-appendix","Addition")][InlineData("await-owner","FifthSeam")]
 public void Artifact_mutations_fail_on_the_relevant_invariant(string change,string expected)
 {
  var map=Read("csharp5-standard-crosswalk.json");var rows=map["productions"]!.AsArray();var first=rows[0]!;var additions=map["mainTextAdditions"]!.AsArray();
  var await=additions.Single(a=>Text(a!,"name")=="await-expression")!;
  switch(change)
  {
   case "missing":rows.RemoveAt(0);break;
   case "duplicate":rows.Add(first.DeepClone());break;
   case "invented":first["standardId"]="invented";break;
   case "body":first["sourceBodySha256"]="changed";break;
   case "main":first["mainTextOccurrences"]=new JsonArray(0);break;
   case "domain":first["grammar"]="invented";break;
   case "source":map["sourceSha256"]="changed";break;
   case "index":map["indexSha256"]="changed";break;
   case "predecessor-hash":map["predecessorCrosswalkSha256"]="changed";break;
   case "predecessor":first["predecessorId"]="invented";break;
   case "relation":first["bodyRelation"]="added";break;
   case "carried-owner":first["targets"]!.AsArray().Add("CSharp.Expressions/rule:Expression");break;
   case "carried-policy":first["policies"]=new JsonArray("csharp5.source-context-checks");break;
   case "carried-mapping":first["mapping"]="normalized";break;
   case "unknown-owner":await["targets"]!.AsArray().Add("CSharp.AwaitExpressions/rule:Invented");break;
   case "factory":map["factory"]!["grammars"]!.AsArray().RemoveAt(0);break;
   case "factory-start":map["factory"]!["start"]="Invented";break;
   case "promoted":map["status"]="verified";break;
   case "row-promoted":await["reviewStatus"]="verified";break;
   case "missing-addition":additions.Remove(await);break;
   case "addition-owner":await["targets"]=new JsonArray("CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.Invented");break;
   case "addition-policy":await["policies"]=new JsonArray();break;
   case "addition-appendix":additions.Single(a=>Text(a!,"name")=="unary-expression")!["appendixOccurrence"]="B:9841:lambda-expression";break;
   case "await-owner":await["targets"]=new JsonArray("CSharp.Expressions/rule:Expression");break;
  }
  Assert.Contains(Evaluate(map),e=>e.StartsWith(expected,StringComparison.Ordinal));
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject map)
 {
  var errors=new List<string>();
  void Exact(string label,IEnumerable<string> a,IEnumerable<string> b)
  {var expected=a.Order(StringComparer.Ordinal).ToArray();var actual=b.Order(StringComparer.Ordinal).ToArray();if(!expected.SequenceEqual(actual)||actual.Distinct().Count()!=actual.Length)errors.Add(label);}
  try
  {
   var index=Read("csharp5-standard-index.json");var delta=Read("csharp5-source-delta-inventory.json");
   var assembly=typeof(FeatureCatalog).Assembly;
   string Resource(string name){using var s=assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var reader=new StreamReader(s);return reader.ReadToEnd();}
   string oldText=Resource("csharp4-artifact.csharp4-standard-crosswalk.json");var old=JsonNode.Parse(oldText)!;
   const string source="6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519";
   if(Text(map,"sourceSha256")!=source||Text(map,"profile")!="CSharp5"||map["schemaVersion"]!.GetValue<int>()!=1||Text(map,"indexSha256")!=Hash(Bytes("csharp5-standard-index.json"))||Text(map,"deltaSha256")!=Hash(Bytes("csharp5-source-delta-inventory.json"))||Text(map,"predecessorCrosswalkSha256")!=Hash(System.Text.Encoding.UTF8.GetBytes(oldText)))errors.Add("SourceIdentity");
   if(Text(map,"status")!="owner-indexed-unreviewed")errors.Add("Status: candidate is not admission");
   var headings=index["productions"]!.AsArray().ToDictionary(r=>Text(r!,"id"),r=>r!);var links=delta["productions"]!.AsArray().ToDictionary(r=>Text(r!,"standardId"),r=>r!);var oldRows=old["productions"]!.AsArray().ToDictionary(r=>Text(r!,"standardId"),r=>r!);var rows=map["productions"]!.AsArray();
   if(headings.Count!=458)errors.Add("OccurrenceSet: source count");Exact("OccurrenceSet",headings.Keys,rows.Select(r=>Text(r!,"standardId")));
   var configured=JsonNode.Parse(Resource("composition.json"))!["languages"]!.AsArray().Single(l=>Text(l!,"name")=="CSharp5")!;
   var factory=map["factory"]!;Exact("Factory",Strings(configured["grammars"]),Strings(factory["grammars"]));
   if(Text(factory,"name")!="CSharp5"||Text(factory,"start")!=Text(configured,"start")||CSharp5Composition.Start.Name!="CompilationUnit")errors.Add("Factory: identity");
   var selected=CSharp5Composition.CreateBuilder().Build();var modules=new Dictionary<string,ModuleDecl>();
   foreach(string path in Strings(configured["grammars"]))
   {
    var parsed=GrammarParser.Parse(Resource("grammar."+path["Grammars/".Length..]));
    if(!parsed.Success||parsed.Diagnostics.Count!=0){errors.Add("Factory: grammar");continue;}
    var module=parsed.File!.Modules.Single();modules.Add(module.Name,module);
   }
   Exact("Factory",modules.Keys,selected.Modules.Select(m=>m.Name));if(modules.Count!=32)errors.Add("Factory: expected32 modules");
   var owners=new HashSet<string>(StringComparer.Ordinal);
   foreach(var m in modules.Values)
   {
    foreach(var r in m.Rules)owners.Add(m.Name+"/rule:"+r.Name);
    foreach(var r in m.Rules.OfType<ExtensibleRule>())foreach(var a in r.Alternatives)owners.Add(m.Name+"/alternative:"+r.Name+"."+a.Name);
    foreach(var e in m.Extends)foreach(var a in e.Alternatives)owners.Add(m.Name+"/extension:"+e.Target+"."+a.Name);
   }
   foreach(var row in rows)
   {
    string id=Text(row!,"standardId");if(!headings.TryGetValue(id,out var heading))continue;var link=links[id];
    if(Text(row!,"sourceBodySha256")!=Text(heading,"sourceBodySha256"))errors.Add("SourceBody: "+id);
    if(Text(row!,"grammar")!=Text(heading,"grammar"))errors.Add("SourceDomain: "+id);
    if(!row!["mainTextOccurrences"]!.ToJsonString().Equals(heading["mainTextOccurrences"]!.ToJsonString(),StringComparison.Ordinal))errors.Add("MainReference: "+id);
    if(Text(row,"predecessorId")!=Text(link,"predecessorId")||Text(row,"bodyRelation")!=Text(link,"bodyRelation"))errors.Add("PredecessorLink: "+id);
    if(Text(row,"reviewStatus")!="owner-candidate")errors.Add("Status: "+id);
    // Appendix owners are the admitted fourth owners, exactly; additions live in mainTextAdditions.
    if(!oldRows.TryGetValue(Text(row,"predecessorId"),out var prior)||row["targets"]!.ToJsonString()!=prior["targets"]!.ToJsonString()||row["policies"]!.ToJsonString()!=prior["policies"]!.ToJsonString()||Text(row,"mapping")!=Text(prior,"mapping")||Text(row,"sourceCorrection")!=Text(prior,"sourceCorrection"))errors.Add("CarriedOwner: "+id);
    var targets=Strings(row["targets"]);var policies=Strings(row["policies"]);
    if(targets.Length+policies.Length==0||targets.Any(t=>!owners.Contains(t)))errors.Add("Owner: "+id);
    if(policies.Any(p=>!FeatureCatalog.KnownPolicies.Contains(p)))errors.Add("Policy: "+id);
   }
   var additions=map["mainTextAdditions"]!.AsArray();Exact("Addition: name set",AdditionNames,additions.Select(a=>Text(a!,"name")));
   foreach(var a in additions)
   {
    string name=Text(a!,"name");string appendix=Text(a!,"appendixOccurrence");
    if(Text(a!,"id")!=$"M:{a!["mainTextLine"]!.GetValue<int>()}:{name}")errors.Add("Addition: id "+name);
    if(Text(a,"reviewStatus")!="owner-candidate")errors.Add("Status: "+name);
    if(name=="await-expression"?a["appendixOccurrence"] is not null:!headings.TryGetValue(appendix,out var omitted)||Text(omitted,"name")!=name)errors.Add("Addition: appendix "+name);
    if(!Strings(a["policies"]).SequenceEqual(["csharp5.source-context-checks"]))errors.Add("Addition: policy "+name);
    var targets=Strings(a["targets"]);if(targets.Length==0||targets.Distinct().Count()!=targets.Length||targets.Any(t=>!owners.Contains(t)))errors.Add("Owner: "+name);
   }
   Require("await-expression","CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.AwaitExpression");
   Require("method-modifier","CSharp.AsyncFunctions/extension:CSharp.Declarations.Modifier.AsyncModifier");
   Require("lambda-expression","CSharp.AsyncFunctions/extension:CSharp.AnonymousMethods.AsyncMarker.AsyncFunction");
   void Require(string name,string owner){var a=additions.FirstOrDefault(x=>Text(x!,"name")==name);if(a is null||!Strings(a["targets"]).Contains(owner))errors.Add("FifthSeam: "+name);}
  }
  catch(Exception e){errors.Add("Malformed: "+e.GetType().Name+": "+e.Message);}
  return errors;
 }
}
