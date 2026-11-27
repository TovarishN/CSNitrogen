using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;

namespace CSNitrogen.Tests.Syntax;

public class CSharp72PrivateProtectedTests
{
    [Theory]
    [InlineData("class C { private protected int x; }", "protected")]
    [InlineData("class C { protected static private int x; }", "private")]
    [InlineData("class C { private protected class Inner {} }", "protected")]
    [InlineData("class C { protected private delegate void D(); }", "private")]
    public void Member_pair_starts_at_csharp72(string source, string secondModifier)
    {
        foreach (string profile in new[] { "CSharp1", "CSharp2", "CSharp3", "CSharp4", "CSharp5", "CSharp6", "CSharp7", "CSharp7_1" })
        {
            using var earlier = Parse(source, profile);
            Assert.False(earlier.Accepted);
            var issue = Assert.Single(earlier.Issues, issue => issue.Code == "CSN2011");
            Assert.Equal(IssueCategory.UnavailableFeature, issue.Category);
            Assert.Equal(secondModifier, source.Substring(issue.Span.Start, issue.Span.Length));
        }
        foreach (string profile in new[] { "CSharp7_2", "CSharp7_3", "CSharp14" })
        {
            using var later = Parse(source, profile);
            Assert.True(later.Accepted, string.Join(" | ", later.Issues));
        }
    }

    [Theory]
    [InlineData("class C { public int P { get; private protected set; } }", "private protected")]
    [InlineData("class C { public int P { get; protected private set; } }", "protected private")]
    [InlineData("class C { public int this[int n] { get { return 1; } private /*gap*/ protected set {} } }", "private /*gap*/ protected")]
    [InlineData("class C { public int this[int n] { get { return 1; } protected private set {} } }", "protected private")]
    public void Accessor_pair_starts_at_csharp72(string source, string modifierText)
    {
        using var earlier = Parse(source, "CSharp7_1");
        using var later = Parse(source, "CSharp7_2");
        Assert.False(earlier.Accepted);
        Assert.True(later.Accepted, string.Join(" | ", later.Issues));
        var tree = later.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount),
            i => PrivateProtectedAccessorAccessibilityAccessorModifiers.Is(tree, i));
        Assert.Equal(modifierText, tree.GetText(index).ToString());
    }

    [Theory]
    [InlineData("class C { protected internal int x; private int y; }")]
    [InlineData("class C { private int x; protected int y; }")]
    [InlineData("class C { protected class D { private int x; } }")]
    [InlineData("class C { private protected internal int x; }")]
    [InlineData("class C { public int P { get; protected internal set; } }")]
    [InlineData("class C { public int this[int n] { get { return 1; } protected internal set {} } }")]
    public void Existing_modifiers_and_separate_lists_are_independent(string source)
    {
        using var parsed = Parse(source, "CSharp6");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
    }

    [Fact]
    public void Explicit_interface_indexers_still_reject_accessor_modifiers()
    {
        const string source = "class C { int I.this[int n] { get { return 1; } private protected set {} } }";
        using var parsed = Parse(source, "CSharp7_2");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2010" &&
            issue.Message == "Explicit-interface indexers cannot declare accessor accessibility.");
    }

    [Fact]
    public void Incomplete_member_does_not_break_the_version_check()
    {
        using var parsed = Parse("class C { private protected int ; }", "CSharp6");
        Assert.False(parsed.Accepted);
    }

    [Fact]
    public void The_accessor_alternative_can_be_selected_independently()
    {
        const string source = "class C { public int P { get; private protected set; } }";
        var baseline = CSharp7_1Composition.CreateBuilder().Build();
        var custom = CSharp7_1Composition.CreateBuilder().Add(PrivateProtectedAccessorsModule.Instance).Build();
        using var old = baseline.Parse(source, CSharp7_1Composition.Start);
        using var current = custom.Parse(source, CSharp7_1Composition.Start);
        Assert.False(old.Success && !old.HasErrors);
        Assert.True(current.Success && !current.HasErrors);
        Assert.DoesNotContain("Grammars/PrivateProtectedAccessors.ngr", ProfileResolver.Resolve("CSharp7_1").Profile!.Grammars);
        Assert.Contains("Grammars/PrivateProtectedAccessors.ngr", ProfileResolver.Resolve("CSharp7_2").Profile!.Grammars);
    }

    private static ParseDocument Parse(string source, string profile) =>
        CSharpParser.Parse(new(source, profile, "accessibility.cs", []));
}
