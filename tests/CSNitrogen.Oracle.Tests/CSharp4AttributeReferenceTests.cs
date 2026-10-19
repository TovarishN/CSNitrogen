using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4AttributeReferenceTests
{
 [Theory]
 [InlineData("/*𐐀*/[A(x:1)]class C{}")]
 [InlineData("/*𐐀*/[A(0,x:1,x=2)]class C{}")]
 [InlineData("/*𐐀*/[A(@x:A::T.M,y:p?1:2)]class C{}")]
 public void Pinned_fourth_syntax_observations_agree_on_attribute_label_and_value_children(string source)
 {
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp4));Assert.DoesNotContain(reference.GetDiagnostics(),d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
  using var parsed=CSharpParser.Parse(new(source,"CSharp4","reference.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));Assert.False(parsed.ProfileComplete);
  var expected=reference.GetRoot().DescendantNodes().OfType<AttributeArgumentSyntax>().ToArray();
  var actual=Enumerable.Range(0,parsed.Tree.NodeCount).Where(n=>PositionalAttributeArgumentNode.Is(parsed.Tree,n)||NamedAttributeArgumentNode.Is(parsed.Tree,n)).ToArray();Assert.Equal(expected.Length,actual.Length);
  for(int i=0;i<expected.Length;i++)
  {
   var arg=expected[i];int value;
   if(arg.NameEquals is {} memberName)
   {
    var member=SyntaxView.Cast<NamedAttributeArgumentNode>(parsed.Tree,actual[i]);value=member.Value.Index;
    Assert.Equal(memberName.Name.Span.Start,member.Name.Span.Start);Assert.Equal(memberName.Name.Span.Length,member.Name.Span.Length);
   }
   else
   {
    var constructor=SyntaxView.Cast<PositionalAttributeArgumentNode>(parsed.Tree,actual[i]);value=constructor.Value.Index;
    Assert.Equal(arg.NameColon is not null,constructor.Name.HasValue);
    if(arg.NameColon is {} label)
    {
     var name=SyntaxView.Cast<NamedArgumentNameArgumentName>(parsed.Tree,constructor.Name.Value.Index).Name;
     Assert.Equal(label.Name.Span.Start,name.Span.Start);Assert.Equal(label.Name.Span.Length,name.Span.Length);
     Assert.Equal(label.Span.Start,constructor.Name.Value.Span.Start);Assert.Equal(label.Span.Length,constructor.Name.Value.Span.Length);
    }
   }
   Assert.Equal(arg.Expression.Span.Start,parsed.Tree.Span(value).Start);Assert.Equal(arg.Expression.Span.Length,parsed.Tree.Span(value).Length);
  }
 }
}
