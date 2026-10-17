using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSNitrogen.Oracle.Tests;

/// <summary>
/// Finite closing witnesses for first-edition ECMA-334 §§16-22 and §24.2.
/// Modifier denotation/compatibility and other declared semantic exclusions remain
/// separate from syntax/tree/span observations. This is not a completion counter.
/// </summary>
public class PendingDeclarationClosureTests
{
    // §§17.1.1,18.1.1,20.1.1,21.2,22.1: enumerate every normative
    // header modifier on its own typed owner. Nesting gives new/access modifiers
    // their ordinary owning context. §25 adds unsafe on the applicable types.
    public static IEnumerable<object[]> TypeHeaderModifiers()
    {
        (string Declaration, string Modifiers)[] families =
        [
            ("class C{}", "new public protected internal private abstract sealed unsafe"),
            ("struct S{}", "new public protected internal private unsafe"),
            ("interface I{}", "new public protected internal private unsafe"),
            ("enum E{A}", "new public protected internal private"),
            ("delegate void D();", "new public protected internal private unsafe")
        ];
        foreach (var family in families)
            foreach (string modifier in family.Modifiers.Split(' '))
                yield return ["class Outer{[A] " + modifier + " " + family.Declaration + "}"];
        yield return ["class Outer{[A] protected internal class C{}[B] protected internal struct S{}" +
            "[C] protected internal interface I{}[D] protected internal enum E{}[E] protected internal delegate void D();}"];
    }

    [Theory, MemberData(nameof(TypeHeaderModifiers))]
    public void Historical_type_modifiers_are_compared_on_their_owning_headers(string source)
        => new DeclarationConformanceParityTests().Declaration_signatures_and_member_payloads_match(source);

    // §§17.3-17.11: each keyword is witnessed in the owning member family.
    // The parser does not decide whether the isolated combination is executable.
    public static IEnumerable<object[]> MemberModifierFamilies()
    {
        const string access = "new public protected internal private";
        const string virtualMember = access + " static virtual sealed override abstract extern unsafe";
        (string Member, string Modifiers)[] families =
        [
            ("const int X=1,Y=2;", access),
            ("int x,y;", access + " static readonly volatile unsafe"),
            ("void M(){}", virtualMember),
            ("int P{get{return 1;}set{}}", virtualMember),
            ("event D E,F;", virtualMember),
            ("int this[int x]{get{return x;}set{}}", access + " virtual sealed override abstract extern unsafe"),
            ("C(){}", "public protected internal private extern unsafe"),
            ("C operator +(C x,C y){return x;}", "public static extern unsafe")
        ];
        foreach (var family in families)
            foreach (string modifier in family.Modifiers.Split(' '))
                yield return ["class C{[A] " + modifier + " " + family.Member + "}"];
        // Both normative event accessor orders, ctor initializers/body alternatives,
        // static/extern permutations and destructor body alternatives.
        foreach (string member in new[]
        {
            "event D E{[A]add{}[B]remove{}}", "event D E{[A]remove{}[B]add{}}",
            "C(int x):base(x){}", "C():this(1){}", "extern C();",
            "static C(){}", "extern static C();", "static extern C();",
            "~C(){}", "extern ~C();", "unsafe ~C(){}"
        }) yield return ["class C{" + member + "}"];
    }

    [Theory, MemberData(nameof(MemberModifierFamilies))]
    public void Historical_member_modifiers_and_body_alternatives_preserve_typed_payloads(string source)
        => new DeclarationConformanceParityTests().Declaration_signatures_and_member_payloads_match(source);

    // §17.9, printed p258: eight unary forms and sixteen binary forms.
    // There are 22 distinct tokens: + and - occur at both arities.
    public static IEnumerable<object[]> OperatorForms()
    {
        foreach (string token in new[] { "+", "-", "!", "~", "++", "--", "true", "false" })
        {
            bool boolean = token is "true" or "false";
            yield return ["class C{public static " + (boolean ? "bool" : "C") + " operator " + token +
                "(C x){return " + (boolean ? "true" : "x") + ";}}"];
        }
        foreach (string token in new[] { "+", "-", "*", "/", "%", "&", "|", "^", "<<", ">>", "==", "!=", ">", "<", ">=", "<=" })
        {
            bool boolean = token is "==" or "!=" or ">" or "<" or ">=" or "<=";
            string secondType = token is "<<" or ">>" ? "int" : "C";
            yield return ["class C{public static " + (boolean ? "bool" : "C") + " operator " + token +
                "(C x," + secondType + " y){return " + (boolean ? "true" : "x") + ";}}"];
        }
        yield return ["class C{public static implicit operator int(C x){return 0;}}"];
        yield return ["class C{public static explicit operator C(int x){return new C();}}"];
        yield return ["class C{public static extern C operator +(C x,C y);}"];
    }

    [Theory, MemberData(nameof(OperatorForms))]
    public void All_historical_operator_tokens_arities_and_conversion_modes_have_typed_spans(string source)
        => new DeclarationConformanceParityTests().Declaration_signatures_and_member_payloads_match(source);

    public static IEnumerable<object[]> DeclarationPayloads()
    {
        foreach (string source in new[]
        {
            // Owning bases lists, declaration bodies and optional type semicolons.
            "class C:B,I,J{};struct S:I,J{};interface K:I,J{};enum E:byte{A=1,B,};delegate void D();",
            "class C{}struct S{}interface I{}enum E{}delegate int D(int x);",
            "class C{int[] x={1,2},y;const int A=1,B=2;event D E=F(),G;}",
            "struct S{public S(int x){this.x=x;}int x;static S(){}static int y=1;}",
            "class C{int I.P{get{return 1;}set{}}void I.M(){}int I.this[int x]{get{return x;}}event D I.E{add{}remove{}}}",
            "interface I{new void M();new int P{get;set;}new int Q{set;}new int R{get;}new event D E;new int this[int x]{set;get;}}",
            "class C{int P{get{return 1;}}int Q{set{}}int R{set{}get{return 1;}}abstract int S{get;set;}}",
            "class C{void M(){}void N(ref T x,out T y,params T[][] rest){}extern void E();}",
            "class C{int this[ref T x,params T[] rest]{get{return 1;}}}interface I{int this[params T[] rest]{get;}}",
            "[A]enum E{[B]A,[C]B=1+2,}[D]delegate void D([E]ref T x,[F]out T y,[G]params T[] rest);",
            // §24.2 historical attribute-argument-expression is expression.
            "[A(x+=1,X=y=1),B,][C()]class C{[A(x+=1,X=y=1)]int x;" +
                "[A(x+=1,X=y=1)]void M([A(x+=1,X=y=1)]ref T x){}" +
                "[A]int P{[A(x+=1,X=y=1)]get{return 1;}[B]set{}}" +
                "[A]event D E{[A(x+=1,X=y=1)]add{}[B]remove{}}}",
            "[type:A,B,]class C{[field:A,B]int x;[method:A]void M([param:A]int x){}" +
                "[return:A]int N(){return 1;}[property:A]int P{[method:A]get{return 1;}}[event:A]event D E;}",
            // Authored UTF-16 offsets and separator spans after supplementary text.
            "/* 🧪 */ [A(1,2,X=3,Y=4),B,]class C:B,/*base*/I{" +
                "[A]void M([B]int x,/*p*/[C]params T[] ys){}" +
                "int x=1,/*v*/y;enum E{A=1,/*e*/B,}}"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(DeclarationPayloads))]
    public void Declaration_attribute_parameter_and_member_lists_compare_children_and_separators(string source)
        => new DeclarationConformanceParityTests().Declaration_signatures_and_member_payloads_match(source);

    [Theory]
    [InlineData("using A.B;using Q=X.Y;[assembly:A(1,X=y=1),B,]namespace N{using R=S.T;class C{}namespace K{};};")]
    [InlineData("/* 🧪 */ [assembly:A(x+=1,X=y=1),B,][module:C()]class C{}")]
    [InlineData("namespace A.B{using C.D;using X=Y.Z;class C{}struct S{}interface I{}enum E{A,}delegate void D();}")]
    public void Compilation_owners_compare_global_attributes_usings_and_namespace_lists(string source)
        => new CompilationUnitConformanceParityTests().Ordered_root_and_namespace_projections_match(source);

    // These sources have malformed tokens/lists/delimiters, independently of
    // modifier compatibility, inheritance denotation or operator/ctor semantics.
    [Theory]
    [InlineData("using A..B;")]
    [InlineData("using X=A.B")]
    [InlineData("namespace N{using X=;}")]
    [InlineData("namespace N{using A.B;")]
    [InlineData("class {}")] [InlineData("struct {}")] [InlineData("interface {}")]
    [InlineData("class C:B,{}")] [InlineData("class C:,B{}")] [InlineData("class C:B,,I{}")]
    [InlineData("struct S:I,{}")] [InlineData("interface I:J,,K{}")]
    [InlineData("class C{int x,;}")] [InlineData("class C{int ,x;}")] [InlineData("class C{int x,,y;}")]
    [InlineData("class C{int x=;}")] [InlineData("class C{const int x;}")] [InlineData("class C{const int x=1,;}")]
    [InlineData("class C{void M(int x,){} }")] [InlineData("class C{void M(,int x){} }")]
    [InlineData("class C{void M(int x,,int y){} }")] [InlineData("class C{void M(ref int){} }")]
    [InlineData("class C{void M(params int[] ){} }")] [InlineData("class C{void M(int x) }")]
    [InlineData("class C{int P{get}}")] [InlineData("class C{int P{get{}}")] 
    [InlineData("class C{int this[int x,]{get{return 1;}}}")]
    [InlineData("class C{int this[,int x]{get{return 1;}}}")]
    [InlineData("class C{int this[int x,,int y]{get{return 1;}}}")]
    [InlineData("class C{event D E,;}")] [InlineData("class C{event D E{add remove{}}}")]
    [InlineData("class C{C():base(,){} }")] [InlineData("class C{C():this(1,){} }")]
        [InlineData("class C{public static C operator +(C x,){} }")]
    [InlineData("class C{public static C operator (C x){} }")]
    [InlineData("class C{public static implicit operator (C x){} }")]
    [InlineData("interface I{void M(int x,);}")] [InlineData("interface I{event D ;}")]
    [InlineData("interface I{int this[int x,]{get;}}")] [InlineData("interface I{int P{get}}")] 
    [InlineData("enum E{,}")] [InlineData("enum E{A,,B}")] [InlineData("enum E{A=}")]
    [InlineData("enum E:int,byte{A}")] [InlineData("delegate void D(int x,);")]
    [InlineData("delegate void D(int x)")] [InlineData("delegate void ();")]
    [InlineData("[A(,1)]class C{}")] [InlineData("[A(1,)]class C{}")] [InlineData("[A(1,,2)]class C{}")]
    [InlineData("[A(X=)]class C{}")] [InlineData("[A,,B]class C{}")] [InlineData("[,A]class C{}")]
    [InlineData("[assembly:A,,B]class C{}")] [InlineData("[assembly:]class C{}")] [InlineData("[A]class C{")]
    public void Malformed_declaration_attribute_parameter_and_base_lists_are_rejected(string source)
    {
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.Contains(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-declaration-errors.cs", []));
        Assert.False(ours.Accepted);
        Assert.Contains(ours.Issues,i=>i.Category==IssueCategory.Syntax && i.Severity==Nitrogen.DiagnosticSeverity.Error);
        Assert.All(ours.Issues, issue =>
        {
            Assert.InRange(issue.Span.Start, 0, source.Length);
            Assert.InRange(issue.Span.End, issue.Span.Start, source.Length);
        });
    }

    // The historical syntax excludes these shapes; the modern compiler may
    // diagnose the restriction after parsing. Keep the observed phase separate.
    [Theory]
    [InlineData("class C{int P{}}")]
    [InlineData("class C{~C(int x){}}")]
    public void Historical_accessor_and_destructor_shapes_do_not_assume_a_compiler_phase(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "pending-declaration-shapes.cs", []));
        Assert.False(ours.Accepted);
    }

    [Fact]
    public void Historical_attribute_assignments_retain_original_expression_spans_and_current_profile_boundary()
    {
        const string source = "/* 🧪 */ [A(x+=1,X=y=1)]class C{}";
        // The existing recursive comparer checks positional and named value trees,
        // their full expression spans, argument order and comma spans.
        new DeclarationConformanceParityTests().Declaration_signatures_and_member_payloads_match(source);
        using var current = CSharpParser.Parse(new(source, "CSharp14", "pending-current-attribute.cs", []));
        Assert.False(current.Accepted);
        var issues = current.Issues.Where(i => i.Code == "CSN2006").ToArray();
        Assert.Equal(new[] { "x+=1", "y=1" }, issues.Select(i => source.Substring(i.Span.Start, i.Span.Length)).ToArray());
        Assert.All(issues, issue => Assert.Equal(IssueCategory.Syntax, issue.Category));
    }

    [Theory]
    [InlineData("class C{D(){}}", "CS1520")]
    [InlineData("class C{public static C operator +(C x,C y,C z){return x;}}", "CS1534")]
    [InlineData("enum E:float{A}", "CS1008")]
    [InlineData("class C{public private int x;}", "CS0107")]
    public void Established_constructor_operator_enum_and_modifier_boundaries_remain_semantic(string source, string diagnostic)
        => new DeclarationConformanceParityTests().Semantic_declaration_constraints_remain_compiler_observations(source, diagnostic);
}
