using CSNitrogen.Profiles;
using System.Text.Json;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3SourceIndexTests
{
 [Fact]public void Source_index_and_approved_Unicode_authority_are_embedded_independently_of_admission()
 {
  var assembly=typeof(FeatureCatalog).Assembly;var resources=assembly.GetManifestResourceNames();
  Assert.Contains("CSNitrogen.csharp3-standard-index.json",resources);Assert.Contains("CSNitrogen.csharp3-reference-policy.json",resources);
  using var source=JsonDocument.Parse(assembly.GetManifestResourceStream("CSNitrogen.csharp3-standard-index.json")!);
  var root=source.RootElement;Assert.Equal("indexed-unreviewed",root.GetProperty("status").GetString());
  var rows=root.GetProperty("productions").EnumerateArray().ToArray();Assert.Equal(455,rows.Length);Assert.Equal(425,rows.Select(r=>r.GetProperty("name").GetString()).Distinct().Count());
  Assert.Equal(2,rows.Count(r=>r.GetProperty("name").GetString() is "explicit-anonymous-function-parameter-list" or "implicit-anonymous-function-parameter-list"));
  Assert.Equal(rows.Length,rows.Select(r=>r.GetProperty("id").GetString()).Distinct().Count());
  Assert.All(rows,r=>{Assert.StartsWith("B:",r.GetProperty("id").GetString());Assert.Equal(64,r.GetProperty("sourceBodySha256").GetString()!.Length);});
  using var policy=JsonDocument.Parse(assembly.GetManifestResourceStream("CSNitrogen.csharp3-reference-policy.json")!);
  Assert.Equal("unicode-4.0-scalar",policy.RootElement.GetProperty("unicode").GetProperty("policy").GetString());
  Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 [Fact]public void Repeated_dimension_separator_occurrences_are_independently_indexed()
 {
  using var source=JsonDocument.Parse(typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen.csharp3-standard-index.json")!);
  var occurrences=source.RootElement.GetProperty("productions").EnumerateArray().Where(r=>r.GetProperty("name").GetString()=="dim-separators").Select(r=>r.GetProperty("id").GetString()).ToArray();
  Assert.Equal(new[]{"B:8527:dim-separators","B:8785:dim-separators"},occurrences);
 }
}
