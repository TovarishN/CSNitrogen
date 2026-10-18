using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Independent versioned observations of the predeclared test-only seam fixtures.</summary>
public class CSharp3SeamReferenceTests
{
 public static IEnumerable<object[]> Cases()
 {
  string path=Path.Combine(AppContext.BaseDirectory,"CoverageInputs/tests/CSNitrogen.Tests/Feasibility/CSharp3GrammarSeamTests.cs");
  var root=CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
  foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
  {
   bool expected=method.Identifier.ValueText=="Candidate_syntax_has_a_generated_owner_in_real_compilation_units";
   if(!expected && method.Identifier.ValueText!="Malformed_skeletons_are_rejected")continue;
   foreach(var attribute in method.AttributeLists.SelectMany(a=>a.Attributes).Where(a=>a.Name.ToString()=="InlineData"))
   {
    var source=(LiteralExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression;
    yield return [source.Token.ValueText,expected];
   }
  }
 }
 [Theory][MemberData(nameof(Cases))]
 public void Predeclared_shapes_have_independent_CSharp3_phase_classified_observations(string expression,bool expected)
 {
  string source="class C{object value="+expression+";}";
  var result=Oracle.Observe(source,LanguageVersion.CSharp3);
  // Roslyn defers these source-local restrictions/version checks to compilation.
  // Unknown names, delegate conversion and other binding errors are deliberately excluded.
  string? deferred=expression switch
  {
   "(int x,y)=>x"=>"CS0748",
   "(ref x)=>x" or "(int x=1)=>x" or "(params int[] xs)=>xs"=>"CS8024",
   "new C{{}}"=>"CS1920",
   _=>null
  };
  bool syntaxAccepted=!result.SyntaxDiagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error);
  if(deferred is not null)
  {
   Assert.True(syntaxAccepted);
   Assert.Contains(result.CompilationDiagnostics,d=>d.Id==deferred&&d.Severity==DiagnosticSeverity.Error);
  }
  bool accepted=syntaxAccepted&&deferred is null;
  Assert.True(expected==accepted,source+" :: "+string.Join(";",result.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage())));
 }
 [Theory]
 [InlineData("class C{System.Func<int,int> f=x=>x+1;}")]
 [InlineData("class C{public int X; object M(){return new C{X=1};}}")]
 [InlineData("class C{object M(){return new{X=1};}}")]
 [InlineData("class C{object M(){return new[]{1,2};}}")]
 public void Feature_availability_is_checked_separately_from_syntax(string source)
 {
  var second=Oracle.Observe(source,LanguageVersion.CSharp2);var third=Oracle.Observe(source,LanguageVersion.CSharp3);
  Assert.DoesNotContain(third.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  Assert.Contains(second.CompilationDiagnostics,d=>d.Id=="CS8023");
  Assert.DoesNotContain(third.CompilationDiagnostics,d=>d.Id=="CS8023");
 }
}
