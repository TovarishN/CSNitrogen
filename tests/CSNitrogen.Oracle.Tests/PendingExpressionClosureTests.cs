using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

/// <summary>Finite alternatives/context closure against ECMA-334 first edition clauses 14, 19.6 and 25.</summary>
public class PendingExpressionClosureTests
{
    private const string Prefix = "unsafe class C{object M(){return ";
    private const string Suffix = ";}}";
    private static readonly string[] AssignmentOperators = ["=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>="];

    public static IEnumerable<object[]> PrimaryShapes()
    {
        foreach (var item in new (string Source, string Root)[]
        {
            ("this", "ThisExpression"), ("base[i]", "BaseIndexExpression"),
            ("base[i,j=k]", "BaseIndexExpression"), ("base.P", "BaseMemberExpression"),
            ("new D(F)", "ObjectCreationExpression"), ("new N.D(o.F)", "ObjectCreationExpression"),
            ("typeof(void)", "TypeOfExpression"), ("typeof(N.T[,])", "TypeOfExpression"),
            ("checked(a=b)", "CheckedExpression"), ("unchecked(a?b:c)", "UncheckedExpression"),
            ("a is T == b", "EqualExpression"), ("a as T == b", "EqualExpression"),
            ("a+b is N.T[]", "IsExpression"), ("a as T as U", "AsExpression"),
            ("a is T && b is U", "AndExpression"),
            ("(a+b)=c", "AssignmentExpression"), ("F()=c", "AssignmentExpression"),
            ("++a=c", "AssignmentExpression"), ("x++", "PostfixIncrementExpression"),
            ("x--", "PostfixDecrementExpression")
        }) yield return [item.Source, item.Root];
        foreach (string type in new[] { "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "decimal", "string", "object" })
            yield return [$"(T){type}.P", "CastExpression"];
        foreach (string op in new[] { "+", "-", "!", "~", "++", "--" })
            yield return [op + "x.P", "UnaryExpression"];
        yield return ["*p->Next", "PointerIndirectionExpression"];
        yield return ["&a[i]", "AddressExpression"];
    }

    [Theory, MemberData(nameof(PrimaryShapes))]
    public void Remaining_primary_unary_and_type_operator_fields_match(string expression, string root) => CompareExpression(expression, root);

    public static IEnumerable<object[]> Assignments() => AssignmentOperators.Select(op => new object[] { op });

    [Theory, MemberData(nameof(Assignments))]
    public void Every_assignment_operator_preserves_target_token_and_right_associativity(string op) => CompareExpression($"a {op} b=c", "AssignmentExpression");

    [Theory, MemberData(nameof(Assignments))]
    public void Every_assignment_operator_rejects_a_nonunary_target_at_its_exact_authored_span(string op)
    {
        string expression = $"a+b {op} c";
        using var ours = CSharpParser.Parse(new(Prefix + expression + Suffix, "CSharp1", "assignment-target.cs", []));
        Assert.False(ours.Accepted);
        var issue = Assert.Single(ours.Issues, i => i.Code == "CSN2013");
        Assert.Equal(IssueCategory.Syntax, issue.Category);
        Assert.Equal(Nitrogen.DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal(new Nitrogen.TextSpan(Prefix.Length, 3), issue.Span);
    }

    [Theory]
    [InlineData("a as T[1]", "ElementExpression", "AsExpression", "a as T", false, "CS0270", SyntaxKind.AsExpression)]
    [InlineData("a is T[1]", "ElementExpression", "IsExpression", "a is T", false, "CS0270", SyntaxKind.IsExpression)]
    [InlineData("a as T.P()", "InvocationExpression", "AsExpression", "a as T.P", true, null, SyntaxKind.None)]
    [InlineData("a is T.P()", "InvocationExpression", "IsExpression", "a is T.P", false, "CS8022", SyntaxKind.IsPatternExpression)]
    [InlineData("a as T[] .P", "MemberExpression", "AsExpression", "a as T[]", true, null, SyntaxKind.None)]
    [InlineData("a is T++", "PostfixIncrementExpression", "IsExpression", "a is T", true, null, SyntaxKind.None)]
    [InlineData("a as T--", "PostfixDecrementExpression", "AsExpression", "a as T", true, null, SyntaxKind.None)]
    [InlineData("a as T->P", "PointerMemberExpression", "AsExpression", "a as T", true, null, SyntaxKind.None)]
    public void Type_operator_results_require_parentheses_before_a_primary_suffix(string expression, string root, string targetKind, string targetText, bool referenceSyntaxError = true, string? compilationCode = null, SyntaxKind referenceRoot = SyntaxKind.None)
    {
        // ECMA-334 14.5.4/.5/.6/.9 and 25.5.2 require a primary target.
        // The shared Pratt grammar can construct this superset tree; public
        // syntax policy must reject it without resolving either operand type.
        string source = Prefix + expression + Suffix;
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Equal(referenceSyntaxError, reference.SyntaxDiagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
        if (compilationCode is not null)
            Assert.True(reference.CompilationDiagnostics.Any(d => d.Id == compilationCode), string.Join(";", reference.CompilationDiagnostics.Select(d => d.Id + ":" + d.GetMessage())));
        if (referenceRoot != SyntaxKind.None)
        {
            var returned = Assert.Single(reference.Tree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression!;
            Assert.Equal(referenceRoot, returned.Kind());
            if (compilationCode == "CS0270")
            {
                var binary = Assert.IsType<BinaryExpressionSyntax>(returned);
                var type = Assert.IsType<ArrayTypeSyntax>(binary.Right);
                var size = Assert.Single(Assert.Single(type.RankSpecifiers).Sizes);
                Assert.Equal("1", size.ToString());
                Assert.Equal(source.IndexOf("[1]", StringComparison.Ordinal) + 1, size.SpanStart);
            }
        }
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "primary-target.cs", []));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => ours.Tree.Span(i) == new Nitrogen.TextSpan(Prefix.Length, expression.Length) && ours.Tree.Language!.GetKindName(ours.Tree.Kind(i)) == root);
        var target = root switch
        {
            "ElementExpression" => SyntaxView.Cast<ElementExpressionExpression>(ours.Tree, index).Target,
            "InvocationExpression" => SyntaxView.Cast<InvocationExpressionExpression>(ours.Tree, index).Target,
            "MemberExpression" => SyntaxView.Cast<MemberExpressionExpression>(ours.Tree, index).Target,
            "PostfixIncrementExpression" => SyntaxView.Cast<PostfixIncrementExpressionExpression>(ours.Tree, index).Value,
            "PostfixDecrementExpression" => SyntaxView.Cast<PostfixDecrementExpressionExpression>(ours.Tree, index).Value,
            "PointerMemberExpression" => SyntaxView.Cast<PointerMemberExpressionExpression>(ours.Tree, index).Target,
            _ => throw new InvalidOperationException(root)
        };
        Assert.Equal(targetKind, ours.Tree.Language!.GetKindName(target.Kind));
        Assert.Equal(new Nitrogen.TextSpan(Prefix.Length, targetText.Length), target.Span);
        Assert.Equal(targetText, source.Substring(target.Span.Start, target.Span.Length));
        Assert.False(ours.Accepted);
        var issue = Assert.Single(ours.Issues, i => i.Code == "CSN2015");
        Assert.Equal(IssueCategory.Syntax, issue.Category);
        Assert.Equal(new Nitrogen.TextSpan(Prefix.Length, expression.Length), issue.Span);
    }

    [Theory]
    [InlineData("a as T+b", "AddExpression", "AsExpression")]
    [InlineData("a is T<<b", "ShiftLeftExpression", "IsExpression")]
    [InlineData("a as T-b", "SubtractExpression", "AsExpression")]
    [InlineData("a as T/b", "DivideExpression", "AsExpression")]
    [InlineData("a as T%b", "RemainderExpression", "AsExpression")]
    [InlineData("a as T<<b", "ShiftLeftExpression", "AsExpression")]
    [InlineData("a as T>>b", "ShiftRightExpression", "AsExpression")]
    [InlineData("a as T[]*b", "MultiplyExpression", "AsExpression")]
    public void Tighter_binary_operators_cannot_take_an_unparenthesized_type_operator_result(string expression, string root, string leftKind)
    {
        // 14.7/14.8/14.9: the left operand of these tighter tiers cannot be
        // a relational-expression. Parentheses are required to raise its category.
        string source = Prefix + expression + Suffix;
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        if(leftKind=="IsExpression")
        {
            Assert.Contains(reference.CompilationDiagnostics,d=>d.Id=="CS8022");
            Assert.Contains(reference.Tree.GetRoot().DescendantNodes(),n=>n is IsPatternExpressionSyntax);
        }
        else if(expression.Contains("[]",StringComparison.Ordinal))
            Assert.Contains(reference.SyntaxDiagnostics,d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        else
            Assert.Contains(reference.SyntaxDiagnostics,d=>d.Id=="CS8848" && d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "type-binary-floor.cs", []));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => ours.Tree.Span(i) == new Nitrogen.TextSpan(Prefix.Length, expression.Length) && ours.Tree.Language!.GetKindName(ours.Tree.Kind(i)) == root);
        ExpressionNode left = root switch
        {
            "AddExpression" => SyntaxView.Cast<AddExpressionExpression>(ours.Tree, index).Left,
            "ShiftLeftExpression" => SyntaxView.Cast<ShiftLeftExpressionExpression>(ours.Tree, index).Left,
            "MultiplyExpression" => SyntaxView.Cast<MultiplyExpressionExpression>(ours.Tree, index).Left,
            "SubtractExpression" => SyntaxView.Cast<SubtractExpressionExpression>(ours.Tree, index).Left,
            "DivideExpression" => SyntaxView.Cast<DivideExpressionExpression>(ours.Tree, index).Left,
            "RemainderExpression" => SyntaxView.Cast<RemainderExpressionExpression>(ours.Tree, index).Left,
            "ShiftRightExpression" => SyntaxView.Cast<ShiftRightExpressionExpression>(ours.Tree, index).Left,
            _ => throw new InvalidOperationException(root)
        };
        Assert.Equal(leftKind, ours.Tree.Language!.GetKindName(left.Kind));
        Assert.Equal(new Nitrogen.TextSpan(Prefix.Length, expression.Contains("[]",StringComparison.Ordinal)?8:6), left.Span);
        Assert.False(ours.Accepted);
        Assert.Contains(ours.Issues, i => i.Category == IssueCategory.Syntax && i.Severity == Nitrogen.DiagnosticSeverity.Error && i.Span == new Nitrogen.TextSpan(Prefix.Length, expression.Length));
    }

    [Theory]
    [InlineData("(a as T)+b", "AddExpression")]
    [InlineData("(a is T)<<b", "ShiftLeftExpression")]
    [InlineData("(a as T)*b", "MultiplyExpression")]
    public void Parentheses_allow_type_operator_results_at_tighter_binary_tiers(string expression, string root) => CompareExpression(expression, root);

    [Theory]
    [InlineData("(a as T)[1]", "ElementExpression")]
    [InlineData("(a is T)[1]", "ElementExpression")]
    [InlineData("(a as T.P)()", "InvocationExpression")]
    [InlineData("(a as T[]).P", "MemberExpression")]
    [InlineData("(a is T)++", "PostfixIncrementExpression")]
    [InlineData("(a as T)--", "PostfixDecrementExpression")]
    [InlineData("(a as T)->P", "PointerMemberExpression")]
    [InlineData("a as T.P", "AsExpression")]
    [InlineData("a is T[]", "IsExpression")]
    public void Parenthesized_type_operator_results_and_qualified_type_operands_keep_their_categories(string expression, string root) => CompareExpression(expression, root);

    [Theory]
    [InlineData("base")][InlineData("base.")][InlineData("base[]")][InlineData("base[i,]")]
    [InlineData("typeof()")][InlineData("typeof(void[])")][InlineData("typeof(int")]
    [InlineData("checked()")][InlineData("unchecked()")][InlineData("checked(a")][InlineData("unchecked(a")]
    [InlineData("new D(")][InlineData("new D(F,)")]
    [InlineData("a as T*b")][InlineData("a is")][InlineData("a as")][InlineData("a is void")][InlineData("a as T[1]")]
    [InlineData("a?b")][InlineData("a?:c")][InlineData("a?b:")]
    [InlineData("*")][InlineData("&")][InlineData("p->")][InlineData("p->1")]
    [InlineData("sizeof()")][InlineData("sizeof(T")][InlineData("sizeof(void)")]
    public void Incomplete_primary_conditional_type_and_unsafe_forms_have_authored_syntax_diagnostics(string expression)
    {
        string source = Prefix + expression + Suffix;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "expression-tail.cs", []));
        AssertSyntaxRejection(ours, source, Prefix.Length, expression.Length);
    }

    [Theory]
    [InlineData("unsafe")][InlineData("unsafe;")]
    [InlineData("fixed(int* p=x,);")][InlineData("fixed(int* p=x)")][InlineData("fixed(int* p=);")]
    public void Incomplete_unsafe_and_fixed_statements_have_authored_syntax_diagnostics(string statement)
    {
        const string prefix = "unsafe class C{void M(){";
        string source = prefix + statement + "}}";
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "unsafe-tail.cs", []));
        AssertSyntaxRejection(ours, source, prefix.Length, statement.Length);
    }

    [Theory]
    [InlineData("class C{void M(){const int x=F();if(1);while(2)break;do{}while(3);for(;4;);}}")]
    [InlineData("class C{void M(){F(ref 1,out 2);}}")]
    public void Constant_boolean_and_variable_reference_wrappers_preserve_expression_syntax(string source)
    {
        var oracle = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(oracle.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(oracle.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        CompareMethodBody(source);
    }

    [Theory]
    [InlineData("class C{void M(){const int x=;}}")]
    [InlineData("class C{void M(){if();}}")]
    [InlineData("class C{void M(){F(ref,out x);}}")]
    public void Semantic_wrappers_still_require_an_expression(string source)
    {
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "wrapper-expression.cs", []));
        AssertSyntaxRejection(ours, source, 0, source.Length);
    }

    [Theory]
    [InlineData("unsafe class C{void M(){int* p=stackalloc int[n+1];}}")]
    [InlineData("unsafe class C{void M(){for(int* p=stackalloc int[1];;);}}")]
    [InlineData("unsafe class C{void M(){using(int* p=stackalloc int[1]);}}")]
    public void Stackalloc_is_a_direct_local_initializer_in_each_grammatical_owner(string source) => CompareMethodBody(source);

    [Theory]
    [InlineData("unsafe class C{int* p=stackalloc int[1];}")]
    [InlineData("unsafe class C{void M(){p=stackalloc int[1];}}")]
    [InlineData("unsafe class C{void M(){F(stackalloc int[1]);}}")]
    [InlineData("unsafe class C{object M(){return stackalloc int[1];}}")]
    [InlineData("unsafe class C{void M(){int* p=(stackalloc int[1]);}}")]
    [InlineData("unsafe class C{void M(){int*[] p={stackalloc int[1]};}}")]
    public void Stackalloc_outside_a_direct_local_initializer_reports_the_exact_allocation_span(string source)
    {
        const string allocation = "stackalloc int[1]";
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "allocation-owner.cs", []));
        Assert.False(ours.Accepted);
        var issue = Assert.Single(ours.Issues, i => i.Code == "CSN2004");
        Assert.Equal(IssueCategory.Syntax, issue.Category);
        Assert.Equal(Nitrogen.DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal(new Nitrogen.TextSpan(source.IndexOf(allocation, StringComparison.Ordinal), allocation.Length), issue.Span);
    }

    [Theory]
    [InlineData("unsafe class C{void M(){try{}catch{int* p=stackalloc int[1];}}}")]
    [InlineData("unsafe class C{void M(){try{}finally{int* p=stackalloc int[1];}}}")]
    public void Catch_and_finally_allocation_prohibition_remains_a_separate_compiler_context_observation(string source)
    {
        // ECMA-334 first edition 25.7: unchanged local-initializer syntax;
        // the handler prohibition belongs to excluded semantic-context validity.
        var oracle = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(oracle.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(oracle.CompilationDiagnostics, d => d.Id == "CS0255");
        CompareMethodBody(source);
    }

    private static void CompareExpression(string expression, string root)
    {
        string source = Prefix + expression + Suffix;
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression!;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "expression-fields.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => ours.Tree.Span(i) == new Nitrogen.TextSpan(Prefix.Length, expression.Length) && ours.Tree.Language!.GetKindName(ours.Tree.Kind(i)) == root);
        ExpressionShapeAssert.Compare(expected, new ExpressionNode(ours.Tree, index));
    }

    private static void CompareMethodBody(string source)
    {
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).Body!;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "expression-owner.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => BlockNode.Is(ours.Tree, i) && ours.Tree.Span(i).Start == expected.SpanStart);
        StatementShapeAssert.Compare(expected, new Nitrogen.SyntaxNode(ours.Tree, index));
    }

    private static void AssertSyntaxRejection(ParseDocument ours, string source, int fragmentStart, int fragmentLength)
    {
        Assert.False(ours.Accepted);
        var errors = ours.Issues.Where(i => i.Severity == Nitrogen.DiagnosticSeverity.Error && i.Category == IssueCategory.Syntax).ToArray();
        Assert.NotEmpty(errors);
        foreach (var issue in ours.Issues)
        {
            Assert.InRange(issue.Span.Start, 0, source.Length);
            Assert.InRange(issue.Span.Length, 0, source.Length - issue.Span.Start);
        }
        // Recovery can place a missing-token diagnostic at the enclosing delimiter.
        Assert.Contains(errors, i => i.Span.Start >= fragmentStart && i.Span.Start <= fragmentStart + fragmentLength + Suffix.Length);
    }
}
