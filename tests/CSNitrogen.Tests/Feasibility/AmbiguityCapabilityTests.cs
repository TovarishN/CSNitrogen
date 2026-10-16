using Nitrogen;
using CSNitrogen.Tests.Probes;
namespace CSNitrogen.Tests.Feasibility;
public class AmbiguityCapabilityTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static (Language Language,Rule Start) Base()=>(ExpressionProbeComposition.CreateBuilder().Build(),ExpressionProbeComposition.Start);
    [Theory][InlineData("a + b * c","Add")][InlineData("(x)","Paren")][InlineData("(T)x","Cast")][InlineData("a < b","Less")][InlineData("a >> b","Shift")][InlineData("from + select","Add")]
    public void Base_expressions_have_declared_shape(string source,string rootKind)
    {
        var probe=Base();using var parsed=probe.Language.Parse(source,probe.Start);
        Assert.True(parsed.Success); Assert.False(parsed.HasErrors);
        int value=parsed.Tree.Child(parsed.Tree.Root,0);
        Assert.Equal(rootKind,probe.Language.GetKindName(parsed.Tree.Kind(value)));
        if(source=="a + b * c") Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>probe.Language.GetKindName(parsed.Tree.Kind(n))=="Multiply");
    }
    [Theory][InlineData("F<T>(x)")][InlineData("F<G<int>>(x)")]
    public void Generic_calls_require_the_generic_module(string source)
    {
        var b=Base();using var absent=b.Language.Parse(source,b.Start);Assert.False(absent.Success && !absent.HasErrors);
        var language=GenericProbeComposition.CreateBuilder().Build();using var present=language.Parse(source,GenericProbeComposition.Start);
        Assert.True(present.Success); Assert.False(present.HasErrors);
        Assert.Contains(language.Modules,m=>m.Name=="Probe.Generic");
        Assert.DoesNotContain(b.Language.Modules,m=>m.Name=="Probe.Generic");
    }
    [Fact] public void Lambda_requires_its_module_and_contextual_names_stay_identifiers()
    {
        var b=Base();using var absent=b.Language.Parse("x => x",b.Start);Assert.False(absent.Success);
        var language=LambdaProbeComposition.CreateBuilder().Build();using var present=language.Parse("x => x",LambdaProbeComposition.Start);Assert.True(present.Success);Assert.False(present.HasErrors);
        using var ordinary=language.Parse("from + select",LambdaProbeComposition.Start);Assert.True(ordinary.Success);Assert.False(ordinary.HasErrors);
        Assert.DoesNotContain(b.Language.Modules,m=>m.Name=="Probe.Lambda");
    }
    [Theory][InlineData(16)][InlineData(64)][InlineData(128)]
    [Trait("Category","Depth")]
    public void Bounded_nesting_and_malformed_tails(int depth)
    {
        long before=GC.GetAllocatedBytesForCurrentThread(); var clock=System.Diagnostics.Stopwatch.StartNew();
        var b=Base();string source=new string('(',depth)+"x"+new string(')',depth);
        using var valid=b.Language.Parse(source,b.Start);Assert.True(valid.Success);Assert.False(valid.HasErrors);
        using var invalid=b.Language.Parse(source+" +",b.Start);Assert.False(invalid.Success);
        output.WriteLine($"depth={depth}; elapsed_ms={clock.Elapsed.TotalMilliseconds:F3}; allocated_bytes={GC.GetAllocatedBytesForCurrentThread()-before}");
    }
}
