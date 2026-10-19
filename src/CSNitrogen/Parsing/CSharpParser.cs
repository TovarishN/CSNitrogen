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
            ["CSharp1"] = (new(() => CSharp1Composition.CreateBuilder().WithTrivia(&CSharpTrivia.SkipHistorical).Build()), CSharp1Composition.Start),
            ["CSharp2"] = (new(() => CSharp2Composition.CreateBuilder().WithTrivia(&CSharpTrivia.SkipUnicode4).Build()), CSharp2Composition.Start),
            ["CSharp3"] = (new(() => CSharp3Composition.CreateBuilder().WithTrivia(&CSharpTrivia.SkipUnicode4).Build()), CSharp3Composition.Start),
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
    private static readonly Lazy<(IReadOnlyList<string> Modules,bool Root)> HistoricalFactoryContract=new(()=>
    {
        var factory=Compositions["CSharp1"];var language=factory.Language.Value;
        using var parsed=language.Parse("",factory.Start);
        bool root=parsed.Success && !parsed.HasErrors && factory.Start.Name=="CompilationUnit"
            && language.GetKindName(parsed.Tree.Kind(parsed.Tree.Root))=="CompilationUnit"
            && language.Modules.Any(m=>m.Id==SyntaxKinds.ModuleOf(parsed.Tree.Kind(parsed.Tree.Root)) && m.Name=="CSharp.CompilationUnits");
        return (Array.AsReadOnly(language.Modules.Select(m=>m.Name).Order(StringComparer.Ordinal).ToArray()),root);
    });
    internal static bool MatchesHistoricalFactory(IReadOnlyList<string> modules)
        =>HistoricalFactoryContract.Value.Root && modules.Distinct(StringComparer.Ordinal).Count()==modules.Count
            && HistoricalFactoryContract.Value.Modules.SequenceEqual(modules.Order(StringComparer.Ordinal));
    private static readonly Lazy<(IReadOnlyList<string> Modules,bool Root)> SecondFactoryContract=new(()=>
    {
        var factory=Compositions["CSharp2"];var language=factory.Language.Value;
        using var parsed=language.Parse("",factory.Start);
        bool root=parsed.Success && !parsed.HasErrors && factory.Start.Name=="CompilationUnit"
            && language.GetKindName(parsed.Tree.Kind(parsed.Tree.Root))=="CompilationUnit"
            && language.Modules.Any(m=>m.Id==SyntaxKinds.ModuleOf(parsed.Tree.Kind(parsed.Tree.Root)) && m.Name=="CSharp.CompilationUnits");
        return (Array.AsReadOnly(language.Modules.Select(m=>m.Name).Order(StringComparer.Ordinal).ToArray()),root);
    });
    internal static bool MatchesSecondFactory(IReadOnlyList<string> modules)
        =>SecondFactoryContract.Value.Root && modules.Distinct(StringComparer.Ordinal).Count()==modules.Count
            && SecondFactoryContract.Value.Modules.SequenceEqual(modules.Order(StringComparer.Ordinal));
    private static readonly Lazy<(IReadOnlyList<string> Modules,bool Root)> ThirdFactoryContract=new(()=>
    {
        var factory=Compositions["CSharp3"];var language=factory.Language.Value;
        using var parsed=language.Parse("",factory.Start);
        bool root=parsed.Success && !parsed.HasErrors && factory.Start.Name=="CompilationUnit"
            && language.GetKindName(parsed.Tree.Kind(parsed.Tree.Root))=="CompilationUnit"
            && language.Modules.Any(m=>m.Id==SyntaxKinds.ModuleOf(parsed.Tree.Kind(parsed.Tree.Root)) && m.Name=="CSharp.CompilationUnits");
        return (Array.AsReadOnly(language.Modules.Select(m=>m.Name).Order(StringComparer.Ordinal).ToArray()),root);
    });
    internal static bool MatchesThirdFactory(IReadOnlyList<string> modules)
        =>ThirdFactoryContract.Value.Root && modules.Distinct(StringComparer.Ordinal).Count()==modules.Count
            && ThirdFactoryContract.Value.Modules.SequenceEqual(modules.Order(StringComparer.Ordinal));
    public static ParseDocument Parse(ParseRequest request)
    {
        var (profile,composition)=Resolve(request);
        var preprocessing=ConditionalPreprocessor.Process(new(request.Source,profile.Version!,request.ConditionalSymbols));
        // Parse the masked buffer; the tree keeps the authored source as its text
        // (same length), so tree text and trivia read what the user wrote.
        var parsed = composition.Language.Value.Parse(preprocessing.MaskedText, composition.Start, request.Source);
        return Document(request,profile,preprocessing,parsed);
    }

    /// <summary>
    /// Parses like <see cref="Parse"/> and keeps what <see cref="Reparse"/> needs to reuse this document after an edit.
    /// An editor parses once with this and then reparses; a one-shot parse should use <see cref="Parse"/>.
    /// </summary>
    public static ParseDocument ParseIncremental(ParseRequest request)
    {
        var (profile,composition)=Resolve(request);
        var preprocessing=ConditionalPreprocessor.Process(new(request.Source,profile.Version!,request.ConditionalSymbols));
        var parsed=composition.Language.Value.ParseIncremental(preprocessing.MaskedText,composition.Start,request.Source);
        return Document(request,profile,preprocessing,parsed);
    }

    /// <summary>
    /// Parses <paramref name="request"/>'s source, an edit of <paramref name="previous"/>'s by <paramref name="changes"/>
    /// (old-text coordinates, sorted, non-overlapping), reusing what the edit left untouched: preprocessing is spliced
    /// when the edit cannot change the directive structure, and the tree reuses every subtree whose parse examined no
    /// changed text. The result equals <see cref="Parse"/>'s, syntax policy issues included (they are recomputed), and
    /// is independent of <paramref name="previous"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="previous"/> is not incremental, or the profile differs.</exception>
    public static ParseDocument Reparse(ParseDocument previous,ParseRequest request,ReadOnlySpan<TextChange> changes)
    {
        ArgumentNullException.ThrowIfNull(previous);
        var (profile,composition)=Resolve(request);
        if(!previous.IsIncremental)throw new ArgumentException("The previous document is not incremental; parse it with ParseIncremental.",nameof(previous));
        if(previous.Profile!=request.Profile)throw new ArgumentException("The previous document was parsed with another profile.",nameof(request));
        var preprocessing=ConditionalPreprocessor.Reprocess(previous.Preprocessing,new(request.Source,profile.Version!,request.ConditionalSymbols),changes,out bool spliced);
        // A full preprocessing run may change the masked text beyond the edit (a directive's reach): reparse from the
        // exact difference of the masked texts instead.
        TextChange[] masked=spliced?changes.ToArray():[TextChange.Between(previous.MaskedText,preprocessing.MaskedText)];
        var parsed=composition.Language.Value.Reparse(previous.Parsed,preprocessing.MaskedText,masked,request.Source);
        return Document(request,profile,preprocessing,parsed);
    }

    static (LanguageProfile Profile,(Lazy<Language> Language,Rule Start) Composition) Resolve(ParseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Source);
        ArgumentNullException.ThrowIfNull(request.ConditionalSymbols);
        ArgumentException.ThrowIfNullOrEmpty(request.DocumentId);
        var profile=ProfileResolver.Resolve(request.Profile).Profile;
        if (profile is null || !Compositions.TryGetValue(request.Profile,out var composition))
            throw new ArgumentException("Unknown language profile.", nameof(request));
        return (profile,composition);
    }

    static ParseDocument Document(ParseRequest request,LanguageProfile profile,PreprocessResult preprocessing,ParseResult parsed)
    {
        try
        {
            var issues = new List<ParseIssue>();
            foreach (var d in parsed.Diagnostics)
                issues.Add(new("NG" + (ushort)d.Code, parsed.FormatMessage(d), d.Span, IssueCategory.Syntax, d.Severity));
            issues.AddRange(preprocessing.Issues);
            issues.AddRange(CSharpSyntaxPolicy.Check(parsed.Tree,profile.Version));
            return new ParseDocument(request, parsed, issues, preprocessing,profile.Complete);
        }
        catch { parsed.Dispose(); throw; }
    }
}
