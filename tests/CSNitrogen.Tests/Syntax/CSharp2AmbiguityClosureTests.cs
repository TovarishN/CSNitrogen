using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2AmbiguityClosureTests
{
 [Theory]
 [InlineData("F<T>(x)")][InlineData("(F<T>)")][InlineData("a[F<T>]")]
 [InlineData("x?F<T>:y")][InlineData("F<T>")][InlineData("f(F<T>,x)")]
 [InlineData("F<T>.M")][InlineData("F<T>?x:y")][InlineData("F<T>==x")][InlineData("F<T>!=x")]
 public void Every_historical_generic_follower_retains_type_arguments(string expression)
 {
  using var p=CSharpParser.Parse(new("class C{object M(){return "+expression+";}}","CSharp2","followers.cs",[]));
  Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="GenericNameExpression");
 }
 [Theory][InlineData("(A::T.M)-x","SubtractExpression")][InlineData("(A::T.M)x","AliasCast")]
 [InlineData("(A::G<int>.M)-x","SubtractExpression")][InlineData("(A::T.M<int>)-x","SubtractExpression")]
 public void Dotted_alias_names_preserve_expression_ambiguity(string expression,string kind)
 {using var p=CSharpParser.Parse(new("class C{object M(){return "+expression+";}}","CSharp2","alias.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==kind);}
 [Theory][InlineData("b ? new int?(1) : null")][InlineData("b ? new G<T>?(x) : null")]
 [InlineData("x is int? ? a : b")][InlineData("x as G<int?> ? a : b")]
 [InlineData("x is int ? (a) : b")][InlineData("x as T ? (a) : b")]
 public void Nullable_construction_in_conditional_arms_remains_a_type(string expression)
 {using var p=CSharpParser.Parse(new("class C{object M(){return "+expression+";}}","CSharp2","nullable.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Theory][InlineData("x is int ? a : b")][InlineData("x as T ? a : b")]
 public void Type_operator_question_mark_preserves_conditional_shape(string expression)
 {
  using var p=CSharpParser.Parse(new("class C{object M(){return "+expression+";}}","CSharp2","nullable.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="ConditionalExpression");
 }
}
