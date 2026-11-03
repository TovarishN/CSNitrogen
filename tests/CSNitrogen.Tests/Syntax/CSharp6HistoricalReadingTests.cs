using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Version5 lexical, preprocessing, attribute and iterator readings reviewed against the ECMA-334 6th edition
/// (docs/coverage/csharp6-historical-reading-review.md). Where the sixth text repeats the reading, CSharp6 keeps it.</summary>
public class CSharp6HistoricalReadingTests
{
 static ParseDocument Parse(string source,string profile)=>CSharpParser.Parse(new(source,profile,"history.cs",[]));
 static string[] Codes(ParseDocument p)=>p.Issues.Select(i=>i.Code+"@"+i.Span.Start+"+"+i.Span.Length).Order(StringComparer.Ordinal).ToArray();
 [Theory]
 [InlineData("class C{}\u001a")]                                   // §6.3: a trailing Control-Z is deleted
 [InlineData("#if(X)\n#endif\nclass C{}")]                          // PP_Whitespace required after if
 [InlineData("#define\tX\n#undef(X)\nclass C{}")]                   // and after undef
 [InlineData("#if false\n#nullable enable\n#endif\nclass C{}")]     // skipped directives shall be lexically correct
 [InlineData("#if false\n#line x\n#endif\nclass C{}")]
 [InlineData("#line 5 \"a.cs\" x\nclass C{}")]                      // exact PP_Line_Indicator forms
 [InlineData("[foo:A]class C{}")]                                   // non-standard attribute target
 [InlineData("[A(x=y=1)]class C{}")]                                // attribute_argument_expression is expression
 [InlineData("class C{System.Collections.IEnumerable M(){try{yield return 1;}catch{}}}")]
 [InlineData("class C{System.Collections.IEnumerable M(){try{}finally{yield break;}}}")]
 public void Sixth_edition_repeats_the_fifth_reading(string source)
 {
  using var five=Parse(source,"CSharp5");using var six=Parse(source,"CSharp6");
  Assert.Equal(Codes(five),Codes(six));Assert.Equal(five.Accepted,six.Accepted);
 }
 [Fact]
 public void Control_Z_is_deleted()
 {
  using var p=Parse("class C{}\u001a","CSharp6");Assert.True(p.Accepted,string.Join(";",p.Issues));
 }
 [Fact]
 public void Typevar_is_a_standardized_target_again()
 {
  using var p=Parse("class C<[typevar:A]T>{}","CSharp6");Assert.DoesNotContain(p.Issues,i=>i.Code=="CSN2014");
 }
 [Fact]
 public void Escaped_underscore_starts_an_identifier()
 {
  using var p=Parse("class \\u005Fx{}","CSharp6");Assert.True(p.Accepted,string.Join(";",p.Issues));
 }
}
