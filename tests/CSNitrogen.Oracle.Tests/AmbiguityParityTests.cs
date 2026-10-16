using CSNitrogen.Tests.Probes;
using Microsoft.CodeAnalysis.CSharp;
using Nitrogen;
namespace CSNitrogen.Oracle.Tests;
public class AmbiguityParityTests
{
    [Theory]
    [InlineData("a + b * c","Add",SyntaxKind.AddExpression)]
    [InlineData("(x)","Paren",SyntaxKind.ParenthesizedExpression)]
    [InlineData("(T)x","Cast",SyntaxKind.CastExpression)]
    [InlineData("a < b","Less",SyntaxKind.LessThanExpression)]
    [InlineData("a >> b","Shift",SyntaxKind.RightShiftExpression)]
    [InlineData("from + select","Add",SyntaxKind.AddExpression)]
    [InlineData("F<T>(x)","GenericCall",SyntaxKind.InvocationExpression)]
    [InlineData("F<G<int>>(x)","GenericCall",SyntaxKind.InvocationExpression)]
    [InlineData("x => x","Lambda",SyntaxKind.SimpleLambdaExpression)]
    public void Probe_shape_matches_Roslyn(string source,string nitrogenKind,SyntaxKind roslynKind)
    {
        var roslyn=SyntaxFactory.ParseExpression(source,options:new CSharpParseOptions(LanguageVersion.CSharp14),consumeFullText:true);
        Assert.False(roslyn.ContainsDiagnostics); Assert.Equal(roslynKind,roslyn.Kind());
        (Language language,Rule start)=nitrogenKind switch {
            "Lambda" => (LambdaProbeComposition.CreateBuilder().Build(),LambdaProbeComposition.Start),
            "GenericCall" => (GenericProbeComposition.CreateBuilder().Build(),GenericProbeComposition.Start),
            _ => (ExpressionProbeComposition.CreateBuilder().Build(),ExpressionProbeComposition.Start)
        };
        using var parsed=language.Parse(source,start); Assert.True(parsed.Success); Assert.False(parsed.HasErrors);
        int value=parsed.Tree.Child(parsed.Tree.Root,0);
        Assert.Equal(nitrogenKind,language.GetKindName(parsed.Tree.Kind(value)));
        Assert.Equal(roslyn.Span.Start,parsed.Tree.Span(value).Start); Assert.Equal(roslyn.Span.Length,parsed.Tree.Span(value).Length);
    }
}
