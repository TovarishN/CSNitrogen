using CSNitrogen.Parsing;
using CSNitrogen.Preprocessing;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public unsafe class UnsafeConformanceTests
{
    private static readonly Lazy<Language> Safe = new(() => CSharp1SafeComposition.CreateBuilder().Build());

    public static IEnumerable<object[]> UnsafeSources()
    {
        foreach (string type in new[] { "int*", "T**", "void*", "void**", "int*[]", "N.T*[,]" })
            yield return [$"class C{{{type} p;}}"];
        foreach (string expression in new[] { "*p", "**p", "&x", "&a[i]", "p->Value", "p->Next->Value", "(int*)p", "(T**)p", "(void*)p", "sizeof(T)", "sizeof(int*)", "sizeof(T[,])" })
            yield return [$"class C{{object M(){{return {expression};}}}}"];
        foreach (string source in new[]{
            "unsafe class C{}","class C{unsafe void M(){}}","class C{void M(){unsafe{F();}}}",
            "class C{void M(){int* p=stackalloc int[4];}}",
            "class C{void M(){fixed(int* p=&x){*p=1;}}int x;}",
            "class C{void M(){fixed(int* p=a,q=b)p->M();}}",
            "class C{void M(){fixed(void** p=x);}}",
            "class C{int* M(int** p){return *p;}}","delegate void* D(int* x);",
            "struct S{int* p;}","interface I{int* M(void* x);}",
            "class C{int* P{get{return p;}set{p=value;}}}",
            "class C{int* this[int i]{get{return p;}}}"
        }) yield return [source];
    }

    [Theory, MemberData(nameof(UnsafeSources))]
    public void Unsafe_module_omission_rejects_each_selected_surface(string source)
    {
        using var full = CSharpParser.Parse(new(source, "CSharp1", "unsafe.cs", []));
        Assert.True(full.Accepted, source + " :: " + string.Join(";", full.Issues));
        using var safe = Safe.Value.Parse(source, CSharp1SafeComposition.Start);
        bool accepted = safe.Success && !safe.HasErrors && !CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0)).Any(i => i.Severity == DiagnosticSeverity.Error);
        Assert.False(accepted, source);
        Assert.DoesNotContain(Safe.Value.Modules, m => m.Name == "CSharp.Unsafe");
        Assert.Contains(full.Tree.Language!.Modules, m => m.Name == "CSharp.Unsafe");
        AssertNoUnsafeNodes(safe.Tree);
    }

    [Theory]
    [InlineData("bool")]
    [InlineData("byte")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("ushort")]
    [InlineData("int")]
    [InlineData("uint")]
    [InlineData("long")]
    [InlineData("ulong")]
    [InlineData("char")]
    [InlineData("float")]
    [InlineData("double")]
    [InlineData("decimal")]
    [InlineData("string")]
    [InlineData("object")]
    public void Builtin_sizeof_surface_remains_in_safe_grammar(string type)
    {
        string source = $"class C{{int M(){{return sizeof({type});}}}}";
        using var full = CSharpParser.Parse(new(source, "CSharp1", "sizeof.cs", [])); Assert.True(full.Accepted);
        using var safe = Safe.Value.Parse(source, CSharp1SafeComposition.Start);
        Assert.True(safe.Success && !safe.HasErrors); Assert.Empty(CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0)));
        AssertNoUnsafeNodes(safe.Tree);
    }

    [Theory]
    [InlineData("a*b")]
    [InlineData("a&b")]
    [InlineData("a*b+c")]
    [InlineData("(a)&b")]
    [InlineData("a<<b")]
    [InlineData("(a+b)*c")]
    [InlineData("F(a,b)")]
    [InlineData("a[0]")]
    public void Shared_expression_symbols_do_not_require_unsafe(string expression)
    {
        string source = $"class C{{object M(){{return {expression};}}}}";
        using var full = CSharpParser.Parse(new(source, "CSharp1", "shared.cs", [])); Assert.True(full.Accepted);
        using var safe = Safe.Value.Parse(source, CSharp1SafeComposition.Start);
        Assert.True(safe.Success && !safe.HasErrors); Assert.Empty(CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0)));
        AssertNoUnsafeNodes(safe.Tree);
    }

    [Fact]
    public void Configured_membership_differs_by_exactly_the_unsafe_module()
    {
        var full = CSharp1Composition.CreateBuilder().Build();
        Assert.Equal(full.Modules.Where(m => m.Name != "CSharp.Unsafe").Select(m => m.Name).Order(StringComparer.Ordinal),
            Safe.Value.Modules.Select(m => m.Name).Order(StringComparer.Ordinal));
        Assert.Single(full.Modules, m => m.Name == "CSharp.Unsafe");
        for (int i = 0; i < 3; i++)
        {
            using var first = full.Parse("class C{int* p;}", CSharp1Composition.Start); Assert.True(first.Success);
            using var second = Safe.Value.Parse("class C{int* p;}", CSharp1SafeComposition.Start); Assert.False(second.Success && !second.HasErrors);
            using var third = full.Parse("class C{int* p;}", CSharp1Composition.Start); Assert.True(third.Success);
        }
    }

    [Fact]
    public void Inactive_unsafe_text_has_no_nodes_in_the_safe_composition()
    {
        const string source = "#if false\nunsafe class Hidden{int* p=stackalloc int[1];}\n#endif\nclass C{int M(){return sizeof(int);}}";
        var preprocessing = ConditionalPreprocessor.Process(new(source, new(1, 0), []));
        Assert.False(preprocessing.HasErrors); Assert.Equal(source.Length, preprocessing.MaskedText.Length);
        using var safe = Safe.Value.Parse(preprocessing.MaskedText, CSharp1SafeComposition.Start);
        Assert.True(safe.Success && !safe.HasErrors); Assert.Empty(CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0)));
        AssertNoUnsafeNodes(safe.Tree);
    }

    [Theory]
    [InlineData("class C{void M(){T*p;}}", true)]
    [InlineData("class C{void M(){T**p;}}", false)]
    public void Ambiguous_pointer_declarations_are_not_accepted_as_arithmetic_statements(string source, bool rawAccepted)
    {
        using var full = CSharpParser.Parse(new(source, "CSharp1", "ambiguous.cs", [])); Assert.True(full.Accepted);
        using var safe = Safe.Value.Parse(source, CSharp1SafeComposition.Start);
        Assert.Equal(rawAccepted, safe.Success && !safe.HasErrors);
        var issues = CSharpSyntaxPolicy.Check(safe.Tree, new(1, 0));
        if (rawAccepted) Assert.Contains(issues, i => i.Code == "CSN2001");
        Assert.False(safe.Success && !safe.HasErrors && !issues.Any());
        AssertNoUnsafeNodes(safe.Tree);
    }

    private static void AssertNoUnsafeNodes(SyntaxTree tree)
    {
        for (int i = 0; i < tree.NodeCount; i++) Assert.NotEqual(UnsafeModule.Instance.Id, SyntaxKinds.ModuleOf(tree.Kind(i)));
    }
}
