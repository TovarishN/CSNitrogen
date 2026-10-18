using Nitrogen;
using P=CSNitrogen.Tests.CSharp3Probes;
namespace CSNitrogen.Tests.Feasibility;
public class CSharp3GrammarSeamTests
{
 [Theory]
 [InlineData("x=>x+1","LambdaExpression")]
 [InlineData("(x,y)=>x+y","LambdaExpression")]
 [InlineData("()=>1","LambdaExpression")]
 [InlineData("(int x)=>x","LambdaExpression")]
 [InlineData("(ref int x,out int y)=>{y=x;}","LambdaExpression")]
 [InlineData("x=>y=>x+y","LambdaExpression")]
 [InlineData("new C{X=1,Y=2,}","InitializedObjectExpression")]
 [InlineData("new C(){X=1}","InitializedObjectExpression")]
 [InlineData("new C{Child={X=1}}","InitializedObjectExpression")]
 [InlineData("new C{1,2,}","InitializedObjectExpression")]
 [InlineData("new C{{1,2},{3,4}}","InitializedObjectExpression")]
 [InlineData("new {X=1,Y=a}","AnonymousObjectExpression")]
 [InlineData("new {x,a.Name}","AnonymousObjectExpression")]
 [InlineData("new []{1,2,}","ImplicitArrayExpression")]
 [InlineData("new [,]{{1,2},{3,4}}","ImplicitArrayExpression")]
 [InlineData("from x in xs select x","QueryExpression")]
 [InlineData("from @from in @where select @from","QueryExpression")]
 [InlineData("from x in xs select new {X=x,F=y=>y+x}","QueryExpression")]
 [InlineData("from int x in xs where x>0 select x","QueryExpression")]
 [InlineData("from x in xs let y=x+1 orderby y descending,x ascending select y","QueryExpression")]
 [InlineData("from x in xs join y in ys on x.K equals y.K into g select g","QueryExpression")]
 [InlineData("from x in xs group x by x.K into g select g","QueryExpression")]
 public void Candidate_syntax_has_a_generated_owner_in_real_compilation_units(string expression,string kind)
 {
  string source="class C{object value="+expression+";}";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3AllProbeComposition.Start);
  Assert.True(parsed.Success&&!parsed.HasErrors,expression+" :: "+string.Join(";",parsed.Diagnostics.ToArray().Select(d=>parsed.FormatMessage(d))));
  Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))==kind);
  var baseline=P.CSharp3BaseProbeComposition.CreateBuilder().Build();using var absent=baseline.Parse(source,P.CSharp3BaseProbeComposition.Start);
  Assert.False(absent.Success&&!absent.HasErrors,expression);
  Assert.True(CSNitrogen.Profiles.ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }

 [Theory]
 [InlineData("(int x,y)=>x")][InlineData("(x,)=>x")][InlineData("(ref x)=>x")]
 [InlineData("x=>")][InlineData("(int x=1)=>x")][InlineData("(params int[] xs)=>xs")]
 [InlineData("new C{,}")][InlineData("new C{X=}")][InlineData("new C{{}}")]
 [InlineData("new {X=}")][InlineData("new [2]{1}")]
 [InlineData("from x in xs")][InlineData("from x xs select x")]
 [InlineData("from x in xs select")][InlineData("from x in xs join y in ys equals y select x")]
 [InlineData("from x in xs orderby select x")][InlineData("from x in xs select x into y")]
 public void Malformed_skeletons_are_rejected(string expression)
 {
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();
  using var parsed=language.Parse("class C{object value="+expression+";}",P.CSharp3AllProbeComposition.Start);
  Assert.False(parsed.Success&&!parsed.HasErrors,expression);
 }
 [Theory]
 [InlineData("x=>x","Probe.CSharp3Lambda")]
 [InlineData("new C{X=1}","Probe.CSharp3Initializers")]
 [InlineData("from x in xs select x","Probe.CSharp3Queries")]
 public void Each_new_module_can_be_omitted_without_breaking_the_baseline(string expression,string omitted)
 {
  var full=P.CSharp3AllProbeComposition.CreateBuilder().Build();
  var builder=new LanguageBuilder();foreach(var module in full.Modules)if(module.Name!=omitted)builder.Add(module);
  var language=builder.Build();using var absent=language.Parse("class C{object value="+expression+";}",P.CSharp3AllProbeComposition.Start);
  Assert.False(absent.Success&&!absent.HasErrors);
  using var ordinary=language.Parse("class C{int x; int M(){return x+1;}}",P.CSharp3AllProbeComposition.Start);
  Assert.True(ordinary.Success&&!ordinary.HasErrors);
 }
 [Fact]public void Queries_and_lambdas_have_independent_syntax_membership()
 {
  var query=P.CSharp3QueryProbeComposition.CreateBuilder().Build();
  Assert.DoesNotContain(query.Modules,m=>m.Name=="Probe.CSharp3Lambda");
  using var q=query.Parse("class C{object value=from x in xs select x;}",P.CSharp3QueryProbeComposition.Start);Assert.True(q.Success&&!q.HasErrors);
  var lambda=P.CSharp3LambdaProbeComposition.CreateBuilder().Build();Assert.DoesNotContain(lambda.Modules,m=>m.Name=="Probe.CSharp3Queries");
  using var l=lambda.Parse("class C{object value=x=>x;}",P.CSharp3LambdaProbeComposition.Start);Assert.True(l.Success&&!l.HasErrors);
 }
 [Theory][InlineData("from+select")][InlineData("fromage+orderby")][InlineData("@from+@where")]
 public void Query_words_remain_ordinary_identifiers_outside_queries(string expression)
 {
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse("class C{object value="+expression+";}",P.CSharp3AllProbeComposition.Start);
  Assert.True(parsed.Success&&!parsed.HasErrors);Assert.DoesNotContain(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="QueryExpression");
 }
 [Theory]
 [InlineData("1+x=>x","anonymous-function operand category")]
 [InlineData("new {a+b}","inferred anonymous member category")]
 [InlineData("new C{1,x=2}","collection scalar non-assignment category")]
 [InlineData("from x in from select x","query-context identifier reservation")]
 public void Raw_skeleton_over_acceptance_is_retained_as_a_policy_obligation(string expression,string obligation)
 {
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse("class C{object value="+expression+";}",P.CSharp3AllProbeComposition.Start);
  Assert.True(parsed.Success&&!parsed.HasErrors,obligation+" :: "+expression);
  Assert.True(CSNitrogen.Profiles.ProfileResolver.Resolve("CSharp3").Profile!.Complete);
 }
 [Fact]public void Lambda_parameters_and_body_have_typed_owners_and_authored_spans()
 {
  const string source="class C{object value=(ref int x,out int y)=>{y=x;};}";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int signature=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="ExplicitSignature");
  var parameters=SyntaxView.Cast<P.ExplicitSignatureNode>(parsed.Tree,signature).Items;
  Assert.Equal(2,parameters.Count);Assert.Equal("x",parameters[0].Name.ToString());Assert.Equal("y",parameters[1].Name.ToString());Assert.Equal(",",parameters.Separator(0).ToString());
  Assert.True(parameters[0].Mode.HasValue);var span=parameters[0].Mode.Value.Span;Assert.Equal("ref",source.Substring(span.Start,span.Length));
  int node=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="LambdaExpression");
  var lambda=SyntaxView.Cast<P.LambdaExpressionExpression>(parsed.Tree,node);var body=lambda.Body.Span;Assert.Equal("{y=x;}",source.Substring(body.Start,body.Length));
 }
 [Fact]public void Query_clause_lists_and_continuation_preserve_typed_structure()
 {
  const string source="class C{object value=from int x in xs where x>0 orderby x descending,x ascending group x by x.K into g select g;}";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int node=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="QueryExpression");
  var query=SyntaxView.Cast<P.QueryExpressionExpression>(parsed.Tree,node);
  Assert.Equal(2,query.Body.Clauses.Count);Assert.True(query.Body.Continuation.HasValue);Assert.Equal("g",query.Body.Continuation.Value.Name.ToString());
  int binding=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="TypedBinding");
  Assert.Equal("x",SyntaxView.Cast<P.TypedBindingNode>(parsed.Tree,binding).Name.ToString());
  int order=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="OrderByClause");
  var items=SyntaxView.Cast<P.OrderByClauseNode>(parsed.Tree,order).Items;Assert.Equal(2,items.Count);Assert.Equal(",",items.Separator(0).ToString());
  Assert.True(items[0].Direction.HasValue);var span=items[0].Direction.Value.Span;Assert.Equal("descending",source.Substring(span.Start,span.Length));
 }
 [Theory][InlineData(8)][InlineData(32)]
 public void Nested_lambdas_have_bounded_valid_and_missing_body_witnesses(int depth)
 {
  string expression=string.Concat(Enumerable.Range(0,depth).Select(n=>"x"+n+"=>"))+"1";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var good=language.Parse("class C{object value="+expression+";}",P.CSharp3AllProbeComposition.Start);Assert.True(good.Success&&!good.HasErrors);
  Assert.Equal(depth,Enumerable.Range(0,good.Tree.NodeCount).Count(n=>language.GetKindName(good.Tree.Kind(n))=="LambdaExpression"));
  using var bad=language.Parse("class C{object value="+expression[..^1]+";}",P.CSharp3AllProbeComposition.Start);Assert.False(bad.Success&&!bad.HasErrors);
 }
 [Fact]public void Construction_children_lists_and_UTF16_spans_are_authored()
 {
  const string source="/*𐐀*/class C{object a=new C(){X=1,Y=2,};object b=new C{{1,2},{3,4}};object c=new {X=1,a.Name};object d=new [,]{{1,2},{3,4}};}";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int Find(string kind)=>Enumerable.Range(0,parsed.Tree.NodeCount).First(n=>language.GetKindName(parsed.Tree.Kind(n))==kind);
  string Slice(TextSpan span)=>source.Substring(span.Start,span.Length);
  var creation=SyntaxView.Cast<P.InitializedObjectExpressionExpression>(parsed.Tree,Find("InitializedObjectExpression"));
  Assert.True(creation.Arguments.HasValue);Assert.Equal("()",Slice(creation.Arguments.Value.Span));Assert.Equal("C",Slice(creation.Type.Span));
  Assert.Equal(source.IndexOf("new C()",StringComparison.Ordinal),creation.Span.Start);
  var memberGroup=SyntaxView.Cast<P.ObjectInitializerNode>(parsed.Tree,Find("ObjectInitializer")).Items;
  Assert.True(memberGroup.HasValue);var members=new SeparatedList<P.MemberInitializerNode>(parsed.Tree,memberGroup.Value.Child(0).Index);
  Assert.Equal(2,members.Count);Assert.Equal("X",members[0].Name.ToString());Assert.Equal("2",Slice(members[1].Value.Span));Assert.Equal(",",members.Separator(0).ToString());
  var elements=SyntaxView.Cast<P.CollectionInitializerNode>(parsed.Tree,Find("CollectionInitializer")).Items;
  Assert.Equal(2,elements.Count);Assert.Equal("{1,2}",Slice(elements[0].Span));Assert.Equal(",",elements.Separator(0).ToString());
  var braced=SyntaxView.Cast<P.BracedElementNode>(parsed.Tree,Find("BracedElement")).Items;
  Assert.Equal(2,braced.Count);Assert.Equal("1",Slice(braced[0].Span));Assert.Equal(",",braced.Separator(0).ToString());
  var anonymous=SyntaxView.Cast<P.AnonymousObjectExpressionExpression>(parsed.Tree,Find("AnonymousObjectExpression"));Assert.True(anonymous.Initializer.Items.HasValue);
  var anonymousMembers=new SeparatedList<SyntaxNode>(parsed.Tree,anonymous.Initializer.Items.Value.Child(0).Index);Assert.Equal(2,anonymousMembers.Count);
  Assert.Equal(",",anonymousMembers.Separator(0).ToString());Assert.Equal("{X=1,a.Name}",Slice(anonymous.Initializer.Span));
  var named=SyntaxView.Cast<P.NamedMemberNode>(parsed.Tree,Find("NamedMember"));Assert.Equal("X",named.Name.ToString());Assert.Equal("1",Slice(named.Value.Span));
  Assert.Equal("a.Name",Slice(SyntaxView.Cast<P.InferredMemberNode>(parsed.Tree,Find("InferredMember")).Value.Span));
  var array=SyntaxView.Cast<P.ImplicitArrayExpressionExpression>(parsed.Tree,Find("ImplicitArrayExpression"));Assert.Equal(1,array.Rank.Commas.Count);Assert.Equal("[,]",Slice(array.Rank.Span));
  Assert.True(array.Initializer.Items.HasValue);var arrayItems=new SeparatedList<SyntaxNode>(parsed.Tree,array.Initializer.Items.Value.Child(0).Index);
  Assert.Equal(2,arrayItems.Count);Assert.Equal("{3,4}",Slice(arrayItems[1].Span));Assert.Equal(",",arrayItems.Separator(0).ToString());
 }
 [Theory][InlineData("x=>x+1","x+1")][InlineData("x=>y=>x+y","y=>x+y")][InlineData("f=x=>x+1","x+1")][InlineData("x=>y=x+1","y=x+1")]
 public void Lambda_body_owns_arithmetic_nested_lambda_and_assignment(string expression,string expectedBody)
 {
  string source="/*𐐀*/class C{object value="+expression+";}";
  var language=P.CSharp3AllProbeComposition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3AllProbeComposition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int node=Enumerable.Range(0,parsed.Tree.NodeCount).Where(n=>language.GetKindName(parsed.Tree.Kind(n))=="LambdaExpression").OrderBy(n=>parsed.Tree.Span(n).Start).First();
  var lambda=SyntaxView.Cast<P.LambdaExpressionExpression>(parsed.Tree,node);Assert.Equal(expectedBody,source.Substring(lambda.Body.Span.Start,lambda.Body.Span.Length));
  Assert.Equal(source.IndexOf("x=>",StringComparison.Ordinal),lambda.Span.Start);
  var add=SyntaxView.Cast<P.AddExpressionExpression>(parsed.Tree,Enumerable.Range(0,parsed.Tree.NodeCount).Single(n=>language.GetKindName(parsed.Tree.Kind(n))=="AddExpression"));
  Assert.Equal("x",source.Substring(add.Left.Span.Start,add.Left.Span.Length));Assert.Equal(expression.Contains("x+y")?"y":"1",source.Substring(add.Right.Span.Start,add.Right.Span.Length));
  if(expression.StartsWith("f=",StringComparison.Ordinal))
  {
   var assignment=SyntaxView.Cast<P.AssignmentExpressionExpression>(parsed.Tree,Enumerable.Range(0,parsed.Tree.NodeCount).Single(n=>language.GetKindName(parsed.Tree.Kind(n))=="AssignmentExpression"));
   Assert.Equal("x=>x+1",source.Substring(assignment.Value.Span.Start,assignment.Value.Span.Length));
  }
 }

}
