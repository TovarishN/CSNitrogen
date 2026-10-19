using Nitrogen;
using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using P=CSNitrogen.Oracle.Tests.CSharp4Probes;
namespace CSNitrogen.Oracle.Tests;
public class CSharp4GrammarSeamTests
{
 static string Authored(string source,TextSpan span)=>source.Substring(span.Start,span.Length);
 [Theory]
 [InlineData("class C{void M(){F(x:1);}}","NamedArgumentName")]
 [InlineData("class C{void M(){F(1,y:2,z:ref a,w:out b);}}","NamedArgumentName")]
 [InlineData("class C{void M(){new C(x:1);}}","NamedArgumentName")]
 [InlineData("class C{C():base(x:1){}}","NamedArgumentName")]
 [InlineData("class C{C():this(x:1){}}","NamedArgumentName")]
 [InlineData("class C{object M(){return a[index:1];}}","NamedElementExpression")]
 [InlineData("class C{object M(){return a[0,index:1];}}","NamedElementExpression")]
 [InlineData("class C{object M(){return base[index:1];}}","NamedBaseIndexExpression")]
 [InlineData("class C{void M(S x=default(S)){}}","DefaultValue")]
 [InlineData("class C{void M(S x=new S()){}}","DefaultValue")]
 [InlineData("class C{void M(int x=1,int y=2,params int[] rest){}}","DefaultValue")]
 [InlineData("class C{void M(int x=1){}}","DefaultValue")]
 [InlineData("class C{C(int x=1){}}","DefaultValue")]
 [InlineData("interface I{void M(int x=1);}","DefaultValue")]
 [InlineData("delegate void D(int x=1);","DefaultValue")]
 [InlineData("class C{int this[int x=1]{get{return x;}}}","DefaultValue")]
 [InlineData("interface I<out T>{}","Covariant")]
 [InlineData("interface I<in T,out U,V>{}","Contravariant")]
 [InlineData("delegate T D<out T>();","Covariant")]
 [InlineData("delegate void D<in T>(T x);","Contravariant")]
 public void Bounded_additions_parse_with_generated_owners_and_are_absent_from_baseline(string source,string kind)
 {
  var all=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=all.Parse(source,P.CSharp4AllProbeComposition.Start);
  Assert.True(p.Success&&!p.HasErrors,source+" :: "+string.Join(";",p.Diagnostics.ToArray().Select(d=>p.FormatMessage(d))));
  Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>all.GetKindName(p.Tree.Kind(n))==kind);
  var baseline=P.CSharp4NoneProbeComposition.CreateBuilder().Build();using var absent=baseline.Parse(source,P.CSharp4NoneProbeComposition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var production=CSharpParser.Parse(new(source,"CSharp4","pending.cs",[]));Assert.True(production.Accepted,string.Join(";",production.Issues));Assert.False(production.ProfileComplete);Assert.True(ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 [Theory]
 [InlineData("class C{void M(){F(A::T);}}")][InlineData("class C{void M(){F(x:);}}")][InlineData("class C{void M(){F(:1);}}")][InlineData("class C{void M(){F(x::1);}}")]
 [InlineData("class C{object M(){return a[index:1,0];}}")][InlineData("class C{object M(){return base[index:1,0];}}")]
 [InlineData("class C{object M(){return a[index:];}}")][InlineData("class C{object M(){return a[index:ref x];}}")]
 [InlineData("class C{void M(int x=){}}")][InlineData("class C{void M(params int[] xs=1){}}")]
 [InlineData("class C{object M(){return (int x=1)=>x;}}")]
 [InlineData("interface I<out>{}")][InlineData("interface I<in out T>{}")][InlineData("delegate T D<out T,>();")]
 public void Malformed_candidate_skeletons_are_rejected(string source)
 {var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.False(p.Success&&!p.HasErrors,source);}
 [Fact]public void Optional_seams_preserve_original_fields_and_absent_baseline_views()
 {
  const string source="class C<T>{void M([A]ref int x){F(ref x);}}";var language=P.CSharp4NoneProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4NoneProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int Find(string kind)=>Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);
  var parameter=SyntaxView.Cast<P.FixedParameterNode>(p.Tree,Find("FixedParameter"));Assert.Equal(1,parameter.Attributes.Count);Assert.False(parameter.Receiver.HasValue);Assert.Equal("ref",parameter.Mode.Value.ToString());Assert.Equal("int",Authored(source,parameter.Type.Span));Assert.Equal("x",parameter.Name.ToString());Assert.False(parameter.Default.HasValue);
  var argument=SyntaxView.Cast<P.ArgumentNode>(p.Tree,Find("Argument"));Assert.False(argument.Name.HasValue);Assert.Equal("ref",argument.Mode.Value.ToString());Assert.Equal("x",Authored(source,argument.Value.Span));
  var type=SyntaxView.Cast<P.TypeParameterNode>(p.Tree,Find("TypeParameter"));Assert.Equal("T",type.Name.ToString());Assert.Equal(0,type.Attributes.Count);Assert.False(type.Variance.HasValue);
 }
 [Theory]
 [InlineData("class C{void M(){F(x:1);}}","Probe.CSharp4NamedArguments")]
 [InlineData("class C{void M(int x=1){}}","Probe.CSharp4OptionalParameters")]
 [InlineData("interface I<out T>{}","Probe.CSharp4Variance")]
 public void Features_can_be_omitted_independently_without_breaking_ordinary_syntax(string source,string omitted)
 {
  var full=P.CSharp4AllProbeComposition.CreateBuilder().Build();var builder=new LanguageBuilder();foreach(var module in full.Modules)if(module.Name!=omitted)builder.Add(module);var language=builder.Build();
  using var absent=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var baseline=language.Parse("class C<T>{void M(int x){F(x);}}",P.CSharp4AllProbeComposition.Start);Assert.True(baseline.Success&&!baseline.HasErrors);
  foreach(var (module,other) in new[]{("Probe.CSharp4NamedArguments","class C{void M(){F(x:1);}}"),("Probe.CSharp4OptionalParameters","class C{void M(int x=1){}}"),("Probe.CSharp4Variance","interface I<out T>{}")})
  {if(module==omitted)continue;using var independent=language.Parse(other,P.CSharp4AllProbeComposition.Start);Assert.True(independent.Success&&!independent.HasErrors,module+" after omitting "+omitted);}
 }
 [Fact]public void Named_default_and_variance_captures_retain_typed_children_and_exact_UTF16_ranges()
 {
  const string source="/*𐐀*/interface I<[A]out T>{void M(int x=1);}";var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  var t=SyntaxView.Cast<P.TypeParameterNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.TypeParameterNode.Is(p.Tree,n)));Assert.Equal(1,t.Attributes.Count);Assert.Equal("T",t.Name.ToString());Assert.Equal("out",Authored(source,t.Variance.Value.Span));Assert.Equal(source.IndexOf("out",StringComparison.Ordinal),t.Variance.Value.Span.Start);
  var param=SyntaxView.Cast<P.FixedParameterNode>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.FixedParameterNode.Is(p.Tree,n)));Assert.Equal("int",Authored(source,param.Type.Span));Assert.Equal("x",param.Name.ToString());Assert.Equal("=1",Authored(source,param.Default.Value.Span));Assert.Equal(source.IndexOf("=1",StringComparison.Ordinal),param.Default.Value.Span.Start);
  const string invocation="/*𐐀*/class C{void M(){F(name:ref x);}}";using var q=language.Parse(invocation,P.CSharp4AllProbeComposition.Start);Assert.True(q.Success&&!q.HasErrors);
  var arg=SyntaxView.Cast<P.ArgumentNode>(q.Tree,Assert.Single(Enumerable.Range(0,q.Tree.NodeCount),n=>P.ArgumentNode.Is(q.Tree,n)));Assert.Equal("name:",Authored(invocation,arg.Name.Value.Span));Assert.Equal("ref",arg.Mode.Value.ToString());Assert.Equal("x",Authored(invocation,arg.Value.Span));Assert.Equal(invocation.IndexOf("name:",StringComparison.Ordinal),arg.Name.Value.Span.Start);
 }
 [Theory]
 [InlineData("class C{void M(){F(x:1,2);}}","named argument followed by positional argument, main3167")]
 [InlineData("class C{void M(int x=1,int y){}}","required parameter after optional parameter, main6131")]
 [InlineData("class C{void M(ref int x=1){}}","ref default, main6132")]
 [InlineData("class C{void M(out int x=1){}}","out default, main6132")]
 [InlineData("class C<out T>{}","variance on class, main7293")]
 [InlineData("class C{void M<out T>(){}}","variance on method, main7293")]
 public void Raw_proof_overacceptance_is_rejected_by_production_context_checks(string source,string obligation)
 {var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors,obligation);using var production=CSharpParser.Parse(new(source,"CSharp4","context.cs",[]));Assert.False(production.Accepted);Assert.Contains(production.Issues,i=>i.Code=="CSN2040");Assert.False(production.ProfileComplete);}
 [Fact]public void Positional_index_owners_and_alias_or_conditional_colons_remain_unchanged()
 {
  const string source="class C{object M(){F(A::T.M,p?x:y);return a[x,y]+base[x,y];}}";var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  var element=SyntaxView.Cast<P.ElementExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.ElementExpressionExpression.Is(p.Tree,n)));Assert.Equal(2,element.Indices.Count);Assert.Equal("x",Authored(source,element.Indices[0].Span));
  var baseElement=SyntaxView.Cast<P.BaseIndexExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.BaseIndexExpressionExpression.Is(p.Tree,n)));Assert.Equal(2,baseElement.Indices.Count);
  Assert.DoesNotContain(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n)) is "NamedElementExpression" or "NamedBaseIndexExpression" or "NamedArgumentName");
 }
 [Fact]public void Named_index_arguments_retain_separate_positional_and_named_typed_owners()
 {
  const string source="/*𐐀*/class C{object M(){return a[0,index:1,last:2];}}";var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  var index=SyntaxView.Cast<P.NamedElementExpressionExpression>(p.Tree,Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>P.NamedElementExpressionExpression.Is(p.Tree,n)));
  Assert.Equal("a",Authored(source,index.Target.Span));Assert.Equal(1,index.Arguments.Leading.Count);Assert.Equal(2,index.Arguments.Named.Count);Assert.Equal("0",Authored(source,index.Arguments.Leading[0].Value.Span));
  var named=index.Arguments.Named[0];Assert.Equal("index",named.Name.ToString());Assert.Equal("1",Authored(source,named.Value.Span));Assert.Equal(source.IndexOf("index:",StringComparison.Ordinal),named.Name.Span.Start);
 }
 [Fact]public void Deep_named_invocation_children_keep_their_original_ranges()
 {
  const int depth=40;string expression="1";for(int i=0;i<depth;i++)expression="F(x:"+expression+")";string source="/*𐐀*/class C{object M(){return "+expression+";}}";
  var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  Assert.Equal(depth,Enumerable.Range(0,p.Tree.NodeCount).Count(n=>language.GetKindName(p.Tree.Kind(n))=="NamedArgumentName"));foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var span=p.Tree.Span(n);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
 }

 [Theory]
 [InlineData("a[A::T.M]","ElementExpression","A::T.M")]
 [InlineData("a[p?x:y]","ElementExpression","p?x:y")]
 [InlineData("base[A::T.M]","BaseIndexExpression","A::T.M")]
 [InlineData("base[p?x:y]","BaseIndexExpression","p?x:y")]
 [InlineData("a[A::T.M,index:1]","NamedElementExpression","A::T.M")]
 [InlineData("a[p?x:y,index:1]","NamedElementExpression","p?x:y")]
 [InlineData("base[A::T.M,index:1]","NamedBaseIndexExpression","A::T.M")]
 [InlineData("base[p?x:y,index:1]","NamedBaseIndexExpression","p?x:y")]
 public void Alias_and_conditional_index_values_preserve_their_owners_and_exact_expression_spans(string expression,string kind,string first)
 {
  string source="/*𐐀*/class C{object M(){return "+expression+";}}";var language=P.CSharp4AllProbeComposition.CreateBuilder().Build();using var p=language.Parse(source,P.CSharp4AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);
  int node=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>language.GetKindName(p.Tree.Kind(n))==kind);Nitrogen.TextSpan span;
  if(kind=="ElementExpression")span=SyntaxView.Cast<P.ElementExpressionExpression>(p.Tree,node).Indices[0].Span;
  else if(kind=="BaseIndexExpression")span=SyntaxView.Cast<P.BaseIndexExpressionExpression>(p.Tree,node).Indices[0].Span;
  else
  {
   var args=kind=="NamedElementExpression"?SyntaxView.Cast<P.NamedElementExpressionExpression>(p.Tree,node).Arguments:SyntaxView.Cast<P.NamedBaseIndexExpressionExpression>(p.Tree,node).Arguments;
   Assert.Equal(1,args.Leading.Count);Assert.Equal(1,args.Named.Count);span=args.Leading[0].Value.Span;Assert.Equal("index",args.Named[0].Name.ToString());
  }
  Assert.Equal(first,Authored(source,span));Assert.Equal(source.IndexOf(first,StringComparison.Ordinal),span.Start);
 }

}
