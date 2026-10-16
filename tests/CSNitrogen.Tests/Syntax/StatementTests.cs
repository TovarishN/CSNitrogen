using CSNitrogen.Syntax;
using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class StatementTests
{
 [Theory][InlineData("{ int x=1; x++; F(); }",true)][InlineData("a+b;",false)][InlineData("new C();",true)]
 [InlineData("if(a) if(b) F(); else G();",true)][InlineData("if(a) int x;",false)]
 [InlineData("for(int i=0;i<10;i++) F();",true)][InlineData("for(;;);",true)][InlineData("foreach(T x in xs) F(x);",true)]
 [InlineData("switch(x){ case 1: break; default: return; }",true)]
 [InlineData("try { F(); } catch(E e) { throw; } finally {}",true)][InlineData("try {}",false)]
 [InlineData("using(T x=F()) { F(x); }",true)][InlineData("lock(x) F();",true)][InlineData("label: goto label;",true)]
 [InlineData("while(x) {continue;}",true)][InlineData("do F(); while(x);",true)][InlineData("checked { F(); }",true)]
 [InlineData("using T x=F();",false)][InlineData("try{}catch(E e) when(x){}",false)]
 public void Statement_surface(string source,bool accepted)
 {var language=CSharp1Composition.CreateBuilder().Build();using var p=language.Parse(source,StatementsModule.StatementFragment);Assert.Equal(accepted,p.Success&&!p.HasErrors&&!CSharpSyntaxPolicy.Check(p.Tree).Any());}
}
