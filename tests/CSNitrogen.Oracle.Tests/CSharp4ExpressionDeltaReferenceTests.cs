using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4ExpressionDeltaReferenceTests
{
 [Theory]
 [InlineData("F<T>|x")][InlineData("a.F<T>^x")][InlineData("base.F<T>|x")]
 [InlineData("p->F<T>^x")][InlineData("A::T.F<U>|x")][InlineData("int.F<T>^x")]
 public void Pinned_reference_retains_same_generic_name_and_binary_expression_spans(string expression)
 {
  string source="/*𐐀*/unsafe class C{object M(){return "+expression+";}}";
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp4));Assert.Empty(reference.GetDiagnostics());
  var binary=Assert.Single(reference.GetRoot().DescendantNodes().OfType<BinaryExpressionSyntax>());var generic=Assert.Single(reference.GetRoot().DescendantNodes().OfType<GenericNameSyntax>());
  using var p=CSharpParser.Parse(new(source,"CSharp4","reference.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  string rootKind=binary.RawKind==(int)SyntaxKind.BitwiseOrExpression?"BitOrExpression":"BitXorExpression";
  Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==rootKind&&p.Tree.Span(n).Start==binary.SpanStart&&p.Tree.Span(n).Length==binary.Span.Length);
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==generic.TypeArgumentList.SpanStart&&p.Tree.Span(n).Length==generic.TypeArgumentList.Span.Length);
 }
 [Theory][InlineData("new {base.X}")][InlineData("new {base.F<T>}")]
 public void Pinned_reference_projection_expression_has_same_authored_range(string expression)
 {
  string source="/*𐐀*/class C{object M(){return "+expression+";}}";var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp4));Assert.Empty(reference.GetDiagnostics());
  var member=Assert.Single(reference.GetRoot().DescendantNodes().OfType<AnonymousObjectMemberDeclaratorSyntax>());
  using var p=CSharpParser.Parse(new(source,"CSharp4","projection-reference.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="InferredMember"&&p.Tree.Span(n).Start==member.SpanStart&&p.Tree.Span(n).Length==member.Span.Length);
 }
}
