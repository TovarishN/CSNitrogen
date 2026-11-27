using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSNitrogen.Oracle.Tests;

public class CSharp7ExpressionBodyObservations
{
    [Theory]
    [InlineData("int M() => f;", true)]
    [InlineData("int P => f;", true)]
    [InlineData("C() => f = 1;", false)]
    [InlineData("C() : this(1) => f = 1; C(int x) { f = x; }", false)]
    [InlineData("static C() => s = 1;", false)]
    [InlineData("~C() => f = 1;", false)]
    [InlineData("int P { get => f; set => f = value; }", false)]
    [InlineData("int this[int i] { get => f; set => f = value; }", false)]
    [InlineData("event System.Action E { add { h += value; } remove { h -= value; } }", true)]
    public void Existing_forms_agree_at_the_release_boundary(string members, bool availableInSix)
    {
        string source = Source(members);
        foreach (var version in new[] { LanguageVersion.CSharp6, LanguageVersion.CSharp7 })
        {
            bool expected = availableInSix || version == LanguageVersion.CSharp7;
            var errors = CompilationErrors(source, version);
            Assert.Equal(expected, errors.Length == 0);
            using var parsed = CSharpParser.Parse(new(source, version.ToString(), "arrow.cs", []));
            Assert.True(expected == parsed.Accepted, string.Join(" | ", parsed.Issues));
        }
    }

    [Theory]
    [InlineData("event System.Action E { add => h += value; remove => h -= value; }")]
    [InlineData("event System.Action E { add { h += value; } remove => h -= value; }")]
    [InlineData("event System.Action E { remove { h -= value; } add => h += value; }")]
    public void Roslyn_accepts_arrow_event_accessors_starting_in_csharp7(string members)
    {
        string source = Source(members);
        var earlierErrors = CompilationErrors(source, LanguageVersion.CSharp6);
        Assert.All(earlierErrors, error => Assert.Equal("CS8059", error.Id));
        Assert.NotEmpty(earlierErrors);
        Assert.Empty(CompilationErrors(source, LanguageVersion.CSharp7));
    }

    private static string Source(string members) =>
        $"class C {{ int f; static int s; System.Action h; {members} }}";

    private static Diagnostic[] CompilationErrors(string source, LanguageVersion version) =>
        Oracle.Observe(source, version).CompilationDiagnostics
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
}
