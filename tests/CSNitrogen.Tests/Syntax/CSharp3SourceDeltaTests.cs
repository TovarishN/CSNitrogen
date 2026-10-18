using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class CSharp3SourceDeltaTests
{
 [Theory][InlineData("class C{object[] x=new object[]{F<int>};}")][InlineData("class C{object x=new {F<int>};}")]
 public void Closing_brace_is_a_third_version_type_argument_follower(string source)
 {using var third=CSharpParser.Parse(new(source,"CSharp3","follower.cs",[]));Assert.True(third.Accepted,string.Join(";",third.Issues));using var second=CSharpParser.Parse(new(source,"CSharp2","previous.cs",[]));Assert.False(second.Accepted);Assert.True(second.ProfileComplete);}
 [Theory]
 [InlineData("[assem\\U000E0001bly:A] class C{}")][InlineData("[assem\U000E0001bly:A] class C{}")]
 public void Global_targets_use_the_approved_format_character_repertoire(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","target.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));Assert.Equal(1,p.Root.Attributes.Count);Assert.Equal(0,p.Classes[0].Attributes.Count);}
 [Theory][InlineData("[assem\u061cbly:A] class C{}")][InlineData("[assem\\u061cbly:A] class C{}")]
 public void Global_target_tokens_cannot_bypass_frozen_Unicode_validation(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","target.cs",[]));Assert.False(p.Accepted);}
 [Theory][InlineData("[vendor:A] class C{}")][InlineData("class C<[typevar:A] T>{}")]
 public void Target_spellings_absent_from_the_third_version_grammar_are_rejected(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","target.cs",[]));Assert.False(p.Accepted);}
 [Fact]public void Attribute_expression_grammar_does_not_perform_constant_binding()
 {using var p=CSharpParser.Parse(new("[A(X=y=1)] class C{}","CSharp3","attribute.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}

 [Theory]
 [InlineData(@"p\u0061rtial class C{partial void M();}",true)]
 [InlineData(@"partial class C{p\u0061rtial void M();}",true)]
 [InlineData(@"partial class C{public p\u0061rtial void M();}",false)]
 public void Partial_method_policy_uses_the_contextual_identifier_identity(string source,bool accepted)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","partial.cs",[]));Assert.Equal(accepted,p.Accepted);}

 [Theory]
 [InlineData("[@return:A] class C{}")]
 [InlineData(@"[ret\u0075rn:A] class C{}")]
 [InlineData("[ev\u200cent:A] class C{}")]
 public void Third_version_reserved_target_keywords_require_raw_spelling(string source)
 {using var third=CSharpParser.Parse(new(source,"CSharp3","target.cs",[]));Assert.False(third.Accepted);using var second=CSharpParser.Parse(new(source,"CSharp2","target.cs",[]));Assert.True(second.Accepted,string.Join(";",second.Issues));}
 [Theory][InlineData("[return:A] class C{}")][InlineData(@"[f\u0069eld:A] class C{}")]
 public void Raw_keyword_and_normalized_contextual_targets_remain_available(string source)
 {using var p=CSharpParser.Parse(new(source,"CSharp3","target.cs",[]));Assert.True(p.Accepted,string.Join(";",p.Issues));}
 [Fact]public void Third_generic_follower_module_is_explicit_and_removable()
 {
  var full=CSNitrogen.Syntax.CSharp3Composition.CreateBuilder().Build();var builder=new Nitrogen.LanguageBuilder();
  Assert.Contains(full.Modules,m=>m.Name=="CSharp.GenericFollowersThird");foreach(var m in full.Modules)if(m.Name!="CSharp.GenericFollowersThird")builder.Add(m);var language=builder.Build();
  using var absent=language.Parse("class C{object[] x=new object[]{F<int>};}",CSNitrogen.Syntax.CSharp3Composition.Start);Assert.False(absent.Success&&!absent.HasErrors);
  using var ordinary=language.Parse("class C{object x=F<int>();}",CSNitrogen.Syntax.CSharp3Composition.Start);Assert.True(ordinary.Success&&!ordinary.HasErrors);
 }
 [Fact]public void Third_generic_follower_capability_requires_explicit_generics()
 {var p=CSNitrogen.Profiles.ProfileResolver.ResolveCustom("incomplete",["csharp3.generic-disambiguation"]);Assert.Null(p.Profile);Assert.Contains(p.Issues,i=>i.Code=="missing-requirement");}

}
