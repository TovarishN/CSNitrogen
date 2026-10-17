using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Preprocessing;
public class PipelineTests
{
    [Theory][InlineData(true,"A")][InlineData(false,"B")]
    public void Public_parser_uses_masked_source_but_retains_authored_spans(bool defined,string name)
    {
        const string source="#if X\nclass A {}\n#else\nclass B {}\n#endif";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","original.cs",defined?["X"]:[]));
        Assert.True(doc.Accepted);Assert.Equal(source,doc.OriginalText);Assert.Equal(source.Length,doc.MaskedText.Length);
        Assert.Equal(name,doc.Classes[0].Name.ToString());Assert.Equal(source.IndexOf(name,StringComparison.Ordinal),doc.Tree.Span(doc.Classes[0].Name.Index).Start);
        Assert.Equal(3,doc.Directives.Count);Assert.NotEmpty(doc.InactiveSpans);Assert.True(doc.ProfileComplete);
    }
    [Theory][InlineData("#error bad\nclass C {}",false)][InlineData("#warning message\nclass C {}",true)]
    [InlineData("#if F()\nclass C {}\n#endif",false)][InlineData("#if false\n\"broken\n#endif\nclass C {}",true)]
    public void Preprocessing_errors_prevent_acceptance(string source,bool accepted)
    { using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));Assert.Equal(accepted,doc.Accepted); }
    [Fact] public void Public_mapping_keeps_physical_and_logical_locations()
    {
        const string source="#line 42 \"mapped.cs\"\nclass C {}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","original.cs",[]));Assert.True(doc.Accepted);
        var location=doc.MapLocation(source.IndexOf('C'));Assert.Equal(1,location.PhysicalLine);Assert.Equal(41,location.LogicalLine);Assert.Equal("mapped.cs",location.File);
    }
    [Theory][InlineData("@class")][InlineData("Δelta")][InlineData("\\u0043")]
    public void Production_uses_lexical_identifier_categories(string name)
    {using var doc=CSharpParser.Parse(new("class "+name+" {}","CSharp1","test.cs",[]));Assert.True(doc.Accepted);Assert.Equal(name,doc.Classes[0].Name.ToString());}
}
