using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp4CapabilityTests
{
 [Fact]public void Fourth_snapshot_mapping_retains_semantic_dynamic_binding_exclusion()
 {
  var p=ProfileResolver.Resolve("CSharp4").Profile!;
  Assert.True(p.Complete);
  Assert.Empty(p.MissingFeatures);
  Assert.Contains("csharp4.source-context-checks",p.Policies);
  foreach(string id in new[]{"named-arguments","optional-parameters","variance"})Assert.Contains("csharp4."+id,p.Features);
  Assert.Equal("semantic-or-tooling",FeatureCatalog.Inventory.Single(f=>f.Id=="csharp-4.0.dynamic-binding").Classification);
  Assert.All(FeatureCatalog.Inventory.Where(f=>f.Introduced==new Version(4,0)&&f.Classification=="syntax-or-policy"),f=>Assert.True(f.Reviewed&&f.Status=="verified"));
  Assert.All(FeatureCatalog.Inventory.Where(f=>f.Introduced==new Version(4,0)&&f.Classification=="semantic-or-tooling"),f=>Assert.False(f.Reviewed));
 }
}
