using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CSNitrogen.Profiles;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen.Grammar;

namespace CSNitrogen.Oracle.Tests;

internal static class CSharp1CoverageGateAudit
{
    private sealed record Fixture(string Path, string Method);
    private sealed record Module(string Grammar, [property: JsonPropertyName("module")] string ModuleName, string[] Productions, string Status, string Reference, Fixture[] Positive, Fixture[] Negative, string Scope);
    private sealed record Policy(string Source, string[] Codes, string Status, string Reference, Fixture[] Positive, Fixture[] Negative);
    private sealed record Exclusion(string Id, string Reason, string Reference);
    private sealed record Obligation(string Id, string Status, string Reason, string Reference);
    private sealed record Ledger(int SchemaVersion, string Profile, Module[] Modules, Policy[] Policies, Dictionary<string, string[]> PolicyGroups, Exclusion[] SemanticExclusions, Obligation[] Obligations);
    internal sealed record Report(int Modules, int Productions, int PolicySources, int PolicyCodes, int SyntaxFeatures, IReadOnlyList<string> Errors, IReadOnlyList<string> Blockers)
    { internal bool Complete => Errors.Count == 0 && Blockers.Count == 0; }
    private static Assembly Assembly => typeof(FeatureCatalog).Assembly;
    private static byte[] Read(string resource)
    { using var stream = Assembly.GetManifestResourceStream(resource) ?? throw new InvalidDataException("Missing resource " + resource); using var bytes = new MemoryStream(); stream.CopyTo(bytes); return bytes.ToArray(); }
    internal static JsonObject Inventory() => JsonNode.Parse(Read("CSNitrogen.coverage.json"))!.AsObject();

    internal static Report Evaluate(JsonObject input, JsonObject? inventory = null, JsonObject? crosswalk = null)
    {
        var errors = new List<string>(); var blockers = new List<string>(); int moduleCount = 0, productionCount = 0, policyCount = 0, codeCount = 0, featureCount = 0;
        try
        {
            var standard = CSharp1StandardCrosswalkAudit.Evaluate(CSharp1StandardCrosswalkAudit.Resource("csharp1-standard-index.json"), crosswalk ?? CSharp1StandardCrosswalkAudit.Resource("csharp1-standard-crosswalk.json"), input);
            errors.AddRange(standard.Errors); blockers.AddRange(standard.Blockers);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
            var ledger = input.Deserialize<Ledger>(options) ?? throw new InvalidDataException("Empty ledger");
            if (ledger.SchemaVersion != 1 || ledger.Profile != "CSharp1") errors.Add("Schema: expected schema 1 and CSharp1");
            void Exact(string label, IEnumerable<string> expected, IEnumerable<string> observed)
            {
                var a = expected.Order(StringComparer.Ordinal).ToArray(); var b = observed.Order(StringComparer.Ordinal).ToArray();
                if (!a.SequenceEqual(b) || b.Distinct(StringComparer.Ordinal).Count() != b.Length) errors.Add(label + ": missing, duplicate or unexpected entries");
            }
            void Reference(string value, string label)
            { if (!Uri.TryCreate(value, UriKind.Absolute, out var url) || url.Scheme != "https") errors.Add("Reference: " + label); }
            void Status(string status, string label)
            {
                if (status is not ("observed" or "pending" or "verified")) errors.Add("Status: " + label);
                if (status != "verified") blockers.Add(label + ": " + status);
            }
            void Evidence(Fixture[] fixtures, string label)
            {
                if (fixtures.Length == 0) errors.Add("Evidence: " + label + " has no fixture");
                foreach (var fixture in fixtures)
                {
                    if (!fixture.Path.StartsWith("tests/", StringComparison.Ordinal) || fixture.Path.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(fixture.Path))
                    { errors.Add("Evidence: invalid path " + fixture.Path); continue; }
                    var path = Path.Combine(AppContext.BaseDirectory, "CoverageInputs", fixture.Path);
                    if (!File.Exists(path)) { errors.Add("Evidence: missing file " + fixture.Path); continue; }
                    var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(path), new CSharpParseOptions(LanguageVersion.CSharp14));
                    bool exists = syntax.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.ValueText == fixture.Method && m.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Split('.').Last() is "Fact" or "Theory" or "FactAttribute" or "TheoryAttribute"));
                    if (!exists) errors.Add("Evidence: missing test " + fixture.Path + "::" + fixture.Method);
                }
            }
            var resources = Assembly.GetManifestResourceNames();
            var grammarResources = resources.Where(n => n.StartsWith("CSNitrogen.grammar.", StringComparison.Ordinal)).ToDictionary(n => "Grammars/" + n["CSNitrogen.grammar.".Length..].Replace('\\', '/'), n => n, StringComparer.Ordinal);
            string GrammarResource(string path) => grammarResources.TryGetValue(path, out var resource) ? resource : throw new InvalidDataException("Unknown grammar path " + path);
            var config = JsonNode.Parse(Read("CSNitrogen.composition.json"))!.AsObject();
            var profile = config["languages"]!.AsArray().Single(p => p!["name"]!.GetValue<string>() == "CSharp1")!;
            var grammarPaths = profile["grammars"]!.AsArray().Select(p => p!.GetValue<string>()).ToArray();
            Exact("ModuleSet", grammarPaths, ledger.Modules.Select(m => m.Grammar));
            foreach (var module in ledger.Modules)
            {
                var parsed = GrammarParser.Parse(System.Text.Encoding.UTF8.GetString(Read(GrammarResource(module.Grammar))));
                if (!parsed.Success || parsed.Diagnostics.Count != 0) { errors.Add("Grammar: " + module.Grammar); continue; }
                var declared = parsed.File!.Modules.Single(); moduleCount++;
                if (declared.Name != module.ModuleName) errors.Add("ModuleSet: wrong module name " + module.ModuleName);
                var productions = declared.Rules.Select(r => "rule:" + r.Name)
                    .Concat(declared.Rules.OfType<ExtensibleRule>().SelectMany(r => r.Alternatives.Select(a => "alternative:" + r.Name + "." + a.Name)))
                    .Concat(declared.Extends.SelectMany(e => e.Alternatives.Select(a => "extension:" + e.Target + "." + a.Name))).ToArray();
                productionCount += productions.Length; Exact("Productions:" + module.ModuleName, productions, module.Productions);
                Reference(module.Reference, module.ModuleName); Status(module.Status, "Module " + module.ModuleName);
                if (string.IsNullOrWhiteSpace(module.Scope)) errors.Add("Schema: missing module scope " + module.ModuleName);
                Evidence(module.Positive, module.ModuleName + " positive"); Evidence(module.Negative, module.ModuleName + " negative");
            }
            var sourceResources = resources.Where(n => n.StartsWith("CSNitrogen.source.", StringComparison.Ordinal)).ToArray();
            string SourcePath(string resource)
            {
                string path = resource["CSNitrogen.source.".Length..].Replace('\\', '/');
                foreach (string dir in new[] { "Parsing", "Preprocessing", "Profiles" })
                {
                    if (path.StartsWith(dir + "/", StringComparison.Ordinal)) return path;
                    if (path.StartsWith(dir + ".", StringComparison.Ordinal)) return dir + "/" + path[(dir.Length + 1)..];
                }
                throw new InvalidDataException("Unmapped source resource " + resource);
            }
            var sourcesByPath = sourceResources.ToDictionary(SourcePath, n => n, StringComparer.Ordinal);
            var codesBySource = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (var resource in sourceResources)
            {
                var syntax = CSharpSyntaxTree.ParseText(System.Text.Encoding.UTF8.GetString(Read(resource)), new CSharpParseOptions(LanguageVersion.CSharp14));
                var codes = syntax.GetRoot().DescendantNodes().OfType<LiteralExpressionSyntax>().Where(n => n.IsKind(SyntaxKind.StringLiteralExpression)).Select(n => n.Token.ValueText)
                    .Where(s => s.Length == 7 && s.StartsWith("CSN", StringComparison.Ordinal) && s[3..].All(char.IsAsciiDigit)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
                if (codes.Length == 0) continue;
                codesBySource.Add(SourcePath(resource), codes);
            }
            policyCount = codesBySource.Count; codeCount = codesBySource.Values.Sum(c => c.Length);
            Exact("PolicySet", codesBySource.Keys, ledger.Policies.Select(p => p.Source));
            foreach (var policy in ledger.Policies)
            {
                if (!codesBySource.TryGetValue(policy.Source, out var actual)) { errors.Add("PolicySet: " + policy.Source); continue; }
                Exact("PolicyCodes:" + policy.Source, actual, policy.Codes); Status(policy.Status, "Policy " + policy.Source); Reference(policy.Reference, policy.Source);
                Evidence(policy.Positive, policy.Source + " positive"); Evidence(policy.Negative, policy.Source + " negative");
            }
            foreach (var group in ledger.PolicyGroups)
            {
                if (group.Value.Length == 0) errors.Add("PolicyGroups: empty group " + group.Key);
                foreach (string source in group.Value) if (!sourcesByPath.ContainsKey(source)) errors.Add("PolicyGroups: unknown source " + source);
            }
            var grouped = ledger.PolicyGroups.Values.SelectMany(s => s).ToHashSet(StringComparer.Ordinal);
            foreach (string source in codesBySource.Keys) if (!grouped.Contains(source)) errors.Add("PolicyGroups: source is not grouped " + source);
            var features = (inventory ?? Inventory())["features"]!.AsArray(); var exclusions = new List<string>(); var policyGroups = new HashSet<string>(StringComparer.Ordinal);
            foreach (var feature in features)
            {
                if (!Version.TryParse(feature!["introduced"]!.GetValue<string>(), out var version)) { errors.Add("Inventory: invalid version"); continue; }
                if (version > new Version(1, 0)) continue;
                string id = feature["id"]!.GetValue<string>(); string classification = feature["classification"]!.GetValue<string>();
                string featureStatus = feature["status"]!.GetValue<string>();
                if (featureStatus is not ("unimplemented" or "implemented" or "verified" or "blocked")) errors.Add("Status: " + id);
                if (classification == "semantic-or-tooling")
                { if (!feature["reviewed"]!.GetValue<bool>()) errors.Add("SemanticExclusions: classification not reviewed " + id); exclusions.Add(id); continue; }
                if (classification != "syntax-or-policy") { errors.Add("Classification: " + id); continue; }
                featureCount++;
                var grammars = feature["grammars"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray();
                var policies = feature["policies"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray();
                if (grammars.Length == 0 && policies.Length == 0) errors.Add("Inventory: missing grammar/policy mapping " + id);
                foreach (string path in grammars) _ = GrammarResource(path);
                foreach (string policy in policies) { policyGroups.Add(policy); if (!ledger.PolicyGroups.ContainsKey(policy)) errors.Add("PolicyGroups: unknown " + policy); }
                var fixturePaths = feature["fixtures"]!.AsArray().Select(n => n!.GetValue<string>()).ToArray();
                if (fixturePaths.Length == 0) errors.Add("InventoryEvidence: no fixtures " + id);
                foreach (string path in fixturePaths) if (!path.StartsWith("tests/", StringComparison.Ordinal) || path.Contains("..", StringComparison.Ordinal) || !File.Exists(Path.Combine(AppContext.BaseDirectory, "CoverageInputs", path))) errors.Add("InventoryEvidence: missing fixture " + path);
                if (!feature["reviewed"]!.GetValue<bool>() || feature["status"]!.GetValue<string>() != "verified") blockers.Add("Feature " + id + ": not verified");
            }
            Exact("PolicyGroups", policyGroups, ledger.PolicyGroups.Keys);
            Exact("SemanticExclusions", exclusions, ledger.SemanticExclusions.Select(e => e.Id));
            foreach (var exclusion in ledger.SemanticExclusions) { if (string.IsNullOrWhiteSpace(exclusion.Reason)) errors.Add("SemanticExclusions: missing reason"); Reference(exclusion.Reference, exclusion.Id); }
            Exact("Obligations", new[] { "historical-standard-crosswalk", "independent-context-review" }, ledger.Obligations.Select(o => o.Id));
            foreach (var obligation in ledger.Obligations) { Status(obligation.Status, "Obligation " + obligation.Id); if (string.IsNullOrWhiteSpace(obligation.Reason)) errors.Add("Obligations: missing reason"); Reference(obligation.Reference, obligation.Id); }
        }
        catch (Exception error) { errors.Add("Schema: " + error.Message); }
        return new(moduleCount, productionCount, policyCount, codeCount, featureCount, errors, blockers);
    }
}
