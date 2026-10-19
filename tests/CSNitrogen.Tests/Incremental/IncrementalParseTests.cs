using System.Text;
using CSNitrogen.Parsing;
using CSNitrogen.Preprocessing;
using Nitrogen;

namespace CSNitrogen.Tests.Incremental;

/// <summary>
/// Incremental reparse of C#: every reparsed document must equal a full parse of the same text: tree node for node,
/// issues, masked text, directives, inactive spans and line mappings. Preprocessing is spliced only for edits that
/// cannot change the directive structure; any other edit preprocesses in full.
/// </summary>
public class IncrementalParseTests
{
    static ParseRequest Request(string source, string profile) => new(source, profile, "doc.cs", []);

    static (string Text, TextChange[] Changes) Edit(string old, params (int Start, int OldLength, string New)[] edits)
    {
        var text = new StringBuilder();
        int at = 0;
        foreach (var e in edits)
        {
            text.Append(old, at, e.Start - at).Append(e.New);
            at = e.Start + e.OldLength;
        }
        text.Append(old, at, old.Length - at);
        return (text.ToString(), edits.Select(e => new TextChange(e.Start, e.OldLength, e.New.Length)).ToArray());
    }

    /// <summary>A C# 1 file with active and inactive conditional blocks, comments and strings.</summary>
    internal static string Generated(int classes) => "using System;\n" + string.Concat(Enumerable.Range(0, classes).Select(i => $$"""
        namespace N{{i}}
        {
            // class number {{i}}
            public class C{{i}}
            {
                private int _value = {{i}};
                /* a block
                   comment */
                public int Get(int x)
                {
                    int total = 0; // anchor
                    string s = "a \"quoted\" {{i}}";
                    char c = 'q';
                    for (int k = 0; k < x; k++) { total += k * {{i}}; }
        #if DEBUG
                    Console.WriteLine(s);
        #else
                    total++;
        #endif
                    return total + _value;
                }
            }
        }

        """));

    static void AssertSame(ParseDocument full, ParseDocument incremental)
    {
        Assert.Equal(full.MaskedText, incremental.MaskedText);
        Assert.Equal(full.Directives, incremental.Directives);
        Assert.Equal(full.InactiveSpans, incremental.InactiveSpans);
        Assert.Equal(full.Preprocessing.ExactLineMappings, incremental.Preprocessing.ExactLineMappings);
        Assert.Equal(full.Issues.Select(i => (i.Code, i.Message, i.Span, i.Category, i.Severity)),
            incremental.Issues.Select(i => (i.Code, i.Message, i.Span, i.Category, i.Severity)));
        Assert.Equal(full.Accepted, incremental.Accepted);
        var e = full.Tree;
        var a = incremental.Tree;
        Assert.Equal(e.Text, a.Text);
        Assert.Equal(e.NodeCount, a.NodeCount);
        for (int n = 0; n < e.NodeCount; n++)
        {
            Assert.True(e.Kind(n) == a.Kind(n), $"kind of node {n}");
            Assert.True(e.Span(n) == a.Span(n), $"span of node {n}: {e.Span(n)} vs {a.Span(n)}");
            Assert.True(e.Flags(n) == a.Flags(n), $"flags of node {n}");
            Assert.True(e.ChildCount(n) == a.ChildCount(n), $"child count of node {n}");
            for (int k = 0; k < e.ChildCount(n); k++) Assert.True(e.Child(n, k) == a.Child(n, k), $"child {k} of node {n}");
        }
        Assert.Equal(e.Trivia.ToArray(), a.Trivia.ToArray());
        Assert.Equal(e.SkippedSpans.ToArray(), a.SkippedSpans.ToArray());
    }

    [Fact]
    public void An_edit_in_code_splices_preprocessing_and_reuses_the_tree()
    {
        string old = Generated(20);
        using var previous = CSharpParser.ParseIncremental(Request(old, "CSharp1"));
        int at = old.IndexOf("total += k * 10", StringComparison.Ordinal) + "total += k * ".Length;
        var (text, changes) = Edit(old, (at, 2, "77"));

        var preprocessed = ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(text, new Version(1, 0), []), changes, out bool spliced);
        using var reparsed = CSharpParser.Reparse(previous, Request(text, "CSharp1"), changes);

        Assert.True(spliced);
        Assert.Equal(ConditionalPreprocessor.Process(new(text, new Version(1, 0), [])).MaskedText, preprocessed.MaskedText);
        using var full = CSharpParser.Parse(Request(text, "CSharp1"));
        AssertSame(full, reparsed);
        Assert.True(reparsed.IsIncremental);
        Assert.True(reparsed.ReusedNodes > reparsed.Tree.NodeCount / 2, $"{reparsed.ReusedNodes} of {reparsed.Tree.NodeCount} reused");
    }

    [Fact]
    public void A_new_line_in_code_still_splices()
    {
        string old = Generated(5);
        using var previous = CSharpParser.ParseIncremental(Request(old, "CSharp1"));
        int at = old.IndexOf("int total = 0; // anchor", StringComparison.Ordinal) + "int total = 0;".Length;
        var (text, changes) = Edit(old, (at, 0, "\n            total += 42;"));

        ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(text, new Version(1, 0), []), changes, out bool spliced);
        using var reparsed = CSharpParser.Reparse(previous, Request(text, "CSharp1"), changes);

        Assert.True(spliced);
        using var full = CSharpParser.Parse(Request(text, "CSharp1"));
        AssertSame(full, reparsed);
    }

    [Fact]
    public void Directive_edit_falls_back_to_full_preprocessing()
    {
        string old = Generated(5);
        using var previous = CSharpParser.ParseIncremental(Request(old, "CSharp1"));
        int at = old.IndexOf("            return total", StringComparison.Ordinal);
        var (text, changes) = Edit(old, (at, 0, "#if X\n#endif\n"));

        ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(text, new Version(1, 0), []), changes, out bool spliced);
        using var reparsed = CSharpParser.Reparse(previous, Request(text, "CSharp1"), changes);

        Assert.False(spliced);
        using var full = CSharpParser.Parse(Request(text, "CSharp1"));
        AssertSame(full, reparsed);
    }

    [Fact]
    public void Edit_inside_inactive_region_falls_back()
    {
        string old = Generated(5);
        using var previous = CSharpParser.ParseIncremental(Request(old, "CSharp1"));
        int at = old.IndexOf("Console.WriteLine(s)", StringComparison.Ordinal) + "Console.".Length;
        var (text, changes) = Edit(old, (at, 5, "Error"));

        ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(text, new Version(1, 0), []), changes, out bool spliced);
        using var reparsed = CSharpParser.Reparse(previous, Request(text, "CSharp1"), changes);

        Assert.False(spliced);
        using var full = CSharpParser.Parse(Request(text, "CSharp1"));
        AssertSame(full, reparsed);
    }

    [Fact]
    public void Opening_a_block_comment_falls_back()
    {
        string old = Generated(5);
        using var previous = CSharpParser.ParseIncremental(Request(old, "CSharp1"));
        int at = old.IndexOf("int total = 0;", StringComparison.Ordinal);
        var (text, changes) = Edit(old, (at, 0, "/*"));

        ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(text, new Version(1, 0), []), changes, out bool spliced);
        using var reparsed = CSharpParser.Reparse(previous, Request(text, "CSharp1"), changes);

        Assert.False(spliced);
        using var full = CSharpParser.Parse(Request(text, "CSharp1"));
        AssertSame(full, reparsed);
    }

    [Fact]
    public void Reparse_requires_an_incremental_previous_document_and_the_same_profile()
    {
        using var plain = CSharpParser.Parse(Request("class A {}", "CSharp1"));
        using var incremental = CSharpParser.ParseIncremental(Request("class A {}", "CSharp1"));

        Assert.Throws<ArgumentException>(() => CSharpParser.Reparse(plain, Request("class B {}", "CSharp1"), [new(6, 1, 1)]));
        Assert.Throws<ArgumentException>(() => CSharpParser.Reparse(incremental, Request("class B {}", "CSharp2"), [new(6, 1, 1)]));
    }

    [Theory]
    [InlineData("class A {}\rclass B {}\n#region R\n#endregion\n", 11, 0, "\n")]   // a lone CR becomes CR LF
    [InlineData("class A {}\rX\nclass B {}\n#region R\n#endregion\n", 11, 1, "")] // CR and LF become one pair
    [InlineData("class A {}\r\nclass B {}\n#region R\n#endregion\n", 11, 0, "x")]  // a CR LF pair is split
    public void Line_breaks_formed_or_split_at_the_edit_keep_directive_lines(string old, int start, int length, string replacement)
    {
        var (text, changes) = Edit(old, (start, length, replacement));
        var previous = ConditionalPreprocessor.Process(new(old, new Version(1, 0), []));

        var spliced = ConditionalPreprocessor.Reprocess(previous, new(text, new Version(1, 0), []), changes);

        var full = ConditionalPreprocessor.Process(new(text, new Version(1, 0), []));
        Assert.Equal(full.MaskedText, spliced.MaskedText);
        Assert.Equal(full.Directives, spliced.Directives);
        Assert.Equal(full.ExactLineMappings, spliced.ExactLineMappings);
        Assert.Equal(full.InactiveSpans, spliced.InactiveSpans);
    }

    static readonly string[] Fragments =
        ["x", "1", ";", " ", "\n", "\r", "\r\n", "(", ")", "{", "}", "/", "/*", "*/", "//", "\"", "'", "@\"", "\\", "#if X\n", "#endif\n",
         "#define Y\n", "int", "class", "=", "+", "<", ">", ".", "a<b>c", "x/y"];

    static IEnumerable<(string Name, string Profile, string Text)> Corpus()
    {
        yield return ("generated", "CSharp1", Generated(12));
        string root = Path.Combine(AppContext.BaseDirectory, "Corpus");
        foreach (string file in Directory.GetFiles(Path.Combine(root, "Mono2004"), "*.cs").Order(StringComparer.Ordinal))
            yield return (Path.GetFileName(file), "CSharp1", File.ReadAllText(file));
        foreach (string file in Directory.GetFiles(Path.Combine(root, "Mono2006"), "*.cs").Order(StringComparer.Ordinal))
            yield return (Path.GetFileName(file), "CSharp2", File.ReadAllText(file));
    }

    [Fact]
    public void Reparse_equals_full_parse_on_csharp_corpus()
    {
        var corpus = Corpus().ToArray();
        int steps = 0, spliced = 0, reusing = 0;
        for (int seed = 1; seed <= 100; seed++)
        {
            var random = new Random(seed);
            var (name, profile, text) = corpus[random.Next(corpus.Length)];
            var previous = CSharpParser.ParseIncremental(Request(text, profile));
            try
            {
                for (int step = 1, count = random.Next(1, 4); step <= count; step++)
                {
                    var edits = RandomEdits(random, text);
                    var (next, changes) = Edit(text, edits);
                    var pre = ConditionalPreprocessor.Reprocess(previous.Preprocessing, new(next, previous.Preprocessing.Version, []), changes, out bool splice);
                    ParseDocument reparsed;
                    try
                    {
                        reparsed = CSharpParser.Reparse(previous, Request(next, profile), changes);
                        using var full = CSharpParser.Parse(Request(next, profile));
                        AssertSame(full, reparsed);
                    }
                    catch (Exception e)
                    {
                        throw new Xunit.Sdk.XunitException(
                            $"{name} seed {seed} step {step}: edits {string.Join(", ", edits.Select(x => $"({x.Start}, {x.OldLength}, \"{x.New}\")"))}\n{e.Message}", e);
                    }
                    previous.Dispose();
                    previous = reparsed;
                    text = next;
                    steps++;
                    if (splice) spliced++;
                    if (reparsed.ReusedNodes > 0) reusing++;
                }
            }
            finally
            {
                previous.Dispose();
            }
        }
        Assert.True(spliced * 5 >= steps, $"only {spliced} of {steps} steps spliced preprocessing");
        Assert.True(reusing * 5 >= steps, $"only {reusing} of {steps} steps reused the tree");
    }

    static (int Start, int OldLength, string New)[] RandomEdits(Random random, string text)
    {
        var edits = new List<(int Start, int OldLength, string New)>();
        int count = random.Next(3) == 0 ? random.Next(2, 4) : 1, floor = 0;
        var candidates = Enumerable.Range(0, count).Select(_ => RandomEdit(random, text)).OrderBy(e => e.Start).ToList();
        foreach (var e in candidates)
        {
            if (e.Start < floor) continue;
            edits.Add(e);
            floor = e.Start + e.OldLength + 1;
        }
        return edits.ToArray();
    }

    static (int Start, int OldLength, string New) RandomEdit(Random random, string text)
    {
        int kind = random.Next(10);
        if (kind < 3 && Run(random, text, char.IsLetter) is { } word) return (word.Start, word.Length, "n" + random.Next(100));
        if (kind < 5 && Run(random, text, char.IsDigit) is { } number) return (number.Start, number.Length, random.Next(1000).ToString());
        if (kind < 6) return (random.Next(text.Length + 1), 0, " ");
        int p = random.Next(text.Length + 1);
        int length = Math.Min(random.Next(0, 6), text.Length - p);
        string replacement = random.Next(4) == 0 ? "" : Fragments[random.Next(Fragments.Length)];
        if (length == 0 && replacement.Length == 0) replacement = Fragments[random.Next(Fragments.Length)];
        return (p, length, replacement);
    }

    static (int Start, int Length)? Run(Random random, string text, Func<char, bool> part)
    {
        for (int tries = 0; tries < 20 && text.Length > 0; tries++)
        {
            int p = random.Next(text.Length);
            if (!part(text[p])) continue;
            int start = p, end = p;
            while (start > 0 && part(text[start - 1])) start--;
            while (end < text.Length && part(text[end])) end++;
            return (start, end - start);
        }
        return null;
    }
}
