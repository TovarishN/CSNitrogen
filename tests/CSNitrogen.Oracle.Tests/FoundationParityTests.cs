using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class FoundationParityTests
{
    [Theory][InlineData("")][InlineData("class C {}")][InlineData("/*a*/ class C {} //b\n class D {}")][InlineData("class C {")][InlineData("class C {} @")]
    public void Supported_foundation_matches_syntax_observations(string source)
    {
        var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        using var parsed=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));
        Assert.Equal(!oracle.SyntaxDiagnostics.Any(d=>d.Severity==DiagnosticSeverity.Error),parsed.Accepted);
        if(!parsed.Accepted) return;
        var classes=oracle.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
        Assert.Equal(classes.Length,parsed.Classes.Count);
        for(int i=0;i<classes.Length;i++)
        {
            var token=parsed.Classes[i].Name;
            Assert.Equal(classes[i].Identifier.Text,token.ToString());
            Assert.Equal(classes[i].Identifier.Span.Start,parsed.Tree.Span(token.Index).Start);
            Assert.Equal(classes[i].Identifier.Span.Length,parsed.Tree.Span(token.Index).Length);
        }
    }
}
