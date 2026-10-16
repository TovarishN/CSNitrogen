# Lexical capability evidence

Historical foundation/probe evidence. For the current implementation, see [C# 1 lexical/preprocessing evidence](csharp1-lexical-preprocessing-evidence.md).

The test-only `tests/CSNitrogen.Tests/Grammars/LexicalProbe.ngr` uses actual Nitrogen productions. `LexicalCapabilityTests` checks regular and verbatim strings, escaped quotes, nested interpolated expressions, raw delimiters of lengths 3/4/8, and raw interpolation with 1/2/3 dollar signs and a three-quote delimiter. Expression nodes and text spans are inspected; interpolation is not an opaque string token.

Default trivia cannot safely skip whitespace inside a string body. The probe uses `LanguageBuilder.WithTrivia` with no automatic trivia and explicit optional whitespace tokens in embedded expressions. This proves the tested forms while retaining whitespace; it does not establish a complete production lexing strategy. Nitrogen rejects zero-length tokens, so the whitespace token is nonempty and its references are optional.

## Unresolved raw delimiter gate

Minimal retained counterexample:

```csharp
class C { string S = """""a"""""; }
```

Roslyn 5.0.0 accepts the string syntax under C# 14. The finite Nitrogen probe rejects the corresponding string fragment because delimiter length 5 is not among its alternatives. A passing test that asserts this limitation is not proof of C# 11+ support.

The inspected `GrammarModel.cs` exposes sequences, choices, repetition, literals, character classes, rule references, and lookahead. `TokenCodeWriter.cs` emits static token matchers; no capture-dependent delimiter parameter or `.ngr` external matcher declaration was found. Runtime `Rule` supports a function pointer, but that alone does not establish a generated token-extension contract. No sibling capability change was attempted. This is an unresolved implementation path, not proof that Nitrogen cannot express the language.

A follow-up should compare: a grammar construction that handles arbitrary delimiter equality and correct token boundaries; a narrow explicit token-matcher extension to Nitrogen; or a source-preserving lexical adapter with typed interpolation structure. It must test adjacent literals, nested interpolation, delimiter mismatches, and source spans, and must not delegate production parsing to Roslyn.

## Preprocessing boundaries

`LexicalBoundaryTests` records Roslyn expectations for directive-looking lines inside verbatim strings, raw strings, and comments, and malformed quotes inside disabled conditional branches. It also checks BOM, LF/CRLF, and UTF-16 spans following supplementary Unicode text. These are reference acceptance requirements, not an implemented conditional preprocessor.

The production foundation rejects conditional symbols and outside-comment hash text as unsupported preprocessing. It handles comment boundaries and reports unterminated block comments locally because Nitrogen's generic trivia intentionally skips such comments to EOF. The foundation has no supported string-containing class members yet; its lexical guard is intentionally limited to the supported class/comment surface and must be replaced/extended before adding strings to production.
