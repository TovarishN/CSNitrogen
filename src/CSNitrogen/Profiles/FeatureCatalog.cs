using System.Security.Cryptography;
using System.Text.Json;
namespace CSNitrogen.Profiles;
public sealed record CoverageFeature(string Id, Version Introduced, string Classification, bool Reviewed, string Status);
/// <summary>Implemented production syntax only. Planned coverage is tracked separately in docs/coverage.</summary>
public static class FeatureCatalog
{
    public static IReadOnlyList<FeatureDefinition> Implemented { get; } = Array.AsReadOnly(new[] {
        new FeatureDefinition("foundation.empty-classes", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Core.ngr","Grammars/Declarations.ngr"})),
        new FeatureDefinition("baseline.unicode-identifiers", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.escaped-identifiers", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.numeric-literals", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.character-literals", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.regular-strings", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.verbatim-strings", Array.Empty<string>(), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Lexical.ngr"})),
        new FeatureDefinition("baseline.trivia", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
        new FeatureDefinition("baseline.conditional-compilation", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()),
        new FeatureDefinition("baseline.location-directives", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()),
        new FeatureDefinition("baseline.diagnostic-directives", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.Empty<string>()),
        new FeatureDefinition("baseline.names-and-types", Array.AsReadOnly(new[]{"baseline.unicode-identifiers"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/NamesTypes.ngr"})),
        new FeatureDefinition("baseline.expressions", Array.AsReadOnly(new[]{"baseline.names-and-types","baseline.numeric-literals"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Expressions.ngr"})),
        new FeatureDefinition("baseline.statement-expressions", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Statements.ngr"})),
        new FeatureDefinition("baseline.statements", Array.AsReadOnly(new[]{"baseline.statement-expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Statements.ngr"})),
        new FeatureDefinition("baseline.attributes", Array.AsReadOnly(new[]{"baseline.expressions"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Attributes.ngr"})),
        new FeatureDefinition("baseline.members", Array.AsReadOnly(new[]{"baseline.attributes","baseline.statements"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Declarations.ngr"})),
        new FeatureDefinition("baseline.namespace-declarations", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/CompilationUnits.ngr"})),
        new FeatureDefinition("baseline.unsafe-syntax", Array.AsReadOnly(new[]{"baseline.members"}), Array.Empty<string>(), Array.AsReadOnly(new[]{"Grammars/Unsafe.ngr"})),
    });
    private sealed record Snapshot(IReadOnlyList<CoverageFeature> Inventory, string Identity);
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
        Read("CSNitrogen.composition.json");
        foreach(var name in assembly.GetManifestResourceNames().Where(n=>n.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)||n.StartsWith("CSNitrogen.source.",StringComparison.Ordinal)))Read(name);
        using var document = JsonDocument.Parse(inventory);
        var entries = document.RootElement.GetProperty("features").EnumerateArray().Select(f => new CoverageFeature(
            f.GetProperty("id").GetString()!, Version.Parse(f.GetProperty("introduced").GetString()!),
            f.GetProperty("classification").GetString()!, f.GetProperty("reviewed").GetBoolean(),
            f.GetProperty("status").GetString()!)).OrderBy(f => f.Id,StringComparer.Ordinal).ToArray();
        return new(Array.AsReadOnly(entries), CoverageHash.Compute(inputs));
    }
}
