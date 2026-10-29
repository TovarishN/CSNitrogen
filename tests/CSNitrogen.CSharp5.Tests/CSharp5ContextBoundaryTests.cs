using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using P=CSNitrogen.CSharp5Probes;
namespace CSNitrogen.CSharp5.Tests;
public class CSharp5ContextBoundaryTests
{
 [Theory]
 [InlineData("class C{[A(await(M()))] async void N(){await(M());}}",0,"InvocationExpression")]
 [InlineData("class C{[A(await(M()))] async void N(){await(M());}}",1,"AwaitExpression")]
 [InlineData("class C{async void N(object x=await(M())){await(M());}}",0,"InvocationExpression")]
 [InlineData("class C{async void N(object x=await(M())){await(M());}}",1,"AwaitExpression")]
 public void Async_method_scope_changes_the_body_without_changing_attributes_or_default_values(string body,int occurrence,string expected)
 {
  string source="/*𐐀*/"+body;int start=CSharp5AwaitContextTests.Start(source,occurrence);
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));Assert.Empty(reference.GetDiagnostics());
  var expression=Assert.Single(reference.GetRoot().DescendantNodes().OfType<ExpressionSyntax>(),n=>n.SpanStart==start&&n.Span.Length==10);Assert.Equal(expected,expression.Kind().ToString());
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int owner=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Span(n).Start==start&&parsed.Tree.Span(n).Length==10&&language.GetKindName(parsed.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  Assert.Equal(expected,language.GetKindName(parsed.Tree.Kind(owner)));
 }
}
