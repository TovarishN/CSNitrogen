using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class ExpressionTests
{
 [Theory][InlineData("a+b*c",true)][InlineData("a-b-c",true)][InlineData("a=b=c",true)][InlineData("a?b:c?d:e",true)]
 [InlineData("(T)x",true)][InlineData("(x)+y",true)][InlineData("(int)-x",true)][InlineData("F(ref x,out y)",true)]
 [InlineData("new C(1)",true)][InlineData("new int[3][]",true)][InlineData("new int[,] {{1,2},{3,4}}",true)]
 [InlineData("typeof(void)",true)][InlineData("checked(x+1)",true)][InlineData("a is T",true)][InlineData("base.F()",true)]
 [InlineData("x=>x",false)][InlineData("a??b",false)][InlineData("F(x:1)",false)][InlineData("new C { X=1 }",false)]
 [InlineData("a+",false)][InlineData("nameof(x)",true)]
 public void Expression_surface(string text,bool accepted)
 { var language=CSharp1Composition.CreateBuilder().Build();using var p=language.Parse(text,ExpressionsModule.ExpressionFragment);Assert.Equal(accepted,p.Success&&!p.HasErrors); }
}
