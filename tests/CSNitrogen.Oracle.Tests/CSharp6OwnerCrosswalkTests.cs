using System.Security.Cryptography;
using System.Text.Json.Nodes;
using CSNitrogen.Profiles;
using Nitrogen.Grammar;
namespace CSNitrogen.Oracle.Tests;
/// <summary>Sixth-edition owners resolve in the actual CSharp6 factory; owner existence does not approve source contexts.</summary>
public class CSharp6OwnerCrosswalkTests
{
 static byte[] Bytes(string name)=>File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"CSharp6",name));
 static JsonObject Read(string name)=>JsonNode.Parse(Bytes(name))!.AsObject();
 static string Text(JsonNode n,string key)=>n[key]?.GetValue<string>()??"";
 static string[] Strings(JsonNode? n)=>n?.AsArray().Select(v=>v!.GetValue<string>()).ToArray()??[];
 static HashSet<string> FactoryOwners()
 {
  var assembly=typeof(FeatureCatalog).Assembly;
  string Resource(string name){using var s=assembly.GetManifestResourceStream("CSNitrogen."+name)!;using var reader=new StreamReader(s);return reader.ReadToEnd();}
  var owners=new HashSet<string>(StringComparer.Ordinal);
  foreach(var g in Strings(JsonNode.Parse(Resource("composition.json"))!["languages"]!.AsArray().Single(l=>Text(l!,"name")=="CSharp6")!["grammars"]))
  {
   var m=GrammarParser.Parse(Resource("grammar."+g["Grammars/".Length..])).File!.Modules.Single();
   foreach(var r in m.Rules)owners.Add(m.Name+"/rule:"+r.Name);
   foreach(var r in m.Rules.OfType<ExtensibleRule>())foreach(var a in r.Alternatives)owners.Add(m.Name+"/alternative:"+r.Name+"."+a.Name);
   foreach(var e in m.Extends)foreach(var a in e.Alternatives)owners.Add(m.Name+"/extension:"+e.Target+"."+a.Name);
  }
  return owners;
 }
 [Fact]public void Every_sixth_rule_is_indexed_once_and_owned_by_the_actual_factory()
 {
  var index=Read("csharp6-standard-index.json");var crosswalk=Read("csharp6-standard-crosswalk.json");
  Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes("csharp6-standard-index.json"))),Text(crosswalk,"indexSha256"));
  var ids=index["productions"]!.AsArray().Select(p=>Text(p!,"id")).ToArray();
  var rows=crosswalk["productions"]!.AsArray().Select(r=>r!).ToArray();
  Assert.Equal(436,ids.Length);Assert.Equal(ids,rows.Select(r=>Text(r,"standardId")));
  var owners=FactoryOwners();
  var missing=rows.SelectMany(r=>Strings(r["targets"]).Where(t=>!owners.Contains(t)).Select(t=>Text(r,"name")+" -> "+t)).ToArray();
  Assert.Empty(missing);
  Assert.All(rows,r=>Assert.True(Strings(r["targets"]).Length+Strings(r["policies"]).Length>0,Text(r,"name")));
  Assert.All(rows,r=>Assert.Equal("owner-candidate",Text(r,"reviewStatus")));
 }
 [Fact]public void Sixth_additions_are_owned_by_sixth_modules()
 {
  var rows=Read("csharp6-standard-crosswalk.json")["productions"]!.AsArray().Select(r=>r!).ToArray();
  string[] sixth=["CSharp.InterpolatedStrings/","CSharp.NullConditional/","CSharp.NameofExpressions/","CSharp.ExceptionFilters/","CSharp.UsingStatic/","CSharp.PropertyInitializers/","CSharp.ExpressionBodies/","CSharp.IndexInitializers/"];
  foreach(var name in new[]{"interpolated_string_expression","null_conditional_member_access","null_conditional_element_access","nameof_expression","exception_filter","using_static_directive","property_initializer","indexer_body","method_body","initializer_target"})
  {var row=rows.Single(r=>Text(r,"name")==name);Assert.Contains(Strings(row["targets"]),t=>sixth.Any(t.StartsWith));}
 }
 [Fact]public void Every_main_text_difference_has_a_non_syntax_disposition()
 {
  var diff=Read("csharp6-main-text-differences.json");var disp=Read("csharp6-main-text-dispositions.json");
  Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes("csharp6-main-text-differences.json"))),Text(disp,"differencesSha256"));
  var unmatched=diff["unmatched"]!.AsArray().Select(r=>Text(r!,"file")+":"+r!["line"]!.GetValue<int>()+":"+Text(r!,"sha256")).ToArray();
  var rows=disp["dispositions"]!.AsArray().Select(r=>r!).ToArray();
  Assert.Equal(unmatched,rows.Select(r=>Text(r,"file")+":"+r["line"]!.GetValue<int>()+":"+Text(r,"sha256")));
  Assert.All(rows,r=>{Assert.False(r["syntaxBearing"]!.GetValue<bool>());Assert.False(string.IsNullOrWhiteSpace(Text(r,"reason")));});
  var counts=diff["counts"]!;Assert.True(counts["paragraphs"]!.GetValue<int>()>6000);
  Assert.Equal(counts["paragraphs"]!.GetValue<int>(),counts["matched"]!.GetValue<int>()+rows.Length);
 }
 [Fact]public void The_PDF_annex_agrees_with_the_working_text()
 {
  var d=Read("csharp6-pdf-differences.json");var counts=d["counts"]!;
  Assert.Equal(436,counts["indexed"]!.GetValue<int>());Assert.Equal(436,counts["agreed"]!.GetValue<int>());
  Assert.Empty(d["differences"]!.AsArray());Assert.Empty(d["pdfOnly"]!.AsArray());
  Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(Bytes("csharp6-standard-index.json"))),Text(d,"indexSha256"));
 }
}
