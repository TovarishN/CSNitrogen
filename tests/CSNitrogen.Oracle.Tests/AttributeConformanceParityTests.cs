using System.Text.Json;
using System.Globalization;
using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class AttributeConformanceParityTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/attribute-conformance-cases.json")));
  foreach(var row in doc.RootElement.EnumerateArray())yield return [row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Attribute_grammar_observations(string source,bool expected)
 {
  string full=source.StartsWith("class",StringComparison.Ordinal)||source.Contains(" using ",StringComparison.Ordinal)?source:source+" class C{}";
  var oracle=Oracle.Observe(full,LanguageVersion.CSharp1);
  bool accepted=!oracle.SyntaxDiagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error)&&!oracle.CompilationDiagnostics.Any(d=>d.Id is "CS8022" or "CS1016");
  // The pinned first edition uses expression; modern editions use
  // non_assignment_expression. Roslyn preserves general syntax here.
  if(source is "[A(x+=1)]" or "[A(X=y=1)]")
  {
   Assert.True(accepted);Assert.True(expected);
   var attribute=Assert.Single(oracle.Tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>());
   Assert.IsAssignableFrom<AssignmentExpressionSyntax>(Assert.Single(attribute.ArgumentList!.Arguments).Expression);
  }
  else Assert.True(expected==accepted,source+" :: "+string.Join(";",oracle.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage())));
  using var parsed=CSharpParser.Parse(new(full,"CSharp1","attributes.cs",[]));Assert.Equal(expected,parsed.Accepted);
 }
 [Theory][InlineData("assembly")][InlineData("@assembly")][InlineData("\\u0061ssembly")][InlineData("module")][InlineData("@module")]
 public void Global_ownership_matches_compiler_syntax(string target)
 {
  string source="["+target+":A] class C{}";
  var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
  Assert.Single(oracle.Tree.GetCompilationUnitRoot().AttributeLists);
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","global.cs",[]));
  Assert.True(parsed.Accepted);Assert.Equal(1,parsed.Root.Attributes.Count);
 }
 [Fact]
 public void Formatting_and_escaped_target_identity_matches_compiler()
 {
  for(int code=0;code<=char.MaxValue;code++)
  {
   if(char.GetUnicodeCategory((char)code)!=UnicodeCategory.Format)continue;
   foreach(string spelling in new[]{((char)code).ToString(),"\\u"+code.ToString("x4"),"\\U0000"+code.ToString("X4")})
   {
    string source="[as"+spelling+"sembly:A] class C{}";
    var tree=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp1));
    Assert.DoesNotContain(tree.GetDiagnostics(),d=>d.Severity==DiagnosticSeverity.Error);
    using var parsed=CSharpParser.Parse(new(source,"CSharp14","format.cs",[]));
    Assert.True(parsed.Accepted);Assert.Equal(tree.GetCompilationUnitRoot().AttributeLists.Count,parsed.Root.Attributes.Count);
   }
  }
 }
}
