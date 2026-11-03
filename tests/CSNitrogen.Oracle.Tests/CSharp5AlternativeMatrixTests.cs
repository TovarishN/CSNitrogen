using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Version5 alternatives link through the admitted CSharp4 matrix; provenance is not admission.</summary>
public class CSharp5AlternativeMatrixTests
{
 const string Source="6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519";
 static byte[] Bytes(string p)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,p));
 static JsonObject Read(string p)=>JsonNode.Parse(Bytes(p))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static readonly string[] Domains=["lexical","expression","statement","unsafe","declaration","finite","fourth-source-specific"];
 static string Corpus(string d)=>d switch{"finite"=>"Corpus/CSharp3/finite-alternative-cases.json","fourth-source-specific"=>"Corpus/CSharp4/source-specific-cases.json",_=>$"Corpus/CSharp3/{d}-production-cases.json"};
 [Fact]public void Every_fifth_nonplaceholder_alternative_has_exact_source_and_fixture_provenance()
 {Assert.Empty(Evaluate(Read("Corpus/CSharp5/alternative-witness-matrix.json")));Assert.True(ProfileResolver.Resolve("CSharp5").Profile!.Complete);}
 [Theory]
 [InlineData("missing","CaseSet")][InlineData("duplicate","CaseSet")][InlineData("invented","CaseSet")]
 [InlineData("source","SourceIdentity")][InlineData("alternative","AlternativeHash")][InlineData("parent","OccurrenceLink")]
 [InlineData("predecessor","PredecessorLink")][InlineData("witness","WitnessLink")][InlineData("domain","FixtureDomain")]
 [InlineData("kind","SourceKind")][InlineData("count","Counts")][InlineData("promoted","Status")]
 [InlineData("matrix-hash","PredecessorMatrix")][InlineData("corpus-hash","CorpusIdentity")]
 [InlineData("missing-specific","SpecificCaseSet")][InlineData("changed-specific","SpecificChecksum")]
 public void Matrix_mutations_fail_on_their_source_or_fixture_invariant(string change,string expected)
 {
  var matrix=Read("Corpus/CSharp5/alternative-witness-matrix.json");var cases=matrix["cases"]!.AsArray();var first=cases[0]!;byte[]? specific=null;
  switch(change)
  {
   case "missing":cases.RemoveAt(0);break;
   case "duplicate":cases.Add(first.DeepClone());break;
   case "invented":first["id"]="invented";break;
   case "source":matrix["sourceSha256"]="changed";break;
   case "alternative":first["sourceAlternativeSha256"]="changed";break;
   case "parent":first["sourceOccurrence"]="invented";break;
   case "predecessor":first["predecessorAlternative"]="B:8395:input-section/1";break;
   case "witness":first["witnessId"]="B:8395:input-section/1";break;
   case "domain":first["domain"]="invented";break;
   case "kind":first["sourceKind"]="invented";break;
   case "count":matrix["counts"]!["production"]=0;break;
   case "promoted":matrix["status"]="verified";break;
   case "matrix-hash":matrix["predecessorMatrix"]!["sha256"]="changed";break;
   case "corpus-hash":matrix["predecessorCorpora"]!["lexical"]!["sha256"]="changed";break;
   default:
    var payload=Read("Corpus/CSharp5/source-specific-cases.json");var rows=payload["cases"]!.AsArray();
    if(change=="missing-specific")rows.RemoveAt(0);else rows[0]!["source"]="class Changed{}";
    specific=Encoding.UTF8.GetBytes(payload.ToJsonString());break;
  }
  Assert.Contains(Evaluate(matrix,specific),e=>e.StartsWith(expected,StringComparison.Ordinal));
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject matrix,byte[]? specificBytes=null)
 {
  var errors=new List<string>();
  try
  {
   byte[] indexBytes=Bytes("CSharp5/csharp5-standard-index.json"),deltaBytes=Bytes("CSharp5/csharp5-source-delta-inventory.json");
   if(Hash(indexBytes)!="41a72d8a84b187708fcd402de8bb61a4cc79c8e77c922924c602b3739882f992"||Hash(deltaBytes)!="5fc564bb9a526e29c11fe61663b117b7d8a95b5bd52dbd77e728355704f6c875")errors.Add("SourceIdentity: source inventory pin");
   var index=JsonNode.Parse(indexBytes)!;var delta=JsonNode.Parse(deltaBytes)!;
   if(Text(matrix,"profile")!="CSharp5"||matrix["schemaVersion"]!.GetValue<int>()!=1||Text(matrix,"sourceSha256")!=Source||Text(matrix,"indexSha256")!=Hash(indexBytes)||Text(matrix,"deltaSha256")!=Hash(deltaBytes))errors.Add("SourceIdentity: matrix source/index/comparison");
   if(Text(matrix,"status")!="source-witnesses-unreviewed")errors.Add("Status: this is not source admission");
   const string fourthPath="Corpus/CSharp4/alternative-witness-matrix.json";var fourthBytes=Bytes(fourthPath);
   if(Text(matrix["predecessorMatrix"]!,"path")!="tests/"+fourthPath||Text(matrix["predecessorMatrix"]!,"sha256")!=Hash(fourthBytes))errors.Add("PredecessorMatrix: identity");
   var fourth=JsonNode.Parse(fourthBytes)!["cases"]!.AsArray().ToDictionary(c=>Text(c!,"id"),c=>c!);
   var alternatives=index["productions"]!.AsArray().SelectMany(r=>r!["alternatives"]!.AsArray().Where(a=>Text(a!,"kind")!="inherited-placeholder").Select(a=>(Row:r!,Alt:a!))).ToDictionary(x=>Text(x.Alt,"id"));
   var candidates=delta["productions"]!.AsArray().SelectMany(r=>r!["alternativeCandidates"]!.AsArray()).ToDictionary(x=>Text(x!,"alternativeId"),x=>x!["predecessorAlternativeCandidates"]!.AsArray().Select(n=>n!.GetValue<string>()).ToArray());
   var cases=matrix["cases"]!.AsArray();var ids=cases.Select(c=>Text(c!,"id")).ToArray();
   if(ids.Length!=1164||ids.Distinct(StringComparer.Ordinal).Count()!=ids.Length||!ids.Order().SequenceEqual(alternatives.Keys.Order()))errors.Add("CaseSet: missing, duplicate or invented alternative");
   var corpora=matrix["predecessorCorpora"]!.AsObject();var witnesses=new Dictionary<string,HashSet<string>>();
   if(!corpora.Select(p=>p.Key).Order().SequenceEqual(Domains.Order()))errors.Add("CorpusIdentity: domain set");
   foreach(string domain in Domains)
   {
    var bytes=Bytes(Corpus(domain));var meta=corpora[domain];
    if(meta is null||Text(meta,"path")!="tests/"+Corpus(domain)||Text(meta,"sha256")!=Hash(bytes))errors.Add("CorpusIdentity: "+domain);
    witnesses[domain]=JsonNode.Parse(bytes)!["cases"]!.AsArray().Select(c=>Text(c!,"id")).ToHashSet();
   }
   specificBytes??=Bytes("Corpus/CSharp5/source-specific-cases.json");var specific=JsonNode.Parse(specificBytes)!;var novelRows=specific["cases"]!.AsArray();
   if(Text(specific,"sourceSha256")!=Source||Text(specific,"profile")!="CSharp5")errors.Add("SourceIdentity: source-specific corpus");
   if(Text(matrix["sourceSpecific"]!,"path")!="tests/Corpus/CSharp5/source-specific-cases.json"||Text(matrix["sourceSpecific"]!,"sha256")!=Hash(specificBytes))errors.Add("SpecificChecksum: source-specific corpus bytes");
   var novelIds=novelRows.Select(c=>Text(c!,"id")).ToArray();var expectedNovel=alternatives.Keys.Where(id=>candidates[id].Length==0).Order().ToArray();
   if(novelIds.Length!=3||novelIds.Distinct().Count()!=novelIds.Length||!novelIds.Order().SequenceEqual(expectedNovel))errors.Add("SpecificCaseSet: source-specific alternative set");
   var novel=novelRows.GroupBy(c=>Text(c!,"id")).ToDictionary(g=>g.Key,g=>g.First()!);
   foreach(var c in cases)
   {
    string id=Text(c!,"id");if(!alternatives.TryGetValue(id,out var source))continue;
    if(Text(c!,"sourceAlternativeSha256")!=Text(source.Alt,"sourceAlternativeSha256"))errors.Add("AlternativeHash: "+id);
    if(Text(c!,"sourceOccurrence")!=Text(source.Row,"id"))errors.Add("OccurrenceLink: "+id);
    string kind=Text(c!,"sourceKind"),domain=Text(c!,"domain"),witness=Text(c!,"witnessId");
    if(kind=="predecessor-matrix")
    {
     string predecessor=Text(c!,"predecessorAlternative");
     if(candidates[id].Length!=1||candidates[id][0]!=predecessor||!fourth.TryGetValue(predecessor,out var previous)){errors.Add("PredecessorLink: "+id);continue;}
     string expectedDomain=Text(previous,"domain")=="source-specific"?"fourth-source-specific":Text(previous,"domain");
     if(domain!=expectedDomain)errors.Add("FixtureDomain: "+id);
     if(witness!=Text(previous,"witnessId")||!witnesses.TryGetValue(domain,out var known)||!known.Contains(witness))errors.Add("WitnessLink: "+id);
    }
    else if(kind=="source-specific")
    {
     if(domain!="source-specific")errors.Add("FixtureDomain: "+id);
     if(witness!=id||!novel.TryGetValue(id,out var row)||Text(row,"sourceAlternativeSha256")!=Text(source.Alt,"sourceAlternativeSha256"))errors.Add("WitnessLink: "+id);
    }
    else errors.Add("SourceKind: "+id);
   }
   var counts=matrix["counts"]!;
   if(counts["production"]!.GetValue<int>()!=955||counts["finite"]!.GetValue<int>()!=209||counts["predecessor"]!.GetValue<int>()!=1161||counts["sourceSpecific"]!.GetValue<int>()!=3)errors.Add("Counts");
  }
  catch(Exception e){errors.Add("Malformed: "+e.GetType().Name+": "+e.Message);}
  return errors;
 }
}
