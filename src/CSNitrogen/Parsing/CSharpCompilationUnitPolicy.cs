using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Parsing;

/// <summary>Source placement constraints that distinguish global attributes from type attributes.</summary>
internal static class CSharpCompilationUnitPolicy
{
    internal static IReadOnlyList<ParseIssue> Check(SyntaxTree tree, Version version)
    {
        var issues = new List<ParseIssue>();
        for (int index = 0; index < tree.NodeCount; index++)
        {
            if (!NamespaceMemberBodyNamespaceMember.Is(tree, index) || tree.ChildCount(index) == 0) continue;
            var value = SyntaxView.Cast<NamespaceMemberBodyNamespaceMember>(tree, index).Value;
            // Namespace members wrap either a namespace or a type alternative.
            // Inspect only type alternatives: nested class members and method/
            // parameter/accessor attributes keep their applicability semantics.
            bool type = ClassTypeDeclarationTypeDeclaration.Is(tree, value.Index)
                || StructTypeDeclarationTypeDeclaration.Is(tree, value.Index)
                || InterfaceTypeDeclarationTypeDeclaration.Is(tree, value.Index)
                || EnumTypeDeclarationTypeDeclaration.Is(tree, value.Index)
                || DelegateTypeDeclarationTypeDeclaration.Is(tree, value.Index);
            if (!type || tree.ChildCount(value.Index) == 0) continue;
            int declaration = tree.Child(value.Index, 0);
            if (tree.ChildCount(declaration) == 0) continue; // recovered/missing declaration
            if (ClassDeclarationNode.Is(tree, declaration)) CheckSections(SyntaxView.Cast<ClassDeclarationNode>(tree, declaration).Attributes);
            else if (StructDeclarationNode.Is(tree, declaration)) CheckSections(SyntaxView.Cast<StructDeclarationNode>(tree, declaration).Attributes);
            else if (InterfaceDeclarationNode.Is(tree, declaration)) CheckSections(SyntaxView.Cast<InterfaceDeclarationNode>(tree, declaration).Attributes);
            else if (EnumDeclarationNode.Is(tree, declaration)) CheckSections(SyntaxView.Cast<EnumDeclarationNode>(tree, declaration).Attributes);
            else if (DelegateDeclarationNode.Is(tree, declaration)) CheckSections(SyntaxView.Cast<DelegateDeclarationNode>(tree, declaration).Attributes);
        }
        return issues.AsReadOnly();

        void CheckSections(SyntaxList<AttributeSectionNode> sections)
        {
            foreach (var section in sections)
            {
                if (tree.ChildCount(section.Index) < 5 || !section.Target.HasValue) continue;
                var targetGroup = section.Target.Value;
                if (targetGroup.ChildCount == 0) continue;
                var target = targetGroup.Child(0);
                if (UnicodePolicy.TryNormalize(target.ToString(), version, true, out string name)
                    && name is "assembly" or "module")
                    issues.Add(new("CSN2012", "Global assembly/module attributes must appear at compilation-unit scope before namespace and type declarations.",
                        target.Span, IssueCategory.Syntax));
            }
        }
    }
}
