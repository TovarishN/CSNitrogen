using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
using P=CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3ConstructionTests
{
 [Theory]
 [InlineData("new C{X=1,Y=2,}")][InlineData("new C(){X=1}")][InlineData("new C{Child={X=1}}")]
 [InlineData("new C{1,2,}")][InlineData("new C{{1,2},{3,4}}")][InlineData("new C{}")]
 [InlineData("new {X=1,Y=a}")][InlineData("new {x,a.Name}")][InlineData("new {}")]
 [InlineData("new []{1,2,}")][InlineData("new [,]{{1,2},{3,4}}")]
 [InlineData("new C{X=1}.X")][InlineData("new {X=1}.X")][InlineData("new []{1}.Length")]
 [InlineData("new C{F=x=>x+1}")][InlineData("new C{X=from x in xs select x}")]
 public void Construction_additions_are_versioned(string expression)
 {
  string source="class C{object x="+expression+";}";
  using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.True(third.Accepted,string.Join(";",third.Issues));
  using var second=CSharpParser.Parse(new(source,"CSharp2","second.cs",[]));Assert.False(second.Accepted);Assert.True(second.ProfileComplete);
 }
 [Theory]
 [InlineData("new C{,}")][InlineData("new C{X=}")][InlineData("new C{{}}")]
 [InlineData("new {X=}")][InlineData("new [2]{1}")][InlineData("new {a+b}")]
 [InlineData("new {F()}")][InlineData("new {a[0]}")][InlineData("new C{1,x=2}")]
 [InlineData("new C{X=1,2}")]
 public void Malformed_and_source_category_neighbors_are_rejected(string expression)
 {using var result=CSharpParser.Parse(new("class C{object x="+expression+";}","CSharp3","bad.cs",[]));Assert.False(result.Accepted);}
 [Theory]
 [InlineData("CSharp.ObjectInitializers","new C{X=1}")]
 [InlineData("CSharp.AnonymousTypes","new {X=1}")]
 [InlineData("CSharp.ImplicitArrays","new []{1}")]
 public void Construction_modules_are_removable(string omitted,string expression)
 {
  var full=CSharp3Composition.CreateBuilder().Build();Assert.Contains(full.Modules,m=>m.Name==omitted);
  var builder=new LanguageBuilder();foreach(var m in full.Modules)if(m.Name!=omitted)builder.Add(m);var language=builder.Build();
  using var missing=language.Parse("class C{object x="+expression+";}",CSharp3Composition.Start);Assert.False(missing.Success&&!missing.HasErrors);
  using var baseline=language.Parse("class C{object x=new C();}",CSharp3Composition.Start);Assert.True(baseline.Success&&!baseline.HasErrors);
 } [Fact]public void Construction_children_lists_and_UTF16_spans_are_authored()
 {
  const string source="/*𐐀*/class C{object a=new C(){X=1,Y=2,};object b=new C{{1,2},{3,4}};object c=new {X=1,a.Name};object d=new [,]{{1,2},{3,4}};}";
  var language=P.CSharp3Composition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3Composition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int Find(string kind)=>Enumerable.Range(0,parsed.Tree.NodeCount).First(n=>language.GetKindName(parsed.Tree.Kind(n))==kind);
  string Slice(TextSpan span)=>source.Substring(span.Start,span.Length);
  var creation=SyntaxView.Cast<P.InitializedObjectExpressionExpression>(parsed.Tree,Find("InitializedObjectExpression"));
  Assert.True(creation.Arguments.HasValue);Assert.Equal("()",Slice(creation.Arguments.Value.Span));Assert.Equal("C",Slice(creation.Type.Span));
  Assert.Equal(source.IndexOf("new C()",StringComparison.Ordinal),creation.Span.Start);
  var memberGroup=SyntaxView.Cast<P.ObjectInitializerNode>(parsed.Tree,Find("ObjectInitializer")).Members;
  Assert.True(memberGroup.HasValue);var members=memberGroup.Value.Items;
  Assert.Equal(2,members.Count);Assert.Equal("X",members[0].Name.ToString());Assert.Equal("2",Slice(members[1].Value.Span));Assert.Equal(",",members.Separator(0).ToString());
  var elements=SyntaxView.Cast<P.CollectionInitializerNode>(parsed.Tree,Find("CollectionInitializer")).Items;
  Assert.Equal(2,elements.Count);Assert.Equal("{1,2}",Slice(elements[0].Span));Assert.Equal(",",elements.Separator(0).ToString());
  var braced=SyntaxView.Cast<P.BracedElementNode>(parsed.Tree,Find("BracedElement")).Items;
  Assert.Equal(2,braced.Count);Assert.Equal("1",Slice(braced[0].Span));Assert.Equal(",",braced.Separator(0).ToString());
  var anonymous=SyntaxView.Cast<P.AnonymousObjectExpressionExpression>(parsed.Tree,Find("AnonymousObjectExpression"));Assert.True(anonymous.Initializer.Members.HasValue);
  var anonymousMembers=anonymous.Initializer.Members.Value.Items;Assert.Equal(2,anonymousMembers.Count);
  Assert.Equal(",",anonymousMembers.Separator(0).ToString());Assert.Equal("{X=1,a.Name}",Slice(anonymous.Initializer.Span));
  var named=SyntaxView.Cast<P.NamedMemberNode>(parsed.Tree,Find("NamedMember"));Assert.Equal("X",named.Name.ToString());Assert.Equal("1",Slice(named.Value.Span));
  Assert.Equal("a.Name",Slice(SyntaxView.Cast<P.InferredMemberNode>(parsed.Tree,Find("InferredMember")).Value.Span));
  var array=SyntaxView.Cast<P.ImplicitArrayExpressionExpression>(parsed.Tree,Find("ImplicitArrayExpression"));Assert.Equal(1,array.Rank.Commas.Count);Assert.Equal("[,]",Slice(array.Rank.Span));
  Assert.True(array.Initializer.Items.HasValue);var arrayItems=new SeparatedList<SyntaxNode>(parsed.Tree,array.Initializer.Items.Value.Child(0).Index);
  Assert.Equal(2,arrayItems.Count);Assert.Equal("{3,4}",Slice(arrayItems[1].Span));Assert.Equal(",",arrayItems.Separator(0).ToString());
 }

}
