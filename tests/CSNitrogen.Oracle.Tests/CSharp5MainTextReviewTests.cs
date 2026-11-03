using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Task3 source inspection inputs for CSharp5; none of them awards admission.</summary>
public class CSharp5MainTextReviewTests
{
 static byte[] Bytes(string name)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp5",name));
 static JsonObject Read(string name)=>JsonNode.Parse(Bytes(name))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 const string Fourth="cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7";
 const string Fifth="6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519";
 static readonly Dictionary<string,int> Counts=new(StringComparer.Ordinal)
 {
  ["async-functions"]=6,["await-expressions"]=5,["generic-member-transcription"]=2,["preprocessing-trailing-comment"]=1,
  ["modifier-compatibility"]=1,["caller-info-attributes"]=3,["field-and-layout-normalization"]=589,["retained-semantic-or-editorial-boundary"]=100
 };
 [Fact]public void Every_main_text_difference_has_a_pinned_disposition_and_the_fifth_profile_stays_partial()
 {Assert.Empty(Evaluate(Read("csharp5-main-text-dispositions.json")));Assert.False(ProfileResolver.Resolve("CSharp5").Profile!.Complete);}
 [Fact]public void The_appendix_index_matches_the_admitted_fourth_except_three_reviewed_bodies()
 {
  var index=Read("csharp5-standard-index.json");var delta=Read("csharp5-source-delta-inventory.json");
  Assert.Equal("indexed-unreviewed",Text(index,"status"));Assert.Equal(Fifth,Text(index,"sourceSha256"));Assert.Equal(458,index["productions"]!.AsArray().Count);
  Assert.Equal("candidate-unreviewed",Text(delta,"status"));Assert.Equal(Hash(Bytes("csharp5-standard-index.json")),Text(delta,"indexSha256"));
  Assert.Equal(Fourth,Text(delta["predecessor"]!,"sourceSha256"));
  Assert.Equal(["B:10112:pointer-member-access","B:9652:new-line","B:9658:new-line-character"],
   delta["productions"]!.AsArray().Where(r=>Text(r!,"bodyRelation")!="body-identical").Select(r=>Text(r!,"standardId")).Order());
  Assert.Equal(0,delta["counts"]!["removedPredecessorOccurrences"]!.GetValue<int>());
  // The appendix summary omits async/await; they are main-text additions only.
  Assert.DoesNotContain(index["productions"]!.AsArray(),r=>Text(r!,"name") is "await-expression" or "async-modifier");
 }
 [Theory]
 [InlineData("missing","RegionSet")][InlineData("duplicate","RegionSet")][InlineData("invented","RegionSet")]
 [InlineData("source","SourceIdentity")][InlineData("index","SourceIdentity")]
 [InlineData("fourth-hash","RegionHash")][InlineData("fifth-hash","RegionHash")]
 [InlineData("unknown-disposition","Disposition")][InlineData("changed-disposition","Disposition")]
 [InlineData("missing-definition","Definition")][InlineData("empty-definition","Definition")]
 [InlineData("count","Counts")][InlineData("report-path","Report")][InlineData("report-hash","Report")]
 [InlineData("report-bytes","Report")][InlineData("promoted","Status")]
 public void Review_metadata_mutations_reject_the_corresponding_invariant(string change,string expected)
 {
  var map=Read("csharp5-main-text-dispositions.json");var rows=map["regions"]!.AsArray();var first=rows[0]!;byte[]? report=null;
  switch(change)
  {
   case "missing":rows.RemoveAt(0);break;
   case "duplicate":rows.Add(first.DeepClone());break;
   case "invented":first["id"]="invented";break;
   case "source":map["sourceSha256"]="changed";break;
   case "index":map["differenceIndexSha256"]="changed";break;
   case "fourth-hash":first["fourthNormalizedSha256"]="changed";break;
   case "fifth-hash":first["fifthNormalizedSha256"]="changed";break;
   case "unknown-disposition":first["disposition"]="invented";break;
   case "changed-disposition":rows.Single(r=>Text(r!,"id")=="D:498")!["disposition"]="retained-semantic-or-editorial-boundary";break;
   case "missing-definition":map["dispositions"]!.AsObject().Remove("async-functions");break;
   case "empty-definition":map["dispositions"]!["async-functions"]="";break;
   case "count":map["counts"]!["async-functions"]=0;break;
   case "report-path":map["report"]="../invented.md";break;
   case "report-hash":map["reportSha256"]="changed";break;
   case "report-bytes":report=[];break;
   case "promoted":map["status"]="admitted";break;
  }
  Assert.Contains(Evaluate(map,report),e=>e.StartsWith(expected,StringComparison.Ordinal));
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject map,byte[]? reportBytes=null)
 {
  var errors=new List<string>();
  try
  {
   byte[] bytes=Bytes("csharp5-main-text-differences.json");var index=JsonNode.Parse(bytes)!;
   const string pin="00eb3d88a8310cdb69ac524bcb1b5ccfb5eeeb76eb045fdf2702a5e0ae6e3c7e";
   if(Hash(bytes)!=pin||Text(map,"differenceIndexSha256")!=pin||Text(map,"sourceSha256")!=Fifth||Text(index,"fourthSourceSha256")!=Fourth||Text(map,"profile")!="CSharp5"||map["schemaVersion"]!.GetValue<int>()!=1)errors.Add("SourceIdentity");
   if(Text(index,"status")!="difference-index-unreviewed"||Text(map,"status")!="source-inspected")errors.Add("Status: inspection is not admission");
   var expected=index["regions"]!.AsArray().ToDictionary(r=>Text(r!,"id"),r=>r!);var rows=map["regions"]!.AsArray();var ids=rows.Select(r=>Text(r!,"id")).ToArray();
   if(expected.Count!=707||ids.Length!=707||ids.Distinct().Count()!=707||!ids.Order().SequenceEqual(expected.Keys.Order()))errors.Add("RegionSet");
   var definitions=map["dispositions"]!.AsObject();
   if(!definitions.Select(p=>p.Key).Order().SequenceEqual(Counts.Keys.Order())||definitions.Any(p=>string.IsNullOrWhiteSpace(p.Value?.GetValue<string>())))errors.Add("Definition");
   foreach(var row in rows)
   {
    string id=Text(row!,"id");if(!expected.TryGetValue(id,out var source))continue;
    if(Text(row!,"fourthNormalizedSha256")!=Text(source["fourth"]!,"normalizedSha256")||Text(row!,"fifthNormalizedSha256")!=Text(source["fifth"]!,"normalizedSha256"))errors.Add("RegionHash: "+id);
    if(!Counts.ContainsKey(Text(row!,"disposition")))errors.Add("Disposition: "+id);
   }
   var counts=map["counts"]!.AsObject();
   if(!counts.Select(p=>p.Key).Order().SequenceEqual(Counts.Keys.Order()))errors.Add("Counts: category set");
   foreach(var (category,count) in Counts)
    if(counts[category]?.GetValue<int>()!=count||rows.Count(r=>Text(r!,"disposition")==category)!=count)errors.Add("Counts: "+category);
   // Parser-affecting discoveries must not be silently relabelled as excluded semantics.
   Require("D:498","async-functions");Require("D:493","async-functions");Require("D:307","await-expressions");Require("D:319","await-expressions");
   Require("D:387","await-expressions");Require("D:032","preprocessing-trailing-comment");Require("D:529","modifier-compatibility");Require("D:667","generic-member-transcription");
   void Require(string id,string disposition){if(rows.SingleOrDefault(r=>Text(r!,"id")==id) is not {} row||Text(row,"disposition")!=disposition)errors.Add("Disposition: "+id);}
   if(Text(map,"report")!="docs/coverage/csharp5-normative-context-review.md"||Text(map,"reportSha256")!=Hash(reportBytes??Bytes("csharp5-normative-context-review.md")))errors.Add("Report");
  }
  catch(Exception e){errors.Add("Malformed: "+e.GetType().Name+": "+e.Message);}
  return errors;
 }
}
