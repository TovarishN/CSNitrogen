using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CSNitrogen.CSharp6.Tests;
/// <summary>Roslyn at LanguageVersion.CSharp6 as an observation: parse diagnostics plus the syntax-shaped and language-version compilation errors listed below. The sixth-edition text is the authority; every
/// acceptance disagreement is a named divergence row, asserted here and copied into csharp6-seam-proof-evidence.md.</summary>
public class CSharp6SpellingContractTests
{
 static readonly CSharpParseOptions Six=new(LanguageVersion.CSharp6);
 /// <summary>Compilation errors Roslyn reports for syntax-shaped or language-version reasons after parsing; binding errors are ignored.</summary>
 static readonly HashSet<string> SyntaxShapedCompilationErrors=new(StringComparer.Ordinal){"CS8026","CS8059","CS8086","CS8089","CS8361","CS8967"};
 static bool RoslynAccepts(string s)
 {
  var tree=CSharpSyntaxTree.ParseText(s,Six);
  if(tree.GetDiagnostics().Any())return false;
  var compilation=CSharpCompilation.Create("c6",[tree],options:new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
  return !compilation.GetDiagnostics().Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error&&SyntaxShapedCompilationErrors.Contains(d.Id));
 }
 public static TheoryData<string> Agreed=>new()
 {
  "class C{string s=$\"a{x}b{y+1}c\";}","class C{string s=$@\"a{x}\nb\";}","class C{string s=$\"{$\"{x}\"}\";}","class C{string s=$\"{x,-5:N2}\";}",
  "class C{string s=$\"{{a}}{x}\";}","class C{string s=$\"{(a?b:c)}\";}","class C{string s=$\"{F(x:1)}\";}","class C{string s=$\"a /*x*/ b\";}",
  "class C{object o=a?.b;}","class C{object o=a?[0];}","class C{object o=a?.b.c?[1]();}","class C{object o=a?.5:1;}",
"class C{string s=nameof(x);}","class C{string s=nameof(List<int>.Count);}",
  "class C{void M(){try{}catch(E e)when(e!=null){}catch when(f){}}}","using static System.Math;",
  "class C{int P{get;}=1;}","class C{int M()=>1;}","class C{int P=>1;}","class C{int this[int i]=>i;}","class C{public static C operator+(C a,C b)=>a;}",
  "class C{object o=new D{[0]=1,A=2,[1,2]={[3]=4}};}","class C{int when;int nameof;void M(){nameof(when);}}",
 };
 public static TheoryData<string> AgreedRejections=>new()
 {
  "class C{string s=$\"a\nb\";}","class C{string s=$\"{x\";}","class C{string s=$\"{}\";}","class C{string s=$\"a}b\";}",
  "class C{object o=a?.;}","class C{int M()=>;}","class C{string s=$\"{x:}\";}",
 };
 /// <summary>Spellings where Roslyn 5.0.0 at version 6 and the sixth-edition grammar disagree. Value: true when the standard accepts.</summary>
 public static TheoryData<string,bool,string> Divergences=>new()
 {
  {"class C{string s=$\"{x:N\n}\";}",true,"ECMA-334 6 ends format text at the new-line, which is then hole whitespace; Roslyn reads format text up to } and reports CS1010"},
  {"class C{string s=$\"{\nx\n}\";}",true,"ECMA-334 6 lexes hole contents as tokens, so a new-line is whitespace; Roslyn reports CS8967 (new-lines in non-verbatim holes need C# 11)"},
  {"class C{string s=$\"{x:a}}b}\";}",true,"Regular_Interpolation_Format elements include Close_Brace_Escape_Sequence; Roslyn reports CS8086"},
  {"class C{string s=nameof(int);}",true,"named_entity_target includes predefined_type; Roslyn parses nameof as an invocation and rejects int as an argument"},
  {"class C{string s=@$\"{x}\";}",false,"only $@ starts a verbatim interpolated string in the sixth edition; Roslyn parses @$ and leaves the version check to compilation"},
  {"using static A=B;",false,"using_static_directive has no alias; Roslyn parses it and reports the error during binding"},
 };
 [Theory][MemberData(nameof(Agreed))]
 public void Roslyn_and_the_probe_accept(string s){Assert.True(RoslynAccepts(s),s);Assert.True(Probe.Accepts("All",s),s);}
 [Theory][MemberData(nameof(AgreedRejections))]
 public void Roslyn_and_the_probe_reject(string s){Assert.False(RoslynAccepts(s),s);Assert.False(Probe.Accepts("All",s),s);}
 [Theory][MemberData(nameof(Divergences))]
 public void Recorded_divergences_follow_the_standard(string s,bool standardAccepts,string reason)
 {
  Assert.NotEqual(standardAccepts,RoslynAccepts(s));
  Assert.Equal(standardAccepts,Probe.Accepts("All",s));
  Assert.NotEmpty(reason);
 }
 /// <summary>The standard's unnested format colon makes this an error; Roslyn agrees, the grammar accepts it, and the Task 2 policy must reject it.</summary>
 [Theory]
 [InlineData("class C{string s=$\"{a?b:c}\";}")]
 [InlineData("class C{string s=$\"{x,-a?b:c}\";}")]
 public void Unparenthesized_conditional_hole_is_rejected_by_roslyn_and_pending_policy(string s)
 {
  Assert.False(RoslynAccepts(s));
  Assert.True(Probe.Accepts("All",s));
 }
 [Theory]
 [InlineData("class C{string s=$\"a{x}b{y+1}c\";}")]
 [InlineData("/*\U00010400*/class C{string s=$@\"{$\"{x}\"}\";}")]
 [InlineData("class C{string s=$\"{ x /*c*/ ,5:N2}\";}")]
 public void Hole_expression_spans_match_roslyn(string s)
 {
  var expected=CSharpSyntaxTree.ParseText(s,Six).GetRoot().DescendantNodes().OfType<InterpolationSyntax>().Select(i=>(i.Expression.SpanStart,i.Expression.Span.Length)).ToList();
  var holes=Probe.Spans("All",s,"Interpolation");
  Assert.Equal(expected.Count,holes.Count);
  Assert.All(expected.Zip(holes),pair=>Assert.Equal(pair.First.Item1,pair.Second.Start));
 }
}
