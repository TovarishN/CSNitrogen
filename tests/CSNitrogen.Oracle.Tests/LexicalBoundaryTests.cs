using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class LexicalBoundaryTests
{
    public static IEnumerable<object[]> DirectiveLikeText()
    {
        yield return ["class C { string S = @\"\n#if X\n#endif\n\"; }",0];
        yield return ["class C { string S = \"\"\"\n#if X\n#endif\n\"\"\"; }",0];
        yield return ["/*\n#if X\n#endif\n*/ class C {}",0];
        yield return ["#if false\n\"unterminated disabled text\n#endif\nclass C {}",2];
    }
    [Theory][MemberData(nameof(DirectiveLikeText))]
    public void Directive_boundaries_are_lexically_sensitive(string source,int directives)
    {
        var observation=Oracle.Observe(source,LanguageVersion.CSharp14);
        Assert.DoesNotContain(observation.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
        Assert.Equal(directives,observation.Tree.GetRoot().DescendantTrivia(descendIntoTrivia:true).Count(t=>t.IsDirective));
    }
    [Theory][InlineData("\n")][InlineData("\r\n")]
    public void Bom_and_supplementary_text_preserve_utf16_offsets(string newline)
    {
        string source="\uFEFF/* 🧪 */"+newline+"class C {}";
        var observation=Oracle.Observe(source,LanguageVersion.CSharp14);
        Assert.DoesNotContain(observation.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
        Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),observation.Tree.GetRoot().DescendantTokens().Single(t=>t.Text=="C").SpanStart);
    }
    [Fact] public void Arbitrary_raw_delimiter_is_valid_CSharp_beyond_the_finite_probe()
    {
        string source="class C { string S = "+new string('"',5)+"a"+new string('"',5)+"; }";
        Assert.DoesNotContain(Oracle.Observe(source,LanguageVersion.CSharp14).SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
    }
}
