using CSNitrogen.Profiles;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Profiles;
public class ProfileResolverTests
{
    [Fact] public void Named_profiles_are_explicit_and_partial()
    {
        foreach(var name in Enumerable.Range(1,14).Select(i=>$"CSharp{i}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))
        {
            var resolution=ProfileResolver.Resolve(name); Assert.Empty(resolution.Issues); Assert.NotNull(resolution.Profile);
            Assert.False(resolution.Profile.Complete); Assert.Contains("foundation.empty-classes",resolution.Profile.Features);
            Assert.Contains("baseline.conditional-compilation",resolution.Profile.Features);
            Assert.Contains("Grammars/Lexical.ngr",resolution.Profile.Grammars);
            Assert.Contains("Grammars/CompilationUnits.ngr",resolution.Profile.Grammars);
            Assert.Contains("Grammars/Unsafe.ngr",resolution.Profile.Grammars);
            using var parsed=CSharpParser.Parse(new("class C {}",name,"test.cs",[])); Assert.True(parsed.Accepted);
        }
        Assert.Equal(new Version(7,0),ProfileResolver.Resolve("CSharp7").Profile!.Version);
        Assert.Equal(new Version(7,3),ProfileResolver.Resolve("CSharp7_3").Profile!.Version);
        Assert.Contains(ProfileResolver.Resolve("CSharp15").Issues,i=>i.Code=="unknown-profile");
    }
    private static readonly FeatureDefinition[] FixtureCatalog = [
        new("base", [], [], ["base.ngr"]),new("lambda", ["base"],[],["lambda.ngr"]),
        new("query",["lambda"],[],["query.ngr"]),new("one",[],["two"],[]),new("two",[],["one"],[])];
    [Fact] public void Missing_unknown_duplicate_and_conflicting_features_are_errors()
    {
        Assert.Contains(ProfileResolver.ResolveCustom("custom",["base","query"],FixtureCatalog).Issues,i=>i.Code=="missing-requirement" && i.Message.Contains("lambda"));
        Assert.Contains(ProfileResolver.ResolveCustom("custom",["base","unknown"],FixtureCatalog).Issues,i=>i.Code=="unknown-feature");
        Assert.Single(ProfileResolver.ResolveCustom("custom",["base","base"],FixtureCatalog).Issues);
        Assert.Contains(ProfileResolver.ResolveCustom("custom",["one","two"],FixtureCatalog).Issues,i=>i.Code=="conflict");
    }
    [Fact] public void Resolution_does_not_depend_on_selection_order()
    {
        var a=ProfileResolver.ResolveCustom("custom",["base","lambda","query"],FixtureCatalog);
        var b=ProfileResolver.ResolveCustom("custom",["query","lambda","base"],FixtureCatalog);
        Assert.Empty(a.Issues); Assert.Equal(a.Profile!.Features,b.Profile!.Features); Assert.Equal(a.Profile.Grammars,b.Profile.Grammars);
        var c=ProfileResolver.ResolveCustom("custom",["query","unknown"],FixtureCatalog);
        var d=ProfileResolver.ResolveCustom("custom",["unknown","query"],FixtureCatalog);
        Assert.Equal(c.Issues,d.Issues);
    }
    [Fact] public void Cyclic_or_duplicate_catalog_is_rejected()
    {
        Assert.Contains(ProfileResolver.ResolveCustom("c",["a","b"],[new("a",["b"],[],[]),new("b",["a"],[],[])]).Issues,i=>i.Code=="catalog-cycle");
        Assert.Contains(ProfileResolver.ResolveCustom("c",["a"],[new("a",[],[],[]),new("a",[],[],[])]).Issues,i=>i.Code=="catalog-duplicate");
    }
    [Fact] public void Requested_coverage_is_distinct_from_implemented_membership()
    {
        var first=ProfileResolver.Resolve("CSharp1").Profile!;
        var latest=ProfileResolver.Resolve("CSharp14").Profile!;
        Assert.Contains("baseline.unicode-identifiers",first.RequestedFeatures);
        Assert.DoesNotContain(first.RequestedFeatures,id=>id.StartsWith("csharp-14."));
        Assert.Contains(latest.RequestedFeatures,id=>id.StartsWith("csharp-14."));
        Assert.True(latest.RequestedFeatures.Count>first.RequestedFeatures.Count);
        Assert.Contains("baseline.regular-strings",latest.Features); Assert.NotEmpty(latest.MissingFeatures);
        Assert.Equal(64,latest.CoverageIdentity.Length);
        using var doc=CSharpParser.Parse(new("class C {}","CSharp14","test.cs",[]));
        Assert.Equal(latest.CoverageIdentity,doc.CoverageIdentity);
    }
 [Fact]
 public void Unicode_policy_membership_matches_each_generated_configuration()
 {
  using var stream=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen.composition.json");
  using var config=System.Text.Json.JsonDocument.Parse(stream!);
  foreach(var language in config.RootElement.GetProperty("languages").EnumerateArray())
  {
   string name=language.GetProperty("name").GetString()!;if(name=="CSharp1Safe")continue;
   var profile=ProfileResolver.Resolve(name).Profile!;
   Assert.Equal(language.GetProperty("grammars").EnumerateArray().Select(g=>g.GetString()!).Order(StringComparer.Ordinal),profile.Grammars);
   Assert.Contains(name=="CSharp1"?"unicode-3.0-bmp":"unicode-current-bmp",profile.Policies);
  }
 }
}
