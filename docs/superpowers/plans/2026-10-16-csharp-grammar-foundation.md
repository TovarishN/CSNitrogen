# C# Grammar Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans for native execution, or superpowers:subagent-driven-development if the user selects delegation. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a real Nitrogen compilation-unit slice, explicit partial version profiles, a reproducible Roslyn oracle, and executable evidence about the lexical and ambiguity risks before expanding to full C# 1–14 coverage.

**Architecture:** Shared `.ngr` modules generate syntax views into `CSNitrogen.Syntax`. Explicit compositions select modules; the consumer API owns parse-result lifetime and distinguishes successful parsing of a supported input from completeness of a profile. Roslyn is confined to oracle tests.

**Tech Stack:** .NET 10 target, Nitrogen Runtime/Generator from the sibling checkout, xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.12.0, xunit.runner.visualstudio 2.8.2, Microsoft.CodeAnalysis.CSharp 5.0.0 in oracle tests only.

**Spec:** [Approved design](../specs/2026-10-16-csharp-versioned-grammars-design.md).

## Global Constraints

- Build real Nitrogen `.ngr` grammars for C# major versions 1 through 14 in CSNitrogen, then use the resulting grammar modules to investigate customizable languages.
- The initial product does not implement C# name binding, type inference, overload resolution, semantic validity, lowering, code generation, or execution.
- Roslyn is a test oracle, not the production parser or a replacement for Nitrogen grammars.
- Use one shared generated syntax namespace for the C# family.
- No sibling repository changes are included.
- Never use an unpinned `latest` or `preview` policy for these profiles.
- Support regular C# source files first; script mode is excluded.
- A recovered or partially supported tree is useful for inspection but is not an accepted complete parse.
- Repository initialization and commits are deferred until requested.

The complete approved scope remains C# 1–14 and customization. This plan implements delivery stage 1 only. Subsequent implementation plans use its measured results; they must not invent a raw-string scanner API or assume C# ambiguity is already solved. This milestone does not advertise any complete version.

## Review Focus

1. Trailing nontrivia after an otherwise valid class must fail acceptance: Task 1.
2. Disposing the owning result must invalidate borrowed tree access: Task 2.
3. Duplicate/unknown feature IDs and reordered selections must produce deterministic outcomes: Task 3.
4. Directive-looking text inside multiline strings/comments must not control conditional compilation: Task 5.
5. An oracle that parses newer syntax under an older version must still detect feature unavailability: Task 4.

## File responsibilities

- `Directory.Build.props`: target framework, nullable, implicit usings, stable C# host version 14, and configurable `NitrogenRoot` defaulting to the sibling checkout.
- `CSNitrogen.slnx`: syntax, consumer, normal-test, and oracle-test projects.
- `src/CSNitrogen.Syntax/CSNitrogen.Syntax.csproj`: Runtime reference, Generator analyzer reference, `.ngr` and configuration AdditionalFiles.
- `src/CSNitrogen.Syntax/Grammars/Core.ngr`: initial names and empty class compilation units; later tasks extend this deliberately.
- `src/CSNitrogen.Syntax/nitrogen.json`: generated composition membership, shared namespace.
- `src/CSNitrogen/Parsing/ParseRequest.cs`, `ParseDocument.cs`, `CSharpParser.cs`: owning parse API and diagnostics.
- `src/CSNitrogen/Profiles/FeatureCatalog.cs`, `LanguageProfile.cs`, `ProfileResolver.cs`: explicit features, version metadata, deterministic validation.
- `tests/CSNitrogen.Tests`: generated-view, API, composition, and feasibility tests; `.ngr` probe inputs are generated only into this test assembly.
- `tests/CSNitrogen.Oracle.Tests`: pinned Roslyn syntax and language-version comparisons.
- `docs/coverage/features.json`: machine-readable coverage ledger.
- `docs/coverage/foundation-evidence.md`: commands, dependency identity, pass/fail/blocked cases, and follow-up boundaries.
- `README.md`: supported foundation inputs, run commands, and explicit partial status.

## Task 1: Real generated compilation-unit slice

**Create:** root build/solution files, both production projects, `Core.ngr`, `nitrogen.json`, `tests/CSNitrogen.Tests/CSNitrogen.Tests.csproj`, `GeneratedSyntaxTests.cs`, `docs/coverage/dependencies.json`.

**Produces:** `CSNitrogen.Syntax.CSharp1Composition.CreateBuilder()` and `.Start`; generated `CompilationUnit` and `ClassDeclaration` syntax views. Inspect the generator's actual emitted type names before binding public aliases.

- [ ] Record `dotnet --list-sdks`, Nitrogen HEAD and dirty file names in the dependency document. Initial inspection found HEAD `f5bf525e3e2723a8fe39e127f800f6376345feb1` and a modified `docs/editor-support.md`; verify again at execution. Do not clean it. Use an installed stable .NET 10 SDK if available; otherwise record the exact SDK used without changing the machine installation.
- [ ] Create minimal build/test projects and a failing generated parser test. Register all `.ngr` inputs and `nitrogen.json` as AdditionalFiles with namespace `CSNitrogen.Syntax`. Use the local Generator as an analyzer (`OutputItemType="Analyzer"`, `ReferenceOutputAssembly="false"`) and Runtime as a normal project reference. Test packages use the pinned versions above.

```csharp
[Theory]
[InlineData("", true)]
[InlineData("class C {}", true)]
[InlineData("class C {} class D {}", true)]
[InlineData("class C {", false)]
[InlineData("class C {} @", false)]
public void Parses_complete_input(string source, bool accepted)
{
    var language = CSharp1Composition.CreateBuilder().Build();
    using var parsed = language.Parse(source, CSharp1Composition.Start);
    Assert.Equal(accepted, parsed.Success);
}
```

- [ ] Run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~GeneratedSyntaxTests`; expect missing generated composition before the grammar is added.
- [ ] Add the initial grammar and config below. ASCII identifiers are explicitly foundation coverage, not the finished C# identifier grammar.

```text
syntax module CSharp.Core
{
  token Identifier = ['a'..'z' 'A'..'Z' '_'] ['a'..'z' 'A'..'Z' '0'..'9' '_']* except "class";
  syntax CompilationUnit = Types:ClassDeclaration*;
  syntax ClassDeclaration = "class" Name:Identifier "{" "}";
}
```

```json
{"languages":[{"name":"CSharp1","start":"CSharp.Core.CompilationUnit","namespace":"CSNitrogen.Syntax","grammars":["Grammars/Core.ngr"]}]}
```

- [ ] Rerun the focused tests. Add generated-view assertions for two class names and their authored spans using the actual generated API; prohibit a string-search substitute. Add comments/whitespace around the input and verify full consumption.
- [ ] Run `dotnet build CSNitrogen.slnx` and record results. Do not call this C# 1 completeness.

## Task 2: Owning parse API with honest acceptance

**Create:** `Parsing/ParseRequest.cs`, `ParseDocument.cs`, `CSharpParser.cs`, `ParseIssue.cs`; `Parsing/ParseDocumentTests.cs` under the normal test project.

**Consumes:** generated CSharp1 factory and Nitrogen `ParseResult`, `SyntaxTree`, `TextSpan`.

**Produces:** the following API. A `ParseIssue` carries code, message, physical UTF-16 span, and category; error severity is explicit. `ParseDocument` implements `IDisposable`, owns exactly one Nitrogen result, and exposes generated root access through its owning tree.

```csharp
public sealed record ParseRequest(string Source, string Profile,
    string DocumentId, IReadOnlyList<string> ConditionalSymbols);
public static class CSharpParser
{
    public static ParseDocument Parse(ParseRequest request);
}
// ParseDocument exposes:
// string OriginalText; string Profile; bool Accepted; bool ProfileComplete;
// Nitrogen.SyntaxTree Tree; IReadOnlyList<ParseIssue> Issues; void Dispose();
```

- [ ] Write tests below and run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~ParseDocumentTests`; expect missing API.

```csharp
[Fact]
public void Supported_input_is_accepted_but_profile_is_partial()
{
    using var doc = CSharpParser.Parse(new("class C {}", "CSharp1", "test.cs", []));
    Assert.True(doc.Accepted);
    Assert.False(doc.ProfileComplete);
    Assert.Equal("class C {}", doc.OriginalText);
}
[Fact]
public void Tree_access_cannot_outlive_owner()
{
    var doc = CSharpParser.Parse(new("class C {}", "CSharp1", "test.cs", []));
    var tree = doc.Tree;
    doc.Dispose();
    Assert.Throws<ObjectDisposedException>(() => tree.Kind(tree.Root));
    doc.Dispose();
}
```

- [ ] Implement the wrapper using the genuine Nitrogen parse result. `Accepted` requires `Success` and no error issues. Map parser diagnostics using their original spans. Validate null source and unknown profile before allocating a tree. Do not fabricate an unavailable-feature code for an unrecognized syntax failure.
- [ ] Until preprocessing is implemented, reject nonempty conditional-symbol requests and source directives with a clearly documented unsupported-coverage issue, rather than pretending they were processed. Plain class input remains usable. The lexical task must prevent directive recognition inside strings/comments once those constructs are supported.
- [ ] Add malformed-input/recovery and repeated-disposal tests; run the focused tests and the whole normal-test project.

## Task 3: Explicit partial profiles and feature selection

**Create:** `Profiles/FeatureCatalog.cs`, `LanguageProfile.cs`, `ProfileResolver.cs`, `Profiles/ProfileResolverTests.cs`, `docs/coverage/features.json`.

**Consumes:** base grammar/composition. **Produces:** `ProfileResolver.Resolve(string name)` and `ResolveCustom(string name, IEnumerable<string> featureIds)`, returning a `ProfileResolution` containing either a validated `LanguageProfile` or composition issues. `LanguageProfile` has name, exact major/minor version metadata, sorted feature IDs, selected grammar paths, policy IDs, and completeness.

- [ ] Write a theory covering CSharp1…CSharp14 plus CSharp7_1, CSharp7_2, CSharp7_3. Assert CSharp7 means 7.0 and all are partial. Run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~ProfileResolverTests`; expect unresolved resolver.
- [ ] Populate the feature ledger from the spec's authoritative sources: identifiers/literals/trivia/directives, C# 1 syntax categories, and every release's feature list through 14. Record syntax versus semantic-only, precise introduction, authoritative link, dependencies, module/policy, fixtures, and status. Mark only the empty-class foundation as implemented; unimplemented entries must not have invented grammar paths or fixtures. Represent absent mappings explicitly with null/empty arrays and status.
- [ ] Define named profile metadata for every required version. Generate explicit composition entries using only currently implemented grammar files and the shared namespace; profiles therefore share the foundation until feature modules exist. Store requested features separately from implemented composition membership, so missing coverage cannot disappear from completeness calculations.
- [ ] Implement deterministic custom resolution. Reject unknown feature IDs, duplicate IDs, missing requirements, and declared conflicts; output stable issue ordering. Tests use a small fixture catalog with `base`, `lambda`, `query` requiring `lambda`, and two mutually exclusive fixtures to exercise validation without pretending those C# features are implemented.

```csharp
// In fixture-catalog tests, passing {base, query} reports the missing lambda.
// Passing {base, lambda, query} and its reverse yields identical sorted resolution.
// Passing {base, base} reports one duplicate issue; {base, unknown} reports unknown.
```

- [ ] Bind the parser to the generated factories for each named partial profile. Verify identical foundation input across profiles and assert the actual selected modules, not just profile labels.
- [ ] Validate ledger references, unique IDs, acyclic requirements, and release assignments in tests. Run the normal suite; do not mark an unimplemented feature verified merely because the metadata test passes.

## Task 4: Pinned Roslyn oracle and version-boundary evidence

**Create:** oracle test project, `Oracle.cs`, `OracleTests.cs`, `FoundationParityTests.cs`.

**Consumes:** owning parse API; Microsoft.CodeAnalysis.CSharp 5.0.0. **Produces:** test-only `Oracle.Observe(string source, LanguageVersion version)` returning syntax diagnostics, compilation diagnostics, and selected Roslyn node kinds/spans. Explicitly use `SourceCodeKind.Regular`.

- [ ] Add failing tests asserting `LanguageVersion.CSharp14` exists and explicit CSharp1/7/7_1/7_2/7_3/14 options survive construction. Run `dotnet test tests/CSNitrogen.Oracle.Tests --filter FullyQualifiedName~OracleTests`.
- [ ] Implement observation with `CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(version, kind: SourceCodeKind.Regular))`. For compilation observations use `CSharpCompilation.Create`, a fixed reference set from the selected target framework reference pack, and DLL output. Keep unrelated semantic diagnostics separate; do not filter errors by localized message text.
- [ ] Prove the version oracle using `class C { object M() => new(); }`: observe under C# 8 and 9, assert the explicit unavailable-feature diagnostic on C# 8, and no such diagnostic on C# 9. Pin its actual diagnostic ID with evidence from this package. Test malformed input independently. Do not expect CSNitrogen to accept this source at the foundation stage.
- [ ] Compare empty files, empty class declarations, comments, malformed braces, and trailing junk with CSNitrogen. Compare class-name spans and declaration counts as structural observations. Record oracle package/assembly identity with results.
- [ ] Run the oracle suite. Inspect production project references and generated dependency manifests to prove Roslyn compiler dependencies are confined to the oracle project; Nitrogen's build-time analyzer dependencies are a separate build concern.

## Task 5: Lexical capability gates without invented scanner support

**Create:** `tests/CSNitrogen.Tests/Feasibility/LexicalCapabilityTests.cs`, `Grammars/LexicalProbe.ngr`, `docs/coverage/lexical-capabilities.md`.

**Consumes:** Nitrogen token expressions, lookahead, syntax fields, and generated probe views. **Produces:** executable lexical evidence; these probes are test-only and do not claim full production support.

- [ ] Write positive and malformed probe cases for escaped quotes, verbatim doubled quotes, and interpolation with an embedded arithmetic expression. Assert an expression child exists in the generated tree, rather than accepting an opaque string token. Run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~LexicalCapabilityTests` before implementing the productions.
- [ ] Use the expression syntax proven by Nitrogen's `Lexical.ngr` and `Calc.ngr` examples. Add only the productions necessary for the listed probes; test whitespace inside and outside string bodies independently, since default trivia skipping must not eat string content.
- [ ] Investigate raw delimiters of lengths 3, 4, and 8; embedded shorter quote sequences; malformed shorter closers; and raw interpolation with one, two, and three dollar signs. A finite enumeration is evidence for those lengths only. Record whether equality of arbitrary delimiter lengths can be expressed by the inspected grammar engine; search implementation for a documented extension hook before proposing an adapter.
- [ ] Add directive-boundary fixtures: `#if` inside a verbatim string, raw string, block comment, and an inactive branch containing malformed quotes. Document how directive recognition differs in active versus inactive text. Test CRLF/LF, a leading BOM, and a supplementary Unicode character before the observed span.
- [ ] Record each probe's source, observed tree/diagnostic, commands, and implemented/blocked status. If arbitrary delimiter handling or trivia boundaries are blocked, retain the reproduction and do not silently cap delimiters or substitute Roslyn. Add an explicit unsupported-coverage expectation for affected production profiles.
- [ ] Rerun the normal suite. Stop expansion into full C# 1–14 when a necessary capability requires a separately scoped Nitrogen change; report independent completed work and the exact reproduction.

## Task 6: Ambiguity and extension-membership gates

**Create:** `Feasibility/AmbiguityCapabilityTests.cs`, `Grammars/ExpressionProbe.ngr`, `Grammars/GenericProbe.ngr`, `Grammars/LambdaProbe.ngr`, `docs/coverage/ambiguity-capabilities.md` under the test/documentation directories.

**Consumes:** module composition and generated syntax observations. **Produces:** evidence for precedence, lookahead, contextual names, and module omission; not a complete C# expression grammar.

- [ ] Write parameterized cases for `a + b * c`, `(x)`, `(T)x`, `a < b`, `F<T>(x)`, `F<G<int>>(x)`, `a >> b`, `x => x`, and `from + select`. Declare expected syntax shape for each syntactically decidable probe. Observe undecidable cases explicitly instead of accepting the first alternative as proof.
- [ ] Run `dotnet test tests/CSNitrogen.Tests --filter FullyQualifiedName~AmbiguityCapabilityTests` to expose missing productions. Implement test-only grammar categories using precedence annotations and syntactic lookahead supported by the Nitrogen implementation.
- [ ] Compose base-only, base-plus-generics, and base-plus-lambda probe languages. Verify the module lists and acceptance pairs: lambdas must fail without the lambda module; a disabled contextual construct must not reserve ordinary identifiers globally. Do not put all alternatives in a base module and call post-parse rejection structural omission.
- [ ] Compare the declared observation points against Roslyn in `tests/CSNitrogen.Oracle.Tests/AmbiguityParityTests.cs`. Record differences that require binding, grammar refactoring, or a Nitrogen capability change.
- [ ] Add bounded deep nesting and malformed tails at depths 16, 64, and 128. Execute in a child test process with a 30-second timeout to contain hangs/crashes; preserve failed inputs. Record elapsed time and allocation measurements as baseline evidence, not a performance guarantee.
- [ ] Run both test projects and publish the membership/ambiguity report with failures preserved.

## Task 7: Foundation acceptance and continuation boundary

**Create:** `README.md`, `docs/coverage/foundation-evidence.md`. **Update:** approved spec status to foundation implemented only after its actual checks; feature ledger entries according to evidence.

- [ ] Run `dotnet build CSNitrogen.slnx`, `dotnet test tests/CSNitrogen.Tests`, and `dotnet test tests/CSNitrogen.Oracle.Tests`. Record failures and capability blockers without claiming all tests pass if expected capability behavior remains unresolved.
- [ ] Verify the README demonstrates only implemented input, states every profile is partial, distinguishes generated typed syntax from C# semantics, and documents the Nitrogen dependency path override. Include the exact commands above and source/test links.
- [ ] Confirm production parsing calls Nitrogen and generated factories; confirm no handwritten C# parser or Roslyn parsing dependency was introduced into production. Recheck that sibling source changes were not made by this work. Configure local intermediate/output paths where required so builds do not write into the sibling checkout.
- [ ] Report foundation acceptance separately from lexical/ambiguity feasibility results. An unmet feasibility gate remains blocked; a test that asserts a known limitation does not turn the limitation into a passing capability.
- [ ] Prepare the next implementation plan from the completed ledger and capability evidence, covering the C# 1 baseline. If core work is necessary, present its minimal reproductions and separate proposed scope before any sibling changes.

## Remaining delivery plans under the approved spec

These are continuation milestones, not claims that this foundation plan implements their grammars:

1. C# 1 baseline: complete Unicode/escaped identifiers, literals, trivia, preprocessing, names/types, attributes, declarations/members, statements, expressions, unsafe syntax, and directive/location diagnostics. Gate: verified C# 1 ledger, recovery/negative coverage, and pinned corpus evidence.
2. C# 2–6 modules: populate exact release feature boundaries from the ledger, including generics, nullable value-type syntax, anonymous methods/iterators, lambda/query syntax, and subsequent declaration/expression additions. Gate: all affected boundary fixtures and earlier-profile regression tests.
3. C# 7.0–8 modules: preserve 7.1/7.2/7.3 distinctions and add the ledger's pattern, tuple, reference, declaration, and expression syntax. Gate: each minor profile has independent coverage results.
4. C# 9–11 modules: add their ledger's records, top-level syntax, pattern families, namespace/usings changes, and lexical additions. Gate: raw/interpolated strings expose embedded expression syntax and preserve source coordinates.
5. C# 12–14 modules: implement the syntax-changing entries, with semantic-only entries explicitly excluded. Gate: stable feature specifications, exact introduction boundaries, and no preview leakage.
6. Customization demonstrations: query-free with lambdas retained, and unsafe-free. Gate: accepted/rejected pairs, dependency failures, actual module membership, and no disabled-feature leakage. Only then propose a broader customizable-language design.

No later milestone may be called complete while its ledger contains an applicable unimplemented or blocked syntax entry. Corpus provenance/licensing and full directive coverage belong to the baseline plan; they are not satisfied by the small foundation fixtures.

## Execution status (2026-10-16)

The generated foundation, owning API, explicit partial profiles, requested-coverage identity, oracle, and selected probes are implemented. Full inventory classification/boundary review, arbitrary raw delimiter handling, and complete preprocessing remain open; do not mark every checklist item complete. See the [evidence report](../../coverage/foundation-evidence.md) and [next lexical/preprocessing plan](2026-10-16-csharp1-lexical-preprocessing.md).

## Plan review

The user approved the architecture; this plan is ready for review. Recommended execution: native in this session, because the foundation tasks share evolving generated APIs and feasibility evidence. Subagent-driven execution is available if explicitly selected. Do not initialize Git, commit, or edit Nitrogen source as an incidental step.
