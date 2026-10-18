using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3UnsafeSourceTests
{
 [Theory]
 [InlineData("unsafe class C{int* p;void** q;int*[] xs;}",true)]
 [InlineData("unsafe class C{void M(){int* p=stackalloc int[10];int** q=&p;*p=1;p->X=2;p[0]++;}}",true)]
 [InlineData("unsafe class C{void M(){fixed(int* p=&x,q=&y){M();}}}",true)]
 [InlineData("unsafe class C{void M(){fixed(A::N.G<int>* p=x)M();}}",true)]
 [InlineData("unsafe struct S{public fixed int x[5],y[10];}",true)]
 [InlineData("using I=System.Int32;unsafe struct S{fixed I x[5];}",true)]
 [InlineData("unsafe struct S{fixed A::N.G<T> x[5];}",true)]
 [InlineData("unsafe class C{object M(){return p->M<int>();}}",true)]
 [InlineData("unsafe class C{object M(){return sizeof(A::N.G<int>*);}}",true)]
 [InlineData("unsafe class C{object M(){return (A::N.G<int>*)p;}}",true)]
 [InlineData("class C{void M(){unsafe {int* p=stackalloc int[1];}}}",true)]
 [InlineData("unsafe class C{object M(){yield break;}}",true)]
 [InlineData("unsafe class C{void M(){fixed(int* p)M();}}",false)]
 [InlineData("unsafe class C{void M(){fixed(int p=x)M();}}",false)]
 [InlineData("unsafe class C{void M(){fixed(int* p=x,)M();}}",false)]
 [InlineData("unsafe class C{void M(){fixed(int* p=x)int x;}}",false)]
 [InlineData("unsafe class C{object M(){return stackalloc int[1];}}",false)]
 [InlineData("unsafe class C{int* p=stackalloc int[1];}",false)]
 [InlineData("unsafe class C{void M(){int* p=(stackalloc int[1]);}}",false)]
 [InlineData("unsafe class C{void M(){int* p=stackalloc int[];}}",false)]
 [InlineData("unsafe class C{fixed int x[1];}",false)]
 [InlineData("unsafe struct S{static fixed int x[1];}",false)]
 [InlineData("unsafe struct S{fixed decimal x[1];}",false)]
 [InlineData("unsafe struct S{fixed int[] x[1];}",false)]
 [InlineData("unsafe struct S{fixed int? x[1];}",false)]
 [InlineData("unsafe struct S{fixed int* x[1];}",false)]
 [InlineData("unsafe struct S{fixed int x[];}",false)]
 [InlineData("unsafe struct S{fixed int x[1] y[2];}",false)]
 [InlineData("unsafe class C{void M(){unsafe M();}}",false)]
 [InlineData("unsafe class C{object M(){return p->;}}",false)]
 public void Source_unsafe_forms_and_context_neighbors_are_checked(string source,bool accepted)
 {
  source="/*𐐀*/"+source;using var p=CSharpParser.Parse(new(source,"CSharp3","unsafe-source.cs",[]));
  Assert.True(p.Accepted==accepted,source+" :: "+string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
  foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var s=p.Tree.Span(n);Assert.InRange(s.Start,0,source.Length);Assert.InRange(s.Length,0,source.Length-s.Start);}
  foreach(var i in p.Issues){Assert.InRange(i.Span.Start,0,source.Length);Assert.InRange(i.Span.Length,0,source.Length-i.Span.Start);}
 }
}
