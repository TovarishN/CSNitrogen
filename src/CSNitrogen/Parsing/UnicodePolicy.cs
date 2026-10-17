using System.Globalization;
using System.Text;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

internal static class UnicodePolicy
{
    internal static bool IsHistorical(Version version)=>version.Major==1;
    internal static bool IsStart(char c,Version version)=>IsHistorical(version)?HistoricalUnicodeTables.IsStart(c):
        c=='_'||char.GetUnicodeCategory(c) is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or
        UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber;
    internal static bool IsPart(char c,Version version)=>IsHistorical(version)?HistoricalUnicodeTables.IsPart(c):
        IsStart(c,version)||char.GetUnicodeCategory(c) is UnicodeCategory.DecimalDigitNumber or UnicodeCategory.ConnectorPunctuation or
        UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Format;
    internal static bool IsFormat(char c,Version version)=>IsHistorical(version)?HistoricalUnicodeTables.IsFormat(c):char.GetUnicodeCategory(c)==UnicodeCategory.Format;
    internal static bool IsWhiteSpace(char c,Version version)=>IsHistorical(version)?
        c is '\t' or '\v' or '\f'||CSharpTrivia.IsNewline(c,true)||HistoricalUnicodeTables.IsSpace(c):char.IsWhiteSpace(c);
    internal static string Trim(string text,Version version)
    {
        int first=0,last=text.Length;
        while(first<last&&IsWhiteSpace(text[first],version))first++;
        while(last>first&&IsWhiteSpace(text[last-1],version))last--;
        return text[first..last];
    }
    internal static bool TryNormalize(string text,Version version,bool verbatim,out string normalized)
    {
        normalized="";int first=verbatim&&text.StartsWith('@')?1:0;
        if(first==text.Length)return false;
        var result=new StringBuilder();bool start=true;
        for(int index=first;index<text.Length;index++)
        {
            char value=text[index];
            bool escaped=value=='\\';
            if(escaped)
            {
                if(index+1>=text.Length || text[index+1] is not ('u' or 'U'))return false;
                int count=text[index+1]=='u'?4:8;
                if(index+2+count>text.Length||!uint.TryParse(text.AsSpan(index+2,count),NumberStyles.HexNumber,CultureInfo.InvariantCulture,out uint code)||code>65535)return false;
                value=(char)code;index+=count+1;
            }
            if(start && escaped && value=='_' && IsHistorical(version))return false;
            if(start?!IsStart(value,version):!IsPart(value,version))return false;
            if(!IsFormat(value,version))result.Append(value);
            start=false;
        }
        normalized=result.ToString();return true;
    }
    internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
    {
        // Keyword recognition uses raw spelling in the grammar. Escaped keyword
        // spellings remain identifiers; normalized identity is not keyword recognition.
        var issues=new List<ParseIssue>();
        for(int node=0;node<tree.NodeCount;node++)
        {
            string kind=tree.Language!.GetKindName(tree.Kind(node));
            if(!IsHistorical(version) && kind is "Character" or "RegularString")
            {
                var literal=tree.Span(node);
                if(tree.Text.AsSpan(literal.Start,literal.Length).Contains('\u0085'))
                    issues.Add(new("CSN2021","A raw NEL is a line terminator under the selected current lexical policy.",literal,IssueCategory.Syntax));
            }
            if(kind!="Identifier")continue;
            var span=tree.Span(node);string text=tree.Text.Substring(span.Start,span.Length);
            if(!TryNormalize(text,version,true,out _))
                issues.Add(new("CSN2020","Identifier characters are not valid under the selected Unicode category policy.",span,IssueCategory.Syntax));
        }
        return issues.AsReadOnly();
    }
}
