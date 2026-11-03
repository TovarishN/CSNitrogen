using Microsoft.CodeAnalysis.CSharp;
using CSNitrogen.Parsing;
namespace CSNitrogen.Oracle.Tests;
/// <summary>ECMA-334 6 §19.2 integral_type_name: Roslyn at LanguageVersion.CSharp6 as an observation; the standard governs.</summary>
public class CSharp6EnumBaseObservationTests
{
 [Theory]
 [InlineData("enum E:System.Byte{A}")]
 [InlineData("enum E:Byte{A}")]
 public void Type_name_enum_bases_follow_the_sixth_edition(string source)
 {
  var roslyn=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp6)).GetDiagnostics().Select(d=>d.Id).ToArray();
  using var six=CSharpParser.Parse(new(source,"CSharp6","enum6.cs",[]));
  Assert.True(six.Accepted);
  Assert.Equal(Observed,roslyn.Length==0);
 }
 /// <summary>Recorded Roslyn 5.0.0 parse behaviour at version 6 for type-name enum bases.</summary>
 const bool Observed=true;
}
