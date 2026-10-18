using System.Text.Json.Nodes;
using System.Security.Cryptography;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen.Grammar;
namespace CSNitrogen.Oracle.Tests;
public class CSharp3CoverageGateTests
{
 static byte[] Bytes(string name){using var s=typeof(FeatureCatalog).Assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var b=new MemoryStream();s.CopyTo(b);return b.ToArray();}
 static JsonNode Read(string name)=>JsonNode.Parse(Bytes(name))!;
 static string Text(JsonNode n,string key)=>n[key]!.GetValue<string>();
 [Fact]public void Admitted_ledger_owners_exist_in_the_real_selected_twenty_six_module_factory()
 {
  var gate=Read("csharp3-coverage-gate.json");var actual=CSharp3Composition.CreateBuilder().Build();Assert.Equal(26,actual.Modules.Count);
  Assert.Equal(actual.Modules.Select(m=>m.Name).Order(),gate["factory"]!["modules"]!.AsArray().Select(n=>n!.GetValue<string>()).Order());
  var owners=new HashSet<string>();var modules=new Dictionary<string,ModuleDecl>();
  // Parse original Nitrogen grammar resources rather than inferring owners from crosswalk strings.
  var assembly=typeof(FeatureCatalog).Assembly;
  foreach(string name in assembly.GetManifestResourceNames().Where(n=>n.StartsWith("CSNitrogen.grammar.",StringComparison.Ordinal)))
  {using var stream=assembly.GetManifestResourceStream(name)!;using var reader=new StreamReader(stream);var parsed=GrammarParser.Parse(reader.ReadToEnd());Assert.NotNull(parsed.File);foreach(var module in parsed.File.Modules)modules.Add(module.Name,module);}
  foreach(var m in actual.Modules)
  {
   var module=modules[m.Name];foreach(var rule in module.Rules)owners.Add(module.Name+"/rule:"+rule.Name);
   foreach(var rule in module.Rules.OfType<ExtensibleRule>())foreach(var a in rule.Alternatives)owners.Add(module.Name+"/alternative:"+rule.Name+"."+a.Name);
   foreach(var extend in module.Extends)foreach(var a in extend.Alternatives)owners.Add(module.Name+"/extension:"+extend.Target+"."+a.Name);
  }
  Assert.All(gate["owners"]!.AsArray(),o=>Assert.Contains(o!.GetValue<string>(),owners));
  Assert.Equal("CSharp.CompilationUnits",Text(gate["factory"]!,"rootModule"));Assert.Equal("CompilationUnit",Text(gate["factory"]!,"start"));
  Assert.True(FeatureCatalog.ThirdSourceReviewed);Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 [Fact]public void Admission_hash_dependencies_are_directed_and_representative_evidence_is_embedded()
 {
  var gate=Read("csharp3-coverage-gate.json");var inputs=gate["reviewInputs"]!.AsObject();var second=Read("csharp2-coverage-gate.json")["reviewInputs"]!.AsObject();
  Assert.False(inputs.ContainsKey("CSNitrogen.source.Profiles/CSharp2CoverageContract.cs"));Assert.False(inputs.ContainsKey("CSNitrogen.source.Profiles/CSharp3CoverageContract.cs"));
  Assert.True(second.ContainsKey("CSNitrogen.source.Profiles/CSharp3CoverageContract.cs"));
  Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes("source.Profiles/CSharp3CoverageContract.cs"))),second["CSNitrogen.source.Profiles/CSharp3CoverageContract.cs"]!.GetValue<string>());
  Assert.Equal(6,inputs.Count(x=>x.Key.StartsWith("CSNitrogen.csharp3-evidence.",StringComparison.Ordinal)));Assert.Equal(5,inputs.Count(x=>x.Key.StartsWith("CSNitrogen.csharp3-review.",StringComparison.Ordinal)));
  foreach(var item in inputs)Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes(item.Key["CSNitrogen.".Length..]))),item.Value!.GetValue<string>());
  Assert.Equal(455,Read("csharp3-standard-crosswalk.json")["productions"]!.AsArray().Count);Assert.Equal(1175,Read("csharp3-standard-index.json")["productions"]!.AsArray().Sum(r=>r!["alternatives"]!.AsArray().Count));
 }
 [Fact]public void Reviewed_policy_ownership_includes_both_executed_third_version_context_policies()
 {
  var expected=new Dictionary<string,string[]>{
   ["attribute-argument-order"]=["Parsing/CSharpAttributePolicy.cs"],
   ["attribute-target-recognition"]=["Parsing/CSharpAttributePolicy.cs"],
   ["csharp-trivia"]=["Parsing/CSharpTrivia.cs"],
   ["csharp1-preprocessing"]=["Preprocessing/ConditionalPreprocessor.cs","Preprocessing/DirectiveScanner.cs","Preprocessing/DirectiveEffects.cs"],
   ["generated-syntax-context-checks"]=["Parsing/CSharpSyntaxPolicy.cs","Parsing/CSharp3ExpressionPolicy.cs","Parsing/CSharp3DeclarationPolicy.cs","Parsing/CSharpAttributePolicy.cs","Parsing/CSharpDeclarationPolicy.cs","Parsing/CSharpCompilationUnitPolicy.cs","Parsing/UnicodePolicy.cs"]};
  var groups=Read("csharp3-coverage-gate.json")["policyGroups"]!.AsObject();Assert.Equal(expected.Keys.Order(),groups.Select(g=>g.Key).Order());
  foreach(var group in expected)Assert.Equal(group.Value.Order(),groups[group.Key]!.AsArray().Select(x=>x!.GetValue<string>()).Order());
 }

}
