using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSNitrogen.Syntax;
using Nitrogen;
namespace CSNitrogen.Oracle.Tests;
public unsafe class LexicalParityTests
{
    [Fact] public void Pinned_BMP_identifier_categories_match_the_pinned_Roslyn_runtime()
    {
        using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"unicode-data.json")));
        var start=json.RootElement.GetProperty("start").EnumerateArray().Select(e=>e.GetInt32()).ToHashSet();
        var part=json.RootElement.GetProperty("part").EnumerateArray().Select(e=>e.GetInt32()).ToHashSet();
        for(int code=0;code<=0xffff;code++)
        {Assert.Equal(SyntaxFacts.IsIdentifierStartCharacter((char)code),start.Contains(code));Assert.Equal(SyntaxFacts.IsIdentifierPartCharacter((char)code),part.Contains(code));}
    }
    [Theory][InlineData("@class")][InlineData("Δelta")][InlineData("\\u0043")][InlineData("a\\u0301")][InlineData("cl\\u0061ss")]
    [InlineData("a\u200Cb")][InlineData("a\\U000003B1")]
    public void Identifier_spelling_and_class_source_match_Roslyn(string name)
    {
        string source="class "+name+" {}";var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        using var ours=CSNitrogen.Parsing.CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));
        Assert.DoesNotContain(oracle.CompilationDiagnostics,d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.True(ours.Accepted);Assert.Equal(name,ours.Classes[0].Name.ToString());
    }
    [Theory][InlineData("'x'",false)][InlineData("'\\u0041'",false)][InlineData("'\\U0000D800'",false)]
    [InlineData("\"a\\\"b\"",true)][InlineData("@\"a\"\"b\"",true)][InlineData("\"\\U0001F600\"",true)]
    [InlineData("\"\\U00110000\"",true)][InlineData("'xy'",false)][InlineData("'\\x'",false)]
    public void Character_and_string_fragment_syntax_matches_Roslyn(string text,bool isString)
    {
        var token=SyntaxFactory.ParseToken(text);bool expected=!token.ContainsDiagnostics&&token.FullSpan.Length==text.Length;
        var language=CSharp1Composition.CreateBuilder().WithTrivia(&StandardTrivia.None).Build();
        using var result=language.Parse(text,isString?LexicalModule.StringFragment:LexicalModule.CharacterFragment);
        Assert.Equal(expected,result.Success&&!result.HasErrors);
    }
    [Theory][InlineData("__arglist")][InlineData("__makeref")][InlineData("__reftype")][InlineData("__refvalue")]
    public void Compiler_reserved_intrinsic_spellings_are_not_class_identifiers(string name)
    {
        string source="class "+name+" {}";var oracle=Oracle.Observe(source,LanguageVersion.CSharp1);
        using var ours=CSNitrogen.Parsing.CSharpParser.Parse(new(source,"CSharp1","test.cs",[]));
        Assert.Equal(!oracle.SyntaxDiagnostics.Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error),ours.Accepted);
    }
    [Fact] public void Generated_escaped_BMP_identifier_tries_match_all_start_and_part_categories()
    {
        var language=CSharp1Composition.CreateBuilder().WithTrivia(&StandardTrivia.None).Build();
        for(int code=0;code<=0xffff;code++)
        {
            string escape="\\u"+code.ToString(code%2==0?"X4":"x4",System.Globalization.CultureInfo.InvariantCulture);
            using var first=language.Parse(escape,LexicalModule.IdentifierFragment);
            using var part=language.Parse("a"+escape,LexicalModule.IdentifierFragment);
            Assert.True(SyntaxFacts.IsIdentifierStartCharacter((char)code)==(first.Success&&!first.HasErrors&&!CSNitrogen.Parsing.CSharpSyntaxPolicy.Check(first.Tree,new(14,0)).Any()),"Start: "+escape);
            Assert.True(SyntaxFacts.IsIdentifierPartCharacter((char)code)==(part.Success&&!part.HasErrors&&!CSNitrogen.Parsing.CSharpSyntaxPolicy.Check(part.Tree,new(14,0)).Any()),"Part: "+escape);
        }
    }
}
