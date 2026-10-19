using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4ProductionTests
{
 [Theory]
 [InlineData("class C{void M(){F(x:1);}}")]
 [InlineData("class C{void M(){F(1,y:2,z:ref a,w:out b);}}")]
 [InlineData("class C{void M(){new C(x:1);}}")]
 [InlineData("class C{C():base(x:1){}}")]
 [InlineData("class C{C():this(x:1){}}")]
 [InlineData("class C{object M(){return a[0,index:1]+base[index:2];}}")]
 [InlineData("class C{void M(int x=1,string y=null){}}")]
 [InlineData("class C{C(int x=1){}}")]
 [InlineData("delegate void D(int x=1);")]
 [InlineData("interface I{void M(int x=1);}")]
 [InlineData("class C{void M(int x=1,params int[] rest){}}")]
 [InlineData("interface I<out T,in U>{}")]
 [InlineData("delegate T D<out T,in U>(U x);")]
 public void Fourth_additions_are_available_from_four_onward_but_not_three(string source)
 {
  foreach(string profile in new[]{"CSharp4","CSharp7_3","CSharp14"})
  {using var p=CSharpParser.Parse(new(source,profile,"four.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);}
  using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.False(third.Accepted);Assert.True(third.ProfileComplete);
 }
 [Theory]
 [InlineData("class C{void M(){F(x:1,2);}}")]
 [InlineData("class C{void M(){F(x:1,x:2);}}")]
 [InlineData(@"class C{void M(){F(x:1,\u0078:2);}}")]
 [InlineData("class C{void M(){F(x:1,@x:2);}}")]
 [InlineData("class C{void M(){F(x:1,x\u200c:2);}}")]
 [InlineData("class C{object M(){return a[x:1,x:2];}}")]
 [InlineData("class C{object M(){return base[x:1,@x:2];}}")]
 [InlineData("class C{object M(){return a[x:1,2];}}")]
 [InlineData("class C{object M(){return a[x:ref y];}}")]
 [InlineData("class C{object M(){return new int[2][x:1];}}")]
 [InlineData("class C{object M(){return a is T[x:1];}}")]
 [InlineData("class C{object M(){return a as T[x:1];}}")]
 [InlineData("class C{object M(){return delegate(int x=1){};}}")]

 [InlineData("class C{void M(int x=1,int y){}}")]
 [InlineData("class C{void M(ref int x=1){}}")]
 [InlineData("class C{void M(out int x=1){}}")]
 [InlineData("class C<out T>{}")]
 [InlineData("struct S<in T>{}")]
 [InlineData("class C{void M<out T>(){}}") ]
 [InlineData("interface I{void M<out T>();}")]
 [InlineData("class C{object x=(int y=1)=>y;}")]
 public void Fourth_source_local_restrictions_are_enforced(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","bad.cs",[]));Assert.False(p.Accepted);}
 [Theory][InlineData("class 𐐀{}")][InlineData(@"class \U00010400{}")]
 [InlineData("\u180eclass C{}")][InlineData("#define 𐐀\n#if 𐐀\nclass C{}\n#endif")]
 public void Fourth_uses_the_approved_frozen_scalar_contract(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","unicode.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Contains("unicode-4.0-scalar",ProfileResolver.Resolve("CSharp4").Profile!.Policies);}
 [Theory][InlineData("csharp4.named-arguments")][InlineData("csharp4.optional-parameters")][InlineData("csharp4.variance")]
 public void Fourth_capabilities_require_explicit_dependencies(string id)
 {var r=ProfileResolver.ResolveCustom("missing",[id]);Assert.Null(r.Profile);Assert.Contains(r.Issues,i=>i.Code=="missing-requirement");}
 [Fact]public void Dynamic_remains_an_ordinary_type_and_identifier_spelling()
 {using var p=CSharpParser.Parse(new("class dynamic{dynamic dynamic;}","CSharp4","dynamic.cs",[]));Assert.True(p.Accepted);}
 [Theory]
 [InlineData("class C{void M(){F(x:1,2);}}","2")]
 [InlineData("class C{void M(){F(x:1,@x:2);}}","@x")]
 [InlineData("class C{void M(int x=1,int y){}}","int y")]
 [InlineData("class C{void M(ref int x=1){}}","=1")]
 [InlineData("class C<out T>{}","out")]
 public void Policy_issues_point_to_authored_source(string body,string rejected)
 {
  string source="/*𐐀*/"+body;using var p=CSharpParser.Parse(new(source,"CSharp4","span.cs",[]));
  var issue=Assert.Single(p.Issues,i=>i.Code=="CSN2040");
  Assert.Equal(source.IndexOf(rejected,StringComparison.Ordinal),issue.Span.Start);
  Assert.Equal(rejected,source.Substring(issue.Span.Start,issue.Span.Length));
 }
 [Theory]
 [InlineData("class C{void M(){F(x:F(x:1),y:2);G(0,x:1);}}")]
 [InlineData("interface I<out T>{void M<U>();}")]
 [InlineData("class C{object M(){return p->M[x:1];}}")]
 [InlineData("class C{void M(int x=1){} void N(int y){}}")]
 public void Policy_state_is_local_to_the_owning_list(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","scope.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}

 [Fact]public void Fourth_context_policy_is_inactive_for_predecessors()
 {
  var language=CSharp4Composition.CreateBuilder().Build();
  using var raw=language.Parse("class C<out T>{void M(ref int x=1){F(x:1,2);}}",CSharp4Composition.Start);
  Assert.True(raw.Success&&!raw.HasErrors);
  foreach(int major in new[]{1,2,3})Assert.Empty(CSharp4ContextPolicy.Check(raw.Tree,new Version(major,0)));
  Assert.NotEmpty(CSharp4ContextPolicy.Check(raw.Tree,new Version(4,0)));
 }
 [Theory]
 [InlineData("class C{void M(int x=")][InlineData("interface I<out")]
 [InlineData("class C{void M(){F(x:")][InlineData("class C{object M(){return a[x:")]
 [InlineData("delegate T D<in")][InlineData("class C{void M(ref int x=){}}")]
 public void Incomplete_fourth_forms_report_issues_without_policy_exceptions(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","incomplete.cs",[]));Assert.False(p.Accepted);Assert.NotEmpty(p.Issues);}
 [Theory]
 [InlineData("class C{void M(){F(x:1,y:2);}}","class C{void M(){F(x:1,x:2);}}")]
 [InlineData("class C{void M(int x=1,int y=2){}}","class C{void M(int x=1,int y){}}")]
 [InlineData("interface I<out T>{}","class I<out T>{}")]
 public void Incremental_edits_recompute_fourth_context_rules(string before,string after)
 {
  using var old=CSharpParser.ParseIncremental(new(before,"CSharp4","edit.cs",[]));Assert.True(old.Accepted);
  using var edited=CSharpParser.Reparse(old,new(after,"CSharp4","edit.cs",[]),new[]{TextChange.Between(before,after)});
  using var fresh=CSharpParser.Parse(new(after,"CSharp4","edit.cs",[]));
  Assert.False(edited.Accepted);Assert.Equal(fresh.Accepted,edited.Accepted);Assert.Equal(fresh.Issues,edited.Issues);
  Assert.Equal(fresh.Tree.NodeCount,edited.Tree.NodeCount);
  for(int n=0;n<fresh.Tree.NodeCount;n++){Assert.Equal(fresh.Tree.Kind(n),edited.Tree.Kind(n));Assert.Equal(fresh.Tree.Span(n),edited.Tree.Span(n));}
 }
}
