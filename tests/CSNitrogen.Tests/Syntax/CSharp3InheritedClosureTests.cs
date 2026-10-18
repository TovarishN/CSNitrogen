using System.Reflection;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3InheritedClosureTests
{
 public static IEnumerable<object[]> Cases()
 {
  var groups=new (Type Type,string Method,bool Accepted,bool Expression)[]{
   (typeof(CSharp2ContextClosureTests),nameof(CSharp2ContextClosureTests.Source_only_context_restrictions_are_enforced),false,false),
   (typeof(CSharp2ContextClosureTests),nameof(CSharp2ContextClosureTests.Neighboring_legal_syntax_remains_accepted),true,false),
   (typeof(CSharp2DeclarationClosureTests),nameof(CSharp2DeclarationClosureTests.Historical_forms_are_accepted),true,false),
   (typeof(CSharp2DeclarationClosureTests),nameof(CSharp2DeclarationClosureTests.Source_grammar_context_restrictions_are_enforced),false,false),
   (typeof(CSharp2AmbiguityClosureTests),nameof(CSharp2AmbiguityClosureTests.Every_historical_generic_follower_retains_type_arguments),true,true),
   (typeof(CSharp2AmbiguityClosureTests),nameof(CSharp2AmbiguityClosureTests.Dotted_alias_names_preserve_expression_ambiguity),true,true),
   (typeof(CSharp2AmbiguityClosureTests),nameof(CSharp2AmbiguityClosureTests.Nullable_construction_in_conditional_arms_remains_a_type),true,true),
   (typeof(CSharp2AmbiguityClosureTests),nameof(CSharp2AmbiguityClosureTests.Type_operator_question_mark_preserves_conditional_shape),true,true),
   (typeof(Lexing.CSharp2LexicalClosureTests),nameof(Lexing.CSharp2LexicalClosureTests.Historical_lexical_spellings_are_supported),true,false),
   (typeof(Lexing.CSharp2LexicalClosureTests),nameof(Lexing.CSharp2LexicalClosureTests.Directive_arguments_require_whitespace),false,false)};
  foreach(var group in groups)
  {
   var method=group.Type.GetMethod(group.Method)!;
   foreach(var attribute in method.GetCustomAttributes<InlineDataAttribute>())foreach(var row in attribute.GetData(method))
   {
    string source=(string)row[0];bool accepted=group.Accepted;
    if(source.StartsWith("#define @class",StringComparison.Ordinal))accepted=false; // CSharp3 conditional-symbol production.
    string kind=group.Method==nameof(CSharp2AmbiguityClosureTests.Every_historical_generic_follower_retains_type_arguments)?"GenericNameExpression":
     group.Method==nameof(CSharp2AmbiguityClosureTests.Type_operator_question_mark_preserves_conditional_shape)?"ConditionalExpression":
     group.Method==nameof(CSharp2AmbiguityClosureTests.Dotted_alias_names_preserve_expression_ambiguity)?(string)row[1]:"";
    if(group.Expression)source="class C{object M(){return "+source+";}}";
    yield return [group.Type.Name+"."+group.Method,source,accepted,kind];
   }
  }
 }
 [Theory][MemberData(nameof(Cases))]
 public void Inherited_normative_neighbors_and_ambiguities_use_the_third_factory(string origin,string source,bool accepted,string kind)
 {
  using var parsed=CSharpParser.Parse(new(source,"CSharp3",origin,[]));
  Assert.True(parsed.Accepted==accepted,source+" :: "+string.Join(";",parsed.Issues));Assert.True(parsed.ProfileComplete);
  if(kind.Length>0)Assert.Contains(Enumerable.Range(0,parsed.Tree.NodeCount),n=>parsed.Tree.Language!.GetKindName(parsed.Tree.Kind(n))==kind);
  foreach(int node in Enumerable.Range(0,parsed.Tree.NodeCount))
  {var span=parsed.Tree.Span(node);Assert.InRange(span.Start,0,source.Length);Assert.InRange(span.Length,0,source.Length-span.Start);}
  foreach(var issue in parsed.Issues){Assert.InRange(issue.Span.Start,0,source.Length);Assert.InRange(issue.Span.Length,0,source.Length-issue.Span.Start);}
 }
 [Theory]
 [InlineData("interface I{void M();void N<T>();}")]
 [InlineData("unsafe struct S{fixed int x[5],y[10];}")]
 [InlineData("class C{N.G<int>.H<string> field;}")]
 public void Main_text_examples_and_prose_resolve_printed_summary_defects(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","summary.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
}
