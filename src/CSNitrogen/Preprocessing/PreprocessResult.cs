using System.Numerics;
using Nitrogen;
using CSNitrogen.Parsing;
namespace CSNitrogen.Preprocessing;
public sealed record DirectiveRecord(string Kind, string Argument, TextSpan Span, int ArgumentStart, int Line, bool Active);
public sealed record LineMapping(int PhysicalLine, int? LogicalLine, string? File, bool Hidden);
public sealed record MappedLocation(int PhysicalLine, int Character, int LogicalLine, string? File, bool Hidden);
/// <summary>Exact zero-based logical coordinate. A historical #line 0 maps to -1.</summary>
public sealed record ExactMappedLocation(int PhysicalLine, int Character, BigInteger LogicalLine, string? File, bool Hidden)
{
    /// <summary>One-based line number; zero is permitted by the historical directive grammar.</summary>
    public BigInteger LineNumber => LogicalLine + BigInteger.One;
}
/// <summary>Exact mapping beginning at PhysicalLine; null LogicalLine restores physical coordinates.
/// NumberSpelling retains authored decimal digits (including leading zeros); DirectiveSpan uses original UTF-16 offsets.</summary>
public sealed record ExactLineMapping(int PhysicalLine, BigInteger? LogicalLine, string? File, bool Hidden,
    string? NumberSpelling, TextSpan DirectiveSpan);
public sealed class PreprocessResult
{
    internal PreprocessResult(string source,string masked,IEnumerable<DirectiveRecord> directives,IEnumerable<TextSpan> inactive,
        IEnumerable<ParseIssue> issues,IEnumerable<ExactLineMapping> mappings,Version version)
    { _version=version;OriginalText=source;MaskedText=masked;Directives=Array.AsReadOnly(directives.ToArray());InactiveSpans=Array.AsReadOnly(inactive.ToArray());
      Issues=Array.AsReadOnly(issues.ToArray());ExactLineMappings=Array.AsReadOnly(mappings.ToArray()); }
    private readonly Version _version;
    /// <summary>The language version this text was preprocessed for.</summary>
    public Version Version=>_version;
    /// <summary>Line starts where the scan carried a block comment or verbatim string in (for incremental reprocessing).</summary>
    internal OpenLine[] OpenLines {get;init;}=[];
    /// <summary>The conditional symbols supplied for this text.</summary>
    internal string[] Symbols {get;init;}=[];
    public string OriginalText {get;}
    public string MaskedText {get;}
    public IReadOnlyList<DirectiveRecord> Directives {get;}
    public IReadOnlyList<TextSpan> InactiveSpans {get;}
    public IReadOnlyList<ParseIssue> Issues {get;}
    public IReadOnlyList<ExactLineMapping> ExactLineMappings {get;}
    /// <summary>Compatibility projection. Throws OverflowException if any logical line is outside Int32.</summary>
    public IReadOnlyList<LineMapping> LineMappings => Array.AsReadOnly(ExactLineMappings.Select(m=>
        new LineMapping(m.PhysicalLine,m.LogicalLine is { } n?checked((int)n):null,m.File,m.Hidden)).ToArray());
    public bool HasErrors=>Issues.Any(i=>i.Severity==DiagnosticSeverity.Error);
    /// <summary>Compatibility query with checked conversion of the exact zero-based logical line.</summary>
    public MappedLocation MapLocation(int offset)
    {
        var exact=MapLocationExact(offset);
        return new(exact.PhysicalLine,exact.Character,checked((int)exact.LogicalLine),exact.File,exact.Hidden);
    }
    /// <summary>Returns false only for a logical-line overflow. Invalid offsets still throw.</summary>
    public bool TryMapLocation(int offset,out MappedLocation? location)
    {
        var exact=MapLocationExact(offset);
        if(exact.LogicalLine<int.MinValue || exact.LogicalLine>int.MaxValue){location=null;return false;}
        location=new(exact.PhysicalLine,exact.Character,(int)exact.LogicalLine,exact.File,exact.Hidden);
        return true;
    }
    public ExactMappedLocation MapLocationExact(int offset)
    {
        if(offset<0 || offset>OriginalText.Length)throw new ArgumentOutOfRangeException(nameof(offset));
        var line=SourceLines.Enumerate(OriginalText,_version).Last(l=>l.Start<=offset);
        var mapping=ExactLineMappings.LastOrDefault(m=>m.PhysicalLine<=line.Number);
        BigInteger logical=mapping?.LogicalLine is { } n?n+line.Number-mapping.PhysicalLine:line.Number;
        return new(line.Number,offset-line.Start,logical,mapping?.File,mapping?.Hidden??false);
    }
}
internal sealed record SourceLine(int Start,int End,int After,int Number);
/// <summary>A line start at which the directive scan carried state <c>State</c> (opened at <c>Opened</c>) in.</summary>
internal readonly record struct OpenLine(int Start,int State,int Opened);
internal static class SourceLines
{
    internal static IEnumerable<SourceLine> Enumerate(string text,Version? version=null)
    {
        int position=0,number=0;
        do
        {
            int start=position;while(position<text.Length&&!CSharpTrivia.IsNewline(text[position],version?.Major==1))position++;
            int end=position;
            if(position<text.Length){if(text[position++]=='\r'&&position<text.Length&&text[position]=='\n')position++;}
            yield return new(start,end,position,number++);
            if(position==end)break;
        }while(position<=text.Length);
    }
}
