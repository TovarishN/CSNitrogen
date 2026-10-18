using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen.Grammar;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Candidate integrity is distinct from normative review and admission.</summary>
public class CSharp3CrosswalkCandidateTests
{
 private static JsonObject Read(string name)
 {using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)??throw new InvalidDataException(name);return JsonNode.Parse(s)!.AsObject();}
 [Fact]public void Candidate_covers_every_occurrence_with_existing_selected_factory_owners()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json")));}
 [Theory]
 [InlineData("missing-row")][InlineData("duplicate-row")][InlineData("wrong-body")]
 [InlineData("unknown-owner")][InlineData("duplicate-owner")][InlineData("unknown-policy")]
 [InlineData("unapproved-correction")][InlineData("missing-correction")][InlineData("promoted-status")]
 [InlineData("missing-correction-declaration")][InlineData("unapproved-correction-declaration")]
 [InlineData("wrong-main-reference")]
 public void Candidate_drift_is_detected_without_admitting_the_profile(string change)
 {
  var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var rows=map["productions"]!.AsArray();var row=rows[0]!;
  switch(change)
  {
   case "missing-row":rows.RemoveAt(0);break;
   case "duplicate-row":rows.Add(row.DeepClone());break;
   case "wrong-body":row["sourceBodySha256"]=new string('0',64);break;
   case "unknown-owner":row["targets"]!.AsArray().Add("CSharp.Expressions/rule:Invented");break;
   case "duplicate-owner":row["targets"]!.AsArray().Add(row["targets"]![0]!.DeepClone());break;
   case "unknown-policy":row["policies"]!.AsArray().Add("invented-policy");break;
   case "unapproved-correction":row["sourceCorrection"]="approved-generic-member-suffix";break;
   case "missing-correction":rows.Single(r=>Text(r!,"standardId")=="B:8548:base-access")!.AsObject().Remove("sourceCorrection");break;
   case "promoted-status":row["status"]="verified";break;
   case "missing-correction-declaration":map["sourceCorrections"]!.AsObject().Remove("approved-generic-member-suffix");break;
   case "unapproved-correction-declaration":map["sourceCorrections"]!["approved-generic-member-suffix"]!["status"]="pending";break;
   case "wrong-main-reference":row["mainTextOccurrences"]=new JsonArray(0);break;
  }
  Assert.NotEmpty(Evaluate(index,map));Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 private static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 private static string[] Strings(JsonNode? n)=>n?.AsArray().Select(v=>v!.GetValue<string>()).ToArray()??[];
 internal static IReadOnlyList<string> Evaluate(JsonObject index,JsonObject map)
 {
  var errors=new List<string>();
  void Exact(string label,IEnumerable<string> expected,IEnumerable<string> actual)
  {var a=expected.Order(StringComparer.Ordinal).ToArray();var b=actual.Order(StringComparer.Ordinal).ToArray();if(!a.SequenceEqual(b)||b.Distinct().Count()!=b.Length)errors.Add(label);}
  try
  {
   const string sourceHash="ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1";
   if(Text(map,"profile")!="CSharp3"||Text(map,"status")!="candidate"||Text(map,"sourceSha256")!=sourceHash||Text(index,"sourceSha256")!=sourceHash)errors.Add("authority");
   var headings=index["productions"]!.AsArray().ToDictionary(n=>Text(n!,"id"),n=>n!);var rows=map["productions"]!.AsArray();
   if(headings.Count!=455)errors.Add("source count");
   Exact("occurrences",headings.Keys,rows.Select(n=>Text(n!,"standardId")));
   var assembly=typeof(FeatureCatalog).Assembly;var factory=CSharp3Composition.CreateBuilder().Build();var owners=new HashSet<string>();
   var declaredModules=assembly.GetManifestResourceNames().Where(n=>n.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal))
    .SelectMany(n=>ReadGrammar(n).File!.Modules).ToDictionary(m=>m.Name,StringComparer.Ordinal);
   foreach(var module in factory.Modules)
   {
    var declared=declaredModules[module.Name];
    foreach(var r in declared.Rules)owners.Add(declared.Name+"/rule:"+r.Name);
    foreach(var r in declared.Rules.OfType<ExtensibleRule>())foreach(var a in r.Alternatives)owners.Add(declared.Name+"/alternative:"+r.Name+"."+a.Name);
    foreach(var e in declared.Extends)foreach(var a in e.Alternatives)owners.Add(declared.Name+"/extension:"+e.Target+"."+a.Name);
   }
   var corrections=new[]{"B:8542:member-access","B:8548:base-access","B:8856:pointer-member-access"};
   var declaration=map["sourceCorrections"]!["approved-generic-member-suffix"]!;
   if(Text(declaration,"status")!="approved")errors.Add("correction declaration");
   Exact("declared occurrences",corrections,Strings(declaration["occurrences"]));
   Exact("corrections",corrections,rows.Where(r=>r!["sourceCorrection"] is not null).Select(r=>Text(r!,"standardId")));
   var policy=Read("csharp3-reference-policy.json")["sourceCorrections"]!;
   if(Text(policy,"status")!="approved")errors.Add("approval");Exact("approved occurrences",corrections,Strings(policy["occurrences"]));
   foreach(var row in rows)
   {
    string id=Text(row!,"standardId");if(!headings.TryGetValue(id,out var heading))continue;
    if(Text(row!,"sourceBodySha256")!=Text(heading,"sourceBodySha256"))errors.Add("body "+id);
    var main=row!["mainTextOccurrences"]!.AsArray().Select(n=>n!.GetValue<int>()).ToArray();
    if(main.Length==0||main.Any(n=>n is <1 or >=8391)||!main.SequenceEqual(main.Distinct().Order()))errors.Add("main reference "+id);
    if(Text(row!,"status")!="candidate"||Text(row!,"reviewStatus")!="pending")errors.Add("review "+id);
    var targets=Strings(row!["targets"]);var policies=Strings(row["policies"]);
    if(targets.Length+policies.Length==0||targets.Distinct().Count()!=targets.Length||targets.Any(t=>!owners.Contains(t)))errors.Add("owner "+id);
    if(policies.Distinct().Count()!=policies.Length||policies.Any(p=>!FeatureCatalog.KnownPolicies.Contains(p)))errors.Add("policy "+id);
    if(row["sourceCorrection"] is not null&&Text(row,"sourceCorrection")!="approved-generic-member-suffix")errors.Add("correction "+id);
   }
   GrammarParseResult ReadGrammar(string resource)
   {using var stream=assembly.GetManifestResourceStream(resource)!;using var reader=new StreamReader(stream);return GrammarParser.Parse(reader.ReadToEnd());}
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
}
