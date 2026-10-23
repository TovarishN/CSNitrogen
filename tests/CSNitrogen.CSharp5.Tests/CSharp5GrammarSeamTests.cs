using Nitrogen;
using P=CSNitrogen.CSharp5Probes;
namespace CSNitrogen.CSharp5.Tests;
public class CSharp5GrammarSeamTests
{
 [Theory]
 [InlineData("class C{async void M(){}}","AsyncModifier")]
 [InlineData("class C{async Task M(){await M();}}","AwaitExpression")]
 [InlineData("class C{object f=async x=>x;}","LambdaExpression")]
 [InlineData("class C{object f=async ()=>await M();}","AwaitExpression")]
 [InlineData("class C{object f=async delegate{await M();};}","AnonymousMethodExpression")]
 public void Source_additions_have_real_generated_owners(string source,string kind)
 {
  var all=P.CSharp5AllProbeComposition.CreateBuilder().Build();using var p=all.Parse(source,P.CSharp5AllProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors,source);Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>all.GetKindName(p.Tree.Kind(n))==kind);
  var none=P.CSharp5NoneProbeComposition.CreateBuilder().Build();using var absent=none.Parse(source,P.CSharp5NoneProbeComposition.Start);Assert.False(absent.Success&&!absent.HasErrors);
 }
 [Theory][InlineData("class C{void M(){await(M());}}")][InlineData("class async{int await;}")][InlineData("class C{object f=x=>x;object g=delegate{return 1;};}")]
 public void Baseline_ordinary_identifiers_and_function_fields_remain_available(string source)
 {var none=P.CSharp5NoneProbeComposition.CreateBuilder().Build();using var p=none.Parse(source,P.CSharp5NoneProbeComposition.Start);Assert.True(p.Success&&!p.HasErrors);}
}
