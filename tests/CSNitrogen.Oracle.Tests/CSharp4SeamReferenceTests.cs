using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;
using P=CSNitrogen.Oracle.Tests.CSharp4Probes;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4SeamReferenceTests
{
 public static IEnumerable<object[]> Programs()
 {
  var method=typeof(CSharp4GrammarSeamTests).GetMethod(nameof(CSharp4GrammarSeamTests.Bounded_additions_parse_with_generated_owners_and_are_absent_from_baseline))!;
  foreach(var attribute in method.GetCustomAttributes<InlineDataAttribute>())foreach(var row in attribute.GetData(method))yield return [row[0]];
 }
 [Theory][MemberData(nameof(Programs))]
 public void Independent_reference_syntax_agrees_on_named_default_and_variance_children_and_ranges(string original)
 {
  const string prefix="/*𐐀*/";string source=prefix+original;
  var reference=CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp4));Assert.Empty(reference.GetDiagnostics());var root=reference.GetRoot();
  var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var actual=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(actual.Success&&!actual.HasErrors);
  var parameters=root.DescendantNodes().OfType<ParameterSyntax>().Where(p=>p.Default is not null).ToArray();
  var actualDefaults=Enumerable.Range(0,actual.Tree.NodeCount).Where(n=>P.FixedParameterNode.Is(actual.Tree,n)).Select(n=>SyntaxView.Cast<P.FixedParameterNode>(actual.Tree,n)).Where(p=>p.Default.HasValue).OrderBy(p=>p.Span.Start).ToArray();
  Assert.Equal(parameters.Length,actualDefaults.Length);
  for(int i=0;i<parameters.Length;i++)
  {
   var expected=parameters[i];var ours=actualDefaults[i];Assert.Equal(expected.Identifier.Text,ours.Name.ToString());Span(expected.Identifier.Span,ours.Name.Span);Span(expected.Default!.Span,ours.Default.Value.Span);
   var value=SyntaxView.Cast<P.DefaultValueParameterDefault>(actual.Tree,ours.Default.Value.Index);Span(expected.Default.Value.Span,value.Value.Span);
  }
  var names=root.DescendantNodes().OfType<ArgumentSyntax>().Where(a=>a.NameColon is not null).Select(a=>a.NameColon!.Name.Identifier).OrderBy(n=>n.SpanStart).ToArray();
  var oursNames=new List<(int Start,int Length,string Text)>();
  foreach(int n in Enumerable.Range(0,actual.Tree.NodeCount))
  {
   if(P.NamedArgumentNameArgumentName.Is(actual.Tree,n)){var name=SyntaxView.Cast<P.NamedArgumentNameArgumentName>(actual.Tree,n).Name;oursNames.Add((name.Span.Start,name.Span.Length,name.ToString()));}
   if(P.NamedIndexArgumentNode.Is(actual.Tree,n)){var name=SyntaxView.Cast<P.NamedIndexArgumentNode>(actual.Tree,n).Name;oursNames.Add((name.Span.Start,name.Span.Length,name.ToString()));}
  }
  var sorted=oursNames.OrderBy(n=>n.Start).ToArray();Assert.Equal(names.Length,sorted.Length);for(int i=0;i<names.Length;i++){Assert.Equal(names[i].Text,sorted[i].Text);Assert.Equal(names[i].SpanStart,sorted[i].Start);Assert.Equal(names[i].Span.Length,sorted[i].Length);}
  var typeParams=root.DescendantNodes().OfType<TypeParameterSyntax>().OrderBy(p=>p.SpanStart).ToArray();var actualTypes=Enumerable.Range(0,actual.Tree.NodeCount).Where(n=>P.TypeParameterNode.Is(actual.Tree,n)).Select(n=>SyntaxView.Cast<P.TypeParameterNode>(actual.Tree,n)).OrderBy(p=>p.Span.Start).ToArray();Assert.Equal(typeParams.Length,actualTypes.Length);
  for(int i=0;i<typeParams.Length;i++){Assert.Equal(typeParams[i].Identifier.Text,actualTypes[i].Name.ToString());Span(typeParams[i].Identifier.Span,actualTypes[i].Name.Span);Assert.Equal(typeParams[i].VarianceKeyword.RawKind!=0,actualTypes[i].Variance.HasValue);if(actualTypes[i].Variance.HasValue)Span(typeParams[i].VarianceKeyword.Span,actualTypes[i].Variance.Value.Span);}
 }
 static void Span(Microsoft.CodeAnalysis.Text.TextSpan expected,Nitrogen.TextSpan actual){Assert.Equal(expected.Start,actual.Start);Assert.Equal(expected.Length,actual.Length);}
}
