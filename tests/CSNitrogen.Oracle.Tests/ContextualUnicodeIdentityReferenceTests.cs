using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class ContextualUnicodeIdentityReferenceTests
{
 [Theory][InlineData("\u180b")][InlineData("\u180c")][InlineData(@"\u180d")]
 public void Unicode4_combining_marks_do_not_form_accessor_keywords(string mark)
 {
  string source="class C{int P{get"+mark+"{return 1;}set{}}}";
  foreach(var version in new[]{LanguageVersion.CSharp2,LanguageVersion.CSharp3})
   Assert.Contains(Oracle.Observe(source,version).SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  foreach(string profile in new[]{"CSharp2","CSharp3"})
  {using var parsed=CSharpParser.Parse(new(source,profile,"accessor.cs",[]));Assert.False(parsed.Accepted);Assert.Contains(parsed.Issues,i=>i.Code=="CSN2023");}
  // The first-edition profile uses the frozen Unicode3 Cf classification;
  // modern Roslyn's CSharp1 switch does not restore that old character table.
  Assert.Contains(Oracle.Observe(source,LanguageVersion.CSharp1).SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  using var first=CSharpParser.Parse(new(source,"CSharp1","accessor.cs",[]));Assert.True(first.Accepted,string.Join(";",first.Issues));
 }
}
