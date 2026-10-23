using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4AttributeStructureTests
{
 static string Text(string source,TextSpan span)=>source.Substring(span.Start,span.Length);
 [Fact]public void Constructor_labels_values_and_named_members_keep_distinct_typed_owners_and_UTF16_spans()
 {
  const string source="/*𐐀*/[A(p:F(x:1),q:2,Field=3)]class C{}";
  using var p=CSharpParser.Parse(new(source,"CSharp4","shape.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  var constructor=Enumerable.Range(0,p.Tree.NodeCount).Where(n=>PositionalAttributeArgumentNode.Is(p.Tree,n)).Select(n=>SyntaxView.Cast<PositionalAttributeArgumentNode>(p.Tree,n)).ToArray();
  Assert.Equal(2,constructor.Length);var first=constructor[0];Assert.True(first.Name.HasValue);
  var label=SyntaxView.Cast<NamedArgumentNameArgumentName>(p.Tree,first.Name.Value.Index).Name;
  Assert.Equal("p",label.ToString());Assert.Equal(source.IndexOf("p:",StringComparison.Ordinal),label.Span.Start);
  Assert.Equal("p:",Text(source,first.Name.Value.Span));Assert.Equal("F(x:1)",Text(source,first.Value.Span));Assert.Equal(source.IndexOf("F(x:1)",StringComparison.Ordinal),first.Value.Span.Start);
  Assert.Equal("q:",Text(source,constructor[1].Name.Value.Span));Assert.Equal("2",Text(source,constructor[1].Value.Span));
  var member=SyntaxView.Cast<NamedAttributeArgumentNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>NamedAttributeArgumentNode.Is(p.Tree,n)));
  Assert.Equal("Field",member.Name.ToString());Assert.Equal("3",Text(source,member.Value.Span));Assert.Equal(source.IndexOf("Field",StringComparison.Ordinal),member.Name.Span.Start);
  var argument=SyntaxView.Cast<ArgumentNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>ArgumentNode.Is(p.Tree,n)));
  Assert.Equal("x:",Text(source,argument.Name.Value.Span));Assert.Equal("1",Text(source,argument.Value.Span));
 }
 [Theory][InlineData("CSharp1")][InlineData("CSharp2")][InlineData("CSharp3")]
 public void Predecessor_attribute_values_keep_their_types_and_absent_name_views(string profile)
 {
  const string source="[A(1,Field=2)]class C{}";using var p=CSharpParser.Parse(new(source,profile,"old.cs",[]));Assert.True(p.Accepted);Assert.True(p.ProfileComplete);
  var positional=SyntaxView.Cast<PositionalAttributeArgumentNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>PositionalAttributeArgumentNode.Is(p.Tree,n)));
  Assert.False(positional.Name.HasValue);Assert.Equal("1",Text(source,positional.Value.Span));Assert.Equal(3,positional.Value.Span.Start);
  var member=SyntaxView.Cast<NamedAttributeArgumentNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>NamedAttributeArgumentNode.Is(p.Tree,n)));
  Assert.Equal("Field",member.Name.ToString());Assert.Equal("2",Text(source,member.Value.Span));
 }
 [Theory]
 [InlineData("[A(x:1),B(0)]class C{}")]
 [InlineData("[A(x:F(x:1),y:2),B(0,x:1)]class C{}")]
 [InlineData("[A(x:1,Field=2),B(0)]class C{}")]
 public void Ordering_and_logical_name_state_stays_with_each_constructor_list(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","scope.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Theory]
 [InlineData("[A(x:1,@x:2)]class C{}","CSN2040","@x")]
 [InlineData("[A(x:1,2)]class C{}","CSN2040","2")]
 [InlineData("[A(Field=1,x:2)]class C{}","CSN2005","x:2")]
 public void Rejected_argument_issues_select_the_authored_constructor_piece(string body,string code,string expected)
 {
  string source="/*𐐀*/"+body;using var p=CSharpParser.Parse(new(source,"CSharp4","issue.cs",[]));Assert.False(p.Accepted);
  var issue=Assert.Single(p.Issues,i=>i.Code==code);Assert.Equal(expected,Text(source,issue.Span));Assert.Equal(source.IndexOf(expected,StringComparison.Ordinal),issue.Span.Start);
 }
 [Fact]public void Omitting_named_arguments_removes_constructor_labels_and_preserves_other_capabilities()
 {
  var all=CSharp4Composition.CreateBuilder().Build();var builder=new LanguageBuilder();foreach(var m in all.Modules)if(m.Name!="CSharp.NamedArguments")builder.Add(m);var language=builder.Build();
  using var absent=language.Parse("[A(x:1)]class C{}",CSharp4Composition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var original=language.Parse("[A(1,Field=2)]class C{}",CSharp4Composition.Start);Assert.True(original.Success&&!original.HasErrors);
  using var other=language.Parse("interface I<out T>{void M(int x=1);}",CSharp4Composition.Start);Assert.True(other.Success&&!other.HasErrors);
 }
 [Theory][InlineData("CSharp.OptionalParameters")][InlineData("CSharp.Variance")]
 public void Constructor_labels_survive_other_capability_omissions(string omitted)
 {
  var all=CSharp4Composition.CreateBuilder().Build();var builder=new LanguageBuilder();foreach(var m in all.Modules)if(m.Name!=omitted)builder.Add(m);var language=builder.Build();
  using var p=language.Parse("[A(x:1)]class C{}",CSharp4Composition.Start);Assert.True(p.Success&&!p.HasErrors);
 }
 [Theory][InlineData("CSharp4")][InlineData("CSharp5")][InlineData("CSharp7_3")][InlineData("CSharp14")]
 public void Factories_select_the_attribute_extension_with_only_fourth_snapshot_admitted(string profile)
 {using var p=CSharpParser.Parse(new("[A(x:1,Field=2)]class C{}",profile,"later.cs",[]));Assert.True(p.Accepted);Assert.Equal(profile=="CSharp4",p.ProfileComplete);}
 [Theory][InlineData("[A(x:")][InlineData("[A(x:1,")][InlineData("[A(x:1,Field=")]
 public void Incomplete_attribute_labels_report_errors_without_policy_exceptions(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","incomplete.cs",[]));Assert.False(p.Accepted);Assert.NotEmpty(p.Issues);}
}
