using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3ProductionEvidenceTests
{
 const string Witness="tests/CSNitrogen.Tests/Syntax/CSharp3ProductionAlternativeTests.cs";
 const string Method="Production_alternatives_have_owning_context_positive_contrast_and_original_range_observations";
 public static IEnumerable<object[]> Domains()=>new object[][]{
 ["expression",241,"ed5ec61b89a1af6db35e771742dc11c4d0ff38a803db790bfa7ef108920d00ef"],
 ["statement",100,"c69be200c5b1dd9c5d97f6c3de05a30716ca8111c6ce00cf9541f34245f8c664"],
 ["unsafe",58,"607c62d06bf9db2f832ab5599c0e1ec7f46dd153391a4c18d1e7863fb7e1427f"],
 ["declaration",365,"3d6e84dc39f5d36e5f1b0c2aeefb816a63dda56e262a0065835f4c62ed0c117d"]};
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static JsonObject Read(string name){using var stream=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(stream)!.AsObject();}
 static byte[] Bytes(string domain)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,$"Corpus/CSharp3/{domain}-production-cases.json"));
 static bool InDomain(JsonNode row,string domain)
 {int line=row["line"]!.GetValue<int>();return domain switch{"expression"=>Text(row,"grammar")=="syntactic"&&line<=8622,"statement"=>Text(row,"grammar")=="syntactic"&&line>=8624&&line<=8675,"unsafe"=>Text(row,"grammar")=="unsafe","declaration"=>Text(row,"grammar")=="syntactic"&&line>=8677,_=>false};}
 [Theory][MemberData(nameof(Domains))]
 public void Production_domain_evidence_links_the_exact_source_set_without_admission(string domain,int count,string sha)
 {Assert.Empty(Evaluate(domain,count,sha,Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json"),Bytes(domain)));Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 static List<string> Evaluate(string domain,int count,string sha,JsonObject index,JsonObject map,byte[] bytes)
 {
  var errors=new List<string>();
  try
  {
   var expected=index["productions"]!.AsArray().Where(r=>InDomain(r!,domain)).SelectMany(r=>r!["alternatives"]!.AsArray()).Where(a=>Text(a!,"kind")=="production").ToDictionary(a=>Text(a!,"id"),a=>a!);
   var fixture=JsonNode.Parse(bytes)!;var cases=fixture["cases"]!.AsArray();
   if(expected.Count!=count||!expected.Keys.Order().SequenceEqual(cases.Select(c=>Text(c!,"id")).Order()))errors.Add("case set");
   if(Convert.ToHexStringLower(SHA256.HashData(bytes))!=sha||Text(fixture,"sourceSha256")!=Text(index,"sourceSha256")||Text(fixture,"profile")!="CSharp3")errors.Add("fixture identity");
   foreach(var c0 in cases)
   {
    var c=c0!;string id=Text(c,"id");
    if(!expected.TryGetValue(id,out var a)||Text(c,"sourceAlternativeSha256")!=Text(a,"sourceAlternativeSha256")||Text(c,"sourceOccurrence")!=id[..id.LastIndexOf('/')])errors.Add("alternative identity");
    if(!new[]{"syntax-witness","binding-boundary","source-normalization"}.Contains(Text(c,"disposition")))errors.Add("disposition");
    if(Text(c,"disposition")=="source-normalization"&&!new[]{"B:8508:namespace-or-type-name/2","B:8542:member-access/3","B:8548:base-access/1","B:8578:additive-expression/3","B:8856:pointer-member-access/1","B:8870:fixed-size-buffer-declarators/2"}.Contains(id))errors.Add("normalization boundary");
    string source=Text(c,"source"),negative=Text(c,"negative");
    if(source.Length==0||negative.Length==0||source==negative)errors.Add("context pair");
    foreach(var (textKey,startKey) in new[]{("focus","focusStart"),("anchorText","anchorStart")})
    {string text=Text(c,textKey);int start=c[startKey]!.GetValue<int>();if(text.Length==0||start<0||start>source.Length-text.Length||source.Substring(start,text.Length)!=text)errors.Add("authored spelling/range");}
    if(c["acceptedNeighbor"] is not null&&(id!="B:8632:local-variable-type/2"||Text(c,"acceptedNeighbor")!="class C{object M(){var =1;}}"||Text(c,"disposition")!="binding-boundary"))errors.Add("contextual neighbor boundary");
    if(id=="B:8632:local-variable-type/2"&&Text(c,"acceptedNeighbor")!="class C{object M(){var =1;}}")errors.Add("contextual neighbor boundary");
   }
   var evidence=map["productionAlternativeEvidence"]?[domain];
   if(evidence is null||Text(evidence,"path")!=$"tests/Corpus/CSharp3/{domain}-production-cases.json"||Text(evidence,"sha256")!=sha||Text(evidence,"status")!="representative-witnesses"||evidence["count"]!.GetValue<int>()!=count)errors.Add("evidence metadata");
   if(evidence is null||Text(evidence["witness"]!,"path")!=Witness||Text(evidence["witness"]!,"method")!=Method)errors.Add("witness metadata");
   var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"CoverageInputs",Witness))).GetRoot();
   if(!syntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m=>m.Identifier.ValueText==Method&&m.AttributeLists.SelectMany(a=>a.Attributes).Any(a=>a.Name.ToString()=="Theory")))errors.Add("executable witness");
   var linked=new List<string>();
   foreach(var row in map["productions"]!.AsArray())
   {
    if(Text(row!,"status")!="verified"||Text(row!,"reviewStatus")!="verified")errors.Add("review status");
    foreach(var link in row!["productionAlternativeWitnesses"]?.AsArray()??new JsonArray())
    {if(Text(link!,"domain")!=domain)continue;string id=Text(link!,"alternativeId");linked.Add(id);if(!expected.ContainsKey(id)||!id.StartsWith(Text(row,"standardId")+"/",StringComparison.Ordinal)||Text(link!,"caseId")!=id)errors.Add("case link");}
   }
   if(!expected.Keys.Order().SequenceEqual(linked.Order()))errors.Add("link set");
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
 [Theory]
 [InlineData("missing-case","case set")][InlineData("duplicate-case","case set")][InlineData("source-hash","alternative identity")][InlineData("occurrence","alternative identity")]
 [InlineData("focus","authored spelling/range")][InlineData("anchor","authored spelling/range")][InlineData("disposition","disposition")][InlineData("normalization","normalization boundary")][InlineData("context","context pair")]
 [InlineData("neighbor","contextual neighbor boundary")][InlineData("missing-link","link set")][InlineData("wrong-case-link","case link")][InlineData("wrong-domain","link set")]
 [InlineData("metadata","evidence metadata")][InlineData("method","witness metadata")][InlineData("review-regression","review status")]
 public void Production_evidence_mutations_require_specific_invariant_failures(string mutation,string error)
 {
  foreach(var domainRow in Domains())
  {
   string domain=(string)domainRow[0];int count=(int)domainRow[1];string sha=(string)domainRow[2];
   var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var bytes=Bytes(domain);var fixture=JsonNode.Parse(bytes)!;var cases=fixture["cases"]!.AsArray();var first=cases[0]!;
   var row=map["productions"]!.AsArray().First(r=>r!["productionAlternativeWitnesses"] is JsonArray a&&a.Any(l=>Text(l!,"domain")==domain))!;
   var links=row["productionAlternativeWitnesses"]!.AsArray();var link=links.First(l=>Text(l!,"domain")==domain)!;
   switch(mutation)
   {
    case "missing-case":cases.RemoveAt(0);break;case "duplicate-case":cases.Add(first.DeepClone());break;
    case "source-hash":first["sourceAlternativeSha256"]="wrong";break;case "occurrence":first["sourceOccurrence"]="invented";break;
    case "focus":first["focusStart"]=int.MaxValue;break;case "anchor":first["anchorStart"]=int.MaxValue;break;
    case "disposition":first["disposition"]="verified";break;case "normalization":first["disposition"]="source-normalization";break;
    case "context":first["negative"]=Text(first,"source");break;case "neighbor":first["acceptedNeighbor"]="invented";break;
    case "missing-link":links.Remove(link);break;case "wrong-case-link":link["caseId"]="invented";break;case "wrong-domain":link["domain"]="invented";break;
    case "metadata":map["productionAlternativeEvidence"]![domain]!["sha256"]="wrong";break;
    case "method":map["productionAlternativeEvidence"]![domain]!["witness"]!["method"]="invented";break;case "review-regression":row["status"]="candidate";break;
   }
   if(new[]{"missing-case","duplicate-case","source-hash","occurrence","focus","anchor","disposition","normalization","context","neighbor"}.Contains(mutation))bytes=Encoding.UTF8.GetBytes(fixture.ToJsonString());
   Assert.Contains(error,Evaluate(domain,count,sha,index,map,bytes));
  }
 }
}
