using Nitrogen;
using CSNitrogen.Syntax;
namespace CSNitrogen.Parsing;
/// <summary>Source categories from historical C#3 §§7.14–7.15; no delegate or LINQ binding.</summary>
internal static class CSharp3ExpressionPolicy
{
 private static readonly HashSet<string> QueryWords=new(StringComparer.Ordinal)
 {"from","where","join","on","equals","into","let","orderby","ascending","descending","select","group","by"};
 internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
 {
  var issues=new List<ParseIssue>();string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
  void Error(int n,string message)=>issues.Add(new("CSN2030",message,tree.Span(n),IssueCategory.Syntax));
  for(int n=0;n<tree.NodeCount;n++)
  {
   string kind=Kind(n);
   if(kind is "LambdaExpression" or "QueryExpression")
   {
    if(version.Major<3){issues.Add(new("CSN2031","This expression requires C# 3.",tree.Span(n),IssueCategory.UnavailableFeature));continue;}
    // Anonymous functions and queries are non-assignment expressions, not
    // unary/binary operands. Parentheses introduce a primary expression.
    int parent=tree.Parent(n);
    if(parent>=0 && Kind(parent) is "UnaryExpression" or "AwaitExpression" or "CastExpression" or "MultiplyExpression" or "DivideExpression" or "RemainderExpression" or
      "AddExpression" or "SubtractExpression" or "ShiftLeftExpression" or "ShiftRightExpression" or "LessExpression" or "GreaterExpression" or
      "LessEqualExpression" or "GreaterEqualExpression" or "IsExpression" or "AsExpression" or "EqualExpression" or "NotEqualExpression" or
      "NullCoalescingExpression" or "BitAndExpression" or "BitXorExpression" or "BitOrExpression" or "AndExpression" or "OrExpression" or "PointerIndirectionExpression" or "AddressExpression")
      Error(n,"A lambda or query expression requires parentheses in this operand position.");
   }
   if(kind=="InferredMember")
   {
    var member=SyntaxView.Cast<InferredMemberNode>(tree,n);
    bool fourthBaseMember=version.Major>=4 && Kind(member.Value.Index) is ("BaseMemberExpression" or "GenericBaseMemberExpression");
    if(!fourthBaseMember && Kind(member.Value.Index) is not ("NameExpression" or "MemberExpression" or "GenericNameExpression" or "GenericMemberExpression" or "GenericPredefinedMemberExpression" or "PredefinedMemberExpression"))
     Error(n,"An inferred anonymous member must be a simple name or member access.");
   }
   if(kind=="ScalarElement" && Kind(SyntaxView.Cast<ScalarElementNode>(tree,n).Value.Index)=="AssignmentExpression")
    Error(n,"A scalar collection element must be a non-assignment expression; braces permit an expression list.");
   if(kind!="Identifier")continue;
   var span=tree.Span(n);string text=tree.Text.Substring(span.Start,span.Length);
   // §2.4.1: Unicode escapes do not form keywords. Reserve raw spellings only.
   if(!QueryWords.Contains(text))continue;
   for(int parent=tree.Parent(n);parent>=0;parent=tree.Parent(parent))
    if(Kind(parent)=="QueryExpression"){Error(n,"Query contextual words require an @ prefix or escaped spelling when used as identifiers inside a query expression.");break;}
  }
  return issues;
 }
}
