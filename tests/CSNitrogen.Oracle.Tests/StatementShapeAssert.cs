using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using Node = Nitrogen.SyntaxNode;

namespace CSNitrogen.Oracle.Tests;

internal static class StatementShapeAssert
{
    private static void Span(Microsoft.CodeAnalysis.Text.TextSpan expected, Nitrogen.TextSpan actual)
    { Assert.Equal(expected.Start, actual.Start); Assert.Equal(expected.Length, actual.Length); }

    internal static void Compare(StatementSyntax expected, Node actual)
    {
        Span(expected.Span, actual.Span);
        var tree = actual.Tree;
        // StandardEmbeddedStatement and UnsafeEmbeddedStatement are transparent: the
        // embedded statement slot already holds the statement node.
        while (StandardStatementStatement.Is(tree, actual.Index))
            actual = SyntaxView.Cast<StandardStatementStatement>(tree, actual.Index).Body;
        Span(expected.Span, actual.Span);
        int n = actual.Index;
        void Body(StatementSyntax statement, int index) => Compare(statement, new Node(tree, index));
        switch (expected)
        {
            case UnsafeStatementSyntax unsafeStatement:
                var unsafeBlock=SyntaxView.Cast<UnsafeBlockNode>(tree,n); Body(unsafeStatement.Block,unsafeBlock.Body.Index); break;
            case FixedStatementSyntax fixedStatement:
                var pinned=SyntaxView.Cast<FixedStatementNode>(tree,n); UnsafeShapeAssert.NonArrayPointer(fixedStatement.Declaration.Type,pinned.Type);
                Assert.Equal(fixedStatement.Declaration.Variables.Count,pinned.Variables.Count);
                var commas=fixedStatement.Declaration.Variables.GetSeparators().ToArray();Assert.Equal(commas.Length,pinned.Variables.SeparatorCount);
                for(int i=0;i<pinned.Variables.Count;i++)
                {
                    var reference=fixedStatement.Declaration.Variables[i];var variable=pinned.Variables[i];Span(reference.Span,variable.Span);
                    Span(reference.Identifier.Span,variable.Name.Span);Assert.Equal(reference.Identifier.Text,variable.Name.ToString());
                    Assert.NotNull(reference.Initializer); ExpressionShapeAssert.Compare(reference.Initializer.Value,variable.Value);
                    if(i<commas.Length)Span(commas[i].Span,pinned.Variables.Separator(i).Span);
                }
                Body(fixedStatement.Statement,pinned.Body.Index);break;
            case BlockSyntax block:
                var b = SyntaxView.Cast<BlockNode>(tree, n); Assert.Equal(block.Statements.Count, b.Statements.Count);
                for (int i = 0; i < b.Statements.Count; i++) Body(block.Statements[i], b.Statements[i].Index);
                break;
            case EmptyStatementSyntax:
                Assert.Equal(";", actual.ToString()); Assert.Equal(0, actual.ChildCount); break;
            case ExpressionStatementSyntax expression:
                ExpressionShapeAssert.Compare(expression.Expression, SyntaxView.Cast<ExpressionStatementNode>(tree, n).Value.Value); break;
            case LocalDeclarationStatementSyntax declaration:
                Assert.Equal(SyntaxKinds.Group, actual.Kind); Assert.Equal(2, actual.ChildCount);
                Assert.Equal(";", actual.Child(1).ToString());
                if (declaration.IsConst)
                { Assert.Equal(declaration.Modifiers[0].SpanStart, actual.Child(0).Span.Start); Assert.Equal(declaration.Declaration.Span.End, actual.Child(0).Span.End); }
                CompareDeclaration(declaration.Declaration, actual.Child(0), declaration.IsConst); break;
            case IfStatementSyntax conditional:
                var c = SyntaxView.Cast<IfStatementNode>(tree, n);
                ExpressionShapeAssert.Compare(conditional.Condition, c.Condition); Body(conditional.Statement, c.Body.Index);
                Assert.Equal(conditional.Else is not null, c.Else.HasValue);
                if (conditional.Else is not null)
                {
                    var group = c.Else.Value; Assert.Equal(SyntaxKinds.Group, group.Kind); Assert.Equal(2, group.ChildCount);
                    Span(conditional.Else.Span, group.Span); Assert.Equal("else", group.Child(0).ToString());
                    Body(conditional.Else.Statement, group.Child(1).Index);
                }
                break;
            case WhileStatementSyntax loop:
                var w = SyntaxView.Cast<WhileStatementNode>(tree, n); ExpressionShapeAssert.Compare(loop.Condition, w.Condition); Body(loop.Statement, w.Body.Index); break;
            case DoStatementSyntax loop:
                var d = SyntaxView.Cast<DoStatementNode>(tree, n); ExpressionShapeAssert.Compare(loop.Condition, d.Condition); Body(loop.Statement, d.Body.Index); break;
            case ForStatementSyntax loop:
                var f = SyntaxView.Cast<ForStatementNode>(tree, n);
                Assert.Equal(loop.Declaration is not null || loop.Initializers.Count > 0, f.Initializer.HasValue);
                if (loop.Declaration is not null) CompareDeclaration(loop.Declaration, f.Initializer.Value, false);
                else if (loop.Initializers.Count > 0) Expressions(loop.Initializers, SyntaxView.Cast<StatementExpressionListNode>(tree, f.Initializer.Value.Index));
                Assert.Equal(loop.Condition is not null, f.Condition.HasValue);
                if (loop.Condition is not null) ExpressionShapeAssert.Compare(loop.Condition, f.Condition.Value);
                Assert.Equal(loop.Incrementors.Count > 0, f.Iterator.HasValue);
                if (f.Iterator.HasValue) Expressions(loop.Incrementors, f.Iterator.Value);
                Body(loop.Statement, f.Body.Index); break;
            case ForEachStatementSyntax loop:
                var fe = SyntaxView.Cast<ForeachStatementNode>(tree, n); Span(loop.Type.Span, fe.Type.Span);
                Assert.Equal(loop.Type.ToString(), tree.GetText(fe.Type.Index).ToString());
                Span(loop.Identifier.Span, fe.Name.Span); Assert.Equal(loop.Identifier.Text, fe.Name.ToString());
                ExpressionShapeAssert.Compare(loop.Expression, fe.Source); Body(loop.Statement, fe.Body.Index); break;
            case UsingStatementSyntax resource:
                var u = SyntaxView.Cast<UsingStatementNode>(tree, n);
                if (resource.Declaration is not null) CompareDeclaration(resource.Declaration, u.Resource, false);
                else ExpressionShapeAssert.Compare(resource.Expression!, new ExpressionNode(tree, u.Resource.Index));
                Body(resource.Statement, u.Body.Index); break;
            case LockStatementSyntax locked:
                var l = SyntaxView.Cast<LockStatementNode>(tree, n); ExpressionShapeAssert.Compare(locked.Expression, l.Value); Body(locked.Statement, l.Body.Index); break;
            case CheckedStatementSyntax checkedBlock:
                var ch = SyntaxView.Cast<CheckedStatementNode>(tree, n);
                Assert.Equal(checkedBlock.Keyword.Text, actual.Child(0).ToString()); Body(checkedBlock.Block, ch.Body.Index); break;
            case LabeledStatementSyntax labeled:
                var label = SyntaxView.Cast<LabeledStatementNode>(tree, n); Span(labeled.Identifier.Span, label.Name.Span);
                Assert.Equal(labeled.Identifier.Text, label.Name.ToString()); Body(labeled.Statement, label.Body.Index); break;
            case SwitchStatementSyntax selection:
                var sw = SyntaxView.Cast<SwitchStatementNode>(tree, n); ExpressionShapeAssert.Compare(selection.Expression, sw.Value);
                Assert.Equal(selection.Sections.Count, sw.Sections.Count);
                for (int i = 0; i < sw.Sections.Count; i++)
                {
                    var section = sw.Sections[i]; var reference = selection.Sections[i]; Span(reference.Span, section.Span);
                    Assert.Equal(reference.Labels.Count, section.Labels.Count); Assert.Equal(reference.Statements.Count, section.Statements.Count);
                    for (int j = 0; j < section.Labels.Count; j++)
                    {
                        var item = section.Labels[j]; Span(reference.Labels[j].Span, item.Span);
                        var group = new Node(tree, tree.Child(item.Index, 0)); Assert.Equal(SyntaxKinds.Group, group.Kind);
                        Assert.Equal(reference.Labels[j].Keyword.Text, group.Child(0).ToString());
                        Span(reference.Labels[j].Keyword.Span, group.Child(0).Span);
                        if (reference.Labels[j] is CaseSwitchLabelSyntax @case)
                        { Assert.Equal(3, group.ChildCount); ExpressionShapeAssert.Compare(@case.Value, new ExpressionNode(tree, group.Child(1).Index)); }
                        else { Assert.IsType<DefaultSwitchLabelSyntax>(reference.Labels[j]); Assert.Equal(2, group.ChildCount); }
                    }
                    for (int j = 0; j < section.Statements.Count; j++) Body(reference.Statements[j], section.Statements[j].Index);
                }
                break;
            case TryStatementSyntax guarded:
                var tr = SyntaxView.Cast<TryStatementNode>(tree, n); Body(guarded.Block, tr.Body.Index);
                var handlers = new List<Node>();
                void Collect(Node node)
                {
                    if (node.IsNull) { } // absent optional
                    else if (SpecificCatchClauseNode.Is(tree, node.Index)) handlers.Add(new Node(tree, SyntaxView.Cast<SpecificCatchClauseNode>(tree, node.Index).Value.Index));
                    else if (GeneralCatchClauseNode.Is(tree, node.Index)) handlers.Add(new Node(tree, SyntaxView.Cast<GeneralCatchClauseNode>(tree, node.Index).Value.Index));
                    else if (FinallyClauseNode.Is(tree, node.Index)) handlers.Add(node);
                    else if (node.Kind is SyntaxKinds.Group or SyntaxKinds.List) { foreach (var child in node.Children) Collect(child); }
                    else Assert.Fail("unexpected handler child " + node.Kind);
                }
                Collect(tr.Handlers); Assert.Equal(guarded.Catches.Count + (guarded.Finally is null ? 0 : 1), handlers.Count);
                for (int i = 0; i < guarded.Catches.Count; i++)
                {
                    var reference = guarded.Catches[i]; var ca = SyntaxView.Cast<CatchClauseNode>(tree, handlers[i].Index);
                    Span(reference.Span, ca.Span); Assert.Equal(reference.Declaration is not null, ca.Declaration.HasValue);
                    if (reference.Declaration is not null)
                    {
                        var group = ca.Declaration.Value; Span(reference.Declaration.Span, group.Span);
                        Assert.Equal(4, group.ChildCount); Span(reference.Declaration.Type.Span, group.Child(1).Span);
                        Assert.Equal(reference.Declaration.Type.ToString(), group.Child(1).ToString());
                        Assert.Equal(reference.Declaration.Identifier.RawKind != 0, !group.Child(2).IsNull);
                        if (reference.Declaration.Identifier.RawKind != 0)
                        { Span(reference.Declaration.Identifier.Span, group.Child(2).Span); Assert.Equal(reference.Declaration.Identifier.Text, group.Child(2).ToString()); }
                    }
                    Body(reference.Block, ca.Body.Index);
                }
                if (guarded.Finally is not null)
                { var fin = SyntaxView.Cast<FinallyClauseNode>(tree, handlers[^1].Index); Span(guarded.Finally.Span, fin.Span); Body(guarded.Finally.Block, fin.Body.Index); }
                break;
            case ReturnStatementSyntax returned: Jump(actual, "return", returned.Expression); break;
            case ThrowStatementSyntax thrown: Jump(actual, "throw", thrown.Expression); break;
            case BreakStatementSyntax: Jump(actual, "break", null); break;
            case ContinueStatementSyntax: Jump(actual, "continue", null); break;
            case GotoStatementSyntax jump:
                var payload = JumpPayload(actual); Assert.Equal("goto", payload.Child(0).ToString());
                var target = payload.Child(1);
                if (jump.IsKind(SyntaxKind.GotoCaseStatement))
                { Assert.Equal(SyntaxKinds.Group, target.Kind); Assert.Equal("case", target.Child(0).ToString()); ExpressionShapeAssert.Compare(jump.Expression!, new ExpressionNode(tree, target.Child(1).Index)); }
                else { Assert.Equal(jump.IsKind(SyntaxKind.GotoDefaultStatement) ? "default" : jump.Expression!.ToString(), target.ToString()); }
                break;
            default: Assert.Fail("Uncompared statement kind: " + expected.Kind()); break;
        }
    }

    private static void Expressions(SeparatedSyntaxList<ExpressionSyntax> expected, StatementExpressionListNode actual)
    {
        Assert.Equal(expected.Count, actual.Items.Count); var separators = expected.GetSeparators().ToArray();
        Assert.Equal(separators.Length, actual.Items.SeparatorCount);
        for (int i = 0; i < actual.Items.Count; i++)
        {
            Span(expected[i].Span, actual.Items[i].Span); ExpressionShapeAssert.Compare(expected[i], actual.Items[i].Value);
            if (i < separators.Length) Span(separators[i].Span, actual.Items.Separator(i).Span);
        }
    }

    internal static void CompareDeclaration(VariableDeclarationSyntax expected, Node actual, bool constant)
    {
        if (!constant) Span(expected.Span, actual.Span);
        TypeReferenceNode type; int count;
        if (constant)
        { var c = SyntaxView.Cast<ConstantDeclarationNode>(actual.Tree, actual.Index); type = c.Type; count = c.Variables.Count; }
        else
        { var v = SyntaxView.Cast<VariableDeclarationNode>(actual.Tree, actual.Index); type = v.Type; count = v.Variables.Count; }
        Span(expected.Type.Span, type.Span); Assert.Equal(expected.Type.ToString(), actual.Tree.GetText(type.Index).ToString());
        Assert.Equal(expected.Variables.Count, count);
        var separators = expected.Variables.GetSeparators().ToArray();
        int separatorCount = constant
            ? SyntaxView.Cast<ConstantDeclarationNode>(actual.Tree, actual.Index).Variables.SeparatorCount
            : SyntaxView.Cast<VariableDeclarationNode>(actual.Tree, actual.Index).Variables.SeparatorCount;
        Assert.Equal(separators.Length, separatorCount);
        for (int i = 0; i < separatorCount; i++)
        {
            var separator = constant
                ? SyntaxView.Cast<ConstantDeclarationNode>(actual.Tree, actual.Index).Variables.Separator(i)
                : SyntaxView.Cast<VariableDeclarationNode>(actual.Tree, actual.Index).Variables.Separator(i);
            Span(separators[i].Span, separator.Span);
        }
        for (int i = 0; i < count; i++)
        {
            var reference = expected.Variables[i]; Token name;
            if (constant)
            { var c = SyntaxView.Cast<ConstantDeclarationNode>(actual.Tree, actual.Index).Variables[i]; name = c.Name; Span(reference.Span, c.Span); ExpressionShapeAssert.Compare(reference.Initializer!.Value, c.Value); }
            else
            {
                var v = SyntaxView.Cast<VariableDeclarationNode>(actual.Tree, actual.Index).Variables[i]; name = v.Name;
                Span(reference.Span, v.Span); Assert.Equal(reference.Initializer is not null, v.Initializer.HasValue);
                if (reference.Initializer is not null)
                {
                    var group = v.Initializer.Value; Span(reference.Initializer.Span, group.Span); Assert.Equal(2, group.ChildCount);
                    Assert.Equal("=", group.Child(0).ToString());
                    if (reference.Initializer.Value is InitializerExpressionSyntax array)
                        ExpressionShapeAssert.CompareInitializer(array, SyntaxView.Cast<ArrayInitializerNode>(actual.Tree, group.Child(1).Index));
                    else ExpressionShapeAssert.Compare(reference.Initializer.Value, new ExpressionNode(actual.Tree, group.Child(1).Index));
                }
            }
            Span(reference.Identifier.Span, name.Span); Assert.Equal(reference.Identifier.Text, name.ToString());
        }
    }

    private static Node JumpPayload(Node actual)
    {
        Assert.True(JumpStatementNode.Is(actual.Tree, actual.Index)); var payload = actual.Child(0);
        while (payload.Kind == SyntaxKinds.Group && payload.ChildCount == 1) payload = payload.Child(0);
        Assert.Equal(SyntaxKinds.Group, payload.Kind); return payload;
    }

    private static void Jump(Node actual, string keyword, ExpressionSyntax? value)
    {
        var payload = JumpPayload(actual); Assert.Equal(keyword, payload.Child(0).ToString());
        if (keyword is "break" or "continue") { Assert.Equal(2, payload.ChildCount); return; }
        Assert.Equal(3, payload.ChildCount); Assert.Equal(value is not null, !payload.Child(1).IsNull);
        if (value is not null) ExpressionShapeAssert.Compare(value, new ExpressionNode(actual.Tree, payload.Child(1).Index));
    }
}
