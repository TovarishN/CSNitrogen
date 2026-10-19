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
            if (version.Major<=3 && (kind == "AttributeSection" || version.Major is 2 or 3 && kind=="GlobalAttributeSection") && tree.ChildCount(node) >= 5)
            {
                int targetIndex;
                if(kind=="GlobalAttributeSection")targetIndex=SyntaxView.Cast<GlobalAttributeSectionNode>(tree,node).Target.Index;
                else
                {
                    var section=SyntaxView.Cast<AttributeSectionNode>(tree,node);
                    targetIndex=section.Target.HasValue?section.Target.Value.Index:-1;
                }
                if(targetIndex>=0 && tree.ChildCount(targetIndex)>0)
                {
                    int leaf=tree.Child(targetIndex,0);
                    while(leaf!=SyntaxTree.NoNode && tree.ChildCount(leaf)>0)leaf=tree.Child(leaf,0);
                    if(leaf==SyntaxTree.NoNode)continue;
                    var targetSpan=tree.Span(leaf);
                    string targetText=tree.Text.Substring(targetSpan.Start,targetSpan.Length);
                    bool recognized=UnicodePolicy.TryNormalize(targetText,version,true,out var name)
                        && name is "field" or "event" or "method" or "param" or "property" or "return" or "type" || version.Major==2 && name=="typevar";
                    if(kind=="GlobalAttributeSection")recognized=name is "assembly" or "module";
                    if(version.Major==3 && name is "event" or "return" && targetText!=name)recognized=false;
                    if(!recognized)issues.Add(new("CSN2014","Unrecognized attribute target; applicability is deferred to the semantic consumer.",
                        targetSpan,IssueCategory.Syntax,version.Major==3?DiagnosticSeverity.Error:DiagnosticSeverity.Warning));
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
            if (version.Major>3 && Kind(value) == "AssignmentExpression")
                issues.Add(new("CSN2006", "An attribute argument requires a non-assignment expression.",
                    tree.Span(value), IssueCategory.Syntax));
        }
        return issues.AsReadOnly();
    }
}
