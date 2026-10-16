using Nitrogen;
namespace CSNitrogen.Parsing;
public enum IssueCategory { Composition, Preprocessing, Syntax, UnavailableFeature, UnsupportedCoverage }
public sealed record ParseIssue(string Code, string Message, TextSpan Span, IssueCategory Category, DiagnosticSeverity Severity = DiagnosticSeverity.Error);
