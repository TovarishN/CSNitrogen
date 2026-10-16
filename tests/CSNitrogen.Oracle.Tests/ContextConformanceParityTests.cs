using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
namespace CSNitrogen.Oracle.Tests;
public class ContextConformanceParityTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/context-conformance-cases.json")));
  foreach(var row in data.RootElement.EnumerateArray())yield return [row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Source_contexts_match_declared_compiler_observations(string source,bool expected)
 {
  var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
  bool accepted=!oracle.SyntaxDiagnostics.Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error)&&
    !oracle.CompilationDiagnostics.Any(d=>d.Id is "CS8022" or "CS1017" or "CS1007" or "CS0548" or "CS0065" or "CS0575" or "CS0132" or "CS0514" or "CS1023");
  Assert.True(expected==accepted,source+" :: "+string.Join(";",oracle.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage())));
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","context.cs",[]));Assert.Equal(expected,parsed.Accepted);
 }
 [Theory][InlineData("a?b:c=d")][InlineData("a?b=c:d=e")][InlineData("a?b:c?d:e=f")]
 public void Conditional_assignment_arms_match_compiler_shape(string source)
 {
  var oracle=SyntaxFactory.ParseExpression(source,options:new CSharpParseOptions(LanguageVersion.CSharp1),consumeFullText:true);
  Assert.False(oracle.ContainsDiagnostics);var conditionalOracle=Assert.IsType<ConditionalExpressionSyntax>(oracle);
  var language=CSharp1Composition.CreateBuilder().Build();using var parsed=language.Parse(source,ExpressionsModule.ExpressionFragment);
  Assert.True(parsed.Success&&!parsed.HasErrors);
  int root=parsed.Tree.Child(parsed.Tree.Root,0);
  Assert.Equal("ConditionalExpression",language.GetKindName(parsed.Tree.Kind(root)));
  var conditional=SyntaxView.Cast<ConditionalExpressionExpression>(parsed.Tree,root);
  string expectedFalse=conditionalOracle.WhenFalse is ConditionalExpressionSyntax?"ConditionalExpression":"AssignmentExpression";
  Assert.Equal(expectedFalse,language.GetKindName(conditional.False.Value.Kind));
  Assert.Equal(conditionalOracle.WhenFalse.Span.Start,conditional.False.Value.Span.Start);
  Assert.Equal(conditionalOracle.WhenFalse.Span.Length,conditional.False.Value.Span.Length);
  void Compare(ExpressionSyntax reference,ExpressionNode actual)
  {
   Assert.Equal(reference.Span.Start,actual.Span.Start);Assert.Equal(reference.Span.Length,actual.Span.Length);
   if(reference is AssignmentExpressionSyntax assignment)
   {
    Assert.Equal("AssignmentExpression",language.GetKindName(actual.Kind));
    var node=SyntaxView.Cast<AssignmentExpressionExpression>(parsed.Tree,actual.Index);
    Compare(assignment.Left,node.Target);Compare(assignment.Right,node.Value);
   }
   else if(reference is ConditionalExpressionSyntax nested)
   {
    Assert.Equal("ConditionalExpression",language.GetKindName(actual.Kind));
    var node=SyntaxView.Cast<ConditionalExpressionExpression>(parsed.Tree,actual.Index);
    Compare(nested.Condition,node.Condition);Compare(nested.WhenTrue,node.True);Compare(nested.WhenFalse,node.False.Value);
   }
   else Assert.Equal("NameExpression",language.GetKindName(actual.Kind));
  }
  Compare(conditionalOracle.Condition,conditional.Condition);Compare(conditionalOracle.WhenTrue,conditional.True);Compare(conditionalOracle.WhenFalse,conditional.False.Value);
 }
}
