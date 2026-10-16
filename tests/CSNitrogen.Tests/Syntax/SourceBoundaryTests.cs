using CSNitrogen.Parsing;
namespace CSNitrogen.Tests.Syntax;
public class SourceBoundaryTests
{
 [Theory][InlineData(16)][InlineData(64)]
 public void Nested_valid_blocks_and_parentheses_parse(int depth)
 {
  string source="class C{void M(){"+new string('{',depth)+"F("+new string('(',depth)+"x"+new string(')',depth)+");"+new string('}',depth)+"}}";
  using var parsed=CSharpParser.Parse(new(source,"CSharp1","nested.cs",[]));Assert.True(parsed.Accepted,string.Join(";",parsed.Issues));
 }
 [Theory][InlineData("class C{void M(){if(x){F();}")][InlineData("namespace N{class C{int M(){return a+;}}}")]
 public void Malformed_source_never_becomes_accepted(string source)
 {using var parsed=CSharpParser.Parse(new(source,"CSharp1","broken.cs",[]));Assert.False(parsed.Accepted);Assert.NotEmpty(parsed.Issues);}
}
