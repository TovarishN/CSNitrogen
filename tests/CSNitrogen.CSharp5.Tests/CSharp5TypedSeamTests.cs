using Nitrogen;
using P=CSNitrogen.CSharp5Probes;
namespace CSNitrogen.CSharp5.Tests;
public class CSharp5TypedSeamTests
{
 [Fact]public void Async_lambda_and_anonymous_method_keep_original_fields_and_authored_marker_spans()
 {
  const string source="/*𐐀*/class C{object f=async (int x)=>await M(x);object g=async delegate(int y){await M(y);};}";
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int Find(string kind)=>Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);
  var lambda=SyntaxView.Cast<P.LambdaExpressionExpression>(p.Tree,Find("LambdaExpression"));
  SyntaxNode parameters=lambda.Parameters;SyntaxNode body=lambda.Body;
  Assert.Equal("(int x)",Text(parameters.Span));Assert.Equal("await M(x)",Text(body.Span));Assert.True(lambda.Async.HasValue);Assert.Equal("async",Text(lambda.Async.Value.Span));Assert.Equal(source.IndexOf("async",StringComparison.Ordinal),lambda.Async.Value.Span.Start);
  var anon=SyntaxView.Cast<P.AnonymousMethodExpressionExpression>(p.Tree,Find("AnonymousMethodExpression"));Optional<P.AnonymousParametersNode> signature=anon.Parameters;P.BlockNode block=anon.Body;
  Assert.True(signature.HasValue);Assert.Equal("(int y)",Text(signature.Value.Span));Assert.Equal("{await M(y);}",Text(block.Span));Assert.True(anon.Async.HasValue);Assert.Equal("async",Text(anon.Async.Value.Span));
  string Text(TextSpan span)=>source.Substring(span.Start,span.Length);
 }
 [Fact]public void Empty_marker_seams_keep_ordinary_parameters_and_bodies_with_absent_async_views()
 {
  const string source="class C{object f=x=>x;object g=delegate(int y){return y;};}";
  var language=P.CSharp5NoneProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5NoneProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int Find(string kind)=>Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);
  var lambda=SyntaxView.Cast<P.LambdaExpressionExpression>(p.Tree,Find("LambdaExpression"));Assert.False(lambda.Async.HasValue);Assert.Equal("x",Text(lambda.Parameters.Span));Assert.Equal("x",Text(lambda.Body.Span));
  var anon=SyntaxView.Cast<P.AnonymousMethodExpressionExpression>(p.Tree,Find("AnonymousMethodExpression"));Assert.False(anon.Async.HasValue);Assert.True(anon.Parameters.HasValue);Assert.Equal("(int y)",Text(anon.Parameters.Value.Span));Assert.Equal("{return y;}",Text(anon.Body.Span));
  string Text(TextSpan span)=>source.Substring(span.Start,span.Length);
 }
 [Fact]public void Await_has_typed_operand_and_its_original_unary_range_before_binary_addition()
 {
  const string source="/*𐐀*/class C{async Task M(){return await M(x)+y;}}";
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  var awaited=SyntaxView.Cast<P.AwaitExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.AwaitExpressionExpression.Is(p.Tree,n)));
  P.ExpressionNode operand=awaited.Operand;Assert.Equal("await",awaited.Keyword.ToString());Assert.Equal("M(x)",source.Substring(operand.Span.Start,operand.Span.Length));Assert.Equal("await M(x)",source.Substring(awaited.Span.Start,awaited.Span.Length));Assert.Equal(source.IndexOf("await",StringComparison.Ordinal),awaited.Span.Start);
  Assert.Equal("AddExpression",language.GetKindName(p.Tree.Kind(p.Tree.Parent(awaited.Index))));
 }
 [Theory][InlineData("Probe.CSharp5AsyncFunctions")][InlineData("Probe.CSharp5AwaitExpressions")]
 public void Prototype_modules_are_independently_removable_without_losing_the_fourth_baseline(string omitted)
 {
  var full=P.CSharp5AllProbeComposition.CreateBuilder().Build();Assert.Equal(32,full.Modules.Count);var builder=new LanguageBuilder();foreach(var m in full.Modules)if(m.Name!=omitted)builder.Add(m);var language=builder.Build();
  using var ordinary=language.Parse("interface I<out T>{void M(int x=1);}class C{void N(){F(x:1);}}",P.CSharp5AllProbeComposition.Start);Assert.True(ordinary.Success&&!ordinary.HasErrors);
  string source=omitted=="Probe.CSharp5AwaitExpressions"?"class C{async void M(){await M();}}":"class C{object f=async x=>x;}";using var absent=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  string survivor=omitted=="Probe.CSharp5AwaitExpressions"?"class C{object f=async x=>x;}":"class C{void M(){await M();}}";using var retained=language.Parse(survivor,P.CSharp5AllProbeComposition.Start);Assert.True(retained.Success&&!retained.HasErrors);
 }
 [Theory]
 [InlineData("class C{async M(){return null;}}")][InlineData("class C{async async(){return null;}}")]
 [InlineData("class C{object f=async=>async;}")][InlineData("class C{object f=await=>await;}")]
 [InlineData("class C{int asynctail;int awaittail;}")][InlineData(@"class C{async\u0054 M(){return null;}}")]
 [InlineData("class C{async𐐀 M(){return null;}}")]
 public void Function_marker_lookahead_does_not_steal_ordinary_identifier_categories(string source)
 {var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors,source);Assert.DoesNotContain(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n)) is "AsyncModifier" or "AsyncFunction");}
}
