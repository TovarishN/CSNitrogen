using System.Security.Cryptography;
using System.Text.Json;
namespace CSNitrogen.Profiles;
public sealed record CoverageFeature(string Id, Version Introduced, string Classification, bool Reviewed, string Status);
/// <summary>Implemented production syntax only. Planned coverage is tracked separately in docs/coverage.</summary>
public static class FeatureCatalog
{
    public static IReadOnlyList<FeatureDefinition> Implemented { get; } = Array.AsReadOnly(new[] {
        new FeatureDefinition("csharp3.object-initializers", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/ObjectInitializers.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.anonymous-types", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/AnonymousTypes.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.implicit-arrays", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/ImplicitArrays.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.extension-receivers", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/ExtensionReceivers.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.partial-methods", Array.AsReadOnly(new[]{"csharp2.declaration-forms"}), Array.Empty<string>(), Array.Empty<string>()) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.auto-properties", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.Empty<string>()) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.generic-disambiguation", Array.AsReadOnly(new[]{"csharp2.generics"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/GenericFollowersThird.ngr"})) { Introduced=new(3,0) },
        new FeatureDefinition("csharp3.lambdas", Array.AsReadOnly(new[]{"baseline.expressions","baseline.statements","csharp2.anonymous-methods"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lambdas.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp3.queries", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Queries.ngr"})) { Introduced=new(3,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp2.generics", Array.AsReadOnly(new[]{"baseline.names-and-types","baseline.expressions","baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Generics.ngr"})) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp2.nullable-values", Array.AsReadOnly(new[]{"baseline.names-and-types","baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/NullableValues.ngr"})) { Introduced=new(2,0) },
        new FeatureDefinition("csharp2.anonymous-methods", Array.AsReadOnly(new[]{"baseline.expressions","baseline.attributes","baseline.statements"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/AnonymousMethods.ngr"})) { Introduced=new(2,0) },
        new FeatureDefinition("csharp2.iterators", Array.AsReadOnly(new[]{"baseline.statements","baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Iterators.ngr"})) { Introduced=new(2,0) },
        new FeatureDefinition("csharp2.default-values", Array.AsReadOnly(new[]{"baseline.expressions","baseline.names-and-types"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/DefaultValues.ngr"})) { Introduced=new(2,0) },
        new FeatureDefinition("csharp2.declaration-forms", Array.AsReadOnly(new[]{"baseline.members","baseline.attributes"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/DeclarationForms.ngr"})) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp2.alias-qualification", Array.AsReadOnly(new[]{"baseline.names-and-types","baseline.namespace-declarations"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/AliasQualification.ngr"})) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp2.pragmas", Array.AsReadOnly(new[]{"baseline.conditional-compilation"}), Array.Empty<string>(), Array.Empty<string>()) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"csharp1-preprocessing"}) },
        new FeatureDefinition("csharp2.generic-unsafe", Array.AsReadOnly(new[]{"csharp2.generics","baseline.unsafe-syntax"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/GenericUnsafe.ngr"})) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("csharp2.fixed-buffers", Array.AsReadOnly(new[]{"baseline.members","baseline.unsafe-syntax"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/FixedBuffers.ngr"})) { Introduced=new(2,0), Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("foundation.empty-classes", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Core.ngr","Grammars/Declarations.ngr"})),
        new FeatureDefinition("baseline.unicode-identifiers", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})) { Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("baseline.escaped-identifiers", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})) { Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("baseline.numeric-literals", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.character-literals", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.regular-strings", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.verbatim-strings", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.trivia", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()) { Policies=Array.AsReadOnly(new[]{"csharp-trivia"}) },
        new FeatureDefinition("baseline.conditional-compilation", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()) { Policies=Array.AsReadOnly(new[]{"csharp1-preprocessing"}) },
        new FeatureDefinition("baseline.location-directives", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()) { Policies=Array.AsReadOnly(new[]{"csharp1-preprocessing"}) },
        new FeatureDefinition("baseline.diagnostic-directives", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()) { Policies=Array.AsReadOnly(new[]{"csharp1-preprocessing"}) },
        new FeatureDefinition("baseline.names-and-types", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/NamesTypes.ngr"})),
        new FeatureDefinition("baseline.expressions", Array.AsReadOnly(new[]{"baseline.names-and-types","baseline.numeric-literals"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Expressions.ngr"})) { Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("baseline.statement-expressions", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Statements.ngr"})) { Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("baseline.statements", Array.AsReadOnly(new[]{"baseline.statement-expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Statements.ngr"})),
        new FeatureDefinition("baseline.attributes", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Attributes.ngr","Grammars/GlobalTargetsCurrent.ngr"})) { Policies=Array.AsReadOnly(new[]{"attribute-argument-order","attribute-target-recognition"}) },
        new FeatureDefinition("baseline.members", Array.AsReadOnly(new[]{"baseline.attributes","baseline.statements"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Declarations.ngr"})) { Policies=Array.AsReadOnly(new[]{"generated-syntax-context-checks"}) },
        new FeatureDefinition("baseline.namespace-declarations", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/CompilationUnits.ngr"})),
        new FeatureDefinition("baseline.unsafe-syntax", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Unsafe.ngr"})),
    });
    private sealed record Snapshot(IReadOnlyList<CoverageFeature> Inventory, string Identity,
        IReadOnlyList<CoverageMapping> Mappings,IReadOnlyList<string> KnownPolicies,
        IReadOnlyDictionary<string,string[]> Factories,bool Reviewed,IReadOnlyList<string> HistoricalModules,bool SecondReviewed,IReadOnlyList<string> SecondModules);
    public static IReadOnlyList<CoverageMapping> CoverageMappings => Coverage.Value.Mappings;
    internal static IReadOnlyList<string> KnownPolicies => Coverage.Value.KnownPolicies;
    internal static IReadOnlyList<string> FactoryGrammars(string name) => Coverage.Value.Factories.TryGetValue(name,out var g)?g:Array.Empty<string>();
    internal static bool SourceReviewed => Coverage.Value.Reviewed;
    internal static bool SecondSourceReviewed => Coverage.Value.SecondReviewed;
    internal static IReadOnlyList<string> SecondModules => Coverage.Value.SecondModules;
    internal static IReadOnlyList<string> HistoricalModules => Coverage.Value.HistoricalModules;
    private static readonly Lazy<Snapshot> Coverage = new(LoadCoverage);
    public static IReadOnlyList<CoverageFeature> Inventory => Coverage.Value.Inventory;
    public static string CoverageIdentity => Coverage.Value.Identity;
    private static Snapshot LoadCoverage()
    {
        var assembly = typeof(FeatureCatalog).Assembly;
        var inputs=new List<KeyValuePair<string,byte[]>>();
        byte[] Read(string name)
        {
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException("Missing coverage resource: " + name);
            using var buffer = new MemoryStream(); stream.CopyTo(buffer);
            var bytes = buffer.ToArray(); inputs.Add(new(name,bytes)); return bytes;
        }
        var inventory = Read("CSNitrogen.coverage.json");
        Read("CSNitrogen.dependencies.json");
        var composition=Read("CSNitrogen.composition.json");
        var reference=Read("CSNitrogen.reference-policy.json");
        Read("CSNitrogen.csharp3-reference-policy.json");Read("CSNitrogen.csharp3-standard-index.json");
        Read("CSNitrogen.csharp3-standard-crosswalk.json");
        var secondReference=Read("CSNitrogen.csharp2-reference-policy.json");var secondIndex=Read("CSNitrogen.csharp2-standard-index.json");
        var secondGate=Read("CSNitrogen.csharp2-coverage-gate.json");var secondCrosswalk=Read("CSNitrogen.csharp2-standard-crosswalk.json");
        var gate=Read("CSNitrogen.csharp1-coverage-gate.json");
        var index=Read("CSNitrogen.csharp1-standard-index.json");
        var crosswalk=Read("CSNitrogen.csharp1-standard-crosswalk.json");
        foreach(var name in assembly.GetManifestResourceNames().Where(n=>n.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)||n.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)||n.StartsWith("CSNitrogen.review.",StringComparison.Ordinal)))Read(name);
        using var document = JsonDocument.Parse(inventory);
        var entries = document.RootElement.GetProperty("features").EnumerateArray().Select(f => new CoverageFeature(
            f.GetProperty("id").GetString()!, Version.Parse(f.GetProperty("introduced").GetString()!),
            f.GetProperty("classification").GetString()!, f.GetProperty("reviewed").GetBoolean(),
            f.GetProperty("status").GetString()!)).OrderBy(f => f.Id,StringComparer.Ordinal).ToArray();
        using var config=JsonDocument.Parse(composition);using var ledger=JsonDocument.Parse(gate);
        using var source=JsonDocument.Parse(index);using var map=JsonDocument.Parse(crosswalk);using var primary=JsonDocument.Parse(reference);
        var mappings=document.RootElement.GetProperty("features").EnumerateArray().Where(f=>f.TryGetProperty("coverage",out _)).Select(f=>
        {
            var coverage=f.GetProperty("coverage");
            return new CoverageMapping(f.GetProperty("id").GetString()!,
                Array.AsReadOnly(coverage.GetProperty("capabilities").EnumerateArray().Select(c=>c.GetString()!).ToArray()),
                Array.AsReadOnly(coverage.GetProperty("policies").EnumerateArray().Select(c=>c.GetString()!).ToArray()));
        }).OrderBy(m=>m.InventoryId,StringComparer.Ordinal).ToArray();
        var factories=config.RootElement.GetProperty("languages").EnumerateArray().ToDictionary(l=>l.GetProperty("name").GetString()!,l=>l.GetProperty("grammars").EnumerateArray().Select(g=>g.GetString()!).ToArray(),StringComparer.Ordinal);
        using var secondLedger=JsonDocument.Parse(secondGate);
        var known=ledger.RootElement.GetProperty("policyGroups").EnumerateObject().Select(p=>p.Name).Concat(["unicode-3.0-bmp","unicode-4.0-scalar","unicode-current-bmp"]).Order(StringComparer.Ordinal).ToArray();
        return new(Array.AsReadOnly(entries),CoverageHash.Compute(inputs),Array.AsReadOnly(mappings),Array.AsReadOnly(known),factories,
            CoverageContract.Reviewed(ledger.RootElement,map.RootElement,source.RootElement,primary.RootElement,config.RootElement),
            Array.AsReadOnly(ledger.RootElement.GetProperty("modules").EnumerateArray().Select(m=>m.GetProperty("module").GetString()!).ToArray()),
            CSharp2CoverageContract.Reviewed(secondGate,secondIndex,secondCrosswalk,secondReference,composition,inputs.ToDictionary(x=>x.Key,x=>x.Value,StringComparer.Ordinal)),
            Array.AsReadOnly(secondLedger.RootElement.GetProperty("factory").GetProperty("modules").EnumerateArray().Select(x=>x.GetString()!).ToArray()));
    }
}
