using CSNitrogen.Parsing;
using Nitrogen;
using CSNitrogen.Syntax;
using CSNitrogen.Profiles;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2GenericsTests
{
    [Theory]
    [InlineData("class Box<T> where T : class, new() { T value; }","GenericParameters")]
    [InlineData("using L = N.List<int>; class C { L x; }","GenericTypeSuffix")]
    [InlineData("class C { T I<T>.M<U>(U x) { return x; } }","GenericMemberNameSuffix")]
    [InlineData("struct Box<T> { N.List<T[]> value; }","GenericTypeName")]
    [InlineData("interface I<T> { T M<U>(U x) where U : T; }","GenericParameters")]
    [InlineData("delegate T D<T>(T value) where T : class;","GenericParameters")]
    [InlineData("class C { T M<T>(T x) where T : class { return x; } }","GenericParameters")]
    [InlineData("class C { object M() { return F<G<int>>(x); } }","GenericNameExpression")]
    [InlineData("class C { object M() { return x.F<int>(x); } }","GenericMemberExpression")]
    [InlineData("class C { object M() { return (N.List<int>)x; } }","GenericCast")]
    [InlineData("class C { object M() { return (N.List<int>[])-x; } }","GenericArrayCast")]
    [InlineData("class C { object M() { return typeof(N.List<>); } }","UnboundTypeOfExpression")]
    public void Public_CSharp2_generics_have_typed_nodes_and_preceding_version_rejection(string source,string kind)
    {
        using var first=CSharpParser.Parse(new(source,"CSharp1","generic.cs",[]));Assert.False(first.Accepted);
        using var second=CSharpParser.Parse(new(source,"CSharp2","generic.cs",[]));
        Assert.True(second.Accepted,string.Join("; ",second.Issues.Select(i=>i.Message)));
        Assert.False(second.ProfileComplete);
        Assert.Contains(Enumerable.Range(0,second.Tree.NodeCount),n=>second.Tree.Language!.GetKindName(second.Tree.Kind(n))==kind);
    }
    [Theory]
    [InlineData("class C { bool M() { return F<T> + x; } }")]
    [InlineData("class C { bool M() { return F<G<int>(x); } }")]
    [InlineData("class C { int M() { return a >> b; } }")]
    public void Relational_and_shift_fallback_remain_accepted(string source)
    {using var result=CSharpParser.Parse(new(source,"CSharp2","operators.cs",[]));Assert.True(result.Accepted);}
    [Theory]
    [InlineData("class C where T : class {}")]
    [InlineData("class C<T> where T : new(), class {}")]
    [InlineData("class C<T> where T : T, struct {}")]
    [InlineData("class C { T M() where T : class { return x; } }")]
    [InlineData("class C<> {}")][InlineData("class C<T,> {}")]
    [InlineData("class C<T {}")][InlineData("class C<out T> {}")]
    [InlineData("class C { object M() { return F<,>(x); } }")]
    public void Malformed_generics_and_later_variance_are_rejected(string source)
    {using var result=CSharpParser.Parse(new(source,"CSharp2","bad.cs",[]));Assert.False(result.Accepted);}
    [Fact]
    public void Generic_parameters_constraints_arguments_and_separators_retain_original_spans()
    {
        const string source="class Box<[A] T,U> where T : class, new() where U : N.I<T> { N.List<T[,],U[]> value; }";
        using var result=CSharpParser.Parse(new(source,"CSharp2","typed.cs",[]));
        Assert.True(result.Accepted,string.Join("; ",result.Issues));
        var tree=result.Tree;
        var declaration=Assert.Single(result.Classes);
        Assert.Equal("Box",declaration.Name.ToString());
        var parameters=SyntaxView.Cast<GenericParametersTypeParameters>(tree,declaration.TypeParameters.Value.Index);
        Assert.Equal(2,parameters.Items.Count);Assert.Equal("T",parameters.Items[0].Name.ToString());
        Assert.Single(Enumerable.Range(0,parameters.Items[0].Attributes.Count));
        Assert.Equal("U",parameters.Items[1].Name.ToString());
        Assert.Equal(new TextSpan(source.IndexOf('U'),1),parameters.Items[1].Name.Span);
        var constraints=SyntaxView.Cast<GenericConstraintsTypeConstraints>(tree,declaration.Constraints.Value.Index);
        Assert.Equal(2,constraints.Items.Count);Assert.Equal("T",constraints.Items[0].Name.ToString());
        Assert.Equal("U",constraints.Items[1].Name.ToString());
        int node=Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>GenericTypeNameTypeBase.Is(tree,n) && tree.Span(n).Start==source.IndexOf("N.List",StringComparison.Ordinal));
        var name=SyntaxView.Cast<GenericTypeNameTypeBase>(tree,node).Name;
        Assert.Equal("List",name.Name.ToString());Assert.Equal(2,name.Arguments.Items.Count);
        Assert.Equal("T[,]",source.Substring(name.Arguments.Items[0].Span.Start,name.Arguments.Items[0].Span.Length));Assert.Equal("U[]",source.Substring(name.Arguments.Items[1].Span.Start,name.Arguments.Items[1].Span.Length));
        Assert.Equal(",",name.Arguments.Items.Separator(0).ToString());
        Assert.Equal(new TextSpan(source.IndexOf(",U[]",StringComparison.Ordinal),1),name.Arguments.Items.Separator(0).Span);
    }
    [Fact]
    public void Named_selection_respects_minimum_versions_and_custom_dependencies_remain_explicit()
    {
        Assert.DoesNotContain("csharp2.generics",ProfileResolver.Resolve("CSharp1").Profile!.Features);
        Assert.Contains("csharp2.generics",ProfileResolver.Resolve("CSharp2").Profile!.Features);
        Assert.Contains("csharp2.generics",ProfileResolver.Resolve("CSharp14").Profile!.Features);
        Assert.Contains(ProfileResolver.ResolveCustom("only-generic",["csharp2.generics"]).Issues,i=>i.Code=="missing-requirement");
        var plain=CSharp1Composition.CreateBuilder().Build();
        using var missing=plain.Parse("class Box<T> {}",CSharp1Composition.Start);
        Assert.False(missing.Success && !missing.HasErrors);
        Assert.DoesNotContain(plain.Modules,m=>m.Name=="CSharp.Generics");
    }
}
