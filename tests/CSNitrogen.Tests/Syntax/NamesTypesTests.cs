using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class NamesTypesTests
{
    [Theory][InlineData("A",true)][InlineData("A.B",true)][InlineData("int",true)][InlineData("object",true)]
    [InlineData("int[]",true)][InlineData("A[,]",true)][InlineData("A[][,]",true)][InlineData("var",true)]
    [InlineData("A.",false)][InlineData("A[",false)][InlineData("A<T>",false)][InlineData("void",false)]
    public void Non_generic_types(string source,bool accepted)
    {var language=CSharp1Composition.CreateBuilder().Build();using var parsed=language.Parse(source,NamesTypesModule.TypeFragment);Assert.Equal(accepted,parsed.Success&&!parsed.HasErrors);}
 [Fact]
 public void Generated_type_views_retain_rank_and_qualified_parts()
 {
  var language=CSharp1Composition.CreateBuilder().Build();using var parsed=language.Parse("N.T[][,]",NamesTypesModule.TypeFragment);
  Assert.True(parsed.Success&&!parsed.HasErrors);var tree=parsed.Tree;
  int Node(string kind)=>Assert.Single(Enumerable.Range(0,tree.NodeCount),n=>language.GetKindName(tree.Kind(n))==kind);
  var type=SyntaxView.Cast<TypeSyntaxTypeReference>(tree,Node("TypeSyntax"));Assert.Equal(2,type.Ranks.Count);
  var name=SyntaxView.Cast<QualifiedNameNode>(tree,Node("QualifiedName"));Assert.Equal(2,name.Parts.Count);Assert.Equal("T",name.Parts[1].ToString());
 }
}
