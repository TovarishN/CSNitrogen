using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;

namespace CSNitrogen.Oracle.Tests;

public class CSharp1StandardCrosswalkTests
{
    private static JsonObject Resource(string suffix)
    {
        using var stream = typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen." + suffix);
        Assert.NotNull(stream);
        return JsonNode.Parse(stream)!.AsObject();
    }

    [Fact]
    public void Independent_standard_index_and_crosswalk_are_embedded()
    {
        var index = Resource("csharp1-standard-index.json");
        var map = Resource("csharp1-standard-crosswalk.json");
        Assert.Equal(362, index["productions"]!.AsArray().Count);
        Assert.Equal(362, map["productions"]!.AsArray().Count);
    }

    [Fact]
    public void Completion_evaluator_distinguishes_closed_source_review_from_pending_runtime_contract()
    {
        var ledger = Resource("csharp1-coverage-gate.json");
        var report = CSharp1CoverageGateAudit.Evaluate(ledger);
        Assert.DoesNotContain(report.Blockers, b => b.StartsWith("Crosswalk:", StringComparison.Ordinal));
        Assert.Contains(report.Blockers,b=>b.Contains("runtime-profile-contract",StringComparison.Ordinal));
        Assert.False(report.Complete);
    }

    [Fact]
    public void Every_standard_occurrence_has_a_valid_reviewed_owner_mapping()
    {
        var report = CSharp1StandardCrosswalkAudit.Evaluate(Resource("csharp1-standard-index.json"), Resource("csharp1-standard-crosswalk.json"), Resource("csharp1-coverage-gate.json"));
        Assert.Empty(report.Errors);
        Assert.Equal(362, report.Occurrences); Assert.Equal(346, report.DistinctNames);
        Assert.Equal(0, report.Pending); Assert.Empty(report.Blockers);
    }

    [Fact]
    public void Repeated_and_unsafe_headings_keep_their_source_occurrences()
    {
        var headings = Resource("csharp1-standard-index.json")["productions"]!.AsArray();
        var embedded = headings.Where(p => p!["name"]!.GetValue<string>() == "embedded-statement").ToArray();
        Assert.Equal(3, embedded.Length);
        Assert.Equal(3, embedded.Select(p => p!["id"]!.GetValue<string>()).Distinct().Count());
        Assert.Equal(new[] { "syntactic", "unsafe", "unsafe" }, embedded.Select(p => p!["grammar"]!.GetValue<string>()));
        Assert.Contains(headings, p => p!["name"]!.GetValue<string>() == "keyword"); // "one of" heading
        Assert.Contains(headings, p => p!["name"]!.GetValue<string>() == "conversion-operator-declarator"); // trailing heading space
    }

    [Theory]
    [InlineData("missing-heading", "CrosswalkIndex")]
    [InlineData("changed-heading", "CrosswalkIndex")]
    [InlineData("duplicate-heading", "CrosswalkIndex")]
    [InlineData("wrong-location", "CrosswalkIndex")]
    [InlineData("wrong-source", "CrosswalkSource")]
    [InlineData("wrong-map-source", "CrosswalkSource")]
    [InlineData("missing-map", "CrosswalkSet")]
    [InlineData("duplicate-map", "CrosswalkSet")]
    [InlineData("invented-map", "CrosswalkSet")]
    [InlineData("invented-target", "CrosswalkTarget")]
    [InlineData("duplicate-target", "CrosswalkTarget")]
    [InlineData("invented-policy", "CrosswalkPolicy")]
    [InlineData("missing-owner", "CrosswalkTarget")]
    [InlineData("missing-notes", "CrosswalkEvidence")]
    [InlineData("invented-strategy", "CrosswalkMapping")]
    [InlineData("invented-status", "CrosswalkStatus")]
    [InlineData("missing-requirement", "CrosswalkRequirements")]
    [InlineData("missing-review", "CrosswalkReviews")]
    [InlineData("invented-review-status", "CrosswalkReviews")]
    [InlineData("unreviewed-promotion", "CrosswalkReviews")]
    [InlineData("schema-override", "CrosswalkSchema")]
    public void Source_and_mapping_drift_fail_closed(string mutation, string expected)
    {
        var index = Resource("csharp1-standard-index.json"); var map = Resource("csharp1-standard-crosswalk.json");
        var heads = index["productions"]!.AsArray(); var rows = map["productions"]!.AsArray(); var row = rows[0]!;
        switch (mutation)
        {
            case "missing-heading": heads.RemoveAt(0); break;
            case "changed-heading": heads[0]!["name"] = "invented"; break;
            case "duplicate-heading": heads.Add(heads[0]!.DeepClone()); break;
            case "wrong-location": heads[0]!["pdfPage"] = 354; break;
            case "wrong-source": index["source"]!["sha256"] = new string('0', 64); break;
            case "wrong-map-source": map["sourceSha256"] = new string('0', 64); break;
            case "missing-map": rows.RemoveAt(0); break;
            case "duplicate-map": rows.Add(row.DeepClone()); break;
            case "invented-map": row["standardId"] = "A:353:1:invented"; break;
            case "invented-target": row["targets"]![0] = "CSharp.CompilationUnits/rule:Invented"; break;
            case "duplicate-target": row["targets"]!.AsArray().Add(row["targets"]![0]!.DeepClone()); break;
            case "invented-policy": row["policies"]!.AsArray().Add("invented"); break;
            case "missing-owner": row["targets"] = new JsonArray(); row["policies"] = new JsonArray(); break;
            case "missing-notes": row["notes"] = ""; break;
            case "invented-strategy": row["mapping"] = "best-guess"; break;
            case "invented-status": row["status"] = "good-enough"; break;
            case "missing-requirement": map["reviewRequirements"]!.AsArray().RemoveAt(0); break;
            case "missing-review": row["reviews"]!.AsObject().Remove("normative-prose"); break;
            case "invented-review-status": row["reviews"]!["normative-prose"] = "skipped"; break;
            case "unreviewed-promotion": row["status"] = "verified"; row["reviews"]!["normative-prose"]="pending"; break;
            case "schema-override": map["profileComplete"] = true; break;
        }
        var report = CSharp1StandardCrosswalkAudit.Evaluate(index, map, Resource("csharp1-coverage-gate.json"));
        Assert.Contains(report.Errors, e => e.StartsWith(expected, StringComparison.Ordinal));
    }

    [Fact]
    public void Promoting_the_old_catalog_cannot_bypass_unclosed_standard_rows()
    {
        var ledger = Resource("csharp1-coverage-gate.json"); var inventory = CSharp1CoverageGateAudit.Inventory();
        foreach (string group in new[] { "modules", "policies", "obligations" })
            foreach (var item in ledger[group]!.AsArray()) item!["status"] = "verified";
        foreach (var feature in inventory["features"]!.AsArray()) { feature!["status"] = "verified"; feature["reviewed"] = true; }
        var map=Resource("csharp1-standard-crosswalk.json");
        map["productions"]![0]!["status"]="observed";
        map["productions"]![0]!["reviews"]!["normative-prose"]="pending";
        var report = CSharp1CoverageGateAudit.Evaluate(ledger, inventory,crosswalk:map);
        Assert.Empty(report.Errors); Assert.False(report.Complete);
        Assert.Single(report.Blockers); Assert.StartsWith("Crosswalk: 1 ", report.Blockers[0]);
    }

    [Fact]
    public void Unresolved_standard_syntax_is_a_blocker_even_when_existing_catalog_is_valid()
    {
        var map = Resource("csharp1-standard-crosswalk.json"); var row = map["productions"]![0]!;
        row["mapping"] = "unresolved"; row["status"] = "unresolved";
        row["targets"] = new JsonArray(); row["policies"] = new JsonArray();
        row["notes"] = "Missing owner discovered during normative review; keep the source production visible.";
        var report = CSharp1CoverageGateAudit.Evaluate(Resource("csharp1-coverage-gate.json"), crosswalk: map);
        Assert.Empty(report.Errors); Assert.False(report.Complete);
        Assert.Contains(report.Blockers, b => b.StartsWith("Crosswalk:", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("csharp1-standard-index.json")]
    [InlineData("csharp1-standard-crosswalk.json")]
    public void Coverage_identity_binds_each_standard_input(string suffix)
    {
        var assembly = typeof(FeatureCatalog).Assembly;
        string[] fixedNames = ["coverage.json", "dependencies.json", "composition.json", "reference-policy.json", "csharp1-coverage-gate.json", "csharp1-standard-index.json", "csharp1-standard-crosswalk.json"];
        var inputs = assembly.GetManifestResourceNames().Where(n => fixedNames.Any(s => n == "CSNitrogen." + s) || n.StartsWith("CSNitrogen.grammar.", StringComparison.Ordinal) || n.StartsWith("CSNitrogen.source.", StringComparison.Ordinal))
            .Select(n => { using var stream = assembly.GetManifestResourceStream(n)!; using var bytes = new MemoryStream(); stream.CopyTo(bytes); return new KeyValuePair<string, byte[]>(n, bytes.ToArray()); }).ToArray();
        Assert.Equal(FeatureCatalog.CoverageIdentity, CoverageHash.Compute(inputs));
        Assert.NotEqual(FeatureCatalog.CoverageIdentity, CoverageHash.Compute(inputs.Where(p => p.Key != "CSNitrogen." + suffix)));
        Assert.NotEqual(FeatureCatalog.CoverageIdentity, CoverageHash.Compute(inputs.Select(p => p.Key == "CSNitrogen." + suffix ? new(p.Key, p.Value.Concat(Encoding.UTF8.GetBytes(" ")).ToArray()) : p)));
    }

    [Fact]
    public void Whole_pending_corpus_review_closes_all_source_occurrences_without_promoting_runtime_profiles()
    {
        var rows=Resource("csharp1-standard-crosswalk.json")["productions"]!.AsArray();
        Assert.Equal(362,rows.Count);
        Assert.All(rows,r=>
        {
            Assert.Equal("verified",r!["status"]!.GetValue<string>());
            Assert.All(r["reviews"]!.AsObject(),p=>Assert.Equal("verified",p.Value!.GetValue<string>()));
            Assert.NotEmpty(r["notes"]!.GetValue<string>());
        });
        var report=CSharp1CoverageGateAudit.Evaluate(Resource("csharp1-coverage-gate.json"));
        Assert.Empty(report.Errors);Assert.False(report.Complete);
        Assert.Single(report.Blockers);Assert.Contains("runtime-profile-contract",report.Blockers[0]);
        Assert.False(ProfileResolver.Resolve("CSharp1").Profile!.Complete);
    }

    [Fact]
    public void Parent_type_review_remains_intact_after_whole_source_review()
    {
        string[] names = ["value-type", "struct-type", "simple-type", "numeric-type", "integral-type", "floating-point-type", "enum-type", "reference-type", "class-type", "interface-type", "delegate-type", "array-type", "non-array-type", "return-type"];
        var expected = Resource("csharp1-standard-index.json")["productions"]!.AsArray().Where(p => names.Contains(p!["name"]!.GetValue<string>(), StringComparer.Ordinal)).Select(p => p!["id"]!.GetValue<string>()).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(16, expected.Count);
        var rows = Resource("csharp1-standard-crosswalk.json")["productions"]!.AsArray();
        Assert.All(rows.Where(p => expected.Contains(p!["standardId"]!.GetValue<string>())), p => Assert.Equal("verified", p!["status"]!.GetValue<string>()));
        Assert.All(rows.Where(p => p!["standardId"]!.GetValue<string>().EndsWith(":cast-expression", StringComparison.Ordinal)), p => Assert.Equal("verified", p!["status"]!.GetValue<string>()));
    }

    [Fact]
    public void Root_pointer_review_closes_exactly_the_four_normalized_source_occurrences()
    {
        string[] reviewed = ["A:360:5:type", "A:377:39:type", "A:377:43:pointer-type", "A:377:46:unmanaged-type"];
        var rows = Resource("csharp1-standard-crosswalk.json")["productions"]!.AsArray();
        Assert.All(reviewed, id => Assert.Equal("verified", rows.Single(p => p!["standardId"]!.GetValue<string>() == id)!["status"]!.GetValue<string>()));
    }
}
