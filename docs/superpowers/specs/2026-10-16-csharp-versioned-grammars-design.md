# C# 1–14 grammars and customizable syntax with Nitrogen

Date: 2026-10-16
Status: approved by the user on 2026-10-16; implementation planning in progress.

## Intent and agreed scope

Build real Nitrogen `.ngr` grammars for C# major versions 1 through 14 in CSNitrogen, then use the resulting grammar modules to investigate customizable languages. The user selected the recommendation to start with parsing and generated typed syntax trees, version diagnostics, feature modules, and version profiles.

The initial product does not implement C# name binding, type inference, overload resolution, semantic validity, lowering, code generation, or execution. Generated typed syntax views describe syntax categories; they are not evidence of a semantically typed C# program. Roslyn is a test oracle, not the production parser or a replacement for Nitrogen grammars.

Success means each advertised profile has explicit coverage, source-located diagnostics, and repeatable acceptance tests. A profile is not complete because its configuration exists or one example parses.

## Current evidence and workspace boundary

CSNitrogen was empty and was not a Git repository when inspected. The sibling `/Users/dmitrikamenetski/work/Nitrogen` was inspected read-only. It has `.ngr` token and syntax rules, extensible syntax, cross-module extensions, precedence annotations, and generated language composition factories. Its composition tests demonstrate separate memberships over shared grammars and rejection of shared grammars configured with conflicting namespaces.

Use one shared generated syntax namespace for the C# family. Select grammar inputs explicitly through `nitrogen.json`; register the configuration and all selected grammar files as generator `AdditionalFiles`. Use generated composition factories rather than hand-maintained lists of runtime syntax modules.

No sibling repository changes are included. If a required Nitrogen capability is missing, isolate a minimal reproduction and report the blocker; a Nitrogen core change needs separate scope and review. Development may reference the local Nitrogen Runtime and Generator projects read-only, recording the dependency revision and dirty state. Release packaging is a later decision.

## Architecture and alternatives

Separate complete grammars per version provide isolation but duplicate syntax and fixes. A chronological extension chain preserves release history but makes individual features difficult to select. The selected architecture uses shared syntax categories, independently testable feature modules, and explicit version profiles. Release order is metadata; it is not the only composition mechanism.

The parser path is:

1. Resolve a version or custom profile into an explicit module set and compatibility policies.
2. Validate feature requirements and known conflicts before building the language.
3. Apply source preprocessing for the chosen conditional symbols, preserving source offsets.
4. Parse with the composed Nitrogen language and its compilation-unit start rule.
5. Inspect the syntax for profile restrictions that cannot be expressed by module membership alone.
6. Return the Nitrogen tree, typed syntax views, diagnostics, and coverage identity.

Runtime profile selection chooses a prebuilt composition. Arbitrary new grammar source requires generation/build; this design does not promise runtime loading of user-authored `.ngr` files.

## Modules and extension boundaries

Shared categories include names, types, expressions, statements, attributes, parameters, members, type declarations, namespaces, and compilation units. Their extension points must be deliberate: an extension to expressions must not automatically become valid in a constant-expression or statement-expression position.

Feature modules add cohesive syntax capabilities, for example generics, anonymous functions, query expressions, pattern families, records, and collection expressions. Split a feature further when version boundaries or independent selection require it. Avoid one file per release containing unrelated changes.

Features declare a stable identity, introduction version, required features, grammar files, compatibility-policy contributions, source references, and positive/negative fixtures. These are CSNitrogen metadata, not claimed Nitrogen built-ins. Selection must be deterministic and independent of input enumeration order. Missing requirements are errors with an explanation; do not silently add disabled features.

Dependencies are syntax dependencies. Query syntax must not require LINQ library binding, and an async grammar must not claim that an awaitable type exists. Features that change only C# semantics are listed as outside parser scope rather than represented by fictitious productions.

Prefer module omission for removable syntax. When versions share a production but restrict its context, use a source-located compatibility check over the parsed syntax. Additive extensions cannot remove an existing production: where independent removal is necessary, split that production into a selectable module or introduce an explicit policy. A profile must disclose whether a restriction is structural or checked after parsing.

## Version profiles and coverage

Provide fourteen named profiles, CSharp1 through CSharp14, corresponding to the numbered major releases. Model historical minor boundaries explicitly, including 7.1, 7.2, and 7.3, as additional selectable profiles; a CSharp7 profile means 7.0, not the union of 7.x. Record other historical release distinctions in the feature inventory and add a separate profile when they change relevant syntax. Never use an unpinned `latest` or `preview` policy for these profiles.

Version inventories come from the C# specification plus version-specific feature specifications. The consolidated grammar alone is not sufficient for C# 14 or historical boundaries. Stable features only; previews are excluded unless separately named and tested.

Build a feature ledger before claiming comprehensive support. Each entry records the precise version, syntax versus semantic classification, module/policy mapping, authoritative reference, fixtures, and status. Statuses are unimplemented, implemented, verified, or blocked. This ledger is the release coverage contract, including lexical and preprocessing coverage.

Each profile records its exact module set, policy set, dependency revision, and supported feature inventory. Later releases can alter interpretation or restrictions, so older positive fixtures must be checked against later profiles rather than assuming unconditional acceptance.

## Lexing, preprocessing, and ambiguities

Preserve original text and UTF-16 source coordinates. Diagnostics must refer to the user's source, including when a preprocessing view masks inactive regions. Treat trivia preservation and syntax spans as separate requirements; do not claim a lossless printer merely because spans exist.

Support regular C# source files first; script mode is excluded. Preprocessing is part of syntax scope: conditional directives, symbol definitions, nesting, inactive branches, and directive placement require explicit tests. Preserve directive records, diagnostics, and inactive text. Mask inactive regions with equal-length text while retaining newlines; retain source mapping for any other transformation. Do not evaluate arbitrary C# expressions as directive conditions. Handle diagnostic and location directives explicitly; physical offsets remain available alongside mapped locations.

Test Unicode and escaped identifiers, reserved/contextual keywords, numeric literals and suffixes, character/string escapes, comments, interpolation nesting, verbatim strings, raw delimiter lengths, and interpolated raw strings. A regex that consumes an entire interpolated string without exposing embedded expressions is not full typed syntax coverage.

Early feasibility tests must establish how Nitrogen can express variable-length raw delimiters, nested interpolation, and preprocessing-aware token boundaries. Do not assume a custom token/scanner API exists. If grammar expressions cannot express a necessary case, document the exact limitation before choosing an adapter; an adapter must not delegate C# parsing to Roslyn or replace `.ngr` productions with a handwritten parser.

Ambiguity fixtures include casts versus parenthesized expressions, generics versus relational operators, nested generic closers versus shifts, declaration versus expression statements, dangling else, lambda forms, query contextual keywords, and patterns versus expressions. Select syntax using C# syntactic disambiguation rules where available. When meaning requires binding, preserve/report the ambiguity rather than claiming semantic resolution. Nitrogen's default first reading is not automatically accepted as C# conformance.

## Public result and diagnostics

A parse request supplies source text, a named profile, a document identity, and conditional symbols. A custom profile supplies an explicit feature set and policy choices. The result exposes the selected profile, source text, owning Nitrogen parse result/tree, typed root access, directive metadata, and diagnostics. The wrapper must preserve Nitrogen tree lifetime/disposal rules; borrowed typed nodes cannot outlive their tree.

Distinguish composition errors, preprocessing errors, lexical/syntax errors, unavailable-feature diagnostics, and unsupported-coverage diagnostics. A recovered or partially supported tree is useful for inspection but is not an accepted complete parse. Acceptance requires full input consumption, no errors, and no unsupported construct in the parsed document. Expose whether the overall profile is partial even when the specific input is supported.

An unavailable feature that can be identified gets its feature name, minimum version or required module, and authored span. Otherwise report the syntax failure honestly. Do not load every disabled module merely to improve error text. Exact Roslyn diagnostic codes and wording are not required.

## Verification strategy

Pin the Roslyn test dependency to a release supporting explicit C# 14 and all required historical language-version options. Production grammar projects must not depend on Microsoft.CodeAnalysis.CSharp. Record the oracle version with test results.

Compare full consumption, syntax classification/structure at declared observation points, relevant spans, and acceptance or rejection; generated node identities do not need to match Roslyn's. Run positive and malformed examples under matching explicit language versions. Roslyn can parse newer syntax under an older language version and report its version error during compilation, so syntax-tree diagnostics alone are not a complete version oracle. Where necessary, use controlled compilations and classify feature-availability diagnostics separately from unrelated semantic or missing-reference errors.

Every implemented feature needs positive syntax fixtures, malformed syntax fixtures, boundary-version fixtures, and interactions with shared grammar where applicable. Include contextual words used as identifiers. Semantic-only differences are excluded from parser parity and recorded in the ledger.

Test profile composition, missing requirements, known conflicts, deterministic ordering, and disabled-feature leakage. Run the same source under paired profiles to prove membership matters. Add resource-bounded malformed/deep input tests and report performance measurements without promising a target before a baseline exists.

Select and pin representative external source corpora, preserving provenance and licensing. A curated corpus is regression evidence, not proof that every legal C# program is accepted. Completeness is judged against the feature ledger and targeted cases as well as corpus results.

## Delivery sequence and completion gates

1. Establish the feature ledger, pinned Nitrogen/Roslyn dependencies, test harness, and minimal real `.ngr` compilation-unit slice with typed syntax assertions. Resolve lexical and ambiguity feasibility risks before investing in all profiles.
2. Complete the C# 1 syntax baseline, including preprocessing, unsafe syntax, declarations, statements, expressions, literals, and diagnostics. Partial stages remain labeled partial.
3. Add feature modules in release groups: C# 2–6; C# 7 and minor releases through 8; C# 9–11; C# 12–14. Each group must pass its feature, boundary, composition, and regression checks before being declared verified.
4. Demonstrate customization with at least two profiles over the verified modules: one that excludes query syntax while retaining lambdas, and another that excludes unsafe syntax. Show accepted and rejected examples and requirement failures. Any third-party syntax extension begins as a separately reviewed experiment.
5. Review the evidence and propose the next design for general customizable languages, including semantic composition if desired. Do not build a general plugin platform before these examples establish the need.

All fourteen major profiles are complete only when their applicable ledger entries are verified, their configured compositions and generated views build, and their tests pass with documented oracle discrepancies. A lexical blocker or unimplemented C# 14 feature keeps the affected profile partial.

## Proposed repository layout

Use `src/CSNitrogen.Syntax` for `.ngr` modules, `nitrogen.json`, feature metadata, and generated views; `src/CSNitrogen` for profile selection, preprocessing, compatibility diagnostics, and the owning parse API; `tests/CSNitrogen.Tests` for composition and parser tests; `tests/CSNitrogen.Oracle.Tests` for Roslyn comparisons; and `docs/coverage` for the feature ledger and reports. Add a small sample runner only to exercise profile selection and inspect results. The solution targets a framework compatible with the pinned Nitrogen dependency.

This layout is a design proposal, not scaffolded implementation. Repository initialization and commits are deferred until requested; the current deliverable is this reviewable document.

## Sources and remaining engineering uncertainty

- [C# language grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/grammar)
- [C# release history](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history)
- [C# 14 features](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [C# feature specifications](https://github.com/dotnet/csharplang/tree/main/proposals)
- Local Nitrogen evidence: `Nitrogen.Tests/Grammars/Base.ngr`, `Lexical.ngr`, `Power.ngr`, `Nitrogen.Tests/Grammar/CompositionGeneratorTests.cs`, `Nitrogen.Grammar/CompositionConfiguration.cs`, and README composition guidance.

Variable-length lexing, interpolation nesting, preprocessing interactions, and C# disambiguation have not yet been demonstrated in this consumer. These are first-stage implementation gates. No parser, profile, or test result is claimed by this document.
