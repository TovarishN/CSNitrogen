using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class CSharp2GenericsParityTests
{
    [Theory]
    [InlineData("class Box<T,U> where T : class, new() where U : N.I<T> { N.List<T[,],U[]> value; }")]
    [InlineData("interface I<T> { T M<U>(U x) where U : T; }")]
    [InlineData("delegate T D<T>(T x) where T : class;")]
    [InlineData("class C { object M() { return F<G<int>>(x); } }")]
    [InlineData("class C { object M() { return x.F<int>(x); } }")]
    [InlineData("using L = N.List<int>; class C { T I<T>.M<U>(U x) { return x; } }")]
    public void Typed_generic_list_items_separators_and_constraint_spans_match_the_reference(string source)
    {
        var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp2));
        Assert.DoesNotContain(reference.GetDiagnostics(),d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var result=CSharpParser.Parse(new(source,"CSharp2","generic-oracle.cs",[]));
        Assert.True(result.Accepted,string.Join(";",result.Issues));
        var tree=result.Tree;var root=reference.GetRoot();
        int[] Nodes(Func<int,bool> predicate)=>Enumerable.Range(0,tree.NodeCount).Where(predicate).ToArray();
        var args=Nodes(n=>TypeArgumentListNode.Is(tree,n)).Select(n=>SyntaxView.Cast<TypeArgumentListNode>(tree,n)).OrderBy(n=>n.Span.Start).ToArray();
        var expected=root.DescendantNodes().OfType<TypeArgumentListSyntax>().OrderBy(n=>n.SpanStart).ToArray();
        Assert.Equal(expected.Length,args.Length);
        for(int i=0;i<args.Length;i++)
        {
            Same(expected[i].Span,args[i].Span);Assert.Equal(expected[i].Arguments.Count,args[i].Items.Count);
            for(int j=0;j<args[i].Items.Count;j++)Same(expected[i].Arguments[j].Span,args[i].Items[j].Span);
            for(int j=0;j<args[i].Items.SeparatorCount;j++)Same(expected[i].Arguments.GetSeparator(j).Span,args[i].Items.Separator(j).Span);
            Same(expected[i].LessThanToken.Span,tree.Span(tree.Child(args[i].Index,0)));
            Same(expected[i].GreaterThanToken.Span,tree.Span(tree.Child(args[i].Index,2)));
        }
        var actualParameters=Nodes(n=>GenericParametersTypeParameters.Is(tree,n)).Select(n=>SyntaxView.Cast<GenericParametersTypeParameters>(tree,n)).OrderBy(n=>n.Span.Start).ToArray();
        var parameters=root.DescendantNodes().OfType<TypeParameterListSyntax>().OrderBy(n=>n.SpanStart).ToArray();Assert.Equal(parameters.Length,actualParameters.Length);
        for(int i=0;i<parameters.Length;i++)
        {
            Same(parameters[i].Span,actualParameters[i].Span);Assert.Equal(parameters[i].Parameters.Count,actualParameters[i].Items.Count);
            for(int j=0;j<parameters[i].Parameters.Count;j++)Same(parameters[i].Parameters[j].Identifier.Span,actualParameters[i].Items[j].Name.Span);
        }
        var constraints=Nodes(n=>ConstraintClauseNode.Is(tree,n)).Select(n=>tree.Span(n)).OrderBy(n=>n.Start).ToArray();
        var expectedConstraints=root.DescendantNodes().OfType<TypeParameterConstraintClauseSyntax>().OrderBy(n=>n.SpanStart).ToArray();Assert.Equal(expectedConstraints.Length,constraints.Length);
        for(int i=0;i<constraints.Length;i++)Same(expectedConstraints[i].Span,constraints[i]);
    }
    private static void Same(Microsoft.CodeAnalysis.Text.TextSpan expected,Nitrogen.TextSpan actual)
    {Assert.Equal(expected.Start,actual.Start);Assert.Equal(expected.Length,actual.Length);}
}
