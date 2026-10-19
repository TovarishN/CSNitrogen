using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp4AttributeSourceTests
{
 [Theory]
 [InlineData("[A(x:1)]class C{}")]
 [InlineData("[A(1,y:2)]class C{}")]
 [InlineData("[A(x:1,y:2)]class C{}")]
 [InlineData("[A(x:1,Field=2)]class C{}")]
 [InlineData("[A(x:1,x=2)]class C{}")]
 [InlineData("[A(x:F(x:1)),B(0)]class C{}")]
 [InlineData("[assembly:A(x:1)]class C{}")]
 [InlineData("[A(x:p?y:z)]class C{}")]
 [InlineData("[A(x:A::T.M)]class C{}")]
 [InlineData("[A(@x:1,y=2)]class C{}")]
 [InlineData(@"[A(out\u0054:1)]class C{}")]
 [InlineData("[A(x:1=y)]class C{}")]
 public void Fourth_attribute_constructor_labels_follow_the_source_name_slot(string source)
 {
  using var fourth=CSharpParser.Parse(new(source,"CSharp4","attribute.cs",[]));Assert.True(fourth.Accepted,string.Join(";",fourth.Issues));Assert.False(fourth.ProfileComplete);
  using var third=CSharpParser.Parse(new(source,"CSharp3","third.cs",[]));Assert.False(third.Accepted);Assert.True(third.ProfileComplete);
 }
 [Theory]
 [InlineData("[A(x:1,2)]class C{}")]
 [InlineData("[A(x:1,@x:2)]class C{}")]
 [InlineData(@"[A(x:1,\u0078:2)]class C{}")]
 [InlineData("[A(x:1,x\u200c:2)]class C{}")]
 [InlineData("[A(Field=1,x:2)]class C{}")]
 [InlineData("[A(Field=1,2)]class C{}")]
 [InlineData("[A(x:ref y)]class C{}")]
 [InlineData("[A(x:out y)]class C{}")]
 [InlineData("[A(x:)]class C{}")]
 [InlineData("[A(x::1)]class C{}")]
 [InlineData("[A(x:1,)]class C{}")]
 public void Constructor_and_member_argument_ordering_and_malformed_neighbors_are_rejected(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","bad-attribute.cs",[]));Assert.False(p.Accepted);}
 [Theory]
 [InlineData("[unknown:A]class C{}")]
 [InlineData("[typevar:A]class C<T>{}")]
 [InlineData("[@event:A]class C{}")]
 [InlineData(@"[\u0065vent:A]class C{}")]
 [InlineData(@"[\u0072eturn:A]class C{}")]
 public void Fourth_target_spellings_follow_the_printed_target_categories(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp4","target.cs",[]));Assert.False(p.Accepted);Assert.Contains(p.Issues,i=>i.Code=="CSN2014");}
 [Theory]
 [InlineData("[A(1=x)]class C{}")]
 [InlineData("[A(X=1=x)]class C{}")]
 public void Attribute_expression_grammar_defers_assignment_constantness_to_semantics(string source)
 {foreach(string profile in new[]{"CSharp3","CSharp4"}){using var p=CSharpParser.Parse(new(source,profile,"expression.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}}
}
