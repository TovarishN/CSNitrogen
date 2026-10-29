using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Parsing;

/// <summary>Source-local async/await restrictions; no task typing, binding or execution.</summary>
internal static class CSharp5ContextPolicy
{
 internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
 {
  if(version.Major<5)return Array.Empty<ParseIssue>();
  var issues=new List<ParseIssue>();
  string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
  bool RawAwait(int n){var span=tree.Span(n);return span.Length==5&&tree.Text.AsSpan(span.Start,span.Length).SequenceEqual("await");}
  void Error(string code,int n,string message)=>issues.Add(new(code,message,tree.Span(n),IssueCategory.Syntax));
  bool Within(int n,int owner){for(int p=n;p>=0;p=tree.Parent(p))if(p==owner)return true;return false;}
  int Function(int n)
  {
   for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))
    if(Kind(p) is "MethodDeclaration" or "LambdaExpression" or "AnonymousMethodExpression")return p;
   return -1;
  }
  int Member(int n)
  {for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))if(Kind(p)=="MemberDeclaration")return p;return -1;}
  bool Has(SyntaxList<ModifierNode> modifiers,string kind)
  {foreach(var modifier in modifiers)if(Kind(modifier.Index)==kind)return true;return false;}
  bool Async(int owner)=>owner>=0&&Kind(owner) switch
  {
   "MethodDeclaration"=>Member(owner) is var member&&member>=0&&Has(SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers,"AsyncModifier"),
   "LambdaExpression"=>SyntaxView.Cast<LambdaExpressionExpression>(tree,owner).Async.HasValue,
   "AnonymousMethodExpression"=>SyntaxView.Cast<AnonymousMethodExpressionExpression>(tree,owner).Async.HasValue,
   _=>false,
  };
  int Body(int owner)=>Kind(owner) switch
  {
   "MethodDeclaration"=>SyntaxView.Cast<MethodDeclarationNode>(tree,owner).Body.Index,
   "LambdaExpression"=>SyntaxView.Cast<LambdaExpressionExpression>(tree,owner).Body.Index,
   "AnonymousMethodExpression"=>SyntaxView.Cast<AnonymousMethodExpressionExpression>(tree,owner).Body.Index,
   _=>SyntaxTree.NoNode,
  };
  bool AsyncLexicalContext(int n)
  {
   for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))
    if(Kind(p) is "MethodDeclaration" or "LambdaExpression" or "AnonymousMethodExpression")
     if(Async(p)&&(Kind(p)!="MethodDeclaration"||Within(n,Body(p))))return true;
   return false;
  }
  bool Unsafe(int n)
  {
   for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))
   {
    bool marked=Kind(p) switch
    {
     "UnsafeBlock"=>true,
     "MemberDeclaration"=>Has(SyntaxView.Cast<MemberDeclarationNode>(tree,p).Modifiers,"UnsafeModifier"),
     "ClassDeclaration"=>Has(SyntaxView.Cast<ClassDeclarationNode>(tree,p).Modifiers,"UnsafeModifier"),
     "StructDeclaration"=>Has(SyntaxView.Cast<StructDeclarationNode>(tree,p).Modifiers,"UnsafeModifier"),
     "InterfaceDeclaration"=>Has(SyntaxView.Cast<InterfaceDeclarationNode>(tree,p).Modifiers,"UnsafeModifier"),
     "DelegateDeclaration"=>Has(SyntaxView.Cast<DelegateDeclarationNode>(tree,p).Modifiers,"UnsafeModifier"),
     _=>false,
    };
    if(marked)return true;
   }
   return false;
  }
  bool ForbiddenQueryPosition(int node,int clause)
  {
   switch(Kind(clause))
   {
    case "FromClause":
     var from=SyntaxView.Cast<FromClauseNode>(tree,clause);
     int parent=tree.Parent(clause);
     return parent<0||Kind(parent)!="QueryExpression"||SyntaxView.Cast<QueryExpressionExpression>(tree,parent).From.Index!=clause||!Within(node,from.Source.Index);
    case "JoinClause":return !Within(node,SyntaxView.Cast<JoinClauseNode>(tree,clause).Source.Index);
    case "LetClause":case "WhereClause":case "SelectClause":case "GroupClause":case "OrderByClause":return true;
    default:return false;
   }
  }
  for(int n=0;n<tree.NodeCount;n++)
  {
   string kind=Kind(n);int owner;
   if(kind=="AwaitExpression")
   {
    var awaited=SyntaxView.Cast<AwaitExpressionExpression>(tree,n);owner=Function(n);string? reason=null;
    if(!Async(owner)||!Within(n,Body(owner)))reason="An await expression requires the body of an async function, including within nested anonymous functions.";
    else if(Unsafe(n))reason="An await expression is not permitted in an unsafe context in this supported source profile.";
    else
     for(int p=tree.Parent(n);p>=0&&p!=owner;p=tree.Parent(p))
     {
      if(version.Major==5&&Kind(p) is "CatchClause" or "FinallyClause")reason="C# 5 does not permit await inside catch or finally blocks of this async function.";
      else if(Kind(p)=="LockStatement"&&Within(n,SyntaxView.Cast<LockStatementNode>(tree,p).Body.Index))reason="Await is not permitted inside the body of a lock statement.";
      else if(ForbiddenQueryPosition(n,p))reason="This query position is translated into a non-async anonymous function and cannot contain await.";
      if(reason is not null)break;
     }
    if(reason is not null)Error("CSN2050",awaited.Keyword.Index,reason);
   }
   else if(kind=="Identifier"&&RawAwait(n))
   {
    if(AsyncLexicalContext(n))
     Error("CSN2051",n,"Raw await cannot be used as an identifier in an async function; escaped identifier spellings remain distinct lexical forms.");
   }
   else if(kind is "FixedParameter" or "AnonymousParameter")
   {
    owner=Function(n);if(!Async(owner))continue;
    var mode=kind=="FixedParameter"?SyntaxView.Cast<FixedParameterNode>(tree,n).Mode:SyntaxView.Cast<AnonymousParameterNode>(tree,n).Mode;
    if(mode.HasValue)Error("CSN2052",mode.Value.Index,"The formal parameter list of an async function cannot contain ref or out parameters.");
   }
  }
  return issues.AsReadOnly();
 }
}
