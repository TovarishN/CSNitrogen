using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3DeclarationSourceTests
{
 [Theory]
 [InlineData("")][InlineData("extern alias A;using N;using X=A::N.G<int>;[assembly:A]namespace N{using Y=G<int>;class C{}};")]
 [InlineData("namespace N.M{extern alias A;using X=A::T;public class C{}}")]
 [InlineData("[A]public partial class C<[B]T,U>:B<T>,I<U> where T:class,I,new() where U:struct{}")]
 [InlineData("partial struct S<T>:I where T:class{}")] [InlineData("partial interface I<T>:J<T> where T:class{}")]
 [InlineData("delegate T D<T>(ref T x,out int y,params int[] z) where T:class;")]
 [InlineData("class C{[A]const int X=1,Y=2;[B]static readonly int[] xs={1,2,};}")]
 [InlineData("class C{public C(int x):base(x){}C():this(1){}static C(){}~C(){}}")]
 [InlineData("class C{T M<T>([A]ref T x,params int[][] xs) where T:class,new(){return x;}}")]
 [InlineData("class C{void I<T>.M<U>(U x) where U:class{}}")]
 [InlineData("class C{int P{set{}get{return 1;}}int Q{get;private set;}}")]
 [InlineData("class C{public int this[int i,params int[] rest]{get{return 1;}set{}}}")]
 [InlineData("class C{int I<T>.P{get{return 1;}set{}}int I<T>.this[int i]{get{return 1;}}}")]
 [InlineData("class C{event E x,y;event E I<T>.Ev{remove{}add{}}}")]
 [InlineData("class C{public static C operator +(C x,C y){return x;}public static bool operator true(C x){return true;}public static bool operator false(C x){return false;}public static explicit operator int(C x){return 1;}}")]
 [InlineData("interface I<T>{void M();U N<U>(U x) where U:class;int P{set;get;}event E Ev;int this[int i]{get;}}")]
 [InlineData("enum E:byte{[A]X=1,Y,}")] [InlineData("class C{class N{}struct S{}interface I{}enum E{}delegate void D();}")]
 [InlineData("partial class C{static partial void M<T>(ref T x) where T:class;static partial void M<T>(ref T x) where T:class{}}")]
 [InlineData("static class E{public static T M<T>(this T x,params int[] rest){return x;}}")]
 [InlineData("[A(1,2,X=3),B(),]class C<[type:A]T>{[field:A]int x;[return:A]int M([param:A]int x){return x;}}")]
 [InlineData("[module:A][assembly:B]class C{}")]
 [InlineData("[A((x=1),Y=2)]class C{}")]
 public void Historical_declaration_forms_execute_in_the_third_factory(string source)
 {Check(source,true);}
 [Theory]
 [InlineData("class C{}using N;")][InlineData("using N;extern alias A;")][InlineData("namespace N{[assembly:A]class C{}}")]
 [InlineData("class C;")][InlineData("class C<> {}")][InlineData("class C<T,>{}")]
 [InlineData("class C<T> where T:class,struct{}")] [InlineData("class C<T> where T:new(),I{}")]
 [InlineData("class C<T> where T:struct,new(){}")] [InlineData("class C<T> where T:int{}")]
 [InlineData("class C where T:class{}")] [InlineData("class C{void M(int x,){}}")]
 [InlineData("class C{void M(params int[] xs,int y){}}")] [InlineData("class C{void M(int x=1){}}")]
 [InlineData("class C{int this[]{get{return 1;}}}")] [InlineData("class C{int this[ref int x]{get{return 1;}}}")]
 [InlineData("class C{int P{get;}}")] [InlineData("class C{int P{get;set{}}}")]
 [InlineData("class C{event E Ev{add{}}}")] [InlineData("class C{event E Ev{add;remove;}}")]
 [InlineData("interface I{void M(){}}")] [InlineData("interface I{int x;}")] [InlineData("interface I{int P{private get;}}")]
 [InlineData("enum E{,}")] [InlineData("enum E{X,,Y}")]
 [InlineData("struct S{public S(){}}")] [InlineData("struct S{~S(){}}")]
 [InlineData("partial class C{public partial void M();}")] [InlineData("partial class C{partial int M();}")]
 [InlineData("partial class C{partial void M(out int x);}")] [InlineData("class C{partial void M();}")]
 [InlineData("class E{static void M(this int x){}}")] [InlineData("static class E<T>{static void M(this T x){}}")]
 [InlineData("static class E{static void M(int x,this int y){}}")] [InlineData("static class E{static void M(this ref int x){}}")]
 [InlineData("[typevar:A]class C<T>{}")][InlineData("[unknown:A]class C{}")] [InlineData("[A(X=1,2)]class C{}")]
 [InlineData("[A(1,)]class C{}")] [InlineData("[assembly:A(1,)]class C{}")]
 public void Malformed_or_wrong_historical_declaration_categories_are_rejected(string source)
 {Check(source,false);}
 [Fact]public void Reviewed_enum_underlying_type_legality_remains_deferred()
 {Check("enum E:char{}",true);}
 private static void Check(string source,bool expected)
 {
  source="/*𐐀*/"+source;using var p=CSharpParser.Parse(new(source,"CSharp3","declaration-source.cs",[]));
  Assert.True(p.Accepted==expected,source+" :: "+string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
  foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var s=p.Tree.Span(n);Assert.InRange(s.Start,0,source.Length);Assert.InRange(s.Length,0,source.Length-s.Start);}
  foreach(var i in p.Issues){Assert.InRange(i.Span.Start,0,source.Length);Assert.InRange(i.Span.Length,0,source.Length-i.Span.Start);}
 }
}
