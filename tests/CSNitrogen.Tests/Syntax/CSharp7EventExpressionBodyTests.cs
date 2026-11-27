using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CSharp7EventExpressionBodyTests
{
    [Theory]
    [InlineData("add => h += value; remove => h -= value;")]
    [InlineData("remove => h -= value; add => h += value;")]
    [InlineData("add { h += value; } remove => h -= value;")]
    [InlineData("remove { h -= value; } add => h += value;")]
    [InlineData("add => h += value; remove { h -= value; }")]
    [InlineData("remove => h -= value; add { h += value; }")]
    public void Arrow_event_accessors_start_at_csharp7(string accessors)
    {
        using var earlier = Parse(accessors, "CSharp6");
        using var current = Parse(accessors, "CSharp7");
        Assert.False(earlier.Accepted);
        Assert.True(current.Accepted, string.Join(" | ", current.Issues));
        var tree = current.Tree;
        Assert.Single(Enumerable.Range(0, tree.NodeCount), i => ExpressionBodiedEventPropertyMemberDeclarationExtension.Is(tree, i));
        Assert.DoesNotContain(Enumerable.Range(0, tree.NodeCount), i => EventPropertyDeclarationNode.Is(tree, i));
        using var later = Parse(accessors, "CSharp14");
        Assert.True(later.Accepted, string.Join(" | ", later.Issues));
    }

    [Fact]
    public void Block_only_events_keep_their_original_typed_accessors()
    {
        foreach (string profile in new[] { "CSharp6", "CSharp7", "CSharp14" })
        {
            using var parsed = Parse("add { h += value; } remove { h -= value; }", profile);
            Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
            var tree = parsed.Tree;
            int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => EventPropertyDeclarationNode.Is(tree, i));
            var declaration = EventPropertyDeclarationNode.Create(tree, index);
            Assert.Equal(2, declaration.Accessors.Count);
            BlockNode body = declaration.Accessors[0].Body;
            Assert.Equal("{ h += value; }", tree.GetText(body.Index).ToString());
            Assert.DoesNotContain(Enumerable.Range(0, tree.NodeCount), i => ArrowEventAccessorNode.Is(tree, i));
        }
    }

    [Fact]
    public void Qualified_mixed_event_preserves_both_body_shapes_and_spans()
    {
        using var parsed = Parse("[A] add { h += value; } [B] remove => h -= value;", "CSharp7", "I<int>.E");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        int blockIndex = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => EventAccessorNode.Is(tree, i));
        int arrowIndex = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => ArrowEventAccessorNode.Is(tree, i));
        var block = EventAccessorNode.Create(tree, blockIndex);
        var arrow = ArrowEventAccessorNode.Create(tree, arrowIndex);
        Assert.Equal("add", tree.GetText(block.Keyword.Index).ToString());
        Assert.Equal("remove", arrow.Keyword.ToString());
        Assert.Equal(1, block.Attributes.Count);
        Assert.Equal(1, arrow.Attributes.Count);
        ExpressionClauseNode arrowBody = arrow.Body;
        Assert.Equal("=> h -= value;", tree.GetText(arrowBody.Index).ToString());
    }

    [Fact]
    public void Arrow_event_bodies_compose_with_throw_expressions()
    {
        using var parsed = Parse("add => throw new System.Exception(); remove {}", "CSharp7");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        int index = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => ArrowEventAccessorNode.Is(tree, i));
        Assert.True(ThrowExpressionExpression.Is(tree, ArrowEventAccessorNode.Create(tree, index).Body.Value.Index));
    }

    [Theory]
    [InlineData("add => h += value;", "requires both add and remove")]
    [InlineData("remove => h -= value;", "requires both add and remove")]
    [InlineData("add => h += value; add { h += value; } remove { h -= value; }", "only once")]
    [InlineData("add { h += value; } remove => h -= value; remove => h -= value;", "only once")]
    public void Mixed_accessor_pair_rules_are_enforced(string accessors, string diagnostic)
    {
        using var parsed = Parse(accessors, "CSharp7");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2010" &&
            issue.Message.Contains(diagnostic, StringComparison.Ordinal));
    }

    [Fact]
    public void Event_bodies_can_be_selected_as_a_separate_module()
    {
        const string source = "class C { event E A { add {} remove => F(value); } }";
        var baseline = CSharp6Composition.CreateBuilder().Build();
        var extended = CSharp6Composition.CreateBuilder().Add(EventExpressionBodiesModule.Instance).Build();
        using var old = baseline.Parse(source, CSharp6Composition.Start);
        using var current = extended.Parse(source, CSharp6Composition.Start);
        Assert.False(old.Success && !old.HasErrors);
        Assert.True(current.Success && !current.HasErrors);
        Assert.DoesNotContain("Grammars/EventExpressionBodies.ngr", ProfileResolver.Resolve("CSharp6").Profile!.Grammars);
        Assert.Contains("Grammars/EventExpressionBodies.ngr", ProfileResolver.Resolve("CSharp7").Profile!.Grammars);
    }

    private static ParseDocument Parse(string accessors, string profile, string name = "E") =>
        CSharpParser.Parse(new($"class C {{ System.Action h; event System.Action {name} {{ {accessors} }} }}", profile, "event.cs", []));
}
