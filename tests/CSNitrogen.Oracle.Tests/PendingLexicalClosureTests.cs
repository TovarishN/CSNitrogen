using CSNitrogen.Parsing;
using CSNitrogen.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nitrogen;

namespace CSNitrogen.Oracle.Tests;

/// <summary>
/// ECMA-334 first edition §§9.3-9.5 alternative/boundary evidence for A353..359.
/// Original Unicode3 BMP/category/formatting and exact BigInteger mapping matrices
/// remain in HistoricalUnicodeParityTests and ExactLineMappingTests respectively.
/// Historical source expectations are normative; compiler differences are explicit.
/// </summary>
public class PendingLexicalClosureTests
{
    private const string Prefix = "class C{object M(){return ";
    private static IEnumerable<int> Nodes(ParseDocument doc, string kind) =>
        Enumerable.Range(0, doc.Tree.NodeCount).Where(i => doc.Tree.Language!.GetKindName(doc.Tree.Kind(i)) == kind);

    private static void Bounds(ParseDocument doc, string source)
    {
        Assert.All(doc.Issues, issue =>
        {
            Assert.InRange(issue.Span.Start, 0, source.Length);
            Assert.InRange(issue.Span.End, issue.Span.Start, source.Length);
        });
    }

    public static IEnumerable<object[]> Literals()
    {
        foreach (string literal in new[] { "0", "42", "0x2A", "0XfF" }
            .Concat(new[] { "U", "u", "L", "l", "UL", "Ul", "uL", "ul", "LU", "Lu", "lU", "lu" }.Select(s => "42" + s)))
            yield return [literal, "Integer"];
        foreach (string literal in new[] { "1.5", ".5", "1e2", "1E+2", "1e-2" }
            .Concat("fFdDmM".Select(c => "1" + c))) yield return [literal, "Real"];
        foreach (string escape in new[] { "'", "\"", "\\", "0", "a", "b", "f", "n", "r", "t", "v", "x1", "x12", "x123", "x1234", "u0041", "U00000041" })
            yield return ["'\\" + escape + "'", "Character"];
        foreach (string literal in new[] { "\"\"", "\"\\u005Cu005C\"", "\"\\U00010000\"", "\"\\U0010FFFF\"", "\"\u0085\"" })
            yield return [literal, "RegularString"];
        foreach (string literal in new[] { "@\"\"", "@\"a\"\"b\"", "@\"\\u0041\"", "@\"a\r\nb\u2028c\u2029d\"" })
            yield return [literal, "VerbatimString"];
        foreach (string literal in new[] { "true", "false", "null" }) yield return [literal, "Literal"];
    }

    [Theory, MemberData(nameof(Literals))]
    public void Literal_alternatives_retain_exact_authored_spans(string literal, string kind)
    {
        string source = Prefix + literal + ";}}";
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "literal.cs", []));
        Assert.True(doc.Accepted, source + " :: " + string.Join(";", doc.Issues));
        int node = Assert.Single(Nodes(doc, kind), i => doc.Tree.Span(i).Start == Prefix.Length && doc.Tree.Span(i).Length == literal.Length
            && (kind != "Literal" || doc.Tree.ChildCount(i) == 0));
        Assert.Equal(literal, doc.Tree.Text.Substring(doc.Tree.Span(node).Start, doc.Tree.Span(node).Length));
        Assert.Equal(source, doc.OriginalText);
        Bounds(doc, source);
    }

    public static IEnumerable<object[]> InvalidLiterals()
    {
        foreach (string literal in new[] { "0x", "0b10", "1_000", "42UU", "42ULU", "1e", "1e+", "1.2ff", "''", "'xy'", "'\\x'", "'\\e'", "'\\U00010000'", "\"\\u123\"", "\"\\U00110000\"", "\"\\x\"" })
            yield return [literal];
        foreach (string newline in new[] { "\r", "\n", "\u2028", "\u2029" })
            foreach (string quote in new[] { "\"", "'" }) yield return [quote + "a" + newline + quote];
    }

    [Theory, MemberData(nameof(InvalidLiterals))]
    public void Malformed_literal_alternatives_are_source_located(string literal)
    {
        string source = Prefix + literal + ";}}";
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "bad-literal.cs", []));
        Assert.False(doc.Accepted);
        Bounds(doc, source);
        Assert.Contains(doc.Issues, i => i.Span.Start >= Prefix.Length && i.Span.Start <= Prefix.Length + literal.Length + 1);
    }

    [Theory]
    [InlineData("1.ToString()")]
    [InlineData("1.0.ToString()")]
    [InlineData("42UL.ToString()")]
    [InlineData("1e+2-3")]
    [InlineData("0x1f+2")]
    [InlineData("a+++b")]
    [InlineData("a---b")]
    public void Longest_numeric_and_operator_tokens_match_source_expression_shape(string expression)
    {
        string source = Prefix + expression + ";}}";
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "munch.cs", []));
        Assert.True(doc.Accepted, source + " :: " + string.Join(";", doc.Issues));
        var reference = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp1));
        Assert.DoesNotContain(reference.GetDiagnostics(), d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        var expected = Assert.Single(reference.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>()).Expression!;
        int actual = Assert.Single(Enumerable.Range(0, doc.Tree.NodeCount), i =>
            doc.Tree.Language!.GetKindName(doc.Tree.Kind(i)).EndsWith("Expression", StringComparison.Ordinal)
            && doc.Tree.Span(i).Start == Prefix.Length && doc.Tree.Span(i).Length == expression.Length);
        ExpressionShapeAssert.Compare(expected, new ExpressionNode(doc.Tree, actual));
        foreach (var literal in expected.DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>())
        {
            string kind = literal.Token.Text.Contains('.') || literal.Token.Text.Contains('e') && !literal.Token.Text.StartsWith("0x", StringComparison.Ordinal) ? "Real" : "Integer";
            Assert.Contains(Nodes(doc, kind), i => doc.Tree.Span(i).Start == literal.SpanStart && doc.Tree.Span(i).Length == literal.Span.Length);
        }
    }

    public static IEnumerable<object[]> Newlines()
    {
        foreach (string newline in new[] { "\r", "\n", "\r\n", "\u2028", "\u2029" }) yield return [newline];
    }

    [Theory, MemberData(nameof(Newlines))]
    public void Comment_terminators_and_directive_newlines_keep_physical_spans(string newline)
    {
        foreach (string comment in new[] { "// #if false /* ignored */", "/* // ignored /* marker */" })
        {
            string source = comment + newline + "class C{}";
            using var doc = CSharpParser.Parse(new(source, "CSharp1", "comment.cs", []));
            Assert.True(doc.Accepted, string.Join(";", doc.Issues));
            Assert.Empty(doc.Directives);
            var name = Assert.Single(doc.Classes).Name;
            Assert.Equal(source.IndexOf("C{}", StringComparison.Ordinal), name.Span.Start);
            Assert.Equal(1, doc.MapLocationExact(name.Span.Start).PhysicalLine);
        }
        string directives = string.Join(newline, new[] { " \t# \tdefine \tX\t// end", "\v# undef X// end", "\f#if true\t// end", "class C{}", "#else// end", "\"broken inactive", "#endif// end" });
        using var parsed = CSharpParser.Parse(new(directives, "CSharp1", "directives.cs", []));
        Assert.True(parsed.Accepted, string.Join(";", parsed.Issues));
        Assert.Equal(5, parsed.Directives.Count);
        var className = Assert.Single(parsed.Classes).Name;
        Assert.Equal(directives.IndexOf("C{}", StringComparison.Ordinal), className.Span.Start);
        Assert.Equal(3, parsed.MapLocationExact(className.Span.Start).PhysicalLine);
        Assert.Equal(directives.Length, parsed.MaskedText.Length);
        for (int i = 0; i < directives.Length; i++)
            if (directives[i] is '\r' or '\n' or '\u2028' or '\u2029') Assert.Equal(directives[i], parsed.MaskedText[i]);
        foreach (var directive in parsed.Directives)
        {
            Assert.Equal('#', directives[directive.Span.Start]);
            string authored = directives.Substring(directive.Span.Start, directive.Span.Length);
            Assert.DoesNotContain('\r', authored); Assert.DoesNotContain('\n', authored);
            Assert.DoesNotContain('\u2028', authored); Assert.DoesNotContain('\u2029', authored);
        }
    }

    [Theory]
    [InlineData("// terminal")]
    [InlineData("// NEL\u0085class Hidden{}")]
    [InlineData("/**/")]
    [InlineData("/***/")]
    [InlineData("/*a**b*/")]
    public void Comments_without_final_newline_form_empty_compilation_units(string source)
    {
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "empty.cs", []));
        Assert.True(doc.Accepted); Assert.Empty(doc.Classes); Assert.Empty(doc.Directives);
        Assert.Equal(source, doc.OriginalText); Assert.Equal(source.Length, doc.MaskedText.Length);
    }

    [Theory]
    [InlineData("class C{}")]
    [InlineData("class C{}\u001a")]
    [InlineData("class C{}\n\u001a")]
    public void EOF_transforms_preserve_authored_source_coordinates(string source)
    {
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "eof.cs", []));
        Assert.True(doc.Accepted);
        Assert.Equal(source, doc.OriginalText); Assert.Equal(source.Length, doc.MaskedText.Length);
        Assert.Equal(source.IndexOf("C{}", StringComparison.Ordinal), Assert.Single(doc.Classes).Name.Span.Start);
    }

    [Theory]
    [InlineData("/*")]
    [InlineData("/* /* */ */")]
    [InlineData("class C{}\u001aclass D{}")]
    public void Unterminated_nonnesting_comments_and_nonterminal_ControlZ_fail(string source)
    {
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "bad-input.cs", []));
        Assert.False(doc.Accepted); Bounds(doc, source);
    }

    public static IEnumerable<object[]> BadDirectives()
    {
        foreach (string directive in new[] { "#define X /*bad*/", "#undef X /*bad*/", "#if true /*bad*/\n#endif", "#if true\n#else /*bad*/\n#endif", "#if true\n#endif /*bad*/", "#line 1 /*bad*/", "#define(X)", "#undef(X)", "#elif(true)", "#line(1)" })
            yield return [directive];
    }

    [Theory, MemberData(nameof(BadDirectives))]
    public void Directive_whitespace_and_comment_grammar_is_checked_in_active_and_skipped_sections(string directive)
    {
        foreach (string source in new[] { directive, "#if false\n" + directive + "\n#endif" })
        {
            using var doc = CSharpParser.Parse(new(source, "CSharp1", "bad-directive.cs", []));
            Assert.False(doc.Accepted); Bounds(doc, source);
            Assert.Contains(doc.Issues, i => i.Category == IssueCategory.Preprocessing);
        }
    }

    [Theory]
    [InlineData("#if false\nnot # a directive\n#endif\nclass C{}", true)]
    [InlineData("#if false\n  #unknown\n#endif", false)]
    [InlineData("#if false\n\"broken <<< /*\n#endregion\n#endif", false)]
    public void Skipped_text_is_unlexed_but_directive_structure_is_validated(string source, bool accepted)
    {
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "skipped.cs", []));
        Assert.Equal(accepted, doc.Accepted); Bounds(doc, source);
        if (accepted) Assert.Single(doc.Classes);
    }

    [Theory]
    [InlineData("true || false && false", true)]
    [InlineData("true == false && false == false", false)]
    [InlineData("!false != false && (X || false)", true)]
    [InlineData("X != Y == true", true)]
    public void Conditional_operator_precedence_selects_authored_branch(string condition, bool selected)
    {
        string source = "#if " + condition + "\nclass A{}\n#else\nclass B{}\n#endif";
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "precedence.cs", ["X"]));
        Assert.True(doc.Accepted, string.Join(";", doc.Issues));
        var name = Assert.Single(doc.Classes).Name;
        string expected = selected ? "A" : "B";
        Assert.Equal(expected, name.ToString());
        Assert.Equal(source.IndexOf(expected + "{}", StringComparison.Ordinal), name.Span.Start);
        Assert.Equal(1, name.Span.Length);
        Assert.Equal(selected ? 1 : 3, doc.MapLocationExact(name.Span.Start).PhysicalLine);
        Assert.Equal(3, doc.Directives.Count);
        Bounds(doc, source);
    }

    [Theory]
    [InlineData("a//b.cs")]
    [InlineData("a\\b.cs")]
    [InlineData("a\u0085b.cs")]
    public void Filename_characters_are_literal_and_trailing_comments_are_distinct(string file)
    {
        string directive = "#line 12 \"" + file + "\"// tail";
        string source = directive + "\nclass C{}";
        using var doc = CSharpParser.Parse(new(source, "CSharp1", "filename.cs", []));
        Assert.True(doc.Accepted, string.Join(";", doc.Issues));
        var name = Assert.Single(doc.Classes).Name;
        Assert.Equal(source.IndexOf("C{}", StringComparison.Ordinal), name.Span.Start);
        var mapped = doc.MapLocationExact(name.Span.Start);
        Assert.Equal(file, mapped.File); Assert.Equal(11, (int)mapped.LogicalLine);
        var record = Assert.Single(doc.Directives);
        Assert.Equal(0, record.Span.Start); Assert.Equal(directive.Length, record.Span.Length);
    }

    [Theory]
    [InlineData("#line 1 \"a\" junk\nclass C{}")]
    [InlineData("#line 1 \"a\\\"b\"\nclass C{}")]
    [InlineData("#line 1 \"a\u2028b\"\nclass C{}")]
    [InlineData("#line 1 \"a\u2029b\"\nclass C{}")]
    public void Invalid_filenames_are_lexically_invalid_even_when_inactive(string text)
    {
        foreach (string source in new[] { text, "#if false\n" + text + "\n#endif" })
        {
            using var doc = CSharpParser.Parse(new(source, "CSharp1", "bad-file.cs", []));
            Assert.False(doc.Accepted); Bounds(doc, source);
            Assert.Contains(doc.Issues, i => i.Code == "CSN1207");
        }
    }

    [Fact]
    public void Diagnostic_and_region_messages_preserve_arbitrary_text_without_tokenizing_it()
    {
        const string message = "arbitrary \" ' /* */ // text";
        string directive = "#warning " + message;
        string source = directive + "\nclass C{}";
        using var warning = CSharpParser.Parse(new(source, "CSharp1", "message.cs", []));
        Assert.True(warning.Accepted);
        var issue = Assert.Single(warning.Issues, i => i.Code == "CSN1203");
        Assert.Equal(message, issue.Message); Assert.Equal(Nitrogen.DiagnosticSeverity.Warning, issue.Severity);
        Assert.Equal(0, issue.Span.Start); Assert.Equal(directive.Length, issue.Span.Length);
        using var inactive = CSharpParser.Parse(new("#if false\n#error " + message + "\n#warning " + message + "\n#endif\nclass C{}", "CSharp1", "inactive-message.cs", []));
        Assert.True(inactive.Accepted); Assert.Empty(inactive.Issues);
        using var region = CSharpParser.Parse(new("#region arbitrary \" /* //\nclass C{}\n#endregion different ' */", "CSharp1", "region.cs", []));
        Assert.True(region.Accepted); Assert.Single(region.Classes);
        using var crossing = CSharpParser.Parse(new("#region x\n#if true\n#endregion\n#endif", "CSharp1", "crossing.cs", []));
        Assert.False(crossing.Accepted); Assert.Contains(crossing.Issues, i => i.Code == "CSN1201");
    }
}
