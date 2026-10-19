using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using Node = Nitrogen.SyntaxNode;

namespace CSNitrogen.Oracle.Tests;

internal static class CompilationUnitShapeAssert
{
    private static Node N<T>(T view) where T : struct, ISyntaxView<T> => new(view.Tree, view.Index);
    private static void Name(SyntaxToken expected, Token actual)
    {
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        Assert.Equal(expected.Text, actual.ToString());
    }

    internal static void Compare(CompilationUnitSyntax expected, ParseDocument actual)
    {
        Usings(expected.Usings, actual.Root.Usings);
        Assert.Equal(expected.AttributeLists.Count, actual.Root.Attributes.Count);
        for (int i = 0; i < actual.Root.Attributes.Count; i++)
        {
            var section = actual.Root.Attributes[i];
            var reference = expected.AttributeLists[i];
            ParameterShapeAssert.Span(reference.Span, section.Span);
            Assert.NotNull(reference.Target);
            ParameterShapeAssert.Span(reference.Target.Identifier.Span, section.Target.Span);
            Assert.Equal(reference.Target.Identifier.Text, N(section.Target).ToString());
            ParameterShapeAssert.AttributeItems(reference, section.Items, N(section).Child(4));
        }
        Members(expected.Members, actual.Root.Members);
        var classes = expected.Members.OfType<ClassDeclarationSyntax>().ToArray();
        Assert.Equal(classes.Length, actual.Classes.Count);
        for (int i = 0; i < classes.Length; i++)
        {
            Name(classes[i].Identifier, actual.Classes[i].Name);
            ParameterShapeAssert.Span(classes[i].Span, actual.Classes[i].Span);
            var member = expected.Members.IndexOf(classes[i]);
            var alias = new Node(actual.Tree, actual.Root.Members[member].Index); // NamespaceMemberBody is transparent
            Assert.Equal(SyntaxView.Cast<ClassTypeDeclarationTypeDeclaration>(actual.Tree, alias.Index).Value.Index, actual.Classes[i].Index);
        }
    }

    private static void Usings(Microsoft.CodeAnalysis.SyntaxList<UsingDirectiveSyntax> expected, Nitrogen.SyntaxList<UsingDirectiveNode> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++)
        {
            var item = actual[i]; var reference = expected[i];
            ParameterShapeAssert.Span(reference.Span, item.Span);
            ParameterShapeAssert.Text(reference.Name!, N(item.Name));
            Assert.Equal(reference.Alias is not null, item.Alias.HasValue);
            if (reference.Alias is not null)
            {
                var alias = item.Alias.Value;
                ParameterShapeAssert.Span(reference.Alias.Span, alias.Span);
                ParameterShapeAssert.Text(reference.Alias.Name, alias.Child(0));
                ParameterShapeAssert.Span(reference.Alias.EqualsToken.Span, alias.Child(1).Span);
            }
        }
    }

    private static void Members(Microsoft.CodeAnalysis.SyntaxList<MemberDeclarationSyntax> expected, Nitrogen.SyntaxList<NamespaceMemberNode> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++)
        {
            ParameterShapeAssert.Span(expected[i].Span, actual[i].Span);
            var value = new Node(actual[i].Tree, actual[i].Index); // NamespaceMemberBody is transparent
            if (expected[i] is NamespaceDeclarationSyntax ns)
            {
                var node = SyntaxView.Cast<NamespaceDeclarationNode>(value.Tree, value.Index);
                ParameterShapeAssert.Span(ns.Span, node.Span);
                ParameterShapeAssert.Text(ns.Name, N(node.Name));
                Usings(ns.Usings, node.Usings);
                Members(ns.Members, node.Members);
            }
            else Type(expected[i], value);
        }
    }

    private static void Type(MemberDeclarationSyntax expected, Node alias)
    {
        ParameterShapeAssert.Span(expected.Span, alias.Span);
        switch (expected)
        {
            case ClassDeclarationSyntax c:
                var classNode = SyntaxView.Cast<ClassTypeDeclarationTypeDeclaration>(alias.Tree, alias.Index).Value;
                Name(c.Identifier, classNode.Name);
                ParameterShapeAssert.Attributes(c.AttributeLists, classNode.Attributes);
                ClassMembers(c.Members, classNode.Members);
                break;
            case StructDeclarationSyntax s:
                var structNode = SyntaxView.Cast<StructTypeDeclarationTypeDeclaration>(alias.Tree, alias.Index).Value;
                Name(s.Identifier, structNode.Name);
                ParameterShapeAssert.Attributes(s.AttributeLists, structNode.Attributes);
                ClassMembers(s.Members, structNode.Members);
                break;
            case InterfaceDeclarationSyntax f:
                var interfaceNode = SyntaxView.Cast<InterfaceTypeDeclarationTypeDeclaration>(alias.Tree, alias.Index).Value;
                Name(f.Identifier, interfaceNode.Name);
                Assert.Equal(f.Members.Count, interfaceNode.Members.Count);
                for (int i = 0; i < f.Members.Count; i++)
                {
                    var member = interfaceNode.Members[i];
                    ParameterShapeAssert.Span(f.Members[i].Span, member.Span);
                    var method = Assert.IsType<MethodDeclarationSyntax>(f.Members[i]);
                    var payload = SyntaxView.Cast<InterfaceMethodNode>(member.Tree, member.Value.Index);
                    Name(method.Identifier, payload.Name);
                    ParameterShapeAssert.Text(method.ReturnType, N(payload.Return));
                    ParameterShapeAssert.Compare(method.ParameterList, N(payload.Parameters));
                }
                break;
            case EnumDeclarationSyntax e:
                var enumNode = SyntaxView.Cast<EnumTypeDeclarationTypeDeclaration>(alias.Tree, alias.Index).Value;
                Name(e.Identifier, enumNode.Name);
                Assert.Equal(e.Members.Count > 0, enumNode.Members.HasValue);
                if (enumNode.Members.HasValue)
                {
                    var items = new SeparatedList<EnumMemberNode>(alias.Tree, enumNode.Members.Value.Child(0).Index);
                    Assert.Equal(e.Members.Count, items.Count);
                    for (int i = 0; i < items.Count; i++) Name(e.Members[i].Identifier, items[i].Name);
                }
                break;
            case DelegateDeclarationSyntax d:
                var delegateNode = SyntaxView.Cast<DelegateTypeDeclarationTypeDeclaration>(alias.Tree, alias.Index).Value;
                Name(d.Identifier, delegateNode.Name);
                ParameterShapeAssert.Text(d.ReturnType, N(delegateNode.Return));
                ParameterShapeAssert.Compare(d.ParameterList, N(delegateNode.Parameters));
                break;
            default: Assert.Fail("Unexpected namespace type: " + expected.GetType().Name); break;
        }
    }

    private static void ClassMembers(Microsoft.CodeAnalysis.SyntaxList<MemberDeclarationSyntax> expected, Nitrogen.SyntaxList<ClassMemberNode> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++)
        {
            ParameterShapeAssert.Span(expected[i].Span, actual[i].Span);
            var value = new Node(actual[i].Tree, actual[i].Index); // StandardClassMember is transparent
            if (expected[i] is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax) { Type(expected[i], value); continue; }
            var member = SyntaxView.Cast<MemberDeclarationNode>(value.Tree, value.Index);
            ParameterShapeAssert.Attributes(expected[i].AttributeLists, member.Attributes);
            switch (expected[i])
            {
                case FieldDeclarationSyntax field:
                    var variables = SyntaxView.Cast<FieldDeclarationNode>(value.Tree, member.Body.Index).Value;
                    StatementShapeAssert.CompareDeclaration(field.Declaration, N(variables), false);
                    break;
                case MethodDeclarationSyntax method:
                    var payload = SyntaxView.Cast<MethodDeclarationNode>(value.Tree, member.Body.Index);
                    Name(method.Identifier, new Token(value.Tree, payload.Name.Index));
                    ParameterShapeAssert.Text(method.ReturnType, N(payload.Return));
                    ParameterShapeAssert.Compare(method.ParameterList, N(payload.Parameters));
                    StatementShapeAssert.Compare(method.Body!, N(payload.Body)); // MethodBody is an alias: the Block stands in
                    break;
                default: Assert.Fail("Unexpected representative member: " + expected[i].GetType().Name); break;
            }
        }
    }
}
