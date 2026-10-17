using CSNitrogen.Profiles;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Profiles;
public class CSharp2CompletionTests
{
 [Fact]public void Reviewed_second_edition_is_admitted_by_the_public_profile_contract()
 {
  var resolution=ProfileResolver.Resolve("CSharp2");Assert.Empty(resolution.Issues);Assert.True(resolution.Profile!.Complete);Assert.Empty(resolution.Profile.MissingFeatures);
  Assert.Contains("unicode-4.0-scalar",resolution.Profile.Policies);Assert.DoesNotContain("unicode-current-bmp",resolution.Profile.Policies);
  using var p=CSharpParser.Parse(new("class C<T>{T M(){return default(T);}}","CSharp2","complete.cs",[]));Assert.True(p.Accepted);Assert.True(p.ProfileComplete);
 }
}
