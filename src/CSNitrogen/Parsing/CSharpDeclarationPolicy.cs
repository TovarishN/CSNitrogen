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
            if(kind=="FixedBufferMember")
            {
                if(!InStruct(node))Error(node,"A fixed buffer declaration requires a struct owner.");
                var buffer=SyntaxView.Cast<FixedBufferMemberClassMember>(tree,node);
                // CSharp3 spells buffer-element-type as type. A named type can
                // denote a permitted primitive through an alias; binding is deferred.
                bool deferredName=version.Major>=3 && SyntaxView.TryCast<TypeSyntaxTypeReference>(tree,buffer.Type.Index,out var element)
                    && element.Ranks.Count==0 && Kind(element.Element.Index) is "NamedTypeName" or "AliasQualifiedTypeName" or "GenericTypeName";
                if(!deferredName && Text(buffer.Type.Index) is not ("bool" or "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "char" or "float" or "double"))
                    Error(buffer.Type.Index,"A fixed buffer requires a permitted primitive element type; named element denotation is deferred in CSharp3 and later.");
                foreach(var modifier in SyntaxView.Cast<FixedBufferMemberClassMember>(tree,node).Modifiers)
                    if(Text(modifier.Index)=="static")Error(modifier.Index,"A fixed buffer declaration cannot be static.");
            }
            if(kind=="PartialModifier")
            {
                int owner=Ancestor(node,"ClassDeclaration","StructDeclaration","InterfaceDeclaration","EnumDeclaration","DelegateDeclaration","MemberDeclaration");
                if(version.Major>=3 && owner>=0 && Kind(owner)=="MemberDeclaration" &&
                    Kind(SyntaxView.Cast<MemberDeclarationNode>(tree,owner).Body.Index)=="MethodDeclaration")continue;
                if(owner<0 || Kind(owner) is not ("ClassDeclaration" or "StructDeclaration" or "InterfaceDeclaration"))
                    Error(node,"This composition supports partial class, struct and interface declarations only.");
                else
                {
                    var modifiers=Kind(owner) switch
                    {
                        "ClassDeclaration"=>SyntaxView.Cast<ClassDeclarationNode>(tree,owner).Modifiers,
                        "StructDeclaration"=>SyntaxView.Cast<StructDeclarationNode>(tree,owner).Modifiers,
                        _=>SyntaxView.Cast<InterfaceDeclarationNode>(tree,owner).Modifiers
                    };
                    if(modifiers.Count==0 || modifiers[modifiers.Count-1].Index!=node)
                        Error(node,"The partial modifier must occur once after the ordinary type modifiers.");
                }
            }
            if(kind is "PropertyDeclaration" or "IndexerDeclaration")
            {
                var items=kind=="PropertyDeclaration"?SyntaxView.Cast<PropertyDeclarationNode>(tree,node).Accessors:SyntaxView.Cast<IndexerDeclarationNode>(tree,node).Accessors;
                int count=0;
                foreach(var item in items)if(item.Modifiers.HasValue)count++;
                bool overriding=false;
                int member=Ancestor(node,"MemberDeclaration");
                if(member>=0)
                    foreach(var modifier in SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers)
                        overriding |= Text(modifier.Index)=="override";
                if(count>1 || count==1 && items.Count!=2 && !overriding)Error(node,"Accessor accessibility requires two accessors unless overriding, and may appear on only one.");
                if(count>0 && kind=="PropertyDeclaration")
                {
                    var property=SyntaxView.Cast<PropertyDeclarationNode>(tree,node);
                    if(property.NamePrefix.HasValue || property.NameExtension.HasValue || property.Name.Parts.Count>1)
                        Error(node,"Explicit-interface properties cannot declare accessor accessibility.");
                }
                if(count>0 && kind=="IndexerDeclaration")
                {
                    var indexer=SyntaxView.Cast<IndexerDeclarationNode>(tree,node);
                    // A simple indexer-name consists solely of the this token.
                    int first=indexer.Name.Index;while(tree.ChildCount(first)>0)first=tree.Child(first,0);
                    if(Text(first)!="this")Error(node,"Explicit-interface indexers cannot declare accessor accessibility.");
                }
            }
            if(kind=="GenericConstraints")
            {
                int owner=Ancestor(node,"ClassDeclaration","StructDeclaration","InterfaceDeclaration","DelegateDeclaration","MethodDeclaration","InterfaceMethod");
                bool parameters=owner>=0 && (Kind(owner) switch
                {
                    "ClassDeclaration"=>SyntaxView.Cast<ClassDeclarationNode>(tree,owner).TypeParameters.HasValue,
                    "StructDeclaration"=>SyntaxView.Cast<StructDeclarationNode>(tree,owner).TypeParameters.HasValue,
                    "InterfaceDeclaration"=>SyntaxView.Cast<InterfaceDeclarationNode>(tree,owner).TypeParameters.HasValue,
                    "DelegateDeclaration"=>SyntaxView.Cast<DelegateDeclarationNode>(tree,owner).TypeParameters.HasValue,
                    "MethodDeclaration"=>SyntaxView.Cast<MethodDeclarationNode>(tree,owner).TypeParameters.HasValue,
                    "InterfaceMethod"=>SyntaxView.Cast<InterfaceMethodNode>(tree,owner).TypeParameters.HasValue,
                    _=>false
                });
                if(!parameters)Error(node,"Constraint clauses require type parameters on the owning declaration.");
            }
            if(version.Major>=2 && kind=="FixedParameter")
            {
                var parameter=SyntaxView.Cast<FixedParameterNode>(tree,node);
                if(parameter.Mode.HasValue && Ancestor(node,"IndexerParameterList")>=0)
                    Error(node,"Indexer parameters cannot have ref or out modifiers.");
            }
            if(kind=="ConstraintClause")
            {
                var items=SyntaxView.Cast<ConstraintClauseNode>(tree,node).Items;
                bool hasStruct=false;foreach(var item in items)hasStruct |= Text(item.Index).Trim()=="struct";
                for(int i=0;i<items.Count;i++)
                {
                    string text=Text(items[i].Index).Trim();
                    if((text is "class" or "struct") && i!=0)Error(items[i].Index,"A class or struct constraint must be first.");
                    int first=items[i].Index;
                    while(tree.ChildCount(first)>0)first=tree.Child(first,0);
                    if(Text(first)=="new" && hasStruct)
                        Error(items[i].Index,"A struct constraint cannot be combined with a constructor constraint.");
                    var pending=new Stack<int>();pending.Push(items[i].Index);
                    while(pending.TryPop(out int part))
                    {
                        if(Kind(part)=="TypeSyntax")
                        {
                            var type=SyntaxView.Cast<TypeSyntaxTypeReference>(tree,part);
                            if(type.Ranks.Count>0 || Kind(type.Element.Index) is "NullableTypeName" or "PointerTypeName" or "VoidPointerTypeName" ||
                                Kind(type.Element.Index)=="BuiltinTypeName" && Text(type.Element.Index) is not ("object" or "string"))
                                Error(part,"A named constraint requires a class/interface/type-parameter name, not a value, nullable, pointer or array type.");
                            break;
                        }
                        for(int child=tree.ChildCount(part)-1;child>=0;child--)pending.Push(tree.Child(part,child));
                    }
                    if(Text(first)=="new" && i!=items.Count-1)
                        Error(items[i].Index,"A constructor constraint must be last.");
                }
            }
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
                    string keywordName=Text(keyword);
                    if(UnicodePolicy.TryNormalize(keywordName,version,true,out var normalizedKeyword))keywordName=normalizedKeyword;
                    if(!names.Add(keywordName))Error(keyword,"An accessor kind can occur only once in this declaration.");
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
