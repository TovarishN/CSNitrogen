using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp5ProductionTests
{
 [Theory]
 [InlineData("x=>x")]
 [InlineData("async ()=>1")]
 [InlineData("from x in xs select x")]
 public void Await_requires_parentheses_around_lambda_and_query_operands(string operand)
 {
  string source="/*𐐀*/class C{async void M(){await "+operand+";}}";
  using var direct=CSharpParser.Parse(new(source,"CSharp5","operand5.cs",[]));
  Assert.False(direct.Accepted);
  Assert.Contains(direct.Issues,i=>i.Code=="CSN2030"&&i.Span.Start==source.IndexOf(operand,StringComparison.Ordinal)&&i.Span.Length==operand.Length);
 }
 [Theory]
 [InlineData("x=>x")]
 [InlineData("async ()=>1")]
 [InlineData("from x in xs select x")]
 public void Await_accepts_parenthesized_lambda_and_query_operands_without_type_claims(string operand)
 {
  string source="class C{async void M(){await ("+operand+");}}";
  using var p=CSharpParser.Parse(new(source,"CSharp5","parenthesized5.cs",[]));
  Assert.True(p.Accepted,string.Join(";",p.Issues));
 }
 [Theory]
 [InlineData("class C{void M(){await(M());}}","InvocationExpression")]
 [InlineData("class C{async void M(){await(M());}}","AwaitExpression")]
 [InlineData("class C{object f=()=>await(M());}","InvocationExpression")]
 [InlineData("class C{object f=async ()=>await(M());}","AwaitExpression")]
 [InlineData("class C{object f=async delegate{await(M());};}","AwaitExpression")]
 public void Actual_fifth_factory_selects_contextual_owners_and_preserves_UTF16_ranges(string body,string expected)
 {
  string source="/*𐐀*/"+body;using var p=CSharpParser.Parse(new(source,"CSharp5","context5.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);
  int start=source.IndexOf("await(M())",StringComparison.Ordinal);
  int owner=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==10&&Kind(n) is "InvocationExpression" or "AwaitExpression");Assert.Equal(expected,Kind(owner));
  string Kind(int n)=>p.Tree.Language!.GetKindName(p.Tree.Kind(n));
 }
 [Theory]
 [InlineData("class C{async void M(){lock(gate){await(M());}}}")]
 [InlineData("class C{async void M(){try{}catch{await(M());}}}")]
 [InlineData("class C{async void M(){try{}finally{await(M());}}}")]
 [InlineData("class C{async void M(){unsafe{await(M());}}}")]
 [InlineData("unsafe class C{async void M(){await(M());}}")]
 [InlineData("class C{async unsafe void M(){await(M());}}")]
 [InlineData("class C{async void M(){object f=delegate{await(M());};}}")]
 [InlineData("class C{async void M(){object f=()=>await(M());}}")]
 [InlineData("class C{async void M(){var q=from x in xs where await(M()) select x;}}")]
 [InlineData("class C{async void M(){var q=from x in xs select await(M());}}")]
 [InlineData("class C{async void M(){var q=from x in xs from y in await(M()) select y;}}")]
 [InlineData("class C{async void M(){var q=from x in xs join y in ys on await(M()) equals y select x;}}")]
 public void Fifth_await_source_restrictions_report_the_authored_operator(string source)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp5","restricted5.cs",[]));Assert.False(p.Accepted);
  Assert.Contains(p.Issues,i=>i.Code=="CSN2050"&&i.Span.Start==source.IndexOf("await",StringComparison.Ordinal)&&i.Span.Length==5);
 }
 [Theory]
 [InlineData("class C{async void M(ref int x){}}","ref")]
 [InlineData("class C{async void M(out int x){x=0;}}","out")]
 [InlineData("class C{object f=async (ref int x)=>x;}","ref")]
 [InlineData("class C{object f=async delegate(out int x){x=0;};}","out")]
 public void Async_formals_reject_ref_and_out_at_the_modifier(string source,string modifier)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp5","formal5.cs",[]));Assert.False(p.Accepted);
  Assert.Contains(p.Issues,i=>i.Code=="CSN2052"&&i.Span.Start==source.IndexOf(modifier,StringComparison.Ordinal)&&i.Span.Length==modifier.Length);
 }
 [Theory]
 [InlineData("class C{async void M(){int await;}}")]
 [InlineData("class C{object f=async (int await)=>1;}")]
 [InlineData("class C{async void M(){object f=delegate{int await;};}}")]
 [InlineData("class C{async void M(){object f=(int await)=>1;}}")]
 public void Raw_await_cannot_be_an_identifier_in_the_async_function(string source)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp5","identifier5.cs",[]));Assert.False(p.Accepted);
  Assert.Contains(p.Issues,i=>i.Code=="CSN2051"&&i.Span.Start==source.IndexOf("await",StringComparison.Ordinal)&&i.Span.Length==5);
 }
 [Theory]
 [InlineData("class C{async void M(){lock(await(M())){}}}")]
 [InlineData("class C{async void M(){lock(gate){object f=async ()=>await(M());}}}")]
 [InlineData("class C{async void M(){try{}catch{object f=async delegate{await(M());};}}}")]
 [InlineData("class C{async void M(){var q=from x in await(M()) select x;}}")]
 [InlineData("class C{async void M(){var q=from x in xs join y in await(M()) on x equals y select x;}}")]
 [InlineData("class C{async void M(){var q=from x in xs select async ()=>await(M());}}")]
 [InlineData("class C{[A(await(M()))] async void M(object x=await(M())){await(M());}}")]
 [InlineData("class C{async void M(){@await(M());}}")]
 [InlineData(@"class C{async void M(){\u0061wait(M());}}")]
 public void Source_local_checks_preserve_allowed_boundaries_and_identifier_escapes(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp5","allowed5.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.False(p.ProfileComplete);}
 [Theory][InlineData("await(M());",true)][InlineData("int await;",false)]
 public void Incremental_async_header_edits_match_full_trees_and_source_issues(string statement,bool acceptedWhenAsync)
 {
  string before="/*𐐀*/class C{void M(){"+statement+"}void N(){F();}}";int offset=before.IndexOf("void M",StringComparison.Ordinal);string after=before.Insert(offset,"async ");
  using var first=CSharpParser.ParseIncremental(new(before,"CSharp5","edit5.cs",[]));Assert.True(first.Accepted);
  using var second=CSharpParser.Reparse(first,new(after,"CSharp5","edit5.cs",[]),[new(offset,0,6)]);using var full=CSharpParser.Parse(new(after,"CSharp5","edit5.cs",[]));
  Assert.Equal(acceptedWhenAsync,second.Accepted);Assert.Equal(Nitrogen.SyntaxDumper.Dump(full.Tree),Nitrogen.SyntaxDumper.Dump(second.Tree));Assert.Equal(full.Issues,second.Issues);Assert.True(second.ReusedNodes>0);
  using var third=CSharpParser.Reparse(second,new(before,"CSharp5","edit5.cs",[]),[new(offset,6,0)]);using var restored=CSharpParser.Parse(new(before,"CSharp5","edit5.cs",[]));Assert.True(third.Accepted);Assert.Equal(Nitrogen.SyntaxDumper.Dump(restored.Tree),Nitrogen.SyntaxDumper.Dump(third.Tree));Assert.Equal(restored.Issues,third.Issues);
 }
 [Theory][InlineData("CSharp1",1)][InlineData("CSharp2",2)][InlineData("CSharp3",3)][InlineData("CSharp4",4)]
 public void Fifth_context_policy_is_inactive_for_predecessors(string profile,int major)
 {
  using var p=CSharpParser.Parse(new("class C{void M(){int await;}}",profile,"previous5.cs",[]));Assert.True(p.Accepted);Assert.True(p.ProfileComplete);
  Assert.Empty(CSharp5ContextPolicy.Check(p.Tree,new(major,0)));
 }
}
