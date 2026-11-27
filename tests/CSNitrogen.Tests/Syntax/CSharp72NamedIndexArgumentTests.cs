using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;

namespace CSNitrogen.Tests.Syntax;

public class CSharp72NamedIndexArgumentTests
{
    [Theory]
    [InlineData("class C { object M() => this[a: 1, 2]; }")]
    [InlineData("class C { object M() => this[0, b: 1, 2]; }")]
    [InlineData("class C : B { object M() => base[a: 1, 2]; }")]
    [InlineData("class C { object M() => this?[a: 1, 2]; }")]
    [InlineData("class C { object M() => new C { [a: 1, 2] = 3 }; }")]
    [InlineData("class C { object M() => this[a: 1, global::C.Value]; }")]
    [InlineData("class C { object M(bool b) => this[a: 1, b ? 2 : 3]; }")]
    public void Mixed_index_arguments_start_at_csharp72(string source)
    {
        using var before = Parse(source, "CSharp7_1");
        using var sixth = Parse(source, "CSharp6");
        using var after = Parse(source, "CSharp7_2");
        Assert.False(before.Accepted);
        Assert.False(sixth.Accepted);
        Assert.True(after.Accepted, string.Join(" | ", after.Issues));
    }

    [Theory]
    [InlineData("this[a: 1, 2, c: 3]", false)]
    [InlineData("base[a: 1, 2, c: 3]", true)]
    public void The_new_typed_view_retains_argument_order(string index, bool fromBase)
    {
        using var parsed = Parse($"class C : B {{ object M() => {index}; }}", "CSharp7_2");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        int node = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => fromBase
            ? MixedNamedBaseIndexExpressionExpression.Is(tree, i)
            : MixedNamedElementExpressionExpression.Is(tree, i));
        var arguments = fromBase
            ? MixedNamedBaseIndexExpressionExpression.Create(tree, node).Arguments
            : MixedNamedElementExpressionExpression.Create(tree, node).Arguments;
        Assert.Equal(3, arguments.Items.Count);
        Assert.True(NamedIndexArgumentNode.Is(tree, arguments.Items[0].Index));
        Assert.Equal("2", tree.GetText(arguments.Items[1].Index).ToString());
        Assert.True(NamedIndexArgumentNode.Is(tree, arguments.Items[2].Index));
        Assert.Equal(index, tree.GetText(node).ToString());
    }

    [Theory]
    [InlineData("class C { object M() => this[1, b: 2]; }", "NamedElementExpression")]
    [InlineData("class C { object M() => this[a: 1, b: 2]; }", "NamedElementExpression")]
    [InlineData("class C : B { object M() => base[a: 1, b: 2]; }", "NamedBaseIndexExpression")]
    [InlineData("class C { object M() => this[1, 2]; }", "ElementExpression")]
    public void Earlier_index_trees_keep_their_original_kinds(string source, string expected)
    {
        using var parsed = Parse(source, "CSharp14");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
        var tree = parsed.Tree;
        Assert.Single(Enumerable.Range(0, tree.NodeCount), i => tree.Language!.GetKindName(tree.Kind(i)) == expected);
        Assert.DoesNotContain(Enumerable.Range(0, tree.NodeCount), i =>
            MixedNamedElementExpressionExpression.Is(tree, i) || MixedNamedBaseIndexExpressionExpression.Is(tree, i));
    }

    [Theory]
    [InlineData("this[a: 1, @a: 2, 3]")]
    [InlineData("this?[a: 1, @a: 2]")]
    [InlineData("new C { [a: 1, @a: 2] = 3 }")]
    public void Duplicate_names_are_checked_in_each_index_list(string expression)
    {
        using var parsed = Parse($"class C {{ object M() => {expression}; }}", "CSharp7_2");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2040" &&
            issue.Message.Contains("repeat a logical parameter name", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("F(this?[a: 1, b: 2], a: 3)")]
    [InlineData("F(new C { [a: 1, b: 2] = 3 }, a: 4)")]
    [InlineData("F(this?[a: 1, b: 2], 3)")]
    public void Index_names_do_not_leak_into_an_enclosing_call(string expression)
    {
        using var parsed = Parse($"class C {{ object M() => {expression}; }}", "CSharp7_1");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
    }

    [Theory]
    [InlineData("class C { object M() => F(a: 1, 2); }")]
    [InlineData("class C { C(int a, int b) {} C(): this(a: 1, 2) {} }")]
    [InlineData("class AAttribute : System.Attribute { public AAttribute(int a, int b) {} } [A(a: 1, 2)] class C {}")]
    public void Existing_call_style_lists_already_have_the_release_boundary(string source)
    {
        using var before = Parse(source, "CSharp7_1");
        using var after = Parse(source, "CSharp7_2");
        Assert.False(before.Accepted);
        Assert.True(after.Accepted, string.Join(" | ", after.Issues));
    }

    [Fact]
    public void The_attribute_member_assignment_order_is_unchanged()
    {
        using var parsed = Parse("[A(1, P = 2, 3)] class C {}", "CSharp7_2");
        Assert.False(parsed.Accepted);
    }

    [Fact]
    public void Mixed_index_access_remains_a_primary_assignment_target()
    {
        using var parsed = Parse("class C { void M() { this[a: 1, 2] = 3; F(this[a: 1, 2].Value); } }", "CSharp7_2");
        Assert.True(parsed.Accepted, string.Join(" | ", parsed.Issues));
    }

    [Fact]
    public void Mixed_index_access_preserves_the_direct_array_restriction()
    {
        using var parsed = Parse("class C { object M() => new int[2][a: 1, 2]; }", "CSharp7_2");
        Assert.False(parsed.Accepted);
        Assert.Contains(parsed.Issues, issue => issue.Code == "CSN2005");
    }

    [Fact]
    public void Mixed_index_syntax_can_be_composed_without_changing_earlier_factories()
    {
        const string source = "class C { object M() => this[a: 1, 2]; }";
        var baseline = CSharp7_1Composition.CreateBuilder().Build();
        var custom = CSharp7_1Composition.CreateBuilder().Add(NonTrailingNamedIndexArgumentsModule.Instance).Build();
        using var before = baseline.Parse(source, CSharp7_1Composition.Start);
        using var after = custom.Parse(source, CSharp7_1Composition.Start);
        Assert.False(before.Success && !before.HasErrors);
        Assert.True(after.Success && !after.HasErrors);
        Assert.DoesNotContain("Grammars/NonTrailingNamedIndexArguments.ngr", ProfileResolver.Resolve("CSharp7_1").Profile!.Grammars);
        Assert.Contains("Grammars/NonTrailingNamedIndexArguments.ngr", ProfileResolver.Resolve("CSharp14").Profile!.Grammars);
    }

    private static ParseDocument Parse(string source, string profile) =>
        CSharpParser.Parse(new(source, profile, "index.cs", []));
}
