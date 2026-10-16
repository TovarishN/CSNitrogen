using CSNitrogen.Syntax;
namespace CSNitrogen.Tests.Syntax;
public class AttributeParameterTests
{
 [Theory][InlineData("[A]",true)][InlineData("[A(1, Name=2), N.B]",true)][InlineData("[return:A]",true)]
 [InlineData("[A(",false)][InlineData("[A,]",true)]
 public void Attribute_forms(string source,bool accepted)
 {var l=CSharp1Composition.CreateBuilder().Build();using var p=l.Parse(source,AttributesModule.AttributeFragment);Assert.Equal(accepted,p.Success&&!p.HasErrors);}
 [Theory][InlineData("()",true)][InlineData("([A] ref int x,out T y,params object[] args)",true)]
 [InlineData("(int x=1)",false)][InlineData("(this T x)",false)][InlineData("(in T x)",false)][InlineData("(params int[] x,int y)",false)]
 public void Parameter_forms(string source,bool accepted)
 {var l=CSharp1Composition.CreateBuilder().Build();using var p=l.Parse(source,AttributesModule.ParameterFragment);Assert.Equal(accepted,p.Success&&!p.HasErrors);}
}
