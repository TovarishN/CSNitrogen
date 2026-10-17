using CSNitrogen.Syntax;
using CSNitrogen.Profiles;
using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2CompositionTests
{
    [Theory]
    [InlineData("CSharp.Generics", "class G<T> {}", "CSharp.GenericUnsafe")]
    [InlineData("CSharp.Generics", "class C:B { int N() { return base.M<int>(); } }", "CSharp.GenericUnsafe")]
    [InlineData("CSharp.NullableValues", "class C { int? x; }", "")]
    [InlineData("CSharp.AnonymousMethods", "class C { object x=delegate {}; }", "")]
    [InlineData("CSharp.Iterators", "class C { object M() { yield return 1; } }", "")]
    [InlineData("CSharp.DefaultValues", "class C { int x=default(int); }", "")]
    [InlineData("CSharp.DeclarationForms", "partial class C {}", "")]
    [InlineData("CSharp.AliasQualification", "extern alias A; class C { A::T x; }", "")]
    [InlineData("CSharp.GenericUnsafe", "class C { void M() { x=(G<int>*)p; } }", "")]
    [InlineData("CSharp.FixedBuffers", "unsafe struct S { fixed int x[5]; }", "")]
    public void Removing_a_module_rejects_its_surface_without_disabling_an_ordinary_class(string omitted,string source,string dependent)
    {
        var full=CSharp2Composition.CreateBuilder().Build();using var present=full.Parse(source,CSharp2Composition.Start);
        Assert.True(present.Success && !present.HasErrors);
        var builder=new LanguageBuilder();foreach(var module in full.Modules)if(module.Name!=omitted && module.Name!=dependent)builder.Add(module);
        var partial=builder.Build();Assert.DoesNotContain(partial.Modules,m=>m.Name==omitted);
        using var absent=partial.Parse(source,CSharp2Composition.Start);Assert.False(absent.Success && !absent.HasErrors);
        using var ordinary=partial.Parse("class C {}",CSharp2Composition.Start);Assert.True(ordinary.Success && !ordinary.HasErrors);
    }
    [Theory][InlineData("csharp2.generic-unsafe")][InlineData("csharp2.fixed-buffers")][InlineData("csharp2.alias-qualification")]
    public void Custom_capabilities_do_not_silently_fill_required_bundles(string capability)
    {var result=ProfileResolver.ResolveCustom("incomplete",[capability]);Assert.Null(result.Profile);Assert.Contains(result.Issues,i=>i.Code=="missing-requirement");}
    [Theory][InlineData(8)][InlineData(32)][InlineData(64)]
    public void Nested_generic_types_and_malformed_closers_have_bounded_witnesses(int depth)
    {
        string type=string.Concat(Enumerable.Repeat("G<",depth))+"int"+new string('>',depth);
        using var valid=CSharpParser.Parse(new("class C { "+type+" value; }","CSharp2","depth.cs",[]));Assert.True(valid.Accepted);
        Assert.Equal(depth,Enumerable.Range(0,valid.Tree.NodeCount).Count(n=>TypeArgumentListNode.Is(valid.Tree,n)));
        using var invalid=CSharpParser.Parse(new("class C { "+type[..^1]+" value; }","CSharp2","depth.cs",[]));Assert.False(invalid.Accepted);
    }
    [Fact]
    public void Public_named_factory_modules_and_root_match_resolved_configuration()
    {
        foreach(string name in Enumerable.Range(2,13).Select(n=>$"CSharp{n}").Concat(["CSharp7_1","CSharp7_2","CSharp7_3"]))
        {
            var profile=ProfileResolver.Resolve(name).Profile!;
            using var result=CSharpParser.Parse(new("",name,"factory.cs",[]));Assert.True(result.Accepted);
            var expected=profile.Grammars.Select(g=>"CSharp."+Path.GetFileNameWithoutExtension(g)).Order(StringComparer.Ordinal);
            Assert.Equal(expected,result.Tree.Language!.Modules.Select(m=>m.Name).Order(StringComparer.Ordinal));
            Assert.Equal("CompilationUnit",result.Tree.Language.GetKindName(result.Tree.Kind(result.Tree.Root)));
            Assert.Equal("CSharp.CompilationUnits",Assert.Single(result.Tree.Language.Modules,m=>m.Id==SyntaxKinds.ModuleOf(result.Tree.Kind(result.Tree.Root))).Name);
        }
    }
}
