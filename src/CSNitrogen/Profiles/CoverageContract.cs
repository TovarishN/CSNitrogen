using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
namespace CSNitrogen.Profiles;
internal sealed record CoverageEvaluation(IReadOnlyList<string> Missing, IReadOnlyList<ProfileIssue> Issues, bool Complete);
internal static class CoverageContract
{
    private static readonly IReadOnlyDictionary<string,string[]> PolicyOwnership = new Dictionary<string,string[]>(StringComparer.Ordinal)
    {
        ["attribute-argument-order"] = ["Parsing/CSharpAttributePolicy.cs"],
        ["attribute-target-recognition"] = ["Parsing/CSharpAttributePolicy.cs"],
        ["csharp-trivia"] = ["Parsing/CSharpTrivia.cs"],
        ["csharp1-preprocessing"] = ["Preprocessing/ConditionalPreprocessor.cs", "Preprocessing/DirectiveScanner.cs", "Preprocessing/DirectiveEffects.cs"],
        ["generated-syntax-context-checks"] = ["Parsing/CSharpSyntaxPolicy.cs", "Parsing/CSharp3ExpressionPolicy.cs", "Parsing/CSharp3DeclarationPolicy.cs", "Parsing/CSharp4ContextPolicy.cs", "Parsing/CSharpAttributePolicy.cs", "Parsing/CSharpDeclarationPolicy.cs", "Parsing/CSharpCompilationUnitPolicy.cs", "Parsing/UnicodePolicy.cs"]
    };
    internal static CoverageEvaluation Evaluate(Version version, IReadOnlyList<CoverageFeature> inventory,
        IReadOnlyList<CoverageMapping> mappings, IReadOnlyList<FeatureDefinition> definitions,
        IReadOnlyList<string> selected, IReadOnlyList<string> policies, IReadOnlyList<string> knownPolicies,
        IReadOnlyList<string> grammars, IReadOnlyList<string> factoryGrammars, bool sourceReviewed, bool actualFactoryMatches)
    {
        var issues=new List<ProfileIssue>();
        var syntax=inventory.Where(f=>f.Classification=="syntax-or-policy").ToDictionary(f=>f.Id,StringComparer.Ordinal);
        var capabilities=definitions.Select(f=>f.Id).ToHashSet(StringComparer.Ordinal);
        var selectedSet=selected.ToHashSet(StringComparer.Ordinal);var policySet=policies.ToHashSet(StringComparer.Ordinal);
        var known=knownPolicies.ToHashSet(StringComparer.Ordinal);var map=new Dictionary<string,CoverageMapping>(StringComparer.Ordinal);
        foreach(string c in selectedSet)if(!capabilities.Contains(c))issues.Add(new("coverage-capability","Unknown selected capability '"+c+"'."));
        foreach(string p in policySet)if(!known.Contains(p))issues.Add(new("coverage-policy","Unknown selected policy '"+p+"'."));
        foreach(var m in mappings)
        {
            void Error(string code,string text)=>issues.Add(new(code,$"Coverage '{m.InventoryId}': {text}"));
            if(!map.TryAdd(m.InventoryId,m))Error("coverage-duplicate","duplicate inventory mapping.");
            if(!syntax.ContainsKey(m.InventoryId))Error("coverage-inventory","unknown syntax inventory identity.");
            if(m.Capabilities.Count==0 && m.Policies.Count==0)Error("coverage-empty","mapping has no required capability or policy.");
            if(m.Capabilities.Distinct(StringComparer.Ordinal).Count()!=m.Capabilities.Count || m.Policies.Distinct(StringComparer.Ordinal).Count()!=m.Policies.Count)
                Error("coverage-duplicate-target","duplicate requirement.");
            foreach(string c in m.Capabilities)if(!capabilities.Contains(c))Error("coverage-capability","unknown capability '"+c+"'.");
            foreach(string p in m.Policies)if(!known.Contains(p))Error("coverage-policy","unknown policy '"+p+"'.");
        }
        if(!Exact(grammars,factoryGrammars) || !actualFactoryMatches)
            issues.Add(new("factory-mismatch","Resolved grammar membership or start rule differs from the named factory."));
        var requested=syntax.Values.Where(f=>f.Introduced<=version).OrderBy(f=>f.Id,StringComparer.Ordinal).ToArray();
        var missing=requested.Where(f=>!map.TryGetValue(f.Id,out var m) || !m.Capabilities.All(selectedSet.Contains) || !m.Policies.All(policySet.Contains))
            .Select(f=>f.Id).ToArray();
        bool complete=version.Major is 1 or 2 or 3 or 4 && version.Minor==0 && sourceReviewed && issues.Count==0 && missing.Length==0
            && requested.All(f=>f.Reviewed && f.Status=="verified") && policySet.Contains(version.Major==1?"unicode-3.0-bmp":"unicode-4.0-scalar") && !policySet.Contains("unicode-current-bmp");
        return new(Array.AsReadOnly(missing),Array.AsReadOnly(issues.OrderBy(i=>i.Code,StringComparer.Ordinal).ThenBy(i=>i.Message,StringComparer.Ordinal).ToArray()),complete);
    }
    private static bool Exact(IEnumerable<string> first,IEnumerable<string> second)
    {
        var a=first.Order(StringComparer.Ordinal).ToArray();var b=second.Order(StringComparer.Ordinal).ToArray();
        return a.Distinct(StringComparer.Ordinal).Count()==a.Length && b.Distinct(StringComparer.Ordinal).Count()==b.Length && a.SequenceEqual(b);
    }
    internal static bool Reviewed(JsonElement gate,JsonElement map,JsonElement index,JsonElement reference,JsonElement composition)
    {
        try
        {
            string[] requirements=["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"];
            if(gate.GetProperty("profile").GetString()!="CSharp1" || map.GetProperty("profile").GetString()!="CSharp1"
                || gate.GetProperty("schemaVersion").GetInt32()!=1 || map.GetProperty("schemaVersion").GetInt32()!=1 || index.GetProperty("schemaVersion").GetInt32()!=1)return false;
            var primary=reference.GetProperty("primary");var source=index.GetProperty("source");
            if(primary.GetProperty("sha256").GetString()!="ac93571b47aa424a31316a30ac7c9b99424b7347b81469847b738f9af8b51c39")return false;
            var headings=index.GetProperty("productions").EnumerateArray().ToArray();
            foreach(var h in headings)
                if(h.GetProperty("id").GetString()!=$"A:{h.GetProperty("pdfPage").GetInt32()}:{h.GetProperty("line").GetInt32()}:{h.GetProperty("name").GetString()}" || h.GetProperty("printedPage").GetInt32()!=h.GetProperty("pdfPage").GetInt32()-14)return false;
            string canonical=string.Concat(headings.Select(h=>h.GetProperty("id").GetString()+"|"+h.GetProperty("grammar").GetString()+"\n"));
            if(Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))!="f898938e8e39e062521ff9d9fd913e122e86e44864aab16b3108b53b4b82fb9d")return false;
            foreach(string name in new[]{"sha256","title","url"})if(primary.GetProperty(name).GetString()!=source.GetProperty(name).GetString())return false;
            if(map.GetProperty("sourceSha256").GetString()!=primary.GetProperty("sha256").GetString())return false;
            if(!Exact(requirements,map.GetProperty("reviewRequirements").EnumerateArray().Select(v=>v.GetString()!)))return false;
            var modules=gate.GetProperty("modules").EnumerateArray().ToArray();var policies=gate.GetProperty("policies").EnumerateArray().ToArray();
            var factories=composition.GetProperty("languages").EnumerateArray().Where(l=>l.GetProperty("name").GetString()=="CSharp1").ToArray();
            if(factories.Length!=1 || factories[0].GetProperty("start").GetString()!="CSharp.CompilationUnits.CompilationUnit" || factories[0].GetProperty("namespace").GetString()!="CSNitrogen.Syntax"
                || !Exact(modules.Select(m=>m.GetProperty("grammar").GetString()!),factories[0].GetProperty("grammars").EnumerateArray().Select(v=>v.GetString()!)))return false;
            if(modules.Length==0 || policies.Length==0 || modules.Concat(policies).Any(r=>r.GetProperty("status").GetString()!="verified"))return false;
            if(!Exact(["historical-standard-crosswalk","independent-context-review","runtime-profile-contract"],gate.GetProperty("obligations").EnumerateArray().Select(o=>o.GetProperty("id").GetString()!)))return false;
            if(gate.GetProperty("obligations").EnumerateArray().Any(o=>o.GetProperty("status").GetString()!="verified"))return false;
            var targets=modules.SelectMany(m=>m.GetProperty("productions").EnumerateArray().Select(p=>m.GetProperty("module").GetString()+"/"+p.GetString())).ToHashSet(StringComparer.Ordinal);
            var policyGroups=gate.GetProperty("policyGroups").EnumerateObject().ToArray();
            var policyNames=policyGroups.Select(p=>p.Name).ToHashSet(StringComparer.Ordinal);
            if(!Exact(PolicyOwnership.Keys,policyGroups.Select(p=>p.Name)))return false;
            foreach(var group in policyGroups)
                if(!Exact(PolicyOwnership[group.Name],group.Value.EnumerateArray().Select(v=>v.GetString()!)))return false;
            var policySources=policies.Select(p=>p.GetProperty("source").GetString()!).ToArray();
            if(policySources.Distinct(StringComparer.Ordinal).Count()!=policySources.Length || !Exact(policySources.Append("Parsing/CSharpTrivia.cs"),policyGroups.SelectMany(p=>p.Value.EnumerateArray().Select(v=>v.GetString()!)).Distinct(StringComparer.Ordinal)))return false;
            var rows=map.GetProperty("productions").EnumerateArray().ToArray();
            if(rows.Length!=362 || !Exact(index.GetProperty("productions").EnumerateArray().Select(r=>r.GetProperty("id").GetString()!),rows.Select(r=>r.GetProperty("standardId").GetString()!)))return false;
            foreach(var row in rows)
            {
                string strategy=row.GetProperty("mapping").GetString()!;
                if(row.GetProperty("status").GetString()!="verified" || strategy is not ("direct" or "inlined" or "shared-syntax" or "policy" or "semantic-wrapper") || string.IsNullOrWhiteSpace(row.GetProperty("notes").GetString()))return false;
                var reviews=row.GetProperty("reviews").EnumerateObject().ToArray();
                if(!Exact(requirements,reviews.Select(r=>r.Name)) || reviews.Any(r=>r.Value.GetString()!="verified"))return false;
                string[] ts=row.GetProperty("targets").EnumerateArray().Select(v=>v.GetString()!).ToArray(),ps=row.GetProperty("policies").EnumerateArray().Select(v=>v.GetString()!).ToArray();
                if(ts.Length+ps.Length==0 || ts.Any(t=>!targets.Contains(t)) || ps.Any(p=>!policyNames.Contains(p)) || ts.Distinct().Count()!=ts.Length || ps.Distinct().Count()!=ps.Length || (strategy=="policy"&&ps.Length==0))return false;
            }
            return true;
        }
        catch(Exception e) when(e is KeyNotFoundException or InvalidOperationException or FormatException){return false;}
    }
}
