using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2TypeOperandViewTests
{
 [Theory][InlineData("CSharp1")][InlineData("CSharp2")]
 public void Approved_wrapper_preserves_the_ordinary_type_child_and_source_span(string profile)
 {
  const string source="class C{bool M(){return x is int;}}";
  using var p=CSharpParser.Parse(new(source,profile,"operand.cs",[]));Assert.True(p.Accepted);
  int node=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>IsExpressionExpression.Is(p.Tree,n));
  var expression=SyntaxView.Cast<IsExpressionExpression>(p.Tree,node);
  Assert.Equal("TypeOperatorOperand",p.Tree.Language!.GetKindName(p.Tree.Kind(expression.Type.Index)));
  Assert.Equal("TypeSyntax",p.Tree.Language.GetKindName(p.Tree.Kind(expression.Type.Value.Index)));
  Assert.Equal("int",expression.Type.Value.ToString());Assert.Equal(source.IndexOf("int",StringComparison.Ordinal),expression.Type.Value.Span.Start);
 }
 [Fact]public void Conditional_specific_operand_owns_only_the_type_spelling()
 {
  const string source="class C{bool M(){return x is int ? a : b;}}";
  using var p=CSharpParser.Parse(new(source,"CSharp2","operand.cs",[]));Assert.True(p.Accepted);
  int node=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>IsExpressionExpression.Is(p.Tree,n));
  var expression=SyntaxView.Cast<IsExpressionExpression>(p.Tree,node);
  var operand=SyntaxView.Cast<NonNullableConditionalOperandConditionalTypeOperand>(p.Tree,expression.Type.Value.Index);
  Assert.Equal("int",source.Substring(operand.Type.Span.Start,operand.Type.Span.Length));Assert.Equal(new TextSpan(source.IndexOf("int",StringComparison.Ordinal),3),operand.Type.Span);
 }
}
