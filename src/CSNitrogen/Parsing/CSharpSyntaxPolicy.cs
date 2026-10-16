using System.Globalization;
using Nitrogen;
using CSNitrogen.Syntax;
namespace CSNitrogen.Parsing;
/// <summary>Restrictions depending only on generated syntax and explicit supported feature scope.</summary>
public static class CSharpSyntaxPolicy
{
    public static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version? version=null)
    {
        ArgumentNullException.ThrowIfNull(tree);version??=new(1,0);var issues=new List<ParseIssue>();
        issues.AddRange(CSharpAttributePolicy.Check(tree));
        string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
        string Text(int n){var span=tree.Span(n);return tree.Text.Substring(span.Start,span.Length);}
        void Later(int n,int minimum,string feature)=>issues.Add(new("CSN2003",
            version<new Version(minimum,0)?$"{feature} requires C# {minimum}.":$"{feature} is not implemented in this composition.",
            tree.Span(n),version<new Version(minimum,0)?IssueCategory.UnavailableFeature:IssueCategory.UnsupportedCoverage));
        int previous=-1;
        for(int n=0;n<tree.NodeCount;n++)
        {
            string kind=Kind(n);
            if(tree.ChildCount(n)==0 && tree.Span(n).Length>0)
            {
                if(previous>=0)
                {
                    var before=tree.Span(previous);var current=tree.Span(n);
                    if(before.Start+before.Length==current.Start && IsIdentifierPart(tree.Text[before.Start+before.Length-1]) &&
                        (IsIdentifierPart(tree.Text[current.Start]) || tree.Text[current.Start]=='\\'))
                        issues.Add(new("CSN2002","Adjacent word tokens must be separated; Unicode escapes do not form keyword boundaries.",current,IssueCategory.Syntax));
                }
                previous=n;
            }
            if(kind=="StackAllocExpression")
            {
                int parent=tree.Parent(n);
                // Nitrogen inlines the choice-only VariableInitializer rule. The
                // initializer's '=' group remains directly under VariableDeclarator.
                int owner=parent>=0?tree.Parent(parent):-1;
                bool direct=parent>=0 && (Kind(parent)=="VariableInitializer" ||
                    (Kind(parent)=="Group" && owner>=0 && Kind(owner)=="VariableDeclarator"));
                bool local=false;
                for(int ancestor=parent;ancestor>=0;ancestor=tree.Parent(ancestor))
                {
                    string context=Kind(ancestor);
                    if(context is "FieldDeclaration" or "EventFieldDeclaration")break;
                    if(context is "StandardStatement" or "ForStatement" or "UsingStatement"){local=true;break;}
                }
                if(!direct||!local)issues.Add(new("CSN2004","The supported C# 1 stackalloc form requires a direct local variable initializer.",tree.Span(n),IssueCategory.Syntax));
            }
            if(kind=="ClassDeclaration")
            {
                var declaration=SyntaxView.Cast<ClassDeclarationNode>(tree,n);
                foreach(var modifier in declaration.Modifiers)if(Text(modifier.Index)=="static")Later(n,2,"Static classes");
            }
            if(kind=="PropertyDeclaration")
            {
                var property=SyntaxView.Cast<PropertyDeclarationNode>(tree,n);
                bool allSemicolons=property.Accessors.Count>0;
                foreach(var accessor in property.Accessors)allSemicolons &= Text(accessor.Body.Index).Trim()==";";
                if(allSemicolons)
                {
                    bool abstractOrExtern=false;
                    for(int parent=tree.Parent(n);parent>=0;parent=tree.Parent(parent))
                    {
                        if(Kind(parent)!="MemberDeclaration")continue;
                        var member=SyntaxView.Cast<MemberDeclarationNode>(tree,parent);
                        foreach(var modifier in member.Modifiers)abstractOrExtern |= Text(modifier.Index) is "abstract" or "extern";
                        break;
                    }
                    if(!abstractOrExtern)Later(n,3,"Automatically implemented properties");
                }
            }
            if(kind!="StatementExpression"||tree.ChildCount(n)==0)continue;
            int value=tree.Child(n,0);string expression=Kind(value);
            bool allowed=expression is "InvocationExpression" or "ObjectCreationExpression" or "AssignmentExpression" or "PostfixIncrementExpression" or "PostfixDecrementExpression";
            if(expression=="UnaryExpression")
            {string text=Text(value).TrimStart();allowed=text.StartsWith("++",StringComparison.Ordinal)||text.StartsWith("--",StringComparison.Ordinal);}
            if(!allowed)issues.Add(new("CSN2001","Only invocation, object creation, assignment or increment/decrement is a statement expression.",tree.Span(value),IssueCategory.Syntax));
        }
        return Array.AsReadOnly(issues.ToArray());
    }
    private static bool IsIdentifierPart(char c)=>char.IsLetterOrDigit(c)||c=='_'||
        char.GetUnicodeCategory(c) is UnicodeCategory.LetterNumber or UnicodeCategory.ConnectorPunctuation or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Format;
}
