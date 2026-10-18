using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp3CompletionTests
{
 [Fact]public void Third_profile_completion_requires_its_reviewed_syntax_snapshot()
 {
  var r=ProfileResolver.Resolve("CSharp3");Assert.Empty(r.Issues);Assert.True(r.Profile!.Complete);Assert.Empty(r.Profile.MissingFeatures);
  using var good=CSharpParser.Parse(new("class C{object M(){return x=>x;}}","CSharp3","good.cs",[]));
  using var bad=CSharpParser.Parse(new("class C{","CSharp3","bad.cs",[]));
  Assert.True(good.Accepted);Assert.False(bad.Accepted);Assert.True(good.ProfileComplete);Assert.True(bad.ProfileComplete);Assert.Equal(good.CoverageIdentity,bad.CoverageIdentity);
 }
 [Fact]public void Third_admission_preserves_predecessors_and_does_not_admit_later_or_custom_profiles()
 {
  foreach(string name in new[]{"CSharp1","CSharp2"})Assert.True(ProfileResolver.Resolve(name).Profile!.Complete);
  foreach(string name in Enumerable.Range(4,11).Select(n=>$"CSharp{n}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))Assert.False(ProfileResolver.Resolve(name).Profile!.Complete);
  Assert.False(ProfileResolver.ResolveCustom("CSharp3",FeatureCatalog.Implemented.Where(f=>f.Introduced<=new Version(3,0)).Select(f=>f.Id)).Profile!.Complete);
 }
}
