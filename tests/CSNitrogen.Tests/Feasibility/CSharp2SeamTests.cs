using Nitrogen;
using CSNitrogen.Tests.Probes;
namespace CSNitrogen.Tests.Feasibility;
public class CSharp2SeamTests
{
    [Theory]
    [InlineData("class Box<T> {}")]
    [InlineData("class Box<T,U> {}")]
    public void Optional_typed_header_seam_is_enabled_only_by_extension_membership(string source)
    {
        var baseline=CSharp2SeamBaseComposition.CreateBuilder().Build();
        using var absent=baseline.Parse(source,CSharp2SeamBaseComposition.Start);
        Assert.False(absent.Success && !absent.HasErrors);
        var extended=CSharp2SeamGenericComposition.CreateBuilder().Build();
        using var present=extended.Parse(source,CSharp2SeamGenericComposition.Start);
        Assert.True(present.Success && !present.HasErrors);
        int node=Assert.Single(Enumerable.Range(0,present.Tree.NodeCount),n=>extended.GetKindName(present.Tree.Kind(n))=="GenericParameters");
        var parameters=SyntaxView.Cast<GenericParametersTypeParameters>(present.Tree,node);
        Assert.Equal(source.Contains(',')?2:1,parameters.Items.Count);
        Assert.Equal("T",parameters.Items[0].ToString());
    }
    [Fact]
    public void Baseline_header_preserves_name_field_and_absent_optional_parameters()
    {
        var baseline=CSharp2SeamBaseComposition.CreateBuilder().Build();
        using var parsed=baseline.Parse("class Plain {}",CSharp2SeamBaseComposition.Start);
        Assert.True(parsed.Success && !parsed.HasErrors);
        var header=SyntaxView.Cast<HeaderNode>(parsed.Tree,parsed.Tree.Root);
        Assert.Equal("Plain",header.Name.ToString());
        Assert.False(header.Parameters.HasValue);
    }
    [Theory]
    [InlineData("class Box<> {}")][InlineData("class Box<T,> {}")][InlineData("class Box<T {}")]
    public void Optional_fallback_does_not_accept_malformed_parameter_lists(string source)
    {
        var extended=CSharp2SeamGenericComposition.CreateBuilder().Build();
        using var parsed=extended.Parse(source,CSharp2SeamGenericComposition.Start);
        Assert.False(parsed.Success && !parsed.HasErrors);
    }
    [Theory]
    [InlineData("F<T>(x)","SeamCall")][InlineData("F<G<int>>(x)","SeamCall")]
    [InlineData("F<T> == x","SeamEqual")][InlineData("(F<T>)","Paren")]
    [InlineData("F<T> + x","SeamGreater")][InlineData("F<G<int>(x)","Less")][InlineData("a < b","Less")][InlineData("a >> b","Shift")]
    public void Generic_followers_and_nested_closers_preserve_operator_classification(string source,string expected)
    {
        var language=CSharp2AmbiguityComposition.CreateBuilder().Build();
        using var parsed=language.Parse(source,CSharp2AmbiguityComposition.Start);
        Assert.True(parsed.Success && !parsed.HasErrors);
        Assert.Equal(expected,language.GetKindName(parsed.Tree.Kind(parsed.Tree.Child(parsed.Tree.Root,0))));
        bool generic=Enumerable.Range(0,parsed.Tree.NodeCount).Any(n=>language.GetKindName(parsed.Tree.Kind(n))=="SeamGenericName");
        Assert.Equal(source.StartsWith("F<") && !source.Contains(" + ") || source.StartsWith("(F<"),generic);
    }
    [Theory][InlineData("F<>()")][InlineData("F<T,>(x)")][InlineData("F<T>")]
    public void Malformed_or_disallowed_generic_followers_do_not_gain_an_accidental_parse(string source)
    {
        var language=CSharp2AmbiguityComposition.CreateBuilder().Build();
        using var parsed=language.Parse(source,CSharp2AmbiguityComposition.Start);
        Assert.False(parsed.Success && !parsed.HasErrors);
    }
}
