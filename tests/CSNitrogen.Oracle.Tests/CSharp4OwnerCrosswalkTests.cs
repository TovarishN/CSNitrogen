using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen.Grammar;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Selected owner existence and provenance do not approve normative source contexts.</summary>
public class CSharp4OwnerCrosswalkTests
{
 static byte[] Bytes(string name)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp4",name));
 static JsonObject Read(string name)=>JsonNode.Parse(Bytes(name))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string[] Strings(JsonNode? n)=>n?.AsArray().Select(v=>v!.GetValue<string>()).ToArray()??[];
 static string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
 [Fact]public void Every_source_occurrence_maps_to_owners_with_separate_snapshot_admission()
 {Assert.Empty(Evaluate(Read("csharp4-standard-crosswalk.json")));Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);}
 [Theory]
 [InlineData("missing","OccurrenceSet")][InlineData("duplicate","OccurrenceSet")][InlineData("invented","OccurrenceSet")]
 [InlineData("mapping","Normalization")][InlineData("body","SourceBody")][InlineData("main","MainReference")][InlineData("domain","SourceDomain")]
 [InlineData("source","SourceIdentity")][InlineData("index","SourceIdentity")][InlineData("predecessor-hash","SourceIdentity")]
 [InlineData("predecessor","PredecessorLink")][InlineData("relation","PredecessorLink")]
 [InlineData("unknown-owner","Owner")][InlineData("duplicate-owner","Owner")][InlineData("missing-owner","Owner")]
 [InlineData("unknown-policy","Policy")][InlineData("factory","Factory")][InlineData("factory-start","Factory")]
 [InlineData("promoted","Status")][InlineData("row-promoted","Status")][InlineData("missing-correction","Correction")]
 [InlineData("unapproved-correction","Correction")][InlineData("missing-name-seam","FourthSeam")]
 public void Artifact_mutations_fail_on_the_relevant_invariant(string change,string expected)
 {
  var map=Read("csharp4-standard-crosswalk.json");var rows=map["productions"]!.AsArray();var first=rows[0]!;
  switch(change)
  {
   case "missing":rows.RemoveAt(0);break;
   case "duplicate":rows.Add(first.DeepClone());break;
   case "invented":first["standardId"]="invented";break;
   case "mapping":first["mapping"]="invented";break;
   case "body":first["sourceBodySha256"]="changed";break;
   case "main":first["mainTextOccurrences"]=new JsonArray(0);break;
   case "domain":first["grammar"]="invented";break;
   case "source":map["sourceSha256"]="changed";break;
   case "index":map["indexSha256"]="changed";break;
   case "predecessor-hash":map["predecessorCrosswalkSha256"]="changed";break;
   case "predecessor":first["predecessorId"]="invented";break;
   case "relation":first["bodyRelation"]="added";break;
   case "unknown-owner":first["targets"]!.AsArray().Add("CSharp.NamedArguments/rule:Invented");break;
   case "duplicate-owner":first["targets"]!.AsArray().Add(first["targets"]![0]!.DeepClone());break;
   case "missing-owner":first["targets"]=new JsonArray();first["policies"]=new JsonArray();break;
   case "unknown-policy":first["policies"]!.AsArray().Add("invented");break;
   case "factory":map["factory"]!["grammars"]!.AsArray().RemoveAt(0);break;
   case "factory-start":map["factory"]!["start"]="Invented";break;
   case "promoted":map["status"]="verified";break;
   case "row-promoted":first["reviewStatus"]="verified";break;
   case "missing-correction":rows.First(r=>r!["sourceCorrection"] is not null)!.AsObject().Remove("sourceCorrection");break;
   case "unapproved-correction":map["sourceCorrections"]!["approved-generic-member-suffix"]!["status"]="approved";break;
   case "missing-name-seam":rows.Single(r=>Text(r!,"standardId")=="B:8854:argument-name")!["targets"]=new JsonArray("CSharp.Expressions/rule:Argument");break;
  }
  Assert.Contains(Evaluate(map),e=>e.StartsWith(expected,StringComparison.Ordinal));Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject map)
 {
  var errors=new List<string>();
  void Exact(string label,IEnumerable<string> a,IEnumerable<string> b)
  {var expected=a.Order(StringComparer.Ordinal).ToArray();var actual=b.Order(StringComparer.Ordinal).ToArray();if(!expected.SequenceEqual(actual)||actual.Distinct().Count()!=actual.Length)errors.Add(label);}
  try
  {
   var index=Read("csharp4-standard-index.json");var delta=Read("csharp4-source-delta-inventory.json");
   var assembly=typeof(FeatureCatalog).Assembly;
   string Resource(string name){using var s=assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var reader=new StreamReader(s);return reader.ReadToEnd();}
   string oldText=Resource("csharp3-standard-crosswalk.json");var old=JsonNode.Parse(oldText)!;
   const string source="cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7";
   if(Text(map,"sourceSha256")!=source||Text(map,"profile")!="CSharp4"||map["schemaVersion"]!.GetValue<int>()!=1||Text(map,"indexSha256")!=Hash(Bytes("csharp4-standard-index.json"))||Text(map,"deltaSha256")!=Hash(Bytes("csharp4-source-delta-inventory.json"))||Text(map,"predecessorCrosswalkSha256")!=Hash(System.Text.Encoding.UTF8.GetBytes(oldText)))errors.Add("SourceIdentity");
   if(Text(map,"status")!="owner-indexed-unreviewed")errors.Add("Status: candidate is not admission");
   var headings=index["productions"]!.AsArray().ToDictionary(r=>Text(r!,"id"),r=>r!);var links=delta["productions"]!.AsArray().ToDictionary(r=>Text(r!,"standardId"),r=>r!);var oldRows=old["productions"]!.AsArray().ToDictionary(r=>Text(r!,"standardId"),r=>r!);var rows=map["productions"]!.AsArray();
   if(headings.Count!=458)errors.Add("OccurrenceSet: source count");Exact("OccurrenceSet",headings.Keys,rows.Select(r=>Text(r!,"standardId")));
   var configured=JsonNode.Parse(Resource("composition.json"))!["languages"]!.AsArray().Single(l=>Text(l!,"name")=="CSharp4")!;
   var factory=map["factory"]!;Exact("Factory",Strings(configured["grammars"]),Strings(factory["grammars"]));
   if(Text(factory,"name")!="CSharp4"||Text(factory,"start")!=Text(configured,"start")||CSharp4Composition.Start.Name!="CompilationUnit")errors.Add("Factory: identity");
   var selected=CSharp4Composition.CreateBuilder().Build();var modules=new Dictionary<string,ModuleDecl>();
   foreach(string path in Strings(configured["grammars"]))
   {
    var parsed=GrammarParser.Parse(Resource("grammar."+path["Grammars/".Length..]));
    if(!parsed.Success||parsed.Diagnostics.Count!=0){errors.Add("Factory: grammar");continue;}
    var module=parsed.File!.Modules.Single();modules.Add(module.Name,module);
   }
   Exact("Factory",modules.Keys,selected.Modules.Select(m=>m.Name));if(modules.Count!=30)errors.Add("Factory: expected30 modules");
   var owners=new HashSet<string>(StringComparer.Ordinal);
   foreach(var m in modules.Values)
   {
    foreach(var r in m.Rules)owners.Add(m.Name+"/rule:"+r.Name);
    foreach(var r in m.Rules.OfType<ExtensibleRule>())foreach(var a in r.Alternatives)owners.Add(m.Name+"/alternative:"+r.Name+"."+a.Name);
    foreach(var e in m.Extends)foreach(var a in e.Alternatives)owners.Add(m.Name+"/extension:"+e.Target+"."+a.Name);
   }
   var correction=map["sourceCorrections"]!["approved-generic-member-suffix"]!;
   var expectedCorrections=links.Values.Where(l=>oldRows.TryGetValue(Text(l,"predecessorId"),out var p)&&p["sourceCorrection"] is not null).Select(l=>Text(l,"standardId")).ToArray();
   if(Text(correction,"status")!="approved-predecessor-continuity"||Text(correction,"decision")!=Text(old["sourceCorrections"]!["approved-generic-member-suffix"]!,"decision"))errors.Add("Correction: approval continuity");
   Exact("Correction",expectedCorrections,Strings(correction["occurrences"]));Exact("Correction",expectedCorrections,rows.Where(r=>r!["sourceCorrection"] is not null).Select(r=>Text(r!,"standardId")));
   foreach(var row in rows)
   {
    string id=Text(row!,"standardId");if(!headings.TryGetValue(id,out var heading))continue;var link=links[id];
    if(Text(row!,"sourceBodySha256")!=Text(heading,"sourceBodySha256"))errors.Add("SourceBody: "+id);
    if(Text(row!,"mapping") is not ("inlined" or "normalized" or "policy" or "semantic-wrapper" or "shared-syntax"))errors.Add("Normalization: "+id);
    if(Text(row!,"grammar")!=Text(heading,"grammar"))errors.Add("SourceDomain: "+id);
    if(!row!["mainTextOccurrences"]!.ToJsonString().Equals(heading["mainTextOccurrences"]!.ToJsonString(),StringComparison.Ordinal))errors.Add("MainReference: "+id);
    if(Text(row,"predecessorId")!=Text(link,"predecessorId")||Text(row,"bodyRelation")!=Text(link,"bodyRelation"))errors.Add("PredecessorLink: "+id);
    if(Text(row,"reviewStatus")!="owner-candidate")errors.Add("Status: "+id);
    var targets=Strings(row["targets"]);var policies=Strings(row["policies"]);
    if(targets.Length+policies.Length==0||targets.Distinct().Count()!=targets.Length||targets.Any(t=>!owners.Contains(t)))errors.Add("Owner: "+id);
    if(policies.Distinct().Count()!=policies.Length||policies.Any(p=>!FeatureCatalog.KnownPolicies.Contains(p)))errors.Add("Policy: "+id);
    if(row["sourceCorrection"] is not null&&Text(row,"sourceCorrection")!="approved-generic-member-suffix")errors.Add("Correction: "+id);
   }
   Require("B:8854:argument-name","CSharp.NamedArguments/extension:CSharp.Expressions.ArgumentName.NamedArgumentName");
   Require("B:9048:default-argument","CSharp.OptionalParameters/extension:CSharp.Attributes.ParameterDefault.DefaultValue");
   Require("B:9114:variance-annotation","CSharp.Variance/extension:CSharp.Generics.VarianceAnnotation.Covariant","CSharp.Variance/extension:CSharp.Generics.VarianceAnnotation.Contravariant");
   void Require(string id,params string[] expected){var row=rows.FirstOrDefault(r=>Text(r!,"standardId")==id);if(row is null||expected.Any(o=>!Strings(row["targets"]).Contains(o)))errors.Add("FourthSeam: "+id);}
  }
  catch(Exception e){errors.Add("Malformed: "+e.GetType().Name+": "+e.Message);}
  return errors;
 }
}
