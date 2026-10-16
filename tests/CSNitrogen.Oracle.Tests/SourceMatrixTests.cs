using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class SourceMatrixTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/source-grammar-cases.json")));
  foreach(var row in doc.RootElement.EnumerateArray())yield return [row.GetProperty("mode").GetString()!,row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
  using var review=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/source-review-cases.json")));
  foreach(var row in review.RootElement.EnumerateArray())yield return ["source",row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Grammar_context_matches_independent_CSharp1_observations(string mode,string source,bool expected)
 {
  string wrapped=mode switch {
   "type"=>"class C { "+source+" field; }",
   "expression"=>"class C { object a, b; object nameof(object x){return x;} object M(){return "+source+";} }",
   "statement"=>"class C {void M(){"+source+"}}",
   "attribute"=>source+" class C{}",
   "parameter"=>"class C {void M"+source+"{}}",
   _=>source};
  var oracle=Oracle.Observe(wrapped,LanguageVersion.CSharp1);
  bool oracleAccepted=!oracle.SyntaxDiagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error)&&!oracle.CompilationDiagnostics.Any(d=>d.Id is "CS8022" or "CS0201" or "CS1023" or "CS0231" or "CS0670");
  // Roslyn 5.0.0 accepts null-coalescing even with LanguageVersion.CSharp1.
  // C# 2 introduced it: retain this explicit oracle discrepancy, not a production feature.
  if(source=="a??b") { Assert.True(oracleAccepted); Assert.False(expected); }
  else Assert.True(expected==oracleAccepted,mode+" "+source+" :: "+string.Join(";",oracle.CompilationDiagnostics.Select(d=>d.Id+":"+d.GetMessage())));
  if(mode=="source") {using var document=CSharpParser.Parse(new(source,"CSharp1","matrix.cs",[]));Assert.Equal(expected,document.Accepted);return;}
  var rule=mode switch {"type"=>NamesTypesModule.TypeFragment,"expression"=>ExpressionsModule.ExpressionFragment,"statement"=>StatementsModule.StatementFragment,"attribute"=>AttributesModule.AttributeFragment,_=>AttributesModule.ParameterFragment};
  var language=CSharp1Composition.CreateBuilder().Build();using var parsed=language.Parse(source,rule);
  Assert.Equal(expected,parsed.Success&&!parsed.HasErrors&&!CSharpSyntaxPolicy.Check(parsed.Tree).Any());
 }
}
