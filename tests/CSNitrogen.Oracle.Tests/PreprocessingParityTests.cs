using CSNitrogen.Parsing;
using CSNitrogen.Preprocessing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class PreprocessingParityTests
{
    public static IEnumerable<object[]> Sources()
    {
        yield return ["#define tr\\u0075e\n#if tr\\u0075e\nclass C {}\n#endif"];
        yield return ["#define fals\\u0065\n#if fals\\u0065\nclass C {}\n#endif"];
        yield return ["#if tr\\u0075e\nclass C {}\n#endif"];
        yield return ["#if tr\u200Cue\nclass C {}\n#endif"];
        yield return ["#line 16707566 nonsense\nclass C {}"];
        yield return ["#line 16707567 \"unterminated\nclass C {}"];
        yield return ["#line 16707566 \"valid.cs\" junk\nclass C {}"];

        yield return ["#define X\n#if X\nclass A {}\n#else\nclass B {}\n#endif"];
        yield return ["#if false\n\"unterminated\n#endif\nclass C {}"];
        yield return ["#if true\n/*\n#else\n*/ class C {}\n#endif"];
        yield return ["#region group\nclass C {}\n#endregion"];
        yield return ["#define class\n#if class\nclass C {}\n#endif"];
        yield return ["#define \\u0058\n#if X\nclass C {}\n#endif"];
        yield return ["#if false\n#if F()\n#endif\n#endif"];
        yield return ["#if false\n#nullable enable\n#endif\nclass C {}"];
        yield return ["#if true\n#region group\n#else\n#endregion\n#endif"];
        yield return ["#if true\n#endif junk"];
        yield return ["#if true\nclass C {}"];
        yield return ["#error failed\nclass C {}"];
        yield return ["#warning text // content\nclass C {}"];
        yield return ["#warning123\nclass C {}"];
        yield return ["#warningΔ\nclass C {}"];
        yield return ["#line100\nclass C {}"];
        yield return ["#if false\n#define true\n#endif\nclass C {}"];
        yield return ["#if false\n#unknown\n#endif\nclass C {}"];
    }
    public static IEnumerable<object[]> HistoricalDifferences()
    {
        yield return ["#line 100 \"mapped.cs\"\nclass A {}\n#line hidden\nclass B {}\n#line default\nclass C {}"];
        yield return ["#if false\n#line nonsense\n#endif\nclass C {}"];
        yield return ["#warning\"text\"\nclass C {}"];
        yield return ["#if(true)\nclass C {}\n#endif"];
    }
    [Theory][MemberData(nameof(HistoricalDifferences))]
    public void Pinned_historical_directive_grammar_differs_from_current_compiler(string source)
    {
        using var historical=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        using var current=CSharpParser.Parse(new(source,"CSharp14","physical.cs",[]));
        Assert.False(historical.Accepted);
        Assert.True(current.Accepted);
        Assert.DoesNotContain(Oracle.Observe(source,LanguageVersion.CSharp1).CompilationDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
    }
    public static IEnumerable<object[]> HistoricalMappingDifferences()
    {
        yield return ["#line 16707566 \"valid.cs\"\nclass C {}"];
        yield return ["#line 16707566\nclass C {}"];
    }
    [Theory][MemberData(nameof(HistoricalMappingDifferences))]
    public void Historical_mapping_preserves_values_ignored_by_current_PDB_policy(string source)
    {
        using var historical=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        using var current=CSharpParser.Parse(new(source,"CSharp14","physical.cs",[]));
        Assert.True(historical.Accepted);Assert.True(current.Accepted);
        int offset=source.IndexOf("class",StringComparison.Ordinal);
        Assert.Equal(16707565,historical.MapLocation(offset).LogicalLine);
        var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        var identifier=oracle.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single().Identifier;
        Assert.Equal(oracle.Tree.GetMappedLineSpan(identifier.Span).StartLinePosition.Line,current.MapLocation(identifier.SpanStart).LogicalLine);
        Assert.Contains(current.Issues,i=>i.Code=="CSN1208");
        Assert.DoesNotContain(historical.Issues,i=>i.Code=="CSN1208");
    }
    [Theory][MemberData(nameof(Sources))]
    public void Public_pipeline_matches_active_source_and_directive_diagnostics(string source)
    {
        var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        using var doc=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        bool errors=oracle.CompilationDiagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error);
        Assert.True(!errors==doc.Accepted,source+"\nOracle: "+string.Join(";",oracle.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage()))+"\nOurs: "+string.Join(";",doc.Issues));
        if(!doc.Accepted)return;
        var classes=oracle.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();Assert.Equal(classes.Length,doc.Classes.Count);
        for(int i=0;i<classes.Length;i++)
        {
            Assert.Equal(classes[i].Identifier.Text,doc.Classes[i].Name.ToString());
            int offset=classes[i].Identifier.SpanStart;
            Assert.Equal(offset,doc.Tree.Span(doc.Classes[i].Name.Index).Start);
            var mapped=oracle.Tree.GetMappedLineSpan(classes[i].Identifier.Span);var ours=doc.MapLocation(offset);
            Assert.Equal(mapped.StartLinePosition.Line,ours.LogicalLine);Assert.Equal(mapped.StartLinePosition.Character,ours.Character);
            Assert.Equal(mapped.Path,ours.File??"");
        }
    }
    [Theory][InlineData("@\"text\n#if X\n#endif\"")][InlineData("/*\n#if X\n#endif\n*/")]
    public void Directive_looking_active_content_is_preserved(string fragment)
    {
        string source=fragment.StartsWith('@')?"class C { string S = "+fragment+"; }":fragment+"class C {}";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        Assert.False(result.HasErrors);Assert.Empty(result.Directives);Assert.Equal(source,result.MaskedText);
        Assert.DoesNotContain(oracle.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
    }
}
