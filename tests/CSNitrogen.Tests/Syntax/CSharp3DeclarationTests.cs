using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3DeclarationTests
{
 [Theory]
 [InlineData("static class E{static int M(this string s){return 1;}}")]
 [InlineData("unsafe static class E{static void M(this int*[] p){}}")]
 [InlineData("static class E{static T M<T>(this T t){return t;}}")]
 [InlineData("static class E{static void M([A] this string s,int x){}}")]
 [InlineData("partial class C{partial void M();}")]
 [InlineData("partial class C{static partial void M(ref int x){}}")]
 [InlineData("partial struct S{partial void M<T>(T x);}")]
 [InlineData("class C{public int X{get;set;}}")]
 [InlineData("class C{public int X{get;private set;}}")]
 [InlineData("struct S{static int X{get;set;}}")]
 public void Third_version_declarations_have_a_preceding_version_boundary(string source)
 {
  using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.True(third.Accepted,string.Join(";",third.Issues));
  using var second=CSharpParser.Parse(new(source,"CSharp2","second.cs",[]));Assert.False(second.Accepted);Assert.True(second.ProfileComplete);
 }
 [Theory]
 [InlineData("class E{static void M(this string s){}}")]
 [InlineData("static class E<T>{static void M(this string s){}}")]
 [InlineData("class C{static class E{static void M(this string s){}}}")]
 [InlineData("static class E{void M(this string s){}}")]
 [InlineData("static class E{static void M(int x,this string s){}}")]
 [InlineData("static class E{static void M(this ref int x){}}")]
 [InlineData("unsafe static class E{static void M(this int* x){}}")]
 [InlineData("delegate void D(this string s);")]
 [InlineData("class C{C(this string s){}}")]
 [InlineData("class C{int this[this int x]{get{return 1;}}}")]
 [InlineData("class C{partial void M();}")]
 [InlineData("partial class C{public partial void M();}")]
 [InlineData("partial class C{partial int M();}")]
 [InlineData("partial class C{partial static void M();}")]
 [InlineData("partial class C{partial void M(out int x);}")]
 [InlineData("partial class C{partial void I.M();}")]
 [InlineData("partial class C{virtual partial void M();}")]
 [InlineData("class C{int X{get;}}")]
 [InlineData("class C{int X{set;}}")]
 [InlineData("class C{int X{get;set{}}}")]
 [InlineData("class C{int X{get{return 1;}set;}}")]
 public void Historical_declaration_context_restrictions_are_enforced(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp3","bad.cs",[]));Assert.False(parsed.Accepted);}
 [Theory]
 [InlineData("abstract class C{public abstract int X{get;}}")]
 [InlineData("class C{public extern int X{get;set;}}")]
 [InlineData("interface I{int X{get;}}")]
 [InlineData("class C{void M(ref int x,out string y){y=null;}}")]
 [InlineData("class C{partial M(){return null;}}")]
 [InlineData("class C{partial voidness(){return null;}}")]
 [InlineData("class C{partial classy(){return null;}}")]
 public void Shared_baseline_declarations_keep_their_meaning(string source)
 {using var first=CSharpParser.Parse(new(source,"CSharp2","second.cs",[]));Assert.True(first.Accepted,string.Join(";",first.Issues));using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.True(third.Accepted,string.Join(";",third.Issues));}
 [Fact]public void Optional_receiver_preserves_existing_parameter_fields_and_authored_spans()
 {
  const string source="/*𐐀*/static class E{static void M(this string s,ref int x){}}";
  using var parsed=CSharpParser.Parse(new(source,"CSharp3","receiver.cs",[]));Assert.True(parsed.Accepted);
  var parameters=Enumerable.Range(0,parsed.Tree.NodeCount).Where(n=>CSNitrogen.Syntax.FixedParameterNode.Is(parsed.Tree,n)).Select(n=>Nitrogen.SyntaxView.Cast<CSNitrogen.Syntax.FixedParameterNode>(parsed.Tree,n)).ToArray();
  Assert.Equal(2,parameters.Length);Assert.True(parameters[0].Receiver.HasValue);Assert.False(parameters[0].Mode.HasValue);Assert.Equal("s",parameters[0].Name.ToString());
  Assert.Equal(source.IndexOf("this",StringComparison.Ordinal),parameters[0].Receiver.Value.Span.Start);
  Assert.False(parameters[1].Receiver.HasValue);Assert.True(parameters[1].Mode.HasValue);Assert.Equal("ref",parameters[1].Mode.Value.ToString());Assert.Equal("x",parameters[1].Name.ToString());
  foreach(string profile in new[]{"CSharp1","CSharp2"})
  {
   using var baseline=CSharpParser.Parse(new("class C{void M(ref int x){}}",profile,"baseline.cs",[]));Assert.True(baseline.Accepted);
   int n=Enumerable.Range(0,baseline.Tree.NodeCount).Single(n=>CSNitrogen.Syntax.FixedParameterNode.Is(baseline.Tree,n));
   var parameter=Nitrogen.SyntaxView.Cast<CSNitrogen.Syntax.FixedParameterNode>(baseline.Tree,n);Assert.False(parameter.Receiver.HasValue);Assert.Equal("ref",parameter.Mode.Value.ToString());Assert.Equal("x",parameter.Name.ToString());
  }
 }
 [Theory]
 [InlineData("class C{object M(){yield return 1;}}",true)]
 [InlineData("class C{object x=a=>{yield return 1;};}",false)]
 [InlineData("class C{object M(out int x){yield return 1;}}",false)]
 [InlineData("class C{object M(){yield return 1;return null;}}",false)]
 public void Third_version_preserves_iterator_source_boundaries(string source,bool accepted)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp3","iterator.cs",[]));Assert.Equal(accepted,parsed.Accepted);}

}
