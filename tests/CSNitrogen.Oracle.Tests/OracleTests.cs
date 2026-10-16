using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class OracleTests
{
    [Theory][InlineData(LanguageVersion.CSharp1)][InlineData(LanguageVersion.CSharp7)][InlineData(LanguageVersion.CSharp7_1)][InlineData(LanguageVersion.CSharp7_2)][InlineData(LanguageVersion.CSharp7_3)][InlineData(LanguageVersion.CSharp14)]
    public void Explicit_version_is_preserved(LanguageVersion version)
    {
        var observation=Oracle.Observe("class C {}",version);
        Assert.Equal(version,((CSharpParseOptions)observation.Tree.Options).LanguageVersion);
        Assert.DoesNotContain(observation.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
    }
    [Fact] public void Version_gate_can_be_a_compilation_diagnostic()
    {
        const string source="class C { object M() => new(); }";
        var old=Oracle.Observe(source,LanguageVersion.CSharp8); var newer=Oracle.Observe(source,LanguageVersion.CSharp9);
        Assert.DoesNotContain(old.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
        Assert.Contains(old.CompilationDiagnostics,d=>d.Id=="CS8400");
        Assert.DoesNotContain(newer.CompilationDiagnostics,d=>d.Id=="CS8400");
        Assert.DoesNotContain(newer.CompilationDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
    }
    [Fact] public void Malformed_input_has_syntax_diagnostics()
    { Assert.Contains(Oracle.Observe("class C {",LanguageVersion.CSharp14).SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error); }
}
