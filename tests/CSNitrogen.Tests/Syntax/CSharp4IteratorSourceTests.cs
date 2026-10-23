using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4IteratorSourceTests
{
 [Theory]
 [InlineData("class C{C(){yield break;}}")]
 [InlineData("class C{static C(){yield break;}}")]
 [InlineData("class C{~C(){yield break;}}")]
 [InlineData("class C{event E Ev{add{yield break;}remove{}}}")]
 [InlineData("class C{object M(){return delegate{yield return 1;};}}")]
 [InlineData("class C{object M(){return ()=>{yield break;};}}")]
 [InlineData("class C{object M(){try{}finally{yield break;}}}")]
 [InlineData("class C{object M(){try{yield return 1;}catch{}}}")]
 [InlineData("class C{object M(){try{}catch{yield return 1;}}}")]
 [InlineData("class C{object M(ref int x){yield break;}}")]
 [InlineData("class C{object M(out int x){x=0;yield break;}}")]
 [InlineData("class C{object M(){unsafe{}yield break;}}")]
 [InlineData("class C{object M(){yield break;return null;}}")]
 public void Fourth_source_iterator_restrictions_are_enforced_without_binding(string source)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp4","iterator.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code=="CSN2022");Assert.True(p.ProfileComplete);
 }
 [Theory]
 [InlineData("class C{object M(){yield return 1;yield break;}}")]
 [InlineData("class C{object P{get{yield return 1;}}}")]
 [InlineData("class C{public static object operator +(C x){yield return 1;}}")]
 [InlineData("class C{object M(){try{yield return 1;}finally{Cleanup();}}}")]
 [InlineData("class C{object M(){try{yield break;}catch{yield break;}}}")]
 [InlineData("class C{object M(){yield return 1;object f=delegate{return null;};}}")]
 [InlineData("class C{object M(){yield return 1;object f=()=>{return 1;};}}")]
 [InlineData("unsafe class C{object M(){yield return 1;}}")]
 [InlineData("class C{object M(){try{yield return 1;try{}catch{}}finally{}}}")]
 [InlineData("class C{object M(int x=1){yield return x;}}")]
 public void Valid_iterator_shapes_and_nested_function_returns_remain_syntax(string source)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp4","valid-iterator.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.DoesNotContain(p.Issues,i=>i.Code=="CSN2022");
 }
 [Theory]
 [InlineData("class C{object M(){try{yield return 1;}catch{}}}","yield return 1;")]
 [InlineData("class C{object M(){try{}finally{yield break;}}}","yield break;")]
 [InlineData("class C{object M(ref int x){yield break;}}","ref int x")]
 [InlineData("class C{object M(){unsafe{}yield break;}}","unsafe{}")]
 [InlineData("class C{object M(){yield break;return null;}}","return null;")]
 public void Iterator_issues_select_the_authored_piece_after_supplementary_trivia(string body,string expected)
 {
  string source="/*𐐀*/"+body;using var p=CSharpParser.Parse(new(source,"CSharp4","span.cs",[]));
  var issue=Assert.Single(p.Issues,i=>i.Code=="CSN2022");Assert.Equal(expected,source.Substring(issue.Span.Start,issue.Span.Length));Assert.Equal(source.IndexOf(expected,StringComparison.Ordinal),issue.Span.Start);
 }
 [Theory]
 [InlineData("class C{object M(){yield return 1;}}","class C{object M(){yield return 1;return null;}}")]
 [InlineData("class C{object M(int x){yield return x;}}","class C{object M(ref int x){yield return x;}}")]
 public void Incremental_iterator_context_changes_match_full_policy_recomputation(string before,string after)
 {
  using var old=CSharpParser.ParseIncremental(new(before,"CSharp4","edit.cs",[]));Assert.True(old.Accepted);
  using var edited=CSharpParser.Reparse(old,new(after,"CSharp4","edit.cs",[]),new[]{TextChange.Between(before,after)});
  using var full=CSharpParser.Parse(new(after,"CSharp4","edit.cs",[]));Assert.False(edited.Accepted);Assert.Equal(full.Issues,edited.Issues);Assert.Equal(full.MaskedText,edited.MaskedText);
 }
}
