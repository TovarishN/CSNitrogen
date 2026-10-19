using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CanonicalCoverageTests
{
    [Fact]
    public void Historical_inventory_is_covered_without_registering_fine_grained_features()
    {
        var first=ProfileResolver.Resolve("CSharp1");Assert.Empty(first.Issues);
        Assert.Equal(40,first.Profile!.RequestedFeatures.Count);
        Assert.Empty(first.Profile.MissingFeatures);
        Assert.Equal(19,first.Profile.Features.Count);
        Assert.DoesNotContain("csharp-1.0.events",first.Profile.Features);
        Assert.Contains("baseline.members",first.Profile.Features);
        Assert.Contains("attribute-target-recognition",first.Profile.Policies);
        Assert.Contains("csharp1-preprocessing",first.Profile.Policies);
    }
    [Fact]
    public void Coverage_names_report_the_bundle_without_silently_selecting_it()
    {
        var result=ProfileResolver.ResolveCustom("events-only",["csharp-1.0.events"]);
        Assert.Null(result.Profile);
        var issue=Assert.Single(result.Issues);
        Assert.Equal("coverage-only-feature",issue.Code);
        Assert.Contains("baseline.members",issue.Message);
        Assert.Contains("not independently selectable",issue.Message);
        Assert.Contains(ProfileResolver.ResolveCustom("members",["baseline.members"]).Issues,i=>i.Code=="missing-requirement");
    }
    [Fact]
    public void Complete_historical_syntax_profile_is_independent_of_source_acceptance()
    {
        Assert.True(ProfileResolver.Resolve("CSharp1").Profile!.Complete);
        using var good=CSharpParser.Parse(new("class C{}","CSharp1","good.cs",[]));
        using var bad=CSharpParser.Parse(new("class C{","CSharp1","bad.cs",[]));
        Assert.True(good.Accepted);Assert.False(bad.Accepted);
        Assert.True(good.ProfileComplete);Assert.True(bad.ProfileComplete);
        Assert.Equal(good.CoverageIdentity,bad.CoverageIdentity);
    }
    [Fact]
    public void Later_and_custom_profiles_do_not_inherit_historical_completeness()
    {
        foreach(string name in Enumerable.Range(4,11).Select(n=>$"CSharp{n}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))
        {
            var profile=ProfileResolver.Resolve(name).Profile!;
            Assert.False(profile.Complete);
            if(name!="CSharp4")Assert.NotEmpty(profile.MissingFeatures);
        }
        var custom=ProfileResolver.ResolveCustom("CSharp1",FeatureCatalog.Implemented.Select(f=>f.Id));
        Assert.Empty(custom.Issues);Assert.False(custom.Profile!.Complete);Assert.Null(custom.Profile.Version);
    }
}
