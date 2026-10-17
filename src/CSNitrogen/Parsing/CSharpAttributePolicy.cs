using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

/// <summary>Attribute argument restrictions that depend only on generated syntax.</summary>
internal static class CSharpAttributePolicy
{
    public static IReadOnlyList<ParseIssue> Check(SyntaxTree tree,Version version)
    {
        var issues = new List<ParseIssue>();
        var namedOwners = new HashSet<int>();
        string Kind(int node) => tree.Language!.GetKindName(tree.Kind(node));

        // Frozen syntax nodes are visited in source preorder. Each attribute owns
        // its ordering state; a named argument must not affect the next attribute.
        for (int node = 0; node < tree.NodeCount; node++)
        {
            string kind = Kind(node);
            if (UnicodePolicy.IsHistorical(version) && kind == "AttributeSection" && tree.ChildCount(node) >= 5)
            {
                var section=SyntaxView.Cast<AttributeSectionNode>(tree,node);
                if(section.Target.HasValue && section.Target.Value.ChildCount>0)
                {
                    var target=section.Target.Value.Child(0);
                    bool recognized=UnicodePolicy.TryNormalize(target.ToString(),version,true,out var name)
                        && name is "field" or "event" or "method" or "param" or "property" or "return" or "type";
                    if(!recognized)issues.Add(new("CSN2014","Unrecognized attribute target; applicability is deferred to the semantic consumer.",
                        target.Span,IssueCategory.Syntax,DiagnosticSeverity.Warning));
                }
            }
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
            if (!UnicodePolicy.IsHistorical(version) && Kind(value) == "AssignmentExpression")
                issues.Add(new("CSN2006", "An attribute argument requires a non-assignment expression.",
                    tree.Span(value), IssueCategory.Syntax));
        }
        return issues.AsReadOnly();
    }
}
