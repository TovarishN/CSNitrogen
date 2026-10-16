using CSNitrogen.Preprocessing;
namespace CSNitrogen.Tests.Preprocessing;
public class ResourceAndBoundaryTests
{
    [Fact] public void Excessive_directive_expression_depth_reports_error_instead_of_recursing_unbounded()
    {
        var source="#if "+new string('!',1024)+"true\nclass C {}\n#endif";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));Assert.True(result.HasErrors);
        Assert.Contains(result.Issues,i=>i.Message.Contains("nesting"));
    }
    [Theory][InlineData("\r")][InlineData("\n")][InlineData("\r\n")][InlineData("\u0085")][InlineData("\u2028")][InlineData("\u2029")]
    public void Masking_retains_every_newline_and_supplementary_code_unit_position(string newline)
    {
        string source="\uFEFF#if false"+newline+"🧪 broken"+newline+"#endif"+newline+"class C {}";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));Assert.False(result.HasErrors);Assert.Equal(source.Length,result.MaskedText.Length);
        for(int i=0;i<source.Length;i++)if(source[i] is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029')Assert.Equal(source[i],result.MaskedText[i]);
        Assert.Equal(source.IndexOf("class",StringComparison.Ordinal),result.MaskedText.IndexOf("class",StringComparison.Ordinal));
    }
    [Fact] public void Escaped_and_formatting_symbol_spellings_share_identity()
    {
        const string source="#define \\u0058\n#if X\nclass C {}\n#endif";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),[]));Assert.False(result.HasErrors);Assert.Contains("class C",result.MaskedText);
        result=ConditionalPreprocessor.Process(new("#if XY\nclass C {}\n#endif",new(1,0),["X\u200CY"]));Assert.False(result.HasErrors);Assert.Contains("class C",result.MaskedText);
    }
}
