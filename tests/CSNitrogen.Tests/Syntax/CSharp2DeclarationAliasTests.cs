using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2DeclarationAliasTests
{
    [Theory]
    [InlineData("partial class C<T> { T x; }","PartialModifier")]
    [InlineData("partial struct S { int x; }","PartialModifier")]
    [InlineData("partial interface I<T> { T M(); }","PartialModifier")]
    [InlineData("static class C { static int x; }","ClassDeclaration")]
    [InlineData("class C { int P { get { return 1; } private set {} } }","AccessorAccessibility")]
    [InlineData("extern alias A; using X = A::N.T; class C { A::N.T x; }","AliasQualifiedTypeName")]
    [InlineData("namespace N { extern alias A; using X=A::N.T; class C {} }","ExternalAliasList")]
    [InlineData("class C { global::N.List<int>? x; }","NullableTypeName")]
    [InlineData("class C { object M() { return A::T.F<int>(x); } }","AliasNameExpression")]
    [InlineData("class C { object M() { return (A::N.T)x; } }","AliasCast")]
    [InlineData("class C { object M() { return typeof(A::N.G<>); } }","UnboundTypeOfExpression")]
    public void Declaration_and_alias_forms_are_added_only_in_CSharp2(string source,string kind)
    {
        using var first=CSharpParser.Parse(new(source,"CSharp1","alias.cs",[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2","alias.cs",[]));
        Assert.True(second.Accepted,string.Join(";",second.Issues));Assert.True(second.ProfileComplete);
        Assert.Contains(Enumerable.Range(0,second.Tree.NodeCount),n=>second.Tree.Language!.GetKindName(second.Tree.Kind(n))==kind);
    }
    [Theory]
    [InlineData("partial enum E { A }")][InlineData("class C { partial void M(); }")]
    [InlineData("partial public class C {}")][InlineData("partial partial class C {}")]
    [InlineData("class C { int P { private get { return 1; } private set {} } }")]
    [InlineData("class C { int P { public get { return 1; } set {} } }")]
    [InlineData("using N; extern alias A; class C {}")]
    [InlineData("class C {} extern alias A;")]
    [InlineData("extern alias; class C {}")][InlineData("class C { A:::T x; }")]
    public void Misplaced_or_later_declarations_are_rejected(string source)
    {using var result=CSharpParser.Parse(new(source,"CSharp2","bad-alias.cs",[]));Assert.False(result.Accepted);}
    [Fact]
    public void Partial_alias_and_where_remain_ordinary_identifiers()
    {using var result=CSharpParser.Parse(new("class partial { int alias; int where; }","CSharp2","names.cs",[]));Assert.True(result.Accepted);}
}
