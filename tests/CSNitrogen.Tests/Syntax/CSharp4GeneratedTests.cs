using Nitrogen;
using P=CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4GeneratedTests
{
 static string Authored(string source,TextSpan span)=>source.Substring(span.Start,span.Length);
 [Fact]public void Optional_seams_preserve_original_fields_and_absent_baseline_views()
 {
  const string source="class C<T>{void M([A]ref int x){F(ref x);}}";var language=P.CSharp3Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp3Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  int Find(string kind)=>Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);
  var parameter=SyntaxView.Cast<P.FixedParameterNode>(p.Tree,Find("FixedParameter"));Assert.Equal(1,parameter.Attributes.Count);Assert.False(parameter.Receiver.HasValue);Assert.Equal("ref",parameter.Mode.Value.ToString());Assert.Equal("int",Authored(source,parameter.Type.Span));Assert.Equal("x",parameter.Name.ToString());Assert.False(parameter.Default.HasValue);
  var argument=SyntaxView.Cast<P.ArgumentNode>(p.Tree,Find("Argument"));Assert.False(argument.Name.HasValue);Assert.Equal("ref",argument.Mode.Value.ToString());Assert.Equal("x",Authored(source,argument.Value.Span));
  var type=SyntaxView.Cast<P.TypeParameterNode>(p.Tree,Find("TypeParameter"));Assert.Equal("T",type.Name.ToString());Assert.Equal(0,type.Attributes.Count);Assert.False(type.Variance.HasValue);
 }
 [Theory]
 [InlineData("class C{void M(){F(x:1);}}","CSharp.NamedArguments")]
 [InlineData("class C{void M(int x=1){}}","CSharp.OptionalParameters")]
 [InlineData("interface I<out T>{}","CSharp.Variance")]
 public void Features_can_be_omitted_independently_without_breaking_ordinary_syntax(string source,string omitted)
 {
  var full=P.CSharp4Composition.CreateBuilder().Build();var builder=new LanguageBuilder();foreach(var module in full.Modules)if(module.Name!=omitted)builder.Add(module);var language=builder.Build();
  using var absent=language.Parse(source,P.CSharp4Composition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var baseline=language.Parse("class C<T>{void M(int x){F(x);}}",P.CSharp4Composition.Start);Assert.True(baseline.Success&&!baseline.HasErrors);
  foreach(var (module,other) in new[]{("CSharp.NamedArguments","class C{void M(){F(x:1);}}"),("CSharp.OptionalParameters","class C{void M(int x=1){}}"),("CSharp.Variance","interface I<out T>{}")})
  {if(module==omitted)continue;using var independent=language.Parse(other,P.CSharp4Composition.Start);Assert.True(independent.Success&&!independent.HasErrors,module+" after omitting "+omitted);}
 }
 [Fact]public void Named_default_and_variance_captures_retain_typed_children_and_exact_UTF16_ranges()
 {
  const string source="/*𐐀*/interface I<[A]out T>{void M(int x=1);}";var language=P.CSharp4Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  var t=SyntaxView.Cast<P.TypeParameterNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.TypeParameterNode.Is(p.Tree,n)));Assert.Equal(1,t.Attributes.Count);Assert.Equal("T",t.Name.ToString());Assert.Equal("out",Authored(source,t.Variance.Value.Span));Assert.Equal(source.IndexOf("out",StringComparison.Ordinal),t.Variance.Value.Span.Start);
  var param=SyntaxView.Cast<P.FixedParameterNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.FixedParameterNode.Is(p.Tree,n)));Assert.Equal("int",Authored(source,param.Type.Span));Assert.Equal("x",param.Name.ToString());Assert.Equal("=1",Authored(source,param.Default.Value.Span));Assert.Equal(source.IndexOf("=1",StringComparison.Ordinal),param.Default.Value.Span.Start);
  const string invocation="/*𐐀*/class C{void M(){F(name:ref x);}}";using var q=language.Parse(invocation,P.CSharp4Composition.Start);Assert.True(q.Success&&!q.HasErrors);
  var arg=SyntaxView.Cast<P.ArgumentNode>(q.Tree,Assert.Single(Enumerable.Range(0,q.Tree.NodeCount),n=>P.ArgumentNode.Is(q.Tree,n)));Assert.Equal("name:",Authored(invocation,arg.Name.Value.Span));Assert.Equal("ref",arg.Mode.Value.ToString());Assert.Equal("x",Authored(invocation,arg.Value.Span));Assert.Equal(invocation.IndexOf("name:",StringComparison.Ordinal),arg.Name.Value.Span.Start);
 }
 [Fact]public void Positional_index_owners_and_alias_or_conditional_colons_remain_unchanged()
 {
  const string source="class C{object M(){F(A::T.M,p?x:y);return a[x,y]+base[x,y];}}";var language=P.CSharp4Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  var element=SyntaxView.Cast<P.ElementExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.ElementExpressionExpression.Is(p.Tree,n)));Assert.Equal(2,element.Indices.Count);Assert.Equal("x",Authored(source,element.Indices[0].Span));
  var baseElement=SyntaxView.Cast<P.BaseIndexExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.BaseIndexExpressionExpression.Is(p.Tree,n)));Assert.Equal(2,baseElement.Indices.Count);
  Assert.DoesNotContain(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n)) is "NamedElementExpression" or "NamedBaseIndexExpression" or "NamedArgumentName");
 }
 [Fact]public void Named_index_arguments_retain_separate_positional_and_named_typed_owners()
 {
  const string source="/*𐐀*/class C{object M(){return a[0,index:1,last:2];}}";var language=P.CSharp4Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  var index=SyntaxView.Cast<P.NamedElementExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.NamedElementExpressionExpression.Is(p.Tree,n)));
  Assert.Equal("a",Authored(source,index.Target.Span));Assert.Equal(1,index.Arguments.Leading.Count);Assert.Equal(2,index.Arguments.Named.Count);Assert.Equal("0",Authored(source,index.Arguments.Leading[0].Value.Span));
  var named=index.Arguments.Named[0];Assert.Equal("index",named.Name.ToString());Assert.Equal("1",Authored(source,named.Value.Span));Assert.Equal(source.IndexOf("index:",StringComparison.Ordinal),named.Name.Span.Start);
 }
 [Fact]public void Deep_named_invocation_children_keep_their_original_ranges()
 {
  const int depth=40;string expression="1";for(int i=0;i<depth;i++)expression="F(x:"+expression+")";string source="/*𐐀*/class C{object M(){return "+expression+";}}";
  var language=P.CSharp4Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  Assert.Equal(depth,Enumerable.Range(0,p.Tree.NodeCount).Count(n=>language.GetKindName(p.Tree.Kind(n))=="NamedArgumentName"));foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var span=p.Tree.Span(n);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
 }

 [Theory]
 [InlineData("a[A::T.M]","ElementExpression","A::T.M")]
 [InlineData("a[p?x:y]","ElementExpression","p?x:y")]
 [InlineData("base[A::T.M]","BaseIndexExpression","A::T.M")]
 [InlineData("base[p?x:y]","BaseIndexExpression","p?x:y")]
 [InlineData("a[A::T.M,index:1]","NamedElementExpression","A::T.M")]
 [InlineData("a[p?x:y,index:1]","NamedElementExpression","p?x:y")]
 [InlineData("base[A::T.M,index:1]","NamedBaseIndexExpression","A::T.M")]
 [InlineData("base[p?x:y,index:1]","NamedBaseIndexExpression","p?x:y")]
 public void Alias_and_conditional_index_values_preserve_their_owners_and_exact_expression_spans(string expression,string kind,string first)
 {
  string source="/*𐐀*/class C{object M(){return "+expression+";}}";var language=P.CSharp4Composition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
  int node=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);Nitrogen.TextSpan span;
  if(kind=="ElementExpression")span=SyntaxView.Cast<P.ElementExpressionExpression>(p.Tree,node).Indices[0].Span;
  else if(kind=="BaseIndexExpression")span=SyntaxView.Cast<P.BaseIndexExpressionExpression>(p.Tree,node).Indices[0].Span;
  else
  {
   var args=kind=="NamedElementExpression"?SyntaxView.Cast<P.NamedElementExpressionExpression>(p.Tree,node).Arguments:SyntaxView.Cast<P.NamedBaseIndexExpressionExpression>(p.Tree,node).Arguments;
   Assert.Equal(1,args.Leading.Count);Assert.Equal(1,args.Named.Count);span=args.Leading[0].Value.Span;Assert.Equal("index",args.Named[0].Name.ToString());
  }
  Assert.Equal(first,Authored(source,span));Assert.Equal(source.IndexOf(first,StringComparison.Ordinal),span.Start);
 }

}
