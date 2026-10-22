using System.Reflection;
using Nitrogen;
using CSNitrogen.Parsing;
using CSNitrogen.Tests.Lexing;
using CSNitrogen.Tests.Preprocessing;
namespace CSNitrogen.Tests.Syntax;
/// <summary>Finite predecessor context witnesses run on the actual fourth factory; not normative approval.</summary>
public class CSharp4InheritedContextTests
{
 static IEnumerable<object[]> Data(Type type,string method)
 {
  var m=type.GetMethod(method)!;
  foreach(var a in m.GetCustomAttributes<InlineDataAttribute>())foreach(var row in a.GetData(m))yield return row;
 }
 public static IEnumerable<object[]> Cases()
 {
  foreach(var (method,expected) in new[]{(nameof(CSharp3ExpressionSourceTests.Source_forms_and_parenthesized_neighbors_are_accepted),true),(nameof(CSharp3ExpressionSourceTests.Malformed_or_wrong_source_categories_are_rejected),false)})
   foreach(var row in Data(typeof(CSharp3ExpressionSourceTests),method))yield return [method,"/*𐐀*/unsafe class C{object M(){return "+row[0]+";}}",expected];
  foreach(var row in Data(typeof(CSharp3ExpressionSourceTests),nameof(CSharp3ExpressionSourceTests.Type_shapes_preserve_grammar_and_defer_named_type_identity)))
   yield return ["type","/*𐐀*/unsafe class C{"+row[0]+" field;}",row[1]];
  foreach(var (method,expected) in new[]{(nameof(CSharp3StatementSourceTests.Source_statement_forms_and_iterator_neighbors_are_accepted),true),(nameof(CSharp3StatementSourceTests.Malformed_categories_and_iterator_placement_are_rejected),false)})
   foreach(var row in Data(typeof(CSharp3StatementSourceTests),method))yield return [method,"/*𐐀*/class C{object M(){"+row[0]+"}}",expected];
  foreach(var row in Data(typeof(CSharp3StatementSourceTests),nameof(CSharp3StatementSourceTests.Iterator_declaration_owners_and_modes_are_restricted)))yield return ["iterator-owner","/*𐐀*/"+row[0],false];
  foreach(var (method,expected) in new[]{(nameof(CSharp3DeclarationSourceTests.Historical_declaration_forms_execute_in_the_third_factory),true),(nameof(CSharp3DeclarationSourceTests.Malformed_or_wrong_historical_declaration_categories_are_rejected),false)})
   foreach(var row in Data(typeof(CSharp3DeclarationSourceTests),method))
   {
    string source=(string)row[0];
    // Version4 main6123/6128 adds optional parameters. This exact predecessor negative changes.
    bool accepted=source=="class C{void M(int x=1){}}"||expected;
    yield return [method,"/*𐐀*/"+source,accepted];
   }
  foreach(var row in Data(typeof(CSharp3UnsafeSourceTests),nameof(CSharp3UnsafeSourceTests.Source_unsafe_forms_and_context_neighbors_are_checked)))yield return ["unsafe","/*𐐀*/"+row[0],row[1]];
  foreach(var row in CSharp3LiteralSourceTests.Spellings())
  {
   string source=(string)row[0]==nameof(LexicalTests.Identifier_fragments)?"class "+row[1]+"{}":"class C{object value="+row[1]+";}";
   yield return ["literal-"+row[0],source,row[2]];
  }
  foreach(var row in Data(typeof(CSharp3LiteralSourceTests),nameof(CSharp3LiteralSourceTests.Literal_escape_bounds_and_historical_numeric_suffixes_have_selected_factory_witnesses)))yield return ["literal-escape","class C{object value="+row[0]+";}",row[1]];
  foreach(var row in Data(typeof(CSharp3LiteralSourceTests),nameof(CSharp3LiteralSourceTests.Shift_components_remain_contiguous_lexemes)))
  {string op=(string)row[0];yield return ["shift","class C{void M(){x="+(op.EndsWith('=')?"(a"+op+"b)":"a"+op+"b")+";}}",row[1]];}
  foreach(var row in Data(typeof(CSharp3LiteralSourceTests),nameof(CSharp3LiteralSourceTests.Source_trivia_and_literal_boundaries_do_not_depend_on_nested_comment_or_directive_text)))yield return ["trivia",row[0],row[1]];
  foreach(var row in Data(typeof(CSharp3DirectiveSourceTests),nameof(CSharp3DirectiveSourceTests.Source_directive_alternatives_and_structure_are_checked)))yield return ["directive",row[0],row[1]];
 }
 [Theory][MemberData(nameof(Cases))]
 public void Predecessor_context_witnesses_execute_with_explicit_fourth_differences(string group,string source,bool expected)
 {
  using var p=CSharpParser.Parse(new(source,"CSharp4",group+".cs",[]));Assert.True(p.Accepted==expected,source+" :: "+string.Join(";",p.Issues));Assert.False(p.ProfileComplete);Assert.Equal(source.Length,p.MaskedText.Length);
  foreach(int n in Enumerable.Range(0,p.Tree.NodeCount)){var span=p.Tree.Span(n);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
  foreach(var issue in p.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
 }
 public static IEnumerable<object[]> Shapes()=>Data(typeof(CSharp3ExpressionSourceTests),nameof(CSharp3ExpressionSourceTests.Ambiguity_and_precedence_keep_authored_nested_expression_ranges));
 [Theory][MemberData(nameof(Shapes))]
 public void Inherited_ambiguities_retain_nested_expression_ownership_and_authored_ranges(string expression,string rootKind,string nestedKind,string nestedText)
 {
  string source="/*𐐀*/unsafe class C{object M(){return "+expression+";}}";using var p=CSharpParser.Parse(new(source,"CSharp4","shape.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));
  int start=source.IndexOf(expression,StringComparison.Ordinal);
  int root=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>Kind(n)==rootKind&&p.Tree.Span(n).Start==start&&p.Tree.Span(n).Length==expression.Length);
  int nested=Assert.Single(Enumerable.Range(0,p.Tree.NodeCount),n=>n!=root&&Kind(n)==nestedKind&&source.Substring(p.Tree.Span(n).Start,p.Tree.Span(n).Length)==nestedText);
  for(int n=nested;;n=p.Tree.Parent(n)){Assert.True(n>=0);if(n==root)break;}
  string Kind(int n)=>p.Tree.Language!.GetKindName(p.Tree.Kind(n));
 }
 [Theory][InlineData("\n")][InlineData("\r")][InlineData("\r\n")][InlineData("\u0085")][InlineData("\u2028")][InlineData("\u2029")]
 public void Source_line_terminators_preserve_class_and_directive_coordinates(string newline)
 {
  string source="// comment"+newline+"#define X"+newline+"#if X"+newline+"class C{}"+newline+"#endif";
  using var p=CSharpParser.Parse(new(source,"CSharp4","newlines.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(source.IndexOf("C",StringComparison.Ordinal),Assert.Single(p.Classes).Name.Span.Start);Assert.Equal(3,p.Directives.Count);
 }
 [Theory][InlineData("warning disable")][InlineData("warning restore 168,169")][InlineData("vendor payload")]
 public void Pragma_payload_retains_the_existing_explicit_compiler_effect_exclusion(string payload)
 {
  using var p=CSharpParser.Parse(new("#pragma "+payload+"\nclass C{}","CSharp4","pragma.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(payload,Assert.Single(p.Directives).Argument);Assert.Contains(p.Issues,i=>i.Code=="CSN1203"&&i.Severity==DiagnosticSeverity.Warning);
 }
}
