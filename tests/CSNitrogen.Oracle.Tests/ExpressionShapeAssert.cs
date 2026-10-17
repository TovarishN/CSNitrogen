using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

internal static class ExpressionShapeAssert
{
    private static void SameSpan(Microsoft.CodeAnalysis.Text.TextSpan expected, Nitrogen.TextSpan actual)
    { Assert.Equal(expected.Start, actual.Start); Assert.Equal(expected.Length, actual.Length); }

    internal static void CompareInitializer(InitializerExpressionSyntax expected, ArrayInitializerNode actual)
    {
        SameSpan(expected.Span, actual.Span);
        Assert.Equal(expected.Expressions.Count > 0, actual.Items.HasValue);
        if (!actual.Items.HasValue) return;
        var group = actual.Items.Value;
        Assert.Equal(SyntaxKinds.Group, group.Kind); Assert.Equal(2, group.ChildCount);
        var list = group.Child(0); Assert.Equal(SyntaxKinds.List, list.Kind);
        var values = new SeparatedList<Nitrogen.SyntaxNode>(actual.Tree, list.Index);
        Assert.Equal(expected.Expressions.Count, values.Count);
        var separators = expected.Expressions.GetSeparators().ToArray();
        Assert.Equal(Math.Max(0, expected.Expressions.Count - 1), values.SeparatorCount);
        for (int i = 0; i < values.Count; i++)
        {
            if (expected.Expressions[i] is InitializerExpressionSyntax nested)
                CompareInitializer(nested, SyntaxView.Cast<ArrayInitializerNode>(actual.Tree, values[i].Index));
            else Compare(expected.Expressions[i], new ExpressionNode(actual.Tree, values[i].Index));
            if (i < values.SeparatorCount) SameSpan(separators[i].Span, values.Separator(i).Span);
        }
        var trailing = group.Child(1);
        Assert.Equal(separators.Length == values.Count, trailing.Kind != SyntaxKinds.Empty);
        if (trailing.Kind != SyntaxKinds.Empty)
        { Assert.Equal(",", trailing.ToString()); SameSpan(separators[^1].Span, trailing.Span); }
    }

    internal static void CompareArguments(ArgumentListSyntax expected, ArgumentsNode actual)
    {
        SameSpan(expected.Span, actual.Span); Assert.Equal(expected.Arguments.Count, actual.Items.Count);
        var separators = expected.Arguments.GetSeparators().ToArray();
        Assert.Equal(separators.Length, actual.Items.SeparatorCount);
        for (int i = 0; i < actual.Items.Count; i++)
        {
            var argument = actual.Items[i]; var reference = expected.Arguments[i];
            SameSpan(reference.Span, argument.Span);
            Assert.Null(reference.NameColon);
            Assert.Equal(reference.RefKindKeyword.RawKind != 0, argument.Mode.HasValue);
            if (argument.Mode.HasValue)
            { SameSpan(reference.RefKindKeyword.Span, argument.Mode.Value.Span); Assert.Equal(reference.RefKindKeyword.Text, argument.Mode.Value.ToString()); }
            Compare(reference.Expression, argument.Value);
            if (i < separators.Length) SameSpan(separators[i].Span, actual.Items.Separator(i).Span);
        }
    }

    private static void CompareArray(ArrayCreationExpressionSyntax expected, ExpressionNode actual)
    {
        var tree = actual.Tree; bool sized = expected.Type.RankSpecifiers[0].Sizes.Any(e => e is not OmittedArraySizeExpressionSyntax);
        TypeBaseNode element; Nitrogen.SyntaxList<ArrayRankNode> ranks; ArrayInitializerNode? initializer;
        if (sized)
        {
            Assert.Equal("SizedArrayExpression", tree.Language!.GetKindName(actual.Kind));
            var array = SyntaxView.Cast<SizedArrayExpressionExpression>(tree, actual.Index).Value;
            SameSpan(expected.Span, array.Span); element = array.Element; ranks = array.Ranks;
            initializer = array.Initializer.HasValue ? array.Initializer.Value : null;
            var sizes = expected.Type.RankSpecifiers[0].Sizes;
            Assert.Equal(sizes.Count, array.Sizes.Count);
            var separators = sizes.GetSeparators().ToArray();
            Assert.Equal(separators.Length, array.Sizes.SeparatorCount);
            for (int i = 0; i < sizes.Count; i++)
            {
                Compare(sizes[i], array.Sizes[i]);
                if (i < separators.Length) SameSpan(separators[i].Span, array.Sizes.Separator(i).Span);
            }
        }
        else
        {
            Assert.Equal("InitializedArrayExpression", tree.Language!.GetKindName(actual.Kind));
            var array = SyntaxView.Cast<InitializedArrayExpressionExpression>(tree, actual.Index).Value;
            SameSpan(expected.Span, array.Span); element = array.Element; ranks = array.Ranks; initializer = array.Initializer;
        }
        SameSpan(expected.Type.ElementType.Span, element.Span);
        Assert.Equal(expected.Type.ElementType.ToString(), tree.Text.Substring(element.Span.Start, element.Span.Length));
        int firstRank = sized ? 1 : 0;
        Assert.Equal(expected.Type.RankSpecifiers.Count - firstRank, ranks.Count);
        for (int i = 0; i < ranks.Count; i++)
        {
            var reference = expected.Type.RankSpecifiers[i + firstRank]; var rank = ranks[i];
            SameSpan(reference.Span, rank.Span); var commas = reference.Sizes.GetSeparators().ToArray();
            Assert.Equal(commas.Length, rank.Commas.Count);
            for (int c = 0; c < commas.Length; c++) SameSpan(commas[c].Span, rank.Commas[c].Span);
        }
        Assert.Equal(expected.Initializer is not null, initializer.HasValue);
        if (expected.Initializer is not null) CompareInitializer(expected.Initializer, initializer!.Value);
    }

    internal static void Compare(ExpressionSyntax expected, ExpressionNode actual)
    {
        var tree = actual.Tree;
        string kind = tree.Language!.GetKindName(actual.Kind);
        void Kind(string name) => Assert.Equal(name, kind);
        void Span(Microsoft.CodeAnalysis.Text.TextSpan span, Nitrogen.TextSpan ours)
        { Assert.Equal(span.Start, ours.Start); Assert.Equal(span.Length, ours.Length); }
        void Type(TypeSyntax type, Nitrogen.TextSpan span)
        { Span(type.Span, span); Assert.Equal(type.ToString(), tree.Text.Substring(span.Start, span.Length)); }
        Span(expected.Span, actual.Span);
        switch (expected)
        {
            case IdentifierNameSyntax name:
                Kind("NameExpression"); Span(name.Identifier.Span, SyntaxView.Cast<NameExpressionExpression>(tree, actual.Index).Name.Span); Assert.Equal(name.Identifier.Text, SyntaxView.Cast<NameExpressionExpression>(tree, actual.Index).Name.ToString()); break;
            case LiteralExpressionSyntax literal:
                Kind("LiteralExpression"); var literalValue = SyntaxView.Cast<LiteralExpressionExpression>(tree, actual.Index).Value;
                Span(literal.Token.Span, literalValue.Span);
                Assert.Equal(literal.Token.Text, tree.Text.Substring(literalValue.Span.Start, literalValue.Span.Length)); break;
            case ArrayCreationExpressionSyntax arrayCreation:
                CompareArray(arrayCreation, actual); break;
            case ParenthesizedExpressionSyntax parenthesized:
                Kind("ParenthesizedExpression"); Compare(parenthesized.Expression, SyntaxView.Cast<ParenthesizedExpressionExpression>(tree, actual.Index).Value); break;
            case CastExpressionSyntax cast:
                Kind("CastExpression"); var c = SyntaxView.Cast<CastExpressionExpression>(tree, actual.Index);
                Assert.Equal(cast.OpenParenToken.Span.Start, c.Head.Span.Start);
                Assert.Equal(cast.CloseParenToken.Span.End, c.Head.Span.End);
                Nitrogen.TextSpan typeSpan;
                if (cast.Type is PointerTypeSyntax || cast.Type is ArrayTypeSyntax { ElementType: PointerTypeSyntax })
                { Assert.Equal("PointerCastHead", tree.Language.GetKindName(c.Head.Kind)); var pointerCast=SyntaxView.Cast<PointerCastHeadCastHead>(tree, c.Head.Index).Type; typeSpan=pointerCast.Span; UnsafeShapeAssert.PointerCast(cast.Type,pointerCast); }
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
                { Kind("PointerIndirectionExpression"); var indirect = SyntaxView.Cast<PointerIndirectionExpressionExpression>(tree, actual.Index); Span(unary.OperatorToken.Span, indirect.MultiplyOp.Span); Compare(unary.Operand, indirect.Value); }
                else if (unary.IsKind(SyntaxKind.AddressOfExpression))
                { Kind("AddressExpression"); var address = SyntaxView.Cast<AddressExpressionExpression>(tree, actual.Index); Span(unary.OperatorToken.Span, address.BitAndOp.Span); Compare(unary.Operand, address.Value); }
                else
                { Kind("UnaryExpression"); var u = SyntaxView.Cast<UnaryExpressionExpression>(tree, actual.Index); Span(unary.OperatorToken.Span, u.Operator.Span); Assert.Equal(unary.OperatorToken.Text, tree.Text.Substring(u.Operator.Span.Start, u.Operator.Span.Length)); Compare(unary.Operand, u.Value); }
                break;
            case PostfixUnaryExpressionSyntax postfix:
                if (postfix.IsKind(SyntaxKind.PostIncrementExpression))
                { Kind("PostfixIncrementExpression"); Compare(postfix.Operand, SyntaxView.Cast<PostfixIncrementExpressionExpression>(tree, actual.Index).Value); }
                else
                { Kind("PostfixDecrementExpression"); Compare(postfix.Operand, SyntaxView.Cast<PostfixDecrementExpressionExpression>(tree, actual.Index).Value); }
                break;
            case MemberAccessExpressionSyntax pointerMember when pointerMember.IsKind(SyntaxKind.PointerMemberAccessExpression):
                Kind("PointerMemberExpression"); var pointer = SyntaxView.Cast<PointerMemberExpressionExpression>(tree, actual.Index);
                Compare(pointerMember.Expression, pointer.Target); Span(pointerMember.Name.Span, pointer.Name.Span); Assert.Equal(pointerMember.Name.ToString(), pointer.Name.ToString());
                Span(pointerMember.OperatorToken.Span, new Nitrogen.SyntaxNode(tree, actual.Index).Child(1).Span); break;
            case MemberAccessExpressionSyntax member when member.Expression is BaseExpressionSyntax:
                Kind("BaseMemberExpression"); Span(member.Name.Span, SyntaxView.Cast<BaseMemberExpressionExpression>(tree, actual.Index).Name.Span); Assert.Equal(member.Name.ToString(), SyntaxView.Cast<BaseMemberExpressionExpression>(tree, actual.Index).Name.ToString()); break;
            case MemberAccessExpressionSyntax member:
                Kind("MemberExpression"); var m = SyntaxView.Cast<MemberExpressionExpression>(tree, actual.Index);
                Compare(member.Expression, m.Target); Span(member.Name.Span, m.Name.Span); Assert.Equal(member.Name.ToString(), m.Name.ToString()); break;
            case InvocationExpressionSyntax invocation:
                Kind("InvocationExpression"); var call = SyntaxView.Cast<InvocationExpressionExpression>(tree, actual.Index);
                Compare(invocation.Expression, call.Target); CompareArguments(invocation.ArgumentList, call.Args); break;
            case ElementAccessExpressionSyntax element:
                Kind("ElementExpression"); var e = SyntaxView.Cast<ElementExpressionExpression>(tree, actual.Index);
                Compare(element.Expression, e.Target); Assert.Equal(element.ArgumentList.Arguments.Count, e.Indices.Count);
                for (int i = 0; i < e.Indices.Count; i++) Compare(element.ArgumentList.Arguments[i].Expression, e.Indices[i]); break;
            case ObjectCreationExpressionSyntax creation:
                Kind("ObjectCreationExpression"); var obj = SyntaxView.Cast<ObjectCreationExpressionExpression>(tree, actual.Index);
                Type(creation.Type, obj.Type.Span); CompareArguments(creation.ArgumentList!, obj.Args); break;
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
                if(sizeOf.Type is PredefinedTypeSyntax)
                { Kind("SizeOfBuiltinExpression"); Type(sizeOf.Type, SyntaxView.Cast<SizeOfBuiltinExpressionExpression>(tree, actual.Index).Type.Span); }
                else { Kind("SizeOfExtendedExpression"); Type(sizeOf.Type, SyntaxView.Cast<SizeOfExtendedExpressionExpression>(tree, actual.Index).Type.Span); }
                break;
            case StackAllocArrayCreationExpressionSyntax stack:
                Kind("StackAllocExpression"); var allocated=SyntaxView.Cast<StackAllocExpressionExpression>(tree,actual.Index);
                Assert.Null(stack.Initializer); var allocatedType=Assert.IsType<ArrayTypeSyntax>(stack.Type);
                Type(allocatedType.ElementType,allocated.Type.Span); Compare(Assert.Single(Assert.Single(allocatedType.RankSpecifiers).Sizes),allocated.Size); break;
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
