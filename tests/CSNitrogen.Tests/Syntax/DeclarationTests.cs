using CSNitrogen.Syntax;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class DeclarationTests
{
 [Theory]
 [InlineData("class C { const int N=1; int x=2,y; public C():base(){} ~C(){} int P {get{return x;} set{x=value;}} event E Changed; int this[int i] {get{return x;}} void M(ref int x,params object[] a){} class Nested {} }",true)]
 [InlineData("struct S:I { public int M(){return 1;} }",true)]
 [InlineData("interface I { void M(); int P {get;set;} event E Changed; int this[int i]{get;} }",true)]
 [InlineData("enum E:byte {A=1,B,}",true)][InlineData("delegate void D(object x);",true)]
 [InlineData("class C { public static C operator +(C a,C b){return a;} public static implicit operator int(C a){return 0;} void I.M(){} event E I.Changed {add{}remove{}} }",true)]
 [InlineData("interface I { void M(){} }",false)][InlineData("class C<T> {}",false)]
 [InlineData("partial class C {}",false)][InlineData("class C {int P {get;private set;}}",false)]
 [InlineData("class C { int M()=>1; }",false)]
 public void Declaration_surface(string source,bool accepted)
 {var l=CSharp1Composition.CreateBuilder().Build();using var p=l.Parse(source,DeclarationsModule.DeclarationFragment);Assert.Equal(accepted,p.Success&&!p.HasErrors);}
 [Fact]
 public void Generated_member_views_retain_authored_fields_and_parameters()
 {
  const string source="class C { int x=2,y; void M(ref int value){} }";
  using var document=CSharpParser.Parse(new(source,"CSharp1","typed.cs",[]));Assert.True(document.Accepted);
  Assert.Equal(2,Assert.Single(document.Classes).Members.Count);
  var tree=document.Tree;
  int Node(string kind)=>Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>tree.Language!.GetKindName(tree.Kind(n))==kind);
  var field=SyntaxView.Cast<FieldDeclarationNode>(tree,Node("FieldDeclaration"));
  Assert.Equal(2,field.Value.Variables.Count);Assert.Equal("x",field.Value.Variables[0].Name.ToString());
  Assert.Equal("y",field.Value.Variables[1].Name.ToString());
  var parameter=SyntaxView.Cast<FixedParameterNode>(tree,Node("FixedParameter"));
  Assert.Equal("value",parameter.Name.ToString());Assert.Equal(source.IndexOf("value",StringComparison.Ordinal),tree.Span(parameter.Name.Index).Start);
 }
}
