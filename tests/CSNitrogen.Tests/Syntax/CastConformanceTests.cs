using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class CastConformanceTests
{
    private static readonly Language Full = CSharp1Composition.CreateBuilder().Build();
    private static readonly Language Safe = CSharp1SafeComposition.CreateBuilder().Build();

    [Theory]
    [InlineData("(T)x", "NamedCast")][InlineData("(N.T)(x)", "NamedCast")]
    [InlineData("(int)-x", "DefiniteCast")][InlineData("(T[])x", "DefiniteCast")]
    [InlineData("(int[,])x", "DefiniteCast")][InlineData("(T*)x", "PointerCastHead")]
    [InlineData("(void**)x", "PointerCastHead")][InlineData("(T*[])x", "PointerCastHead")]
    public void Cast_heads_keep_selected_alternative_and_operand_coordinates(string source, string expectedHead)
    {
        using var parsed = Full.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        var cast = SyntaxView.Cast<CastExpressionExpression>(parsed.Tree, parsed.Tree.Child(parsed.Tree.Root, 0));
        Assert.Equal(expectedHead, Full.GetKindName(cast.Head.Kind));
        int close = source.IndexOf(')');
        Assert.Equal(new TextSpan(0, close + 1), cast.Head.Span);
        Assert.Equal(new TextSpan(close + 1, source.Length - close - 1), cast.Value.Span);
    }

    [Theory]
    [InlineData("(T)+x", "AddExpression")][InlineData("(T)-x", "SubtractExpression")]
    [InlineData("(T)*x", "MultiplyExpression")][InlineData("(T)&x", "BitAndExpression")]
    [InlineData("(T)++", "PostfixIncrementExpression")][InlineData("(T)--", "PostfixDecrementExpression")]
    [InlineData("(T)", "ParenthesizedExpression")]
    [InlineData("(T) is U", "IsExpression")][InlineData("(T) as U", "AsExpression")]
    public void Ambiguous_names_use_token_grammar_without_binding(string source, string expected)
    {
        using var parsed = Safe.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        Assert.Equal(expected, Safe.GetKindName(parsed.Tree.Kind(parsed.Tree.Child(parsed.Tree.Root, 0))));
        Assert.DoesNotContain(Enumerable.Range(0, parsed.Tree.NodeCount), n => CastExpressionExpression.Is(parsed.Tree, n));
    }

    [Theory]
    [InlineData("(int)")][InlineData("(int)+")][InlineData("(T)x y")][InlineData("(T[1])x")]
    [InlineData("(void)x")][InlineData("(int[]x")][InlineData("(T)++x")][InlineData("(T)--x")]
    [InlineData("(T)(int)")][InlineData("(T)(x")][InlineData("(T)()")][InlineData("(T[])x[]")]
    [InlineData("()x")][InlineData("((T)x")][InlineData("(T*)x y")][InlineData("(int) is T")][InlineData("(int) as T")]
    public void Malformed_casts_require_complete_heads_and_operands(string source)
    {
        using var parsed = Full.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.False(parsed.Success && !parsed.HasErrors);
        using var owned = CSharpParser.Parse(new("unsafe class C{object M(){return " + source + ";}}", "CSharp1", "bad-cast.cs", []));
        Assert.False(owned.Accepted);
    }

    [Theory]
    [InlineData("*")][InlineData("/")][InlineData("%")][InlineData("+")][InlineData("-")]
    [InlineData("<<")][InlineData(">>")][InlineData("<")][InlineData(">")]
    [InlineData("<=")][InlineData(">=")][InlineData("==")][InlineData("!=")]
    [InlineData("&")][InlineData("^")][InlineData("|")][InlineData("&&")][InlineData("||")]
    public void Cast_then_binary_operator_requires_a_right_operand(string op)
    {
        string source = "(T)a " + op;
        using var parsed = Full.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.False(parsed.Success && !parsed.HasErrors);
        using var owned = CSharpParser.Parse(new("class C{object M(){return " + source + ";}}", "CSharp1", "tail.cs", []));
        Assert.False(owned.Accepted);
    }

    [Theory]
    [InlineData("(T*)x", false)][InlineData("((T*)p)[0]", false)]
    [InlineData("(int)*p", false)][InlineData("(int)&x", false)]
    [InlineData("(T)*p", true)][InlineData("(T)&x", true)][InlineData("((T)x).P", true)]
    public void Unsafe_cast_and_unary_syntax_stays_in_its_module(string source, bool safeExpected)
    {
        using var full = Full.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.True(full.Success && !full.HasErrors);
        using var safe = Safe.Parse(source, ExpressionsModule.ExpressionFragment);
        Assert.Equal(safeExpected, safe.Success && !safe.HasErrors);
        Assert.DoesNotContain(Safe.Modules, module => module.Name == "CSharp.Unsafe");
    }
}
