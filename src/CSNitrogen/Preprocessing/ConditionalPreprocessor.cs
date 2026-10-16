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
            if(supplied is null || !ConditionalSymbols.TryNormalize(supplied,out var name))issues.Add(new("CSN1101","Invalid supplied conditional symbol.",new(0,0),IssueCategory.Preprocessing));
            else symbols.Add(name);
        }
        var scanner=new DirectiveScanner(request.Source);var branches=new Stack<Branch>();var directives=new List<DirectiveRecord>();
        var inactive=new List<TextSpan>();char[] masked=request.Source.ToCharArray();
        var effects=new DirectiveEffects(request.Version,issues);
        bool Active()=>branches.Count==0||branches.Peek().Active;
        void Mask(SourceLine line){for(int i=line.Start;i<line.End;i++)masked[i]=' ';}
        void Error(string code,string message,DirectiveRecord d)=>issues.Add(new(code,message,d.Span,IssueCategory.Preprocessing));
        bool Condition(DirectiveRecord d)
        {
            var result=ConditionalExpression.Evaluate(d.Argument,symbols);
            if(result.Error>=0)issues.Add(new("CSN1102",result.Message!,new(Math.Min(request.Source.Length,d.ArgumentStart+result.Error),0),IssueCategory.Preprocessing));
            return result.Value;
        }
        foreach(var line in SourceLines.Enumerate(request.Source))
        {
            bool active=Active();var d=scanner.ScanLine(line,active);
            if(d is null){if(!active){Mask(line);inactive.Add(new(line.Start,line.End-line.Start));}continue;}
            directives.Add(d);Mask(line);
            string argument=DirectiveArguments.WithoutComment(d.Argument).Trim();
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
                    if(!ConditionalSymbols.TryNormalize(argument,out var name))Error("CSN1101","Invalid conditional symbol.",d);
                    else if(active)
                    {
                        if(scanner.SawToken)Error("CSN1105","#define and #undef must precede source tokens.",d);
                        else if(d.Kind=="define")symbols.Add(name);else symbols.Remove(name);
                    }
                    break;
                default:effects.Apply(d,branches.Count);break;
            }
        }
        scanner.Finish();effects.Finish();issues.AddRange(scanner.Issues);
        foreach(var branch in branches)issues.Add(new("CSN1103","Expected #endif.",new(branch.Start,0),IssueCategory.Preprocessing));
        return new(request.Source,new(masked),directives,inactive,issues,effects.Mappings);
    }
}
