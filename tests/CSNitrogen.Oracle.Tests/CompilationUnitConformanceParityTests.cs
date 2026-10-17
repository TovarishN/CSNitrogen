using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSNitrogen.Oracle.Tests;

public class CompilationUnitConformanceParityTests
{
    public static IEnumerable<object[]> Files()
    {
        string[] usings = ["", "using A;", "using A.B;using Q=X.Y;"];
        string[] attributes = ["", "[assembly:A]", "[assembly:A(1,Name=2),B,][module:C()]"];
        string[] members = ["", "class C{}", "struct S{}interface I{}enum E{A,B}delegate void D(int x);",
            "namespace N{}", "namespace A.B{using Q=X.Y;class C{[A] int x=1,y;void M(){F();}}namespace K{struct S{int x;}interface I{void M(ref int x);}}}",
            "class C{class Inner{int x;}struct S{}delegate void D();}namespace N{class Hidden{}}class Last{}"];
        foreach (string u in usings)
            foreach (string a in attributes)
                foreach (string m in members) yield return [u + a + m];
        foreach (string source in new[] {
            "// empty\r\n/*🧪*/", "namespace N{};namespace N{class C{}};",
            "using @using=Names.@class;namespace @namespace.A{using X=Q.R;class @class{int @int;}}",
            "using /*u*/ A . /*n*/ B;[assembly: /*a*/ A] namespace /*n*/ A . B{using Q = /*q*/ R.S;class C{int x=1;}}",
            "[A]class C{}namespace N{[B]struct S{int x;}}",
            "namespace A{namespace B{namespace C{class D{void M(){int x=1;F();}}}}}",
            "namespace N{enum E{}delegate T D([A]ref T x,params T[] rest);}",
            "namespace N{class C{int[] x={1,2};void M(ref int x){if(x>0)F();}}}"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(Files))]
    public void Ordered_root_and_namespace_projections_match(string source)
    {
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "units.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        Assert.True(ours.ProfileComplete);
        CompilationUnitShapeAssert.Compare((CompilationUnitSyntax)reference.GetRoot(), ours);
    }

    [Theory]
    [InlineData("class C{} using A;")]
    [InlineData("[assembly:A] using B;")]
    [InlineData("namespace N{class C{}using A;}")]
    [InlineData("namespace N{namespace K{}using A;}")]
    [InlineData("namespace N;class C{}")]
    [InlineData("global using A;")]
    [InlineData("using static A;")]
    [InlineData("extern alias A;")]
    [InlineData("using A.;")]
    [InlineData("using X=;")]
    [InlineData("using ;")]
    [InlineData("namespace A.{}")]
    [InlineData("namespace {class C{}}")]
    [InlineData("namespace N{void M(){}}")]
    [InlineData("void M(){}")]
    [InlineData("int x;")]
    [InlineData("namespace N{class C{}")]
    [InlineData("namespace N{using A}")]

    [InlineData("[A]namespace N{}")]
    [InlineData("namespace N{[assembly:A]}")]
    [InlineData("class C{using A;}")]
    [InlineData("class C{namespace N{}}")]
    public void Invalid_or_later_source_contexts_are_rejected(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "invalid-units.cs", []));
        Assert.False(ours.Accepted);
        Assert.Contains(ours.Issues, i => i.Severity == Nitrogen.DiagnosticSeverity.Error);
        Assert.All(ours.Issues, i => { Assert.InRange(i.Span.Start, 0, source.Length); Assert.InRange(i.Span.End, i.Span.Start, source.Length); Assert.False(string.IsNullOrWhiteSpace(i.Message)); });
    }

    public static IEnumerable<object[]> MisplacedAttributes()
    {
        string[] targets = ["assembly", "module", "@assembly", "\\u0061ssembly", "as\u200dsembly", "\\U0000006dodule"];
        string[] declarations = ["class C{}", "struct S{}", "interface I{}", "enum E{}", "delegate void D();"];
        foreach (string target in targets)
            foreach (string declaration in declarations)
                foreach (string container in new[] { "class First{{}} {0}", "namespace N{{{0}}}", "namespace N{{class First{{}} {0}}}" })
                    yield return [string.Format(container, "[" + target + ":A]" + declaration), target];
    }

    [Theory, MemberData(nameof(MisplacedAttributes))]
    public void Misplaced_global_attributes_match_reference_syntax_diagnostics(string source, string target)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.SyntaxDiagnostics, d => d.Id == "CS1730");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "placement.cs", []));
        Assert.False(ours.Accepted);
        var issue = Assert.Single(ours.Issues, i => i.Code == "CSN2012");
        Assert.Equal(source.IndexOf(target, StringComparison.Ordinal), issue.Span.Start);
        Assert.Equal(target.Length, issue.Span.Length);
        Assert.Equal(IssueCategory.Syntax, issue.Category);
    }

    [Theory]
    [InlineData("class C{[assembly:A]void M(){}}")]
    [InlineData("class C{[assembly:A]class Nested{}}")]
    [InlineData("class C{void M([module:A]int x){}}")]
    public void Local_target_applicability_remains_a_compiler_observation(string source)
    {
        var reference=Oracle.Observe(source,LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics,d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics,d=>d.Id=="CS0657");
        using var ours=CSharpParser.Parse(new(source,"CSharp1","local-targets.cs",[]));
        Assert.True(ours.Accepted,string.Join(";",ours.Issues));
    }

    [Theory]
    [InlineData("\n", false)]
    [InlineData("\r\n", false)]
    [InlineData("\r", false)]
    [InlineData("\n", true)]
    [InlineData("\r\n", true)]
    [InlineData("\r", true)]
    public void Active_members_keep_authored_offsets_through_directives(string newline, bool enabled)
    {
        string source = string.Join(newline, new[] { "#define LOCAL", "using A;", "[assembly:A]", "namespace N{", "#if FEATURE && LOCAL", "using X=Q.R;class Active{int x=1;void M(){F();}}", "#else", "using Y=Q.S;struct Other{int y;}", "#endif", "#line 200 \"mapped.cs\"", "namespace Inner{class Nested{}}", "}", "class Global{}", "#if false", "malformed <<< class Hidden{[assembly:A]class Bad{}", "#endif", "" });
        string[] symbols = enabled ? ["FEATURE"] : [];
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1, preprocessorSymbols: symbols));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "directives.cs", symbols));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        Assert.Equal(source.Length, ours.MaskedText.Length);
        CompilationUnitShapeAssert.Compare((CompilationUnitSyntax)reference.GetRoot(), ours);
        foreach (var declaration in reference.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            var expected = reference.GetMappedLineSpan(declaration.Identifier.Span);
            var mapped = ours.MapLocation(declaration.Identifier.SpanStart);
            Assert.Equal(expected.StartLinePosition.Line, mapped.LogicalLine);
            Assert.Equal(expected.StartLinePosition.Character, mapped.Character);
            if (expected.Path == "mapped.cs") Assert.Equal(expected.Path, mapped.File);
        }
        Assert.NotEmpty(ours.InactiveSpans); Assert.NotEmpty(ours.Directives);
    }
}
