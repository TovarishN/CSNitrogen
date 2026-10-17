using System.Text.Json;
using System.Globalization;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class AttributeConformanceTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/attribute-conformance-cases.json")));
  foreach(var row in doc.RootElement.EnumerateArray())yield return [row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Attribute_syntax_contexts(string source,bool expected)
 {
  string full=source.StartsWith("class",StringComparison.Ordinal)||source.Contains(" using ",StringComparison.Ordinal)?source:source+" class C{}";
  using var parsed=CSharpParser.Parse(new(full,"CSharp1","attributes.cs",[]));
  Assert.True(expected==parsed.Accepted,source+" :: "+string.Join(";",parsed.Issues));
 }
 [Theory][InlineData("assembly")][InlineData("@assembly")][InlineData("\\u0061ssembly")][InlineData("module")][InlineData("@module")]
 public void Global_targets_have_global_typed_ownership(string target)
 {
  using var parsed=CSharpParser.Parse(new("["+target+":A] class C{}","CSharp1","global.cs",[]));
  Assert.True(parsed.Accepted);Assert.Equal(1,parsed.Root.Attributes.Count);
  Assert.Equal(0,Assert.Single(parsed.Classes).Attributes.Count);
 }
 [Fact]
 public void Global_target_formatting_matches_the_recorded_runtime_categories()
 {
  using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/attribute-target-formatting.json")));
  var actual=data.RootElement.GetProperty("format").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
  var expected=Enumerable.Range(0,65536).Where(n=>char.GetUnicodeCategory((char)n)==UnicodeCategory.Format).ToArray();
  Assert.Equal(expected,actual);
  for(int code=0;code<=char.MaxValue;code++)
  {
   if(char.GetUnicodeCategory((char)code)!=UnicodeCategory.Format)continue;
   foreach(string spelling in new[]{((char)code).ToString(),"\\u"+code.ToString("x4"),"\\U0000"+code.ToString("X4")})
   {
    using var parsed=CSharpParser.Parse(new("[as"+spelling+"sembly:A] class C{}","CSharp14","format.cs",[]));
    Assert.True(parsed.Accepted,"U+"+code.ToString("X4")+" :: "+string.Join(";",parsed.Issues));
    Assert.Equal(1,parsed.Root.Attributes.Count);
   }
  }
 }
 [Theory][InlineData("assemblyExtra")][InlineData("@assemblyExtra")][InlineData("moduleName")]
 public void Longer_target_names_are_not_global_targets(string target)
 {
  using var parsed=CSharpParser.Parse(new("["+target+":A] class C{}","CSharp1","boundary.cs",[]));
  Assert.True(parsed.Accepted);Assert.Equal(0,parsed.Root.Attributes.Count);
  Assert.Equal(1,Assert.Single(parsed.Classes).Attributes.Count);
 }
 [Theory][InlineData("[A(")][InlineData("[A(X=)]")][InlineData("[A(,)]")][InlineData("[A(X=1,,2)]")]
 public void Recovery_nodes_remain_rejected_without_policy_exceptions(string source)
 {
  using var parsed=CSharpParser.Parse(new(source+" class C{}","CSharp1","broken-attribute.cs",[]));
  Assert.False(parsed.Accepted);
 }
 [Theory][InlineData("CSharp1")][InlineData("CSharp14")]
 public void Policy_is_source_located_and_does_not_reject_method_assignments(string profile)
 {
  const string invalid="[A(X=1, 2)] class C{}";
  using var bad=CSharpParser.Parse(new(invalid,profile,"located.cs",[]));
  var issue=Assert.Single(bad.Issues,i=>i.Code=="CSN2005");
  Assert.Equal(invalid.IndexOf("2",StringComparison.Ordinal),issue.Span.Start);
  using var good=CSharpParser.Parse(new("[A(X=1)] class C{void M(){x=1;}}",profile,"ordinary.cs",[]));
  Assert.True(good.Accepted);Assert.Equal(profile=="CSharp1",good.ProfileComplete);
 }
}
