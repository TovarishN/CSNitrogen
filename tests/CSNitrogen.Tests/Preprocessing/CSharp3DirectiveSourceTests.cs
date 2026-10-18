using CSNitrogen.Parsing;
using Nitrogen;
namespace CSNitrogen.Tests.Preprocessing;
public class CSharp3DirectiveSourceTests
{
 [Theory]
 [InlineData("#define X\n#if (X && !false) == true || false\nclass A{}\n#elif true\ninvalid\n#else\ninvalid\n#endif",true)]
 [InlineData("#define class\n#if class\nclass C{}\n#endif",true)]
 [InlineData("#undef X\n#if X\ninvalid\n#else\nclass C{}\n#endif",true)]
 [InlineData("#if false\n\"unterminated disabled text\n#elif true\nclass C{}\n#endif",true)]
 [InlineData("#region outer\n#if true\n#region inner\nclass C{}\n#endregion\n#endif\n#endregion",true)]
 [InlineData("#if true\n#region cross\n#else\n#endregion\n#endif",false)]
 [InlineData("#else\nclass C{}",false)][InlineData("#endif\nclass C{}",false)]
 [InlineData("#if true\nclass C{}",false)][InlineData("#if true\n#else\n#elif true\n#endif",false)]
 [InlineData("#if true\n#else\n#else\n#endif",false)]
 [InlineData("class C{}\n#define X",false)][InlineData("#define true\nclass C{}",false)]
 [InlineData("#unknown\nclass C{}",false)][InlineData("#if true false\n#endif",false)]
 [InlineData("#if false\n#line bad\n#endif\nclass C{}",false)]
 public void Source_directive_alternatives_and_structure_are_checked(string source,bool accepted)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp3","directives.cs",[]));Assert.True(accepted==p.Accepted,string.Join(";",p.Issues));
  Assert.Equal(source.Length,p.MaskedText.Length);
  foreach(var issue in p.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
 }
 [Theory][InlineData("\n")][InlineData("\r")][InlineData("\r\n")][InlineData("\u0085")][InlineData("\u2028")][InlineData("\u2029")]
 public void All_source_line_terminators_split_comments_and_directives_with_original_offsets(string newline)
 {
  string source="// comment"+newline+"#define X"+newline+"#if X"+newline+"class C{}"+newline+"#endif";
  using var p=CSharpParser.Parse(new(source,"CSharp3","newlines.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),Assert.Single(p.Classes).Name.Span.Start);Assert.Equal(3,p.Directives.Count);
 }
 [Theory][InlineData("warning disable")][InlineData("warning restore 168,169")][InlineData("vendor payload")]
 public void Pragma_information_is_preserved_with_an_explicit_compiler_effect_exclusion(string payload)
 {
  using var p=CSharpParser.Parse(new("#pragma "+payload+"\nclass C{}","CSharp3","pragma.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  Assert.Equal(payload,Assert.Single(p.Directives).Argument);Assert.Contains(p.Issues,i=>i.Severity==DiagnosticSeverity.Warning&&i.Code=="CSN1203");
 }
 [Fact]public void Diagnostic_message_text_is_not_lexed_as_tokens()
 {
  const string message="can't /* or // must remain message text";
  using var warning=CSharpParser.Parse(new("#warning "+message+"\nclass C{}","CSharp3","warning.cs",[]));Assert.True(warning.Accepted);
  var issue=Assert.Single(warning.Issues);Assert.Equal(message,issue.Message);Assert.Equal(DiagnosticSeverity.Warning,issue.Severity);
  using var error=CSharpParser.Parse(new("#error "+message+"\nclass C{}","CSharp3","error.cs",[]));Assert.False(error.Accepted);
  using var inactive=CSharpParser.Parse(new("#if false\n#error "+message+"\n#endif\nclass C{}","CSharp3","inactive.cs",[]));Assert.True(inactive.Accepted);
 }
}
