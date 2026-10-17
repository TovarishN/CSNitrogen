using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

public class StatementConformanceParityTests
{
    public static IEnumerable<object[]> StatementShapes()
    {
        string[] bodies = ["F();", ";", "{}", "{T x;F();}", "if(b)G();else H();", "while(b)G();", "for(;;)G();", "using(G())H();", "lock(y)G();"];
        (string Before, string After)[] outers = [("if(a) ", ""), ("while(a) ", ""), ("do ", " while(a);"),
            ("for(;;) ", ""), ("foreach(T x in xs) ", ""), ("using(F()) ", ""), ("lock(x) ", ""), ("if(a) ", " else J();")];
        foreach (var outer in outers)
            foreach (string body in bodies) yield return [outer.Before + body + outer.After];
        foreach (string initializer in new[] { "", "int i=0,j=1", "i=0,j=1" })
            foreach (string condition in new[] { "", "i<10" })
                foreach (string iterator in new[] { "", "i++", "i++,j--" }) yield return [$"for({initializer};{condition};{iterator})F();"];
        foreach (string statement in new[]
        {
            "switch(x){}", "switch(x){case 1:case 2:F();break;default:return;}",
            "switch(x){default:switch(y){default:break;}break;}",
            "switch(x){case 1:goto case 2;case 2:goto default;default:break;}",
            "switch(x){default:break;}switch(y){default:break;}",
            "try{F();}catch(E e){throw;}", "try{}catch(E){}catch{}finally{F();}",
            "try{}catch(E e){}catch(F f){}", "try{}catch{}", "try{}finally{}",
            "try{}catch(E e){try{}catch(F f){}finally{G();}}finally{H();}",
            "using(System.IDisposable x=F(),y=G()){T z;}",
            "{int x=1,y;const int c=1,d=2;label:F();goto label;return;}",
            "{int[] x={1,2,};x[0]=1;}", "checked{unchecked{F();}}",
            "while(x){continue;break;}", "{return F();}", "{throw e;}",
            "if(/*c*/a) F(); /*gap*/ else G();",
            "if(a) /*b*/ if(b) F(); else /*e*/ G();",
            "for(int i=0, /*v*/ j=1;i<2;i++, /*it*/ j--) /*body*/ F();",
            "using(System.IDisposable x= /*v*/ F(),y=G()){T z;}",
            "switch(x){case /*c*/ 1: /*s*/ F();break;default:break;}",
            "try{}catch(/*t*/ E /*n*/ e){throw;} /*f*/ finally{}",
            "label: /*body*/ F();", "\r\nif(a)\nF();\r\nelse G();"
        }) yield return [statement];
    }

    [Theory, MemberData(nameof(StatementShapes))]
    public void Recursive_statement_bodies_headers_and_spans_match(string statement)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var method = Assert.Single(reference.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>());
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "statements.cs", []));
        Assert.True(ours.Accepted, statement + " :: " + string.Join(";", ours.Issues));
        int block = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), n => BlockNode.Is(ours.Tree,n) && ours.Tree.Span(n).Start == method.Body!.SpanStart);
        StatementShapeAssert.Compare(method.Body!, new Nitrogen.SyntaxNode(ours.Tree, block));
    }

    [Theory]
    [InlineData("using(System.IDisposable x){}", "CS0210", true, null)]
    [InlineData("using(System.IDisposable x=F(),y){}", "CS0210", true, null)]
    [InlineData("using(System.IDisposable x,y=F()){}", "CS0210", true, null)]
    [InlineData("switch(x){default:break;default:break;}", "CS0152", false, "CSN2006")]
    [InlineData("switch(x){default:default:break;}", "CS0152", false, "CSN2006")]
    [InlineData("switch(x){case 1:default:break;default:return;}", "CS0152", false, "CSN2006")]
    public void Compiler_deferred_context_constraints_have_declared_parser_outcomes(string statement, string diagnostic, bool accepted, string? oursCode)
    {
        string source = "class C{System.IDisposable F(){return null;}void M(int x){" + statement + "}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "constraints.cs", []));
        Assert.Equal(accepted, ours.Accepted);
        if (oursCode is not null) Assert.Contains(ours.Issues, issue => issue.Code == oursCode);
    }

    [Theory]
    [InlineData("if(x)T y;")][InlineData("while(x)const int y=1;")]
    [InlineData("for(;;)label:F();")][InlineData("lock(x)label:F();")]
    [InlineData("a+b;")][InlineData("for(a+b;;);")][InlineData("for(;;a+b);")]
    [InlineData("(F());")][InlineData("new int[1];")]
    public void Statement_eligibility_compilation_observations_match(string statement)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id is "CS1023" or "CS0201");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "eligibility.cs", []));
        Assert.False(ours.Accepted);
    }

    [Theory]
    [InlineData("try{}")][InlineData("try{}finally{}catch(E e){}")]
    [InlineData("if(x)")][InlineData("if(x)F();else")][InlineData("do F();while(x)")]
    public void Malformed_control_flow_has_reference_syntax_errors(string statement)
    {
        string source = "class C{void M(){" + statement + "}}";
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.Contains(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "malformed-flow.cs", []));
        Assert.False(ours.Accepted);
    }
    [Theory]
    [InlineData("switch(x){case 1:}")][InlineData("switch(x){default:}")]
    public void Empty_final_switch_sections_have_explicit_compilation_errors(string statement)
    {
        string source = "class C{void M(int x){" + statement + "}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8070");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "empty-switch.cs", []));
        Assert.False(ours.Accepted);
    }

    [Fact]
    public void Invocation_shaped_case_is_a_modern_pattern_reinterpretation()
    {
        const string source = "class C{void M(int x){switch(x){case F():break;}}}";
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.DoesNotContain(reference.SyntaxDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.Single(reference.Tree.GetRoot().DescendantNodes().OfType<CasePatternSwitchLabelSyntax>());
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == "CS8022");
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "case-interpretation.cs", []));
        // C#1 constant-expression grammar is Expression; constancy/binding is excluded.
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
    }
}
