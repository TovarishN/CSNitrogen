using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;

namespace CSNitrogen.Tests.Profiles;

public class CSharp6CoverageContractTests
{
    [Fact]
    public void The_sixth_gate_records_the_actual_factory_and_controls_profile_completeness()
    {
        var resolution = ProfileResolver.Resolve("CSharp6");
        Assert.Empty(resolution.Issues);
        Assert.True(CSharpParser.MatchesFactory("CSharp6", FeatureCatalog.ModulesFor(6)));
        Assert.False(CSharpParser.MatchesFactory("CSharp6", FeatureCatalog.ModulesFor(5)));
        Assert.Equal(FeatureCatalog.SourceReviewedFor(6), resolution.Profile!.Complete);
        using var valid = CSharpParser.Parse(new("class C{}", "CSharp6", "valid.cs", []));
        using var invalid = CSharpParser.Parse(new("class C{", "CSharp6", "invalid.cs", []));
        Assert.True(valid.Accepted);
        Assert.False(invalid.Accepted);
        Assert.Equal(resolution.Profile.Complete, valid.ProfileComplete);
        Assert.Equal(valid.ProfileComplete, invalid.ProfileComplete);
    }

    private const string GateResource = "CSNitrogen.csharp6-coverage-gate.json";
    private const string ApprovalResource = "CSNitrogen.csharp6-approval.json";
    private const string CompositionResource = "CSNitrogen.composition.json";
    private const string ReferenceResource = "CSNitrogen.csharp6-artifact.csharp6-reference-policy.json";
    private const string DispositionsResource = "CSNitrogen.csharp6-artifact.csharp6-main-text-dispositions.json";
    private const string MatrixResource = "CSNitrogen.csharp6-evidence.alternative-witness-matrix.json";
    private const string SourceCasesResource = "CSNitrogen.csharp6-evidence.source-specific-cases.json";

    private static Dictionary<string, byte[]> Inputs()
    {
        var assembly = typeof(FeatureCatalog).Assembly;
        return assembly.GetManifestResourceNames().Where(name => name.StartsWith("CSNitrogen.", StringComparison.Ordinal))
            .ToDictionary(name => name, name =>
            {
                using var stream = assembly.GetManifestResourceStream(name)!;
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
                return buffer.ToArray();
            }, StringComparer.Ordinal);
    }

    private static byte[] Approval(byte[] ledger, string reviewer = "Test reviewer", string status = "approved") =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            schemaVersion = 1, profile = "CSharp6", status, reviewer,
            approvedOn = "2026-11-19", ledgerSha256 = ContractJson.Hash(ledger)
        });

    [Fact]
    public void The_exact_snapshot_requires_both_a_fifth_admission_and_sixth_approval()
    {
        var inputs = Inputs();
        byte[] ledger = inputs[GateResource];
        bool approved = inputs.ContainsKey(ApprovalResource);
        Assert.Equal(approved, FeatureCatalog.SourceReviewedFor(6));
        Assert.False(ProfileResolver.Resolve("CSharp7").Profile!.Complete);
        Assert.False(ProfileResolver.ResolveCustom("CSharp6", FeatureCatalog.Implemented.Select(f => f.Id)).Profile!.Complete);

        inputs.Remove(ApprovalResource);
        Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, true));
        inputs[ApprovalResource] = Approval(ledger, reviewer: " ");
        Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, true));
        inputs[ApprovalResource] = Approval(ledger, status: "pending");
        Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, true));
        inputs[ApprovalResource] = Approval(ledger);
        Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, false));
        Assert.True(CSharp6CoverageContract.Reviewed(ledger, inputs, true));
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{}")]
    [InlineData("{\"factory\":{\"modules\":17}}")]
    [InlineData("{\"factory\":{\"modules\":[null]}}")]
    public void A_malformed_sixth_ledger_yields_no_factory_modules(string ledger)
    {
        Assert.Empty(CSharp6CoverageContract.ReadFactoryModules(Encoding.UTF8.GetBytes(ledger)));
    }

    [Fact]
    public void Every_pinned_input_rejects_byte_drift_or_omission()
    {
        var inputs = Inputs();
        byte[] ledger = inputs[GateResource];
        inputs[ApprovalResource] = Approval(ledger);
        foreach (string name in inputs.Keys.Where(CSharp6CoverageContract.Eligible).ToArray())
        {
            byte[] original = inputs[name];
            inputs[name] = [..original, 32];
            Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, true), name);
            inputs.Remove(name);
            Assert.False(CSharp6CoverageContract.Reviewed(ledger, inputs, true), name);
            inputs[name] = original;
        }
        byte[] changedLedger = [..ledger, 32];
        Assert.False(CSharp6CoverageContract.Reviewed(changedLedger, inputs, true, ContractJson.Hash(changedLedger)));
    }

    [Theory]
    [InlineData("factory")]
    [InlineData("composition")]
    [InlineData("unicode")]
    [InlineData("exclusion")]
    [InlineData("standalone-await-exclusion")]
    [InlineData("long-underscore-exclusion")]
    [InlineData("disposition")]
    [InlineData("witness")]
    [InlineData("wrong-predecessor-witness")]
    [InlineData("focus")]
    public void Structural_drift_fails_even_with_recomputed_digests(string change)
    {
        var inputs = Inputs();
        var ledger = JsonNode.Parse(inputs[GateResource])!;
        void Mutate(string resource, Action<JsonNode> edit)
        {
            var document = JsonNode.Parse(inputs[resource])!;
            edit(document);
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document);
            inputs[resource] = bytes;
            ledger["reviewInputs"]![resource] = ContractJson.Hash(bytes);
        }

        switch (change)
        {
            case "factory":
                ledger["factory"]!["modules"]![0] = "CSharp.Unreviewed";
                break;
            case "composition":
                Mutate(CompositionResource, d => d["languages"]!.AsArray()
                    .Single(x => (string?)x!["name"] == "CSharp6")!["start"] = "CSharp.Other.Start");
                break;
            case "unicode":
                Mutate(ReferenceResource, d => d["unicode"]!["policy"] = "unicode-current-bmp");
                break;
            case "exclusion":
                Mutate(ReferenceResource, d => d["normalizedExclusions"]!.AsArray().RemoveAt(0));
                break;
            case "standalone-await-exclusion":
            case "long-underscore-exclusion":
                var omitted = change == "standalone-await-exclusion"
                    ? "inherited-standalone-await-identifier-statement" : "long-unicode-escape-for-initial-underscore";
                Mutate(ReferenceResource, d =>
                {
                    var exclusions = d["normalizedExclusions"]!.AsArray();
                    Assert.True(exclusions.Remove(exclusions.Single(e => (string?)e == omitted)));
                });
                break;
            case "disposition":
                Mutate(DispositionsResource, d => d["dispositions"]!.AsArray().RemoveAt(0));
                break;
            case "witness":
                Mutate(MatrixResource, d => d["cases"]![0]!["witnessId"] = "missing");
                break;
            case "wrong-predecessor-witness":
                Mutate(MatrixResource, d =>
                {
                    var links = d["cases"]!.AsArray();
                    var input = links.Single(row => (string?)row!["id"] == "A:22:input/1")!;
                    input["predecessorAlternative"] = "B:9648:input-section-part/1";
                    input["witnessId"] = "B:8396:input-section-part/1";
                });
                break;
            case "focus":
                Mutate(SourceCasesResource, d => d["cases"]![0]!["focusStart"] = -1);
                Mutate(MatrixResource, d => d["sourceSpecific"]!["sha256"] = ContractJson.Hash(inputs[SourceCasesResource]));
                break;
        }

        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(ledger);
        inputs[ApprovalResource] = Approval(bytes);
        Assert.False(CSharp6CoverageContract.Reviewed(bytes, inputs, true, ContractJson.Hash(bytes)));
    }
}
