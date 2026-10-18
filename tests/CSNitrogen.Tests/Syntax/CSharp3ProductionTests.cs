using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using CSNitrogen.Syntax;
using Nitrogen;
using P=CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3ProductionTests
{
 [Theory]
 [InlineData("x=>x+1")][InlineData("(x,y)=>x+y")][InlineData("()=>1")]
 [InlineData("(int x)=>x")][InlineData("(ref int x,out int y)=>{y=x;}")]
 [InlineData("x=>y=>x+y")][InlineData("f=x=>x+1")][InlineData("x=>y=x+1")]
 [InlineData("c?x=>x:y=>y")][InlineData("(F)(x=>x)")]
 [InlineData("from x in xs select x")][InlineData("from int x in xs where x>0 select x")]
 [InlineData("from x in xs let y=x+1 orderby y descending,x ascending select y")]
 [InlineData("from x in xs join y in ys on x.K equals y.K into g select g")]
 [InlineData("from x in xs group x by x.K into g select g")]
 [InlineData("from @from in @where select @from")]
 [InlineData(@"from x in fr\u006fm select x")][InlineData(@"from x in xs select x.\u0073elect")]
 [InlineData("from x in xs select y=>y+x")]
 public void Third_version_expressions_are_accepted_and_unavailable_in_the_second(string expression)
 {
  string source="class C{object value="+expression+";}";
  using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.True(third.Accepted,string.Join(";",third.Issues));Assert.False(third.ProfileComplete);
  using var second=CSharpParser.Parse(new(source,"CSharp2","second.cs",[]));Assert.False(second.Accepted);Assert.True(second.ProfileComplete);
 }
 [Theory]
 [InlineData("x??y=>y")][InlineData("x??from y in ys select y")]
 [InlineData("1+x=>x")][InlineData("!x=>x")][InlineData("x=>x+1+y=>y")]
 [InlineData("(int x,y)=>x")][InlineData("(ref x)=>x")][InlineData("(int x=1)=>x")]
 [InlineData("(params int[] xs)=>xs")][InlineData("x=>")]
 [InlineData("from x in from select x")][InlineData("from from in xs select @from")]
 [InlineData("from x in xs select x.select")][InlineData("from x in xs select x into select select @select")]
 [InlineData("from x in xs")][InlineData("from x in xs orderby select x")]
 public void Invalid_expression_neighbors_are_rejected(string expression)
 {using var result=CSharpParser.Parse(new("class C{object x="+expression+";}","CSharp3","bad.cs",[]));Assert.False(result.Accepted);}
 [Theory][InlineData("CSharp.Lambdas","x=>x")][InlineData("CSharp.Queries","from x in xs select x")]
 public void Modules_are_independently_removable(string omitted,string expression)
 {
  var full=CSharp3Composition.CreateBuilder().Build();Assert.Contains(full.Modules,m=>m.Name==omitted);
  var builder=new LanguageBuilder();foreach(var m in full.Modules)if(m.Name!=omitted)builder.Add(m);
  var partial=builder.Build();using var absent=partial.Parse("class C{object x="+expression+";}",CSharp3Composition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var baseline=partial.Parse("class C{int x;int M(){return x+1;}}",CSharp3Composition.Start);Assert.True(baseline.Success&&!baseline.HasErrors);
  string other=omitted=="CSharp.Lambdas"?"from x in xs select x":"x=>x";
  using var independent=partial.Parse("class C{object x="+other+";}",CSharp3Composition.Start);Assert.True(independent.Success&&!independent.HasErrors);
 }
 [Theory][InlineData("csharp3.lambdas")][InlineData("csharp3.queries")]
 public void Capabilities_require_their_explicit_baseline_bundles(string id)
 {var result=ProfileResolver.ResolveCustom("incomplete",[id]);Assert.Null(result.Profile);Assert.Contains(result.Issues,i=>i.Code=="missing-requirement");}
 [Theory][InlineData("CSharp3")][InlineData("CSharp7_3")][InlineData("CSharp14")]
 public void Later_factories_include_the_third_version_modules_but_remain_partial(string profile)
 {using var parsed=CSharpParser.Parse(new("class C{object x=a=>a;}",profile,"later.cs",[]));Assert.True(parsed.Accepted);Assert.False(parsed.ProfileComplete);} [Fact]public void Lambda_parameters_and_body_have_typed_owners_and_authored_spans()
 {
  const string source="class C{object value=(ref int x,out int y)=>{y=x;};}";
  var language=P.CSharp3Composition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3Composition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
  int signature=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="LambdaExplicitSignature");
  var parameters=SyntaxView.Cast<P.LambdaExplicitSignatureNode>(parsed.Tree,signature).Items;
  Assert.Equal(2,parameters.Count);Assert.Equal("x",parameters[0].Name.ToString());Assert.Equal("y",parameters[1].Name.ToString());Assert.Equal(",",parameters.Separator(0).ToString());
  Assert.True(parameters[0].Mode.HasValue);var span=parameters[0].Mode.Value.Span;Assert.Equal("ref",source.Substring(span.Start,span.Length));
  int node=Assert.Single(Enumerable.Range(0,parsed.Tree.NodeCount),n=>language.GetKindName(parsed.Tree.Kind(n))=="LambdaExpression");
  var lambda=SyntaxView.Cast<P.LambdaExpressionExpression>(parsed.Tree,node);var body=lambda.Body.Span;Assert.Equal("{y=x;}",source.Substring(body.Start,body.Length));
 }
 [Fact]public void Query_clause_lists_and_continuation_preserve_typed_structure()
 {
  const string source="class C{object value=from int x in xs where x>0 orderby x descending,x ascending group x by x.K into g select g;}";
  var language=P.CSharp3Composition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3Composition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
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
  var language=P.CSharp3Composition.CreateBuilder().Build();using var good=language.Parse("class C{object value="+expression+";}",P.CSharp3Composition.Start);Assert.True(good.Success&&!good.HasErrors);
  Assert.Equal(depth,Enumerable.Range(0,good.Tree.NodeCount).Count(n=>language.GetKindName(good.Tree.Kind(n))=="LambdaExpression"));
  using var bad=language.Parse("class C{object value="+expression[..^1]+";}",P.CSharp3Composition.Start);Assert.False(bad.Success&&!bad.HasErrors);
 }
 [Theory][InlineData("x=>x+1","x+1")][InlineData("x=>y=>x+y","y=>x+y")][InlineData("f=x=>x+1","x+1")][InlineData("x=>y=x+1","y=x+1")]
 public void Lambda_body_owns_arithmetic_nested_lambda_and_assignment(string expression,string expectedBody)
 {
  string source="/*𐐀*/class C{object value="+expression+";}";
  var language=P.CSharp3Composition.CreateBuilder().Build();using var parsed=language.Parse(source,P.CSharp3Composition.Start);Assert.True(parsed.Success&&!parsed.HasErrors);
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
