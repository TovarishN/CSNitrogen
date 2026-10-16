# C# 1 Lexical and Preprocessing Baseline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans for the preserved native execution method. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Extend the foundation into a source-preserving C# 1 lexical layer and conditional preprocessor, with a runnable public parse path for conditional empty-class files.

**Architecture:** Nitrogen continues to parse all language syntax. A narrowly scoped preprocessing pass identifies directives using C# 1 lexical states, evaluates only directive boolean expressions, and masks inactive regions without changing UTF-16 offsets or newlines. Real `.ngr` lexical categories expose tokens for the future expression/declaration grammars; no C# compiler semantics are added.

**Tech Stack:** Existing .NET 10 solution, pinned Nitrogen snapshot, xUnit suites, and Roslyn 5.0.0 oracle.

**Spec:** [Approved C# 1–14 design](../specs/2026-10-16-csharp-versioned-grammars-design.md).

## Global Constraints

- Roslyn is a test oracle, not the production parser or a replacement for Nitrogen grammars.
- No sibling repository changes are included.
- Support regular C# source files first; script mode is excluded.
- Preserve original text and UTF-16 source coordinates.
- Do not evaluate arbitrary C# expressions as directive conditions.
- The initial product does not implement C# name binding, type inference, overload resolution, semantic validity, lowering, code generation, or execution.
- Repository initialization and commits are deferred until requested.

All profiles remain partial after this milestone. C# 11 raw strings and C# 6 interpolation are outside C# 1 scope, so the unresolved arbitrary-delimiter gate does not block this baseline. It must be settled before those features are advertised. This is a continuation plan ready for review, not approval of a Nitrogen core extension.

## Review Focus

1. An inactive branch containing malformed C# quotes must still find its conditional directives: Task 3.
2. A directive-looking line inside an active verbatim string or block comment must remain content: Task 2.
3. Every masked source character retains its original UTF-16 offset, including CRLF and supplementary text: Task 3.
4. A directive expression containing invocation/member-access syntax must fail before branch activation: Task 3.
5. Version metadata must not promote later-version lexical syntax or directives into C# 1: Tasks 1/4.

## Interfaces and files

- `src/CSNitrogen.Syntax/Grammars/Lexical.ngr`: C# 1 identifier, number, character, regular/verbatim-string token definitions.
- `src/CSNitrogen/Preprocessing/PreprocessRequest.cs`: original source, immutable conditional-symbol input, profile version.
- `PreprocessResult.cs`: original source, equal-length masked source, immutable directive records, inactive spans, physical/logical line mappings, diagnostics.
- `DirectiveScanner.cs`: directive boundaries; lexical states in active code and special disabled-text scanning.
- `ConditionalExpression.cs`: only `!`, `&&`, `||`, `==`, `!=`, identifiers, `true`, `false`, and parentheses, with explicit precedence.
- `ConditionalPreprocessor.cs`: symbol mutations, conditional stack, masking, directive metadata.
- `DirectiveEffects.cs`: regions, diagnostic directives, version-aware directive handling, location mappings.
- `tests/CSNitrogen.Tests/Lexing/` and `Preprocessing/`: focused source/diagnostic/span tests.
- `tests/CSNitrogen.Oracle.Tests/PreprocessingParityTests.cs`: token/directive/active-class observations under explicit versions.

Public preprocessing contract:

```csharp
public sealed record PreprocessRequest(string Source, Version Version,
    IReadOnlyList<string> ConditionalSymbols);
public static class ConditionalPreprocessor
{
    public static PreprocessResult Process(PreprocessRequest request);
}
// Result exposes OriginalText, MaskedText, Directives, InactiveSpans, Issues,
// line mappings, and HasErrors. No parse-tree ownership is transferred here.
```

Directive records retain the physical UTF-16 span, directive kind, active state, and authored argument text. Location mappings are metadata; physical offsets never change.

## Task 1: Real C# 1 lexical categories and reviewed inventory slice

**Create:** `Lexical.ngr`, `tests/CSNitrogen.Tests/Lexing/LexicalTests.cs`. **Modify:** core identifier reference, registered grammar membership, and corresponding baseline inventory entries.

- [x] Review the identifier/literal/trivia sections of the language specification and historical version requirements. Replace automatic classifications only for this milestone's entries, retaining reference URLs and documented differences. Leave unrelated inventory candidates unreviewed.
- [x] Write lexical-fragment tests with positive and negative cases below, then run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~LexicalTests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false`. Observe missing categories or incorrect acceptance before implementing.

```text
Identifiers: C, _x, @class, Greek-letter names, valid Unicode escapes.
Reject: empty, a leading digit, incomplete Unicode escapes, reserved unescaped names.
Integers: 0, 42, 0x2A, applicable U/L suffix combinations.
Reject C# 1: 0b10, 1_000; reject missing hexadecimal digits.
Reals: .5, 1.5, 1e2, 1f, 1m; reject malformed exponents/suffixes.
Characters: 'x', '\n', '\u0041'; reject empty/multiple characters and malformed escapes.
Strings: "", "a\"b", @"a""b"; reject unterminated forms.
Reject C# 1: interpolated/raw/UTF-8 strings and the later \e escape.
```

- [x] Use actual `.ngr` tokens. The existing probe gives the regular/verbatim token mechanism:

```text
token RegularString = "\"" ([^ '"' '\\' '\r' '\n'] / "\\" Escape)* "\"";
token VerbatimString = "@\"" ("\"\"" / !"\"" .)* "\"";
```

`Escape` must enumerate the C# 1 simple/hex/Unicode escape forms instead of accepting arbitrary characters. Verify Unicode categories and escape-value constraints against the engine before selecting a matcher; if the grammar requires an unproven capability, retain a minimal reproduction instead of admitting arbitrary characters. Escaped identifier spelling and decoded identity are separate observations; no binding is added.

- [x] Expose lexical fragments through generated test compositions. Assert authored spans/text and malformed-source diagnostics. Do not substitute a regular-expression tokenizer or Roslyn for the `.ngr` categories.
- [x] Rewire the production class-name token after the lexical tests pass. Compare class names under matching explicit Roslyn versions. Run both suites; mark only demonstrated lexical ledger entries verified.

## Task 2: Directive scanning with active-code lexical states

**Create:** request/result/record contracts and `DirectiveScanner.cs`; `DirectiveScannerTests.cs`.

- [x] Write scanner fixtures and run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~DirectiveScannerTests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` before implementation:

```text
#if X on a line with legal preceding whitespace -> directive.
class C {} #if X -> placement error, not a valid directive.
Active @"multiline\n#if X\n#endif" -> neither line is a directive.
Active /*\n#if X\n#endif\n*/ -> neither line is a directive.
Regular-string/char escaped quotes -> do not prematurely leave string state.
CR, LF, CRLF, NEL, LS, PS -> preserve offsets and identify line starts.
```

- [x] Implement a single pass that records directive spans while maintaining active C# 1 string/character/comment states. Do not parse declarations or expressions. Use the proven newline definition in `CSharpTrivia`; avoid creating two conflicting definitions.
- [x] Return malformed lexical-state diagnostics with physical spans. Add a scanner mode for inactive text that ignores broken C# lexical states but still locates conditional-directive lines; Task 3 controls transitions into/out of that mode.
- [x] Test source beginning with a BOM and source containing supplementary characters. Assert all recorded ranges index the original string exactly. Run the scanner tests and the normal suite.

## Task 3: Conditional expressions, branch state, and source masking

**Create:** `ConditionalExpression.cs`, `ConditionalPreprocessor.cs`, and their focused tests.

- [x] Start with branch and expression tests. Verify RED before implementing:

```csharp
// Input: "#if X\nclass A {}\n#else\nclass B {}\n#endif"
// Symbols X: active class A; no symbols: active class B.
// For both: MaskedText.Length == OriginalText.Length;
// every CR/LF/NEL/LS/PS character retains its exact index and value.
```

Include nested branches, `#elif`, duplicate `#else`, unmatched/unterminated conditionals, `#define`/`#undef`, and forbidden definitions after source tokens. Retain the exact failing source as fixture data.

- [x] Implement directive boolean parsing with precedence `!`, equality, `&&`, then `||`. Restrict tokens to the contract above. Tests must reject `F()`, `a.b`, arithmetic, and incomplete expressions with useful spans; no C# execution or reflection is permitted.
- [x] Maintain conditional stack entries containing parent activity, whether a branch already matched, current activity, and whether `#else` occurred. Definitions mutate only a private symbol set when allowed; caller input remains unchanged.
- [x] Mask inactive text and recognized directive lines with spaces, retaining line terminators and equal UTF-16 length. Preserve inactive spans and authored directive records. Never mask based on an invalid condition without retaining an error that prevents acceptance.
- [x] Add a disabled branch containing unterminated quotes before `#endif`; it must recover the conditional structure. Add directive-looking active verbatim-string content; it must not affect the stack. Run the preprocessor tests and whole normal suite.

## Task 4: Directive effects and version-aware diagnostics

**Create:** `DirectiveEffects.cs` and `DirectiveEffectsTests.cs`.

- [x] Test `#region`/`#endregion` pairing, active/inactive `#error`/`#warning`, `#line` default/hidden/number+file forms, unknown directives, and later-version directives under C# 1. Observe failing expectations before implementing.
- [x] Preserve warnings separately from errors. An inactive `#error` does not make an otherwise valid file fail; an active `#error` does. Invalid directive syntax always retains an authored span.
- [x] Store `#line` mappings without rewriting source coordinates. Compare physical and mapped line/file observations against the oracle. Reject later-version forms or report the precisely identified unavailable feature; do not silently accept them as comments.
- [x] Test the C# 1 directive inventory against authoritative historical requirements; add any discovered C# 1 directive to this task's fixtures and effects. Do not claim that current Roslyn's newest directive set is the historical baseline.
- [x] Run focused effects tests and both suites. Update only the covered inventory entries.

## Task 5: Public pipeline integration and milestone acceptance

**Modify:** `CSharpParser`, `ParseDocument`, normal/oracle parity tests, README, dependency/coverage identity inputs, evidence report.

- [x] Add public API tests for the conditional empty-class fixture, original versus masked text, inactive spans, active diagnostic directives, and line mappings. Unknown profile/null input validation must still happen before tree allocation. Verify the current unsupported-preprocessing behavior fails these new expectations.
- [x] Invoke preprocessing before `Language.Parse`, using the selected profile's explicit version. Parse the masked source, merge diagnostic lists, and expose the original source and directive metadata alongside the owning tree. Since masking preserves offsets, spans still index the original text. Explain that `Tree.Text` is the masked coordinate buffer; use `OriginalText` for authored slices.
- [x] Remove only the now-obsolete blanket preprocessing rejection and comment-only guard. Keep disposal, full-consumption, recovered-tree rejection, and all existing class/trivia regressions passing.
- [x] Compare active class names/spans, directive counts, physical/mapped locations, malformed conditions, and inactive-branch behavior with Roslyn under C# 1. Classify oracle semantic diagnostics separately. Preserve every discrepancy with source and expected/actual observations.
- [x] Run the full solution build and both suites with the existing flags; include the 30-second hang timeout for normal tests. Audit production dependencies for Roslyn leakage and sibling source hashes/state for isolation.
- [x] Update the coverage identity to include every production `.ngr` input and preprocessing implementation identity, rather than only the old core grammar. Prove a changed grammar input changes the identity while input enumeration order does not.
- [x] Document this milestone's accepted inputs and remaining unsupported syntax. All version profiles remain partial. Prepare the next plan for C# 1 types/declarations/statements/expressions using the established lexical categories and source mapping.

## Execution status (2026-10-16)

Implemented the planned lexical categories, scanner, conditional preprocessing, directive effects, public integration, source identity, and reviewed ledger slice. Full C# source grammars remain partial. Review findings and final verification are in the [evidence report](../../coverage/csharp1-lexical-preprocessing-evidence.md); the [next plan](2026-10-16-csharp1-source-grammar.md) is ready for review.

## Handoff

Native execution remains the selected method. This plan requires review before implementation. No full C# 1 syntax completeness, later feature support, or Nitrogen core changes are implied by approving it.
