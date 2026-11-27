using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

internal static class CSharp7ThrowExpressionPolicy
{
    internal static void Check(SyntaxTree tree, int node, List<ParseIssue> issues)
    {
        var throwExpression = ThrowExpressionExpression.Create(tree, node);
        if (!AllowsThrowExpression(tree, node))
            issues.Add(new("CSN2070", "A throw expression must be a conditional arm, the right operand of ??, or an expression body.",
                throwExpression.Span, IssueCategory.Syntax));
        if (LambdaExpressionExpression.Is(tree, throwExpression.Operand.Index) ||
            QueryExpressionExpression.Is(tree, throwExpression.Operand.Index))
            issues.Add(new("CSN2071", "A throw expression operand must be a null-coalescing expression.",
                throwExpression.Operand.Span, IssueCategory.Syntax));
    }

    private static bool AllowsThrowExpression(SyntaxTree tree, int node)
    {
        int parent = tree.Parent(node);
        if (parent < 0) return false;
        if (ConditionalExpressionExpression.Is(tree, parent))
            return ConditionalExpressionExpression.Create(tree, parent).True.Index == node;
        if (ConditionalOperandNode.Is(tree, parent))
        {
            int conditional = tree.Parent(parent);
            return conditional >= 0 && ConditionalExpressionExpression.Is(tree, conditional) &&
                ConditionalExpressionExpression.Create(tree, conditional).False.Index == parent &&
                ConditionalOperandNode.Create(tree, parent).Value.Index == node;
        }
        if (NullCoalescingExpressionExpression.Is(tree, parent))
            return NullCoalescingExpressionExpression.Create(tree, parent).Right.Index == node;
        if (LambdaExpressionExpression.Is(tree, parent))
            return LambdaExpressionExpression.Create(tree, parent).Body.Index == node;
        return ExpressionClauseNode.Is(tree, parent) && ExpressionClauseNode.Create(tree, parent).Value.Index == node;
    }
}
