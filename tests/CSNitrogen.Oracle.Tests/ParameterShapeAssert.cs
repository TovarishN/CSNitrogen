using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using Node = Nitrogen.SyntaxNode;

namespace CSNitrogen.Oracle.Tests;

internal static class ParameterShapeAssert
{
    internal static void Span(Microsoft.CodeAnalysis.Text.TextSpan expected, Nitrogen.TextSpan actual)
    { Assert.Equal(expected.Start, actual.Start); Assert.Equal(expected.Length, actual.Length); }

    internal static void Text(Microsoft.CodeAnalysis.SyntaxNode expected, Node actual)
    { Span(expected.Span, actual.Span); Assert.Equal(expected.ToString(), actual.ToString()); }

    internal static void Attributes(Microsoft.CodeAnalysis.SyntaxList<AttributeListSyntax> expected, Nitrogen.SyntaxList<AttributeSectionNode> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++)
        {
            var section = actual[i]; var reference = expected[i]; Span(reference.Span, section.Span);
            Assert.Equal(reference.Target is not null, section.Target.HasValue);
            if (reference.Target is not null)
            { var target = section.Target.Value; Span(reference.Target.Span, target.Span); Span(reference.Target.Identifier.Span, target.Child(0).Span); }
            AttributeItems(reference, section.Items, new Node(section.Tree, section.Index).Child(3));
        }
    }

    internal static void AttributeItems(AttributeListSyntax reference, SeparatedList<AttributeNode> items, Node trailing)
    {
        Assert.Equal(reference.Attributes.Count, items.Count);
        var separators = reference.Attributes.GetSeparators().ToArray();
        Assert.Equal(separators.Length == reference.Attributes.Count, !trailing.IsNull);
        if (separators.Length == reference.Attributes.Count) Span(separators[^1].Span, trailing.Span);
        Assert.Equal(reference.Attributes.Count - 1, items.SeparatorCount);
        for (int j = 0; j < items.Count; j++)
        {
            var attribute = items[j]; var attr = reference.Attributes[j];
            Span(attr.Span, attribute.Span); Text(attr.Name, new Node(attribute.Tree, attribute.Name.Index));
            Assert.Equal(attr.ArgumentList is not null, attribute.Arguments.HasValue);
            if (attr.ArgumentList is not null)
            {
                var group = attribute.Arguments.Value; Span(attr.ArgumentList.Span, group.Span); Assert.Equal(3, group.ChildCount);
                var args = new SeparatedList<Node>(group.Tree, group.Child(1).Index);
                Assert.Equal(attr.ArgumentList.Arguments.Count, args.Count);
                var argSeparators = attr.ArgumentList.Arguments.GetSeparators().ToArray(); Assert.Equal(argSeparators.Length, args.SeparatorCount);
                for (int k = 0; k < args.Count; k++)
                {
                    var arg = attr.ArgumentList.Arguments[k]; Span(arg.Span, args[k].Span); Assert.Null(arg.NameColon);
                    if (arg.NameEquals is not null)
                    { var named = SyntaxView.Cast<NamedAttributeArgumentNode>(group.Tree, args[k].Index); Span(arg.NameEquals.Name.Span, named.Name.Span); ExpressionShapeAssert.Compare(arg.Expression, named.Value); }
                    else ExpressionShapeAssert.Compare(arg.Expression, SyntaxView.Cast<PositionalAttributeArgumentNode>(group.Tree, args[k].Index).Value);
                    if (k < argSeparators.Length) Span(argSeparators[k].Span, args.Separator(k).Span);
                }
            }
            if (j < items.SeparatorCount) Span(separators[j].Span, items.Separator(j).Span);
        }
    }

    internal static void Compare(BaseParameterListSyntax expected, Node actual)
    {
        Span(expected.Span, actual.Span);
        Node items;
        if (expected is ParameterListSyntax) items = new Node(actual.Tree, SyntaxView.Cast<ParameterListNode>(actual.Tree, actual.Index).Items.Index);
        else items = SyntaxView.Cast<IndexerParameterListNode>(actual.Tree, actual.Index).Items;
        var parameters = new List<Node>(); var separators = new List<Node>();
        void Collect(Node node)
        {
            if (node.IsNull) { } // absent optional or empty list
            else if (FixedParameterNode.Is(node.Tree, node.Index) || ParameterArrayNode.Is(node.Tree, node.Index)) parameters.Add(node);
            else if (node.Kind is SyntaxKinds.Group or SyntaxKinds.List || node.Tree.Language!.GetKindName(node.Kind) == "Parameters")
            { foreach (var child in node.Children) Collect(child); }
            else { Assert.Equal(",", node.ToString()); separators.Add(node); }
        }
        Collect(items); Assert.Equal(expected.Parameters.Count, parameters.Count);
        var commas = expected.Parameters.GetSeparators().ToArray(); Assert.Equal(commas.Length, separators.Count);
        for (int i = 0; i < separators.Count; i++) Span(commas[i].Span, separators[i].Span);
        for (int i = 0; i < parameters.Count; i++)
        {
            var reference = expected.Parameters[i]; var node = parameters[i]; Span(reference.Span, node.Span);
            Assert.Null(reference.Default); bool paramsArray = reference.Modifiers.Any(t => t.IsKind(SyntaxKind.ParamsKeyword));
            if (paramsArray)
            {
                var p = SyntaxView.Cast<ParameterArrayNode>(node.Tree, node.Index); Attributes(reference.AttributeLists, p.Attributes);
                var array = Assert.IsType<ArrayTypeSyntax>(reference.Type); Text(array.ElementType, new Node(node.Tree, p.Element.Index));
                Assert.Equal(array.RankSpecifiers.Count, p.Ranks.Count);
                for (int r = 0; r < p.Ranks.Count; r++)
                {
                    Span(array.RankSpecifiers[r].Span, p.Ranks[r].Span);
                    var rankCommas = array.RankSpecifiers[r].Sizes.GetSeparators().ToArray(); Assert.Equal(rankCommas.Length, p.Ranks[r].Commas.Count);
                    for (int c = 0; c < rankCommas.Length; c++) Span(rankCommas[c].Span, p.Ranks[r].Commas[c].Span);
                }
                Span(reference.Identifier.Span, p.Name.Span); Assert.Equal(reference.Identifier.Text, p.Name.ToString());
            }
            else
            {
                var p = SyntaxView.Cast<FixedParameterNode>(node.Tree, node.Index); Attributes(reference.AttributeLists, p.Attributes);
                Text(reference.Type!, new Node(node.Tree, p.Type.Index)); Span(reference.Identifier.Span, p.Name.Span);
                Assert.Equal(reference.Identifier.Text, p.Name.ToString()); Assert.Equal(reference.Modifiers.Count > 0, p.Mode.HasValue);
                if (p.Mode.HasValue) { Assert.Single(reference.Modifiers); Span(reference.Modifiers[0].Span, p.Mode.Value.Span); Assert.Equal(reference.Modifiers[0].Text, p.Mode.Value.ToString()); }
            }
        }
    }
}
