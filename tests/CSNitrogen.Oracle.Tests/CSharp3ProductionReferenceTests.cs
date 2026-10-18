using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3ProductionReferenceTests
{
 public static IEnumerable<object[]> PositiveSources()
 {
  var inputs=new[]{
   ("CSharp3ProductionTests.cs","Third_version_expressions_are_accepted_and_unavailable_in_the_second",true),
   ("CSharp3ConstructionTests.cs","Construction_additions_are_versioned",true),
   ("CSharp3DeclarationTests.cs","Third_version_declarations_have_a_preceding_version_boundary",false),
   ("CSharp3DeclarationTests.cs","Shared_baseline_declarations_keep_their_meaning",false)};
  foreach(var (file,name,expression) in inputs)
  {
   string path=Path.Combine(AppContext.BaseDirectory,"CoverageInputs/tests/CSNitrogen.Tests/Syntax",file);
   var root=CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
   var method=root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m=>m.Identifier.ValueText==name);
   foreach(var attribute in method.AttributeLists.SelectMany(a=>a.Attributes).Where(a=>a.Name.ToString()=="InlineData"))
   {
    string source=((LiteralExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression).Token.ValueText;
    yield return [expression?"class C{object value="+source+";}":source];
   }
  }
 }
 [Theory][MemberData(nameof(PositiveSources))]
 public void Positive_production_fixtures_have_independent_CSharp3_syntax_observations(string source)
 {
  var observation=Oracle.Observe(source,LanguageVersion.CSharp3);
  Assert.DoesNotContain(observation.SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  using var parsed=CSharpParser.Parse(new(source,"CSharp3","positive.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));Assert.False(parsed.ProfileComplete);
 }
 [Fact]public void Historical_raw_query_reservation_agrees_with_Roslyn_syntax_rejection()
 {
  const string source="class C{object value=from x in xs select x.select;}";
  Assert.Contains(Oracle.Observe(source,LanguageVersion.CSharp3).SyntaxDiagnostics,d=>d.Id=="CS1525");
  using var historical=CSharpParser.Parse(new(source,"CSharp3","query-word.cs",[]));Assert.False(historical.Accepted);Assert.Contains(historical.Issues,i=>i.Code=="CSN2030");
 }
}

public class CSharp3StatementCategoryReferenceTests
{
 [Theory]
 [InlineData("new C{X=1};",true)]
 [InlineData("new {X=1};",false)]
 [InlineData("new []{1};",false)]
 public void Creation_statement_categories_have_independent_source_local_diagnostics(string statement,bool accepted)
 {
  string source="class C{int X;void M(){"+statement+"}}";
  var observed=Oracle.Observe(source,LanguageVersion.CSharp3);
  Assert.Equal(!accepted,observed.CompilationDiagnostics.Any(d=>d.Id=="CS0201"));
  using var parsed=CSharpParser.Parse(new(source,"CSharp3","statement.cs",[]));Assert.Equal(accepted,parsed.Accepted);
 }
}
