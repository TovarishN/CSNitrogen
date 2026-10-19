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
            // NamespaceMemberBody is transparent: a namespace member slot holds the
            // namespace or type alternative directly. Inspect only type alternatives
            // under a namespace member slot: nested class members and method/
            // parameter/accessor attributes keep their applicability semantics.
            // The kind checks come first: they are cheap, and most nodes fail them.
            bool type = ClassTypeDeclarationTypeDeclaration.Is(tree, index)
                || StructTypeDeclarationTypeDeclaration.Is(tree, index)
                || InterfaceTypeDeclarationTypeDeclaration.Is(tree, index)
                || EnumTypeDeclarationTypeDeclaration.Is(tree, index)
                || DelegateTypeDeclarationTypeDeclaration.Is(tree, index);
            if (!type || tree.ChildCount(index) == 0) continue;
            int parent = tree.Parent(index);
            while (parent >= 0 && tree.Kind(parent) is SyntaxKinds.List or SyntaxKinds.Ambiguous) parent = tree.Parent(parent);
            if (parent < 0 || !(CompilationUnitNode.Is(tree, parent) || NamespaceDeclarationNode.Is(tree, parent))) continue;
            int declaration = tree.Child(index, 0);
            if (declaration == SyntaxTree.NoNode || tree.ChildCount(declaration) == 0) continue; // recovered/missing declaration
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
