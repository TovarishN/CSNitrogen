using System.Security.Cryptography;
using System.Text.Json;
using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3ExternalCorpusTests
{
 private static string Root=>Path.Combine(AppContext.BaseDirectory,"Corpus","Mono2008");
 public static IEnumerable<object[]> Sources()
 {
  using var provenance=JsonDocument.Parse(File.ReadAllText(Path.Combine(Root,"provenance.json")));
  foreach(var file in provenance.RootElement.GetProperty("files").EnumerateArray())
   yield return [file.GetProperty("local").GetString()!,file.GetProperty("sha256").GetString()!];
 }
 [Theory][MemberData(nameof(Sources))]
 public void Every_predeclared_historical_source_has_pinned_bytes_and_a_CSharp3_parse(string name,string hash)
 {
  byte[] bytes=File.ReadAllBytes(Path.Combine(Root,name));Assert.Equal(hash,Convert.ToHexStringLower(SHA256.HashData(bytes)));
  string source=System.Text.Encoding.UTF8.GetString(bytes);
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp3));
  Assert.DoesNotContain(reference.GetDiagnostics(),d=>d.Severity==DiagnosticSeverity.Error);
  using var third=CSharpParser.Parse(new(source,"CSharp3",name,[]));
  Assert.True(third.Accepted,string.Join(";",third.Issues));Assert.True(third.ProfileComplete);
  Assert.Equal(source,third.OriginalText);
 }
 [Fact]public void Predeclared_selection_and_license_notices_are_retained()
 {
  Assert.Equal("11cbb0864c8f44d4aba3c2cd67b2dea9f23027c377711679590f8b8fd014038f",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root,"provenance.json")))));
  using var provenance=JsonDocument.Parse(File.ReadAllText(Path.Combine(Root,"provenance.json")));
  Assert.Equal("d7a4c6473ed2cd096c9223519c378bf136912a97",provenance.RootElement.GetProperty("revision").GetString());
  var files=provenance.RootElement.GetProperty("files").EnumerateArray().ToArray();Assert.Equal(20,files.Length);
  string[] groups=["linq","initialize","implicitarray","anontype","collectioninit","autoproperty","exmethod"];
  var expected=groups.SelectMany(group=>Enumerable.Range(1,group=="implicitarray"?2:3).Select(n=>$"gtest-{group}-{n:00}.cs")).Order(StringComparer.Ordinal).ToArray();
  Assert.Equal(expected,files.Select(f=>f.GetProperty("local").GetString()).Order(StringComparer.Ordinal));
  Assert.Equal(expected.Select(n=>"mcs/tests/"+n),files.Select(f=>f.GetProperty("sourcePath").GetString()).Order(StringComparer.Ordinal));
  foreach(var file in files)Assert.StartsWith($"gtest-{file.GetProperty("group").GetString()}-",file.GetProperty("local").GetString());
  var licenses=provenance.RootElement.GetProperty("licenseFiles").EnumerateArray().ToArray();
  Assert.Equal(new[]{"COPYING","LICENSE","LICENSE.GPL"},licenses.Select(f=>f.GetProperty("local").GetString()).Order(StringComparer.Ordinal));
  foreach(var file in licenses)
   Assert.Equal(file.GetProperty("sha256").GetString(),Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root,file.GetProperty("local").GetString()!)))));
 }
 [Theory]
 [InlineData("using I=System.Int32; unsafe struct S{fixed I x[2];}")]
 [InlineData("unsafe struct S{fixed System.Int32 x[2];}")]
 [InlineData("using N=System; unsafe struct S{fixed N::Int32 x[2];}")]
 public void Primitive_denotation_through_a_name_has_independent_compiler_evidence(string source)
 {
  Assert.DoesNotContain(Oracle.Observe(source,LanguageVersion.CSharp3).CompilationDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
  using var parsed=CSharpParser.Parse(new(source,"CSharp3","buffer.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));
 }
}
