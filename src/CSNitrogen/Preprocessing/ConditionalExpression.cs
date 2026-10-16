using System.Globalization;
using System.Text;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Preprocessing;
internal static unsafe class ConditionalSymbols
{
    private static readonly Lazy<Language> Language=new(()=>CSharp1Composition.CreateBuilder().WithTrivia(&StandardTrivia.None).Build());
    internal static bool TryNormalize(string value,out string normalized)
    {
        normalized="";if(value.Length==0)return false;
        using var parsed=Language.Value.Parse(value,LexicalModule.ConditionalSymbolFragment);
        if(!parsed.Success || parsed.HasErrors)return false;
        var decoded=new StringBuilder();
        for(int i=0;i<value.Length;i++)
        {
            char c=value[i];
            if(c=='\\')
            {
                int digits=value[i+1]=='u'?4:8;
                c=(char)int.Parse(value.AsSpan(i+2,digits),NumberStyles.HexNumber,CultureInfo.InvariantCulture);i+=digits+1;
            }
            if(char.GetUnicodeCategory(c)!=UnicodeCategory.Format)decoded.Append(c);
        }
        normalized=decoded.ToString();return true;
    }
}
internal static class DirectiveArguments
{
    internal static string WithoutComment(string value)
    {
        bool quoted=false;
        for(int i=0;i+1<value.Length;i++)
        {
            if(value[i]=='"')quoted=!quoted;
            if(!quoted&&value[i]=='/'&&value[i+1]=='/')return value[..i].TrimEnd();
        }
        return value.TrimEnd();
    }
}
internal sealed class ConditionalExpression
{
    private readonly string _text;
    private readonly IReadOnlySet<string> _symbols;
    private int _position,_depth;
    private ConditionalExpression(string text,IReadOnlySet<string> symbols){_text=text;_symbols=symbols;}
    internal static (bool Value,int Error,string? Message) Evaluate(string text,IReadOnlySet<string> symbols)
    {
        var parser=new ConditionalExpression(DirectiveArguments.WithoutComment(text),symbols);
        try {bool value=parser.Or();parser.Skip();if(parser._position!=parser._text.Length)throw new FormatException("Unexpected directive expression text.");return(value,-1,null);}
        catch(FormatException e){return(false,parser._position,e.Message);}
    }
    private void Skip(){while(_position<_text.Length&&char.IsWhiteSpace(_text[_position]))_position++;}
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
            while(_position<_text.Length&&!char.IsWhiteSpace(_text[_position])&&"!&|=()".IndexOf(_text[_position])<0)_position++;
            string token=_text[start.._position];
            if(token=="true")return true;if(token=="false")return false;
            if(!ConditionalSymbols.TryNormalize(token,out var normalized)){_position=start;throw new FormatException("Expected a conditional symbol or boolean literal.");}
            // Roslyn recognizes decoded boolean spellings as literals in expressions,
            // although escaped spellings are permitted in definition directives.
            if(normalized=="true")return true;if(normalized=="false")return false;
            return _symbols.Contains(normalized);
        }
        finally {_depth--;}
    }
}
