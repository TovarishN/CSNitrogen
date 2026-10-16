using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CompilationUnitTests
{
 [Theory][InlineData("using A; using B=N.B; [assembly:A] namespace N {using Q; class C{void M(){F();}} struct S{}} class D{}",true)]
 [InlineData("enum E{A} delegate void D(); interface I{}",true)][InlineData("",true)]
 [InlineData("class C{} using A;",false)][InlineData("using static A;",false)][InlineData("namespace N; class C{}",false)]
 [InlineData("class C { void M(){a+b;} }",false)][InlineData("[assembly:A] using B;",false)]
 public void Whole_file_contexts(string source,bool accepted)
 {using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));Assert.Equal(accepted,doc.Accepted);if(accepted)Assert.True(doc.Root.Members.Count>=0);}
 [Fact] public void Class_projection_is_top_level_and_source_located()
 {
  const string source="#if false\nclass Hidden{}\n#endif\nnamespace N{class Nested{}} class C{class Inner{}}";
  using var doc=CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));Assert.True(doc.Accepted);var c=Assert.Single(doc.Classes);
  Assert.Equal("C",c.Name.ToString());Assert.Equal(source.IndexOf("C{",StringComparison.Ordinal),doc.Tree.Span(c.Name.Index).Start);
 }
}
