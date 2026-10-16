namespace CSNitrogen.Preprocessing;
public sealed record PreprocessRequest(string Source, Version Version, IReadOnlyList<string> ConditionalSymbols);
