using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSNitrogen.Oracle.Tests;

public class CSharp7ReleaseObservationTests
{
    private static readonly LanguageVersion[] Versions =
    [
        LanguageVersion.CSharp6,
        LanguageVersion.CSharp7,
        LanguageVersion.CSharp7_1,
        LanguageVersion.CSharp7_2,
        LanguageVersion.CSharp7_3,
        LanguageVersion.CSharp8,
    ];

    [Theory]
    [InlineData("typed default", LanguageVersion.CSharp6, "class C { int M() => default(int); }")]
    [InlineData("binary literal", LanguageVersion.CSharp7, "class C { int X = 0b1010; }")]
    [InlineData("digit separators", LanguageVersion.CSharp7, "class C { int X = 1_000; double Y = 1_000.0_5; }")]
    [InlineData("out variable", LanguageVersion.CSharp7, "class C { bool M() => int.TryParse(\"1\", out var x) && x > 0; }")]
    [InlineData("guarded type pattern", LanguageVersion.CSharp7, "class C { int M(object o) { switch (o) { case int x when x > 0: return x; default: return 0; } } }")]
    [InlineData("tuple and deconstruction", LanguageVersion.CSharp7, "class C { int M() { var (x, _) = (1, 2); return x; } }")]
    [InlineData("local function", LanguageVersion.CSharp7, "class C { int M() { int Twice(int x) => x * 2; return Twice(1); } }")]
    [InlineData("ref local and return", LanguageVersion.CSharp7, "class C { int f; ref int M() { ref int x = ref f; return ref x; } }")]
    [InlineData("expression-bodied constructor and accessors", LanguageVersion.CSharp7, "class C { int f; C() => f = 1; int P { get => f; set => f = value; } }")]
    [InlineData("throw expression", LanguageVersion.CSharp7, "class C { object M(object o) => o ?? throw new System.Exception(); }")]
    [InlineData("default literal", LanguageVersion.CSharp7_1, "class C { int M() => default; }")]
    [InlineData("inferred tuple name", LanguageVersion.CSharp7_1, "class C { int M() { int name = 1; var t = (name, 2); return t.name; } }")]
    [InlineData("pattern on a type parameter", LanguageVersion.CSharp7_1, "class C { bool M<T>(T value) => value is int i && i > 0; }")]
    [InlineData("in parameter and readonly ref return", LanguageVersion.CSharp7_2, "class C { int f; ref readonly int M(in int x) => ref f; }")]
    [InlineData("readonly ref struct", LanguageVersion.CSharp7_2, "readonly ref struct X { }")]
    [InlineData("ref conditional", LanguageVersion.CSharp7_2, "class C { int x, y; ref int M(bool b) => ref (b ? ref x : ref y); }")]
    [InlineData("private protected", LanguageVersion.CSharp7_2, "class C { private protected int x; }")]
    [InlineData("non-trailing named argument", LanguageVersion.CSharp7_2, "class C { void M(int a, int b) {} void N() => M(a: 0, 1); }")]
    [InlineData("non-trailing named index", LanguageVersion.CSharp7_2, "class C { public int this[int a, int b] => 1; int M() => this[a: 1, 2]; }")]
    [InlineData("non-trailing named base index", LanguageVersion.CSharp7_2, "class B { public int this[int a, int b] => 1; } class C : B { int M() => base[a: 1, 2]; }")]
    [InlineData("non-trailing conditional index", LanguageVersion.CSharp7_2, "class C { public int this[int a, int b] => 1; int? M() => this?[a: 1, 2]; }")]
    [InlineData("non-trailing index initializer", LanguageVersion.CSharp7_2, "class C { public int this[int a, int b] { set {} } C M() => new C { [a: 1, 2] = 3 }; }")]
    [InlineData("leading separators", LanguageVersion.CSharp7_2, "class C { int X = 0x_FF; int Y = 0b_01; }")]
    [InlineData("stackalloc local to Span", LanguageVersion.CSharp7_2, "class C { void M() { System.Span<int> s = stackalloc int[1]; } }")]
    [InlineData("stackalloc conditional local", LanguageVersion.CSharp7_2, "class C { void M(bool b) { System.Span<int> s = b ? stackalloc int[1] : stackalloc int[2]; } }")]
    [InlineData("stackalloc array initializer", LanguageVersion.CSharp7_3, "class C { void M() { System.Span<int> s = stackalloc[] { 1, 2 }; } }")]
    [InlineData("ref reassignment", LanguageVersion.CSharp7_3, "class C { void M() { int x = 0, y = 1; ref int r = ref x; r = ref y; } }")]
    [InlineData("unmanaged constraint", LanguageVersion.CSharp7_3, "class C<T> where T : unmanaged { }")]
    [InlineData("tuple equality", LanguageVersion.CSharp7_3, "class C { bool M() => (1, 2) == (1, 2); }")]
    [InlineData("out variable in field initializer", LanguageVersion.CSharp7_3, "class C { bool X = int.TryParse(\"1\", out var n) && n > 0; }")]
    [InlineData("stackalloc in an argument", LanguageVersion.CSharp8, "class C { static void N(System.Span<int> s) {} static void M() => N(stackalloc int[1]); }")]
    public void Observe_first_version_without_compilation_errors(string feature, LanguageVersion first, string source)
    {
        foreach (var version in Versions)
        {
            var errors = Oracle.Observe(source, version).CompilationDiagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
            bool expected = version >= first;
            Assert.True(expected == (errors.Length == 0),
                $"{feature} under {version}, expected compilation success: {expected}. Diagnostics: {string.Join<Diagnostic>(" | ", errors)}");
        }
    }

    [Fact]
    public void Trailing_digit_separator_is_invalid_across_the_boundary()
    {
        foreach (var version in Versions)
            Assert.Contains(Oracle.Observe("class C { int X = 123_; }", version).CompilationDiagnostics,
                diagnostic => diagnostic.Id == "CS1013");
    }

    [Fact]
    public void Backing_field_attribute_target_stops_warning_in_csharp73()
    {
        const string source = "class C { [field: System.NonSerialized] int P { get; set; } }";
        foreach (var version in Versions)
        {
            var diagnostics = Oracle.Observe(source, version).CompilationDiagnostics;
            Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            Assert.Equal(version < LanguageVersion.CSharp7_3,
                diagnostics.Any(diagnostic => diagnostic.Id == "CS8371" && diagnostic.Severity == DiagnosticSeverity.Warning));
        }
    }

    [Theory]
    [InlineData("stackalloc int[1]")]
    [InlineData("b ? stackalloc int[1] : stackalloc int[2]")]
    public void Stackalloc_assignment_reports_escape_diagnostic_after_parsing(string value)
    {
        string source = $"class C {{ void M(bool b) {{ System.Span<int> s = default; s = {value}; }} }}";
        var observation = Oracle.Observe(source, LanguageVersion.CSharp7_3);
        Assert.DoesNotContain(observation.SyntaxDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains(observation.CompilationDiagnostics, diagnostic => diagnostic.Id == "CS8353");
    }
}
