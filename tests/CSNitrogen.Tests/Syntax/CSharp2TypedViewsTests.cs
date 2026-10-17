using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2TypedViewsTests
{
    [Fact] public void Coalescing_is_right_associative_and_anonymous_parameter_modes_are_typed()
    {
        const string source="class C { object d=delegate(ref int x,out int y) { y=x; }; int M(int? x,int? y) { return x ?? y ?? 0; } }";
        using var result=CSharpParser.Parse(new(source,"CSharp2","views.cs",[]));Assert.True(result.Accepted);
        var tree=result.Tree;
        int anon=Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>AnonymousMethodExpressionExpression.Is(tree,n));
        var method=SyntaxView.Cast<AnonymousMethodExpressionExpression>(tree,anon);Assert.True(method.Parameters.HasValue);
        Assert.Equal(2,method.Parameters.Value.Items.Count);Assert.Equal("x",method.Parameters.Value.Items[0].Name.ToString());
        Assert.Equal("y",method.Parameters.Value.Items[1].Name.ToString());Assert.True(method.Parameters.Value.Items[0].Mode.HasValue);
        Assert.Equal("ref",source.Substring(method.Parameters.Value.Items[0].Mode.Value.Span.Start,method.Parameters.Value.Items[0].Mode.Value.Span.Length));
        Assert.Equal(1,method.Body.Statements.Count);
        int outer=Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>NullCoalescingExpressionExpression.Is(tree,n) && tree.Span(n).Start==source.IndexOf("x ??",StringComparison.Ordinal));
        var expression=SyntaxView.Cast<NullCoalescingExpressionExpression>(tree,outer);
        Assert.True(NullCoalescingExpressionExpression.Is(tree,expression.Right.Index));Assert.False(NullCoalescingExpressionExpression.Is(tree,expression.Left.Index));
    }
    [Fact] public void Alias_directives_accessibility_and_fixed_declarators_have_owning_typed_fields()
    {
        const string source="extern alias A; unsafe partial struct S { public fixed int x[5],y[10]; int P { get { return 1; } private set {} } }";
        using var result=CSharpParser.Parse(new(source,"CSharp2","views.cs",[]));Assert.True(result.Accepted);
        var tree=result.Tree;Assert.True(result.Root.ExternAliases.HasValue);
        var aliases=SyntaxView.Cast<ExternalAliasListExternalAliasDirectives>(tree,result.Root.ExternAliases.Value.Index);
        Assert.Equal(1,aliases.Items.Count);Assert.Equal("A",aliases.Items[0].Name.ToString());
        int fixedNode=Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>FixedBufferMemberClassMember.Is(tree,n));
        var member=SyntaxView.Cast<FixedBufferMemberClassMember>(tree,fixedNode);Assert.Equal(2,member.Variables.Count);
        Assert.Equal("x",member.Variables[0].Name.ToString());Assert.Equal("y",member.Variables[1].Name.ToString());
        Assert.Equal(new TextSpan(source.IndexOf("10",StringComparison.Ordinal),2),member.Variables[1].Size.Span);
        Assert.Equal(",",member.Variables.Separator(0).ToString());
        int access=Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>AccessorAccessibilityAccessorModifiers.Is(tree,n));
        Assert.Equal(new TextSpan(source.IndexOf("private",StringComparison.Ordinal),7),tree.Span(access));
    }
}
