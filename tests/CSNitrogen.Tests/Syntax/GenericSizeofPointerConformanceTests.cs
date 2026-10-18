using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Syntax;
public class GenericSizeofPointerConformanceTests
{
 [Theory]
 [InlineData("G<int>*")][InlineData("N.G<int>**")][InlineData("G<int>.H<T>*[]")]
 [InlineData("A::T*")][InlineData("global::N.T**")][InlineData("A::G<int>*")]
 [InlineData("A::N.G<int>.H<T>**[,] ")]
 public void Generic_and_alias_sizeof_pointer_operands_keep_type_and_original_ranges(string operand)
 {
  var rejected=new List<string>();
  foreach(string profile in new[]{"CSharp2","CSharp3"})
  {
   string source="/*𐐀*/unsafe class C{int M(){return sizeof("+operand+");}}";
   using var p=CSharpParser.Parse(new(source,profile,"sizeof-pointer.cs",[]));if(!p.Accepted){rejected.Add(profile+" :: "+string.Join(";",p.Issues));continue;}
   int node=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="SizeOfExtendedExpression");
   var expression=SyntaxView.Cast<SizeOfExtendedExpressionExpression>(p.Tree,node);
   Assert.Equal(operand.TrimEnd(),source.Substring(expression.Type.Span.Start,expression.Type.Span.Length));
   Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))=="PointerCastType");
  }
  Assert.Empty(rejected);
 }
 [Theory][InlineData("G<int>","GenericSizeofType")][InlineData("A::T","AliasSizeofType")]
 public void Neighboring_non_pointer_owners_are_preserved(string operand,string expected)
 {
  foreach(string profile in new[]{"CSharp2","CSharp3"})
  {using var p=CSharpParser.Parse(new("unsafe class C{int M(){return sizeof("+operand+");}}",profile,"sizeof.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Contains(Enumerable.Range(0,p.Tree.NodeCount),n=>p.Tree.Language!.GetKindName(p.Tree.Kind(n))==expected);}
 }
 [Theory][InlineData("G<int>*+")][InlineData("A::T*+")][InlineData("G<int>*[1]")][InlineData("A::T*[1]")]
 public void Pointer_type_neighbors_require_complete_type_syntax(string operand)
 {foreach(string profile in new[]{"CSharp2","CSharp3"}){using var p=CSharpParser.Parse(new("unsafe class C{int M(){return sizeof("+operand+");}}",profile,"sizeof-bad.cs",[]));Assert.False(p.Accepted);}}
}
