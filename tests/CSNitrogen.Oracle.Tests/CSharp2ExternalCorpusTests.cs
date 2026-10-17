using System.Security.Cryptography;
using System.Text.Json;
using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class CSharp2ExternalCorpusTests
{
    [Theory][InlineData("gtest-001.cs")][InlineData("gtest-002.cs")][InlineData("gtest-003.cs")]
    public void Predeclared_historical_generic_sources_keep_provenance_and_version_boundary(string name)
    {
        string root=Path.Combine(AppContext.BaseDirectory,"Corpus","Mono2006");
        using var provenance=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"provenance.json")));
        var file=provenance.RootElement.GetProperty("files").EnumerateArray().Single(f=>f.GetProperty("local").GetString()==name);
        byte[] bytes=File.ReadAllBytes(Path.Combine(root,name));Assert.Equal(file.GetProperty("sha256").GetString(),Convert.ToHexStringLower(SHA256.HashData(bytes)));
        string source=System.Text.Encoding.UTF8.GetString(bytes);
        var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp2));Assert.DoesNotContain(reference.GetDiagnostics(),d=>d.Severity==DiagnosticSeverity.Error);
        using var first=CSharpParser.Parse(new(source,"CSharp1",name,[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2",name,[]));Assert.True(second.Accepted,string.Join(";",second.Issues));Assert.True(second.ProfileComplete);
    }
}
