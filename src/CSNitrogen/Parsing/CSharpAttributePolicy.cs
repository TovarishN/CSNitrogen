using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

/// <summary>Attribute argument restrictions that depend only on generated syntax.</summary>
internal static class CSharpAttributePolicy
{
    public static IReadOnlyList<ParseIssue> Check(SyntaxTree tree)
    {
        var issues = new List<ParseIssue>();
        var namedOwners = new HashSet<int>();
        string Kind(int node) => tree.Language!.GetKindName(tree.Kind(node));

        // Frozen syntax nodes are visited in source preorder. Each attribute owns
        // its ordering state; a named argument must not affect the next attribute.
        for (int node = 0; node < tree.NodeCount; node++)
        {
            string kind = Kind(node);
            bool named = kind == "NamedAttributeArgument";
            if (!named && kind != "PositionalAttributeArgument") continue;
            if (tree.ChildCount(node) < (named ? 3 : 1)) continue; // recovered node

            int owner = tree.Parent(node);
            while (owner >= 0 && Kind(owner) != "Attribute") owner = tree.Parent(owner);
            if (owner >= 0)
            {
                if (named) namedOwners.Add(owner);
                else if (namedOwners.Contains(owner))
                    issues.Add(new("CSN2005", "Positional attribute arguments must precede named arguments.",
                        tree.Span(node), IssueCategory.Syntax));
            }

            int value = named
                ? SyntaxView.Cast<NamedAttributeArgumentNode>(tree, node).Value.Index
                : SyntaxView.Cast<PositionalAttributeArgumentNode>(tree, node).Value.Index;
            if (Kind(value) == "AssignmentExpression")
                issues.Add(new("CSN2006", "An attribute argument requires a non-assignment expression.",
                    tree.Span(value), IssueCategory.Syntax));
        }
        return issues.AsReadOnly();
    }
}
