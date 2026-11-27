using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSNitrogen.Oracle.Tests;

public class CSharp72PrivateProtectedObservations
{
    [Theory]
    [InlineData("class C { private protected int x; }")]
    [InlineData("class C { protected private int x; }")]
    [InlineData("class C { protected static private int x; }")]
    [InlineData("class C { private protected class Inner {} }")]
    [InlineData("class C { protected private delegate void D(); }")]
    [InlineData("class C { public int P { get; private protected set; } }")]
    [InlineData("class C { public int P { get; protected private set; } }")]
    [InlineData("class C { public int this[int n] { get { return 1; } private protected set {} } }")]
    public void Roslyn_enables_the_pair_in_csharp72(string source)
    {
        Assert.Contains(Errors(source, LanguageVersion.CSharp6), error => error.Id == "CS8059");
        Assert.Contains(Errors(source, LanguageVersion.CSharp7_1), error => error.Id == "CS8302");
        Assert.Empty(Errors(source, LanguageVersion.CSharp7_2));
        Assert.Empty(Errors(source, LanguageVersion.CSharp7_3));
    }

    [Fact]
    public void Protected_internal_already_compiles_in_csharp6() =>
        Assert.Empty(Errors("class C { protected internal int x; }", LanguageVersion.CSharp6));

    [Theory]
    [InlineData("class C { public private int x; }", "CS0107")]
    [InlineData("class C { private protected internal int x; }", "CS0107")]
    [InlineData("struct C { private protected int x; }", "CS0666")]
    [InlineData("static class C { private protected static int x; }", "CS1057")]
    public void Roslyn_checks_accessibility_combinations_and_containing_types(string source, string code) =>
        Assert.Contains(Errors(source, LanguageVersion.CSharp7_2), error => error.Id == code);

    private static IEnumerable<Diagnostic> Errors(string source, LanguageVersion version) =>
        Oracle.Observe(source, version).CompilationDiagnostics
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
