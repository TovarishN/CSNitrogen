using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
internal sealed record Observation(SyntaxTree Tree, IReadOnlyList<Diagnostic> SyntaxDiagnostics, IReadOnlyList<Diagnostic> CompilationDiagnostics);
internal static class Oracle
{
    private static readonly Lazy<MetadataReference[]> References = new(() => {
        var root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "../../.."));
        var referencePath = Path.Combine(root, "packs/Microsoft.NETCore.App.Ref/10.0.0/ref/net10.0");
        if (!Directory.Exists(referencePath)) throw new DirectoryNotFoundException("Oracle requires the pinned .NET 10.0.0 reference pack: " + referencePath);
        return Directory.GetFiles(referencePath,"*.dll").Order(StringComparer.Ordinal)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray();
    });
    public static Observation Observe(string source, LanguageVersion version)
    {
        var tree=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(version,kind:SourceCodeKind.Regular));
        var compilation=CSharpCompilation.Create("OracleInput",[tree],References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true));
        return new(tree,tree.GetDiagnostics().ToArray(),compilation.GetDiagnostics().ToArray());
    }
}
