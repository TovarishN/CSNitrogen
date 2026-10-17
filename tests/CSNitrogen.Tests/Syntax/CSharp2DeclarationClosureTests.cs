using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp2DeclarationClosureTests
{
 [Theory]
 [InlineData("[A((x)=1)] class C{}")]
 [InlineData("[global::N.A] class C{}")]
 [InlineData("[A::N.Attr] class C{}")]
 [InlineData("class C{public override int P{protected set{}}}")]
 public void Historical_forms_are_accepted(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","declaration.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Theory]
 [InlineData("class C{int I.P{get{return 0;}private set{}}}")]
 [InlineData("class C{int this[ref int i]{get{return 0;}}}")]
 [InlineData("interface I{int this[out int i]{get;}}")]
 [InlineData("class C<T> where T:struct,new(){}")]
 [InlineData("class C<T> where T:int[]{}")]
 [InlineData("class C<T> where T:int?{}")]
 [InlineData("unsafe struct S{fixed decimal x[5];}")]
 public void Source_grammar_context_restrictions_are_enforced(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","declaration.cs",[]));Assert.False(p.Accepted);}
 [Theory][InlineData("[vendor:A] [assembly:B] class C{}",2)][InlineData("[vendor:A] namespace N{}",1)]
 public void Unknown_global_target_can_precede_known_globals_or_namespaces(string source,int count)
 {using var p=CSharpParser.Parse(new(source,"CSharp2","global.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(count,p.Root.Attributes.Count);Assert.Contains(p.Issues,i=>i.Code=="CSN2014");}
 [Fact] public void Unknown_global_target_is_accepted_with_a_warning()
 {using var p=CSharpParser.Parse(new("[vendor:A]","CSharp2","target.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(1,p.Root.Attributes.Count);Assert.Contains(p.Issues,i=>i.Code=="CSN2014");}
 [Theory][InlineData("vendor",true)][InlineData("typevar",false)]
 public void Attribute_target_recognition_is_version_correct(string target,bool warning)
 {using var p=CSharpParser.Parse(new("["+target+":A] class C{}","CSharp2","targets.cs",[]));Assert.True(p.Accepted);Assert.Equal(warning,p.Issues.Any(i=>i.Code=="CSN2014"));}
}
