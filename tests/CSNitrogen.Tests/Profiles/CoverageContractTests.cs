using System.Text.Json.Nodes;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Profiles;
public class CoverageContractTests
{
    private static JsonObject Resource(string name)
    {using var stream=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;return JsonNode.Parse(stream)!.AsObject();}
    private static bool Reviewed(JsonObject? gate=null,JsonObject? map=null,JsonObject? index=null,JsonObject? reference=null,JsonObject? config=null)
        =>CoverageContract.Reviewed(System.Text.Json.JsonSerializer.SerializeToElement(gate??Resource("csharp1-coverage-gate.json")),
            System.Text.Json.JsonSerializer.SerializeToElement(map??Resource("csharp1-standard-crosswalk.json")),
            System.Text.Json.JsonSerializer.SerializeToElement(index??Resource("csharp1-standard-index.json")),
            System.Text.Json.JsonSerializer.SerializeToElement(reference??Resource("reference-policy.json")),System.Text.Json.JsonSerializer.SerializeToElement(config??Resource("composition.json")));
    [Fact] public void Embedded_contract_agrees_with_the_actual_named_factory_and_immutable_mapping_catalog()
    {
        Assert.True(Reviewed());Assert.Equal(40,FeatureCatalog.CoverageMappings.Count(m=>FeatureCatalog.Inventory.Any(f=>f.Id==m.InventoryId && f.Introduced==new Version(1,0))));
        Assert.True(CSharpParser.MatchesHistoricalFactory(FeatureCatalog.HistoricalModules));
        Assert.False(CSharpParser.MatchesHistoricalFactory(FeatureCatalog.HistoricalModules.Skip(1).ToArray()));
        var list=Assert.IsAssignableFrom<IList<CoverageMapping>>(FeatureCatalog.CoverageMappings);
        Assert.Throws<NotSupportedException>(()=>list.Clear());
        var caps=Assert.IsAssignableFrom<IList<string>>(FeatureCatalog.CoverageMappings[0].Capabilities);
        Assert.Throws<NotSupportedException>(()=>caps.Clear());
    }
    [Theory]
    [InlineData("factory-start")][InlineData("factory-namespace")][InlineData("factory-grammar")][InlineData("duplicate-factory")]
    [InlineData("pending-dimension")][InlineData("missing-dimension")][InlineData("extra-dimension")]
    [InlineData("duplicate-row")][InlineData("missing-row")][InlineData("wrong-source")]
    [InlineData("wrong-shared-source")][InlineData("changed-index-name")][InlineData("missing-owner")]
    [InlineData("unknown-owner")][InlineData("unknown-policy")][InlineData("unresolved-mapping")]
    [InlineData("pending-module")][InlineData("pending-policy")][InlineData("pending-obligation")]
    [InlineData("unknown-policy-group")][InlineData("wrong-policy-group-source")][InlineData("swapped-policy-group-sources")]
    public void Source_review_cannot_be_forged_by_verified_row_statuses(string mutation)
    {
        var gate=Resource("csharp1-coverage-gate.json");var map=Resource("csharp1-standard-crosswalk.json");
        var index=Resource("csharp1-standard-index.json");var reference=Resource("reference-policy.json");var config=Resource("composition.json");
        var rows=map["productions"]!.AsArray();var first=rows[0]!;
        switch(mutation)
        {
            case "factory-start":config["languages"]![0]!["start"]="Fake.Start";break;
            case "factory-namespace":config["languages"]![0]!["namespace"]="Fake";break;
            case "factory-grammar":gate["modules"]![0]!["grammar"]="Fake.ngr";break;
            case "duplicate-factory":config["languages"]!.AsArray().Add(config["languages"]![0]!.DeepClone());break;
            case "pending-dimension":first["reviews"]!["normative-prose"]="pending";break;
            case "missing-dimension":first["reviews"]!.AsObject().Remove("normative-prose");break;
            case "extra-dimension":first["reviews"]!["invented"]="verified";break;
            case "duplicate-row":rows[1]=first.DeepClone();break;
            case "missing-row":rows.RemoveAt(0);break;
            case "wrong-source":map["sourceSha256"]=new string('0',64);break;
            case "wrong-shared-source":map["sourceSha256"]=new string('0',64);index["source"]!["sha256"]=new string('0',64);reference["primary"]!["sha256"]=new string('0',64);break;
            case "changed-index-name":index["productions"]![0]!["name"]="invented";break;
            case "missing-owner":first["targets"]=new JsonArray();first["policies"]=new JsonArray();break;
            case "unknown-owner":first["targets"]![0]="Fake/rule:None";break;
            case "unknown-policy":first["policies"]!.AsArray().Add("fake");break;
            case "unresolved-mapping":first["mapping"]="unresolved";break;
            case "pending-module":gate["modules"]![0]!["status"]="observed";break;
            case "pending-policy":gate["policies"]![0]!["status"]="observed";break;
            case "pending-obligation":gate["obligations"]![0]!["status"]="pending";break;
            case "unknown-policy-group":gate["policyGroups"]!["fake"]=new JsonArray("Parsing/CSharpTrivia.cs");break;
            case "swapped-policy-group-sources":
                var original=gate["policyGroups"]!["csharp-trivia"]!.DeepClone();
                gate["policyGroups"]!["csharp-trivia"]=gate["policyGroups"]!["attribute-argument-order"]!.DeepClone();
                gate["policyGroups"]!["attribute-argument-order"]=original;break;
            case "wrong-policy-group-source":gate["policyGroups"]!["csharp-trivia"]![0]="Fake.cs";break;
        }
        Assert.False(Reviewed(gate,map,index,reference,config));
    }
    [Theory]
    [InlineData("missing-mapping")][InlineData("empty-mapping")][InlineData("duplicate-mapping")]
    [InlineData("unknown-inventory")][InlineData("unknown-capability")][InlineData("unknown-policy")]
    [InlineData("duplicate-target")][InlineData("missing-capability")][InlineData("missing-policy")]
    [InlineData("factory-grammar")][InlineData("duplicate-grammar")][InlineData("actual-factory")]
    [InlineData("unknown-selected-capability")][InlineData("unknown-selected-policy")]
    [InlineData("source-unreviewed")][InlineData("feature-unverified")][InlineData("wrong-unicode")][InlineData("dual-unicode")]
    public void Coverage_completeness_fails_closed_on_missing_or_invalid_requirements(string mutation)
    {
        var profile=ProfileResolver.Resolve("CSharp1").Profile!;
        var mappings=FeatureCatalog.CoverageMappings.ToList();var inventory=FeatureCatalog.Inventory.ToList();
        var selected=profile.Features.ToList();var policies=profile.Policies.ToList();var factory=profile.Grammars.ToList();
        bool source=true,actual=true;
        switch(mutation)
        {
            case "missing-mapping":mappings.RemoveAt(0);break;
            case "empty-mapping":mappings[0]=mappings[0] with{Capabilities=[],Policies=[]};break;
            case "duplicate-mapping":mappings.Add(mappings[0]);break;
            case "unknown-inventory":mappings[0]=mappings[0] with{InventoryId="fake"};break;
            case "unknown-capability":mappings[0]=mappings[0] with{Capabilities=["fake"]};break;
            case "unknown-policy":mappings[0]=mappings[0] with{Policies=["fake"]};break;
            case "duplicate-target":mappings[0]=mappings[0] with{Capabilities=["baseline.members","baseline.members"]};break;
            case "unknown-selected-capability":selected.Add("fake");break;
            case "unknown-selected-policy":policies.Add("fake");break;
            case "missing-capability":selected.Remove("baseline.members");break;
            case "missing-policy":policies.Remove("csharp1-preprocessing");break;
            case "factory-grammar":factory.RemoveAt(0);break;
            case "duplicate-grammar":factory.Add(factory[0]);break;
            case "actual-factory":actual=false;break;
            case "source-unreviewed":source=false;break;
            case "feature-unverified":inventory[0]=inventory[0] with{Reviewed=false};break;
            case "dual-unicode":policies.Add("unicode-current-bmp");break;
            case "wrong-unicode":policies.Remove("unicode-3.0-bmp");policies.Add("unicode-current-bmp");break;
        }
        var result=CoverageContract.Evaluate(new(1,0),inventory,mappings,FeatureCatalog.Implemented,selected,policies,FeatureCatalog.KnownPolicies,profile.Grammars,factory,source,actual);
        Assert.False(result.Complete);
        if(mutation is "missing-mapping" or "missing-capability" or "missing-policy")Assert.NotEmpty(result.Missing);
        if(mutation is "empty-mapping" or "duplicate-mapping" or "unknown-inventory" or "unknown-capability" or "unknown-policy" or "duplicate-target" or "factory-grammar" or "duplicate-grammar" or "actual-factory" or "unknown-selected-capability" or "unknown-selected-policy")Assert.NotEmpty(result.Issues);
    }
}
