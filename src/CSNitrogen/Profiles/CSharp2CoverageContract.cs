using System.Security.Cryptography;
using System.Text.Json;
namespace CSNitrogen.Profiles;

/// <summary>Admission of the reviewed fourth-edition syntax snapshot, not semantic compilation.</summary>
internal static class CSharp2CoverageContract
{
    internal const string PrimarySha256="de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652";
    internal const string ApprovedLedgerSha256="c55db605410a12afc6dc1d8b77339a09107fd163d47f103436d92f937ea7c876";
    internal static bool Reviewed(byte[] ledger,byte[] index,byte[] crosswalk,byte[] reference,byte[] composition,
        IReadOnlyDictionary<string,byte[]> inputs)
    {
        static string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
        static bool Exact(IEnumerable<string> a,IEnumerable<string> b)
        {var x=a.Order(StringComparer.Ordinal).ToArray();var y=b.Order(StringComparer.Ordinal).ToArray();return x.Distinct().Count()==x.Length&&y.Distinct().Count()==y.Length&&x.SequenceEqual(y);}
        static IEnumerable<string> Strings(JsonElement value)=>value.EnumerateArray().Select(x=>x.GetString()!);
        try
        {
            if(Hash(ledger)!=ApprovedLedgerSha256)return false;
            using var g=JsonDocument.Parse(ledger);using var i=JsonDocument.Parse(index);using var m=JsonDocument.Parse(crosswalk);
            using var r=JsonDocument.Parse(reference);using var c=JsonDocument.Parse(composition);
            var gate=g.RootElement;var map=m.RootElement;var source=i.RootElement;var primary=r.RootElement;
            if(gate.GetProperty("profile").GetString()!="CSharp2"||gate.GetProperty("status").GetString()!="verified"||map.GetProperty("status").GetString()!="verified")return false;
            if(source.GetProperty("sourceSha256").GetString()!=PrimarySha256||map.GetProperty("sourceSha256").GetString()!=PrimarySha256||primary.GetProperty("primary").GetProperty("sha256").GetString()!=PrimarySha256)return false;
            if(Hash(index)!=gate.GetProperty("indexSha256").GetString()||Hash(crosswalk)!=gate.GetProperty("crosswalkSha256").GetString()||Hash(reference)!=gate.GetProperty("referenceSha256").GetString())return false;
            if(primary.GetProperty("unicode").GetProperty("policy").GetString()!="unicode-4.0-scalar"||primary.GetProperty("unicode").GetProperty("dataSha256").GetString()!="77818a7e04c563a6a628e4f10be60323bf13f08e9fb208bffce11057b008ad7e")return false;
            var reviewedNames=gate.GetProperty("reviewInputs").EnumerateObject().Select(x=>x.Name).ToArray();
            var eligible=inputs.Keys.Where(x=>(x.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)&&x!="CSNitrogen.source.Profiles/CSharp2CoverageContract.cs")||x.StartsWith("CSNitrogen.review.",StringComparison.Ordinal));
            if(!Exact(eligible,reviewedNames.Where(x=>!x.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal))))return false;
            // All grammar resources, including independently reviewed CSharp1/later helpers,
            // are pinned by the ledger; selected membership is checked separately below.
            if(!Exact(inputs.Keys.Where(x=>x.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)),reviewedNames.Where(x=>x.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal))))return false;
            foreach(var input in gate.GetProperty("reviewInputs").EnumerateObject())
                if(!inputs.TryGetValue(input.Name,out var bytes)||Hash(bytes)!=input.Value.GetString())return false;
            var factory=gate.GetProperty("factory");
            if(factory.GetProperty("name").GetString()!="CSharp2"||factory.GetProperty("start").GetString()!="CompilationUnit"||factory.GetProperty("rootModule").GetString()!="CSharp.CompilationUnits")return false;
            var configuration=c.RootElement.GetProperty("languages").EnumerateArray().Single(x=>x.GetProperty("name").GetString()=="CSharp2");
            if(configuration.GetProperty("start").GetString()!="CSharp.CompilationUnits.CompilationUnit"||configuration.GetProperty("namespace").GetString()!="CSNitrogen.Syntax")return false;
            if(!Exact(Strings(factory.GetProperty("grammars")),Strings(configuration.GetProperty("grammars"))))return false;
            var modules=gate.GetProperty("modules").EnumerateArray().ToArray();
            if(!Exact(modules.Select(x=>x.GetProperty("grammar").GetString()!),Strings(factory.GetProperty("grammars")))||!Exact(modules.Select(x=>x.GetProperty("module").GetString()!),Strings(factory.GetProperty("modules"))))return false;
            var targets=modules.SelectMany(x=>Strings(x.GetProperty("productions")).Select(p=>x.GetProperty("module").GetString()+"/"+p)).ToHashSet(StringComparer.Ordinal);
            var policies=gate.GetProperty("policyGroups").EnumerateObject().Select(x=>x.Name).ToHashSet(StringComparer.Ordinal);
            var heads=source.GetProperty("productions").EnumerateArray().ToArray();var rows=map.GetProperty("productions").EnumerateArray().ToArray();
            if(heads.Length!=409||!Exact(heads.Select(x=>x.GetProperty("id").GetString()!),rows.Select(x=>x.GetProperty("standardId").GetString()!)))return false;
            string[] dimensions=["normative-prose","positive-negative","ambiguity-context","spans-diagnostics"];
            foreach(var row in rows)
            {
                if(row.GetProperty("status").GetString()!="verified"||string.IsNullOrWhiteSpace(row.GetProperty("normativeClauses").GetString())||string.IsNullOrWhiteSpace(row.GetProperty("notes").GetString()))return false;
                var reviews=row.GetProperty("reviews").EnumerateObject().ToArray();
                if(!Exact(dimensions,reviews.Select(x=>x.Name))||reviews.Any(x=>x.Value.GetString()!="verified"))return false;
                if(Strings(row.GetProperty("targets")).Any(x=>!targets.Contains(x))||Strings(row.GetProperty("policies")).Any(x=>!policies.Contains(x)))return false;
                var review=row.GetProperty("sourceReview");
                if(review.GetProperty("occurrenceId").GetString()!=row.GetProperty("standardId").GetString()||review.GetProperty("sourceSha256").GetString()!=PrimarySha256||review.GetProperty("status").GetString()!="verified")return false;
                foreach(string kind in new[]{"positive","negative","ambiguity","span"})
                    if(row.GetProperty("witnesses").GetProperty(kind).GetArrayLength()==0)return false;
            }
            return map.GetProperty("supplementCandidates").EnumerateArray().All(x=>x.GetProperty("status").GetString()=="verified");
        }
        catch(Exception exception) when(exception is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){return false;}
    }
}
