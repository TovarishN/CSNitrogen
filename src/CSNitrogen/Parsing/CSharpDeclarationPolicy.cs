using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

/// <summary>Declaration restrictions determined from syntax, without symbol binding.</summary>
internal static class CSharpDeclarationPolicy
{
    public static IReadOnlyList<ParseIssue> Check(SyntaxTree tree, Version version)
    {
        var issues = new List<ParseIssue>();
        var accessors = new Dictionary<int, HashSet<string>>();
        string Kind(int node) => tree.Language!.GetKindName(tree.Kind(node));
        string Text(int node) { var span=tree.Span(node); return tree.Text.Substring(span.Start,span.Length); }
        int Ancestor(int node, params string[] kinds)
        {
            for (int parent=tree.Parent(node);parent>=0;parent=tree.Parent(parent))
                if (kinds.Contains(Kind(parent),StringComparer.Ordinal)) return parent;
            return -1;
        }
        bool Static(int node)
        {
            int member=Ancestor(node,"MemberDeclaration");
            if(member<0 || tree.ChildCount(member)<3)return false;
            foreach(var modifier in SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers)
                if(Text(modifier.Index)=="static")return true;
            return false;
        }
        bool InStruct(int node)
        {
            int type=Ancestor(node,"ClassDeclaration","StructDeclaration","InterfaceDeclaration");
            return type>=0 && Kind(type)=="StructDeclaration";
        }
        bool HasParameters(int root)
        {
            var pending=new Stack<int>();pending.Push(root);
            while(pending.TryPop(out int node))
            {
                if(Kind(node) is "FixedParameter" or "ParameterArray")return true;
                for(int child=0;child<tree.ChildCount(node);child++)pending.Push(tree.Child(node,child));
            }
            return false;
        }
        void Error(int node,string message)=>issues.Add(new("CSN2010",message,tree.Span(node),IssueCategory.Syntax));
        void Later(int node,string feature)=>issues.Add(new("CSN2011",
            version<new Version(10,0)?$"{feature} requires C# 10.":$"{feature} is not implemented in this composition.",
            tree.Span(node),version<new Version(10,0)?IssueCategory.UnavailableFeature:IssueCategory.UnsupportedCoverage));

        for(int node=0;node<tree.NodeCount;node++)
        {
            string kind=Kind(node);
            if(kind is "Accessor" or "InterfaceAccessor" or "EventAccessor")
            {
                if(tree.ChildCount(node)<3)continue;
                int keyword=kind switch {
                    "Accessor"=>SyntaxView.Cast<AccessorNode>(tree,node).Keyword.Index,
                    "InterfaceAccessor"=>SyntaxView.Cast<InterfaceAccessorNode>(tree,node).Keyword.Index,
                    _=>SyntaxView.Cast<EventAccessorNode>(tree,node).Keyword.Index};
                int owner=Ancestor(node,"PropertyDeclaration","IndexerDeclaration","InterfaceProperty","InterfaceIndexer","EventPropertyDeclaration");
                if(owner>=0)
                {
                    if(!accessors.TryGetValue(owner,out var names))accessors[owner]=names=new(StringComparer.Ordinal);
                    if(!names.Add(Text(keyword)))Error(keyword,"An accessor kind can occur only once in this declaration.");
                }
            }
            if(kind=="DestructorDeclaration" && InStruct(node))
                Error(node,"A struct declaration cannot contain a destructor.");
            if(kind=="ConstructorDeclaration" && tree.ChildCount(node)>=4)
            {
                var constructor=SyntaxView.Cast<ConstructorDeclarationNode>(tree,node);
                bool parameters=HasParameters(constructor.Parameters.Index);
                if(Static(node))
                {
                    if(parameters)Error(node,"A static constructor must have an empty parameter list.");
                    if(constructor.Initializer.HasValue)Error(node,"A static constructor cannot have a constructor initializer.");
                }
                else if(InStruct(node) && !parameters)Later(node,"Explicit parameterless struct constructors");
            }
            if(kind=="VariableDeclarator" && tree.ChildCount(node)>=2 && InStruct(node) && !Static(node))
            {
                int field=Ancestor(node,"FieldDeclaration","EventFieldDeclaration","MethodDeclaration","ConstructorDeclaration","Accessor");
                if(field>=0 && (Kind(field) is "FieldDeclaration" or "EventFieldDeclaration") &&
                    SyntaxView.Cast<VariableDeclaratorNode>(tree,node).Initializer.HasValue)
                    Later(node,"Struct instance field initializers");
            }
        }
        for(int node=0;node<tree.NodeCount;node++)
            if(Kind(node)=="EventPropertyDeclaration" &&
                (!accessors.TryGetValue(node,out var names)||!names.Contains("add")||!names.Contains("remove")))
                Error(node,"An event accessor declaration requires both add and remove.");
        return issues.AsReadOnly();
    }
}
