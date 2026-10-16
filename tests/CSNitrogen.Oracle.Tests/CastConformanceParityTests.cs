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
        Compare(expected, new ExpressionNode(tree, root));
    }

    private static void Compare(ExpressionSyntax expected, ExpressionNode actual)
    {
        var tree = actual.Tree;
        string kind = tree.Language!.GetKindName(actual.Kind);
        void Kind(string name) => Assert.Equal(name, kind);
        void Span(Microsoft.CodeAnalysis.Text.TextSpan span, Nitrogen.TextSpan ours)
        { Assert.Equal(span.Start, ours.Start); Assert.Equal(span.Length, ours.Length); }
        void Type(TypeSyntax type, Nitrogen.TextSpan span)
        { Span(type.Span, span); Assert.Equal(type.ToString(), tree.Text.Substring(span.Start, span.Length)); }
        void Arguments(ArgumentListSyntax expectedArgs, ArgumentsNode args)
        {
            Span(expectedArgs.Span, args.Span); Assert.Equal(expectedArgs.Arguments.Count, args.Items.Count);
            for (int i = 0; i < args.Items.Count; i++)
            { Span(expectedArgs.Arguments[i].Span, args.Items[i].Span); Compare(expectedArgs.Arguments[i].Expression, args.Items[i].Value); }
        }
        Span(expected.Span, actual.Span);
        switch (expected)
        {
            case IdentifierNameSyntax name:
                Kind("NameExpression"); Span(name.Identifier.Span, SyntaxView.Cast<NameExpressionExpression>(tree, actual.Index).Name.Span); Assert.Equal(name.Identifier.Text, SyntaxView.Cast<NameExpressionExpression>(tree, actual.Index).Name.ToString()); break;
            case LiteralExpressionSyntax:
                Kind("LiteralExpression"); Assert.Equal(expected.ToString(), tree.Text.Substring(actual.Span.Start, actual.Span.Length)); break;
            case ParenthesizedExpressionSyntax parenthesized:
                Kind("ParenthesizedExpression"); Compare(parenthesized.Expression, SyntaxView.Cast<ParenthesizedExpressionExpression>(tree, actual.Index).Value); break;
            case CastExpressionSyntax cast:
                Kind("CastExpression"); var c = SyntaxView.Cast<CastExpressionExpression>(tree, actual.Index);
                Assert.Equal(cast.OpenParenToken.Span.Start, c.Head.Span.Start);
                Assert.Equal(cast.CloseParenToken.Span.End, c.Head.Span.End);
                Nitrogen.TextSpan typeSpan;
                if (cast.Type is PointerTypeSyntax || cast.Type is ArrayTypeSyntax { ElementType: PointerTypeSyntax })
                { Assert.Equal("PointerCastHead", tree.Language.GetKindName(c.Head.Kind)); typeSpan = SyntaxView.Cast<PointerCastHeadCastHead>(tree, c.Head.Index).Type.Span; }
                else if (cast.Type is PredefinedTypeSyntax or ArrayTypeSyntax)
                { Assert.Equal("DefiniteCast", tree.Language.GetKindName(c.Head.Kind)); typeSpan = SyntaxView.Cast<DefiniteCastCastHead>(tree, c.Head.Index).Type.Span; }
                else
                { Assert.Equal("NamedCast", tree.Language.GetKindName(c.Head.Kind)); typeSpan = SyntaxView.Cast<NamedCastCastHead>(tree, c.Head.Index).Name.Span; }
                Type(cast.Type, typeSpan); Compare(cast.Expression, c.Value); break;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.IsExpression):
                Kind("IsExpression"); var isNode = SyntaxView.Cast<IsExpressionExpression>(tree, actual.Index);
                Compare(binary.Left, isNode.Value); Type((TypeSyntax)binary.Right, isNode.Type.Span); break;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                Kind("AsExpression"); var asNode = SyntaxView.Cast<AsExpressionExpression>(tree, actual.Index);
                Compare(binary.Left, asNode.Value); Type((TypeSyntax)binary.Right, asNode.Type.Span); break;
            case BinaryExpressionSyntax binary:
                var operands = BinaryOperands(kind, actual); Compare(binary.Left, operands.Left); Compare(binary.Right, operands.Right);
                Assert.Equal(BinaryName(binary.Kind()), kind); break;
            case AssignmentExpressionSyntax assignment:
                Kind("AssignmentExpression"); var a = SyntaxView.Cast<AssignmentExpressionExpression>(tree, actual.Index);
                Span(assignment.OperatorToken.Span, a.Operator.Span); Assert.Equal(assignment.OperatorToken.Text, tree.Text.Substring(a.Operator.Span.Start, a.Operator.Span.Length));
                Compare(assignment.Left, a.Target); Compare(assignment.Right, a.Value); break;
            case ConditionalExpressionSyntax conditional:
                Kind("ConditionalExpression"); var q = SyntaxView.Cast<ConditionalExpressionExpression>(tree, actual.Index);
                Compare(conditional.Condition, q.Condition); Compare(conditional.WhenTrue, q.True); Compare(conditional.WhenFalse, q.False.Value); break;
            case PrefixUnaryExpressionSyntax unary:
                if (unary.IsKind(SyntaxKind.PointerIndirectionExpression))
                { Kind("PointerIndirectionExpression"); Compare(unary.Operand, SyntaxView.Cast<PointerIndirectionExpressionExpression>(tree, actual.Index).Value); }
                else if (unary.IsKind(SyntaxKind.AddressOfExpression))
                { Kind("AddressExpression"); Compare(unary.Operand, SyntaxView.Cast<AddressExpressionExpression>(tree, actual.Index).Value); }
                else
                { Kind("UnaryExpression"); var u = SyntaxView.Cast<UnaryExpressionExpression>(tree, actual.Index); Span(unary.OperatorToken.Span, u.Operator.Span); Assert.Equal(unary.OperatorToken.Text, tree.Text.Substring(u.Operator.Span.Start, u.Operator.Span.Length)); Compare(unary.Operand, u.Value); }
                break;
            case PostfixUnaryExpressionSyntax postfix:
                if (postfix.IsKind(SyntaxKind.PostIncrementExpression))
                { Kind("PostfixIncrementExpression"); Compare(postfix.Operand, SyntaxView.Cast<PostfixIncrementExpressionExpression>(tree, actual.Index).Value); }
                else
                { Kind("PostfixDecrementExpression"); Compare(postfix.Operand, SyntaxView.Cast<PostfixDecrementExpressionExpression>(tree, actual.Index).Value); }
                break;
            case MemberAccessExpressionSyntax member when member.Expression is BaseExpressionSyntax:
                Kind("BaseMemberExpression"); Span(member.Name.Span, SyntaxView.Cast<BaseMemberExpressionExpression>(tree, actual.Index).Name.Span); Assert.Equal(member.Name.ToString(), SyntaxView.Cast<BaseMemberExpressionExpression>(tree, actual.Index).Name.ToString()); break;
            case MemberAccessExpressionSyntax member:
                Kind("MemberExpression"); var m = SyntaxView.Cast<MemberExpressionExpression>(tree, actual.Index);
                Compare(member.Expression, m.Target); Span(member.Name.Span, m.Name.Span); Assert.Equal(member.Name.ToString(), m.Name.ToString()); break;
            case InvocationExpressionSyntax invocation:
                Kind("InvocationExpression"); var call = SyntaxView.Cast<InvocationExpressionExpression>(tree, actual.Index);
                Compare(invocation.Expression, call.Target); Arguments(invocation.ArgumentList, call.Args); break;
            case ElementAccessExpressionSyntax element:
                Kind("ElementExpression"); var e = SyntaxView.Cast<ElementExpressionExpression>(tree, actual.Index);
                Compare(element.Expression, e.Target); Assert.Equal(element.ArgumentList.Arguments.Count, e.Indices.Count);
                for (int i = 0; i < e.Indices.Count; i++) Compare(element.ArgumentList.Arguments[i].Expression, e.Indices[i]); break;
            case ObjectCreationExpressionSyntax creation:
                Kind("ObjectCreationExpression"); var obj = SyntaxView.Cast<ObjectCreationExpressionExpression>(tree, actual.Index);
                Type(creation.Type, obj.Type.Span); Arguments(creation.ArgumentList!, obj.Args); break;
            case ThisExpressionSyntax: Kind("ThisExpression"); break;
            case CheckedExpressionSyntax check:
                if (check.IsKind(SyntaxKind.CheckedExpression))
                { Kind("CheckedExpression"); Compare(check.Expression, SyntaxView.Cast<CheckedExpressionExpression>(tree, actual.Index).Value); }
                else
                { Kind("UncheckedExpression"); Compare(check.Expression, SyntaxView.Cast<UncheckedExpressionExpression>(tree, actual.Index).Value); }
                break;
            case TypeOfExpressionSyntax typeOf:
                Kind("TypeOfExpression"); Type(typeOf.Type, SyntaxView.Cast<TypeOfExpressionExpression>(tree, actual.Index).Type.Span); break;
            case SizeOfExpressionSyntax sizeOf:
                Kind("SizeOfBuiltinExpression"); Type(sizeOf.Type, SyntaxView.Cast<SizeOfBuiltinExpressionExpression>(tree, actual.Index).Type.Span); break;
            default: Assert.Fail("Uncompared oracle expression kind: " + expected.Kind()); break;
        }
    }

    private static string BinaryName(SyntaxKind kind) => kind switch
    {
        SyntaxKind.MultiplyExpression => "MultiplyExpression", SyntaxKind.DivideExpression => "DivideExpression",
        SyntaxKind.ModuloExpression => "RemainderExpression", SyntaxKind.AddExpression => "AddExpression",
        SyntaxKind.SubtractExpression => "SubtractExpression", SyntaxKind.LeftShiftExpression => "ShiftLeftExpression",
        SyntaxKind.RightShiftExpression => "ShiftRightExpression", SyntaxKind.LessThanExpression => "LessExpression",
        SyntaxKind.GreaterThanExpression => "GreaterExpression", SyntaxKind.LessThanOrEqualExpression => "LessEqualExpression",
        SyntaxKind.GreaterThanOrEqualExpression => "GreaterEqualExpression", SyntaxKind.EqualsExpression => "EqualExpression",
        SyntaxKind.NotEqualsExpression => "NotEqualExpression", SyntaxKind.BitwiseAndExpression => "BitAndExpression",
        SyntaxKind.ExclusiveOrExpression => "BitXorExpression", SyntaxKind.BitwiseOrExpression => "BitOrExpression",
        SyntaxKind.LogicalAndExpression => "AndExpression", SyntaxKind.LogicalOrExpression => "OrExpression",
        _ => throw new InvalidOperationException("Uncompared binary kind: " + kind)
    };

    private static (ExpressionNode Left, ExpressionNode Right) BinaryOperands(string kind, ExpressionNode node)
    {
        var t = node.Tree; int n = node.Index;
        return kind switch
        {
            "MultiplyExpression" => (SyntaxView.Cast<MultiplyExpressionExpression>(t,n).Left, SyntaxView.Cast<MultiplyExpressionExpression>(t,n).Right),
            "DivideExpression" => (SyntaxView.Cast<DivideExpressionExpression>(t,n).Left, SyntaxView.Cast<DivideExpressionExpression>(t,n).Right),
            "RemainderExpression" => (SyntaxView.Cast<RemainderExpressionExpression>(t,n).Left, SyntaxView.Cast<RemainderExpressionExpression>(t,n).Right),
            "AddExpression" => (SyntaxView.Cast<AddExpressionExpression>(t,n).Left, SyntaxView.Cast<AddExpressionExpression>(t,n).Right),
            "SubtractExpression" => (SyntaxView.Cast<SubtractExpressionExpression>(t,n).Left, SyntaxView.Cast<SubtractExpressionExpression>(t,n).Right),
            "ShiftLeftExpression" => (SyntaxView.Cast<ShiftLeftExpressionExpression>(t,n).Left, SyntaxView.Cast<ShiftLeftExpressionExpression>(t,n).Right),
            "ShiftRightExpression" => (SyntaxView.Cast<ShiftRightExpressionExpression>(t,n).Left, SyntaxView.Cast<ShiftRightExpressionExpression>(t,n).Right),
            "LessExpression" => (SyntaxView.Cast<LessExpressionExpression>(t,n).Left, SyntaxView.Cast<LessExpressionExpression>(t,n).Right),
            "GreaterExpression" => (SyntaxView.Cast<GreaterExpressionExpression>(t,n).Left, SyntaxView.Cast<GreaterExpressionExpression>(t,n).Right),
            "LessEqualExpression" => (SyntaxView.Cast<LessEqualExpressionExpression>(t,n).Left, SyntaxView.Cast<LessEqualExpressionExpression>(t,n).Right),
            "GreaterEqualExpression" => (SyntaxView.Cast<GreaterEqualExpressionExpression>(t,n).Left, SyntaxView.Cast<GreaterEqualExpressionExpression>(t,n).Right),
            "EqualExpression" => (SyntaxView.Cast<EqualExpressionExpression>(t,n).Left, SyntaxView.Cast<EqualExpressionExpression>(t,n).Right),
            "NotEqualExpression" => (SyntaxView.Cast<NotEqualExpressionExpression>(t,n).Left, SyntaxView.Cast<NotEqualExpressionExpression>(t,n).Right),
            "BitAndExpression" => (SyntaxView.Cast<BitAndExpressionExpression>(t,n).Left, SyntaxView.Cast<BitAndExpressionExpression>(t,n).Right),
            "BitXorExpression" => (SyntaxView.Cast<BitXorExpressionExpression>(t,n).Left, SyntaxView.Cast<BitXorExpressionExpression>(t,n).Right),
            "BitOrExpression" => (SyntaxView.Cast<BitOrExpressionExpression>(t,n).Left, SyntaxView.Cast<BitOrExpressionExpression>(t,n).Right),
            "AndExpression" => (SyntaxView.Cast<AndExpressionExpression>(t,n).Left, SyntaxView.Cast<AndExpressionExpression>(t,n).Right),
            "OrExpression" => (SyntaxView.Cast<OrExpressionExpression>(t,n).Left, SyntaxView.Cast<OrExpressionExpression>(t,n).Right),
            _ => throw new InvalidOperationException("Unexpected binary node: " + kind)
        };
    }
}
