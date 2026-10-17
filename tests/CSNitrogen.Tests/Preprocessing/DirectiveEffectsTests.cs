using CSNitrogen.Preprocessing;
using Nitrogen;
namespace CSNitrogen.Tests.Preprocessing;
public class DirectiveEffectsTests
{
    private static PreprocessResult Process(string text,Version? version=null)=>ConditionalPreprocessor.Process(new(text,version??new(1,0),[]));
    [Theory][InlineData("#region group\nclass C {}\n#endregion",false)]
    [InlineData("#region group",true)][InlineData("#endregion",true)]
    [InlineData("#error failed",true)][InlineData("#warning message",false)]
    [InlineData("#if false\n#error ignored\n#endif\nclass C {}",false)]
    [InlineData("#unknown",true)][InlineData("#line 0",false)][InlineData("#line 16707566",false)]
    public void Directive_effects_and_invalid_forms(string source,bool error)=>Assert.Equal(error,Process(source).HasErrors);
    [Fact] public void Warnings_preserve_authored_message_and_severity()
    { var issue=Assert.Single(Process("#warning text // content").Issues);Assert.Equal(DiagnosticSeverity.Warning,issue.Severity);Assert.Contains("text // content",issue.Message); }
    [Fact] public void Line_mapping_retains_physical_coordinates_and_resets()
    {
        const string source="#line 100 \"generated.cs\"\nclass A {}\n#line hidden\nclass B {}\n#line default\nclass C {}";
        var result=Process(source,new(14,0));Assert.False(result.HasErrors);
        var a=result.MapLocation(source.IndexOf("class A",StringComparison.Ordinal));
        Assert.Equal(1,a.PhysicalLine);Assert.Equal(99,a.LogicalLine);Assert.Equal("generated.cs",a.File);Assert.False(a.Hidden);
        Assert.True(result.MapLocation(source.IndexOf("class B",StringComparison.Ordinal)).Hidden);
        var c=result.MapLocation(source.IndexOf("class C",StringComparison.Ordinal));Assert.Equal(5,c.LogicalLine);Assert.Null(c.File);Assert.False(c.Hidden);
    }
    [Theory][InlineData("#nullable enable")][InlineData("#pragma warning disable 0168")][InlineData("#:property value")]
    public void Later_directives_are_unavailable_under_CSharp1(string source)
    { Assert.Contains(Process(source).Issues,i=>i.Category==CSNitrogen.Parsing.IssueCategory.UnavailableFeature); }
    [Fact] public void Unimplemented_later_directives_are_not_silently_admitted()
    { Assert.Contains(Process("#nullable enable",new(8,0)).Issues,i=>i.Category==CSNitrogen.Parsing.IssueCategory.UnsupportedCoverage); }
    [Theory][InlineData("#warning123")][InlineData("#line100")]
    public void Directive_keywords_require_a_token_boundary(string source)
    { Assert.True(Process(source).HasErrors); }
}
