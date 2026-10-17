using Nitrogen;
using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class PendingNormativeGapTests
{
    [Theory][InlineData("int.MaxValue")][InlineData("string.Empty")][InlineData("bool.TrueString")]
    public void Named_cast_accepts_a_predefined_type_member_operand(string operand)
    {
        string source="class C{object M(){return (T)"+operand+";}}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","cast.cs",[]));
        Assert.True(doc.Accepted,string.Join(";",doc.Issues));
        int node=Assert.Single(Enumerable.Range(0,doc.Tree.NodeCount),n=>CastExpressionExpression.Is(doc.Tree,n));
        var cast=SyntaxView.Cast<CastExpressionExpression>(doc.Tree,node);
        Assert.True(PredefinedMemberExpressionExpression.Is(doc.Tree,cast.Value.Index));
        Assert.Equal(source.IndexOf(operand,StringComparison.Ordinal),cast.Value.Span.Start);
    }
    [Theory][InlineData("x+=1")][InlineData("X=y=1")]
    public void Historical_attribute_argument_uses_expression_instead_of_modern_non_assignment(string argument)
    {
        using var historical=CSharpParser.Parse(new("[A("+argument+")] class C{}","CSharp1","attr.cs",[]));
        Assert.True(historical.Accepted,string.Join(";",historical.Issues));
        using var current=CSharpParser.Parse(new("[A("+argument+")] class C{}","CSharp14","attr.cs",[]));
        Assert.False(current.Accepted);Assert.Contains(current.Issues,i=>i.Code=="CSN2006");
    }
    [Theory][InlineData("fielld")][InlineData("int")][InlineData("class")]
    public void Unrecognized_historical_attribute_targets_warn_without_binding_rejection(string target)
    {
        string source="["+target+":A] class C{}";
        using var doc=CSharpParser.Parse(new(source,"CSharp1","target.cs",[]));
        Assert.True(doc.Accepted);
        var warning=Assert.Single(doc.Issues,i=>i.Code=="CSN2014");
        Assert.Equal(Nitrogen.DiagnosticSeverity.Warning,warning.Severity);
        Assert.Equal(new TextSpan(1,target.Length),warning.Span);
    }
}
