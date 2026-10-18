using Nitrogen;
using CSNitrogen.Syntax;
namespace CSNitrogen.Parsing;
/// <summary>Historical source-local member restrictions; pairing, erasure and extension lookup are excluded.</summary>
internal static class CSharp3DeclarationPolicy
{
 internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
 {
  var issues=new List<ParseIssue>();if(version.Major<3)return issues;
  string Kind(int n)=>tree.Language!.GetKindName(tree.Kind(n));
  string Text(int n){var s=tree.Span(n);return tree.Text.Substring(s.Start,s.Length);}
  string ModifierName(int n)=>UnicodePolicy.TryNormalize(Text(n),version,true,out string name)?name:Text(n);
  int Ancestor(int n,params string[] names)
  {for(int p=tree.Parent(n);p>=0;p=tree.Parent(p))if(names.Contains(Kind(p),StringComparer.Ordinal))return p;return -1;}
  void Error(int n,string message)=>issues.Add(new("CSN2032",message,tree.Span(n),IssueCategory.Syntax));
  bool Has(SyntaxList<ModifierNode> modifiers,string value)
  {foreach(var m in modifiers)if(ModifierName(m.Index)==value)return true;return false;}
  for(int n=0;n<tree.NodeCount;n++)
  {
   string kind=Kind(n);
   if(kind=="FixedParameter")
   {
    var parameter=SyntaxView.Cast<FixedParameterNode>(tree,n);if(!parameter.Receiver.HasValue)continue;
    int method=Ancestor(n,"MethodDeclaration","ConstructorDeclaration","DelegateDeclaration","IndexerDeclaration","OperatorDeclaration","InterfaceMethod");
    if(method<0||Kind(method)!="MethodDeclaration"){Error(n,"An extension receiver requires a method declaration.");continue;}
    var declaration=SyntaxView.Cast<MethodDeclarationNode>(tree,method);
    int first=-1;for(int i=declaration.Parameters.Index+1;i<tree.NodeCount&&tree.Span(i).Start<tree.Span(declaration.Parameters.Index).End;i++)if(Kind(i)=="FixedParameter"){first=i;break;}
    int member=Ancestor(method,"MemberDeclaration");
    int owner=Ancestor(method,"ClassDeclaration","StructDeclaration","InterfaceDeclaration");
    bool valid=first==n&&!parameter.Mode.HasValue&&member>=0&&Has(SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers,"static")&&owner>=0&&Kind(owner)=="ClassDeclaration";
    if(valid)
    {
     var type=SyntaxView.Cast<ClassDeclarationNode>(tree,owner);
     valid=Has(type.Modifiers,"static")&&!type.TypeParameters.HasValue&&Ancestor(owner,"ClassDeclaration","StructDeclaration","InterfaceDeclaration")<0;
    }
    if(SyntaxView.TryCast<TypeSyntaxTypeReference>(tree,parameter.Type.Index,out var receiverType) &&
       receiverType.Ranks.Count==0 && Kind(receiverType.Element.Index) is "PointerTypeName" or "VoidPointerTypeName")valid=false;
    if(!valid)Error(n,"The receiver must be the first unmodified, non-pointer parameter of a static method in a non-generic, non-nested static class.");
   }
   if(kind=="MemberDeclaration")
   {
    var member=SyntaxView.Cast<MemberDeclarationNode>(tree,n);if(!Has(member.Modifiers,"partial"))continue;
    if(Kind(member.Body.Index)!="MethodDeclaration")continue; // Shared declaration policy rejects other owners.
    var method=SyntaxView.Cast<MethodDeclarationNode>(tree,member.Body.Index);
    bool valid=Text(method.Return.Index)=="void"&&member.Modifiers.Count>0&&ModifierName(member.Modifiers[member.Modifiers.Count-1].Index)=="partial";
    foreach(var m in member.Modifiers)if(ModifierName(m.Index) is not ("partial" or "static" or "unsafe"))valid=false;
    int partialCount=0;foreach(var m in member.Modifiers)if(ModifierName(m.Index)=="partial")partialCount++;if(partialCount!=1)valid=false;
    int owner=Ancestor(n,"ClassDeclaration","StructDeclaration","InterfaceDeclaration");
    valid &= owner>=0 && Kind(owner) is "ClassDeclaration" or "StructDeclaration";
    if(owner>=0 && Kind(owner)=="ClassDeclaration")valid &= Has(SyntaxView.Cast<ClassDeclarationNode>(tree,owner).Modifiers,"partial");
    if(owner>=0 && Kind(owner)=="StructDeclaration")valid &= Has(SyntaxView.Cast<StructDeclarationNode>(tree,owner).Modifiers,"partial");
    valid &= !method.NamePrefix.HasValue&&!method.NameExtension.HasValue&&method.Name.Parts.Count==1;
    for(int i=method.Parameters.Index+1;i<tree.NodeCount&&tree.Span(i).Start<tree.Span(method.Parameters.Index).End;i++)
     if(Kind(i)=="FixedParameter")
     {var p=SyntaxView.Cast<FixedParameterNode>(tree,i);if(p.Mode.HasValue&&Text(p.Mode.Value.Index)=="out")valid=false;}
    if(!valid)Error(n,"A historical partial method requires a partial class/struct, final partial modifier, void return, no access/virtual/extern modifiers, no explicit interface name and no out parameters.");
   }
   if(kind=="PropertyDeclaration")
   {
    var property=SyntaxView.Cast<PropertyDeclarationNode>(tree,n);int member=Ancestor(n,"MemberDeclaration");
    if(member<0)continue;var modifiers=SyntaxView.Cast<MemberDeclarationNode>(tree,member).Modifiers;
    if(Has(modifiers,"abstract")||Has(modifiers,"extern"))continue;
    int semicolons=0;foreach(var accessor in property.Accessors)if(Text(accessor.Body.Index)==";")semicolons++;
    if(semicolons>0&&(semicolons!=2||property.Accessors.Count!=2))Error(n,"A historical automatic property requires both get and set accessors, each with a semicolon body.");
   }
  }
  return issues;
 }
}
