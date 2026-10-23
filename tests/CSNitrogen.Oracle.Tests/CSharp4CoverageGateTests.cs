using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4CoverageGateTests
{
 static byte[] Bytes(string name){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static JsonNode Read(string name)=>JsonNode.Parse(Bytes(name))!;
 static string Text(JsonNode n,string key)=>n[key]!.GetValue<string>();
 static string Hash(byte[] b)=>Convert.ToHexStringLower(SHA256.HashData(b));
 [Fact]public void All_reviewed_occurrences_bind_the_candidate_owners_and_source_context_report()
 {
  var reviewed=Read("csharp4-artifact.csharp4-reviewed-occurrences.json");var map=Read("csharp4-artifact.csharp4-standard-crosswalk.json");var index=Read("csharp4-artifact.csharp4-standard-index.json");
  Assert.Equal("verified",Text(reviewed,"status"));Assert.Equal("owner-indexed-unreviewed",Text(map,"status"));Assert.Equal("indexed-unreviewed",Text(index,"status"));
  Assert.Equal(Hash(Bytes("csharp4-artifact.csharp4-standard-crosswalk.json")),Text(reviewed,"ownerCrosswalkSha256"));Assert.Equal(Hash(Bytes("csharp4-review.csharp4-normative-context-review.md")),Text(reviewed,"contextReportSha256"));
  var rows=reviewed["productions"]!.AsArray();Assert.Equal(458,rows.Count);Assert.Equal(index["productions"]!.AsArray().Select(r=>Text(r!,"id")).Order(),rows.Select(r=>Text(r!,"standardId")).Order());
  foreach(var (domain,count) in new[]{("lexical",100),("expression",114),("statement",52),("declaration",154),("unsafe",38)})Assert.Equal(count,rows.Count(r=>Text(r!["sourceReview"]!,"domain")==domain));
  Assert.All(rows,r=>{Assert.Equal("verified",Text(r!,"status"));Assert.All(r!["reviews"]!.AsObject(),d=>Assert.Equal("verified",d.Value!.GetValue<string>()));Assert.Equal(Text(r,"standardId"),Text(r["sourceReview"]!,"occurrenceId"));});
  Assert.Empty(CSharp4OwnerCrosswalkTests.Evaluate(map.AsObject()));Assert.True(FeatureCatalog.FourthSourceReviewed);
 }
 [Fact]public void Fourth_review_inputs_are_exact_and_contract_hash_dependencies_are_directed()
 {
  var gate=Read("csharp4-coverage-gate.json");var inputs=gate["reviewInputs"]!.AsObject();
  foreach(string contract in new[]{"CSharp2CoverageContract.cs","CSharp3CoverageContract.cs","CSharp4CoverageContract.cs"})Assert.False(inputs.ContainsKey("CSNitrogen.source.Profiles/"+contract));
  var third=Read("csharp3-coverage-gate.json")["reviewInputs"]!.AsObject();var second=Read("csharp2-coverage-gate.json")["reviewInputs"]!.AsObject();
  Assert.Equal(Hash(Bytes("source.Profiles/CSharp4CoverageContract.cs")),third["CSNitrogen.source.Profiles/CSharp4CoverageContract.cs"]!.GetValue<string>());
  Assert.Equal(Hash(Bytes("source.Profiles/CSharp4CoverageContract.cs")),second["CSNitrogen.source.Profiles/CSharp4CoverageContract.cs"]!.GetValue<string>());
  Assert.Equal(7,inputs.Count(p=>p.Key.StartsWith("CSNitrogen.csharp4-artifact.",StringComparison.Ordinal)));Assert.Equal(1,inputs.Count(p=>p.Key.StartsWith("CSNitrogen.csharp4-review.",StringComparison.Ordinal)));Assert.Equal(2,inputs.Count(p=>p.Key.StartsWith("CSNitrogen.csharp4-evidence.",StringComparison.Ordinal)));Assert.Equal(6,inputs.Count(p=>p.Key.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)));
  foreach(var p in inputs)Assert.Equal(Hash(Bytes(p.Key["CSNitrogen.".Length..])),p.Value!.GetValue<string>());
  Assert.Equal(new[]{"Parsing/CSharp4ContextPolicy.cs"},gate["policyGroups"]!["csharp4.source-context-checks"]!.AsArray().Select(n=>n!.GetValue<string>()).ToArray());
 }
 [Fact]public void Recognizable_pointer_type_argument_legality_is_an_explicit_normalized_exclusion()
 {
  var reference=Read("csharp4-artifact.csharp4-reference-policy.json");Assert.Contains("explicit-pointer-type-argument-legality",reference["normalizedExclusions"]!.AsArray().Select(n=>n!.GetValue<string>()));
  using var p=CSharpParser.Parse(new("unsafe class C{G<int*> field;}","CSharp4","excluded-legality.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
 }
}
