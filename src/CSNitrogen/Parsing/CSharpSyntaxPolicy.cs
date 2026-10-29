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
        issues.AddRange(UnicodePolicy.Check(tree,version));
        issues.AddRange(CSharp3ExpressionPolicy.Check(tree,version));
        issues.AddRange(CSharp3DeclarationPolicy.Check(tree,version));
        issues.AddRange(CSharp4ContextPolicy.Check(tree,version));
        issues.AddRange(CSharp5ContextPolicy.Check(tree,version));
        issues.AddRange(CSharpAttributePolicy.Check(tree,version));
        issues.AddRange(CSharpCompilationUnitPolicy.Check(tree,version));
        issues.AddRange(CSharpDeclarationPolicy.Check(tree,version));
        if(version.Major is 2 or 3 or 4 or 5)CheckIterators(tree,issues);
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
                    if(before.Start+before.Length==current.Start && IsIdentifierPartAt(tree.Text,before.Start+before.Length-1,version) &&
                        (IsIdentifierPartAt(tree.Text,current.Start,version) || tree.Text[current.Start]=='\\'))
                        issues.Add(new("CSN2002","Adjacent word tokens must be separated; Unicode escapes do not form keyword boundaries.",current,IssueCategory.Syntax));
                }
                previous=n;
            }
            if(kind=="AssignmentExpression" && tree.ChildCount(n)>=3)
            {
                var target=SyntaxView.Cast<AssignmentExpressionExpression>(tree,n).Target;
                bool unary=Kind(target.Index) is "NameExpression" or "LiteralExpression" or "ThisExpression" or
                    "GenericNameExpression" or "GenericBaseMemberExpression" or "GenericPredefinedMemberExpression" or "GenericMemberExpression" or "UnboundTypeOfExpression" or "AnonymousMethodExpression" or "DefaultValueExpression" or "AliasNameExpression" or "GenericPointerMemberExpression" or
                    "BaseMemberExpression" or "BaseIndexExpression" or "NamedBaseIndexExpression" or "ParenthesizedExpression" or "CastExpression" or
                    "ObjectCreationExpression" or "InitializedObjectExpression" or "AnonymousObjectExpression" or "ImplicitArrayExpression" or "SizedArrayExpression" or "InitializedArrayExpression" or
                    "SizeOfBuiltinExpression" or "TypeOfExpression" or "CheckedExpression" or "UncheckedExpression" or
                    "PredefinedMemberExpression" or "MemberExpression" or "InvocationExpression" or "ElementExpression" or "NamedElementExpression" or
                    "PostfixIncrementExpression" or "PostfixDecrementExpression" or "UnaryExpression" or
                    "PointerIndirectionExpression" or "AddressExpression" or "PointerMemberExpression" or "SizeOfExtendedExpression";
                unary |= version.Major>=5&&Kind(target.Index)=="AwaitExpression";
                if(!unary)issues.Add(new("CSN2013","An assignment target must be a unary expression; writability is a separate semantic check.",tree.Span(target.Index),IssueCategory.Syntax));
            }
            if(tree.ChildCount(n)>0 && kind is "MultiplyExpression" or "DivideExpression" or "RemainderExpression" or
                "AddExpression" or "SubtractExpression" or "ShiftLeftExpression" or "ShiftRightExpression")
            {
                var left=kind switch
                {
                    "MultiplyExpression" => SyntaxView.Cast<MultiplyExpressionExpression>(tree,n).Left,
                    "DivideExpression" => SyntaxView.Cast<DivideExpressionExpression>(tree,n).Left,
                    "RemainderExpression" => SyntaxView.Cast<RemainderExpressionExpression>(tree,n).Left,
                    "AddExpression" => SyntaxView.Cast<AddExpressionExpression>(tree,n).Left,
                    "SubtractExpression" => SyntaxView.Cast<SubtractExpressionExpression>(tree,n).Left,
                    "ShiftLeftExpression" => SyntaxView.Cast<ShiftLeftExpressionExpression>(tree,n).Left,
                    _ => SyntaxView.Cast<ShiftRightExpressionExpression>(tree,n).Left
                };
                if(Kind(left.Index) is "IsExpression" or "AsExpression")
                    issues.Add(new("CSN2016","A type-operator result requires parentheses before an operator at this tighter precedence.",tree.Span(n),IssueCategory.Syntax));
            }
            if(tree.ChildCount(n)>0 && kind is "MemberExpression" or "InvocationExpression" or "ElementExpression" or "NamedElementExpression" or
                "PostfixIncrementExpression" or "PostfixDecrementExpression" or "PointerMemberExpression" or "GenericPointerMemberExpression" or "GenericMemberExpression")
            {
                // A Pratt type operand (is/as) is not an expression recursion and can
                // leave a following suffix for the outer loop. Such a suffix still
                // requires the primary-expression category specified by the grammar.
                var target=kind switch
                {
                    "MemberExpression" => SyntaxView.Cast<MemberExpressionExpression>(tree,n).Target,
                    "InvocationExpression" => SyntaxView.Cast<InvocationExpressionExpression>(tree,n).Target,
                    "ElementExpression" => SyntaxView.Cast<ElementExpressionExpression>(tree,n).Target,
                    "NamedElementExpression" => SyntaxView.Cast<NamedElementExpressionExpression>(tree,n).Target,
                    "PostfixIncrementExpression" => SyntaxView.Cast<PostfixIncrementExpressionExpression>(tree,n).Value,
                    "PostfixDecrementExpression" => SyntaxView.Cast<PostfixDecrementExpressionExpression>(tree,n).Value,
                    "GenericMemberExpression" => SyntaxView.Cast<GenericMemberExpressionExpression>(tree,n).Target,
                    "GenericPointerMemberExpression" => SyntaxView.Cast<GenericPointerMemberExpressionExpression>(tree,n).Target,
                    _ => SyntaxView.Cast<PointerMemberExpressionExpression>(tree,n).Target
                };
                if(!IsPrimaryKind(Kind(target.Index)))issues.Add(new("CSN2015","A primary suffix requires a primary expression target; parenthesize a type-operator result.",tree.Span(n),IssueCategory.Syntax));
            }
            if(kind is "ElementExpression" or "NamedElementExpression" && tree.ChildCount(n)>0)
            {
                var target=kind=="ElementExpression"?SyntaxView.Cast<ElementExpressionExpression>(tree,n).Target:SyntaxView.Cast<NamedElementExpressionExpression>(tree,n).Target;
                if(Kind(target.Index) is "SizedArrayExpression" or "InitializedArrayExpression" or "ImplicitArrayExpression")
                    issues.Add(new("CSN2005","Direct array creation requires parentheses before element access.",tree.Span(n),IssueCategory.Syntax));
            }
            if(kind=="SwitchStatement" && tree.ChildCount(n)>5)
            {
                bool sawDefault=false;
                foreach(var section in SyntaxView.Cast<SwitchStatementNode>(tree,n).Sections)
                    if(tree.ChildCount(section.Index)>0)
                        foreach(var label in section.Labels)
                            if(tree.ChildCount(label.Index)>0)
                            {
                                int payload=tree.Child(label.Index,0);
                                if(payload==SyntaxTree.NoNode)continue;
                                int keyword=Kind(payload)=="Group" && tree.ChildCount(payload)>0?tree.Child(payload,0):payload;
                                if(keyword==SyntaxTree.NoNode)continue;
                                if(Text(keyword)!="default")continue;
                                if(sawDefault)issues.Add(new("CSN2006","A switch statement permits only one default label.",tree.Span(keyword),IssueCategory.Syntax));
                                sawDefault=true;
                            }
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
                foreach(var modifier in declaration.Modifiers)if(Text(modifier.Index)=="static" && version<new Version(2,0))Later(n,2,"Static classes");
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
                    if(!abstractOrExtern && version.Major<3)Later(n,3,"Automatically implemented properties");
                }
            }
            if(kind!="StatementExpression"||tree.ChildCount(n)==0)continue;
            int value=tree.Child(n,0);string expression=Kind(value);
            bool allowed=expression is "InvocationExpression" or "InitializedObjectExpression" or "ObjectCreationExpression" or "AssignmentExpression" or "PostfixIncrementExpression" or "PostfixDecrementExpression";
            allowed |= version.Major>=5&&expression=="AwaitExpression";
            if(expression=="UnaryExpression")
            {string text=Text(value).TrimStart();allowed=text.StartsWith("++",StringComparison.Ordinal)||text.StartsWith("--",StringComparison.Ordinal);}
            if(!allowed)issues.Add(new("CSN2001","Only invocation, object creation, assignment or increment/decrement is a statement expression.",tree.Span(value),IssueCategory.Syntax));
        }
        return Array.AsReadOnly(issues.ToArray());
    }
    // ECMA-334 fourth edition §15.14/§26.1 and Version4 §8.2/§8.14/§10.14:
    // Containment is determined
    // from generated syntax; return types, conversions and execution are deferred.
    private static void CheckIterators(SyntaxTree tree,List<ParseIssue> issues)
    {
        string Kind(int node)=>tree.Language!.GetKindName(tree.Kind(node));
        string Text(int node){var span=tree.Span(node);return tree.Text.Substring(span.Start,span.Length);}
        bool Boundary(int node)=>Kind(node) is "MethodDeclaration" or "OperatorDeclaration" or "ConversionDeclaration" or
            "Accessor" or "EventAccessor" or "ConstructorDeclaration" or "DestructorDeclaration" or "AnonymousMethodExpression" or "LambdaExpression";
        int Owner(int node)
        {for(int p=tree.Parent(node);p>=0;p=tree.Parent(p))if(Boundary(p))return p;return -1;}
        void Error(int node,string message)=>issues.Add(new("CSN2022",message,tree.Span(node),IssueCategory.Syntax));
        var iterators=new HashSet<int>();
        for(int node=0;node<tree.NodeCount;node++)
        {
            string kind=Kind(node);
            if(kind is not ("YieldReturnStatement" or "YieldBreakStatement"))continue;
            int owner=Owner(node);
            if(owner<0 || Kind(owner) is not ("MethodDeclaration" or "OperatorDeclaration" or "ConversionDeclaration" or "Accessor"))
            {Error(node,"A yield statement requires a method, operator or accessor body and cannot occur in an anonymous method.");continue;}
            iterators.Add(owner);
            for(int parent=tree.Parent(node);parent>=0&&parent!=owner;parent=tree.Parent(parent))
            {
                if(Kind(parent)=="FinallyClause")Error(node,"A yield statement cannot occur in a finally clause.");
                if(kind=="YieldReturnStatement" && Kind(parent)=="TryStatement")
                {
                    var statement=SyntaxView.Cast<TryStatementNode>(tree,parent);
                    bool hasCatch=false;
                    var pending=new Stack<int>();pending.Push(statement.Handlers.Index);
                    while(pending.TryPop(out int part))
                    {
                        if(Kind(part) is "SpecificCatchClause" or "GeneralCatchClause"){hasCatch=true;break;}
                        if(Kind(part) is "Block" or "AnonymousMethodExpression")continue;
                        for(int c=0;c<tree.ChildCount(part);c++){int child=tree.Child(part,c);if(child!=SyntaxTree.NoNode)pending.Push(child);}
                    }
                    if(hasCatch)Error(node,"A yield return statement cannot occur in a try statement containing catches.");
                }
            }
        }
        for(int node=0;node<tree.NodeCount;node++)
        {
            if(!iterators.Contains(Owner(node)))continue;
            string kind=Kind(node);
            if(kind=="FixedParameter")
            {
                var parameter=SyntaxView.Cast<FixedParameterNode>(tree,node);
                if(parameter.Mode.HasValue)Error(node,"An iterator member cannot declare ref or out parameters.");
            }
            if(kind=="JumpStatement")
            {
                int first=node;while(first!=SyntaxTree.NoNode && tree.ChildCount(first)>0)first=tree.Child(first,0);
                if(first!=SyntaxTree.NoNode && Text(first)=="return")Error(node,"An iterator block cannot contain an ordinary return statement.");
            }
            if(kind=="UnsafeBlock")Error(node,"An iterator block cannot contain an unsafe context.");
        }
    }
    private static bool IsPrimaryKind(string kind) => kind is "GenericNameExpression" or "GenericBaseMemberExpression" or "GenericPredefinedMemberExpression" or "GenericMemberExpression" or "UnboundTypeOfExpression" or "AnonymousMethodExpression" or "DefaultValueExpression" or "AliasNameExpression" or "GenericPointerMemberExpression" or "NameExpression" or "LiteralExpression" or "ThisExpression" or
        "BaseMemberExpression" or "BaseIndexExpression" or "NamedBaseIndexExpression" or "ParenthesizedExpression" or "ObjectCreationExpression" or "InitializedObjectExpression" or "AnonymousObjectExpression" or "ImplicitArrayExpression" or
        "SizedArrayExpression" or "InitializedArrayExpression" or "SizeOfBuiltinExpression" or "TypeOfExpression" or
        "CheckedExpression" or "UncheckedExpression" or "PredefinedMemberExpression" or "MemberExpression" or
        "InvocationExpression" or "ElementExpression" or "NamedElementExpression" or "PostfixIncrementExpression" or "PostfixDecrementExpression" or
        "PointerMemberExpression" or "SizeOfExtendedExpression";
    private static bool IsIdentifierPartAt(string text,int index,Version version)
    {
        char c=text[index];
        if(UnicodePolicy.UsesUnicode4(version))
        {
            if(char.IsLowSurrogate(c)&&index>0&&char.IsHighSurrogate(text[index-1]))index--;
            if(char.IsHighSurrogate(text[index])&&index+1<text.Length&&char.IsLowSurrogate(text[index+1]))
                return CSharp2UnicodeTables.IsPart(char.ConvertToUtf32(text,index));
            return UnicodePolicy.IsPart(c,version);
        }
        return IsIdentifierPart(c);
    }
    private static bool IsIdentifierPart(char c)=>char.IsLetterOrDigit(c)||c=='_'||
        char.GetUnicodeCategory(c) is UnicodeCategory.LetterNumber or UnicodeCategory.ConnectorPunctuation or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Format;
}
