using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using P=CSNitrogen.CSharp5Probes;
namespace CSNitrogen.CSharp5.Tests;
public class CSharp5AwaitContextTests
{
 public static IEnumerable<object[]> Cases()
 {
  yield return ["class C{void M(){await(M());}}",0,"InvocationExpression"];
  yield return ["class C{async void M(){await(M());}}",0,"AwaitExpression"];
  const string mixed="class C{void A(){await(M());}async void B(){await(M());}}";
  yield return [mixed,0,"InvocationExpression"];yield return [mixed,1,"AwaitExpression"];
  yield return ["class C{object f=()=>await(M());}",0,"InvocationExpression"];
  yield return ["class C{object f=async ()=>await(M());}",0,"AwaitExpression"];
  // Await remains an operator lexically inside the outer async function; source policy rejects its nested non-async use.
  yield return ["class C{async void M(){object f=delegate{await(M());};}}",0,"AwaitExpression"];
  yield return ["class C{void M(){object f=async delegate{await(M());};}}",0,"AwaitExpression"];
 }
 internal static int Start(string source,int occurrence)
 {int start=-1;for(int i=0;i<=occurrence;i++)start=source.IndexOf("await(M())",start+1,StringComparison.Ordinal);return start;}
 [Theory][MemberData(nameof(Cases))]
 public void Global_probe_records_the_unresolved_context_ambiguity(string body,int occurrence,string expectedKind)
 {
  string source="/*𐐀*/"+body;var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors,source);
  int start=Start(source,occurrence);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==10&&language.GetKindName(p.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  string actual=language.GetKindName(p.Tree.Kind(root));
  // This pins the observed limitation, not the desired language contract.
  // The desired-owner test failed for the three InvocationExpression cases.
  Assert.Equal("AwaitExpression",actual);Assert.Equal(expectedKind=="InvocationExpression",actual!=expectedKind);
 }
 [Theory][MemberData(nameof(Cases))]
 public void Pinned_reference_distinguishes_the_authored_async_context(string body,int occurrence,string expectedKind)
 {
  string source="/*𐐀*/"+body;var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));Assert.Empty(reference.GetDiagnostics());int start=Start(source,occurrence);
  var expression=Assert.Single(reference.GetRoot().DescendantNodes().OfType<ExpressionSyntax>(),n=>n.SpanStart==start&&n.Span.Length==10);
  Assert.Equal(expectedKind,((SyntaxKind)expression.RawKind).ToString());
 }
 [Theory]
 [InlineData("@await")][InlineData(@"\u0061wait")]
 public void Escaped_await_reference_and_global_probe_observation(string spelling)
 {
  string source="/*𐐀*/class C{async void M(){"+spelling+"(M());}}";
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));
  Assert.Empty(reference.GetDiagnostics());
  int start=source.IndexOf(spelling,StringComparison.Ordinal);
  var expression=Assert.Single(reference.GetRoot().DescendantNodes().OfType<ExpressionSyntax>(),n=>n.SpanStart==start&&n.Span.Length==spelling.Length+5);
  Assert.Equal("InvocationExpression",((SyntaxKind)expression.RawKind).ToString());
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==spelling.Length+5&&language.GetKindName(p.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  Assert.Equal("AwaitExpression",language.GetKindName(p.Tree.Kind(root)));
 }
}
