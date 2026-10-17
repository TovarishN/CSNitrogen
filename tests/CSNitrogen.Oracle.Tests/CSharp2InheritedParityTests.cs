using System.Reflection;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class CSharp2InheritedParityTests
{
 public static IEnumerable<object[]> Expressions()
 {
  var method=typeof(SourceGrammarParityTests).GetMethod(nameof(SourceGrammarParityTests.Expression_roots_match))!;
  foreach(var attribute in method.GetCustomAttributes<InlineDataAttribute>())
   foreach(var row in attribute.GetData(method))yield return row;
 }
 [Theory][MemberData(nameof(Expressions))]
 public void Inherited_expression_shapes_and_spans_match_in_CSharp2(string source,string kind,SyntaxKind expected)
 {
  var reference=SyntaxFactory.ParseExpression(source,options:new CSharpParseOptions(LanguageVersion.CSharp2),consumeFullText:true);
  Assert.False(reference.ContainsDiagnostics);Assert.Equal(expected,reference.Kind());
  var language=CSharp2Composition.CreateBuilder().Build();using var parsed=language.Parse(source,ExpressionsModule.ExpressionFragment);
  Assert.True(parsed.Success&&!parsed.HasErrors);
  int root=parsed.Tree.Child(parsed.Tree.Root,0);
  Assert.Equal(kind,language.GetKindName(parsed.Tree.Kind(root)));
  Assert.Equal(reference.Span.Start,parsed.Tree.Span(root).Start);Assert.Equal(reference.Span.Length,parsed.Tree.Span(root).Length);
 }
 [Theory][MemberData(nameof(SourceGrammarParityTests.ExternalCorpus),MemberType=typeof(SourceGrammarParityTests))]
 public void Pinned_first_edition_external_sources_remain_accepted_in_CSharp2(string file)
 {
  string source=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/Mono2004",file));
  var reference=Oracle.Observe(source,LanguageVersion.CSharp2);Assert.DoesNotContain(reference.SyntaxDiagnostics,d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
  using var parsed=CSharpParser.Parse(new(source,"CSharp2",file,[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));
 }
}
