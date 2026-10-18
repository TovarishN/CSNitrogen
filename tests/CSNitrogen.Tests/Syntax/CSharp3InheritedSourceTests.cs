using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3InheritedSourceTests
{
 [Theory][MemberData(nameof(CSharp2InheritedSourceTests.Cases),MemberType=typeof(CSharp2InheritedSourceTests))]
 public void Inherited_source_forms_execute_in_the_selected_third_factory(string fixture,string source,bool expected)
 {
  // Exact source deltas retained from the first selected-factory run.
  if(source is "class C {int P {get;private set;}}" or "class C{object M(){return x=>x;}}" or "class C{object M(){return new C { X=1 };}}")expected=true;
  if(source is "[int:A] class C{}" or "[class:A] class C{}" or "class C{[assembly:A]void M(){}}")expected=false;
  using var p=CSharpParser.Parse(new(source,"CSharp3",fixture,[]));
  Assert.True(expected==p.Accepted,source+" :: "+string.Join(";",p.Issues));Assert.False(p.ProfileComplete);
  Assert.Equal(source.Length,p.MaskedText.Length);
  for(int node=0;node<p.Tree.NodeCount;node++)
  {var span=p.Tree.Span(node);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
  foreach(var issue in p.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
 }
}
