using Nitrogen;
using P=CSNitrogen.CSharp6Probes;
namespace CSNitrogen.CSharp6.Tests;
/// <summary>Builds the test-only sixth-version compositions and reads node kinds and spans from their trees.</summary>
static class Probe
{
 static readonly Dictionary<string,Lazy<(Language Language,Rule Start)>> s_languages=new(StringComparer.Ordinal)
 {
  ["None"]=new(()=>(P.CSharp6NoneProbeComposition.CreateBuilder().Build(),P.CSharp6NoneProbeComposition.Start)),
  ["All"]=new(()=>(P.CSharp6AllProbeComposition.CreateBuilder().Build(),P.CSharp6AllProbeComposition.Start)),
  ["InterpolatedStrings"]=new(()=>(P.CSharp6WithoutInterpolatedStringsProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutInterpolatedStringsProbeComposition.Start)),
  ["NullConditional"]=new(()=>(P.CSharp6WithoutNullConditionalProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutNullConditionalProbeComposition.Start)),
  ["NameofExpressions"]=new(()=>(P.CSharp6WithoutNameofExpressionsProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutNameofExpressionsProbeComposition.Start)),
  ["ExceptionFilters"]=new(()=>(P.CSharp6WithoutExceptionFiltersProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutExceptionFiltersProbeComposition.Start)),
  ["UsingStatic"]=new(()=>(P.CSharp6WithoutUsingStaticProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutUsingStaticProbeComposition.Start)),
  ["PropertyInitializers"]=new(()=>(P.CSharp6WithoutPropertyInitializersProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutPropertyInitializersProbeComposition.Start)),
  ["ExpressionBodies"]=new(()=>(P.CSharp6WithoutExpressionBodiesProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutExpressionBodiesProbeComposition.Start)),
  ["IndexInitializers"]=new(()=>(P.CSharp6WithoutIndexInitializersProbeComposition.CreateBuilder().Build(),P.CSharp6WithoutIndexInitializersProbeComposition.Start)),
 };
 /// <summary>A complete parse: the whole input is consumed and every error is a sanctioned nameof ambiguity.</summary>
 internal static bool Accepts(string probe,string source){var (l,s)=s_languages[probe].Value;using var p=l.Parse(source,s);return Complete(l,p);}
 /// <summary>Nitrogen reports each tie as an Ambiguous error; the sixth-version contract accepts exactly the nameof tie (Task 2 moves this into the parse wrapper).</summary>
 internal static bool Complete(Language l,ParseResult p)
 {
  if(!p.Success)return false;
  foreach(var d in p.Diagnostics)
  {
   if(d.Severity!=DiagnosticSeverity.Error)continue;
   if(d.Code!=DiagnosticCode.Ambiguous||!SanctionedNameof(l,p.Tree,d.Arg0))return false;
  }
  return true;
 }
 static bool SanctionedNameof(Language l,SyntaxTree t,int n)
 {
  var kinds=Enumerable.Range(0,t.ChildCount(n)).Select(i=>l.GetKindName(t.Kind(t.Child(n,i)))).Order(StringComparer.Ordinal).ToArray();
  return kinds is ["NameofExpression","NameofInvocation"];
 }
 /// <summary>Spans (start,length) of every node of the named kind, in tree order, for a source the probe must accept.</summary>
 internal static List<(int Start,int Length)> Spans(string probe,string source,string kind)
 {
  var (l,s)=s_languages[probe].Value;using var p=l.Parse(source,s);Assert.True(Complete(l,p),source);
  return Enumerable.Range(0,p.Tree.NodeCount).Where(n=>l.GetKindName(p.Tree.Kind(n))==kind).Select(n=>(p.Tree.Span(n).Start,p.Tree.Span(n).Length)).ToList();
 }
 /// <summary>Kind names of the children of every node of the named kind.</summary>
 internal static List<string[]> Children(string probe,string source,string kind)
 {
  var (l,s)=s_languages[probe].Value;using var p=l.Parse(source,s);Assert.True(Complete(l,p),source);var t=p.Tree;
  return Enumerable.Range(0,t.NodeCount).Where(n=>l.GetKindName(t.Kind(n))==kind).Select(n=>Enumerable.Range(0,t.ChildCount(n)).Select(i=>t.Child(n,i)).Where(c=>c!=SyntaxTree.NoNode).Select(c=>l.GetKindName(t.Kind(c))).ToArray()).ToList();
 }
 internal static (int,int) At(string source,string text,int occurrence=0){int i=-1;for(int k=0;k<=occurrence;k++)i=source.IndexOf(text,i+1,StringComparison.Ordinal);Assert.True(i>=0,text);return (i,text.Length);}
}
