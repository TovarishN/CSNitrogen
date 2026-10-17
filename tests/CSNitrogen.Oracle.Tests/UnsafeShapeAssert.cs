using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using Node = Nitrogen.SyntaxNode;

namespace CSNitrogen.Oracle.Tests;

internal static class UnsafeShapeAssert
{
    internal static void NonArrayPointer(TypeSyntax expected, PointerNonArrayTypeNode actual)
    {
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        var stars = new List<Microsoft.CodeAnalysis.SyntaxToken>();
        while (expected is PointerTypeSyntax pointer) { stars.Insert(0, pointer.AsteriskToken); expected = pointer.ElementType; }
        ParameterShapeAssert.Text(expected, actual.Element);
        Assert.Equal(stars.Count, actual.Stars.Count);
        for (int i = 0; i < stars.Count; i++) ParameterShapeAssert.Span(stars[i].Span, actual.Stars[i].Span);
    }

    internal static void PointerCast(TypeSyntax expected, PointerCastTypeNode actual)
    {
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        if (expected is ArrayTypeSyntax array)
        {
            NonArrayPointer(array.ElementType, actual.Type); Assert.Equal(array.RankSpecifiers.Count, actual.Ranks.Count);
            for (int r = 0; r < actual.Ranks.Count; r++)
            {
                ParameterShapeAssert.Span(array.RankSpecifiers[r].Span, actual.Ranks[r].Span);
                var commas = array.RankSpecifiers[r].Sizes.GetSeparators().ToArray(); Assert.Equal(commas.Length, actual.Ranks[r].Commas.Count);
                for (int c = 0; c < commas.Length; c++) ParameterShapeAssert.Span(commas[c].Span, actual.Ranks[r].Commas[c].Span);
            }
        }
        else { Assert.Equal(0, actual.Ranks.Count); NonArrayPointer(expected, actual.Type); }
    }

    internal static void PointerBase(TypeSyntax expected, TypeBaseNode actual)
    {
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        if (expected is PointerTypeSyntax pointer)
        {
            if (pointer.ElementType is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
            {
                var p = SyntaxView.Cast<VoidPointerTypeNameTypeBase>(actual.Tree, actual.Index);
                ParameterShapeAssert.Span(predefined.Span, new Node(p.Tree, p.Index).Child(0).Span);
            }
            else PointerBase(pointer.ElementType, SyntaxView.Cast<PointerTypeNameTypeBase>(actual.Tree, actual.Index).Target);
            ParameterShapeAssert.Span(pointer.AsteriskToken.Span, new Node(actual.Tree, actual.Index).Child(1).Span);
        }
        else if (expected is PredefinedTypeSyntax) ParameterShapeAssert.Text(expected, new Node(actual.Tree, SyntaxView.Cast<BuiltinTypeNameTypeBase>(actual.Tree, actual.Index).Value.Index));
        else ParameterShapeAssert.Text(expected, new Node(actual.Tree, SyntaxView.Cast<NamedTypeNameTypeBase>(actual.Tree, actual.Index).Name.Index));
    }
}
