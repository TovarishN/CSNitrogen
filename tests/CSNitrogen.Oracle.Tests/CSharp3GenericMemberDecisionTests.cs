using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Observations for a pending source-authority decision, not source admission.</summary>
public class CSharp3GenericMemberDecisionTests
{
 [Theory]
 [InlineData("class B{public void M<T>(){}} class C:B{void F(){base.M<int>();}}","GenericBaseMemberExpression","base.M<int>")]
 [InlineData("using A=N; namespace N{class T{public static void M<U>(){}}} class C{void F(){A::T.M<int>();}}","GenericMemberExpression","A::T.M<int>")]
 [InlineData("struct S{public void M<T>(){}} class C{unsafe void F(S* p){p->M<int>();}}","GenericPointerMemberExpression","p->M<int>")]
 public void Reviewed_second_and_partial_third_factories_preserve_generic_member_forms(string source,string kind,string spelling)
 {
  foreach(var version in new[]{LanguageVersion.CSharp2,LanguageVersion.CSharp3})
   Assert.DoesNotContain(Oracle.Observe(source,version).CompilationDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  foreach(string profile in new[]{"CSharp2","CSharp3"})
  {
   using var parsed=CSharpParser.Parse(new(source,profile,"generic-member.cs",[]));
   Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));Assert.Equal(profile=="CSharp2",parsed.ProfileComplete);
   int node=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))==kind);
   var span=parsed.Tree.Span(node);Assert.Equal(spelling,source.Substring(span.Start,span.Length));
  }
 }
}
