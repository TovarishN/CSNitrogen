using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Parsing;
public class ParseDocumentTests
{
    [Fact] public void Supported_input_is_accepted_under_the_reviewed_historical_profile()
    {
        using var doc = CSharpParser.Parse(new("class C {}", "CSharp1", "test.cs", []));
        Assert.True(doc.Accepted); Assert.True(doc.ProfileComplete); Assert.Equal("class C {}", doc.OriginalText);
        Assert.Equal("test.cs", doc.DocumentId); Assert.Equal("C", doc.Classes[0].Name.ToString());
    }
    [Fact] public void Tree_access_cannot_outlive_owner()
    {
        var doc = CSharpParser.Parse(new("class C {}", "CSharp1", "test.cs", [])); var tree = doc.Tree;
        doc.Dispose(); Assert.Throws<ObjectDisposedException>(() => tree.Kind(tree.Root)); doc.Dispose();
    }
    [Theory][InlineData("class C {")][InlineData("class C {} @")]
    public void Recovered_input_is_not_accepted(string source)
    {
        using var doc = CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));
        Assert.False(doc.Accepted); Assert.Contains(doc.Issues, i => i.Severity == Nitrogen.DiagnosticSeverity.Error);
        Assert.All(doc.Issues, i => Assert.InRange(i.Span.Start,0,source.Length));
    }
    [Fact] public void Unknown_profile_and_null_source_fail_before_parsing()
    {
        Assert.Throws<ArgumentException>(() => CSharpParser.Parse(new("", "unknown", "test.cs", [])));
        Assert.Throws<ArgumentNullException>(() => CSharpParser.Parse(new(null!,"CSharp1","test.cs",[])));
    }
    [Theory][InlineData("#if X\nclass C {}\n#endif",false)][InlineData("class C {}",true)]
    public void Supported_preprocessing_requests_are_accepted(string source,bool symbols)
    {
        using var doc = CSharpParser.Parse(new(source,"CSharp1","test.cs",symbols ? ["X"] : []));
        Assert.True(doc.Accepted); Assert.DoesNotContain(doc.Issues, i => i.Category == IssueCategory.UnsupportedCoverage);
    }
    [Theory][InlineData("// # comment\nclass C {}")][InlineData("/* #if X\n#endif */ class C {}")]
    public void Hash_in_supported_comments_is_not_a_directive(string source)
    { using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[])); Assert.True(doc.Accepted); }
    [Theory][InlineData("class int {}")][InlineData("class public {}")][InlineData("class return {}")]
    public void Reserved_keywords_cannot_be_class_names(string source)
    { using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[])); Assert.False(doc.Accepted); }
    [Fact] public void Utf16_offsets_include_unicode_comment_text()
    {
        const string source="/* 🧪 */ class C {}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[])); Assert.True(doc.Accepted);
        Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),doc.Tree.Span(doc.Classes[0].Name.Index).Start);
    }
    [Theory][InlineData("class C {} /* unterminated")][InlineData("/* unterminated")]
    public void Unterminated_comments_are_errors(string source)
    { using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[])); Assert.False(doc.Accepted); }
    [Theory][InlineData("\r")][InlineData("\u0085")][InlineData("\u2028")][InlineData("\u2029")]
    public void All_CSharp_line_terminators_end_line_comments(string newline)
    {
        using var doc=CSharpParser.Parse(new("class C {} // text"+newline+"class D {}","CSharp14","test.cs",[]));
        Assert.True(doc.Accepted); Assert.Equal(2,doc.Classes.Count);
    }
    [Fact] public void Leading_bom_preserves_original_offsets()
    {
        const string source="\uFEFFclass C {}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[])); Assert.True(doc.Accepted);
        Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),doc.Tree.Span(doc.Classes[0].Name.Index).Start);
    }
}
