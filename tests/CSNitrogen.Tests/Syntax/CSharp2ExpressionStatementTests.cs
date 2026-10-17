using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2ExpressionStatementTests
{
    [Theory]
    [InlineData("class C { int?[] a; int M(int? x) { return x ?? 0; } }","NullCoalescingExpression")]
    [InlineData("class C { object M() { return (int?)-x; } }","NullableCast")]
    [InlineData("class C { object M() { return (List<int>?)x; } }","NullableCast")]
    [InlineData("class C { object d = delegate(int x) { return x; }; }","AnonymousMethodExpression")]
    [InlineData("class C { object d = delegate(ref int x, out int y) { y = x; }; }","AnonymousMethodExpression")]
    [InlineData("class C { object d = delegate { return x; }; }","AnonymousMethodExpression")]
    [InlineData("class C { object d = delegate() { return x; }; }","AnonymousMethodExpression")]
    [InlineData("class C { object M() { return (T)delegate { }; } }","AnonymousMethodExpression")]
    [InlineData("class C { object M() { return (T)default(T); } }","DefaultValueExpression")]
    [InlineData("class C { object M() { yield return 1; yield break; } }","YieldReturnStatement")]
    [InlineData("class C { object M() { return default(List<int>); } }","DefaultValueExpression")]
    public void Added_expression_and_statement_syntax_is_version_gated(string source,string kind)
    {
        using var first=CSharpParser.Parse(new(source,"CSharp1","slice.cs",[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2","slice.cs",[]));
        Assert.True(second.Accepted,string.Join(";",second.Issues));Assert.True(second.ProfileComplete);
        Assert.Contains(Enumerable.Range(0,second.Tree.NodeCount),n=>second.Tree.Language!.GetKindName(second.Tree.Kind(n))==kind);
    }
    [Theory]
    [InlineData("class C { int?? x; }")][InlineData("class C { int*? x; }")]
    [InlineData("class C { int[]? x; }")][InlineData("class C { string? x; }")]
    [InlineData("class C { object M() { return x ??; } }")]
    [InlineData("class C { object d = delegate(params int[] x) {}; }")]
    [InlineData("class C { object d = delegate([A] int x) {}; }")]
    [InlineData("class C { object M() { yield return; } }")]
    [InlineData("class C { object M() { return default; } }")]
    public void Malformed_and_later_forms_are_rejected(string source)
    {using var result=CSharpParser.Parse(new(source,"CSharp2","bad-slice.cs",[]));Assert.False(result.Accepted);}
    [Fact]
    public void Contextual_yield_remains_an_identifier()
    {using var result=CSharpParser.Parse(new("class C { int yield; void M() { yield = 1; } }","CSharp2","identifier.cs",[]));Assert.True(result.Accepted);}
}
