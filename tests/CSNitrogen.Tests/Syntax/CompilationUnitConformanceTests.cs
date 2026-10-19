using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CompilationUnitConformanceTests
{
    [Theory]
    [InlineData("class First{}[assembly:A]class C{}", "assembly")]
    [InlineData("namespace N{[module:A]struct S{}}", "module")]
    [InlineData("namespace N{[assembly:A]interface I{}}", "assembly")]
    [InlineData("namespace N{[assembly:A]enum E{}}", "assembly")]
    [InlineData("namespace N{[assembly:A]delegate void D();}", "assembly")]
    [InlineData("[A][assembly:B]class C{}", "assembly")]
    [InlineData("class First{}[@assembly:A]class C{}", "@assembly")]
    [InlineData("namespace N{[\\u006dodule:A]class C{}}", "\\u006dodule")]
    public void Misplaced_global_target_is_a_source_syntax_error(string source, string target)
    {
        foreach (string profile in new[] { "CSharp1", "CSharp14" })
        {
            using var document = CSharpParser.Parse(new(source, profile, "placement.cs", []));
            Assert.False(document.Accepted);
            var issue = Assert.Single(document.Issues, i => i.Code == "CSN2012");
            Assert.Equal(IssueCategory.Syntax, issue.Category);
            Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
            Assert.Equal(source.IndexOf(target, StringComparison.Ordinal), issue.Span.Start);
            Assert.Equal(target.Length, issue.Span.Length);
        }
    }

    [Theory]
    [InlineData("[assembly:A]")]
    [InlineData("using N;[module:A]")]
    [InlineData("[assembly:A]class C{}")]
    [InlineData("class C{[assembly:A]void M(){}}")]
    [InlineData("class C{[assembly:A]class Nested{}}")]
    [InlineData("namespace N{[assemblyExtra:A]class C{}}")]
    [InlineData("namespace N{[Assembly:A]class C{}}")]
    public void Legal_global_ownership_and_local_target_applicability_remain_accepted(string source)
    {
        using var document = CSharpParser.Parse(new(source, "CSharp1", "accepted.cs", []));
        Assert.True(document.Accepted, string.Join(";", document.Issues));
        Assert.DoesNotContain(document.Issues, i => i.Code == "CSN2012");
        Assert.True(document.ProfileComplete);
    }

    [Fact]
    public void Root_namespace_lists_and_projected_classes_expire_with_owner()
    {
        const string source = "using A;[assembly:A]namespace N{using B;class Nested{}}class Global{}";
        var document = CSharpParser.Parse(new(source, "CSharp1", "lifetime.cs", []));
        Assert.True(document.Accepted);
        var root = document.Root; var rootUsings = root.Usings; var members = root.Members; var attributes = root.Attributes;
        var ns = SyntaxView.Cast<NamespaceDeclarationNode>(document.Tree, members[0].Index);
        var namespaceName = ns.Name; var namespaceUsings = ns.Usings; var namespaceMembers = ns.Members;
        var projected = Assert.Single(document.Classes); var name = projected.Name;
        document.Dispose();
        Assert.Throws<ObjectDisposedException>(() => root.Usings.Count);
        Assert.Throws<ObjectDisposedException>(() => rootUsings[0].Name);
        Assert.Throws<ObjectDisposedException>(() => members[0].Span);
        Assert.Throws<ObjectDisposedException>(() => attributes[0].Span);
        Assert.Throws<ObjectDisposedException>(() => namespaceName.Parts.Count);
        Assert.Throws<ObjectDisposedException>(() => namespaceUsings[0].Name);
        Assert.Throws<ObjectDisposedException>(() => namespaceMembers[0].Span);
        Assert.Throws<ObjectDisposedException>(() => projected.Name);
        Assert.Throws<ObjectDisposedException>(() => name.ToString());
        document.Dispose();
    }

    [Theory]
    [InlineData("namespace N{class C{}")]
    [InlineData("namespace N{using A}")]
    [InlineData("namespace A.{}")]
    [InlineData("using X=;namespace N{}")]
    [InlineData("namespace N{class C{void M(){}}")]
    [InlineData("namespace N{[assembly:A]class")]
    [InlineData("namespace N{[module:")]
    [InlineData("namespace N{} trailing")]
    [InlineData("namespace N{} using A;")]
    public void Malformed_or_trailing_source_never_becomes_accepted(string source)
    {
        using var document = CSharpParser.Parse(new(source, "CSharp1", "recovery.cs", []));
        Assert.False(document.Accepted);
        Assert.Contains(document.Issues, i => i.Severity == DiagnosticSeverity.Error && i.Category == IssueCategory.Syntax);
        Assert.All(document.Issues, i => { Assert.InRange(i.Span.Start, 0, source.Length); Assert.InRange(i.Span.End, i.Span.Start, source.Length); Assert.False(string.IsNullOrWhiteSpace(i.Message)); });
    }

    [Theory]
    [InlineData("CSharp1")]
    [InlineData("CSharp14")]
    public void Placement_diagnostics_keep_physical_spans_through_masked_source(string profile)
    {
        const string source = "/*🧪*/\r\n#if false\r\nnamespace Hidden{[assembly:A]class X{}}\r\n#endif\r\n#line 300 \"mapped.cs\"\r\nnamespace N{[module:A]class C{}}";
        using var document = CSharpParser.Parse(new(source, profile, "masked-placement.cs", []));
        Assert.False(document.Accepted);
        var issue = Assert.Single(document.Issues, i => i.Code == "CSN2012");
        Assert.Equal(source.IndexOf("module", StringComparison.Ordinal), issue.Span.Start);
        Assert.Equal("module", source.Substring(issue.Span.Start, issue.Span.Length));
        var mapped = document.MapLocation(issue.Span.Start);
        Assert.Equal(299, mapped.LogicalLine); Assert.Equal("mapped.cs", mapped.File);
    }

    [Fact]
    public void Multiple_misplaced_sections_report_their_own_target_spans()
    {
        const string source = "namespace N{[assembly:A][module:B]class C{}}";
        using var document = CSharpParser.Parse(new(source, "CSharp1", "multiple.cs", []));
        Assert.False(document.Accepted);
        var issues = document.Issues.Where(i => i.Code == "CSN2012").ToArray(); Assert.Equal(2, issues.Length);
        Assert.Equal("assembly", source.Substring(issues[0].Span.Start, issues[0].Span.Length));
        Assert.Equal("module", source.Substring(issues[1].Span.Start, issues[1].Span.Length));
    }
}
