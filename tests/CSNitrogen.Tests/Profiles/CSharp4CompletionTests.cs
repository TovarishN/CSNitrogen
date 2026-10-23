using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CSharp4CompletionTests
{
 [Fact]public void Fourth_normalized_syntax_admission_is_independent_of_accepting_the_source()
 {
  var r=ProfileResolver.Resolve("CSharp4");Assert.Empty(r.Issues);Assert.True(r.Profile!.Complete);Assert.Empty(r.Profile.MissingFeatures);
  using var good=CSharpParser.Parse(new("interface I<out T>{void M(int x=1);}","CSharp4","good.cs",[]));
  using var bad=CSharpParser.Parse(new("class C{","CSharp4","bad.cs",[]));
  Assert.True(good.Accepted);Assert.False(bad.Accepted);Assert.True(good.ProfileComplete);Assert.True(bad.ProfileComplete);Assert.Equal(good.CoverageIdentity,bad.CoverageIdentity);
 }
 [Fact]public void Fourth_admission_keeps_predecessors_admitted_and_later_and_custom_profiles_partial()
 {
  foreach(string name in new[]{"CSharp1","CSharp2","CSharp3","CSharp4"})Assert.True(ProfileResolver.Resolve(name).Profile!.Complete);
  foreach(string name in Enumerable.Range(5,10).Select(n=>$"CSharp{n}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))Assert.False(ProfileResolver.Resolve(name).Profile!.Complete);
  Assert.False(ProfileResolver.ResolveCustom("CSharp4",FeatureCatalog.Implemented.Where(f=>f.Introduced<=new Version(4,0)).Select(f=>f.Id)).Profile!.Complete);
 }
}
