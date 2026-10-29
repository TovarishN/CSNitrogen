using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using P=CSNitrogen.CSharp5Probes;
namespace CSNitrogen.CSharp5.Tests;
public class CSharp5SpellingContractTests
{
 [Theory]
 [InlineData("async",true)][InlineData("@async",false)][InlineData(@"\u0061sync",false)][InlineData("as\u200bync",false)][InlineData(@"as\u200bync",false)]
 public void Only_raw_async_is_a_function_modifier_in_the_pinned_reference_and_scoped_grammar(string spelling,bool expected)
 {
  string source="class C{"+spelling+" void M(){await(M());}}";
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));
  var method=reference.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
  Assert.Equal(expected,method?.Modifiers.Any(m=>m.RawKind==(int)SyntaxKind.AsyncKeyword)??false);
  Assert.Equal(!expected,reference.GetDiagnostics().Any());
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp5AllProbeComposition.Start);
  Assert.Equal(expected,parsed.Success&&!parsed.HasErrors);
 }
 [Theory]
 [InlineData("await","AwaitExpression")][InlineData("@await","InvocationExpression")][InlineData(@"\u0061wait","InvocationExpression")][InlineData("aw\u200bait","InvocationExpression")][InlineData(@"aw\u200bait","InvocationExpression")]
 public void Raw_await_and_identifier_spellings_keep_the_same_owner_as_the_pinned_reference(string spelling,string expected)
 {
  string source="/*𐐀*/class C{async void M(){"+spelling+"(M());}}";
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp5));Assert.Empty(reference.GetDiagnostics());
  int start=source.IndexOf(spelling,StringComparison.Ordinal);
  var expr=Assert.Single(reference.GetRoot().DescendantNodes().OfType<ExpressionSyntax>(),n=>n.SpanStart==start&&n.Span.Length==spelling.Length+5);
  Assert.Equal(expected,expr.Kind().ToString());
  var language=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int owner=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Span(n).Start==start&&parsed.Tree.Span(n).Length==spelling.Length+5&&language.GetKindName(parsed.Tree.Kind(n)) is "InvocationExpression" or "AwaitExpression");
  Assert.Equal(expected,language.GetKindName(parsed.Tree.Kind(owner)));
 }
}
