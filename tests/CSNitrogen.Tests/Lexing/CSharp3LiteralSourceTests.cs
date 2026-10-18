using System.Reflection;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Lexing;
public class CSharp3LiteralSourceTests
{
 public static IEnumerable<object[]> Spellings()
 {
  foreach(string name in new[]{nameof(LexicalTests.Identifier_fragments),nameof(LexicalTests.Integer_fragments),nameof(LexicalTests.Real_fragments),nameof(LexicalTests.Character_fragments),nameof(LexicalTests.String_fragments)})
  {
   var method=typeof(LexicalTests).GetMethod(name)!;
   foreach(var a in method.GetCustomAttributes<InlineDataAttribute>())foreach(var row in a.GetData(method))
    yield return [name,(string)row[0],(bool)row[1]];
  }
 }
 [Theory][MemberData(nameof(Spellings))]
 public void Historical_token_spellings_execute_in_third_version_compilation_units(string category,string spelling,bool accepted)
 {
  string source=category==nameof(LexicalTests.Identifier_fragments)?"class "+spelling+"{}":"class C{object value="+spelling+";}";
  using var parsed=CSharpParser.Parse(new(source,"CSharp3","literal.cs",[]));
  Assert.True(parsed.Accepted==accepted,source+" :: "+string.Join(";",parsed.Issues));Assert.True(parsed.ProfileComplete);
  if(accepted)
  {
   int start=category==nameof(LexicalTests.Identifier_fragments)?6:21;
   Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>{var span=parsed.Tree.Span(n);return span.Start==start&&span.Length==spelling.Length;});
  }
 }
 [Theory]
 [InlineData("'\\U0001F600'",false)][InlineData("\"\\U00110000\"",false)]
 [InlineData("'\\U0000D800'",true)][InlineData("\"\\u005Cu005C\"",true)]
 [InlineData("0XfFuL",true)][InlineData(".25E-2D",true)][InlineData("123e+2m",true)]
 public void Literal_escape_bounds_and_historical_numeric_suffixes_have_selected_factory_witnesses(string spelling,bool accepted)
 {using var parsed=CSharpParser.Parse(new("class C{object value="+spelling+";}","CSharp3","literal.cs",[]));Assert.Equal(accepted,parsed.Accepted);}
 [Theory][InlineData(">>",true)][InlineData("> >",false)][InlineData(">/*x*/>",false)]
 [InlineData(">>=",true)][InlineData("> >=",false)]
 public void Shift_components_remain_contiguous_lexemes(string op,bool accepted)
 {using var p=CSharpParser.Parse(new("class C{void M(){x="+(op.EndsWith('=')?"(a"+op+"b)":"a"+op+"b")+";}}","CSharp3","shift.cs",[]));Assert.Equal(accepted,p.Accepted);}
 [Theory]
 [InlineData("/* /* */ class C{}",true)]
 [InlineData("// /* not a delimiter */\nclass C{}",true)]
 [InlineData("/* // not a line comment */ class C{}",true)]
 [InlineData("/* unterminated",false)]
 [InlineData("class C{string s=\"/* not a comment */\";}",true)]
 [InlineData("class C{string s=@\"\n#if X\n/* text */\n\";}",true)]
 [InlineData("\ufeffclass C{}",true)]
 [InlineData("class C{}\u001a class D{}",false)]
 public void Source_trivia_and_literal_boundaries_do_not_depend_on_nested_comment_or_directive_text(string source,bool accepted)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","trivia.cs",[]));Assert.True(p.Accepted==accepted,string.Join(";",p.Issues));}
}
