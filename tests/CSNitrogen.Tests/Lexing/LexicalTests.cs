using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Tests.Lexing;
public class LexicalTests
{
    [Theory]
    [InlineData("C",true)][InlineData("_x",true)][InlineData("@class",true)][InlineData("Δelta",true)]
    [InlineData("\\u0043",true)][InlineData("a\\u0301",true)][InlineData("\\u0030x",false)]
    [InlineData("",false)][InlineData("0x",false)][InlineData("\\u12",false)][InlineData("class",false)]
    public void Identifier_fragments(string text,bool accepted)=>Check(text,LexicalModule.IdentifierFragment,accepted);
    [Theory][InlineData("0",true)][InlineData("42",true)][InlineData("0x2A",true)][InlineData("42UL",true)]
    [InlineData("0b10",false)][InlineData("1_000",false)][InlineData("0x",false)][InlineData("42UU",false)]
    public void Integer_fragments(string text,bool accepted)=>Check(text,LexicalModule.IntegerFragment,accepted);
    [Theory][InlineData(".5",true)][InlineData("1.5",true)][InlineData("1e2",true)][InlineData("1f",true)][InlineData("1m",true)]
    [InlineData("1e",false)][InlineData("1.2ff",false)]
    public void Real_fragments(string text,bool accepted)=>Check(text,LexicalModule.RealFragment,accepted);
    [Theory][InlineData("'x'",true)][InlineData("'\\n'",true)][InlineData("'\\u0041'",true)]
    [InlineData("''",false)][InlineData("'xy'",false)][InlineData("'\\e'",false)][InlineData("'\\U00110000'",false)]
    public void Character_fragments(string text,bool accepted)=>Check(text,LexicalModule.CharacterFragment,accepted);
    [Theory][InlineData("\"\"",true)][InlineData("\"a\\\"b\"",true)][InlineData("@\"a\"\"b\"",true)]
    [InlineData("\"unterminated",false)][InlineData("$\"x\"",false)][InlineData("\"\"\"x\"\"\"",false)][InlineData("\"x\"u8",false)]
    [InlineData("\"\\e\"",false)][InlineData("\"\\u0041\"",true)][InlineData("\"\\U0001F600\"",true)]
    public void String_fragments(string text,bool accepted)=>Check(text,LexicalModule.StringFragment,accepted);
    private static void Check(string source,Rule rule,bool accepted)
    {
        var language=CSharp1Composition.CreateBuilder().Build();using var result=language.Parse(source,rule);
        Assert.Equal(accepted,result.Success && !result.HasErrors);
        if(accepted) Assert.Equal(source,result.Tree.Text);
    }
}
