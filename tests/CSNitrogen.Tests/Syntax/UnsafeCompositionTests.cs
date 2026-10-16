using CSNitrogen.Syntax;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class UnsafeCompositionTests
{
 [Theory][InlineData("unsafe class C { int* p; void M(){unsafe {int* q=stackalloc int[4]; *q=1; fixed(int* r=&x){r->M();}}} int x; }",true,false)]
 [InlineData("class C {void M(){ sizeof(int); }}",false,false)]
 [InlineData("class C {int M(){return sizeof(int);}}",true,true)]
 [InlineData("unsafe class C {int M(){return sizeof(C);}}",true,false)]
 [InlineData("unsafe class C { fixed int data[16]; }",false,false)]
 [InlineData("unsafe class C {void M(){int* p=stackalloc[] {1,2};}}",false,false)]
 public void Unsafe_membership_changes_syntax(string source,bool fullExpected,bool safeExpected)
 {
  using var full=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));Assert.Equal(fullExpected,full.Accepted);
  var safe=CSharp1SafeComposition.CreateBuilder().Build();using var p=safe.Parse(source,CSharp1SafeComposition.Start);
  Assert.Equal(safeExpected,p.Success&&!p.HasErrors&&!CSharpSyntaxPolicy.Check(p.Tree).Any());
  Assert.DoesNotContain(safe.Modules,m=>m.Name=="CSharp.Unsafe");
 }
}
