using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3LexicalProductionEvidenceTests
{
 const string Fixture="tests/Corpus/CSharp3/lexical-production-cases.json";
 const string Sha="07f34155802477e6749eafe62760c4e0d41b5390cd01156b7b0eda0b2a9c0554";
 const string Witness="tests/CSNitrogen.Tests/Lexing/CSharp3LexicalAlternativeTests.cs";
 const string Method="Lexical_production_alternatives_retain_context_effects_and_original_coordinates";
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static JsonObject Read(string name){using var stream=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(stream)!.AsObject();}
 static byte[] Bytes()=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"Corpus/CSharp3/lexical-production-cases.json"));
 [Fact]public void Lexical_production_evidence_links_every_exact_alternative_without_admission()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json"),Bytes()));Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 static List<string> Evaluate(JsonObject index,JsonObject map,byte[] bytes)
 {
  var errors=new List<string>();
  try
  {
   var expected=index["productions"]!.AsArray().Where(r=>Text(r!,"grammar")=="lexical").SelectMany(r=>r!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")=="production").ToDictionary(a=>Text(a!,"id"),a=>a!);
   var fixture=JsonNode.Parse(bytes)!;var cases=fixture["cases"]!.AsArray();
   if(expected.Count!=184||!expected.Keys.Order().SequenceEqual(cases.Select(c=>Text(c!,"id")).Order()))errors.Add("case set");
   if(Convert.ToHexStringLower(SHA256.HashData(bytes))!=Sha||Text(fixture,"sourceSha256")!=Text(index,"sourceSha256")||Text(fixture,"profile")!="CSharp3")errors.Add("fixture identity");
   foreach(var c0 in cases)
   {
    var c=c0!;string id=Text(c,"id");
    if(!expected.TryGetValue(id,out var a)||Text(c,"sourceAlternativeSha256")!=Text(a,"sourceAlternativeSha256")||Text(c,"sourceOccurrence")!=id[..id.LastIndexOf('/')])errors.Add("alternative identity");
    string disposition=Text(c,"disposition");
    if(!new[]{"syntax-witness","unicode-policy","source-input-normalization","diagnostic-effect","opaque-pragma"}.Contains(disposition))errors.Add("disposition");
    bool opaque=new[]{"pp-pragma","pragma-body","pragma-warning-body","warning-action","warning-list"}.Any(n=>id.Contains(":"+n+"/",StringComparison.Ordinal))||id.EndsWith(":pp-directive/6",StringComparison.Ordinal);
    if((disposition=="opaque-pragma")!=opaque)errors.Add("pragma boundary");
    if(c["contrast"]!["accepted"]!.GetValue<bool>()&&(!opaque||Text(c["contrast"]!,"source")!="#pragma warning disable 1,,2\nclass C{}"))errors.Add("accepted contrast boundary");
    var positives=c["positives"]!.AsArray();if(positives.Count==0)errors.Add("positive context");
    foreach(var p0 in positives)
    {
     var p=p0!;bool effect=id.Contains(":pp-diagnostic/1",StringComparison.Ordinal);
     if(!p["accepted"]!.GetValue<bool>()&&!effect)errors.Add("diagnostic boundary");
     if(effect&&(disposition!="diagnostic-effect"||p["accepted"]!.GetValue<bool>()||!p["issueCodes"]!.AsArray().Any(x=>x!.GetValue<string>()=="CSN1202")))errors.Add("diagnostic boundary");
     string source=Text(p,"source");
     foreach(var anchor in p["anchors"]!.AsArray())
     {int start=anchor!["start"]!.GetValue<int>();string text=Text(anchor,"text");if(text.Length==0||start<0||start>source.Length-text.Length||source.Substring(start,text.Length)!=text)errors.Add("anchor spelling/range");}
     if(!p["classNames"]!.AsArray().Select(n=>n!.GetValue<string>()).SequenceEqual(p["anchors"]!.AsArray().Select(n=>Text(n!,"text"))))errors.Add("class anchors");
    }
   }
   var evidence=map["lexicalProductionAlternativeEvidence"];
   if(evidence is null||Text(evidence,"path")!=Fixture||Text(evidence,"sha256")!=Sha||Text(evidence,"status")!="representative-witnesses"||evidence["count"]!.GetValue<int>()!=184)errors.Add("evidence metadata");
   if(evidence is null||Text(evidence["witness"]!,"path")!=Witness||Text(evidence["witness"]!,"method")!=Method)errors.Add("witness metadata");
   var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"CoverageInputs",Witness))).GetRoot();
   if(!syntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m=>m.Identifier.ValueText==Method&&m.AttributeLists.SelectMany(a=>a.Attributes).Any(a=>a.Name.ToString()=="Theory")))errors.Add("executable witness");
   var linked=new List<string>();
   foreach(var row in map["productions"]!.AsArray())
   {
    if(Text(row!,"status")!="verified"||Text(row!,"reviewStatus")!="verified")errors.Add("review status");
    foreach(var link in row!["lexicalProductionAlternativeWitnesses"]?.AsArray()??new JsonArray())
    {string id=Text(link!,"alternativeId");linked.Add(id);if(!expected.ContainsKey(id)||!id.StartsWith(Text(row,"standardId")+"/",StringComparison.Ordinal)||Text(link!,"caseId")!=id)errors.Add("case link");}
   }
   if(!expected.Keys.Order().SequenceEqual(linked.Order()))errors.Add("link set");
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
 [Theory]
 [InlineData("missing-case","case set")][InlineData("duplicate-case","case set")][InlineData("alternative-hash","alternative identity")]
 [InlineData("anchor","anchor spelling/range")][InlineData("class-anchor","class anchors")][InlineData("disposition","disposition")]
 [InlineData("pragma","pragma boundary")][InlineData("accepted-contrast","accepted contrast boundary")][InlineData("error","diagnostic boundary")]
 [InlineData("missing-link","link set")][InlineData("wrong-link","case link")][InlineData("metadata","evidence metadata")][InlineData("method","witness metadata")][InlineData("review-regression","review status")]
 public void Mutations_reject_the_specific_broken_lexical_obligation(string mutation,string error)
 {
  var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var bytes=Bytes();var fixture=JsonNode.Parse(bytes)!;var cases=fixture["cases"]!.AsArray();var first=cases[0]!;
  var row=map["productions"]!.AsArray().First(r=>r!["lexicalProductionAlternativeWitnesses"] is JsonArray a&&a.Count>0)!;
  switch(mutation)
  {
   case "missing-case":cases.RemoveAt(0);break;case "duplicate-case":cases.Add(first.DeepClone());break;
   case "alternative-hash":first["sourceAlternativeSha256"]=new string('0',64);break;
   case "anchor":cases.First(c=>c!["positives"]![0]!["anchors"]!.AsArray().Count>0)!["positives"]![0]!["anchors"]![0]!["start"]=int.MaxValue;break;
   case "class-anchor":first["positives"]![0]!["classNames"]!.AsArray().Add("Invented");break;
   case "disposition":first["disposition"]="verified";break;case "pragma":first["disposition"]="opaque-pragma";break;
   case "accepted-contrast":first["contrast"]!["accepted"]=true;break;case "error":first["positives"]![0]!["accepted"]=false;break;
   case "missing-link":row["lexicalProductionAlternativeWitnesses"]!.AsArray().RemoveAt(0);break;
   case "wrong-link":row["lexicalProductionAlternativeWitnesses"]![0]!["caseId"]="invented";break;
   case "metadata":map["lexicalProductionAlternativeEvidence"]!["sha256"]="wrong";break;
   case "method":map["lexicalProductionAlternativeEvidence"]!["witness"]!["method"]="invented";break;
   case "review-regression":row["status"]="candidate";break;
  }
  if(new[]{"missing-case","duplicate-case","alternative-hash","anchor","class-anchor","disposition","pragma","accepted-contrast","error"}.Contains(mutation))bytes=Encoding.UTF8.GetBytes(fixture.ToJsonString());
  Assert.Contains(error,Evaluate(index,map,bytes));
 }
}
