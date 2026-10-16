using System.Text.Json;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class ContextConformanceTests
{
 public static IEnumerable<object[]> Cases()
 {
  using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Corpus/context-conformance-cases.json")));
  foreach(var row in data.RootElement.EnumerateArray())yield return [row.GetProperty("source").GetString()!,row.GetProperty("expected").GetBoolean()];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Source_contexts(string source,bool expected)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","contexts.cs",[]));Assert.True(expected==parsed.Accepted,source+" :: "+string.Join(";",parsed.Issues));}
 [Theory]
 [InlineData("struct S{class C{int x=1;public C(){}~C(){}}}")]
 [InlineData("struct S{void M(){int x=1;}}")]
 [InlineData("struct S{static S(/* no parameters */){}}")]
 [InlineData("class C{int P{get{return 0;}}int Q{get{return 1;}}}")]
 [InlineData("class C{event E X{add{}remove{}}event E Y{add{}remove{}}}")]
 public void Context_state_does_not_leak_to_other_declarations(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","nested.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));}
 [Theory][InlineData("CSharp1",IssueCategory.UnavailableFeature)][InlineData("CSharp9",IssueCategory.UnavailableFeature)]
 [InlineData("CSharp10",IssueCategory.UnsupportedCoverage)][InlineData("CSharp14",IssueCategory.UnsupportedCoverage)]
 public void Later_struct_forms_remain_explicitly_unavailable_or_unsupported(string profile,IssueCategory category)
 {
  foreach(string source in new[]{"struct S{public S(){}}","struct S{int x=1;}"})
  {using var parsed=CSharpParser.Parse(new(source,profile,"version.cs",[]));Assert.False(parsed.Accepted);Assert.Contains(parsed.Issues,i=>i.Code=="CSN2011"&&i.Category==category);}
 }
 [Theory][InlineData("class C{event E X{add{")][InlineData("class C{static C(int")] [InlineData("class C{int P{get;")]
 public void Recovered_declarations_are_rejected_without_policy_exceptions(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","broken.cs",[]));Assert.False(parsed.Accepted);}
}

