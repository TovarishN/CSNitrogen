using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Parsing;

/// <summary>Version 4 source-local rules; parameter binding and value/type legality are separate.</summary>
internal static class CSharp4ContextPolicy
{
    internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
    {
        if(version.Major<4)return Array.Empty<ParseIssue>();
        var issues=new List<ParseIssue>();
        string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
        string Text(int n){var s=tree.Span(n);return tree.Text.Substring(s.Start,s.Length);}
        void Error(int n,string message)=>issues.Add(new("CSN2040",message,tree.Span(n),IssueCategory.Syntax));
        int Owner(int n,params string[] kinds)
        {for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))if(kinds.Contains(Kind(p),StringComparer.Ordinal))return p;return -1;}
        var namedOwners=new HashSet<int>();
        var optionalOwners=new HashSet<int>();
        var names=new Dictionary<int,HashSet<string>>();
        void Named(int owner,int label)
        {
            if(owner<0)return;
            namedOwners.Add(owner);
            if(!UnicodePolicy.TryNormalize(Text(label),version,true,out var name))return;
            if(!names.TryGetValue(owner,out var seen))names[owner]=seen=new(StringComparer.Ordinal);
            if(!seen.Add(name))Error(label,"A named argument cannot repeat a logical parameter name in the same argument list.");
        }
        for(int n=0;n<tree.NodeCount;n++)
        {
            string kind=Kind(n);
            if(kind=="Argument")
            {
                var argument=SyntaxView.Cast<ArgumentNode>(tree,n);
                int owner=Owner(n,"Arguments");
                if(argument.Name.HasValue)
                {
                    var label=SyntaxView.Cast<NamedArgumentNameArgumentName>(tree,argument.Name.Value.Index).Name;
                    Named(owner,label.Index);
                }
                else if(owner>=0&&namedOwners.Contains(owner) && version<new Version(7,2))
                    Error(n,"In C# 4 through 7.1, positional arguments must precede named arguments.");
            }
            else if(kind=="NamedIndexArgument")
            {
                var argument=SyntaxView.Cast<NamedIndexArgumentNode>(tree,n);
                Named(Owner(n,"NamedIndexArguments"),argument.Name.Index);
            }
            else if(kind=="FixedParameter")
            {
                var parameter=SyntaxView.Cast<FixedParameterNode>(tree,n);
                int owner=Owner(n,"Parameters","IndexerParameterList");
                if(parameter.Default.HasValue)
                {
                    if(owner>=0)optionalOwners.Add(owner);
                    if(parameter.Mode.HasValue)Error(parameter.Default.Value.Index,"A ref or out parameter cannot have a default argument.");
                }
                else if(owner>=0&&optionalOwners.Contains(owner))
                    Error(n,"A required parameter cannot follow an optional parameter.");
            }
            else if(kind=="TypeParameter")
            {
                var parameter=SyntaxView.Cast<TypeParameterNode>(tree,n);
                if(!parameter.Variance.HasValue)continue;
                int owner=Owner(n,"ClassDeclaration","StructDeclaration","InterfaceDeclaration","DelegateDeclaration","MethodDeclaration","InterfaceMethod");
                if(owner<0||Kind(owner) is not ("InterfaceDeclaration" or "DelegateDeclaration"))
                    Error(parameter.Variance.Value.Index,"Variance annotations require an interface or delegate type parameter.");
            }
        }
        return issues.AsReadOnly();
    }
}
