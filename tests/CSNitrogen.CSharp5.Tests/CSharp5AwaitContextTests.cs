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
 public void Scoped_context_selects_the_authored_owner(string body,int occurrence,string expectedKind)
 {
  string source="/*𐐀*/"+body;var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors,source);
  int start=Start(source,occurrence);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==10&&language.GetKindName(p.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  string actual=language.GetKindName(p.Tree.Kind(root));
  Assert.Equal(expectedKind,actual);
  if(actual=="AwaitExpression")
  {
   var awaited=Nitrogen.SyntaxView.Cast<P.AwaitExpressionExpression>(p.Tree,root);P.ExpressionNode operand=awaited.Operand;
   Assert.Equal("(M())",source.Substring(operand.Span.Start,operand.Span.Length));Assert.Equal(root,p.Tree.Parent(operand.Index));
  }
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
 public void Escaped_await_stays_an_identifier_in_an_async_context(string spelling)
 {
  string source="/*𐐀*/class C{async void M(){"+spelling+"(M());}}";
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));
  Assert.Empty(reference.GetDiagnostics());
  int start=source.IndexOf(spelling,StringComparison.Ordinal);
  var expression=Assert.Single(reference.GetRoot().DescendantNodes().OfType<ExpressionSyntax>(),n=>n.SpanStart==start&&n.Span.Length==spelling.Length+5);
  Assert.Equal("InvocationExpression",((SyntaxKind)expression.RawKind).ToString());
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==spelling.Length+5&&language.GetKindName(p.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  Assert.Equal("InvocationExpression",language.GetKindName(p.Tree.Kind(root)));
 }
 [Fact]public void Adding_and_removing_an_async_header_reuses_unaffected_nodes_and_matches_a_full_parse()
 {
  const string before="/*𐐀*/class C{void M(){await(M());}void N(){F();}}";
  int offset=before.IndexOf("void M",StringComparison.Ordinal);string after=before.Insert(offset,"async ");var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();
  using var first=language.ParseIncremental(before,P.CSharp5AllProbeComposition.Start);Assert.True(first.Success&&!first.HasErrors);
  using var second=language.Reparse(first,after,[new(offset,0,6)]);using var full=language.Parse(after,P.CSharp5AllProbeComposition.Start);
  Assert.True(second.Success&&!second.HasErrors);Assert.Equal(Nitrogen.SyntaxDumper.Dump(full.Tree),Nitrogen.SyntaxDumper.Dump(second.Tree));Assert.True(second.Stats.ReusedNodes>0);
  using var third=language.Reparse(second,before,[new(offset,6,0)]);using var restored=language.Parse(before,P.CSharp5AllProbeComposition.Start);
  Assert.True(third.Success&&!third.HasErrors);Assert.Equal(Nitrogen.SyntaxDumper.Dump(restored.Tree),Nitrogen.SyntaxDumper.Dump(third.Tree));
 }
}
