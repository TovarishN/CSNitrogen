using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CSNitrogen.Profiles;

namespace CSNitrogen.Oracle.Tests;

/// <summary>Structural traceability audit. Review statuses are human declarations, not proofs.</summary>
internal static class CSharp1StandardCrosswalkAudit
{
    private sealed record Source(string Title, string Url, string Sha256);
    private sealed record Heading(string Id, string Name, int PdfPage, int PrintedPage, int Line, string Grammar);
    private sealed record Index(int SchemaVersion, Source Source, string Scope, Heading[] Productions);
    private sealed record Mapping(string StandardId, string[] Targets, string[] Policies, [property: JsonPropertyName("mapping")] string MappingKind, string Status, string Notes, Dictionary<string, string> Reviews);
    private sealed record Crosswalk(int SchemaVersion, string Profile, string SourceSha256, string[] ReviewRequirements, Mapping[] Productions);
    internal sealed record Report(int Occurrences, int DistinctNames, int Pending, IReadOnlyList<string> Errors, IReadOnlyList<string> Blockers);
    private static readonly string[] Requirements = ["normative-prose", "positive-negative", "ambiguity-context", "spans-diagnostics"];
    internal static JsonObject Resource(string name)
    {
        using var stream = typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen." + name) ?? throw new InvalidDataException("Missing standard resource " + name);
        return JsonNode.Parse(stream)!.AsObject();
    }

    internal static Report Evaluate(JsonObject indexInput, JsonObject mapInput, JsonObject gate)
    {
        var errors = new List<string>(); var blockers = new List<string>(); int count = 0, distinct = 0, pending = 0;
        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectRequiredConstructorParameters = true };
            var index = indexInput.Deserialize<Index>(options)!;
            var map = mapInput.Deserialize<Crosswalk>(options)!;
            void Error(string code, string detail) => errors.Add("Crosswalk" + code + ": " + detail);
            void Exact(string code, IEnumerable<string> expected, IEnumerable<string> actual)
            {
                var a = expected.Order(StringComparer.Ordinal).ToArray(); var b = actual.Order(StringComparer.Ordinal).ToArray();
                if (!a.SequenceEqual(b) || b.Distinct(StringComparer.Ordinal).Count() != b.Length) Error(code, "missing, duplicate or unexpected entries");
            }
            if (index.SchemaVersion != 1 || map.SchemaVersion != 1 || map.Profile != "CSharp1") Error("Schema", "expected schema 1 and CSharp1");
            var reference = Resource("reference-policy.json")["primary"]!;
            if (index.Source.Title != reference["title"]!.GetValue<string>() || index.Source.Url != reference["url"]!.GetValue<string>() || index.Source.Sha256 != reference["sha256"]!.GetValue<string>() || map.SourceSha256 != index.Source.Sha256)
                Error("Source", "source differs from the pinned historical reference");
            if (string.IsNullOrWhiteSpace(index.Scope)) Error("Source", "missing scope");
            count = index.Productions.Length; distinct = index.Productions.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count();
            if (count != 362 || distinct != 346) Error("Index", "unexpected first-edition heading counts");
            // Golden heading identity from independent pinned-PDF extraction; it is
            // not derived from implemented grammar rules or crosswalk rows.
            string canonical = string.Concat(index.Productions.Select(p => p.Id + "|" + p.Grammar + "\n"));
            if (Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))) != "f898938e8e39e062521ff9d9fd913e122e86e44864aab16b3108b53b4b82fb9d")
                Error("Index", "heading identity drift; re-extract and review the pinned PDF");
            foreach (var heading in index.Productions)
            {
                if (heading.Id != $"A:{heading.PdfPage}:{heading.Line}:{heading.Name}" || heading.PdfPage is < 353 or > 378 || heading.PrintedPage != heading.PdfPage - 14 || heading.Line is < 1 or > 50 || heading.Grammar is not ("lexical" or "syntactic" or "unsafe"))
                    Error("Index", "invalid source location " + heading.Id);
            }
            Exact("Set", index.Productions.Select(p => p.Id), map.Productions.Select(p => p.StandardId));
            Exact("Requirements", Requirements, map.ReviewRequirements);
            var targets = gate["modules"]!.AsArray().SelectMany(m => m!["productions"]!.AsArray().Select(p => m["module"]!.GetValue<string>() + "/" + p!.GetValue<string>())).ToHashSet(StringComparer.Ordinal);
            var policies = gate["policyGroups"]!.AsObject().Select(p => p.Key).ToHashSet(StringComparer.Ordinal);
            foreach (var row in map.Productions)
            {
                if (row.Status is not ("observed" or "unresolved" or "verified")) Error("Status", row.StandardId);
                if (row.MappingKind is not ("direct" or "inlined" or "shared-syntax" or "policy" or "semantic-wrapper" or "unresolved")) Error("Mapping", row.StandardId);
                if (string.IsNullOrWhiteSpace(row.Notes)) Error("Evidence", "missing explanation " + row.StandardId);
                if (row.Targets.Length == 0 && row.Policies.Length == 0 && row.MappingKind != "unresolved") Error("Target", "no owner for " + row.StandardId);
                if (row.Targets.Distinct(StringComparer.Ordinal).Count() != row.Targets.Length || row.Targets.Any(t => !targets.Contains(t))) Error("Target", row.StandardId);
                if (row.Policies.Distinct(StringComparer.Ordinal).Count() != row.Policies.Length || row.Policies.Any(p => !policies.Contains(p))) Error("Policy", row.StandardId);
                if (row.MappingKind == "policy" && row.Policies.Length == 0) Error("Policy", "no policy for " + row.StandardId);
                Exact("Reviews", Requirements, row.Reviews.Keys);
                if (row.Reviews.Values.Any(s => s is not ("pending" or "verified"))) Error("Reviews", "unknown review state " + row.StandardId);
                bool closed = row.Reviews.Count == Requirements.Length && row.Reviews.Values.All(s => s == "verified");
                if (row.Status == "verified" && (!closed || row.MappingKind == "unresolved")) Error("Reviews", "verified row lacks review closure " + row.StandardId);
                if (row.Status != "verified" || !closed || row.MappingKind == "unresolved") pending++;
            }
            if (pending != 0) blockers.Add($"Crosswalk: {pending} standard production occurrences await normative/context closure");
        }
        catch (Exception error) { errors.Add("CrosswalkSchema: " + error.Message); }
        return new(count, distinct, pending, errors, blockers);
    }
}
