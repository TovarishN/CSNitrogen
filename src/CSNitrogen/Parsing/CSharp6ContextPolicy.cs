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
  bool Static(int member)
  {if(member<0)return false;foreach(var modifier in SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers)if(Text(modifier.Index)=="static")return true;return false;}
  int Ancestor(int n,params string[] kinds)
  {for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))if(kinds.Contains(Kind(p),StringComparer.Ordinal))return p;return -1;}
  // §11.7.3: a format colon is recognised when it is not nested in any bracket within the interpolation, so an
  // unparenthesized conditional's colon would start the format and leave its condition incomplete. One in-order walk
  // per interpolation tracks bracket depth over its own tokens; nested interpolations are walked separately.
  void CheckInterpolation(int interpolation)
  {
   var unnested=new List<int>();int depth=0;
   void Walk(int node)
   {
    int count=tree.ChildCount(node);
    if(count==0)
    {
     var s=tree.Span(node);if(s.Length!=1)return;
     char c=tree.Text[s.Start];
     if(c is '(' or '[' or '{')depth++;
     else if(c is ')' or ']' or '}')depth--;
     else if(c==':'&&depth==0&&tree.Parent(node) is int owner&&owner>=0&&Kind(owner)=="ConditionalExpression")unnested.Add(owner);
     return;
    }
    for(int i=0;i<count;i++){int child=tree.Child(node,i);if(child!=SyntaxTree.NoNode&&Kind(child)!="Interpolation")Walk(child);}
   }
   Walk(interpolation);
   var set=unnested.ToHashSet();
   foreach(int conditional in unnested)
   {
    bool outerReported=false;
    for(int p=tree.Parent(conditional);p>=0&&p!=interpolation;p=tree.Parent(p))if(set.Contains(p)){outerReported=true;break;}
    if(!outerReported)Error("CSN2062",conditional,"An unparenthesized conditional expression in an interpolation: its colon begins the format specifier.");
   }
  }
  for(int n=0;n<tree.NodeCount;n++)
  {
   string kind=Kind(n);
   if(kind=="Interpolation")CheckInterpolation(n);
   else if(kind=="PropertyInitializer")
   {
    // §14.7.4: only an automatically implemented property (every accessor body a semicolon, not abstract or extern) has an initializer.
    int property=Ancestor(n,"PropertyDeclaration"),member=Ancestor(n,"MemberDeclaration");
    if(property<0)continue;
    bool automatic=true;
    foreach(var accessor in SyntaxView.Cast<PropertyDeclarationNode>(tree,property).Accessors)if(Text(accessor.Body.Index)!=";")automatic=false;
    if(member>=0)foreach(var modifier in SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers)if(Text(modifier.Index) is "abstract" or "extern")automatic=false;
    if(!automatic)Error("CSN2060",n,"A property initializer requires an automatically implemented property that is neither abstract nor extern.");
    // §14.7.4 applies the initializer to the backing field, and §15.4.8 forbids instance field initializers in structs.
    else if(Ancestor(n,"ClassDeclaration","StructDeclaration","InterfaceDeclaration") is int type&&type>=0&&Kind(type)=="StructDeclaration"&&!Static(member))
     issues.Add(new("CSN2063",version<new Version(10,0)?"Struct instance property initializers require C# 10.":"Struct instance property initializers are not implemented in this composition.",
      tree.Span(n),version<new Version(10,0)?IssueCategory.UnavailableFeature:IssueCategory.UnsupportedCoverage));
   }
   else if(kind=="ExpressionBody")
   {
    // §14.6.1, §14.10, §14.11: methods, operators and conversion operators only in the sixth edition.
    int owner=Ancestor(n,"MethodDeclaration","OperatorDeclaration","ConversionDeclaration","ConstructorDeclaration","DestructorDeclaration","Accessor");
    // C# 7.0 adds these expression bodies; later profiles are not restricted here.
    if(version.Major==6&&owner>=0&&Kind(owner) is "ConstructorDeclaration" or "DestructorDeclaration" or "Accessor")
     Error("CSN2061",n,"Constructors, finalizers and accessors cannot have expression bodies in C# 6.");
   }
  }
  return issues.AsReadOnly();
 }
}
