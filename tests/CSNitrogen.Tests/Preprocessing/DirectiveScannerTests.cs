using CSNitrogen.Preprocessing;
namespace CSNitrogen.Tests.Preprocessing;
public class DirectiveScannerTests
{
    [Theory][InlineData(" #if X",1,false)][InlineData("class C {} #if X",1,true)]
    [InlineData("@\"multiline\n#if X\n#endif\"",0,false)][InlineData("/*\n#if X\n#endif\n*/",0,false)]
    [InlineData("\"a\\\"#if X\"",0,false)][InlineData("'\\\'' // #if X",0,false)]
    public void Active_lexical_boundaries(string source,int expected,bool placementError)
    {
        var scan=DirectiveScanner.Scan(source);
        Assert.Equal(expected,scan.Directives.Count);
        Assert.Equal(placementError,scan.Issues.Any(i=>i.Code=="CSN1001"));
        foreach(var d in scan.Directives)Assert.Equal('#',source[d.Span.Start]);
    }
    [Theory][InlineData("\r")][InlineData("\n")][InlineData("\r\n")][InlineData("\u0085")][InlineData("\u2028")][InlineData("\u2029")]
    public void All_newlines_and_bom_preserve_spans(string newline)
    {
        string source="\uFEFF/* 🧪 */"+newline+"  #if X";
        var scan=DirectiveScanner.Scan(source);var d=Assert.Single(scan.Directives);
        Assert.Equal(source.IndexOf('#'),d.Span.Start);Assert.Equal(1,d.Line);Assert.Equal("if",d.Kind);
    }
    [Theory][InlineData("/*unterminated")][InlineData("\"unterminated")][InlineData("'x")][InlineData("@\"unterminated")]
    public void Unterminated_lexical_states_report_errors(string source)
    { Assert.NotEmpty(DirectiveScanner.Scan(source).Issues); }
}
