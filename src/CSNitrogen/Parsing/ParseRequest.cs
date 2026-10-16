namespace CSNitrogen.Parsing;
public sealed record ParseRequest(string Source, string Profile, string DocumentId, IReadOnlyList<string> ConditionalSymbols);
