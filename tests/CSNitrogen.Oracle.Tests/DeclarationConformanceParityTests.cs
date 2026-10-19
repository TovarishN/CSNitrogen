using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using Node = Nitrogen.SyntaxNode;

namespace CSNitrogen.Oracle.Tests;

public class DeclarationConformanceParityTests
{
    public static IEnumerable<object[]> Signatures()
    {
        string[] parameters = ["", "int x", "ref int x,out string y", "params int[] xs",
            "[A] int x, [B(1,Name=2)] ref T y, [C] params T[][] zs",
            "int /*type*/ x, /*comma*/ params string[] names"];
        foreach (string p in parameters)
            foreach (string declaration in new[] {
                $"class C{{[A] public void M({p}){{}}}}",
                $"class C{{public C({p}):base(){{}}}}",
                $"interface I{{[A] new void M({p});}}",
                $"[A] public delegate void D({p});",
                $"class C{{void I.M({p}){{}}}}" }) yield return [declaration];
        foreach (string source in new[] {
            "[A] public class C:B,I{[B] protected internal int x=1,y;const int z=2;class N{}struct S{}interface I{}enum E{A,B=2,}delegate int D();}",
            "struct S:I{public int x;public S(int x){this.x=x;}public int M(){return x;}}",
            "class C{static C(){}~C(){} C(int x):this(){} C(){} }",
            "class C{[A] public int P{[B] get{return 1;}[C] set{F();}} int I.P{get{return 2;}set{}}}",
            "class C{int this[int x,string y]{get{return x;}set{}} int I.this[int x]{get{return x;}}}",
            "interface I{[A] new int P{[B] get;set;}int this[int x]{get;set;}event D E;}",
            "class C{[A] public event D E,F; event D I.E{[B] add{F();}remove{G();}}}",
            "class C{public static C operator +(C x,C y){return x;}public static bool operator true(C x){return true;}public static implicit operator int(C x){return 1;}public static explicit operator C(int x){return new C();}}",
            "enum E:byte{[A] One=1,Two,Three=1+2,}", "enum E{}", "enum E{A}", "enum E{A,B}",
            "[type:A,B,] class C{[field:A,] int x;[return:A] int M([param:A,] int x){return x;}}",
            "class C{int this[params int[] xs]{get{return 1;}}}interface I{int this[params int[] xs]{get;}}",
            "class C{C(int x):base(x,ref x,out x){}void A(){int x=1;F();}void B(){return;}}",
            "interface I{void A();int B(int x);event D E;}",
            "class C{extern void M();abstract int P{get;set;}readonly int x;volatile int y;}"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(Signatures))]
    public void Declaration_signatures_and_member_payloads_match(string source)
    {
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "declarations.cs", []));
        Assert.True(ours.Accepted, source + " :: " + string.Join(";", ours.Issues));
        var root = reference.GetRoot();
        T Match<T>(Microsoft.CodeAnalysis.SyntaxNode expected) where T : struct, ISyntaxView<T>
        {
            int index = Assert.Single(Enumerable.Range(0, ours.Tree.NodeCount), i => T.Is(ours.Tree, i)
                && ours.Tree.Span(i).Start >= expected.SpanStart && ours.Tree.Span(i).End == expected.Span.End);
            return T.Create(ours.Tree, index);
        }
        Node N<T>(T node) where T : struct, ISyntaxView<T> => new(ours.Tree, node.Index);
        void Name(SyntaxToken expected, Node actual) { ParameterShapeAssert.Span(expected.Span, actual.Span); Assert.Equal(expected.Text, actual.ToString()); }
        void Qualified(SyntaxToken name, ExplicitInterfaceSpecifierSyntax? spec, Node actual)
        {
            int start = spec?.SpanStart ?? name.SpanStart;
            ParameterShapeAssert.Span(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(start, name.Span.End), actual.Span);
            Assert.Equal(source[start..name.Span.End], actual.ToString());
        }
        void Body(BlockSyntax? block, SyntaxToken semicolon, Nitrogen.SyntaxNode body)
        {
            var child = body; // MethodBody is an alias: the Block or ";" leaf stands in for it
            if (block is not null) StatementShapeAssert.Compare(block, child);
            else Name(semicolon, child);
        }
        void Header(Microsoft.CodeAnalysis.SyntaxList<AttributeListSyntax> attributes, SyntaxTokenList modifiers,
            Nitrogen.SyntaxList<AttributeSectionNode> actualAttributes, Nitrogen.SyntaxList<ModifierNode> actualModifiers)
        {
            ParameterShapeAssert.Attributes(attributes, actualAttributes);
            var tokens = modifiers.Where(t => !t.IsKind(SyntaxKind.ConstKeyword)).ToArray();
            Assert.Equal(tokens.Length, actualModifiers.Count);
            for (int i = 0; i < tokens.Length; i++) Name(tokens[i], N(actualModifiers[i]));
        }
        void Bases(BaseListSyntax? expected, Nitrogen.Optional<BasesNode> actual)
        {
            Assert.Equal(expected is not null, actual.HasValue);
            if (expected is null) return;
            ParameterShapeAssert.Span(expected.Span, actual.Value.Span);
            Assert.Equal(expected.Types.Count, actual.Value.Types.Count);
            var commas = expected.Types.GetSeparators().ToArray(); Assert.Equal(commas.Length, actual.Value.Types.SeparatorCount);
            for (int i = 0; i < expected.Types.Count; i++) ParameterShapeAssert.Text(expected.Types[i].Type, N(actual.Value.Types[i]));
            for (int i = 0; i < commas.Length; i++) ParameterShapeAssert.Span(commas[i].Span, actual.Value.Types.Separator(i).Span);
        }
        void Members<T>(Microsoft.CodeAnalysis.SyntaxList<MemberDeclarationSyntax> expected, Nitrogen.SyntaxList<T> actual) where T : struct, ISyntaxView<T>
        { Assert.Equal(expected.Count, actual.Count); for (int i = 0; i < actual.Count; i++) ParameterShapeAssert.Span(expected[i].Span, N(actual[i]).Span); }
        void Accessors<T>(AccessorListSyntax expected, Nitrogen.SyntaxList<T> actual) where T : struct, ISyntaxView<T>
        { Assert.Equal(expected.Accessors.Count, actual.Count); for (int i = 0; i < actual.Count; i++) ParameterShapeAssert.Span(expected.Accessors[i].Span, N(actual[i]).Span); }
        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            switch (type)
            {
                case ClassDeclarationSyntax:
                    var c = Match<ClassDeclarationNode>(type); Header(type.AttributeLists, type.Modifiers, c.Attributes, c.Modifiers); Name(type.Identifier, N(c.Name)); Bases(type.BaseList, c.Bases); Members(type.Members, c.Members); ParameterShapeAssert.Span(type.Span, c.Span); break;
                case StructDeclarationSyntax:
                    var s = Match<StructDeclarationNode>(type); Header(type.AttributeLists, type.Modifiers, s.Attributes, s.Modifiers); Name(type.Identifier, N(s.Name)); Bases(type.BaseList, s.Bases); Members(type.Members, s.Members); ParameterShapeAssert.Span(type.Span, s.Span); break;
                case InterfaceDeclarationSyntax:
                    var f = Match<InterfaceDeclarationNode>(type); Header(type.AttributeLists, type.Modifiers, f.Attributes, f.Modifiers); Name(type.Identifier, N(f.Name)); Bases(type.BaseList, f.Bases); Members(type.Members, f.Members); ParameterShapeAssert.Span(type.Span, f.Span); break;
                default: Assert.Fail("Unexpected type: " + type.Kind()); break;
            }
        }
        foreach (var d in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
        { var n = Match<DelegateDeclarationNode>(d); Header(d.AttributeLists, d.Modifiers, n.Attributes, n.Modifiers); ParameterShapeAssert.Compare(d.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(d.ReturnType, N(n.Return)); Name(d.Identifier, N(n.Name)); ParameterShapeAssert.Span(d.Span, n.Span); }
        foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>().Where(m => m.Parent is TypeDeclarationSyntax && m is not BaseTypeDeclarationSyntax && m is not DelegateDeclarationSyntax))
        {
            if (member.Parent is InterfaceDeclarationSyntax)
            { var n = Match<InterfaceMemberNode>(member); ParameterShapeAssert.Attributes(member.AttributeLists, n.Attributes); var optional = N(n).Child(1); Assert.Equal(member.Modifiers.Count == 0, optional.IsNull); if (member.Modifiers.Count != 0) Name(Assert.Single(member.Modifiers), optional.Child(0)); }
            else { var n = Match<MemberDeclarationNode>(member); Header(member.AttributeLists, member.Modifiers, n.Attributes, n.Modifiers); ParameterShapeAssert.Span(member.Span, n.Span); }
        }
        foreach (var e in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            var n = Match<EnumDeclarationNode>(e); Header(e.AttributeLists, e.Modifiers, n.Attributes, n.Modifiers); Name(e.Identifier, N(n.Name)); ParameterShapeAssert.Span(e.Span, n.Span);
            Assert.Equal(e.BaseList is not null, n.Base.HasValue);
            if (e.BaseList is not null) { ParameterShapeAssert.Span(e.BaseList.Span, n.Base.Value.Span); ParameterShapeAssert.Text(Assert.Single(e.BaseList.Types).Type, n.Base.Value.Child(1)); }
            Assert.Equal(e.Members.Count > 0, n.Members.HasValue);
            if (n.Members.HasValue)
            {
                var group = n.Members.Value; var list = new SeparatedList<EnumMemberNode>(ours.Tree, group.Child(0).Index); Assert.Equal(e.Members.Count, list.Count);
                for (int i = 0; i < list.Count; i++) ParameterShapeAssert.Span(e.Members[i].Span, list[i].Span);
                var commas = e.Members.GetSeparators().ToArray(); Assert.Equal(Math.Max(0, e.Members.Count - 1), list.SeparatorCount);
                for (int i = 0; i < list.SeparatorCount; i++) ParameterShapeAssert.Span(commas[i].Span, list.Separator(i).Span);
                Assert.Equal(commas.Length == e.Members.Count, !group.Child(1).IsNull);
                if (commas.Length == e.Members.Count) Name(commas[^1], group.Child(1));
            }
        }
        foreach (var e in root.DescendantNodes().OfType<EnumMemberDeclarationSyntax>())
        { var n = Match<EnumMemberNode>(e); ParameterShapeAssert.Attributes(e.AttributeLists, n.Attributes); Name(e.Identifier, N(n.Name)); Assert.Equal(e.EqualsValue is not null, n.Value.HasValue); if (e.EqualsValue is not null) { ParameterShapeAssert.Span(e.EqualsValue.Span, n.Value.Value.Span); ExpressionShapeAssert.Compare(e.EqualsValue.Value, SyntaxView.Cast<ExpressionNode>(ours.Tree, n.Value.Value.Child(1).Index)); } }
        foreach (var e in root.DescendantNodes().OfType<EventDeclarationSyntax>())
        { var n = Match<EventPropertyDeclarationNode>(e); ParameterShapeAssert.Text(e.Type, N(n.Type)); Qualified(e.Identifier, e.ExplicitInterfaceSpecifier, N(n.Name)); Accessors(e.AccessorList!, n.Accessors); }
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.Parent is InterfaceDeclarationSyntax)
            { var n = Match<InterfaceMethodNode>(method); ParameterShapeAssert.Compare(method.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(method.ReturnType, N(n.Return)); Name(method.Identifier, N(n.Name)); }
            else
            { var n = Match<MethodDeclarationNode>(method); ParameterShapeAssert.Compare(method.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(method.ReturnType, N(n.Return)); Qualified(method.Identifier, method.ExplicitInterfaceSpecifier, N(n.Name)); Body(method.Body, method.SemicolonToken, n.Body); }
        }
        foreach (var ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
        {
            var n = Match<ConstructorDeclarationNode>(ctor); ParameterShapeAssert.Compare(ctor.ParameterList, N(n.Parameters)); Name(ctor.Identifier, N(n.Name)); Body(ctor.Body, ctor.SemicolonToken, n.Body);
            Assert.Equal(ctor.Initializer is not null, n.Initializer.HasValue);
            if (ctor.Initializer is not null)
            { var init = n.Initializer.Value; ParameterShapeAssert.Span(ctor.Initializer.Span, init.Span); Name(ctor.Initializer.ThisOrBaseKeyword, init.Child(1)); ExpressionShapeAssert.CompareArguments(ctor.Initializer.ArgumentList, SyntaxView.Cast<ArgumentsNode>(ours.Tree, init.Child(2).Index)); }
        }
        foreach (var dtor in root.DescendantNodes().OfType<DestructorDeclarationSyntax>())
        { var n = Match<DestructorDeclarationNode>(dtor); Name(dtor.Identifier, N(n.Name)); Body(dtor.Body, dtor.SemicolonToken, n.Body); }
        foreach (var field in root.DescendantNodes().OfType<BaseFieldDeclarationSyntax>())
        {
            if (field.Parent is InterfaceDeclarationSyntax)
            { var n = Match<InterfaceEventNode>(field); ParameterShapeAssert.Text(field.Declaration.Type, N(n.Type)); Name(Assert.Single(field.Declaration.Variables).Identifier, N(n.Name)); continue; }
            Node variables; bool constant = field.Modifiers.Any(t => t.IsKind(SyntaxKind.ConstKeyword));
            if (field is EventFieldDeclarationSyntax) variables = N(Match<EventFieldDeclarationNode>(field).Variables);
            else if (constant) variables = N(Match<ConstantMemberNode>(field).Value);
            else variables = N(Match<FieldDeclarationNode>(field).Value);
            StatementShapeAssert.CompareDeclaration(field.Declaration, variables, constant);
        }
        foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Parent is InterfaceDeclarationSyntax)
            { var n = Match<InterfacePropertyNode>(prop); ParameterShapeAssert.Text(prop.Type, N(n.Type)); Name(prop.Identifier, N(n.Name)); Accessors(prop.AccessorList!, n.Accessors); }
            else { var n = Match<PropertyDeclarationNode>(prop); ParameterShapeAssert.Text(prop.Type, N(n.Type)); Qualified(prop.Identifier, prop.ExplicitInterfaceSpecifier, N(n.Name)); Accessors(prop.AccessorList!, n.Accessors); }
        }
        foreach (var indexer in root.DescendantNodes().OfType<IndexerDeclarationSyntax>())
        {
            if (indexer.Parent is InterfaceDeclarationSyntax) { var n = Match<InterfaceIndexerNode>(indexer); ParameterShapeAssert.Compare(indexer.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(indexer.Type, N(n.Type)); Accessors(indexer.AccessorList!, n.Accessors); }
            else { var n = Match<IndexerDeclarationNode>(indexer); ParameterShapeAssert.Compare(indexer.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(indexer.Type, N(n.Type)); Qualified(indexer.ThisKeyword, indexer.ExplicitInterfaceSpecifier, N(n.Name)); Accessors(indexer.AccessorList!, n.Accessors); }
        }
        foreach (var accessor in root.DescendantNodes().OfType<AccessorDeclarationSyntax>())
        {
            if (accessor.Parent!.Parent!.Parent is InterfaceDeclarationSyntax)
            { var n = Match<InterfaceAccessorNode>(accessor); ParameterShapeAssert.Attributes(accessor.AttributeLists, n.Attributes); Name(accessor.Keyword, n.Keyword); Name(accessor.SemicolonToken, N(n).Child(2)); }
            else if (accessor.IsKind(SyntaxKind.AddAccessorDeclaration) || accessor.IsKind(SyntaxKind.RemoveAccessorDeclaration))
            { var n = Match<EventAccessorNode>(accessor); ParameterShapeAssert.Attributes(accessor.AttributeLists, n.Attributes); Name(accessor.Keyword, n.Keyword); StatementShapeAssert.Compare(accessor.Body!, N(n.Body)); }
            else { var n = Match<AccessorNode>(accessor); ParameterShapeAssert.Attributes(accessor.AttributeLists, n.Attributes); Name(accessor.Keyword, n.Keyword); Body(accessor.Body, accessor.SemicolonToken, n.Body); }
        }
        foreach (var op in root.DescendantNodes().OfType<OperatorDeclarationSyntax>())
        { var n = Match<OperatorDeclarationNode>(op); ParameterShapeAssert.Compare(op.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(op.ReturnType, N(n.Return)); Name(op.OperatorToken, N(n.Operator)); Body(op.Body, op.SemicolonToken, n.Body); }
        foreach (var op in root.DescendantNodes().OfType<ConversionOperatorDeclarationSyntax>())
        { var n = Match<ConversionDeclarationNode>(op); ParameterShapeAssert.Compare(op.ParameterList, N(n.Parameters)); ParameterShapeAssert.Text(op.Type, N(n.Type)); Name(op.ImplicitOrExplicitKeyword, n.Mode); Body(op.Body, op.SemicolonToken, n.Body); }
    }

    [Theory]
    [InlineData("class C{D(){}}", "CS1520")]
    [InlineData("class C{public static C operator +(C x,C y,C z){return x;}}", "CS1534")]
    [InlineData("enum E:float{A}", "CS1008")]
    [InlineData("class C{public private int x;}", "CS0107")]
    public void Semantic_declaration_constraints_remain_compiler_observations(string source, string diagnostic)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Id == diagnostic);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "semantic-declarations.cs", []));
        Assert.True(ours.Accepted, string.Join(";", ours.Issues));
    }

    [Theory]
    [InlineData("interface I{void M(){}}")]
    [InlineData("interface I{int x;}")]
    [InlineData("interface I{I();}")]
    [InlineData("interface I{int P{get{return 1;}}}")]
    [InlineData("class C{void M(int x=1){}}")]
    [InlineData("class C{void M<T>(){}}")]
    [InlineData("class C{int ;}")]
    [InlineData("class C{void M(,){}}")]
    [InlineData("class C{void M(params int[] xs,int x){}}")]
    [InlineData("class C{void M(in int x){}}")]
    [InlineData("class C{void M(this C x){}}")]
    [InlineData("class C{int P{get;private set;}}")]
    [InlineData("class C{int this[]{get{return 1;}}}")]
    [InlineData("class C{event D E{add{}}}")]
    [InlineData("enum E{A=}")]
    [InlineData("class C{int P{get{}get{}}}")]
    [InlineData("class C{C():base(,) {}}")]
    [InlineData("interface I{int P{get;get;}}")]
    [InlineData("partial class C{}")]
    [InlineData("class C{int P=>1;}")]
    public void Malformed_later_and_class_only_forms_are_excluded(string source)
    {
        var reference = Oracle.Observe(source, LanguageVersion.CSharp1);
        Assert.Contains(reference.CompilationDiagnostics, d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var ours = CSharpParser.Parse(new(source, "CSharp1", "boundaries.cs", []));
        Assert.False(ours.Accepted);
    }
}
