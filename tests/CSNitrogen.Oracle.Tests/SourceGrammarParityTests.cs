using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nitrogen;
namespace CSNitrogen.Oracle.Tests;
public class SourceGrammarParityTests
{
 [Theory]
 [InlineData("a+b*c","AddExpression",SyntaxKind.AddExpression)]
 [InlineData("a-b-c","SubtractExpression",SyntaxKind.SubtractExpression)]
 [InlineData("a=b=c","AssignmentExpression",SyntaxKind.SimpleAssignmentExpression)]
 [InlineData("a?b:c?d:e","ConditionalExpression",SyntaxKind.ConditionalExpression)]
 [InlineData("(T)x","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(x)+y","AddExpression",SyntaxKind.AddExpression)]
 [InlineData("(int)-x","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)(x)","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)sizeof(int)","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)typeof(int)","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)checked(x)","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)unchecked(x)","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)new C()","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)this","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("(T)base.P","CastExpression",SyntaxKind.CastExpression)]
 [InlineData("int.MaxValue","PredefinedMemberExpression",SyntaxKind.SimpleMemberAccessExpression)]
 [InlineData("a+++b","AddExpression",SyntaxKind.AddExpression)]
 [InlineData("a---b","SubtractExpression",SyntaxKind.SubtractExpression)]
 [InlineData("a&&b","AndExpression",SyntaxKind.LogicalAndExpression)]
 [InlineData("a<<b+1","ShiftLeftExpression",SyntaxKind.LeftShiftExpression)]
 [InlineData("F(ref x,out y)","InvocationExpression",SyntaxKind.InvocationExpression)]
 public void Expression_roots_match(string source,string kind,SyntaxKind expected)
 {
  var roslyn=SyntaxFactory.ParseExpression(source,options:new CSharpParseOptions(LanguageVersion.CSharp1),consumeFullText:true);
  Assert.False(roslyn.ContainsDiagnostics);Assert.Equal(expected,roslyn.Kind());
  var l=CSharp1Composition.CreateBuilder().Build();using var parsed=l.Parse(source,ExpressionsModule.ExpressionFragment);
  Assert.True(parsed.Success&&!parsed.HasErrors,source+" :: "+string.Join(";",parsed.Diagnostics.ToArray().Select(d=>parsed.FormatMessage(d))));
  var tree=parsed.Tree;int root=tree.Child(tree.Root,0);Assert.Equal(kind,l.GetKindName(tree.Kind(root)));Assert.Equal(roslyn.Span.Length,tree.Span(root).Length);
  if(source=="a+++b"){var add=SyntaxView.Cast<AddExpressionExpression>(tree,root);Assert.Equal("PostfixIncrementExpression",l.GetKindName(add.Left.Kind));}
  if(source=="a---b"){var sub=SyntaxView.Cast<SubtractExpressionExpression>(tree,root);Assert.Equal("PostfixDecrementExpression",l.GetKindName(sub.Left.Kind));}
  if(source=="a+b*c") {var add=SyntaxView.Cast<AddExpressionExpression>(tree,root);Assert.Equal("MultiplyExpression",l.GetKindName(add.Right.Kind));}
  if(source=="a-b-c") {var sub=SyntaxView.Cast<SubtractExpressionExpression>(tree,root);Assert.Equal("SubtractExpression",l.GetKindName(sub.Left.Kind));}
  if(source=="a=b=c") {var assignment=SyntaxView.Cast<AssignmentExpressionExpression>(tree,root);Assert.Equal("AssignmentExpression",l.GetKindName(assignment.Value.Kind));}
 }
 [Theory]
 [InlineData("namespace N {using A; [A] public class C:I{public int P{get{return 1;}} public void M(){for(int i=0;i<10;i++)F();}}}")]
 [InlineData("interface I {void M();int P{get;set;}event E X;int this[int i]{get;}}")]
 [InlineData("enum E:byte{A=1,B,} delegate void D(ref int x);")]
 [InlineData("class C{void M(){try{F();}catch(E e){throw;}finally{G();}}}")]
 [InlineData("class\\u0043 {}")][InlineData("classⅠ {}")]
 [InlineData("static class C {}")][InlineData("class C {int P{get;set;}}")]
 public void Source_syntax_and_version_contexts_match(string source)
 {
  var reference=Oracle.Observe(source,LanguageVersion.CSharp1);
  bool rejected=reference.SyntaxDiagnostics.Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error)||reference.CompilationDiagnostics.Any(d=>d.Id is "CS8022" or "CS0201" or "CS1023");
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));
  Assert.True(!rejected==parsed.Accepted,source+"\nOracle "+string.Join(";",reference.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage()))+"\nOurs "+string.Join(";",parsed.Issues));
 }
 [Theory][InlineData("Boolean.cs")][InlineData("EventArgs.cs")]
 public void Pinned_external_CSharp1_corpus(string file)
 {
  string source=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/Mono2004",file));var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
  Assert.DoesNotContain(oracle.SyntaxDiagnostics,d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
  using var ours=CSharpParser.Parse(new(source,"CSharp1",file,[]));Assert.True(ours.Accepted,file+" :: "+string.Join(";",ours.Issues));
 }
}
