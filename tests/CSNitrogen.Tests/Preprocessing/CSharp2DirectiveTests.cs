using System.Numerics;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Preprocessing;
public class CSharp2DirectiveTests
{
    [Theory][InlineData("vendor arbitrary \"unterminated")][InlineData("warning disable 1234")][InlineData("")]
    public void Pragma_payload_is_preserved_without_turning_unknown_compiler_information_into_an_error(string payload)
    {
        string source="#pragma "+payload+"\nclass C {}";
        using var first=CSharpParser.Parse(new(source,"CSharp1","pragma.cs",[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2","pragma.cs",[]));Assert.True(second.Accepted,string.Join(";",second.Issues));
        var directive=Assert.Single(second.Directives);Assert.Equal("pragma",directive.Kind);Assert.Equal(payload,directive.Argument);
        Assert.DoesNotContain(second.Issues,i=>i.Severity==DiagnosticSeverity.Error);
    }
    [Fact] public void Pragma_requires_whitespace_before_nonempty_payload()
    {using var result=CSharpParser.Parse(new("#pragma!\nclass C {}","CSharp2","pragma.cs",[]));Assert.False(result.Accepted);Assert.Contains(result.Issues,i=>i.Code=="CSN1001");}
    [Theory][InlineData("vendor")][InlineData("if")]
    public void Fourth_edition_unknown_line_indicators_warn_and_preserve_the_previous_mapping(string indicator)
    {
        using var result=CSharpParser.Parse(new("#line 10\n#line "+indicator+"\nclass C {}","CSharp2","line.cs",[]));
        Assert.True(result.Accepted);Assert.Contains(result.Issues,i=>i.Severity==DiagnosticSeverity.Warning);
        Assert.Single(result.ExactLineMappings);
    }
    [Theory][InlineData("0")][InlineData("99999999999999999999999999999999999")]
    public void Fourth_edition_decimal_digits_have_exact_unbounded_mapping(string digits)
    {
        string source="#line "+digits+"\nclass C {}";
        using var result=CSharpParser.Parse(new(source,"CSharp2","line.cs",[]));Assert.True(result.Accepted);
        Assert.Equal(BigInteger.Parse(digits)-1,result.MapLocationExact(source.IndexOf("class",StringComparison.Ordinal)).LogicalLine);
    }
    [Fact] public void Fourth_edition_line_default_uses_identifier_equality()
    {
        using var result=CSharpParser.Parse(new("#line 10\n#line de\\u0066ault\nclass C {}","CSharp2","line.cs",[]));Assert.True(result.Accepted);
        Assert.Equal(2,result.MapLocationExact(result.OriginalText.IndexOf("class",StringComparison.Ordinal)).LogicalLine);
    }
}
