namespace CSNitrogen.Profiles;
public sealed record FeatureDefinition(string Id, IReadOnlyList<string> Requires, IReadOnlyList<string> Conflicts, IReadOnlyList<string> Grammars);
public sealed record LanguageProfile(string Name, Version? Version, IReadOnlyList<string> Features,
    IReadOnlyList<string> Grammars, IReadOnlyList<string> Policies, bool Complete)
{
    public IReadOnlyList<string> RequestedFeatures { get; init; } = Features;
    public IReadOnlyList<string> MissingFeatures { get; init; } = Array.Empty<string>();
    public string CoverageIdentity { get; init; } = FeatureCatalog.CoverageIdentity;
}
public sealed record ProfileIssue(string Code, string Message);
public sealed record ProfileResolution(LanguageProfile? Profile, IReadOnlyList<ProfileIssue> Issues);
