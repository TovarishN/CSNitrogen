using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3StatementSourceTests
{
 [Theory]
 [InlineData(";")][InlineData("{}")] [InlineData("label:label2:;")]
 [InlineData("int x,y=1;const int a=1,b=2;int[] xs={1,2,};")]
 [InlineData("var x=new {X=1};var xs=new []{1};")]
 [InlineData("var q=from x in xs select x;var f=(int x)=>x;")]
 [InlineData("if(a)M();else N();")][InlineData("if(a)if(b)M();else N();")]
 [InlineData("if(a){int x=1;label:;}")][InlineData("while(a);")]
 [InlineData("do {M();}while(a);")][InlineData("for(;;);")]
 [InlineData("for(int i=0,j=1;i<j;i++,j--)M();")]
 [InlineData("for(M(),i=0;a;N(),++i);")][InlineData("foreach(var x in xs)M();")]
 [InlineData("foreach(G<T>[] x in xs){M();}")]
 [InlineData("switch(x){}")][InlineData("switch(x){case 1:case 2:M();break;default:N();break;}")]
 [InlineData("try{}catch(E e){}catch(F){}catch{}finally{}")]
 [InlineData("try{}catch{}")][InlineData("try{}finally{}")]
 [InlineData("lock(x)using(r)M();")][InlineData("using(R r=M(),s=N()){M();}")]
 [InlineData("checked{unchecked{M();}}")][InlineData("M();new C();new C{X=1};a=b;c++;--d;")]
 [InlineData("break;continue;goto missing;goto case 1;goto default;return;throw;")]
 [InlineData("return x;")][InlineData("yield return x;yield break;")]
 [InlineData("try{yield return x;}finally{M();}")]
 [InlineData("try{yield break;}catch(E e){yield break;}catch{yield break;}")]
 [InlineData("try{try{}catch{}}finally{}yield return x;")]
 [InlineData("object f=()=>{return 1;};yield break;")]
 [InlineData("object f=delegate(ref int x){return x;};yield break;")]
 [InlineData("object f=()=>{try{M();}catch{}return 1;};yield return x;")]
 [InlineData("int yield=0;yield++;yield=M();")]
 public void Source_statement_forms_and_iterator_neighbors_are_accepted(string statements)
 {Check(statements,true);}
 [Theory]
 [InlineData("if(a)int x;")][InlineData("if(a)label:;")][InlineData("while(a)const int x=1;")]
 [InlineData("for(;;)label:;")][InlineData("using(r)int x;")][InlineData("lock(x)label:;")]
 [InlineData("do{}while(a)")][InlineData("for(;)M();")][InlineData("foreach(var x xs);")]
 [InlineData("int x=;")][InlineData("const int x;")][InlineData("label:")]
 [InlineData("a+b;")][InlineData("new []{1};")][InlineData("new {X=1};")][InlineData("(M());")]
 [InlineData("for(a+b;;);")][InlineData("for(;;a+b);")][InlineData("for(const int x=1;;);")]
 [InlineData("switch(x){case 1:}")][InlineData("switch(x){default:;default:;}")]
 [InlineData("try{}")][InlineData("try{}catch{}catch(E e){}")]
 [InlineData("try{}catch{}catch{}")][InlineData("try{}catch(E e);")]
 [InlineData("try{}finally;")][InlineData("catch{}")][InlineData("using(){M();}")]
 [InlineData("yield return;")][InlineData("yield break 1;")]
 [InlineData("object f=()=>{yield break;};")][InlineData("object f=x=>{yield return x;};")]
 [InlineData("try{}finally{yield break;}")][InlineData("try{yield return x;}catch{}")] 
 [InlineData("try{}catch{yield return x;}")][InlineData("yield break;return;")]
 [InlineData("yield return x;unsafe{}")]
 public void Malformed_categories_and_iterator_placement_are_rejected(string statements)
 {Check(statements,false);}
 [Theory]
 [InlineData("class C{C(){yield break;}}")][InlineData("class C{~C(){yield break;}}")]
 [InlineData("class C{object M(ref int x){yield break;}}")]
 [InlineData("class C{object M(out int x){x=0;yield break;}}")]
 [InlineData("class C{event E Ev{add{yield break;}remove{}}}")]
 public void Iterator_declaration_owners_and_modes_are_restricted(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","owner.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code=="CSN2022");}
 [Fact]public void Dangling_else_attaches_to_the_inner_if_with_original_ranges()
 {
  const string source="/*𐐀*/class C{void M(){if(a)if(b)M();else N();}}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","if.cs",[]));Assert.True(p.Accepted);
  var nodes=Enumerable.Range(0,p.Tree.NodeCount).Where(n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="IfStatement").OrderBy(n=>p.Tree.Span(n).Start).ToArray();Assert.Equal(2,nodes.Length);
  var outer=SyntaxView.Cast<IfStatementNode>(p.Tree,nodes[0]);var inner=SyntaxView.Cast<IfStatementNode>(p.Tree,nodes[1]);
  Assert.False(outer.Else.HasValue);Assert.True(inner.Else.HasValue);
  Assert.Equal("a",source.Substring(outer.Condition.Span.Start,outer.Condition.Span.Length));Assert.Equal("b",source.Substring(inner.Condition.Span.Start,inner.Condition.Span.Length));
  Assert.Equal("else N();",source.Substring(inner.Else.Value.Span.Start,inner.Else.Value.Span.Length));
 }
 [Fact]public void Iterator_diagnostic_retains_the_entire_original_yield_range()
 {
  const string source="/*𐐀*/class C{object f=()=>{yield /*gap*/ break;};}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","yield.cs",[]));var issue=Assert.Single(p.Issues,i=>i.Code=="CSN2022");
  Assert.Equal(IssueCategory.Syntax,issue.Category);Assert.Equal("yield /*gap*/ break;",source.Substring(issue.Span.Start,issue.Span.Length));
 }
 private static void Check(string statements,bool expected)
 {
  string source="/*𐐀*/class C{object M(){"+statements+"}}";
  using var p=CSharpParser.Parse(new(source,"CSharp3","statement-source.cs",[]));
  Assert.True(p.Accepted==expected,statements+" :: "+string.Join(";",p.Issues));Assert.True(p.ProfileComplete);
  foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var s=p.Tree.Span(n);Assert.InRange(s.Start,0,source.Length);Assert.InRange(s.Length,0,source.Length-s.Start);}
  foreach(var i in p.Issues){Assert.InRange(i.Span.Start,0,source.Length);Assert.InRange(i.Span.Length,0,source.Length-i.Span.Start);}
 }
}
