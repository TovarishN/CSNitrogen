using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class CastConformanceParityTests
{
    public static IEnumerable<object[]> CastShapes()
    {
        string[] types = ["bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "string", "object", "T", "N.T", "T[]", "int[,]", "T*", "void**", "T*[]"];
        string[] values = ["x", "x.P", "F(x)", "x[0]", "x++", "!x", "~x", "(x)", "(U)x"];
        foreach (string type in types)
            foreach (string value in values) yield return [$"({type}){value}"];
        foreach (string source in new[]
        {
            "(x)+y", "(x)-y", "(x)*y", "(x)&y", "(x)++", "(x)--",
            "(int)+x", "(int)-x", "(int)++x", "(int)*p", "(T*)-x",
            "(T)x+y*z", "(T)(x)+y", "(T)((x)+y)", "((T)x).P", "((T)x)[0]",
            "(T)new C()", "(T)this", "(T)base.P", "(T)checked(x)", "(T)unchecked(x)",
            "(T)typeof(int)", "(T)sizeof(int)", "(T)true", "(T)null", "(T)1", "(T)@int",
            "(T)1.0", "(T)'x'", "(T)\"x\"", "(T)@\"x\"", "(T)false", "(int)--x",
            "(@int)+x", @"(cl\u0061ss)x", "(N.@this)(x)", "(int)&x", "(T*)&x", "(T) is U", "(T) as U",
            "( /*head*/ N . T [ , ] ) /*operand*/ F(x)", "(T)\n(x) /*tail*/ + y"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(CastShapes))]
    public void Cast_disambiguation_and_recursive_operands_match(string expression) => CompareSource(expression);

    public static IEnumerable<object[]> BinaryPairs()
    {
        string[] operators = ["*", "/", "%", "+", "-", "<<", ">>", "<", ">", "<=", ">=", "==", "!=", "&", "^", "|", "&&", "||"];
        foreach (string first in operators)
            foreach (string second in operators) yield return [$"(T)a {first} b {second} c"];
    }

    [Theory, MemberData(nameof(BinaryPairs))]
    public void Cast_operand_floor_preserves_binary_precedence_and_associativity(string expression) => CompareSource(expression);

    [Theory]
    [InlineData("(T)a=b=c")][InlineData("(T)a?b:c=d")][InlineData("a=(T)b?c:(U)d")]
    [InlineData("(T)(a=b)")][InlineData("(T)(a?b:c)")][InlineData("a+=(T)b*c")]
    public void Casts_interact_with_assignment_and_conditional_children(string expression) => CompareSource(expression);

    [Theory]
    [InlineData("(int)")][InlineData("(int)+")][InlineData("(T)x y")][InlineData("(T[1])x")]
    [InlineData("(int) is T")][InlineData("(int) as T")]
    [InlineData("(void)x")][InlineData("(int[]x")][InlineData("(T)++x")][InlineData("(T)--x")]
    [InlineData("(T)(int)")][InlineData("(T)(x")][InlineData("(T)()")][InlineData("(T[])x[]")]
    [InlineData("()x")][InlineData("((T)x")][InlineData("(T*)x y")]
    public void Malformed_cast_sources_have_oracle_syntax_errors(string expression)
    {
        string source = "unsafe class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.Contains(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "bad-cast.cs", []));
        Assert.False(ours.Accepted);
    }

    private static void CompareSource(string expression)
    {
        string source = "unsafe class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression!;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "casts.cs", []));
        Assert.True(ours.Accepted, expression + " :: " + string.Join(";", ours.Issues));
        var tree = ours.Tree;
        int root = Assert.Single(Enumerable.Range(0, tree.NodeCount), n =>
            tree.Language!.GetKindName(tree.Kind(n)).EndsWith("Expression", StringComparison.Ordinal) &&
            tree.Span(n).Start == expected.Span.Start && tree.Span(n).Length == expected.Span.Length);
        ExpressionShapeAssert.Compare(expected, new ExpressionNode(tree, root));
    }

}
