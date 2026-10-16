using Nitrogen;
using CSNitrogen.Tests.Probes;
namespace CSNitrogen.Tests.Feasibility;
public unsafe class LexicalCapabilityTests
{
    private static int NoTrivia(ReadOnlySpan<char> text,int position)=>position;
    internal static Language Language()=>LexicalProbeComposition.CreateBuilder().WithTrivia(&NoTrivia).Build();
    [Theory]
    [InlineData("\" a \\\" b \"",true)]
    [InlineData("@\" a \"\" b \"",true)]
    [InlineData("\"unterminated",false)]
    [InlineData("$\" a {1 + 2} b \"",true)]
    [InlineData("$\"outer {$\"inner {1}\"}\"",true)]
    [InlineData("$\"x {1 + }\"",false)]
    public void Strings_and_nested_interpolation_parse(string source,bool expected)
    {
        using var parsed=Language().Parse(source,LexicalProbeComposition.Start);
        Assert.True(expected == (parsed.Success && !parsed.HasErrors), source+" :: "+string.Join("; ",parsed.Diagnostics.ToArray().Select(d=>$"{d.Span}: {parsed.FormatMessage(d)}")));
        if(expected && source.StartsWith('$')) Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))=="Hole");
    }
    [Fact] public void Text_whitespace_is_preserved()
    {
        const string source="$\"  a {1}  b \"";
        using var parsed=Language().Parse(source,LexicalProbeComposition.Start); Assert.True(parsed.Success,string.Join("; ",parsed.Diagnostics.ToArray().Select(d=>$"{d.Span}: {parsed.FormatMessage(d)}")));
        var texts=Enumerable.Range(0,parsed.Tree.NodeCount).Where(n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))=="InterpolatedText")
            .Select(n=>source.Substring(parsed.Tree.Span(n).Start,parsed.Tree.Span(n).Length)).ToArray();
        Assert.Equal(new[]{"  a ","  b "},texts);
    }
    [Theory][InlineData(3)][InlineData(4)][InlineData(8)]
    public void Enumerated_raw_delimiters_preserve_shorter_quotes(int count)
    {
        string delimiter=new('"',count),source=delimiter+" a "+new string('"',count-1)+" b "+delimiter;
        using var parsed=Language().Parse(source,LexicalProbeComposition.Start); Assert.True(parsed.Success,string.Join("; ",parsed.Diagnostics.ToArray().Select(d=>$"{d.Span}: {parsed.FormatMessage(d)}"))); Assert.False(parsed.HasErrors);
        using var broken=Language().Parse(delimiter+"a"+new string('"',count-1),LexicalProbeComposition.Start); Assert.False(broken.Success);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)]
    public void Enumerated_raw_interpolation_exposes_expression(int dollars)
    {
        string source=new string('$',dollars)+"\"\"\" a "+new string('{',dollars)+"1 + 2"+new string('}',dollars)+" b \"\"\"";
        using var parsed=Language().Parse(source,LexicalProbeComposition.Start); Assert.True(parsed.Success,string.Join("; ",parsed.Diagnostics.ToArray().Select(d=>$"{d.Span}: {parsed.FormatMessage(d)}"))); Assert.False(parsed.HasErrors);
        Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))=="LexAdd");
    }
    [Fact] public void Unenumerated_raw_delimiter_is_explicitly_outside_probe_coverage()
    {
        using var parsed=Language().Parse(new string('"',5)+"a"+new string('"',5),LexicalProbeComposition.Start);
        Assert.False(parsed.Success);
    }
}
