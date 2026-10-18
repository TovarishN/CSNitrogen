using CSNitrogen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
namespace CSNitrogen.Oracle.Tests;
public class GenericSizeofPointerReferenceTests
{
 [Theory]
 [InlineData("G<int>*")][InlineData("N.G<int>**")][InlineData("G<int>.H<T>*[]")]
 [InlineData("A::T*")][InlineData("global::N.T**")][InlineData("A::G<int>*")]
 [InlineData("A::N.G<int>.H<T>**[,] ")]
 public void Generic_and_alias_sizeof_pointer_syntax_has_independent_observations(string operand)
 {
  string source="unsafe class C{int M(){return sizeof("+operand+");}}";
  foreach(var (profile,version) in new[]{("CSharp2",LanguageVersion.CSharp2),("CSharp3",LanguageVersion.CSharp3)})
  {
   Assert.DoesNotContain(Oracle.Observe(source,version).SyntaxDiagnostics,d=>d.Severity==DiagnosticSeverity.Error);
   using var p=CSharpParser.Parse(new(source,profile,"sizeof-pointer.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  }
 }
}
