using System.Numerics;
using CSNitrogen.Parsing;
using CSNitrogen.Preprocessing;
namespace CSNitrogen.Tests.Preprocessing;
public class ExactLineMappingTests
{
    [Theory]
    [InlineData("0",-1)]
    [InlineData("00042",41)]
    [InlineData("16707566",16707565)]
    [InlineData("2147483648",int.MaxValue)]
    public void Historical_numeric_lines_are_not_limited_by_PDB_or_positive_Int32(string spelling,int expected)
    {
        string source="#line "+spelling+" \"mapped.cs\"\nclass C{}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        Assert.True(doc.Accepted,string.Join(";",doc.Issues));
        var location=doc.MapLocation(source.IndexOf("class",StringComparison.Ordinal));
        Assert.Equal(expected,location.LogicalLine);
        Assert.Equal("mapped.cs",location.File);
        Assert.Equal(1,location.PhysicalLine);
    }
    [Fact]
    public void Arbitrarily_large_historical_lines_are_accepted_without_ignored_mapping_warning()
    {
        const string source="#line 999999999999999999999999999999999999999999\nclass C{}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        Assert.True(doc.Accepted,string.Join(";",doc.Issues));
        Assert.DoesNotContain(doc.Issues,i=>i.Code=="CSN1208");
        Assert.Throws<OverflowException>(()=>doc.MapLocation(source.IndexOf("class",StringComparison.Ordinal)));
    }
    [Fact]
    public void Exact_query_preserves_large_values_spelling_spans_filename_and_reset()
    {
        const string digits="000999999999999999999999999999999999999999999";
        string source="/* 🧪 */\n#line "+digits+" \"mapped.cs\"\nclass A{}\n#line 0\nclass B{}\n#line default\nclass C{}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        Assert.True(doc.Accepted);
        var first=doc.ExactLineMappings[0];
        Assert.Equal(digits,first.NumberSpelling);
        Assert.Equal("#line "+digits+" \"mapped.cs\"",source.Substring(first.DirectiveSpan.Start,first.DirectiveSpan.Length));
        var a=doc.MapLocationExact(source.IndexOf("class A",StringComparison.Ordinal));
        Assert.Equal(BigInteger.Parse(digits),a.LineNumber);
        Assert.Equal(BigInteger.Parse(digits)-1,a.LogicalLine);
        Assert.Equal(2,a.PhysicalLine);Assert.Equal(0,a.Character);
        Assert.Equal("mapped.cs",a.File);
        Assert.False(doc.TryMapLocation(source.IndexOf("class A",StringComparison.Ordinal),out var overflow));
        Assert.Null(overflow);
        Assert.Throws<OverflowException>(()=>doc.LineMappings);
        var b=doc.MapLocationExact(source.IndexOf("class B",StringComparison.Ordinal));
        Assert.Equal(BigInteger.Zero,b.LineNumber);Assert.Equal(-BigInteger.One,b.LogicalLine);
        Assert.Equal("mapped.cs",b.File);
        Assert.True(doc.TryMapLocation(source.IndexOf("class B",StringComparison.Ordinal),out var small));
        Assert.Equal(-1,small!.LogicalLine);
        var c=doc.MapLocationExact(source.IndexOf("class C",StringComparison.Ordinal));
        Assert.Equal(6,c.PhysicalLine);Assert.Equal(new BigInteger(6),c.LogicalLine);Assert.Null(c.File);
        Assert.Null(doc.ExactLineMappings[2].NumberSpelling);
        Assert.Throws<ArgumentOutOfRangeException>(()=>doc.MapLocationExact(-1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>doc.TryMapLocation(source.Length+1,out _));
    }
    [Fact]
    public void Exact_increment_crosses_Int32_boundary_without_wrapping()
    {
        const string source="#line 2147483648\nclass A{}\nclass B{}";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));
        Assert.Equal(int.MaxValue,result.MapLocation(source.IndexOf("class A",StringComparison.Ordinal)).LogicalLine);
        Assert.Equal(new BigInteger(int.MaxValue)+1,result.MapLocationExact(source.IndexOf("class B",StringComparison.Ordinal)).LogicalLine);
        Assert.Throws<OverflowException>(()=>result.MapLocation(source.Length));
    }
    [Fact]
    public void Inactive_large_directives_validate_but_do_not_change_mapping()
    {
        const string source="#line 42 \"a.cs\"\n#if false\n#line 9999999999999999999999999999999999999999 \"inactive.cs\"\n#endif\nclass C{}";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));
        Assert.False(result.HasErrors);Assert.Single(result.ExactLineMappings);
        var mapped=result.MapLocationExact(source.IndexOf("class",StringComparison.Ordinal));
        Assert.Equal(new BigInteger(44),mapped.LogicalLine);Assert.Equal("a.cs",mapped.File);
    }
    [Fact]
    public void Exact_mapping_preserves_CRLF_and_supplementary_UTF16_column()
    {
        const string source="#line 999999999999999999999999999 \"x.cs\"\r\n/*🧪*/ class C{}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","physical.cs",[]));
        Assert.True(doc.Accepted);
        var location=doc.MapLocationExact(source.IndexOf("C{}",StringComparison.Ordinal));
        Assert.Equal(1,location.PhysicalLine);Assert.Equal(13,location.Character);
        Assert.Equal(BigInteger.Parse("999999999999999999999999999"),location.LineNumber);
    }
}
