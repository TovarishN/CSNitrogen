using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4MainTextReviewTests
{
 static byte[] Bytes(string name)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp4",name));
 static JsonObject Read(string name)=>JsonNode.Parse(Bytes(name))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 static readonly Dictionary<string,int> Counts=new(StringComparer.Ordinal)
 {
  ["optional-parameters"]=5,["named-arguments"]=21,["variance"]=20,["generic-followers"]=1,["base-projections"]=2,
  ["transcription-normalization"]=6,["unicode-continuity"]=1,["contextual-name-binding"]=3,["documentation-trivia"]=8,["retained-semantic-or-editorial-boundary"]=201
 };
 [Fact]public void Every_main_text_difference_has_a_pinned_disposition_and_separate_snapshot_admission()
 {Assert.Empty(Evaluate(Read("csharp4-main-text-dispositions.json")));Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);}
 [Theory]
 [InlineData("missing","RegionSet")][InlineData("duplicate","RegionSet")][InlineData("invented","RegionSet")]
 [InlineData("source","SourceIdentity")][InlineData("index","SourceIdentity")]
 [InlineData("third-hash","RegionHash")][InlineData("fourth-hash","RegionHash")]
 [InlineData("unknown-disposition","Disposition")][InlineData("changed-disposition","Disposition")]
 [InlineData("missing-definition","Definition")][InlineData("empty-definition","Definition")]
 [InlineData("count","Counts")][InlineData("report-path","Report")][InlineData("report-hash","Report")]
 [InlineData("report-bytes","Report")][InlineData("promoted","Status")]
 public void Review_metadata_mutations_reject_the_corresponding_invariant(string change,string expected)
 {
  var map=Read("csharp4-main-text-dispositions.json");var rows=map["regions"]!.AsArray();var first=rows[0]!;byte[]? report=null;
  switch(change)
  {
   case "missing":rows.RemoveAt(0);break;
   case "duplicate":rows.Add(first.DeepClone());break;
   case "invented":first["id"]="invented";break;
   case "source":map["sourceSha256"]="changed";break;
   case "index":map["differenceIndexSha256"]="changed";break;
   case "third-hash":first["thirdNormalizedSha256"]="changed";break;
   case "fourth-hash":first["fourthNormalizedSha256"]="changed";break;
   case "unknown-disposition":first["disposition"]="invented";break;
   case "changed-disposition":rows.Single(r=>Text(r!,"id")=="D:109")!["disposition"]="retained-semantic-or-editorial-boundary";break;
   case "missing-definition":map["dispositions"]!.AsObject().Remove("generic-followers");break;
   case "empty-definition":map["dispositions"]!["generic-followers"]="";break;
   case "count":map["counts"]!["generic-followers"]=0;break;
   case "report-path":map["report"]="../invented.md";break;
   case "report-hash":map["reportSha256"]="changed";break;
   case "report-bytes":report=[];break;
   case "promoted":map["status"]="admitted";break;
  }
  Assert.Contains(Evaluate(map,report),e=>e.StartsWith(expected,StringComparison.Ordinal));Assert.True(ProfileResolver.Resolve("CSharp4").Profile!.Complete);
 }
 internal static IReadOnlyList<string> Evaluate(JsonObject map,byte[]? reportBytes=null)
 {
  var errors=new List<string>();
  try
  {
   byte[] bytes=Bytes("csharp4-main-text-differences.json");var index=JsonNode.Parse(bytes)!;
   const string pin="280d6c4e091ea9dad816fc1efd871e7b54ea0389e704ca5c396ccda8eec47768";
   if(Hash(bytes)!=pin||Text(map,"differenceIndexSha256")!=pin||Text(map,"sourceSha256")!="cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7"||Text(map,"profile")!="CSharp4"||map["schemaVersion"]!.GetValue<int>()!=1)errors.Add("SourceIdentity");
   if(Text(index,"status")!="difference-index-unreviewed"||Text(map,"status")!="source-inspected")errors.Add("Status: inspection is not admission");
   var expected=index["regions"]!.AsArray().ToDictionary(r=>Text(r!,"id"),r=>r!);var rows=map["regions"]!.AsArray();var ids=rows.Select(r=>Text(r!,"id")).ToArray();
   if(expected.Count!=268||ids.Length!=268||ids.Distinct().Count()!=268||!ids.Order().SequenceEqual(expected.Keys.Order()))errors.Add("RegionSet");
   var definitions=map["dispositions"]!.AsObject();
   if(!definitions.Select(p=>p.Key).Order().SequenceEqual(Counts.Keys.Order())||definitions.Any(p=>string.IsNullOrWhiteSpace(p.Value?.GetValue<string>())))errors.Add("Definition");
   foreach(var row in rows)
   {
    string id=Text(row!,"id");if(!expected.TryGetValue(id,out var source))continue;
    if(Text(row!,"thirdNormalizedSha256")!=Text(source["third"]!,"normalizedSha256")||Text(row!,"fourthNormalizedSha256")!=Text(source["fourth"]!,"normalizedSha256"))errors.Add("RegionHash: "+id);
    if(!Counts.ContainsKey(Text(row!,"disposition")))errors.Add("Disposition: "+id);
   }
   var counts=map["counts"]!.AsObject();
   if(!counts.Select(p=>p.Key).Order().SequenceEqual(Counts.Keys.Order()))errors.Add("Counts: category set");
   foreach(var (category,count) in Counts)
    if(counts[category]?.GetValue<int>()!=count||rows.Count(r=>Text(r!,"disposition")==category)!=count)errors.Add("Counts: "+category);
   // Specific parser-affecting discoveries must not be silently relabelled as excluded semantics.
   Require("D:109","generic-followers");Require("D:137","base-projections");Require("D:140","base-projections");Require("D:250","named-arguments");
   void Require(string id,string disposition){if(rows.SingleOrDefault(r=>Text(r!,"id")==id) is not {} row||Text(row,"disposition")!=disposition)errors.Add("Disposition: "+id);}
   if(Text(map,"report")!="docs/coverage/csharp4-normative-context-review.md"||Text(map,"reportSha256")!=Hash(reportBytes??Bytes("csharp4-normative-context-review.md")))errors.Add("Report");
  }
  catch(Exception e){errors.Add("Malformed: "+e.GetType().Name+": "+e.Message);}
  return errors;
 }
}
