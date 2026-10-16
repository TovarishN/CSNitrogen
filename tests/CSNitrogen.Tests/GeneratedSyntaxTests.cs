using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests;
public class GeneratedSyntaxTests {
[Theory][InlineData("",true)][InlineData("class C {}",true)][InlineData("class C {} class D {}",true)][InlineData("class C {",false)][InlineData("class C {} @",false)]
public void Parses_complete_input(string source,bool accepted) { var language=CSharp1Composition.CreateBuilder().Build(); using var parsed=language.Parse(source,CSharp1Composition.Start); Assert.Equal(accepted,parsed.Success); }
[Fact] public void Generated_views_preserve_names_and_spans() {
 const string source=" /* leading */ class C {} // middle\n class D {} ";
 var language=CSharp1Composition.CreateBuilder().Build(); using var parsed=language.Parse(source,CSharp1Composition.Start);
 Assert.True(parsed.Success); Assert.False(parsed.HasErrors);
 var root=SyntaxView.Cast<CompilationUnitNode>(parsed.Tree,parsed.Tree.Root);
 Assert.Equal(2,root.Members.Count);
 var classes=Enumerable.Range(0,parsed.Tree.NodeCount).Where(n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))=="ClassDeclaration").Select(n=>SyntaxView.Cast<ClassDeclarationNode>(parsed.Tree,n)).ToArray();
 Assert.Equal("C",classes[0].Name.ToString());Assert.Equal("D",classes[1].Name.ToString());
 Assert.Equal(source.IndexOf("C"),parsed.Tree.Span(classes[0].Name.Index).Start);
 Assert.Equal(source.IndexOf("D"),parsed.Tree.Span(classes[1].Name.Index).Start);
}
}
