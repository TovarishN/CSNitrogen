using CSNitrogen.Syntax;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Preprocessing;
internal static unsafe class ConditionalSymbols
{
    private static readonly Lazy<Language> Language=new(()=>CSharp1Composition.CreateBuilder().WithTrivia(&StandardTrivia.None).Build());
    internal static bool TryNormalize(string value,Version version,out string normalized)
    {
        normalized="";if(value.Length==0)return false;
        using var parsed=Language.Value.Parse(value,LexicalModule.ConditionalSymbolFragment);
        return parsed.Success&&!parsed.HasErrors&&UnicodePolicy.TryNormalize(value,version,false,out normalized);
    }
}
internal static class DirectiveArguments
{
    internal static string WithoutComment(string value,Version version)
    {
        bool quoted=false;
        for(int i=0;i+1<value.Length;i++)
        {
            if(value[i]=='"')quoted=!quoted;
            if(!quoted&&value[i]=='/'&&value[i+1]=='/')return UnicodePolicy.Trim(value[..i],version);
        }
        return UnicodePolicy.Trim(value,version);
    }
}
internal sealed class ConditionalExpression
{
    private readonly string _text;
    private readonly IReadOnlySet<string> _symbols;
    private int _position,_depth;
    private readonly Version _version;
    private ConditionalExpression(string text,IReadOnlySet<string> symbols,Version version){_text=text;_symbols=symbols;_version=version;}
    internal static (bool Value,int Error,string? Message) Evaluate(string text,IReadOnlySet<string> symbols,Version version)
    {
        var parser=new ConditionalExpression(DirectiveArguments.WithoutComment(text,version),symbols,version);
        try {bool value=parser.Or();parser.Skip();if(parser._position!=parser._text.Length)throw new FormatException("Unexpected directive expression text.");return(value,-1,null);}
        catch(FormatException e){return(false,parser._position,e.Message);}
    }
    private void Skip(){while(_position<_text.Length&&UnicodePolicy.IsWhiteSpace(_text[_position],_version))_position++;}
    private bool Match(string token){Skip();if(!_text.AsSpan(_position).StartsWith(token,StringComparison.Ordinal))return false;_position+=token.Length;return true;}
    private bool Or(){bool value=And();while(Match("||")){bool right=And();value|=right;}return value;}
    private bool And(){bool value=Equality();while(Match("&&")){bool right=Equality();value&=right;}return value;}
    private bool Equality()
    {
        bool value=Unary();while(true){if(Match("==")){bool right=Unary();value=value==right;}else if(Match("!=")){bool right=Unary();value=value!=right;}else return value;}
    }
    private bool Unary()
    {
        if(++_depth>256)throw new FormatException("Directive expression nesting exceeds 256.");
        try
        {
            if(Match("!"))return !Unary();
            if(Match("(")){bool value=Or();if(!Match(")"))throw new FormatException("Expected ')'.");return value;}
            Skip();int start=_position;
            while(_position<_text.Length&&!UnicodePolicy.IsWhiteSpace(_text[_position],_version)&&"!&|=()".IndexOf(_text[_position])<0)_position++;
            string token=_text[start.._position];
            if(token=="true")return true;if(token=="false")return false;
            if(!ConditionalSymbols.TryNormalize(token,_version,out var normalized)){_position=start;throw new FormatException("Expected a conditional symbol or boolean literal.");}
            // Current compiler compatibility decodes boolean spellings as literals.
            // The first edition treats escaped/formatting spellings as symbols.
            if(!UnicodePolicy.IsHistorical(_version))
            {if(normalized=="true")return true;if(normalized=="false")return false;}
            return _symbols.Contains(normalized);
        }
        finally {_depth--;}
    }
}
