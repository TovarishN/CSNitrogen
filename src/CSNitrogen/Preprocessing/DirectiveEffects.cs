using System.Globalization;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Preprocessing;
internal sealed class DirectiveEffects(Version version,List<ParseIssue> issues)
{
    private readonly Stack<(int Depth,int Start)> _regions=[];
    internal List<LineMapping> Mappings {get;}=[];
    private void Error(string code,string message,DirectiveRecord d,IssueCategory category=IssueCategory.Preprocessing)=>issues.Add(new(code,message,d.Span,category));
    internal void ConditionalBoundary(DirectiveRecord d,int depth)
    {
        if(_regions.TryPeek(out var region)&&region.Depth>=depth)Error("CSN1201","A region cannot cross conditional sections.",d);
    }
    internal void Apply(DirectiveRecord d,int depth)
    {
        string argument=DirectiveArguments.WithoutComment(d.Argument,version);
        switch(d.Kind)
        {
            case "region":_regions.Push((depth,d.Span.Start));break;
            case "endregion":
                if(!_regions.TryPeek(out var top)||top.Depth!=depth)Error("CSN1201","Unmatched or overlapping #endregion.",d);
                else _regions.Pop();break;
            case "error":case "warning":
                if(d.Active)issues.Add(new(d.Kind=="error"?"CSN1202":"CSN1203",d.Argument.Trim(),d.Span,IssueCategory.Preprocessing,
                    d.Kind=="error"?DiagnosticSeverity.Error:DiagnosticSeverity.Warning));break;
            case "line":if(d.Active)ApplyLine(d,argument);break;
            case "nullable":if(d.Active)Later(d,8);break;
            case "pragma":if(d.Active)Later(d,2);break;
            case "" when d.Argument.StartsWith(':'):if(d.Active)Later(d,14);break;
            default:Error("CSN1204","Unknown preprocessing directive.",d);break;
        }
    }
    private void Later(DirectiveRecord d,int minimum)
    {
        if(version<new Version(minimum,0))Error("CSN1205",$"Directive requires C# {minimum}.",d,IssueCategory.UnavailableFeature);
        else Error("CSN1206","This later-version directive is not implemented.",d,IssueCategory.UnsupportedCoverage);
    }
    private void ApplyLine(DirectiveRecord d,string argument)
    {
        int physical=d.Line+1;
        var previous=Mappings.LastOrDefault();
        if(argument=="default")
        { if(d.Active)Mappings.Add(new(physical,null,null,false));return; }
        if(argument=="hidden")
        {
            if(d.Active)Mappings.Add(new(physical,previous?.LogicalLine is int logical?logical+physical-previous.PhysicalLine:null,previous?.File,true));return;
        }
        if(argument.StartsWith('(')){Later(d,10);return;}
        int end=0;while(end<argument.Length&&char.IsAsciiDigit(argument[end]))end++;
        if(end==0||!int.TryParse(argument.AsSpan(0,end),NumberStyles.None,CultureInfo.InvariantCulture,out int number)||number<1)
        {Error("CSN1207","Expected a positive #line number representable by Int32.",d);return;}
        bool ignored=number>=16707566;
        if(ignored)issues.Add(new("CSN1208","Line number exceeds the PDB representable limit; this mapping is ignored.",d.Span,IssueCategory.Preprocessing,DiagnosticSeverity.Warning));
        string? file=previous?.File;
        if(end<argument.Length)
        {
            if(!UnicodePolicy.IsWhiteSpace(argument[end],version)){Error("CSN1207","Whitespace is required before the mapped file name.",d);return;}
            string rest=UnicodePolicy.Trim(argument[end..],version);
            if(rest.Length<2||rest[0]!='"'||rest[^1]!='"'||rest[1..^1].Contains('"'))
            {Error("CSN1207","Expected a quoted mapped file name.",d);return;}
            file=rest[1..^1];
        }
        if(d.Active&&!ignored)Mappings.Add(new(physical,number-1,file,false));
    }
    internal void Finish()
    {
        foreach(var region in _regions)issues.Add(new("CSN1201","Expected #endregion.",new(region.Start,0),IssueCategory.Preprocessing));
    }
}
