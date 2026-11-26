using CSNitrogen.Parsing;
using CSNitrogen.Profiles;
using Nitrogen;

namespace CSNitrogen.Tests.Lexing;

public sealed class CSharp7UnicodeContractTests
{
    [Theory]
    [InlineData("CSharp7")]
    [InlineData("CSharp7_1")]
    [InlineData("CSharp7_2")]
    [InlineData("CSharp7_3")]
    public void Seventh_profiles_carry_the_sixth_unicode_policy_through_the_public_parser(string profile)
    {
        var resolved = ProfileResolver.Resolve(profile).Profile!;
        Assert.Contains("unicode-4.0-scalar", resolved.Policies);
        Assert.DoesNotContain("unicode-current-bmp", resolved.Policies);
        Assert.Contains("Grammars/GlobalTargetsThird.ngr", resolved.Grammars);

        (string Source, bool Accepted)[] cases =
        [
            ("class \U00010400{}", true),
            ("class \\U00010400{}", true),
            ("class \u0860{}", false),
            ("#define \U00010400\n#if \U00010400\nclass Selected{}\n#endif", true),
            ("class\u200bC{}", true),
            ("[as\u0604sembly:A]class C{}", false),
            ("[as\u00adsembly:A]class C{}", true),
        ];

        foreach (var (source, accepted) in cases)
        {
            using var sixth = CSharpParser.Parse(new(source, "CSharp6", "unicode.cs", []));
            using var seventh = CSharpParser.Parse(new(source, profile, "unicode.cs", []));
            Assert.True(accepted == seventh.Accepted, source);
            // Attribute target warnings belong to the version's source policy.
            Assert.Equal(sixth.Issues.Where(issue => issue.Severity == DiagnosticSeverity.Error)
                    .Select(issue => (issue.Code, issue.Span)),
                seventh.Issues.Where(issue => issue.Severity == DiagnosticSeverity.Error)
                    .Select(issue => (issue.Code, issue.Span)));
            string seventhTree = SyntaxDumper.Dump(seventh.Tree);
            Assert.Equal(SyntaxDumper.Dump(sixth.Tree), seventhTree);
            if (source.StartsWith("#define", StringComparison.Ordinal))
                Assert.Contains("Selected", seventhTree, StringComparison.Ordinal);
        }
        Assert.False(resolved.Complete);
    }

    [Fact]
    public void Eighth_profile_keeps_its_current_bmp_behavior()
    {
        using var supplementary = CSharpParser.Parse(new("class \U00010400{}", "CSharp8", "unicode.cs", []));
        using var currentLetter = CSharpParser.Parse(new("class \u0860{}", "CSharp8", "unicode.cs", []));
        Assert.False(supplementary.Accepted);
        Assert.True(currentLetter.Accepted);
    }
}
