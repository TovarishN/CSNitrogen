using Nitrogen;
using CSNitrogen.Parsing;
namespace CSNitrogen.Preprocessing;
public sealed record DirectiveRecord(string Kind, string Argument, TextSpan Span, int ArgumentStart, int Line, bool Active);
public sealed record LineMapping(int PhysicalLine, int? LogicalLine, string? File, bool Hidden);
public sealed record MappedLocation(int PhysicalLine, int Character, int LogicalLine, string? File, bool Hidden);
public sealed class PreprocessResult
{
    internal PreprocessResult(string source,string masked,IEnumerable<DirectiveRecord> directives,IEnumerable<TextSpan> inactive,
        IEnumerable<ParseIssue> issues,IEnumerable<LineMapping> mappings)
    { OriginalText=source;MaskedText=masked;Directives=Array.AsReadOnly(directives.ToArray());InactiveSpans=Array.AsReadOnly(inactive.ToArray());
      Issues=Array.AsReadOnly(issues.ToArray());LineMappings=Array.AsReadOnly(mappings.ToArray()); }
    public string OriginalText {get;}
    public string MaskedText {get;}
    public IReadOnlyList<DirectiveRecord> Directives {get;}
    public IReadOnlyList<TextSpan> InactiveSpans {get;}
    public IReadOnlyList<ParseIssue> Issues {get;}
    public IReadOnlyList<LineMapping> LineMappings {get;}
    public bool HasErrors=>Issues.Any(i=>i.Severity==DiagnosticSeverity.Error);
    public MappedLocation MapLocation(int offset)
    {
        if(offset<0 || offset>OriginalText.Length)throw new ArgumentOutOfRangeException(nameof(offset));
        var line=SourceLines.Enumerate(OriginalText).Last(l=>l.Start<=offset);
        var mapping=LineMappings.LastOrDefault(m=>m.PhysicalLine<=line.Number);
        int logical=mapping?.LogicalLine is int n?n+line.Number-mapping.PhysicalLine:line.Number;
        return new(line.Number,offset-line.Start,logical,mapping?.File,mapping?.Hidden??false);
    }
}
internal sealed record SourceLine(int Start,int End,int After,int Number);
internal static class SourceLines
{
    internal static IEnumerable<SourceLine> Enumerate(string text)
    {
        int position=0,number=0;
        do
        {
            int start=position;while(position<text.Length&&!CSharpTrivia.IsNewline(text[position]))position++;
            int end=position;
            if(position<text.Length){if(text[position++]=='\r'&&position<text.Length&&text[position]=='\n')position++;}
            yield return new(start,end,position,number++);
            if(position==end)break;
        }while(position<=text.Length);
    }
}
