using System.Security.Cryptography;
using System.Text.Json;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class HistoricalUnicodeParityTests
{
 [Fact]
 public void Every_BMP_identifier_category_matches_independently_read_original_records()
 {
  string directory=Path.Combine(AppContext.BaseDirectory,"Unicode3");
  byte[] raw=File.ReadAllBytes(Path.Combine(directory,"UnicodeData-3.0.0.txt"));
  using var provenance=JsonDocument.Parse(File.ReadAllText(Path.Combine(directory,"provenance.json")));
  Assert.Equal(provenance.RootElement.GetProperty("dataSha256").GetString(),Convert.ToHexStringLower(SHA256.HashData(raw)));
  var categories=ReadCategories();
  var language=CSharp1Composition.CreateBuilder().Build();
  for(int code=0;code<65536;code++)
  {
   bool start=code==95||categories[code] is "Lu" or "Ll" or "Lt" or "Lm" or "Lo" or "Nl";
   bool part=start||categories[code] is "Nd" or "Pc" or "Mn" or "Mc" or "Cf";
   string escape="\\u"+code.ToString("X4");
   using var a=language.Parse(escape,LexicalModule.IdentifierFragment);
   using var b=language.Parse("a"+escape,LexicalModule.IdentifierFragment);
   Assert.True(start==(a.Success&&!a.HasErrors&&!CSharpSyntaxPolicy.Check(a.Tree,new(1,0)).Any()),"Historical start U+"+code.ToString("X4"));
   Assert.True(part==(b.Success&&!b.HasErrors&&!CSharpSyntaxPolicy.Check(b.Tree,new(1,0)).Any()),"Historical part U+"+code.ToString("X4"));
  }
 }
 [Theory][InlineData("\u1885",true,false)][InlineData("a\u1369",true,false)][InlineData("\u0220",false,true)]
 public void Historical_and_current_compiler_differences_are_explicit(string name,bool historical,bool current)
 {
  string source="class "+name+"{}";
  using var old=CSharpParser.Parse(new(source,"CSharp1","old.cs",[]));Assert.Equal(historical,old.Accepted);
  using var modern=CSharpParser.Parse(new(source,"CSharp14","current.cs",[]));Assert.Equal(current,modern.Accepted);
  Assert.Equal(current,!Oracle.Observe(source,LanguageVersion.CSharp1).SyntaxDiagnostics.Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
 }
 private static string[] ReadCategories()
 {
  byte[] raw=File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"Unicode3/UnicodeData-3.0.0.txt"));
  string[] categories=Enumerable.Repeat("Cn",65536).ToArray();int first=-1;
  foreach(string line in System.Text.Encoding.UTF8.GetString(raw).Split('\n',StringSplitOptions.RemoveEmptyEntries))
  {
   string[] fields=line.TrimEnd('\r').Split(';');int code=Convert.ToInt32(fields[0],16);
   if(fields[1].Contains(", First>")){first=code;continue;}
   if(fields[1].Contains(", Last>"))
   {for(int item=first;item<=Math.Min(code,65535);item++)categories[item]=fields[2];first=-1;continue;}
   if(code<65536)categories[code]=fields[2];
  }
  Assert.Equal(-1,first);
  return categories;
 }
 [Fact]
 public void Historical_whitespace_matches_original_categories_through_the_public_parser()
 {
  var categories=ReadCategories();
  for(int code=0;code<65536;code++)
  {
   bool space=categories[code]=="Zs"||code is 9 or 10 or 11 or 12 or 13 or 0x85 or 0x2028 or 0x2029;
   using var parsed=CSharpParser.Parse(new("class C{"+(char)code+"}","CSharp1","space.cs",[]));
   Assert.True(space==parsed.Accepted,"Historical whitespace U+"+code.ToString("X4"));
  }
 }
 [Fact]
 public void Every_historical_format_character_has_global_target_identity()
 {
  var categories=ReadCategories();
  for(int code=0;code<65536;code++)
  {
   if(categories[code]!="Cf")continue;
   foreach(string spelling in new[]{((char)code).ToString(),"\\u"+code.ToString("X4"),"\\U0000"+code.ToString("x4")})
   {
    string source="[as"+spelling+"sembly:A] class C{}";
    using var parsed=CSharpParser.Parse(new(source,"CSharp1","format.cs",[]));
    Assert.True(parsed.Accepted);Assert.Equal(1,parsed.Root.Attributes.Count);
   }
  }
 }
 [Fact]
 public void Historical_formatting_identity_is_shared_with_preprocessing()
 {
  var categories=ReadCategories();
  for(int code=0;code<65536;code++)
  {
   if(categories[code]!="Cf")continue;
   foreach(string spelling in new[]{((char)code).ToString(),"\\u"+code.ToString("X4"),"\\U0000"+code.ToString("x4")})
   {
    string source="#define A"+spelling+"\n#if A\nclass C{}\n#endif";
    using var parsed=CSharpParser.Parse(new(source,"CSharp1","symbol.cs",[]));
    Assert.True(parsed.Accepted);Assert.Single(parsed.Classes);
   }
  }
 }
}
