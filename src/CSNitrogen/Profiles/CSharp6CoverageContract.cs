using System.Text;
using System.Text.Json;
using static CSNitrogen.Profiles.ContractJson;

namespace CSNitrogen.Profiles;

/// <summary>Admission of the reviewed ECMA-334 sixth-edition normalized syntax snapshot.</summary>
internal static class CSharp6CoverageContract
{
    internal const string ApprovedLedgerSha256 = "e37490990b76365afd57542df66039ee9f6f8fd3132cb957834c6a6fd4087cb7";
    internal const string ApprovalResource = "CSNitrogen.csharp6-approval.json";
    private const string Profile = "CSharp6";
    private const string PrimarySha256 = "4b859dfce21f9935f234b48af55c6a9de36e2eda1eb58d3b565e5aab2b44922f";
    private const string WorkingCommit = "721ef2964efeb5b4b803e856a00094dc4fe21bb7";
    private const string WorkingGrammarSha256 = "909c7c563d0034df3ed74395f7777933df776301551d7e7b38f3966d81f35d83";
    private const string PdfTextSha256 = "b74337ad3d4151f370a1e591b09731044ae39981af163520afd7c8dfc160b70c";
    private const string Start = "CompilationUnit";
    private const string RootModule = "CSharp.CompilationUnits";
    private const string ArtifactPrefix = "CSNitrogen.csharp6-artifact.csharp6-";
    private const string EvidencePrefix = "CSNitrogen.csharp6-evidence.";

    internal static bool IsEvidenceResource(string name) =>
        name.StartsWith("CSNitrogen.csharp6-artifact.", StringComparison.Ordinal) ||
        name.StartsWith("CSNitrogen.csharp6-review.", StringComparison.Ordinal) ||
        name.StartsWith(EvidencePrefix, StringComparison.Ordinal);

    internal static bool Eligible(string name) =>
        name is "CSNitrogen.coverage.json" or "CSNitrogen.dependencies.json" or "CSNitrogen.composition.json" ||
        IsEvidenceResource(name) ||
        name is "CSNitrogen.csharp5-artifact.csharp5-standard-index.json" or
            "CSNitrogen.csharp5-artifact.csharp5-standard-crosswalk.json" or
            "CSNitrogen.csharp5-evidence.alternative-witness-matrix.json" ||
        name.StartsWith("CSNitrogen.grammar.", StringComparison.Ordinal) ||
        name.StartsWith("CSNitrogen.source.", StringComparison.Ordinal) &&
            !Enumerable.Range(2, 5).Any(v => name == $"CSNitrogen.source.Profiles/CSharp{v}CoverageContract.cs");

    internal static IReadOnlyList<string> ReadFactoryModules(byte[] ledger)
    {
        try
        {
            using var document = JsonDocument.Parse(ledger);
            var modules = Strings(document.RootElement.GetProperty("factory").GetProperty("modules"));
            return modules.All(m => !string.IsNullOrWhiteSpace(m)) ? Array.AsReadOnly(modules) : [];
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            return [];
        }
    }

    internal static bool Reviewed(byte[] ledger, IReadOnlyDictionary<string, byte[]> inputs, bool fifthReviewed,
        string expectedDigest = ApprovedLedgerSha256)
    {
        try
        {
            string digest = Hash(ledger);
            if (!fifthReviewed || digest != expectedDigest || !inputs.TryGetValue(ApprovalResource, out var approvalBytes))
                return false;

            using var approvalDocument = JsonDocument.Parse(approvalBytes);
            var approval = approvalDocument.RootElement;
            if (Text(approval, "profile") != Profile || Text(approval, "status") != "approved" ||
                approval.GetProperty("schemaVersion").GetInt32() != 1 ||
                string.IsNullOrWhiteSpace(Text(approval, "reviewer")) ||
                !DateOnly.TryParseExact(Text(approval, "approvedOn"), "yyyy-MM-dd", out _) ||
                Text(approval, "ledgerSha256") != digest)
                return false;

            using var ledgerDocument = JsonDocument.Parse(ledger);
            var gate = ledgerDocument.RootElement;
            if (Text(gate, "profile") != Profile || Text(gate, "status") != "review-candidate" ||
                gate.GetProperty("schemaVersion").GetInt32() != 1 ||
                Text(gate, "predecessor") != "CSharp5" || Text(gate, "primarySha256") != PrimarySha256)
                return false;

            var pins = gate.GetProperty("reviewInputs").EnumerateObject().ToArray();
            if (!Exact(pins.Select(p => p.Name), inputs.Keys.Where(Eligible)) ||
                pins.Any(p => Hash(inputs[p.Name]) != p.Value.GetString()))
                return false;

            byte[] Artifact(string name) => inputs[ArtifactPrefix + name + ".json"];
            byte[] Evidence(string name) => inputs[EvidencePrefix + name + ".json"];
            using var referenceDocument = JsonDocument.Parse(Artifact("reference-policy"));
            using var indexDocument = JsonDocument.Parse(Artifact("standard-index"));
            using var crosswalkDocument = JsonDocument.Parse(Artifact("standard-crosswalk"));
            using var annexDocument = JsonDocument.Parse(Artifact("pdf-differences"));
            using var differencesDocument = JsonDocument.Parse(Artifact("main-text-differences"));
            using var dispositionsDocument = JsonDocument.Parse(Artifact("main-text-dispositions"));
            using var matrixDocument = JsonDocument.Parse(Evidence("alternative-witness-matrix"));
            using var casesDocument = JsonDocument.Parse(Evidence("source-specific-cases"));
            using var predecessorIndexDocument = JsonDocument.Parse(inputs["CSNitrogen.csharp5-artifact.csharp5-standard-index.json"]);
            using var predecessorMatrixDocument = JsonDocument.Parse(inputs["CSNitrogen.csharp5-evidence.alternative-witness-matrix.json"]);
            using var compositionDocument = JsonDocument.Parse(inputs["CSNitrogen.composition.json"]);

            var index = indexDocument.RootElement;
            var crosswalk = crosswalkDocument.RootElement;
            var annex = annexDocument.RootElement;
            var differences = differencesDocument.RootElement;
            var matrix = matrixDocument.RootElement;
            var cases = casesDocument.RootElement;
            if (new[] { index, crosswalk, annex, differences, matrix, cases }.Any(
                    entry => Text(entry, "profile") != Profile || Text(entry, "primarySha256") != PrimarySha256) ||
                !SourceMatches(referenceDocument.RootElement, index, annex, differences))
                return false;

            string indexHash = Hash(Artifact("standard-index"));
            string predecessorMatrixHash = Hash(inputs["CSNitrogen.csharp5-evidence.alternative-witness-matrix.json"]);
            if (Text(crosswalk, "indexSha256") != indexHash || Text(annex, "indexSha256") != indexHash ||
                Text(matrix, "indexSha256") != indexHash ||
                Text(crosswalk, "predecessorIndexSha256") != Hash(inputs["CSNitrogen.csharp5-artifact.csharp5-standard-index.json"]) ||
                Text(crosswalk, "predecessorCrosswalkSha256") != Hash(inputs["CSNitrogen.csharp5-artifact.csharp5-standard-crosswalk.json"]) ||
                Text(matrix, "crosswalkSha256") != Hash(Artifact("standard-crosswalk")) ||
                Text(matrix.GetProperty("predecessorMatrix"), "sha256") != predecessorMatrixHash ||
                Text(matrix.GetProperty("sourceSpecific"), "sha256") != Hash(Evidence("source-specific-cases")))
                return false;

            var rules = Rows(index, "productions", "id");
            if (rules.Count != 436 || !FactoryAndOwnersMatch(gate, compositionDocument.RootElement, crosswalk, rules, inputs))
                return false;
            return DispositionsMatch(differences, dispositionsDocument.RootElement, Hash(Artifact("main-text-differences"))) &&
                WitnessesMatch(rules.Values, crosswalk, matrix, cases, predecessorIndexDocument.RootElement, predecessorMatrixDocument.RootElement);
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException or OverflowException)
        {
            return false;
        }
    }

    private static bool SourceMatches(JsonElement reference, JsonElement index, JsonElement annex, JsonElement differences)
    {
        var unicode = reference.GetProperty("unicode");
        var exclusions = Strings(reference.GetProperty("normalizedExclusions"));
        string[] requiredExclusions = ["binding-types-inference-and-accessibility", "general-modifier-compatibility-and-finite-sets",
            "constantness-variance-and-pointer-legality", "control-flow-and-runtime", "awaitability-task-types-and-caller-substitution",
            "interpolation-conversion-and-evaluation", "nameof-value-and-static-import-lookup", "filter-order-and-read-only-assignment",
            "inherited-non-abstract-indexer-semicolon-accessors", "inherited-standalone-await-identifier-statement",
            "long-unicode-escape-for-initial-underscore"];
        var counts = annex.GetProperty("counts");
        return Text(reference, "profile") == Profile && Text(reference.GetProperty("primary"), "sha256") == PrimarySha256 &&
            Text(unicode, "status") == "approved" && Text(unicode, "policy") == "unicode-4.0-scalar" &&
            Text(reference.GetProperty("nameof"), "status") == "approved" && Exact(exclusions, requiredExclusions) &&
            Text(reference.GetProperty("workingText"), "commit") == WorkingCommit &&
            Text(index.GetProperty("source"), "commit") == WorkingCommit &&
            Text(index.GetProperty("source"), "grammarSha256") == WorkingGrammarSha256 &&
            Text(reference.GetProperty("extraction"), "textSha256") == PdfTextSha256 &&
            Text(annex, "extractionSha256") == PdfTextSha256 && Text(differences, "extractionSha256") == PdfTextSha256 &&
            Text(differences.GetProperty("workingText"), "commit") == WorkingCommit &&
            index.GetProperty("mainTextDisagreements").GetArrayLength() == 0 && index.GetProperty("withoutMainText").GetArrayLength() == 0 &&
            counts.GetProperty("agreed").GetInt32() == 436 && counts.GetProperty("indexed").GetInt32() == 436 &&
            counts.GetProperty("pdfRules").GetInt32() == 436 && counts.GetProperty("differences").GetInt32() == 0 &&
            counts.GetProperty("pdfOnly").GetInt32() == 0 && annex.GetProperty("differences").GetArrayLength() == 0 &&
            annex.GetProperty("pdfOnly").GetArrayLength() == 0;
    }

    private static bool FactoryAndOwnersMatch(JsonElement gate, JsonElement composition, JsonElement crosswalk,
        Dictionary<string, JsonElement> rules, IReadOnlyDictionary<string, byte[]> inputs)
    {
        var factory = gate.GetProperty("factory");
        var configured = composition.GetProperty("languages").EnumerateArray().Single(x => Text(x, "name") == Profile);
        var mapped = crosswalk.GetProperty("factory");
        var grammars = Strings(factory.GetProperty("grammars"));
        var modules = Strings(factory.GetProperty("modules"));
        if (Text(factory, "name") != Profile || Text(factory, "start") != Start || Text(factory, "rootModule") != RootModule ||
            Text(configured, "start") != RootModule + "." + Start || Text(configured, "namespace") != "CSNitrogen.Syntax" ||
            Text(mapped, "name") != Profile || Text(mapped, "start") != Text(configured, "start") ||
            !Exact(grammars, Strings(configured.GetProperty("grammars"))) ||
            !Exact(grammars, Strings(mapped.GetProperty("grammars"))) ||
            modules.Length != 41 || !Parsing.CSharpParser.MatchesFactory(Profile, modules))
            return false;

        const string grammarDirectory = "Grammars/";
        if (grammars.Any(g => !g.StartsWith(grammarDirectory, StringComparison.Ordinal))) return false;
        var declared = CSharp5CoverageContract.DeclaredOwners(grammars
            .Select(g => Encoding.UTF8.GetString(inputs["CSNitrogen.grammar." + g[grammarDirectory.Length..]])));
        var rows = Rows(crosswalk, "productions", "standardId");
        if (!Exact(rules.Keys, rows.Keys)) return false;
        var groups = gate.GetProperty("policyGroups").EnumerateObject()
            .ToDictionary(p => p.Name, p => Strings(p.Value), StringComparer.Ordinal);
        if (!groups.TryGetValue("csharp6.source-context-checks", out var sixthPolicy) ||
            !Exact(sixthPolicy, ["Parsing/CSharp6ContextPolicy.cs"]) ||
            groups.Values.SelectMany(p => p).Any(p => !inputs.ContainsKey("CSNitrogen.source." + p)))
            return false;

        var owners = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (id, row) in rows)
        {
            var rule = rules[id];
            var targets = Strings(row.GetProperty("targets"));
            var policies = Strings(row.GetProperty("policies"));
            if (Text(row, "sourceBodySha256") != Text(rule, "sourceBodySha256") ||
                Text(row, "grammar") != Text(rule, "grammar") || Text(row, "name") != Text(rule, "name") ||
                Text(row, "reviewStatus") != "owner-candidate" || !rule.GetProperty("mainTextAgrees").GetBoolean() ||
                rule.GetProperty("mainTextOccurrences").GetArrayLength() == 0 ||
                targets.Length + policies.Length == 0 || targets.Any(t => !declared.Contains(t)) ||
                policies.Any(p => !groups.ContainsKey(p)) ||
                !Exact(targets, targets.Distinct()) || !Exact(policies, policies.Distinct()))
                return false;
            owners.UnionWith(targets);
        }
        return Exact(owners, Strings(gate.GetProperty("owners")));
    }

    private static bool DispositionsMatch(JsonElement differences, JsonElement record, string sourceHash)
    {
        if (Text(record, "profile") != Profile || Text(record, "differencesSha256") != sourceHash ||
            differences.GetProperty("schemaVersion").GetInt32() != 2 || record.GetProperty("schemaVersion").GetInt32() != 2)
            return false;

        var working = differences.GetProperty("workingTextOnly").EnumerateArray()
            .Select(r => ("working-text-only", $"{Text(r, "file")}:{r.GetProperty("line").GetInt32()}", Text(r, "kind"), Text(r, "sha256")));
        var pdf = differences.GetProperty("pdfOnly").EnumerateArray()
            .Select(r => ("pdf-only", Text(r, "before"), "pdf", Text(r, "sha256")));
        var pending = working.Concat(pdf).ToHashSet();
        if (pending.Count != 69 || differences.GetProperty("workingTextOnly").GetArrayLength() != 36 ||
            differences.GetProperty("pdfOnly").GetArrayLength() != 33) return false;
        var categories = record.GetProperty("categories");
        foreach (var row in record.GetProperty("dispositions").EnumerateArray())
        {
            string kind = Text(row, "kind");
            string category = Text(row, "disposition");
            bool syntaxBearing = kind == "grammar" || kind == "pdf" && category == "grammar-comment-layout";
            if (!pending.Remove((Text(row, "direction"), Text(row, "at"), kind, Text(row, "sha256"))) ||
                !categories.TryGetProperty(category, out _) || string.IsNullOrWhiteSpace(Text(row, "reason")) ||
                row.GetProperty("syntaxBearing").GetBoolean() != syntaxBearing ||
                syntaxBearing && (category != "grammar-comment-layout" || Text(row, "verifiedBy") != "tools/compare-csharp6-pdf.py"))
                return false;
        }
        return pending.Count == 0;
    }

    private static bool WitnessesMatch(IEnumerable<JsonElement> rules, JsonElement crosswalk, JsonElement matrix,
        JsonElement specific, JsonElement predecessorIndex, JsonElement predecessorMatrix)
    {
        var alternatives = rules.SelectMany(rule => rule.GetProperty("alternatives").EnumerateArray()
                .Select((alternative, position) => (Rule: rule, Alternative: alternative, Position: position)))
            .ToDictionary(entry => Text(entry.Alternative, "id"), StringComparer.Ordinal);
        var crosswalkRows = Rows(crosswalk, "productions", "standardId");
        var predecessorRules = Rows(predecessorIndex, "productions", "id");
        var links = Rows(matrix, "cases", "id");
        var cases = Rows(specific, "cases", "id");
        var inherited = Rows(predecessorMatrix, "cases", "id");
        if (alternatives.Count != 1115 || cases.Count != 418 || !Exact(alternatives.Keys, links.Keys)) return false;

        int count = 0;
        foreach (var (id, link) in links)
        {
            var (rule, alternative, position) = alternatives[id];
            string hash = Text(alternative, "sourceAlternativeSha256");
            string witness = Text(link, "witnessId");
            string domain = Text(link, "domain");
            if (Text(link, "sourceAlternativeSha256") != hash) return false;
            if (Text(link, "sourceKind") == "sixth-specific")
            {
                if (id != witness || domain != "sixth-specific" || !cases.Remove(witness, out var entry) ||
                    Text(entry, "sourceAlternativeSha256") != hash) return false;
                var source = Text(entry, "source");
                var focus = Text(entry, "focus");
                int start = entry.GetProperty("focusStart").GetInt32();
                if (start < 0 || focus.Length == 0 || start > source.Length - focus.Length ||
                    !source.AsSpan(start, focus.Length).SequenceEqual(focus)) return false;
            }
            else
            {
                string predecessorId = Text(link, "predecessorAlternative");
                if (Text(link, "sourceKind") != "predecessor-positional" ||
                    !inherited.TryGetValue(predecessorId, out var entry) ||
                    Text(entry, "witnessId") != witness || Text(entry, "domain") != domain) return false;

                var sourceRow = crosswalkRows[Text(rule, "id")];
                var sources = Strings(sourceRow.GetProperty("predecessors"));
                if (Text(sourceRow, "relation") != "name-carried" || sources.Length != 1 ||
                    !predecessorRules.TryGetValue(sources[0], out var predecessorRule) ||
                    Text(predecessorRule, "name") != Text(rule, "name").Replace('_', '-').ToLowerInvariant())
                    return false;
                var predecessorAlternatives = predecessorRule.GetProperty("alternatives");
                if (predecessorAlternatives.GetArrayLength() != rule.GetProperty("alternatives").GetArrayLength() ||
                    Text(predecessorAlternatives[position], "id") != predecessorId)
                    return false;
                count++;
            }
        }
        return cases.Count == 0 && count == 697;
    }

    private static Dictionary<string, JsonElement> Rows(JsonElement document, string collection, string key) =>
        document.GetProperty(collection).EnumerateArray().ToDictionary(row => Text(row, key), StringComparer.Ordinal);
}
