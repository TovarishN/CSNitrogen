using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Xunit.Abstractions;

namespace CSNitrogen.Oracle.Tests;

public class CSharp1CoverageGateTests(ITestOutputHelper output)
{
    private static JsonObject Ledger() => JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "csharp1-coverage-gate.json")))!.AsObject();

    [Fact]
    public void Current_catalog_and_runtime_contract_agree_on_complete_historical_syntax()
    {
        var report = CSharp1CoverageGateAudit.Evaluate(Ledger());
        Assert.Empty(report.Errors); Assert.True(report.Complete);
        Assert.Empty(report.Blockers);
        Assert.True(ProfileResolver.Resolve("CSharp1").Profile!.Complete);
        output.WriteLine($"Audit: modules={report.Modules}, authored={report.Productions}, policySources={report.PolicySources}, sourceCodes={report.PolicyCodes}, syntaxFeatures={report.SyntaxFeatures}, blockers={report.Blockers.Count}");
    }

    [Fact]
    public void Source_review_cannot_bypass_the_runtime_profile_contract()
    {
        var ledger=Ledger();var inventory=CSharp1CoverageGateAudit.Inventory();
        foreach(string group in new[]{"modules","policies"})
            foreach(var row in ledger[group]!.AsArray())row!["status"]="verified";
        foreach(var obligation in ledger["obligations"]!.AsArray())
            obligation!["status"]=obligation["id"]!.GetValue<string>()=="runtime-profile-contract"?"pending":"verified";
        foreach(var feature in inventory["features"]!.AsArray()){feature!["status"]="verified";feature["reviewed"]=true;}
        var map=JsonNode.Parse(typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen.csharp1-standard-crosswalk.json")!)!.AsObject();
        foreach(var row in map["productions"]!.AsArray())
        {row!["status"]="verified";foreach(var key in row["reviews"]!.AsObject().Select(p=>p.Key).ToArray())row["reviews"]![key]="verified";}
        var report=CSharp1CoverageGateAudit.Evaluate(ledger,inventory,crosswalk:map);
        Assert.Empty(report.Errors);Assert.False(report.Complete);
        Assert.Contains(report.Blockers,b=>b.Contains("runtime-profile-contract",StringComparison.Ordinal));
        var profile=ProfileResolver.Resolve("CSharp1").Profile!;
        Assert.True(profile.Complete);Assert.Empty(profile.MissingFeatures);
        Assert.Contains(ProfileResolver.ResolveCustom("events-only",["csharp-1.0.events"]).Issues,i=>i.Code=="coverage-only-feature");
    }

    [Theory]
    [InlineData("remove-module", "ModuleSet")]
    [InlineData("duplicate-module", "ModuleSet")]
    [InlineData("remove-production", "Productions")]
    [InlineData("extra-production", "Productions")]
    [InlineData("duplicate-production", "Productions")]
    [InlineData("remove-policy", "PolicySet")]
    [InlineData("extra-code", "PolicyCodes")]
    [InlineData("remove-code", "PolicyCodes")]
    [InlineData("broken-fixture", "Evidence")]
    [InlineData("missing-negative", "Evidence")]
    [InlineData("missing-exclusion", "SemanticExclusions")]
    [InlineData("missing-obligation", "Obligations")]
    [InlineData("wrong-policy-source", "PolicyGroups")]
    [InlineData("ungrouped-policy", "PolicyGroups")]
    [InlineData("unknown-policy-group", "PolicyGroups")]
    [InlineData("unknown-status", "Status")]
    [InlineData("override-complete", "Schema")]
    public void Incomplete_or_invented_catalog_evidence_fails_closed(string mutation, string expectedError)
    {
        var ledger = Ledger(); var modules = ledger["modules"]!.AsArray(); var policies = ledger["policies"]!.AsArray();
        switch (mutation)
        {
            case "remove-module": modules.RemoveAt(0); break;
            case "duplicate-module": modules.Add(modules[0]!.DeepClone()); break;
            case "remove-production": modules[0]!["productions"]!.AsArray().RemoveAt(0); break;
            case "extra-production": modules[0]!["productions"]!.AsArray().Add("rule:Invented"); break;
            case "duplicate-production": modules[0]!["productions"]!.AsArray().Add(modules[0]!["productions"]![0]!.DeepClone()); break;
            case "remove-policy": policies.RemoveAt(0); break;
            case "extra-code": policies[0]!["codes"]!.AsArray().Add("CSN9999"); break;
            case "remove-code": policies[0]!["codes"]!.AsArray().RemoveAt(0); break;
            case "broken-fixture": modules[0]!["positive"]![0]!["method"] = "Invented_method"; break;
            case "missing-negative": modules[0]!["negative"] = new JsonArray(); break;
            case "missing-exclusion": ledger["semanticExclusions"]!.AsArray().RemoveAt(0); break;
            case "missing-obligation": ledger["obligations"]!.AsArray().RemoveAt(0); break;
            case "wrong-policy-source": ledger["policyGroups"]!["csharp-trivia"]![0] = "Profiles/CSharpTrivia.cs"; break;
            case "ungrouped-policy": ledger["policyGroups"]!["generated-syntax-context-checks"]!.AsArray().RemoveAt(3); break;
            case "unknown-policy-group": ledger["policyGroups"]!["invented"] = new JsonArray("Parsing/CSharpTrivia.cs"); break;
            case "unknown-status": modules[0]!["status"] = "complete-enough"; break;
            case "override-complete": ledger["profileComplete"] = true; break;
        }
        var report = CSharp1CoverageGateAudit.Evaluate(ledger);
        Assert.False(report.Complete); Assert.Contains(report.Errors, e => e.StartsWith(expectedError, StringComparison.Ordinal));
    }

    [Fact]
    public void Promoting_grouped_samples_cannot_bypass_standard_review_obligations()
    {
        var ledger = Ledger();
        foreach (var item in ledger["modules"]!.AsArray().Concat(ledger["policies"]!.AsArray())) item!["status"] = "verified";
        var inventory = CSharp1CoverageGateAudit.Inventory();
        foreach (var feature in inventory["features"]!.AsArray()) feature!["status"] = "verified";
        foreach(var obligation in ledger["obligations"]!.AsArray())
            obligation!["status"]=obligation["id"]!.GetValue<string>()=="historical-standard-crosswalk"?"pending":"verified";
        var report = CSharp1CoverageGateAudit.Evaluate(ledger, inventory);
        Assert.False(report.Complete); Assert.Contains(report.Blockers, b => b.Contains("historical-standard-crosswalk", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("missing")][InlineData("empty")][InlineData("unknown-capability")][InlineData("unknown-policy")]
    public void Coverage_mappings_are_an_audited_completeness_requirement(string mutation)
    {
        var inventory=CSharp1CoverageGateAudit.Inventory();
        var feature=inventory["features"]!.AsArray().First(f=>f!["id"]!.GetValue<string>()=="csharp-1.0.events")!;
        switch(mutation)
        {
            case "missing":feature.AsObject().Remove("coverage");break;
            case "empty":feature["coverage"]!["capabilities"]=new JsonArray();feature["coverage"]!["policies"]=new JsonArray();break;
            case "unknown-capability":feature["coverage"]!["capabilities"]![0]="fake";break;
            case "unknown-policy":feature["coverage"]!["policies"]=new JsonArray("fake");break;
        }
        var report=CSharp1CoverageGateAudit.Evaluate(Ledger(),inventory);
        Assert.False(report.Complete);
        Assert.True(report.Errors.Any(e=>e.StartsWith("CoverageMappings",StringComparison.Ordinal)) || report.Blockers.Any(b=>b.StartsWith("RuntimeCoverage",StringComparison.Ordinal)));
    }

    [Fact]
    public void Unknown_inventory_classification_cannot_silently_be_excluded()
    {
        var inventory = CSharp1CoverageGateAudit.Inventory();
        inventory["features"]!.AsArray().First(f => f!["introduced"]!.GetValue<string>() == "1.0")!["classification"] = "unknown";
        var report = CSharp1CoverageGateAudit.Evaluate(Ledger(), inventory);
        Assert.False(report.Complete); Assert.Contains(report.Errors, e => e.StartsWith("Classification", StringComparison.Ordinal));
    }

    [Fact]
    public void Unreviewed_semantic_classification_cannot_bypass_the_syntax_gate()
    {
        var inventory = CSharp1CoverageGateAudit.Inventory();
        inventory["features"]!.AsArray().First(f => f!["id"]!.GetValue<string>() == "csharp-1.0.boxing-and-unboxing")!["reviewed"] = false;
        var report = CSharp1CoverageGateAudit.Evaluate(Ledger(), inventory);
        Assert.False(report.Complete); Assert.Contains(report.Errors, e => e.StartsWith("SemanticExclusions", StringComparison.Ordinal));
    }

    [Fact]
    public void Coverage_identity_binds_the_completion_ledger()
    {
        var assembly = typeof(FeatureCatalog).Assembly;
        const string gate = "CSNitrogen.csharp1-coverage-gate.json";
        Assert.Contains(gate, assembly.GetManifestResourceNames());
        string[] fixedInputs = ["CSNitrogen.csharp3-reference-policy.json", "CSNitrogen.csharp3-standard-index.json", "CSNitrogen.csharp2-coverage-gate.json", "CSNitrogen.csharp2-standard-crosswalk.json", "CSNitrogen.csharp2-reference-policy.json", "CSNitrogen.csharp2-standard-index.json", "CSNitrogen.coverage.json", "CSNitrogen.dependencies.json", "CSNitrogen.composition.json", "CSNitrogen.reference-policy.json", gate, "CSNitrogen.csharp1-standard-index.json", "CSNitrogen.csharp1-standard-crosswalk.json"];
        var names = assembly.GetManifestResourceNames().Where(n => fixedInputs.Contains(n) || n.StartsWith("CSNitrogen.grammar.", StringComparison.Ordinal) || n.StartsWith("CSNitrogen.source.", StringComparison.Ordinal) || n.StartsWith("CSNitrogen.review.", StringComparison.Ordinal));
        var inputs = names.Select(n => { using var stream = assembly.GetManifestResourceStream(n)!; using var bytes = new MemoryStream(); stream.CopyTo(bytes); return new KeyValuePair<string, byte[]>(n, bytes.ToArray()); }).ToArray();
        Assert.Equal(FeatureCatalog.CoverageIdentity, CoverageHash.Compute(inputs));
        var without = inputs.Where(p => p.Key != gate); Assert.NotEqual(FeatureCatalog.CoverageIdentity, CoverageHash.Compute(without));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("class C{}", true)]
    [InlineData("class C{} @", false)]
    public void Legacy_alias_preserves_full_consumption(string source, bool accepted)
    {
        var language = CSharp1Composition.CreateBuilder().Build();
        using var parsed = language.Parse(source, CoreModule.LegacyCompilationUnit);
        Assert.Equal(accepted, parsed.Success && !parsed.HasErrors);
    }
}
