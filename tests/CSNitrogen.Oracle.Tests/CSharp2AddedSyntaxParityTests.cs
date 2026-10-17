using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.Oracle.Tests;
public class CSharp2AddedSyntaxParityTests
{
    [Theory]
    [InlineData("class C { int?[] x; int M(int? x,int? y) { return x ?? y ?? 0; } }")]
    [InlineData("class C { object d=delegate(ref int x,out int y) { y=x; }; }")]
    [InlineData("class C { object d=delegate { }; }")]
    [InlineData("class C { object M() { yield return 1; yield break; } }")]
    [InlineData("class C { object M() { return default(G<int>); } }")]
    [InlineData("partial class C { int P { get { return 1; } private set {} } }")]
    [InlineData("extern alias A; using X=A::N.T; class C { A::N.List<int>? x; }")]
    [InlineData("unsafe struct S { public fixed int x[5],y[10]; }")]
    [InlineData("unsafe class C { void M() { p->F<int>(); x=(G<int>*)p; } }")]
    public void Added_syntax_shapes_have_matching_authored_spans(string source)
    {
        var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp2));
        Assert.DoesNotContain(reference.GetDiagnostics(),d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var result=CSharpParser.Parse(new(source,"CSharp2","added-oracle.cs",[]));Assert.True(result.Accepted,string.Join(";",result.Issues));
        var root=reference.GetRoot();var tree=result.Tree;
        void Spans(IEnumerable<Microsoft.CodeAnalysis.SyntaxNode> expected,params string[] kinds)
        {
            var a=expected.Select(n=>(n.Span.Start,n.Span.Length)).OrderBy(s=>s.Start).ThenBy(s=>s.Length).ToArray();
            var b=Enumerable.Range(0,tree.NodeCount).Where(n=>kinds.Contains(tree.Language!.GetKindName(tree.Kind(n)),StringComparer.Ordinal))
                .Select(n=>(tree.Span(n).Start,tree.Span(n).Length)).OrderBy(s=>s.Start).ThenBy(s=>s.Length).ToArray();Assert.Equal(a,b);
        }
        Spans(root.DescendantNodes().OfType<NullableTypeSyntax>(),"NullableTypeName");
        Spans(root.DescendantNodes().OfType<BinaryExpressionSyntax>().Where(n=>n.IsKind(SyntaxKind.CoalesceExpression)),"NullCoalescingExpression");
        Spans(root.DescendantNodes().OfType<AnonymousMethodExpressionSyntax>(),"AnonymousMethodExpression");
        Spans(root.DescendantNodes().OfType<YieldStatementSyntax>(),"YieldReturnStatement","YieldBreakStatement");
        Spans(root.DescendantNodes().OfType<DefaultExpressionSyntax>(),"DefaultValueExpression");
        Spans(root.DescendantNodes().OfType<ExternAliasDirectiveSyntax>(),"ExternalAliasDirective");
        Spans(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(n=>n.ArgumentList is not null),"FixedBufferDeclarator");
        Spans(root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Where(n=>n.IsKind(SyntaxKind.PointerMemberAccessExpression) && n.Name is GenericNameSyntax),"GenericPointerMemberExpression");
    }
    [Theory][InlineData("#line vendor\nclass C {}")][InlineData("#line 0\nclass C {}")][InlineData("#line 9999999999999999999999999999\nclass C {}")]
    public void Fourth_edition_line_rules_remain_explicit_reference_discrepancies(string source)
    {
        var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp2));Assert.Contains(reference.GetDiagnostics(),d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        using var result=CSharpParser.Parse(new(source,"CSharp2","historical-line.cs",[]));Assert.True(result.Accepted);Assert.True(result.ProfileComplete);
    }
}
