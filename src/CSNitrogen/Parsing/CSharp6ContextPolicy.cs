using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Parsing;

/// <summary>Sixth-edition source-local restrictions (ECMA-334 6th edition); no binding, conversion or evaluation.</summary>
internal static class CSharp6ContextPolicy
{
 /// <summary>The one tie the sixth-edition contract accepts: a nameof expression and the invocation of a simple name nameof (§11.7.20).</summary>
 internal static bool SanctionedAmbiguity(SyntaxTree tree,int node)
 {
  if(node<0||node>=tree.NodeCount||tree.Kind(node)!=SyntaxKinds.Ambiguous||tree.ChildCount(node)!=2)return false;
  string a=tree.Language!.GetKindName(tree.Kind(tree.Child(node,0))),b=tree.Language!.GetKindName(tree.Kind(tree.Child(node,1)));
  return (a,b) is ("NameofExpression","NameofInvocation") or ("NameofInvocation","NameofExpression");
 }
 internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
 {
  if(version.Major<6)return Array.Empty<ParseIssue>();
  var issues=new List<ParseIssue>();
  string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
  string Text(int n){var s=tree.Span(n);return tree.Text.Substring(s.Start,s.Length);}
  void Error(string code,int n,string message)=>issues.Add(new(code,message,tree.Span(n),IssueCategory.Syntax));
  int Ancestor(int n,params string[] kinds)
  {for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))if(kinds.Contains(Kind(p),StringComparer.Ordinal))return p;return -1;}
  // §11.7.3: a format colon is recognised when it is not nested in any bracket within the interpolation, so an
  // unparenthesized conditional's colon would start the format and leave its condition incomplete.
  bool UnnestedColon(int interpolation,int colon)
  {
   int start=tree.Span(interpolation).Start,end=tree.Span(colon).Start,depth=0;
   for(int n=0;n<tree.NodeCount;n++)
   {
    var s=tree.Span(n);
    if(tree.ChildCount(n)!=0||s.Length!=1||s.Start<start||s.Start>=end||Ancestor(n,"Interpolation")!=interpolation)continue;
    char c=tree.Text[s.Start];
    if(c is '(' or '[' or '{')depth++;else if(c is ')' or ']' or '}')depth--;
   }
   return depth==0;
  }
  for(int n=0;n<tree.NodeCount;n++)
  {
   string kind=Kind(n);
   if(kind=="ConditionalExpression")
   {
    int interpolation=Ancestor(n,"Interpolation");
    if(interpolation<0)continue;
    int colon=-1;
    for(int i=0;i<tree.ChildCount(n);i++){int c=tree.Child(n,i);if(c!=SyntaxTree.NoNode&&tree.ChildCount(c)==0&&Text(c)==":"){colon=c;break;}}
    int outer=Ancestor(n,"ConditionalExpression");
    bool reportedByOuter=outer>=0&&Ancestor(outer,"Interpolation")==interpolation&&UnnestedOuter(outer);
    if(colon>=0&&!reportedByOuter&&UnnestedColon(interpolation,colon))
     Error("CSN2062",n,"An unparenthesized conditional expression in an interpolation: its colon begins the format specifier.");
    bool UnnestedOuter(int o){for(int i=0;i<tree.ChildCount(o);i++){int c=tree.Child(o,i);if(c!=SyntaxTree.NoNode&&tree.ChildCount(c)==0&&Text(c)==":")return UnnestedColon(interpolation,c);}return false;}
   }
   else if(kind=="PropertyInitializer")
   {
    // §14.7.4: only an automatically implemented property (every accessor body a semicolon, not abstract or extern) has an initializer.
    int property=Ancestor(n,"PropertyDeclaration"),member=Ancestor(n,"MemberDeclaration");
    if(property<0)continue;
    bool automatic=true;
    foreach(var accessor in SyntaxView.Cast<PropertyDeclarationNode>(tree,property).Accessors)if(Text(accessor.Body.Index)!=";")automatic=false;
    if(member>=0)foreach(var modifier in SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers)if(Text(modifier.Index) is "abstract" or "extern")automatic=false;
    if(!automatic)Error("CSN2060",n,"A property initializer requires an automatically implemented property that is neither abstract nor extern.");
   }
   else if(kind=="ExpressionBody")
   {
    // §14.6.1, §14.10, §14.11: methods, operators and conversion operators only in the sixth edition.
    int owner=Ancestor(n,"MethodDeclaration","OperatorDeclaration","ConversionDeclaration","ConstructorDeclaration","DestructorDeclaration","Accessor");
    if(owner>=0&&Kind(owner) is "ConstructorDeclaration" or "DestructorDeclaration" or "Accessor")
     Error("CSN2061",n,"Constructors, finalizers and accessors cannot have expression bodies in C# 6.");
   }
  }
  return issues.AsReadOnly();
 }
}
