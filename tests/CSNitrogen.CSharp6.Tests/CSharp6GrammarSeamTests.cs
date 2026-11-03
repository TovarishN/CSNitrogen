namespace CSNitrogen.CSharp6.Tests;
public class CSharp6GrammarSeamTests
{
 public static TheoryData<string,string,string> Witnesses=>new()
 {
  {"class C{string s=$\"a{x}b\";}","InterpolatedRegularString","InterpolatedStrings"},
  {"class C{string s=$@\"a{x}\nb\";}","InterpolatedVerbatimString","InterpolatedStrings"},
  {"class C{object o=a?.b;}","NullConditionalAccess","NullConditional"},
  {"class C{string s=nameof(int);}","NameofExpression","NameofExpressions"},
  {"class C{void M(){try{}catch(E e)when(e!=null){}}}","ExceptionFilter","ExceptionFilters"},
  {"using static System.Math;","UsingStaticMarker","UsingStatic"},
  {"class C{int P{get;}=1;}","PropertyInitializer","PropertyInitializers"},
  {"class C{int M()=>1;}","ExpressionBody","ExpressionBodies"},
  {"class C{object o=new D{[0]=1};}","IndexInitializer","IndexInitializers"},
 };
 [Theory][MemberData(nameof(Witnesses))]
 public void Source_additions_have_real_generated_owners(string source,string kind,string module)
 {
  Assert.NotEmpty(Probe.Spans("All",source,kind));
  Assert.False(Probe.Accepts("None",source),source);
  Assert.False(Probe.Accepts(module,source),source);
 }
 [Theory][MemberData(nameof(Witnesses))]
 public void Omitting_one_module_keeps_every_other_witness(string source,string kind,string module)
 {
  foreach(var (other,_,owner) in Witnesses.Select(r=>((string)r[0],(string)r[1],(string)r[2])))
   if(owner!=module)Assert.True(Probe.Accepts(module,other),$"{module} omitted: {other}");
  _=kind;_=source;
 }
 [Theory]
 [InlineData("class C{int when;int nameof;void M(){nameof(when);}}")]
 [InlineData("class C{object o=a?b:c;}")]
 [InlineData("class C{object o=a?.5:1;}")]
 [InlineData("class C{void M(){try{}catch(E e){}catch{}}}")]
 [InlineData("using System;using A=System.Math;class C{int P{get;set;}object o=new D{A=1};}")]
 public void Baseline_ordinary_identifiers_remain_available(string source)
 {
  Assert.True(Probe.Accepts("None",source),source);
  Assert.True(Probe.Accepts("All",source),source);
 }
 [Fact]
 public void Numeric_after_question_dot_keeps_the_conditional_reading()
 {
  const string s="class C{object o=a?.5:1;}";
  Assert.Empty(Probe.Spans("All",s,"NullConditionalAccess"));
  Assert.Equal([Probe.At(s,"a?.5:1")],Probe.Spans("All",s,"ConditionalExpression"));
 }
 [Theory]
 [InlineData("class C{object o=a?.b;}","NullConditionalAccess","a?.b")]
 [InlineData("class C{object o=a?[0];}","NullConditionalElementAccess","a?[0]")]
 [InlineData("class C{object o=a?.b();}","NullConditionalAccess","a?.b")]
 [InlineData("class C{object o=a?.b.c?[1];}","NullConditionalElementAccess","a?.b.c?[1]")]
 public void Null_conditional_forms_have_exact_spans(string source,string kind,string text)
 {
  Assert.Contains(Probe.At(source,text),Probe.Spans("All",source,kind));
  string shifted="/*\U00010400*/"+source;
  Assert.Contains(Probe.At(shifted,text),Probe.Spans("All",shifted,kind));
 }
 [Theory]
 [InlineData("class C{void M(){try{}catch when(f){}}}")]
 [InlineData("class C{void M(){try{}catch(E)when(f){}catch(F e)when(e.X){}finally{}}}")]
 [InlineData("class C{async void M(){try{}catch(E)when(f){await x;}finally{await y;}}}")]
 public void Exception_filters_attach_to_specific_and_general_clauses(string source)=>Assert.NotEmpty(Probe.Spans("All",source,"ExceptionFilter"));
 [Theory]
 [InlineData("class C{void M(){try{}catch when(f){}catch(E){}}}")]
 [InlineData("class C{void M(){try{}catch(E)when{}}}")]
 [InlineData("using static A=B;")]
 [InlineData("using static;")]
 [InlineData("class C{object o=a?.;}")]
 [InlineData("class C{object o=new D{[]=1};}")]
 [InlineData("class C{int P{get;}=;}")]
 [InlineData("class C{int M()=>;}")]
 public void Malformed_sixth_forms_are_rejected(string source)=>Assert.False(Probe.Accepts("All",source),source);
 [Theory]
 [InlineData("class C{int P=>1;}","ExpressionBodiedProperty")]
 [InlineData("class C{int this[int i]=>i;}","ExpressionBodiedIndexer")]
 [InlineData("class C{public static C operator+(C a,C b)=>a;}","ExpressionBody")]
 [InlineData("class C{public static implicit operator int(C c)=>1;}","ExpressionBody")]
 [InlineData("class C{async Task M()=>await x;}","AwaitExpression")]
 [InlineData("class C{void M()=>x?.F();}","ExpressionBody")]
 [InlineData("class C{public int P{get;private set;}=F(1);}","PropertyInitializer")]
 [InlineData("class C{int[] P{get;}={1,2};}","PropertyInitializer")]
 [InlineData("class C{object o=new D{[0]=1,A=2,[1,2]={[3]=4}};}","IndexInitializer")]
 [InlineData("class C{object o=new D{[x:1]=1};}","IndexInitializer")]
 public void Member_and_initializer_forms_have_owners(string source,string kind)=>Assert.NotEmpty(Probe.Spans("All",source,kind));
 [Fact]
 public void Expression_body_spans_are_exact_after_supplementary_trivia()
 {
  const string s="/*\U00010400*/class C{int M()=>1+2;int P=>3;}";
  Assert.Equal([Probe.At(s,"=>1+2;")],Probe.Spans("All",s,"ExpressionBody"));
  Assert.Equal([Probe.At(s,"int P=>3;")],Probe.Spans("All",s,"ExpressionBodiedProperty"));
 }
}
