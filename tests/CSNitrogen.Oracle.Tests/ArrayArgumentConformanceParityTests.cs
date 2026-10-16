using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class ArrayArgumentConformanceParityTests
{
    public static IEnumerable<object[]> ArrayShapes()
    {
        string[] elements = ["int", "N.T", "@int", "int*"];
        string[] dimensions = ["1", "a+b", "F(x)", "a?b:c", "a=b", "1,2", "F(a),G(b),3"];
        string[] tails = ["", "[]", "[,]", "[][,]"];
        foreach (string element in elements)
            foreach (string sizes in dimensions)
                foreach (string tail in tails) yield return [$"new {element}[{sizes}]{tail}"];
        foreach (string source in new[]
        {
            "new int[2]{1,2}", "new int[0]{}", "new int[]{1,2,}", "new int[,]{{1,2},{3,4},}",
            "new int[][]{new int[]{1},new int[0],}", "new int[][,]{}", "new int[,][]{}",
            "new T[]{(T)x,F(y),a?b:c,a=b}", "(new int[]{})[0]", "new int[3].Length",
            "new N /*t*/ . T [ F(a), /*d*/ G(b) ] [ , ] { {x,y}, {z,w}, }",
            "F(new int[]{1},new T[2][])"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(ArrayShapes))]
    public void Arrays_and_nested_initializers_match_recursive_source_shape(string expression) => CompareSource(expression);

    [Theory]
    [InlineData("F()")][InlineData("F(x)")][InlineData("F(x,y,z)")]
    [InlineData("F(ref x,out y,z)")][InlineData("F(out a[0],ref o.P)")]
    [InlineData("F(a=b,c?d:e,(T)x,new int[]{1,2})")]
    [InlineData("F(ref /*mode*/ x, /*next*/ out y)")]
    [InlineData("new C(ref x,out y)")][InlineData("new C(F(x),G(y))")]
    [InlineData("F(refValue,outValue,@ref,@out)")]
    public void Arguments_keep_modifier_values_separators_and_spans(string expression) => CompareSource(expression);

    [Theory]
    [InlineData("class C{int[] x={1,2,};}")]
    [InlineData("class C{void M(){int[,] x={{1,2},{3,4},};}}")]
    [InlineData("class C{C():base(ref x,out y){}int x,y;}")]
    [InlineData("class C{C():this(new int[]{1,2}){}C(int[] x){}}")]
    public void Initializers_and_arguments_retain_owning_declaration_contexts(string source)
    {
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "declarations.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        foreach (var initializer in reference.GetRoot().DescendantNodes().OfType<InitializerExpressionSyntax>()
                     .Where(n => n.Parent is EqualsValueClauseSyntax))
        {
            int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => ArrayInitializerNode.Is(ours.Tree, n) && ours.Tree.Span(n).Start == initializer.SpanStart);
            ExpressionShapeAssert.CompareInitializer(initializer, SyntaxView.Cast<ArrayInitializerNode>(ours.Tree, node));
        }
        foreach (var constructor in reference.GetRoot().DescendantNodes().OfType<ConstructorInitializerSyntax>())
        {
            int node = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => ArgumentsNode.Is(ours.Tree, n) && ours.Tree.Span(n).Start == constructor.ArgumentList.SpanStart);
            ExpressionShapeAssert.CompareArguments(constructor.ArgumentList, SyntaxView.Cast<ArgumentsNode>(ours.Tree, node));
        }
    }

    [Theory]
    [InlineData("new int[]{1}[0]")][InlineData("new int[1]{1}[0]")]
    public void Initialized_direct_array_indexing_is_an_explicit_compiler_discrepancy(string expression)
    {
        string source = "class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var returned = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression;
        Assert.IsType<ElementAccessExpressionSyntax>(returned);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "array-discrepancy.cs", []));
        Assert.False(ours.Accepted); Assert.Contains(ours.Issues, issue => issue.Code == "CSN2005");
    }

    [Theory]
    [InlineData("new int[1][2]")][InlineData("new int[1][][2]")]
    public void Non_initialized_direct_array_indexing_has_an_explicit_compilation_error(string expression)
    {
        string source = "class C{object M(){return " + expression + ";}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS0178");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "array-rank.cs", []));
        Assert.False(ours.Accepted); Assert.Contains(ours.Issues, issue => issue.Code == "CSN2005");
    }

    [Theory]
    [InlineData("F(x:1)")][InlineData("F(out int x)")][InlineData("F(in x)")]
    [InlineData("new[]{1,2}")]
    public void Later_argument_and_array_forms_have_explicit_version_errors(string expression)
    {
        string source = "class C{object M(){return " + expression + ";}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8022");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "later-array.cs", []));
        Assert.False(ours.Accepted);
    }

    [Theory]
    [InlineData("new int[1,]")][InlineData("new int[,1]")]
    [InlineData("new int[1")][InlineData("new void[1]")][InlineData("new int[]{,}")]
    [InlineData("new int[]{1,,2}")][InlineData("new int[]{1,,}")]
    [InlineData("F(,)")][InlineData("F(x,)")][InlineData("F(ref)")][InlineData("F(out)")]
    [InlineData("F(ref out x)")][InlineData("F(ref x out y)")][InlineData("F({1,2})")]
    public void Malformed_array_and_argument_sources_have_reference_syntax_errors(string expression)
    {
        string source = "class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.Contains(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "malformed-array.cs", []));
        Assert.False(ours.Accepted);
    }

    [Theory]
    [InlineData("new int[]")][InlineData("new int[,]")][InlineData("new int[][1]")]
    public void Missing_array_sizes_and_initializer_have_an_explicit_reference_error(string expression)
    {
        string source = "class C{object M(){return " + expression + ";}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS1586");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "missing-array.cs", []));
        Assert.False(ours.Accepted);
    }

    private static void CompareSource(string expression)
    {
        string source = "unsafe class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression!;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "arrays.cs", []));
        Assert.True(ours.Accepted, expression + " :: " + string.Join(";", ours.Issues));
        var tree = ours.Tree;
        int root = Assert.Single(Enumerable.Range(0, tree.NodeCount), n =>
            tree.Language!.GetKindName(tree.Kind(n)).EndsWith("Expression", StringComparison.Ordinal) &&
            tree.Span(n).Start == expected.Span.Start && tree.Span(n).Length == expected.Span.Length);
        ExpressionShapeAssert.Compare(expected, new ExpressionNode(tree, root));
    }
}
