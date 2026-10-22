using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4ExpressionDeltaTests
{
 [Theory]
 [InlineData("F<T>|x","BitOrExpression","GenericNameExpression","F<T>")]
 [InlineData("F<T>^x","BitXorExpression","GenericNameExpression","F<T>")]
 [InlineData("a.F<T>|x","BitOrExpression","GenericMemberExpression","a.F<T>")]
 [InlineData("a.F<T>^x","BitXorExpression","GenericMemberExpression","a.F<T>")]
 [InlineData("base.F<T>|x","BitOrExpression","GenericBaseMemberExpression","base.F<T>")]
 [InlineData("base.F<T>^x","BitXorExpression","GenericBaseMemberExpression","base.F<T>")]
 [InlineData("p->F<T>|x","BitOrExpression","GenericPointerMemberExpression","p->F<T>")]
 [InlineData("p->F<T>^x","BitXorExpression","GenericPointerMemberExpression","p->F<T>")]
 [InlineData("A::T.F<U>|x","BitOrExpression","GenericMemberExpression","A::T.F<U>")]
 [InlineData("int.F<T>^x","BitXorExpression","GenericPredefinedMemberExpression","int.F<T>")]
 public void Fourth_generic_followers_retain_type_arguments_and_original_expression_spans(string expression,string rootKind,string genericKind,string genericText)
 {
  string source="/*𐐀*/unsafe class C{object M(){return "+expression+";}}";
  using var p=CSharpParser.Parse(new(source,"CSharp4","follower.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);
  int start=source.IndexOf(expression,StringComparison.Ordinal);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>Kind(n)==rootKind&&p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==expression.Length);
  int generic=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>Kind(n)==genericKind&&source.Substring(p.Tree.Span(n).Start,p.Tree.Span(n).Length)==genericText);
  for(int n=generic;;n=p.Tree.Parent(n)){Assert.True(n>=0);if(n==root)break;}
  using var third=CSharpParser.Parse(new(source,"CSharp3","previous.cs",[]));Assert.False(third.Accepted);Assert.True(third.ProfileComplete);
  string Kind(int n)=>p.Tree.Language!.GetKindName(p.Tree.Kind(n));
 }
 [Theory]
 [InlineData("new {base.X}")][InlineData("new {base.X,Other=1}")][InlineData("new {base.F<T>}")]
 public void Fourth_projection_initializers_include_base_member_access(string expression)
 {
  string source="/*𐐀*/class C{object M(){return "+expression+";}}";
  using var p=CSharpParser.Parse(new(source,"CSharp4","projection.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  using var third=CSharpParser.Parse(new(source,"CSharp3","previous.cs",[]));Assert.False(third.Accepted);
 }
 [Theory]
 [InlineData("F<T>||x")][InlineData("F<T>|=x")][InlineData("F<T>^=x")]
 [InlineData("new {base[0]}")][InlineData("new {base.M()}")][InlineData("new {base}")]
 public void Distinct_operator_tokens_and_non_projection_neighbors_stay_rejected(string expression)
 {
  using var p=CSharpParser.Parse(new("class C{object M(){return "+expression+";}}","CSharp4","neighbor.cs",[]));Assert.False(p.Accepted);
 }
 [Fact]public void Fourth_follower_capability_is_explicit_and_independently_removable()
 {
  var full=CSNitrogen.Syntax.CSharp4Composition.CreateBuilder().Build();var builder=new Nitrogen.LanguageBuilder();
  Assert.Contains(full.Modules,m=>m.Name=="CSharp.GenericFollowersFourth");foreach(var m in full.Modules)if(m.Name!="CSharp.GenericFollowersFourth")builder.Add(m);var absent=builder.Build();
  foreach(string expression in new[]{"F<T>|x","p->F<T>^x"})
  {using var p=absent.Parse("unsafe class C{object M(){return "+expression+";}}",CSNitrogen.Syntax.CSharp4Composition.Start);Assert.False(p.Success&&!p.HasErrors);}
  foreach(string source in new[]{"class C{object x=F<T>();}","class C{void M(int x=1){F(x:1);}}","interface I<out T>{}","class C{object x=new {base.X};}"})
  {using var p=absent.Parse(source,CSNitrogen.Syntax.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);}
  Assert.Contains("csharp4.generic-disambiguation",CSNitrogen.Profiles.ProfileResolver.Resolve("CSharp4").Profile!.Features);
  var custom=CSNitrogen.Profiles.ProfileResolver.ResolveCustom("missing-generics",["csharp4.generic-disambiguation"]);Assert.Null(custom.Profile);Assert.Contains(custom.Issues,i=>i.Code=="missing-requirement");
 }
 [Theory][InlineData("CSharp5")][InlineData("CSharp7_3")][InlineData("CSharp14")]
 public void Selected_later_factories_retain_fourth_expression_additions_without_admission(string profile)
 {
  using var p=CSharpParser.Parse(new("class C{object M(){return new {X=F<T>|x,base.P};}}",profile,"later.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);
 }

}
