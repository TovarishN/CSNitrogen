using System.Text;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Preprocessing;
public static class ConditionalPreprocessor
{
    private sealed class Branch(bool parent,bool matched,int start)
    { internal bool Parent=parent,Matched=matched,Active=parent&&matched,Else;internal int Start=start; }
    public static PreprocessResult Process(PreprocessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);ArgumentNullException.ThrowIfNull(request.Source);
        ArgumentNullException.ThrowIfNull(request.Version);ArgumentNullException.ThrowIfNull(request.ConditionalSymbols);
        var symbols=new HashSet<string>(StringComparer.Ordinal);var issues=new List<ParseIssue>();
        foreach(var supplied in request.ConditionalSymbols)
        {
            if(supplied is null || !ConditionalSymbols.TryNormalize(supplied,request.Version,out var name))issues.Add(new("CSN1101","Invalid supplied conditional symbol.",new(0,0),IssueCategory.Preprocessing));
            else symbols.Add(name);
        }
        string lexicalSource=request.Version.Major<=3&&request.Source.EndsWith('\u001a')?request.Source[..^1]+" ":request.Source;
        var scanner=new DirectiveScanner(lexicalSource,request.Version);var branches=new Stack<Branch>();var directives=new List<DirectiveRecord>();
        var inactive=new List<TextSpan>();char[] masked=lexicalSource.ToCharArray();
        var effects=new DirectiveEffects(request.Version,issues);var openLines=new List<OpenLine>();
        bool Active()=>branches.Count==0||branches.Peek().Active;
        void Mask(SourceLine line){for(int i=line.Start;i<line.End;i++)masked[i]=' ';}
        void Error(string code,string message,DirectiveRecord d)=>issues.Add(new(code,message,d.Span,IssueCategory.Preprocessing));
        bool Condition(DirectiveRecord d)
        {
            var result=ConditionalExpression.Evaluate(d.Argument,symbols,request.Version);
            if(result.Error>=0)issues.Add(new("CSN1102",result.Message!,new(Math.Min(request.Source.Length,d.ArgumentStart+result.Error),0),IssueCategory.Preprocessing));
            return result.Value;
        }
        foreach(var line in SourceLines.Enumerate(lexicalSource,request.Version))
        {
            bool active=Active();
            if(scanner.LineState!=0)openLines.Add(new(line.Start,scanner.LineState,scanner.Opened));
            var d=scanner.ScanLine(line,active);
            if(d is null){if(!active){Mask(line);inactive.Add(new(line.Start,line.End-line.Start));}continue;}
            directives.Add(d);Mask(line);
            string argument=DirectiveArguments.WithoutComment(d.Argument,request.Version);
            switch(d.Kind)
            {
                case "if":branches.Push(new(active,Condition(d),d.Span.Start));break;
                case "elif":
                    effects.ConditionalBoundary(d,branches.Count);
                    bool condition=Condition(d);
                    if(branches.Count==0){Error("CSN1103","Unmatched #elif.",d);break;}
                    var elif=branches.Peek();if(elif.Else)Error("CSN1103","#elif cannot follow #else.",d);
                    elif.Active=elif.Parent&&!elif.Matched&&condition&&!elif.Else;elif.Matched|=condition;break;
                case "else":
                    effects.ConditionalBoundary(d,branches.Count);
                    if(argument.Length>0)Error("CSN1104","Unexpected text after #else.",d);
                    if(branches.Count==0){Error("CSN1103","Unmatched #else.",d);break;}
                    var other=branches.Peek();if(other.Else)Error("CSN1103","Duplicate #else.",d);
                    other.Active=other.Parent&&!other.Matched&&!other.Else;other.Else=true;other.Matched=true;break;
                case "endif":
                    effects.ConditionalBoundary(d,branches.Count);
                    if(argument.Length>0)Error("CSN1104","Unexpected text after #endif.",d);
                    if(!branches.TryPop(out _))Error("CSN1103","Unmatched #endif.",d);break;
                case "define":case "undef":
                    if(!ConditionalSymbols.TryNormalize(argument,request.Version,out var name))Error("CSN1101","Invalid conditional symbol.",d);
                    else if(active)
                    {
                        if(scanner.SawToken)Error("CSN1105","#define and #undef must precede source tokens.",d);
                        else if(d.Kind=="define")symbols.Add(name);else symbols.Remove(name);
                    }
                    break;
                default:effects.Apply(d,branches.Count);break;
            }
        }
        foreach(int position in scanner.HistoricalWhitespace)masked[position]=' ';
        scanner.Finish();effects.Finish();issues.AddRange(scanner.Issues);
        foreach(var branch in branches)issues.Add(new("CSN1103","Expected #endif.",new(branch.Start,0),IssueCategory.Preprocessing));
        return new(request.Source,new(masked),directives,inactive,issues,effects.Mappings,request.Version)
            {OpenLines=openLines.ToArray(),Symbols=request.ConditionalSymbols.ToArray()};
    }

    /// <summary>
    /// Preprocesses <paramref name="request"/>'s source, an edit of <paramref name="previous"/>'s by
    /// <paramref name="changes"/> (old-text coordinates, sorted, non-overlapping). The result always equals
    /// <see cref="Process"/>'s. When the edit cannot change the directive structure it is spliced: only the edited
    /// lines are rescanned (from the lexical state they started in), and everything else is moved.
    /// </summary>
    public static PreprocessResult Reprocess(PreprocessResult previous,PreprocessRequest request,ReadOnlySpan<TextChange> changes)
        =>Reprocess(previous,request,changes,out _);

    /// <param name="spliced">True when the result was spliced; false when the source was preprocessed in full.</param>
    public static PreprocessResult Reprocess(PreprocessResult previous,PreprocessRequest request,ReadOnlySpan<TextChange> changes,out bool spliced)
    {
        ArgumentNullException.ThrowIfNull(previous);ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Source);ArgumentNullException.ThrowIfNull(request.Version);ArgumentNullException.ThrowIfNull(request.ConditionalSymbols);
        spliced=false;
        var result=TrySplice(previous,request,changes);
        if(result is null)return Process(request);
        spliced=true;
        return result;
    }

    /// <summary>The spliced result, or null when the edit could change what <see cref="Process"/> computes beyond the edited lines.</summary>
    static PreprocessResult? TrySplice(PreprocessResult previous,PreprocessRequest request,ReadOnlySpan<TextChange> changes)
    {
        string old=previous.OriginalText,text=request.Source;var version=request.Version;
        if(changes.Length==0||version!=previous.Version||!request.ConditionalSymbols.SequenceEqual(previous.Symbols))return null;
        // Issues would need re-attributing; and a historical end-of-file marker changes the lexical source.
        if(previous.Issues.Count>0)return null;
        if(version.Major<=3&&(old.EndsWith('\u001a')||text.EndsWith('\u001a')))return null;
        int delta=0,end=0;
        foreach(var c in changes)
        {
            if(c.Start<end||c.OldLength<0||c.NewLength<0||c.Start+c.OldLength>old.Length)return null;
            end=c.Start+c.OldLength;delta+=c.NewLength-c.OldLength;
        }
        if(old.Length+delta!=text.Length)return null;
        bool historical=version.Major==1;
        // The edited lines, whole: from the start of the line holding the first change to past the line break ending
        // the line holding the last change's end. Nothing after them moves except by delta.
        int start=LineStart(old,changes[0].Start,historical),oldEnd=LineEnd(old,end,historical),newEnd=oldEnd+delta;
        foreach(var d in previous.Directives)
        {
            if(d.Span.Start<oldEnd&&d.Span.End>=start)return null;
            if(d.Kind is "define" or "undef"&&d.Span.Start>=start)return null; // a token before them changes their meaning
        }
        foreach(var span in previous.InactiveSpans)if(span.Start<oldEnd&&span.End>=start)return null;
        var (startState,startOpened)=StateAt(previous.OpenLines,start);
        var (endState,endOpened)=StateAt(previous.OpenLines,oldEnd);
        // Rescan the edited lines from the state they started in; they must hold no directive, raise no issue,
        // and leave the state the following line started in before.
        var scanner=new DirectiveScanner(text,version);scanner.Resume(startState,startOpened);
        var open=new List<OpenLine>();
        foreach(var line in Lines(text,version,start,newEnd))
        {
            if(scanner.LineState!=0)open.Add(new(line.Start,scanner.LineState,scanner.Opened));
            if(scanner.ScanLine(line,true) is not null)return null;
        }
        if(scanner.Issues.Count>0||scanner.LineState!=endState||(endState!=0&&(scanner.Opened!=endOpened||endOpened>=start)))return null;
        var masked=new StringBuilder(text.Length);
        masked.Append(previous.MaskedText,0,start).Append(text,start,newEnd-start).Append(previous.MaskedText,oldEnd,old.Length-oldEnd);
        foreach(int position in scanner.HistoricalWhitespace)masked[position]=' ';
        int lineDelta=LineBreaks(text,start,newEnd,historical)-LineBreaks(old,start,oldEnd,historical);
        var directives=previous.Directives.Select(d=>d.Span.Start<start?d:d with{Span=new(d.Span.Start+delta,d.Span.Length),ArgumentStart=d.ArgumentStart+delta,Line=d.Line+lineDelta});
        var inactive=previous.InactiveSpans.Select(s=>s.Start<start?s:new TextSpan(s.Start+delta,s.Length));
        var mappings=previous.ExactLineMappings.Select(m=>m.DirectiveSpan.Start<start?m
            :m with{PhysicalLine=m.PhysicalLine+lineDelta,DirectiveSpan=new(m.DirectiveSpan.Start+delta,m.DirectiveSpan.Length)});
        var openLines=previous.OpenLines.Where(o=>o.Start<start).Concat(open).Concat(previous.OpenLines.Where(o=>o.Start>=oldEnd).Select(o=>o with{Start=o.Start+delta}));
        return new(text,masked.ToString(),directives,inactive,[],mappings,version){OpenLines=openLines.ToArray(),Symbols=previous.Symbols};
    }

    static (int State,int Opened) StateAt(OpenLine[] lines,int start)
    {
        int lo=0,hi=lines.Length;
        while(lo<hi){int mid=(lo+hi)>>>1;if(lines[mid].Start<start)lo=mid+1;else hi=mid;}
        return lo<lines.Length&&lines[lo].Start==start?(lines[lo].State,lines[lo].Opened):(0,0);
    }

    /// <summary>The start of the line holding <paramref name="position"/>, or of the line before when that line ends with a CR.</summary>
    static int LineStart(string text,int position,bool historical)
    {
        int i=Math.Min(position,text.Length);
        while(i>0&&!CSharpTrivia.IsNewline(text[i-1],historical))i--;
        // After a CR, start a line earlier: an edit here can join that CR with a following LF into one line break
        // (or split such a pair), and only then do the line breaks counted over the edited lines see it.
        if(i>0&&text[i-1]=='\r')return LineStart(text,i-1,historical);
        return i;
    }

    /// <summary>Just past the line break ending the line holding <paramref name="position"/>, or the end of the text.</summary>
    static int LineEnd(string text,int position,bool historical)
    {
        int i=position;
        while(i<text.Length&&!CSharpTrivia.IsNewline(text[i],historical))i++;
        if(i==text.Length)return i;
        return text[i]=='\r'&&i+1<text.Length&&text[i+1]=='\n'?i+2:i+1;
    }

    static int LineBreaks(string text,int start,int end,bool historical)
    {
        int count=0;
        for(int i=start;i<end;i++)
        {
            if(!CSharpTrivia.IsNewline(text[i],historical))continue;
            count++;
            if(text[i]=='\r'&&i+1<end&&text[i+1]=='\n')i++;
        }
        return count;
    }

    /// <summary>The source lines from <paramref name="start"/> (a line start) up to <paramref name="end"/> (a line start or the end).</summary>
    static IEnumerable<SourceLine> Lines(string text,Version version,int start,int end)
    {
        bool historical=version.Major==1;int position=start;
        do
        {
            int lineStart=position;
            while(position<text.Length&&!CSharpTrivia.IsNewline(text[position],historical))position++;
            int lineEnd=position;
            if(position<text.Length){if(text[position++]=='\r'&&position<text.Length&&text[position]=='\n')position++;}
            yield return new(lineStart,lineEnd,position,0);
            if(position==lineEnd)break;
        }while(position<end);
    }
}
