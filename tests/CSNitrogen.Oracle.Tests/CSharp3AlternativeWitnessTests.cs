using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3AlternativeWitnessTests
{
 private static JsonObject Read(string name)
 {using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(s)!.AsObject();}
 [Fact]public void Every_source_occurrence_has_independent_alternative_identities()
 {
  var index=Read("csharp3-standard-index.json");var rows=index["productions"]!.AsArray();Assert.Equal(455,rows.Count);
  var alternatives=rows.SelectMany(r=>r!["alternatives"]!.AsArray()).ToArray();Assert.Equal(1175,alternatives.Length);
  Assert.Equal(1175,alternatives.Select(a=>a!["id"]!.GetValue<string>()).Distinct().Count());
  Assert.Equal(18,alternatives.Count(a=>a!["kind"]!.GetValue<string>()=="inherited-placeholder"));
  foreach(var (kind,count) in new[]{("lexical",370),("syntactic",729),("unsafe",76)}){Assert.Equal(count,index["alternativeCounts"]![kind]!.GetValue<int>());Assert.Equal(count,rows.Where(r=>r!["grammar"]!.GetValue<string>()==kind).Sum(r=>r!["alternatives"]!.AsArray().Count));}
  foreach(var row in rows)
  {
   string id=row!["id"]!.GetValue<string>();int ordinal=0;
   foreach(var a in row["alternatives"]!.AsArray())
   {Assert.Equal(++ordinal,a!["ordinal"]!.GetValue<int>());Assert.Equal(id+"/"+ordinal,a["id"]!.GetValue<string>());Assert.Matches("^[a-f0-9]{64}$",a["sourceAlternativeSha256"]!.GetValue<string>());}
  }
  Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 private const string AlternativeIdentity="7d4761a0a218673210b624e57baf88f738b1319f53bd67cf2eeddc421ae3c6f2";
 private const string FixtureSha="1f57a55da4a3e8e6deb9d879137c5f169eb180a23b9bb65930d7fda44615425c";
 private const string FixturePath="tests/Corpus/CSharp3/finite-alternative-cases.json";
 private const string WitnessPath="tests/CSNitrogen.Tests/Syntax/CSharp3FiniteAlternativeTests.cs";
 private const string WitnessMethod="Every_finite_one_of_alternative_has_positive_negative_and_original_range_witnesses";
 private static byte[] CasesBytes()=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"Corpus/CSharp3/finite-alternative-cases.json"));
 [Fact]public void Finite_alternative_evidence_matches_exact_source_tokens_and_executable_case_links()
 {Assert.Empty(Evaluate(Read("csharp3-standard-index.json"),Read("csharp3-standard-crosswalk.json"),CasesBytes()));Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);}
 [Fact]public void Long_source_continuations_and_repeated_lists_keep_independent_alternative_structure()
 {
  var rows=Read("csharp3-standard-index.json")["productions"]!.AsArray();
  foreach(string id in new[]{"B:8677:compilation-unit","B:8692:class-declaration","B:8721:method-header","B:8773:struct-declaration","B:8790:interface-declaration","B:8797:interface-method-declaration","B:8811:delegate-declaration","B:8866:fixed-size-buffer-declaration"})Assert.Single(rows.Single(r=>Text(r!,"id")==id)!["alternatives"]!.AsArray());
  var first=rows.Single(r=>Text(r!,"id")=="B:8527:dim-separators")!["alternatives"]!.AsArray();
  var second=rows.Single(r=>Text(r!,"id")=="B:8785:dim-separators")!["alternatives"]!.AsArray();
  Assert.Equal(2,first.Count);Assert.Equal(first.Select(a=>Text(a!,"sourceAlternativeSha256")),second.Select(a=>Text(a!,"sourceAlternativeSha256")));
  Assert.False(first.Select(a=>Text(a!,"id")).Intersect(second.Select(a=>Text(a!,"id"))).Any());
 }
 [Theory]
 [InlineData("missing-alternative","source alternative identity")][InlineData("duplicate-alternative","source alternative identity")][InlineData("changed-alternative","source alternative identity")][InlineData("changed-kind","source alternative identity")][InlineData("changed-ordinal","alternative ordinal")]
 [InlineData("missing-finite-case","finite case set")][InlineData("duplicate-finite-case","finite case set")][InlineData("changed-token","token alternative")][InlineData("bad-range","fixture spelling/range")]
 [InlineData("missing-case-link","finite link set")][InlineData("wrong-case-link","case link")][InlineData("wrong-fixture-hash","evidence metadata")][InlineData("invented-witness","witness metadata")][InlineData("premature-admission","premature admission")]
 public void Alternative_and_fixture_mutations_fail_closed(string change,string expectedError)
 {
  var index=Read("csharp3-standard-index.json");var map=Read("csharp3-standard-crosswalk.json");var bytes=CasesBytes();var fixture=JsonNode.Parse(bytes)!;
  var alts=index["productions"]![0]!["alternatives"]!.AsArray();var rows=map["productions"]!.AsArray();var keyword=rows.Single(r=>Text(r!,"standardId")=="B:8431:keyword")!;
  switch(change)
  {
   case "missing-alternative":alts.RemoveAt(0);break;
   case "duplicate-alternative":alts.Add(alts[0]!.DeepClone());break;
   case "changed-alternative":alts[0]!["sourceAlternativeSha256"]=new string('0',64);break;
   case "changed-kind":alts[0]!["kind"]="one-of-token";break;
   case "changed-ordinal":alts[0]!["ordinal"]=999;break;
   case "missing-finite-case":fixture["cases"]!.AsArray().RemoveAt(0);break;
   case "duplicate-finite-case":fixture["cases"]!.AsArray().Add(fixture["cases"]![0]!.DeepClone());break;
   case "changed-token":fixture["cases"]![0]!["token"]="@abstract";break;
   case "bad-range":fixture["cases"]![0]!["spanStart"]=int.MaxValue;break;
   case "missing-case-link":keyword["finiteAlternativeWitnesses"]!.AsArray().RemoveAt(0);break;
   case "wrong-case-link":keyword["finiteAlternativeWitnesses"]![0]!["caseId"]="invented";break;
   case "wrong-fixture-hash":map["finiteAlternativeEvidence"]!["sha256"]=new string('0',64);break;
   case "invented-witness":map["finiteAlternativeEvidence"]!["witness"]!["method"]="Invented";break;
   case "premature-admission":rows[0]!["status"]="verified";break;
  }
  if(change is "missing-finite-case" or "duplicate-finite-case" or "changed-token" or "bad-range")bytes=Encoding.UTF8.GetBytes(fixture.ToJsonString());
  Assert.Contains(expectedError,Evaluate(index,map,bytes));Assert.False(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 private static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 private static string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
 internal static IReadOnlyList<string> Evaluate(JsonObject index,JsonObject map,byte[] bytes)
 {
  var errors=new List<string>();
  try
  {
   var rows=index["productions"]!.AsArray();var all=rows.SelectMany(r=>r!["alternatives"]!.AsArray()).Select(a=>a!).ToArray();
   string identity=string.Concat(all.Select(a=>Text(a,"id")+"|"+Text(a,"kind")+"|"+Text(a,"sourceAlternativeSha256")+"\n"));
   if(all.Length!=1175||Hash(Encoding.UTF8.GetBytes(identity))!=AlternativeIdentity)errors.Add("source alternative identity");
   foreach(var row in rows)
   {
    int ordinal=0;foreach(var a in row!["alternatives"]!.AsArray())
     if(a!["ordinal"]!.GetValue<int>()!=++ordinal||Text(a,"id")!=Text(row,"id")+"/"+ordinal)errors.Add("alternative ordinal");
   }
   var finite=rows.Where(r=>Text(r!["alternatives"]![0]!,"kind")=="one-of-token").ToArray();
   var expected=finite.SelectMany(r=>r!["alternatives"]!.AsArray()).Select(a=>Text(a!,"id")).Order().ToArray();
   if(finite.Length!=10||expected.Length!=209)errors.Add("finite source set");
   var corpus=JsonNode.Parse(bytes)!;
   if(Hash(bytes)!=FixtureSha||Text(corpus,"sourceSha256")!=Text(index,"sourceSha256")||Text(corpus,"profile")!="CSharp3")errors.Add("fixture identity");
   var cases=corpus["cases"]!.AsArray();
   if(!expected.SequenceEqual(cases.Select(c=>Text(c!,"id")).Order()))errors.Add("finite case set");
   var indexed=all.ToDictionary(a=>Text(a,"id"));
   foreach(var c in cases)
   {
    string id=Text(c!,"id"),positive=Text(c!,"positive"),negative=Text(c!,"negative"),span=Text(c!,"spanText");int start=c!["spanStart"]!.GetValue<int>();
    if(!indexed.TryGetValue(id,out var a)||Text(a,"kind")!="one-of-token"||Text(c,"sourceOccurrence")!=id[..id.LastIndexOf('/')]||Text(c,"sourceAlternativeSha256")!=Text(a,"sourceAlternativeSha256")||Hash(Encoding.UTF8.GetBytes(Text(c,"token")))!=Text(a,"sourceAlternativeSha256"))errors.Add("token alternative");
    if(positive.Length==0||negative.Length==0||positive==negative||span.Length==0||start<0||start>positive.Length-span.Length||positive.Substring(start,span.Length)!=span)errors.Add("fixture spelling/range");
   }
   var evidence=map["finiteAlternativeEvidence"]!;
   if(Text(evidence,"path")!=FixturePath||Text(evidence,"sha256")!=FixtureSha||Text(evidence,"status")!="finite-witnesses"||evidence["count"]!.GetValue<int>()!=209)errors.Add("evidence metadata");
   if(Text(evidence["witness"]!,"path")!=WitnessPath||Text(evidence["witness"]!,"method")!=WitnessMethod)errors.Add("witness metadata");
   var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"CoverageInputs",WitnessPath))).GetRoot();
   if(!syntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m=>m.Identifier.ValueText==WitnessMethod&&m.AttributeLists.SelectMany(a=>a.Attributes).Any(a=>a.Name.ToString()=="Theory")))errors.Add("executable witness method");
   var linked=new List<string>();
   foreach(var row in map["productions"]!.AsArray())
   {
    if(Text(row!,"status")!="candidate"||Text(row!,"reviewStatus")!="pending")errors.Add("premature admission");
    foreach(var link in row!["finiteAlternativeWitnesses"]?.AsArray()??new JsonArray())
    {
     string id=Text(link!,"alternativeId");linked.Add(id);
     if(Text(link!,"caseId")!=id||!id.StartsWith(Text(row,"standardId")+"/",StringComparison.Ordinal)||!expected.Contains(id))errors.Add("case link");
    }
   }
   if(!expected.SequenceEqual(linked.Order()))errors.Add("finite link set");
  }
  catch(Exception e){errors.Add(e.Message);}
  return errors;
 }
}
