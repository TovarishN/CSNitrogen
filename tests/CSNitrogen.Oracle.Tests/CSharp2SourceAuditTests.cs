using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen.Grammar;

namespace CSNitrogen.Oracle.Tests;

/// <summary>Independent structural audit; fixture references and review declarations are not execution proofs.</summary>
public class CSharp2SourceAuditTests
{
    private const string SourceHash = "de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652";
    // Independently extracted fourth-edition IDs, bounds and alternatives, including all twelve restored one-of headings.
    private const string BodyIdentity = "458d72ef6d434514d6fa2ef9a977221f0c7fc1f42d86f01f90d174e3f6ee92b8";
    private static readonly string[] Reviews = ["normative-prose", "positive-negative", "ambiguity-context", "spans-diagnostics"];
    private static readonly string[] Witnesses = ["positive", "negative", "ambiguity", "span"];

    private static JsonObject Resource(string suffix)
    {
        using var stream = typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen." + suffix)
            ?? throw new InvalidDataException("Missing audit resource " + suffix);
        return JsonNode.Parse(stream)!.AsObject();
    }

    [Fact]
    public void Fourth_edition_source_owners_factory_and_witness_references_are_closed()
    {
        var errors = Evaluate(Resource("csharp2-standard-index.json"), Resource("csharp2-standard-crosswalk.json"), Resource("csharp2-coverage-gate.json"));
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Repeated_source_occurrences_and_fixed_buffer_supplement_are_independent()
    {
        var headings = Resource("csharp2-standard-index.json")["productions"]!.AsArray();
        Assert.Equal(new[] { "A:478:17:embedded-statement", "A:493:39:embedded-statement", "A:494:29:embedded-statement" },
            headings.Where(h => Text(h!, "name") == "embedded-statement").Select(h => Text(h!, "id")));
        Assert.Equal(2, headings.Count(h => Text(h!, "name") == "type"));
        Assert.Equal(2, headings.Count(h => Text(h!, "name") == "variable-initializer"));
        Assert.Contains(headings, h => Text(h!, "id") == "A:486:43:overloadable-unary-operator");
        Assert.Contains(headings, h => Text(h!, "id") == "A:487:2:overloadable-binary-operator");
        Assert.DoesNotContain(headings, h => Text(h!, "name").Contains("fixed-buffer", StringComparison.Ordinal));
        var supplement = Assert.Single(Resource("csharp2-standard-crosswalk.json")["supplementCandidates"]!.AsArray());
        Assert.Equal("MSVS2005:959:Fixed-Size-Buffers", Text(supplement!, "standardId"));
    }

    [Theory]
    [InlineData("missing-row", "SourceSet")]
    [InlineData("duplicate-row", "SourceSet")]
    [InlineData("empty-body", "IndexBody")]
    [InlineData("changed-body", "IndexIdentity")]
    [InlineData("bad-bound", "IndexBody")]
    [InlineData("wrong-source", "SourceIdentity")]
    [InlineData("wrong-review-id", "SourceReview")]
    [InlineData("pending-review", "ReviewClosure")]
    [InlineData("unknown-target", "Target")]
    [InlineData("duplicate-target", "Target")]
    [InlineData("unknown-policy", "Policy")]
    [InlineData("missing-module", "Factory")]
    [InlineData("invented-production", "ModuleProductions")]
    [InlineData("missing-witness", "Witness")]
    [InlineData("invented-method", "Witness")]
    [InlineData("duplicate-witness", "Witness")]
    public void Metadata_mutations_fail_closed(string mutation, string expected)
    {
        var index = Resource("csharp2-standard-index.json");
        var map = Resource("csharp2-standard-crosswalk.json");
        var gate = Resource("csharp2-coverage-gate.json");
        var rows = map["productions"]!.AsArray();
        var row = rows[0]!;
        switch (mutation)
        {
            case "missing-row": rows.RemoveAt(0); break;
            case "duplicate-row": rows.Add(row.DeepClone()); break;
            case "empty-body": index["productions"]![0]!["bodyLines"] = new JsonArray(); break;
            case "changed-body": index["productions"]![0]!["bodyLines"]![0] = "invented-alternative"; break;
            case "bad-bound": index["productions"]![0]!["endLine"] = 0; break;
            case "wrong-source": map["sourceSha256"] = new string('0', 64); break;
            case "wrong-review-id": row["sourceReview"]!["occurrenceId"] = "A:465:1:invented"; break;
            case "pending-review": row["reviews"]!["normative-prose"] = "pending"; break;
            case "unknown-target": row["targets"]!.AsArray().Add("CSharp.CompilationUnits/rule:Invented"); break;
            case "duplicate-target": row["targets"]!.AsArray().Add(row["targets"]![0]!.DeepClone()); break;
            case "unknown-policy": row["policies"]!.AsArray().Add("invented-policy"); break;
            case "missing-module": gate["factory"]!["modules"]!.AsArray().RemoveAt(0); break;
            case "invented-production": gate["modules"]![0]!["productions"]!.AsArray().Add("rule:Invented"); break;
            case "missing-witness": row["witnesses"]!.AsObject().Remove("positive"); break;
            case "invented-method": row["witnesses"]!["positive"]![0]!["method"] = "InventedTestMethod"; break;
            case "duplicate-witness": row["witnesses"]!["positive"]!.AsArray().Add(row["witnesses"]!["positive"]![0]!.DeepClone()); break;
        }
        Assert.Contains(Evaluate(index, map, gate), e => e.StartsWith(expected + ":", StringComparison.Ordinal));
    }

    private static string Text(JsonNode node, string field) => node[field]?.GetValue<string>() ?? "";
    private static string[] Strings(JsonNode? node) => node?.AsArray().Select(n => n!.GetValue<string>()).ToArray() ?? [];

    internal static IReadOnlyList<string> Evaluate(JsonObject index, JsonObject map, JsonObject gate)
    {
        var errors = new List<string>();
        void Error(string code, string detail) => errors.Add(code + ": " + detail);
        void Exact(string code, IEnumerable<string> expected, IEnumerable<string> actual)
        {
            var a = expected.Order(StringComparer.Ordinal).ToArray();
            var b = actual.Order(StringComparer.Ordinal).ToArray();
            if (!a.SequenceEqual(b) || b.Distinct(StringComparer.Ordinal).Count() != b.Length)
                Error(code, "missing, duplicate or unexpected entries");
        }
        try
        {
            if (index["schemaVersion"]!.GetValue<int>() != 1 || map["schemaVersion"]!.GetValue<int>() != 1 || gate["schemaVersion"]!.GetValue<int>() != 1 ||
                Text(index, "profile") != "CSharp2" || Text(map, "profile") != "CSharp2" || Text(gate, "profile") != "CSharp2")
                Error("Schema", "expected schema1 CSharp2 inputs");
            if (Text(index, "sourceSha256") != SourceHash || Text(map, "sourceSha256") != SourceHash)
                Error("SourceIdentity", "different pinned fourth-edition PDF");
            var headings = index["productions"]!.AsArray();
            var rows = map["productions"]!.AsArray();
            if (headings.Count != 409 || headings.Select(h => Text(h!, "id")).Distinct(StringComparer.Ordinal).Count() != 409)
                Error("SourceSet", "expected409 unique source occurrences");
            Exact("SourceSet", headings.Select(h => Text(h!, "id")), rows.Select(r => Text(r!, "standardId")));
            Exact("ReviewRequirements", Reviews, Strings(map["reviewRequirements"]));
            var canonical = new StringBuilder();
            foreach (var heading in headings)
            {
                int page = heading!["pdfPage"]!.GetValue<int>(), line = heading["line"]!.GetValue<int>();
                int endPage = heading["endPdfPage"]!.GetValue<int>(), endLine = heading["endLine"]!.GetValue<int>();
                string id = Text(heading, "id"), name = Text(heading, "name");
                var body = Strings(heading["bodyLines"]);
                if (id != $"A:{page}:{line}:{name}" || page is < 465 or > 494 || line is < 1 or > 100 ||
                    endPage < page || endPage > 494 || endLine is < 1 or > 100 || endPage == page && endLine <= line ||
                    body.Length == 0 || body.Any(string.IsNullOrWhiteSpace) || string.IsNullOrWhiteSpace(Text(heading, "annexSection")))
                    Error("IndexBody", id);
                canonical.Append(id).Append('|').Append(endPage).Append(':').Append(endLine).Append('|').AppendJoin('\n', body).Append('\n');
            }
            if (Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()))) != BodyIdentity)
                Error("IndexIdentity", "independent source IDs/bounds/alternatives drifted");

            var assembly = typeof(FeatureCatalog).Assembly;
            string Read(string resource)
            {
                using var stream = assembly.GetManifestResourceStream(resource) ?? throw new InvalidDataException("Missing " + resource);
                using var reader = new StreamReader(stream); return reader.ReadToEnd();
            }
            var config = JsonNode.Parse(Read("CSNitrogen.composition.json"))!;
            var configured = config["languages"]!.AsArray().Single(l => Text(l!, "name") == "CSharp2")!;
            var paths = Strings(configured["grammars"]);
            var modules = gate["modules"]!.AsArray();
            var factory = gate["factory"]!;
            if (Text(factory, "name") != "CSharp2" || Text(factory, "start") != "CompilationUnit" || Text(factory, "rootModule") != "CSharp.CompilationUnits" ||
                Text(configured, "start") != "CSharp.CompilationUnits.CompilationUnit" || CSharp2Composition.Start.Name != "CompilationUnit")
                Error("Factory", "wrong selected factory/start/root identity");
            Exact("Factory", paths, Strings(factory["grammars"]));
            Exact("Factory", paths, modules.Select(m => Text(m!, "grammar")));
            var resources = assembly.GetManifestResourceNames().Where(n => n.StartsWith("CSNitrogen.grammar.", StringComparison.Ordinal))
                .ToDictionary(n => "Grammars/" + n["CSNitrogen.grammar.".Length..].Replace('\\', '/'), StringComparer.Ordinal);
            var actualTargets = new HashSet<string>(StringComparer.Ordinal);
            var actualModules = new List<string>();
            foreach (var module in modules)
            {
                string path = Text(module!, "grammar");
                if (!resources.TryGetValue(path, out string? resource)) { Error("Factory", "unknown grammar " + path); continue; }
                var parsed = GrammarParser.Parse(Read(resource));
                if (!parsed.Success || parsed.Diagnostics.Count != 0) { Error("Grammar", path); continue; }
                var declared = parsed.File!.Modules.Single();
                actualModules.Add(declared.Name);
                if (Text(module!, "module") != declared.Name) Error("Factory", "wrong module identity " + path);
                var productions = declared.Rules.Select(r => "rule:" + r.Name)
                    .Concat(declared.Rules.OfType<ExtensibleRule>().SelectMany(r => r.Alternatives.Select(a => "alternative:" + r.Name + "." + a.Name)))
                    .Concat(declared.Extends.SelectMany(e => e.Alternatives.Select(a => "extension:" + e.Target + "." + a.Name))).ToArray();
                Exact("ModuleProductions", productions, Strings(module!["productions"]));
                foreach (string production in productions) actualTargets.Add(declared.Name + "/" + production);
            }
            Exact("Factory", actualModules, Strings(factory["modules"]));
            Exact("Factory", CSharp2Composition.CreateBuilder().Build().Modules.Select(m => m.Name), Strings(factory["modules"]));
            var policyGroups = gate["policyGroups"]!.AsObject().Select(p => p.Key).ToHashSet(StringComparer.Ordinal);
            foreach (string policy in policyGroups)
                if (!FeatureCatalog.KnownPolicies.Contains(policy, StringComparer.Ordinal)) Error("Policy", "unregistered group " + policy);

            // Reuse fixture parsing per path across409 occurrences, but never infer execution from method existence.
            var fixtureMethods = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            void Evidence(JsonNode row)
            {
                if (row["witnesses"] is not JsonObject witnesses) { Error("Witness", "missing witness object " + Text(row, "standardId")); return; }
                Exact("Witness", Witnesses, witnesses.Select(w => w.Key));
                foreach (string kind in Witnesses)
                {
                    if (witnesses[kind] is not JsonArray fixtures || fixtures.Count == 0) { Error("Witness", "missing " + kind); continue; }
                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var fixture in fixtures)
                    {
                        if (fixture is null) { Error("Witness", "null fixture"); continue; }
                        string path = Text(fixture, "path"), method = Text(fixture, "method");
                        Exact("Witness", ["path", "method"], fixture.AsObject().Select(p => p.Key));
                        if (!seen.Add(path + "::" + method)) Error("Witness", "duplicate " + path + "::" + method);
                        if (!path.StartsWith("tests/", StringComparison.Ordinal) || path.Contains("..", StringComparison.Ordinal) || path.Contains('\\') || Path.IsPathRooted(path) || string.IsNullOrWhiteSpace(method))
                        { Error("Witness", "invalid reference " + path); continue; }
                        if (!fixtureMethods.TryGetValue(path, out var methods))
                        {
                            string fullPath = Path.Combine(AppContext.BaseDirectory, "CoverageInputs", path);
                            if (!File.Exists(fullPath)) { Error("Witness", "missing file " + path); continue; }
                            var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), new CSharpParseOptions(LanguageVersion.CSharp14));
                            methods = syntax.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
                                .Where(m => m.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Split('.').Last() is "Fact" or "Theory" or "FactAttribute" or "TheoryAttribute"))
                                .Select(m => m.Identifier.ValueText).ToHashSet(StringComparer.Ordinal);
                            fixtureMethods.Add(path, methods);
                        }
                        if (!methods.Contains(method)) Error("Witness", "missing test " + path + "::" + method);
                    }
                }
            }
            void Mapping(JsonNode row, bool supplement)
            {
                string id = Text(row, "standardId");
                var targets = Strings(row["targets"]); var policies = Strings(row["policies"]);
                if (targets.Distinct(StringComparer.Ordinal).Count() != targets.Length || targets.Any(t => !actualTargets.Contains(t))) Error("Target", id);
                if (policies.Distinct(StringComparer.Ordinal).Count() != policies.Length || policies.Any(p => !policyGroups.Contains(p))) Error("Policy", id);
                if (targets.Length == 0 && policies.Length == 0) Error("Target", "missing owner " + id);
                string mapping = Text(row, "mapping");
                if (mapping is not ("direct" or "inlined" or "shared-syntax" or "policy" or "semantic-wrapper") || mapping == "policy" && policies.Length == 0)
                    Error("Mapping", id);
                if (string.IsNullOrWhiteSpace(Text(row, "notes"))) Error("Mapping", "missing notes " + id);
                if (row["reviews"] is not JsonObject reviews) Error("ReviewClosure", "missing reviews " + id);
                else
                {
                    Exact("ReviewClosure", Reviews, reviews.Select(r => r.Key));
                    if (reviews.Any(r => r.Value?.GetValue<string>() != "verified")) Error("ReviewClosure", id);
                }
                if (Text(row, "status") != "verified") Error("ReviewClosure", id);
                var review = row["sourceReview"];
                if (review is null || Text(review, "domain") is not ("lexical" or "expression" or "statement" or "declaration") ||
                    !supplement && (Text(review, "occurrenceId") != id || Text(review, "sourceSha256") != SourceHash)) Error("SourceReview", id);
                Evidence(row);
            }
            foreach (var row in rows) Mapping(row!, false);
            var supplements = map["supplementCandidates"]!.AsArray();
            Exact("Supplement", ["MSVS2005:959:Fixed-Size-Buffers"], supplements.Select(s => Text(s!, "standardId")));
            foreach (var supplement in supplements) Mapping(supplement!, true);
        }
        catch (Exception exception) { Error("Schema", exception.Message); }
        return errors.AsReadOnly();
    }
}
