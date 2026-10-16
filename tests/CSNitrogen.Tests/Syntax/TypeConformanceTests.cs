using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;

namespace CSNitrogen.Tests.Syntax;

public class TypeConformanceTests
{
    private static readonly Language Safe = CSharp1SafeComposition.CreateBuilder().Build();
    public static IEnumerable<object[]> Builtins() => new[]
    {
        "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
        "char", "float", "double", "decimal", "string", "object"
    }.Select(type => new object[] { type });

    [Theory, MemberData(nameof(Builtins))]
    public void Every_builtin_has_a_typed_base_and_exact_span(string source)
    {
        using var parsed = Safe.Parse(source, NamesTypesModule.TypeFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        var fragment = SyntaxView.Cast<TypeFragmentNode>(parsed.Tree, parsed.Tree.Root);
        var type = SyntaxView.Cast<TypeSyntaxTypeReference>(parsed.Tree, fragment.Value.Index);
        Assert.Equal(0, type.Ranks.Count);
        var builtin = SyntaxView.Cast<BuiltinTypeNameTypeBase>(parsed.Tree, type.Element.Index);
        Assert.Equal(new TextSpan(0, source.Length), builtin.Value.Span);
    }

    [Theory]
    [InlineData("A", 1)][InlineData("A.B.C", 3)][InlineData("@int.@void", 2)]
    [InlineData(@"N.cl\u0061ss", 2)][InlineData("var.dynamic.async", 3)]
    public void Qualified_names_retain_raw_parts_and_spans(string source, int count)
    {
        using var parsed = Safe.Parse(source, NamesTypesModule.TypeFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        var type = SyntaxView.Cast<TypeSyntaxTypeReference>(parsed.Tree, parsed.Tree.Child(parsed.Tree.Root, 0));
        var named = SyntaxView.Cast<NamedTypeNameTypeBase>(parsed.Tree, type.Element.Index);
        Assert.Equal(count, named.Name.Parts.Count);
        int offset = 0;
        foreach (var part in named.Name.Parts)
        {
            Assert.Equal(new TextSpan(offset, part.ToString().Length), part.Span);
            offset += part.Span.Length + 1;
        }
        Assert.Equal(new TextSpan(0, source.Length), named.Name.Span);
    }

    [Theory]
    [InlineData("int[]", 1)][InlineData("N.T[,][][,,]", 3)]
    [InlineData("@void[,,,,]", 1)][InlineData("A [ , ] [ ]", 2)]
    public void Array_ranks_retain_order_commas_and_bracket_spans(string source, int count)
    {
        using var parsed = Safe.Parse(source, NamesTypesModule.TypeFragment);
        Assert.True(parsed.Success && !parsed.HasErrors);
        var type = SyntaxView.Cast<TypeSyntaxTypeReference>(parsed.Tree, parsed.Tree.Child(parsed.Tree.Root, 0));
        Assert.Equal(count, type.Ranks.Count);
        int next = source.IndexOf('[');
        foreach (var rank in type.Ranks)
        {
            int end = source.IndexOf(']', next);
            Assert.Equal(new TextSpan(next, end - next + 1), rank.Span);
            Assert.Equal(source[next..(end + 1)].Count(c => c == ','), rank.Commas.Count);
            foreach (var comma in rank.Commas) Assert.Equal(",", comma.ToString());
            next = source.IndexOf('[', end + 1);
        }
        Assert.Equal(new TextSpan(0, source.Length), type.Span);
    }

    [Theory]
    [InlineData("")][InlineData(".")][InlineData("A.")][InlineData(".A")][InlineData("A..B")]
    [InlineData("int.X")][InlineData("A.int")][InlineData("void")][InlineData("void[]")]
    [InlineData("A[")][InlineData("A]")][InlineData("A[1]")][InlineData("A[,1]")]
    [InlineData("A[][]tail")][InlineData("A B")][InlineData("A<T>")][InlineData("global::A")]
    [InlineData("int?")][InlineData("int*?")][InlineData("int*")][InlineData("void*")]
    public void Safe_type_fragment_rejects_malformed_later_and_unsafe_forms(string source)
    {
        using var parsed = Safe.Parse(source, NamesTypesModule.TypeFragment);
        Assert.False(parsed.Success && !parsed.HasErrors);
    }

    [Theory]
    [InlineData("void", true)][InlineData("int", true)][InlineData("A[]", true)]
    [InlineData("voided", true)][InlineData("void[]", false)][InlineData("void.X", false)]
    [InlineData("ref int", false)][InlineData("void trailing", false)]
    public void Return_type_has_its_own_void_alternative(string source, bool expected)
    {
        using var parsed = Safe.Parse(source, NamesTypesModule.ReturnType);
        Assert.Equal(expected, parsed.Success && !parsed.HasErrors);
        if (expected)
        {
            Assert.Equal(new TextSpan(0, source.Length), parsed.Tree.Span(parsed.Tree.Root));
            Assert.Equal(source == "void" ? 0 : 1,
                Enumerable.Range(0, parsed.Tree.NodeCount).Count(i => TypeSyntaxTypeReference.Is(parsed.Tree, i)));
        }
    }

    [Theory]
    [InlineData("name", "A.B", true)][InlineData("name", "A.", false)]
    [InlineData("name", "int", false)][InlineData("name", "A int", false)]
    [InlineData("builtin", "uint", true)][InlineData("builtin", "void", false)]
    [InlineData("builtin", "dynamic", false)][InlineData("builtin", "uint[]", false)]
    [InlineData("rank", "[,]", true)][InlineData("rank", "[1]", false)]
    [InlineData("rank", "[][,,]", false)][InlineData("rank", "[,", false)]
    public void Leaf_productions_require_full_consumption(string production, string source, bool expected)
    {
        var rule = production switch
        {
            "name" => NamesTypesModule.QualifiedName,
            "builtin" => NamesTypesModule.BuiltinType,
            _ => NamesTypesModule.ArrayRank
        };
        using var parsed = Safe.Parse(source, rule);
        Assert.Equal(expected, parsed.Success && !parsed.HasErrors);
        if (expected) Assert.Equal(new TextSpan(0, source.Length), parsed.Tree.Span(parsed.Tree.Root));
    }

    [Theory]
    [InlineData("class C{void M(){} delegate void D(); object T(){return typeof(void);}}", true)]
    [InlineData("interface I{void M();N.T[,][] P{get;}}", true)]
    [InlineData("class C{void x;}", false)][InlineData("class C{void[] x;}", false)]
    [InlineData("class C{void M(void x){}}", false)][InlineData("class C{void P{get{return;}}}", false)]
    [InlineData("class C{object M(){return new void();}}", false)]
    [InlineData("class C{object M(){return typeof(void[]);}}", false)]
    [InlineData("class C{void M(){void x;}}", false)]
    [InlineData("unsafe class C{void* p;int*[] q;void** M(){return p;}}", true)]
    public void Owning_source_type_contexts(string source, bool expected)
    {
        using var parsed = CSharpParser.Parse(new(source, "CSharp1", "types.cs", []));
        Assert.True(expected == parsed.Accepted, source + " :: " + string.Join(";", parsed.Issues));
    }

    [Fact]
    public void Source_trivia_preserves_qualified_parts_and_rank_coordinates()
    {
        const string source = "class C{ N/*a*/ . @T [/*b*/,] [] x; }";
        using var parsed = CSharpParser.Parse(new(source, "CSharp1", "trivia.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
        var tree = parsed.Tree;
        int node = Assert.Single(Enumerable.Range(0, tree.NodeCount), i => NamedTypeNameTypeBase.Is(tree, i));
        var name = SyntaxView.Cast<NamedTypeNameTypeBase>(tree, node).Name;
        Assert.Equal("N", name.Parts[0].ToString()); Assert.Equal("@T", name.Parts[1].ToString());
        Assert.Equal(source.IndexOf("@T", StringComparison.Ordinal), name.Parts[1].Span.Start);
        var ranks = Enumerable.Range(0, tree.NodeCount).Where(i => ArrayRankNode.Is(tree, i))
            .Select(i => SyntaxView.Cast<ArrayRankNode>(tree, i)).OrderBy(rank => rank.Span.Start).ToArray();
        Assert.Equal(2, ranks.Length);
        Assert.Equal("[/*b*/,]", source.Substring(ranks[0].Span.Start, ranks[0].Span.Length));
        Assert.Equal(source.IndexOf(',', StringComparison.Ordinal), ranks[0].Commas[0].Span.Start);
    }
}
