using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2UnsafeAdditionsTests
{
    [Theory]
    [InlineData("unsafe class C { void M() { x = (G<int>*)p; } }","PointerCastHead")]
    [InlineData("unsafe class C { void M() { x = (A::G<int>*)p; } }","PointerCastHead")]
    [InlineData("unsafe class C { void M() { x = (A::T*)p; } }","PointerCastHead")]
    [InlineData("unsafe class C { void M() { p->F<int>(); } }","GenericPointerMemberExpression")]
    [InlineData("unsafe class C { int M() { return sizeof(G<int>); } }","SizeOfExtendedExpression")]
    [InlineData("unsafe struct C { public fixed int x[5], y[10]; }","FixedBufferMember")]
    public void Unsafe_deltas_are_typed_and_preceding_version_gated(string source,string kind)
    {
        using var first=CSharpParser.Parse(new(source,"CSharp1","unsafe2.cs",[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2","unsafe2.cs",[]));Assert.True(second.Accepted,string.Join(";",second.Issues));
        Assert.Contains(Enumerable.Range(0,second.Tree.NodeCount),n=>second.Tree.Language!.GetKindName(second.Tree.Kind(n))==kind);
    }
    [Theory]
    [InlineData("unsafe class C { fixed int x[5]; }")]
    [InlineData("unsafe struct C { static fixed int x[5]; }")]
    [InlineData("unsafe struct C { fixed int x[]; }")]
    [InlineData("unsafe class C { void M() { p->F<>(x); } }")]
    public void Fixed_buffer_contexts_and_malformed_unsafe_generics_are_rejected(string source)
    {using var result=CSharpParser.Parse(new(source,"CSharp2","bad-unsafe.cs",[]));Assert.False(result.Accepted);}
}
