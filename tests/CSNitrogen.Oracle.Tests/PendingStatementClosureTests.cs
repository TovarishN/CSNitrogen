using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

/// <summary>
/// Finite witnesses for ECMA-334 first edition §15 (printed pp179-201).
/// These test syntax, typed children and authored spans, not execution or binding.
/// </summary>
public class PendingStatementClosureTests
{
    // §15, §15.2-15.6: all statement categories, optional blocks/lists, labels,
    // declarations and the seven permitted statement-expression alternatives.
    // §15.7-15.13: both selections, all loops/jumps/handlers and resource forms.
    public static IEnumerable<object[]> NormativeShapes()
    {
        foreach (string statement in new[]
        {
            "{}", ";", "{;{}F();}", "label:;", "label:int x;",
            "{T x;T y=F(),z;const int a=1,b=2;}",
            "{F();new C();x=1;x++;x--;++x;--x;}",
            "if(a)F();", "if(a)if(b)F();else G();", "if(a)F();else if(b)G();else H();",
            "switch(x){}", "switch(x){case 1:case 2:F();break;default:return;}",
            "while(a);", "do {}while(a);",
            "for(;;);", "for(T x,y;;);", "for(i=0,j=1;a;i++,j--);",
            "foreach(T x in xs){}",
            "while(a){break;continue;}", "{goto target;target:;}",
            "switch(x){case 1:goto case 2;case 2:goto default;default:break;}",
            "{return;}", "{return F();}", "{throw e;}",
            "try{}catch(E e){throw;}", "try{}catch(E){}catch(F f){}catch{}",
            "try{}catch{}", "try{}finally{}", "try{}catch(E){}finally{}", "try{}catch{}finally{}",
            "checked{}", "unchecked{}", "lock(x);",
            "using(F());", "using(T x=F(),y=G()){}",
            "{\r\n/* 🧪 */ int x=F(), /*comma*/ y; const int a=1, b=2;\r\n" +
                "for(x=0, y=1; x<2; x++, y--) F();\r\n" +
                "try{}catch(/*type*/ E /*name*/ e){}catch{}finally{}\r\n}"
        }) yield return [statement];
    }

    [Theory, MemberData(nameof(NormativeShapes))]
    public void Normative_alternatives_preserve_recursive_children_lists_and_original_spans(string statement)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var method = Assert.Single(reference.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-statement-shapes.cs", []));
        Assert.True(ours.Accepted, statement + " :: " + string.Join(";", ours.Issues));
        int block = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount),
            n => BlockNode.Is(ours.Tree, n) && ours.Tree.Span(n).Start == method.Body!.SpanStart);
        // This comparer visits the generated wrappers, list fields, option presence,
        // declarator/iterator separators, handler order, expressions and UTF-16 spans.
        StatementShapeAssert.Compare(method.Body!, new Nitrogen.SyntaxNode(ours.Tree, block));
    }

    // Each item is malformed under the cited grammar, independently of binding.
    // Roslyn syntax rejection is checked separately from the public parser result.
    [Theory]
    [InlineData("{", "15.2")]
    [InlineData("label:", "15.4")]
    [InlineData("int ;", "15.5.1")]
    [InlineData("int ,x;", "15.5.1")]
    [InlineData("int x,;", "15.5.1")]
    [InlineData("int x,,y;", "15.5.1")]
    [InlineData("int x=;", "15.5.1")]
    [InlineData("int x", "15.5.1")]
    [InlineData("const int x;", "15.5.2")]
    [InlineData("const int x=;", "15.5.2")]
    [InlineData("const int ,x=1;", "15.5.2")]
    [InlineData("const int x=1,;", "15.5.2")]
    [InlineData("const int x=1,,y=2;", "15.5.2")]
    [InlineData("const int x=1", "15.5.2")]
    [InlineData("F()", "15.6")]
    [InlineData("if()F();", "15.7.1")]
    [InlineData("if(a)F();else", "15.7.1")]
    [InlineData("switch(){ }", "15.7.2")]
    [InlineData("switch(x){case :break;}", "15.7.2")]
    [InlineData("switch(x){case 1 break;}", "15.7.2")]
    [InlineData("switch(x){default break;}", "15.7.2")]
    [InlineData("while()F();", "15.8.1")]
    [InlineData("do F();while(a)", "15.8.2")]
    [InlineData("do F();while();", "15.8.2")]
    [InlineData("for(i=0,;;);", "15.8.3")]
    [InlineData("for(,i=0;;);", "15.8.3")]
    [InlineData("for(i=0,,j=1;;);", "15.8.3")]
    [InlineData("for(;int x;);", "15.8.3")]
    [InlineData("for(;a,b;);", "15.8.3")]
    [InlineData("for(;;int x);", "15.8.3")]
    [InlineData("for(;;i++,);", "15.8.3")]
    [InlineData("for(;;,i++);", "15.8.3")]
    [InlineData("for(;;i++,,j--);", "15.8.3")]
    [InlineData("foreach(T in xs);", "15.8.4")]
    [InlineData("foreach(T x xs);", "15.8.4")]
    [InlineData("break x;", "15.9.1")]
    [InlineData("break", "15.9.1")]
    [InlineData("continue x;", "15.9.2")]
    [InlineData("continue", "15.9.2")]
    [InlineData("goto ;", "15.9.3")]
    [InlineData("goto case ;", "15.9.3")]
    [InlineData("goto default x;", "15.9.3")]
    [InlineData("return ,;", "15.9.4")]
    [InlineData("return", "15.9.4")]
    [InlineData("throw ,;", "15.9.5")]
    [InlineData("throw", "15.9.5")]
    [InlineData("try{}", "15.10")]
    [InlineData("try{}catch(E e,){}", "15.10")]
    [InlineData("try{}catch(){}", "15.10")]
    [InlineData("try{}catch(E e)F();", "15.10")]
    [InlineData("try{}finally F();", "15.10")]
    [InlineData("checked F();", "15.11")]
    [InlineData("unchecked F();", "15.11")]
    [InlineData("lock();", "15.12")]
    [InlineData("using();", "15.13")]
    [InlineData("using(T x=F(),);", "15.13")]
    [InlineData("using(T x=F(),,y=G());", "15.13")]
    public void Malformed_delimiters_lists_and_operands_have_syntax_errors(string statement, string section)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.True(reference.GetDiagnostics().Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error),
            "Expected malformed syntax under ECMA-334 first edition §" + section + ": " + statement);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-statement-errors.cs", []));
        Assert.False(ours.Accepted);
        Assert.Contains(ours.Issues,i=>i.Category==IssueCategory.Syntax && i.Severity==Nitrogen.DiagnosticSeverity.Error);
        Assert.All(ours.Issues,i=>{Assert.InRange(i.Span.Start,0,source.Length);Assert.InRange(i.Span.End,i.Span.Start,source.Length);});
    }

    // §15.10 explicitly permits one general catch, last among catches. Modern
    // compiler diagnostics can arise after parsing, so do not prescribe that phase.
    [Theory]
    [InlineData("try{}catch{}catch{}")]
    [InlineData("try{}catch{}catch(System.Exception e){}")]
    public void General_catch_cardinality_and_order_are_historical_syntax_constraints(string statement)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-catch-order.cs", []));
        Assert.False(ours.Accepted);
    }

    // Established semantic exclusions: §15.13 initializer requirement; §15.10
    // catch type validity through the shared normalized TypeReference union;
    // §15.9.1 break targets. These remain accepted syntax with compiler errors.
    [Theory]
    [InlineData("using(System.IDisposable x){}", "CS0210")]
    [InlineData("try{}catch(int x){}", "CS0155")]
    [InlineData("try{}catch(int[] x){}", "CS0155")]
    [InlineData("break;", "CS0139")]
    public void Explicit_semantic_boundaries_do_not_become_parser_rejections(string statement, string diagnostic)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-statement-semantics.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
    }

    [Fact]
    public void Reachability_warning_is_not_a_statement_syntax_failure()
    {
        // §15.1 printed p180: unreachable statements produce a warning, not an error.
        const string source = "class C{void F(){}void M(){return;F();}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS0162" && d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
        Assert.DoesNotContain(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-reachability.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
    }
}
