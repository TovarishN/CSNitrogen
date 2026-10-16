using CSNitrogen.Preprocessing;
namespace CSNitrogen.Tests.Preprocessing;
public class ConditionalPreprocessorTests
{
    internal static PreprocessResult Process(string source,params string[] symbols)=>ConditionalPreprocessor.Process(new(source,new(1,0),symbols));
    [Theory][InlineData(true,"A")][InlineData(false,"B")]
    public void Conditional_branches_preserve_source_coordinates(bool defined,string name)
    {
        const string source="#if X\r\nclass A {}\r\n#else\r\nclass B {}\r\n#endif";
        var result=defined?Process(source,"X"):Process(source);
        Assert.False(result.HasErrors);Assert.Contains("class "+name,result.MaskedText);
        Assert.DoesNotContain("class "+(name=="A"?"B":"A"),result.MaskedText);
        Assert.Equal(source.Length,result.MaskedText.Length);Assert.Equal(source,result.OriginalText);
        for(int i=0;i<source.Length;i++)if(source[i] is '\r' or '\n')Assert.Equal(source[i],result.MaskedText[i]);
        Assert.Equal(3,result.Directives.Count);Assert.NotEmpty(result.InactiveSpans);
    }
    [Theory][InlineData("true",true)][InlineData("false",false)][InlineData("!X",false)]
    [InlineData("X && !Y",true)][InlineData("Y || X && true",true)][InlineData("(Y || X) == true",true)]
    [InlineData("X != Y",true)][InlineData("false == false == true",true)]
    public void Restricted_boolean_expressions(string condition,bool included)
    {
        var result=Process("#if "+condition+"\nclass A {}\n#endif","X");
        Assert.False(result.HasErrors);Assert.Equal(included,result.MaskedText.Contains("class A"));
    }
    [Theory][InlineData("F()")][InlineData("a.b")][InlineData("1+2")][InlineData("X &&")][InlineData("")]
    public void Non_directive_expression_syntax_is_rejected(string condition)
    { Assert.True(Process("#if "+condition+"\nclass A {}\n#endif").HasErrors); }
    [Fact] public void Nested_elif_and_definitions_are_applied_without_mutating_caller_symbols()
    {
        string[] symbols=["X"];
        const string source="#define Y\n#undef X\n#if X\nclass A {}\n#elif Y\n#if true\nclass B {}\n#endif\n#else\nclass C {}\n#endif";
        var result=ConditionalPreprocessor.Process(new(source,new(1,0),symbols));
        Assert.False(result.HasErrors);Assert.Contains("class B",result.MaskedText);Assert.DoesNotContain("class A",result.MaskedText);
        Assert.Equal(new[]{"X"},symbols);
    }
    [Fact] public void Inactive_broken_strings_do_not_hide_endif()
    { var result=Process("#if false\n\"unterminated\n#endif\nclass C {}");Assert.False(result.HasErrors);Assert.Contains("class C",result.MaskedText); }
    [Fact] public void Active_verbatim_content_is_not_a_directive()
    { var result=Process("@\"text\n#if X\n#endif\"\nclass C {}");Assert.False(result.HasErrors);Assert.Empty(result.Directives); }
    [Theory][InlineData("#else")][InlineData("#endif")][InlineData("#if true")]
    [InlineData("#if true\n#else\n#else\n#endif")][InlineData("#if true\n#else\n#elif true\n#endif")]
    [InlineData("class C {}\n#define X")][InlineData("#define true")]
    public void Structural_and_definition_errors_are_reported(string source)=>Assert.True(Process(source).HasErrors);
}
