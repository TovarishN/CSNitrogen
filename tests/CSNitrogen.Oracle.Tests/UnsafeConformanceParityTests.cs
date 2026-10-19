using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class UnsafeConformanceParityTests
{
    [Theory]
    [InlineData("*p")]
    [InlineData("**p")]
    [InlineData("&x")]
    [InlineData("&a[i]")]
    [InlineData("p->Value")]
    [InlineData("p->M(x)")]
    [InlineData("p->Next->Value")]
    [InlineData("(*p).Value")]
    [InlineData("*p++")]
    [InlineData("(*p)++")]
    [InlineData("*(p+i)")]
    [InlineData("&*p")]
    [InlineData("*&x")]
    [InlineData("(int*)p")]
    [InlineData("(T**)p")]
    [InlineData("(void*)p")]
    [InlineData("(int*[])p")]
    [InlineData("sizeof(T)")]
    [InlineData("sizeof(T*)")]
    [InlineData("sizeof(int*)")]
    [InlineData("sizeof(void*)")]
    [InlineData("sizeof(int[])")]
    [InlineData("sizeof(T[,])")]
    [InlineData("sizeof(int)")]
    [InlineData("p+1")]
    [InlineData("p[1]")]
    [InlineData("p->Next[0]")]
    [InlineData("p /*p*/ -> /*n*/ Next->Value")]
    public void Unsafe_expression_children_and_precedence_match(string expression)
    {
        string source = "unsafe class C{object M(){return " + expression + ";}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "expression.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        var cls = Assert.Single(ours.Classes); Assert.Equal(1, cls.Modifiers.Count); Assert.True(UnsafeModifierModifier.Is(ours.Tree, cls.Modifiers[0].Index));
        ParameterShapeAssert.Span(Assert.Single(reference.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()).Modifiers[0].Span, cls.Modifiers[0].Span);
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => JumpStatementNode.Is(ours.Tree, i));
        StatementShapeAssert.Compare(expected, new Nitrogen.SyntaxNode(ours.Tree, index));
    }

    [Theory]
    [InlineData("unsafe{}")]
    [InlineData("unsafe{int* p=stackalloc int[4];*p=1;}")]
    [InlineData("fixed(int* p=&x){*p=1;}")]
    [InlineData("fixed(int* p=a,q=b)p->M();")]
    [InlineData("fixed(T** p=x); ")]
    [InlineData("fixed(void* p=x)while(b)F();")]
    [InlineData("unsafe{fixed(int* p=&x){unsafe{*p=1;}}}")]
    [InlineData("fixed(int* /*t*/ p= /*v*/ &x, /*c*/ q=y) /*b*/ {}")]
    public void Unsafe_and_fixed_statement_owning_fields_match(string statement)
    {
        string source = "unsafe class C{void M(){" + statement + "}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var method = Assert.Single(reference.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "statements.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => BlockNode.Is(ours.Tree, i) && ours.Tree.Span(i).Start == method.Body!.SpanStart);
        StatementShapeAssert.Compare(method.Body!, new Nitrogen.SyntaxNode(ours.Tree, index));
    }

    [Theory]
    [InlineData("int")]
    [InlineData("T")]
    [InlineData("int*")]
    [InlineData("void*")]
    public void Stackalloc_keeps_element_and_size_fields(string element)
    {
        string source = "unsafe class C{void M(){int* p=stackalloc " + element + "[n+1];}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<StackAllocArrayCreationExpressionSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "stackalloc.cs", [])); Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => StackAllocExpressionExpression.Is(ours.Tree, i));
        var actual = SyntaxView.Cast<StackAllocExpressionExpression>(ours.Tree, index); ParameterShapeAssert.Span(expected.Span, actual.Span);
        var type = Assert.IsType<ArrayTypeSyntax>(expected.Type); ParameterShapeAssert.Text(type.ElementType, new Nitrogen.SyntaxNode(ours.Tree, actual.Type.Index));
        ExpressionShapeAssert.Compare(Assert.Single(Assert.Single(type.RankSpecifiers).Sizes), actual.Size);
    }
    [Theory]
    [InlineData("int*")]
    [InlineData("T**")]
    [InlineData("void*")]
    [InlineData("void**")]
    [InlineData("int*[]")]
    [InlineData("N.T*[,][]")]
    [InlineData("int /*a*/ * /*b*/ *")]
    public void Pointer_type_elements_stars_and_array_ranks_match(string type)
    {
        string source = "class C{" + type + " p;}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>()).Declaration.Type;
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "types.cs", [])); Assert.True(ours.Accepted, string.Join(";", ours.Issues));
        var cls = Assert.Single(ours.Classes); Assert.Equal(1, cls.Members.Count); var member = cls.Members[0]; // StandardClassMember is transparent
        var field = SyntaxView.Cast<FieldDeclarationNode>(ours.Tree, SyntaxView.Cast<MemberDeclarationNode>(ours.Tree, member.Index).Body.Index);
        var actual = SyntaxView.Cast<TypeSyntaxTypeReference>(ours.Tree, field.Value.Type.Index);
        ParameterShapeAssert.Span(expected.Span, actual.Span);
        if (expected is ArrayTypeSyntax array)
        {
            UnsafeShapeAssert.PointerBase(array.ElementType, actual.Element); Assert.Equal(array.RankSpecifiers.Count, actual.Ranks.Count);
            for (int r = 0; r < actual.Ranks.Count; r++)
            { ParameterShapeAssert.Span(array.RankSpecifiers[r].Span, actual.Ranks[r].Span); var commas = array.RankSpecifiers[r].Sizes.GetSeparators().ToArray(); Assert.Equal(commas.Length, actual.Ranks[r].Commas.Count); for (int c = 0; c < commas.Length; c++) ParameterShapeAssert.Span(commas[c].Span, actual.Ranks[r].Commas[c].Span); }
        }
        else { Assert.Equal(0, actual.Ranks.Count); UnsafeShapeAssert.PointerBase(expected, actual.Element); }
    }

    [Theory]
    [InlineData("int", null)]
    [InlineData("decimal", null)]
    [InlineData("S", "CS0233")]
    [InlineData("string", "CS0233")]
    [InlineData("object", "CS0233")]
    public void Sizeof_type_and_unsafe_context_are_compilation_observations(string operand, string? diagnostic)
    {
        string source = "struct S{int x;}class C{int M(){return sizeof(" + operand + ");}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        if (diagnostic is null) Assert.DoesNotContain(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        else Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "sizeof-context.cs", [])); Assert.True(ours.Accepted);
    }

    [Theory]
    [InlineData("unsafe struct S{fixed int values[4];}")]
    [InlineData("unsafe class C{void M(){int* p=stackalloc[]{1,2};}}")]
    [InlineData("unsafe class C{void M(){int* p=stackalloc int[]{1,2};}}")]
    [InlineData("unsafe class C{void M(){int* p=stackalloc int[2]{1,2};}}")]
    [InlineData("unsafe class C{void M(){fixed(int* p=)F();}}")]
    [InlineData("unsafe class C{void M(){fixed(int* p=x,)F();}}")]
    [InlineData("unsafe class C{void M(){fixed(int* p=x)}}")]
    [InlineData("unsafe class C{void M(){int* p=stackalloc int[];}}")]
    public void Later_or_malformed_unsafe_forms_remain_rejected(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "unsafe-invalid.cs", [])); Assert.False(ours.Accepted);
    }

}
