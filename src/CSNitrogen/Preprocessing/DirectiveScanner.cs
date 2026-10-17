using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Preprocessing;
public sealed record DirectiveScanResult(IReadOnlyList<DirectiveRecord> Directives,IReadOnlyList<ParseIssue> Issues);
/// <summary>Scans directive boundaries, not C# declarations or expressions.</summary>
public sealed class DirectiveScanner
{
    private enum State { Normal, BlockComment, RegularString, VerbatimString, Character }
    private readonly string _source;
    private readonly Version _version;
    private State _state;
    private int _opened;
    internal List<ParseIssue> Issues {get;}=[];
    internal List<int> HistoricalWhitespace {get;}=[];
    internal bool SawToken {get;private set;}
    internal DirectiveScanner(string source,Version? version=null){_source=source;_version=version??new(14,0);}
    public static DirectiveScanResult Scan(string source)
    {
        ArgumentNullException.ThrowIfNull(source);var scanner=new DirectiveScanner(source);var directives=new List<DirectiveRecord>();
        foreach(var line in SourceLines.Enumerate(source))if(scanner.ScanLine(line,true) is {} directive)directives.Add(directive);
        scanner.Finish();return new(Array.AsReadOnly(directives.ToArray()),Array.AsReadOnly(scanner.Issues.ToArray()));
    }
    internal DirectiveRecord? ScanLine(SourceLine line,bool active)
    {
        bool prefix=true;
        if(!active)_state=State.Normal;
        for(int i=line.Start;i<line.End;i++)
        {
            char c=_source[i];
            if(!active)
            {
                if(UnicodePolicy.IsWhiteSpace(c,_version)||(i==0&&c=='\uFEFF'))continue;
                return c=='#'?ReadDirective(i,line,false):null;
            }
            if(_state==State.BlockComment)
            { prefix=false;if(c=='*'&&i+1<line.End&&_source[i+1]=='/'){_state=State.Normal;i++;}continue; }
            if(_state==State.VerbatimString)
            { prefix=false;if(c=='"'){if(i+1<line.End&&_source[i+1]=='"')i++;else _state=State.Normal;}continue; }
            if(_state is State.RegularString or State.Character)
            {
                prefix=false;if(c=='\\'){i++;continue;}
                if(c==(_state==State.RegularString?'"':'\''))_state=State.Normal;
                continue;
            }
            if(UnicodePolicy.IsWhiteSpace(c,_version)||(i==0&&c=='\uFEFF'))
            {
                if(_version.Major<=2&&!char.IsWhiteSpace(c)&&c!='\uFEFF')HistoricalWhitespace.Add(i);
                continue;
            }
            if(c=='#')
            {
                if(!prefix)Issues.Add(new("CSN1001","A directive must be the first non-whitespace item on its line.",new(i,1),IssueCategory.Preprocessing));
                return ReadDirective(i,line,true);
            }
            prefix=false;
            if(c=='/'&&i+1<line.End)
            {
                if(_source[i+1]=='/')return null;
                if(_source[i+1]=='*'){_state=State.BlockComment;_opened=i;i++;continue;}
            }
            SawToken=true;
            if(c=='@'&&i+1<line.End&&_source[i+1]=='"'){_state=State.VerbatimString;_opened=i;i++;}
            else if(c=='"'){_state=State.RegularString;_opened=i;}
            else if(c=='\''){_state=State.Character;_opened=i;}
        }
        if(_state is State.RegularString or State.Character)
        { Issues.Add(Unterminated(line.End));_state=State.Normal; }
        return null;
    }
    private DirectiveRecord ReadDirective(int hash,SourceLine line,bool active)
    {
        int i=hash+1;while(i<line.End&&UnicodePolicy.IsWhiteSpace(_source[i],_version))i++;
        int start=i;while(i<line.End&&IsDirectiveNamePart(_source[i]))i++;
        string kind=_source[start..i];
        bool separated=i<line.End&&UnicodePolicy.IsWhiteSpace(_source[i],_version);
        if(_version.Major<=2 || kind=="pragma" && _version>=new Version(2,0))
        {
            bool requires=kind is "if" or "elif" or "define" or "undef" or "line" || kind=="pragma" && i<line.End;
            bool message=kind is "error" or "warning" or "region" or "endregion";
            if(!separated && (requires || (message && i<line.End)))
                Issues.Add(new("CSN1001","Whitespace is required before this directive argument.",new(i,0),IssueCategory.Preprocessing));
        }
        while(i<line.End&&UnicodePolicy.IsWhiteSpace(_source[i],_version))i++;
        return new(kind,_source[i..line.End],new(hash,line.End-hash),i,line.Number,active);
    }
    private bool IsDirectiveNamePart(char c)=>c=='\\'||UnicodePolicy.IsPart(c,_version);
    private ParseIssue Unterminated(int end)=>new("CSN1002","Unterminated string, character, or block comment.",new(_opened,end-_opened),IssueCategory.Syntax);
    internal void Finish(){if(_state!=State.Normal)Issues.Add(Unterminated(_source.Length));}
}
