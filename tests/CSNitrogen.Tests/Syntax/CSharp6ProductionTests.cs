using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Sixth-edition additions through the production CSharp6 factory and parse wrapper.</summary>
public class CSharp6ProductionTests
{
 static ParseDocument Parse(string source,string profile="CSharp6")=>CSharpParser.Parse(new(source,profile,"six.cs",[]));
 static string Kind(ParseDocument p,int n)=>p.Tree.Language!.GetKindName(p.Tree.Kind(n));
 static string Issues(ParseDocument p)=>string.Join(";",p.Issues);
 public static TheoryData<string,string> Witnesses=>new()
 {
  {"class C{string s=$\"a{x}b\";}","InterpolatedRegularString"},
  {"class C{string s=$@\"a{x}\nb\";}","InterpolatedVerbatimString"},
  {"class C{object o=a?.b;}","NullConditionalAccess"},
  {"class C{object o=a?[0];}","NullConditionalElementAccess"},
  {"class C{string s=nameof(int);}","NameofExpression"},
  {"class C{void M(){try{}catch(E e)when(e!=null){}catch when(f){}catch{}}}","ExceptionFilter"},
  {"using static System.Math;","UsingStaticMarker"},
  {"class C{int P{get;}=1;}","PropertyInitializer"},
  {"class C{int M()=>1;}","ExpressionBody"},
  {"class C{int P=>1;}","ExpressionBodiedProperty"},
  {"class C{int this[int i]=>i;}","ExpressionBodiedIndexer"},
  {"class C{object o=new D{[0]=1,A=2};}","IndexInitializer"},
 };
 [Theory][MemberData(nameof(Witnesses))]
 public void Sixth_additions_are_accepted_with_typed_owners(string source,string kind)
 {
  using var p=Parse("/*\U00010400*/"+source);
  Assert.True(p.Accepted,Issues(p));
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>Kind(p,n)==kind);
 }
 [Theory][MemberData(nameof(Witnesses))]
 public void Fifth_profile_does_not_accept_sixth_additions(string source,string kind)
 {
  using var p=Parse(source,"CSharp5");
  Assert.False(p.Accepted&&Enumerable.Range(0,p.Tree.NodeCount).Any(n=>Kind(p,n)==kind),source);
 }
 [Theory]
 [InlineData("class C{object o=nameof(x);}")]
 [InlineData("class C{void M(){nameof(x);}}")]
 [InlineData("class C{object o=nameof(x).Length;}")]
 [InlineData("class C{void M(){y=nameof(a.b);}}")]
 public void The_nameof_tie_is_accepted_without_an_issue(string source)
 {
  using var p=Parse(source);
  Assert.True(p.Accepted,Issues(p));Assert.Empty(p.Issues);
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>Kind(p,n)=="Ambiguous");
 }
 [Theory]
 [InlineData("class C{async void M(){try{}catch{await(M());}}}")]
 [InlineData("class C{async void M(){try{}finally{await(M());}}}")]
 public void Await_in_catch_and_finally_is_sixth(string source)
 {
  using var six=Parse(source);Assert.True(six.Accepted,Issues(six));
  using var five=Parse(source,"CSharp5");Assert.Contains(five.Issues,i=>i.Code=="CSN2050");
 }
 [Theory]
 [InlineData("class C{async Task M()=>await x;}")]
 [InlineData("class C{async void M(){try{}catch(E)when(f){await(M());}}}")]
 [InlineData("class C{object o=new{a?.b};}")]
 [InlineData("class C{int P{get;}}")]
 [InlineData("class C{public int P{get;private set;}=F(1);}")]
 [InlineData("class C{int[] P{get;}={1,2};}")]
 [InlineData("class C{void M()=>x?.F();}")]
 [InlineData("class C{object o=a?.b.c?[1]?.d();}")]
 [InlineData("class C{string s=$\"{(a?b:c)}{F(x:1)}{x,-5:N2}\";}")]
 [InlineData("class C{public static C operator+(C a,C b)=>a;public static implicit operator int(C c)=>1;}")]
 public void Sixth_forms_pass_existing_policies(string source)
 {
  using var p=Parse(source);Assert.True(p.Accepted,Issues(p));
 }
 [Theory]
 [InlineData("class C{string s=$\"{a?b:c}\";}","CSN2062","a?b:c")]
 [InlineData("class C{string s=$\"{x,-a?b:c}\";}","CSN2062","-a?b:c")]
 [InlineData("class C{string s=$\"{x+(y)+(a?b:c)+d?e:f}\";}","CSN2062","x+(y)+(a?b:c)+d?e:f")]
 [InlineData("class C{int P{get{return 1;}}=1;}","CSN2060","=1;")]
 [InlineData("abstract class C{abstract int P{get;}=1;}","CSN2060","=1;")]
 [InlineData("class C{extern int P{get;}=1;}","CSN2060","=1;")]
 [InlineData("class C{C()=>F();}","CSN2061","=>F();")]
 [InlineData("class C{~C()=>F();}","CSN2061","=>F();")]
 [InlineData("class C{int P{get=>1;}}","CSN2061","=>1;")]
 [InlineData("class C{int P{set;}}","CSN2032","int P{set;}")]
 public void Sixth_source_local_checks(string source,string code,string text)
 {
  string shifted="/*\U00010400*/"+source;
  using var p=Parse(shifted);Assert.False(p.Accepted);
  int start=shifted.IndexOf(text,StringComparison.Ordinal);
  Assert.Contains(p.Issues,i=>i.Code==code&&i.Span.Start==start&&i.Span.Length==text.Length);
 }
 [Fact]
 public void Sixth_context_policy_is_inactive_for_predecessors()
 {
  using var p=Parse("class C{C()=>F();int P{get{return 1;}}=1;string s=$\"{a?b:c}\";}");
  Assert.Contains(p.Issues,i=>i.Code is "CSN2060" or "CSN2061" or "CSN2062");
  Assert.DoesNotContain(CSharpSyntaxPolicy.Check(p.Tree,new(5,0)),i=>i.Code is "CSN2060" or "CSN2061" or "CSN2062");
 }
 [Theory]
 [InlineData("class C{C()=>M();}")]
 [InlineData("class C{int P{get=>1;}}")]
 public void Expression_body_restriction_is_sixth_only(string source)
 {
  using var p=Parse(source,"CSharp7");Assert.DoesNotContain(p.Issues,i=>i.Code=="CSN2061");
 }
 [Fact]
 public void Struct_instance_property_initializer_requires_a_later_version()
 {
  const string s="struct S{int P{get;}=1;static int Q{get;}=2;}";
  using var p=Parse(s);Assert.False(p.Accepted);
  var issue=Assert.Single(p.Issues);Assert.Equal("CSN2063",issue.Code);Assert.Equal(IssueCategory.UnavailableFeature,issue.Category);
  Assert.Equal((s.IndexOf("=1;",StringComparison.Ordinal),3),(issue.Span.Start,issue.Span.Length));
 }
 /// <summary>CSN2062 walks each interpolation once; thousands of legal holes must not cost a tree walk each.</summary>
 [Fact]
 public void Interpolation_colon_check_scales_with_the_number_of_holes()
 {
  string body=string.Concat(Enumerable.Repeat("s=$\"{(a?b:c)}\";",4000));
  string source="class C{void M(){"+body+"}}";
  using(Parse(source)){}
  var clock=System.Diagnostics.Stopwatch.StartNew();
  using var p=Parse(source);clock.Stop();
  Assert.True(p.Accepted,Issues(p));
  Assert.True(clock.ElapsedMilliseconds<2500,$"{clock.ElapsedMilliseconds} ms");
 }
 [Fact]
 public void Fifth_profile_keeps_its_automatic_property_rule()
 {
  using var p=Parse("class C{int P{get;}}","CSharp5");Assert.Contains(p.Issues,i=>i.Code=="CSN2032");
 }
 [Theory]
 [InlineData("class Ƞ{}")]
 [InlineData("class \U00010400{}")]
 [InlineData("class \\U00010400{}")]
 public void Sixth_profile_uses_frozen_Unicode4_scalars(string source)
 {
  using var p=Parse(source);Assert.True(p.Accepted,Issues(p));
  Assert.Contains("unicode-4.0-scalar",CSNitrogen.Profiles.ProfileResolver.Resolve("CSharp6").Profile!.Policies);
 }
 [Fact]
 public void Sixth_profile_remains_partial()
 {
  using var p=Parse("class C{int M()=>1;}");Assert.True(p.Accepted);Assert.False(p.ProfileComplete);
 }
}
