using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4AlternativeMatrixTests
{
 const string Source="cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7";
 static byte[] Bytes(string p)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,p));
 static JsonObject Read(string p)=>JsonNode.Parse(Bytes(p))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static readonly string[] Domains=["lexical","expression","statement","unsafe","declaration","finite"];
 static string Corpus(string d)=>d=="finite"?"Corpus/CSharp3/finite-alternative-cases.json":$"Corpus/CSharp3/{d}-production-cases.json";
 [Fact]public void Every_fourth_nonplaceholder_alternative_has_exact_source_and_fixture_provenance()
 {Assert.Empty(Evaluate(Read("Corpus/CSharp4/alternative-witness-matrix.json")));Assert.False(ProfileResolver.Resolve("CSharp4").Profile!.Complete);}
 [Theory]
 [InlineData("missing","CaseSet")][InlineData("duplicate","CaseSet")][InlineData("invented","CaseSet")]
 [InlineData("source","SourceIdentity")][InlineData("alternative","AlternativeHash")][InlineData("parent","OccurrenceLink")]
 [InlineData("witness","WitnessLink")][InlineData("domain","FixtureDomain")][InlineData("kind","SourceKind")]
 [InlineData("count","Counts")][InlineData("promoted","Status")][InlineData("corpus-path","CorpusIdentity")]
 [InlineData("corpus-hash","CorpusIdentity")][InlineData("missing-specific","SpecificCaseSet")]
 [InlineData("duplicate-specific","SpecificCaseSet")][InlineData("changed-specific","SpecificChecksum")]
 public void Matrix_mutations_fail_on_their_source_or_fixture_invariant(string change,string expected)
 {
  var matrix=Read("Corpus/CSharp4/alternative-witness-matrix.json");var cases=matrix["cases"]!.AsArray();var first=cases[0]!;byte[]? specific=null;
  switch(change)
  {
   case "missing":cases.RemoveAt(0);break;
   case "duplicate":cases.Add(first.DeepClone());break;
   case "invented":first["id"]="invented";break;
   case "source":matrix["sourceSha256"]="changed";break;
   case "alternative":first["sourceAlternativeSha256"]="changed";break;
   case "parent":first["sourceOccurrence"]="invented";break;
   case "witness":first["witnessId"]="B:8395:input-section/1";break;
   case "domain":first["domain"]="invented";break;
   case "kind":first["sourceKind"]="invented";break;
   case "count":matrix["counts"]!["production"]=0;break;
   case "promoted":matrix["status"]="verified";break;
   case "corpus-path":matrix["predecessorCorpora"]!["lexical"]!["path"]="../invented";break;
   case "corpus-hash":matrix["predecessorCorpora"]!["lexical"]!["sha256"]="changed";break;
   default:
    var payload=Read("Corpus/CSharp4/source-specific-cases.json");var rows=payload["cases"]!.AsArray();
    if(change=="missing-specific")rows.RemoveAt(0);
    else if(change=="duplicate-specific")rows.Add(rows[0]!.DeepClone());
    else rows[0]!["source"]="class Changed{}";
    specific=Encoding.UTF8.GetBytes(payload.ToJsonString());break;
  }
  Assert.Contains(Evaluate(matrix,specific),e=>e.StartsWith(expected,StringComparison.Ordinal));Assert.False(ProfileResolver.Resolve("CSharp4").Profile!.Complete);
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject matrix,byte[]? specificBytes=null)
 {
  var errors=new List<string>();
  try
  {
   byte[] indexBytes=Bytes("CSharp4/csharp4-standard-index.json"),deltaBytes=Bytes("CSharp4/csharp4-source-delta-inventory.json");
   if(Hash(indexBytes)!="590306bf7c6ee67156e28c5b16dc3121fb6172e56f7467e8c1f082d7f8ba63e7"||Hash(deltaBytes)!="c360f026562177303c30774cc165ee77caa123bee784e3bb24acf1fb2ceebacb")errors.Add("SourceIdentity: source inventory pin");
   var index=JsonNode.Parse(indexBytes)!;var delta=JsonNode.Parse(deltaBytes)!;
   if(Text(matrix,"profile")!="CSharp4"||matrix["schemaVersion"]!.GetValue<int>()!=1||Text(matrix,"sourceSha256")!=Source||Text(matrix,"indexSha256")!=Hash(indexBytes)||Text(matrix,"deltaSha256")!=Hash(deltaBytes))errors.Add("SourceIdentity: matrix source/index/comparison");
   if(Text(matrix,"status")!="source-witnesses-unreviewed")errors.Add("Status: this is not source admission");
   var alternatives=index["productions"]!.AsArray().SelectMany(r=>r!["alternatives"]!.AsArray().Where(a=>Text(a!,"kind")!="inherited-placeholder").Select(a=>(Row:r!,Alt:a!))).ToDictionary(x=>Text(x.Alt,"id"));
   var candidates=delta["productions"]!.AsArray().SelectMany(r=>r!["alternativeCandidates"]!.AsArray()).ToDictionary(x=>Text(x!,"alternativeId"),x=>x!["predecessorAlternativeCandidates"]!.AsArray().Select(n=>n!.GetValue<string>()).ToArray());
   var cases=matrix["cases"]!.AsArray();var ids=cases.Select(c=>Text(c!,"id")).ToArray();
   if(ids.Length!=1164||ids.Distinct(StringComparer.Ordinal).Count()!=ids.Length||!ids.Order().SequenceEqual(alternatives.Keys.Order()))errors.Add("CaseSet: missing, duplicate or invented alternative");
   var old=new Dictionary<string,Dictionary<string,JsonNode>>();
   var corpora=matrix["predecessorCorpora"]!.AsObject();
   if(!corpora.Select(p=>p.Key).Order().SequenceEqual(Domains.Order()))errors.Add("CorpusIdentity: domain set");
   foreach(string domain in Domains)
   {
    string path=Corpus(domain);var bytes=Bytes(path);var meta=corpora[domain]!;
    if(Text(meta,"path")!="tests/"+path||Text(meta,"sha256")!=Hash(bytes))errors.Add("CorpusIdentity: "+domain);
    var payload=JsonNode.Parse(bytes)!;
    if(Text(payload,"sourceSha256")!="ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1")errors.Add("CorpusIdentity: predecessor source");
    old[domain]=payload["cases"]!.AsArray().ToDictionary(c=>Text(c!,"id"),c=>c!);
   }
   specificBytes??=Bytes("Corpus/CSharp4/source-specific-cases.json");var specific=JsonNode.Parse(specificBytes)!;var novelRows=specific["cases"]!.AsArray();
   if(Text(specific,"sourceSha256")!=Source||Text(specific,"profile")!="CSharp4")errors.Add("SourceIdentity: source-specific corpus");
   var novelIds=novelRows.Select(c=>Text(c!,"id")).ToArray();var expectedNovel=alternatives.Keys.Where(id=>candidates[id].Length==0).Order().ToArray();
   if(novelIds.Length!=22||novelIds.Distinct().Count()!=novelIds.Length||!novelIds.Order().SequenceEqual(expectedNovel))errors.Add("SpecificCaseSet: source-specific alternative set");
   var novel=novelRows.GroupBy(c=>Text(c!,"id")).ToDictionary(g=>g.Key,g=>g.First()!);
   if(Text(matrix["sourceSpecific"]!,"path")!="tests/Corpus/CSharp4/source-specific-cases.json"||Text(matrix["sourceSpecific"]!,"sha256")!=Hash(specificBytes))errors.Add("SpecificChecksum: source-specific payload");
   foreach(var entry in cases)
   {
    string id=Text(entry!,"id");if(!alternatives.TryGetValue(id,out var source))continue;
    if(Text(entry!,"sourceOccurrence")!=Text(source.Row,"id"))errors.Add("OccurrenceLink: "+id);
    string sha=Text(source.Alt,"sourceAlternativeSha256");if(Text(entry!,"sourceAlternativeSha256")!=sha)errors.Add("AlternativeHash: "+id);
    string kind=Text(entry!,"sourceKind"),domain=Text(entry!,"domain"),witness=Text(entry!,"witnessId");JsonNode? fixture=null;
    if(kind=="predecessor-corpus")
    {
     if(!old.TryGetValue(domain,out var corpus)){errors.Add("FixtureDomain: "+id);continue;}
     if(candidates[id].Length!=1||candidates[id][0]!=witness||!corpus.TryGetValue(witness,out fixture))errors.Add("WitnessLink: "+id);
    }
    else if(kind=="source-specific")
    {
     if(domain!="source-specific")errors.Add("FixtureDomain: "+id);
     if(candidates[id].Length!=0||witness!=id||!novel.TryGetValue(id,out fixture))errors.Add("WitnessLink: "+id);
    }
    else errors.Add("SourceKind: "+id);
    if(fixture is not null&&Text(fixture,"sourceAlternativeSha256")!=sha)errors.Add("AlternativeHash: witness "+id);
   }
   var counts=matrix["counts"]!;
   if(counts["production"]!.GetValue<int>()!=955||counts["finite"]!.GetValue<int>()!=209||counts["predecessor"]!.GetValue<int>()!=1142||counts["sourceSpecific"]!.GetValue<int>()!=22||cases.Count(c=>Text(c!,"sourceKind")=="predecessor-corpus")!=1142||cases.Count(c=>Text(c!,"sourceKind")=="source-specific")!=22)errors.Add("Counts: expected955+209 /1142+22");
  }
  catch(Exception e)when(e is InvalidOperationException or KeyNotFoundException or ArgumentException or NullReferenceException){errors.Add("Schema: "+e.Message);}
  return errors;
 }
}
