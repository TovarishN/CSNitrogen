using CSNitrogen.Syntax;
using CSNitrogen.Profiles;
using Nitrogen;
using CSNitrogen.Preprocessing;
namespace CSNitrogen.Parsing;
public static unsafe class CSharpParser
{
    private static readonly IReadOnlyDictionary<string, (Lazy<Language> Language, Rule Start)> Compositions =
        new Dictionary<string, (Lazy<Language>, Rule)>(StringComparer.Ordinal)
        {
            ["CSharp1"] = (new(() => CSharp1Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp1Composition.Start),
            ["CSharp2"] = (new(() => CSharp2Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp2Composition.Start),
            ["CSharp3"] = (new(() => CSharp3Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp3Composition.Start),
            ["CSharp4"] = (new(() => CSharp4Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp4Composition.Start),
            ["CSharp5"] = (new(() => CSharp5Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp5Composition.Start),
            ["CSharp6"] = (new(() => CSharp6Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp6Composition.Start),
            ["CSharp7"] = (new(() => CSharp7Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp7Composition.Start),
            ["CSharp8"] = (new(() => CSharp8Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp8Composition.Start),
            ["CSharp9"] = (new(() => CSharp9Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp9Composition.Start),
            ["CSharp10"] = (new(() => CSharp10Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp10Composition.Start),
            ["CSharp11"] = (new(() => CSharp11Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp11Composition.Start),
            ["CSharp12"] = (new(() => CSharp12Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp12Composition.Start),
            ["CSharp13"] = (new(() => CSharp13Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp13Composition.Start),
            ["CSharp14"] = (new(() => CSharp14Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp14Composition.Start),
            ["CSharp7_1"] = (new(() => CSharp7_1Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp7_1Composition.Start),
            ["CSharp7_2"] = (new(() => CSharp7_2Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp7_2Composition.Start),
            ["CSharp7_3"] = (new(() => CSharp7_3Composition.CreateBuilder().WithTrivia(&CSharpTrivia.Skip).Build()), CSharp7_3Composition.Start),
        };
    public static ParseDocument Parse(ParseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Source);
        ArgumentNullException.ThrowIfNull(request.ConditionalSymbols);
        ArgumentException.ThrowIfNullOrEmpty(request.DocumentId);
        var profile=ProfileResolver.Resolve(request.Profile).Profile;
        if (profile is null || !Compositions.TryGetValue(request.Profile,out var composition))
            throw new ArgumentException("Unknown language profile.", nameof(request));
        var preprocessing=ConditionalPreprocessor.Process(new(request.Source,profile.Version!,request.ConditionalSymbols));
        var parsed = composition.Language.Value.Parse(preprocessing.MaskedText, composition.Start);
        try
        {
            var issues = new List<ParseIssue>();
            foreach (var d in parsed.Diagnostics)
                issues.Add(new("NG" + (ushort)d.Code, parsed.FormatMessage(d), d.Span, IssueCategory.Syntax, d.Severity));
            issues.AddRange(preprocessing.Issues);
            issues.AddRange(CSharpSyntaxPolicy.Check(parsed.Tree,profile.Version));
            return new ParseDocument(request, parsed, issues, preprocessing);
        }
        catch { parsed.Dispose(); throw; }
    }
}
