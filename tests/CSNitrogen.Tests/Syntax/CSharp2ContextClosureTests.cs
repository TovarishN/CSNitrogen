using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2ContextClosureTests
{
 [Theory][InlineData("-")][InlineData("+")][InlineData("*")][InlineData("&")]
 public void Alias_qualified_types_force_cast_shape(string op)
 {
  string source="class C{object M(){return (A::T)"+op+"x;}}";
  using var p=CSharpParser.Parse(new(source,"CSharp2","cast.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="AliasCast");
 }
 [Theory]
 [InlineData("class C{object M(){return A::T;}}")]
 [InlineData("class C{object M(){return A::F<int>(x);}} ")]
 [InlineData("class C{C(){yield break;}}")]
 [InlineData("class C{object d=delegate {yield break;};}")]
 [InlineData("class C{object M(){try{}finally{yield break;}}}")]
 [InlineData("class C{object M(){try{yield return 1;}catch{}}}")]
 [InlineData("class C{object M(){try{}catch{yield return 1;}}}")]
 [InlineData("class C{object M(ref int x){yield break;}}")]
 [InlineData("class C{object M(out int x){x=0;yield break;}}")]
 [InlineData("class C{object M(){yield break;return null;}}")]
 public void Source_only_context_restrictions_are_enforced(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","context.cs",[]));Assert.False(p.Accepted);}
 [Fact]public void Iterator_context_diagnostic_anchors_the_yield_statement()
 {
  const string source="class C{C(){yield break;}}";
  using var p=CSharpParser.Parse(new(source,"CSharp2","context.cs",[]));
  var issue=Assert.Single(p.Issues,i=>i.Code=="CSN2022");Assert.Equal(IssueCategory.Syntax,issue.Category);
  Assert.Equal("yield break;",source.Substring(issue.Span.Start,issue.Span.Length));
 }
 [Theory]
 [InlineData("class C{object M(){return A::T.Member;}}")]
 [InlineData("class C{object M(){object d=delegate(ref int x){return x;};yield break;}}")]
 [InlineData("class C{object M(){try{try{}catch{}}finally{}yield return 1;}}")]

 [InlineData("class C{object M(){try{yield return 1;}finally{}}}")]
 [InlineData("class C{object M(){try{yield break;}catch{yield break;}}}")]
 [InlineData("class C{object M(){object d=delegate{return null;};yield break;}}")]
 [InlineData("class C{object P{get{yield break;}}}")]
 public void Neighboring_legal_syntax_remains_accepted(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","context.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
}
