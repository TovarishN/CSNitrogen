using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3ExpressionSourceTests
{
 [Theory]
 [InlineData("M(ref x,out y,a=1)")][InlineData("base[1,2]")][InlineData("base.M<int>()")]
 [InlineData("int.Parse(s)")][InlineData("A::T.M<int>()")][InlineData("p->M<int>()")]
 [InlineData("new int[1,2][]{{a,b}}")][InlineData("new int[][]{new int[]{1},null}")]
 [InlineData("new []{new {X=1},new {X=2},}")][InlineData("(new []{1})[0]")]
 [InlineData("new C{Child={X=1},Items={1,{2,3}}}")][InlineData("new C{(x=1)}")]
 [InlineData("new C{{x=1,y=2}}")][InlineData("new {x,int.MaxValue,@class=1}")]
 [InlineData("typeof(void)")][InlineData("typeof(A::N.C<,>.D<>)")][InlineData("typeof(N.C<int>.D<string>[]) ")]
 [InlineData("default(N.C<int>?[])")][InlineData("sizeof(void*)")][InlineData("checked(a+unchecked(b*c))")]
 [InlineData("(T)+x")][InlineData("(T)-x")][InlineData("(T)*x")][InlineData("(T)&x")]
 [InlineData("(T)~x")][InlineData("(T)!x")][InlineData("(N.T)x")][InlineData("(G<T>[])x")]
 [InlineData("(T)(x=>x)")][InlineData("(int)-x")][InlineData("(T*)*p")]
 [InlineData("a?b=c:d=e")][InlineData("a??b??c")][InlineData("a=b=c")]
 [InlineData("a is T?b:c")][InlineData("a as T?b:c")][InlineData("a is T?[]")]
 [InlineData("(a is T).M()")][InlineData("(a as T)+b")][InlineData("F<T>==x")]
 [InlineData("F<T>!=x")][InlineData("new C{F<T>}")][InlineData("F<T>+x")]
 [InlineData("()=>x=>x")][InlineData("(ref int x,out int y)=>{y=x;}")]
 [InlineData("from T x in xs from y in ys let z=x where p join U w in ws on x equals w into g orderby x ascending,y descending select z into q group q by q")]
 [InlineData("from x in xs select new {x,@select=x}")]
 [InlineData("from x in xs select (y=>y)")]
 public void Source_forms_and_parenthesized_neighbors_are_accepted(string expression)
 {Check(expression,true);}
 [Theory]
 [InlineData("M(ref)")][InlineData("M(out x,)")][InlineData("base[]")][InlineData("a[]")]
 [InlineData("new int[1][2]")][InlineData("new int[]")][InlineData("new []")][InlineData("new []{,}")]
 [InlineData("new []{1}[0]")][InlineData("new int[1][0]")][InlineData("new C{1,x=2}")]
 [InlineData("new C{{}}")][InlineData("new {a[0]}")][InlineData("new {M()}")]
 [InlineData("typeof(C<int>.D<>)")][InlineData("typeof(C<>.D<int>)")][InlineData("typeof(C<>[])")]
 [InlineData("default(void)")][InlineData("(void)x")][InlineData("(T)++x")][InlineData("(T)--x")]
 [InlineData("a+b=c")][InlineData("a??b=c")][InlineData("a is T=b")]
 [InlineData("a as T.M()")][InlineData("a is T[0]")][InlineData("a as T+b")]
 [InlineData("a+ x=>x")][InlineData("a??x=>x")][InlineData("(T)x=>x")]
 [InlineData("()=>")][InlineData("(x,int y)=>x")][InlineData("(params int[] x)=>x")]
 [InlineData("from x in xs")][InlineData("from x in xs select x where p")]
 [InlineData("from x in xs select select")][InlineData("from x in xs select x.select")]
 [InlineData("from x in xs join y in ys on x equals y into select y")]
 public void Malformed_or_wrong_source_categories_are_rejected(string expression)
 {Check(expression,false);}
 [Theory]
 [InlineData("a+b*c","AddExpression","MultiplyExpression","b*c")]
 [InlineData("a<<b+c","ShiftLeftExpression","AddExpression","b+c")]
 [InlineData("a==b<c","EqualExpression","LessExpression","b<c")]
 [InlineData("a|b^c&d","BitOrExpression","BitXorExpression","b^c&d")]
 [InlineData("a||b&&c","OrExpression","AndExpression","b&&c")]
 [InlineData("a??b??c","NullCoalescingExpression","NullCoalescingExpression","b??c")]
 [InlineData("a=b=c","AssignmentExpression","AssignmentExpression","b=c")]
 [InlineData("a?b:c?d:e","ConditionalExpression","ConditionalExpression","c?d:e")]
 [InlineData("(A::T)-x","CastExpression","UnaryExpression","-x")]
 [InlineData("(T)-x","SubtractExpression","ParenthesizedExpression","(T)")]
 public void Ambiguity_and_precedence_keep_authored_nested_expression_ranges(string expression,string rootKind,string nestedKind,string nestedText)
 {
  string source="/*𐐀*/unsafe class C{object M(){return "+expression+";}}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","shape.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  int start=source.IndexOf(expression,StringComparison.Ordinal);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==rootKind&&p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==expression.Length);
  int nested=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>n!=root&&p.Tree.Language!.GetKindName(p.Tree.Kind(n))==nestedKind&&source.Substring(p.Tree.Span(n).Start,p.Tree.Span(n).Length)==nestedText);
  for(int n=nested;;n=p.Tree.Parent(n)){Assert.True(n>=0);if(n==root)break;}
 }
 [Theory]
 [InlineData("bool",true)][InlineData("decimal",true)][InlineData("string",true)][InlineData("object",true)]
 [InlineData("A::N.G<int>.H<T[]>?[,][]",true)][InlineData("int*[]",true)][InlineData("void**",true)]
 [InlineData("int?",true)][InlineData("T?[]",true)][InlineData("G<T>[][,]",true)]
 [InlineData("void",false)][InlineData("int[]?",false)][InlineData("int*?",false)]
 [InlineData("int??",false)][InlineData("string?",false)][InlineData("int[1]",false)]
 [InlineData("G<>",false)][InlineData("G<int,>",false)]
 public void Type_shapes_preserve_grammar_and_defer_named_type_identity(string type,bool accepted)
 {
  string source="unsafe class C{"+type+" field;}";using var p=CSharpParser.Parse(new(source,"CSharp3","type-source.cs",[]));
  Assert.True(p.Accepted==accepted,type+" :: "+string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
 }
 private static void Check(string expression,bool expected)
 {
  string source="/*𐐀*/unsafe class C{object M(){return "+expression+";}}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","expression-source.cs",[]));
  Assert.True(p.Accepted==expected,expression+" :: "+string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
  foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var s=p.Tree.Span(n);Assert.InRange(s.Start,0,source.Length);Assert.InRange(s.Length,0,source.Length-s.Start);}
  foreach(var i in p.Issues){Assert.InRange(i.Span.Start,0,source.Length);Assert.InRange(i.Span.Length,0,source.Length-i.Span.Start);}
 }
}
