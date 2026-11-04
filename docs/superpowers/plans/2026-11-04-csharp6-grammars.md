# CSharp6 Grammar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the sixth-edition source additions (interpolated strings, null-conditional access, nameof, exception filters, using static, property initializers, expression-bodied members, index initializers) through typed Nitrogen grammar composition, then admit CSharp6 through an independent reviewed guard.

**Architecture:** Start from the admitted 32-module CSharp5 factory. Prove interpolation lexing, `?.` disambiguation and the Ambiguous nameof contract in a test-only composition under a separate generated namespace before any production edit; the other additions are proved there too but are not gates. Production adds one independently selectable module per inventory entry. Admission follows the CSharp5 pattern: independent index, main-text review, executed alternative matrix, owner crosswalk, then a fail-closed contract with a recorded approval.

**Tech Stack:** .NET 10, pinned Nitrogen `b3494870dfb5f187bae9065227d4ef7f742aa048`, xunit 2.9.3, Roslyn `Microsoft.CodeAnalysis.CSharp` 5.0.0 with `LanguageVersion.CSharp6`, ECMA-334 6th edition PDF `4b859dfce21f9935f234b48af55c6a9de36e2eda1eb58d3b565e5aab2b44922f`, csharpstandard `V6-standard` commit `721ef2964efeb5b4b803e856a00094dc4fe21bb7`.

**Spec:** docs/superpowers/specs/2026-11-04-csharp6-source-contract-decision.md. Unicode4 scalar continuity and the Ambiguous nameof node approved 2026-11-04.

## Global Constraints

- Preserve CSharp1–5 admission and unrelated benchmarks; no sibling Nitrogen writes, handwritten C# parser or semantic/compiler/runtime claim.
- Preserve existing generated property types and original UTF16 coordinates; a public property-type or Nitrogen core change returns for a separate concrete decision.
- No CSharp6 admission from the bounded proof; CSharp7–14, 7.x minors and custom selections stay partial throughout.
- The sixth-edition PDF is the authority; Roslyn at `LanguageVersion.CSharp6` is an observation, and a disagreement is a recorded oracle divergence with its witness.
- Neither the PDF nor the markdown is redistributed; record hashes and line/section references only.
- Interpolated strings must expose typed holes; a token that consumes a whole interpolated string is not acceptable.
- Ambiguous `nameof(x)` spellings yield `SyntaxKinds.Ambiguous` with both typed readings and no issue.
- Verification command for every full run: `dotnet test CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false`.

## Review Focus

- Whitespace and comments inside interpolated text must be literal text, not skipped trivia: `$"a /*x*/ b"` keeps its text, while `$"{ x /*c*/ }"` treats the comment as trivia inside the hole.
- Format colon versus other colons: `$"{a?b:c}"` is rejected because the unnested colon starts a format and leaves `a?b` incomplete (Roslyn agrees); `$"{(a?b:c)}"`, `$"{F(x:1)}"` and `$"{global::A.B}"` have no format.
- `a?.5:1` stays a conditional expression with literal `.5`; `a?.b`, `a?[0]` and `a?.b()` are null-conditional.
- Ordinary identifiers stay identifiers: `nameof`, `when` and `static`-adjacent names (`int when; var nameof=1;`), and `using static` is recognised only as `using` `static` type-name `;`, leaving using-alias and using-namespace directives unchanged.
- Spans after a supplementary character (`/*𐐀*/` prefix) are exact UTF16 offsets for every hole, filter and expression body.

---

## Task 1: Source record and test-only seam proof

**Files:**
- Create: `docs/coverage/csharp6-reference-policy.json`
- Create: `tests/CSNitrogen.CSharp6.Tests/CSNitrogen.CSharp6.Tests.csproj` (pattern of `tests/CSNitrogen.CSharp5.Tests/CSNitrogen.CSharp5.Tests.csproj`, namespace `CSNitrogen.CSharp6Probes`, production `AsyncFunctions.ngr` and `AwaitExpressions.ngr` added to the shared list)
- Create: `tests/CSNitrogen.CSharp6.Tests/Grammars/CSharp6NoneProbe.ngr`, `CSharp6AllProbe.ngr`, test copies of only the grammars that need new empty seams, and one probe module per addition: `InterpolatedStrings.ngr`, `NullConditional.ngr`, `NameofExpressions.ngr`, `ExceptionFilters.ngr`, `UsingStatic.ngr`, `PropertyInitializers.ngr`, `ExpressionBodies.ngr`, `IndexInitializers.ngr`
- Create: `tests/CSNitrogen.CSharp6.Tests/CSharp6GrammarSeamTests.cs`, `CSharp6InterpolationTests.cs`, `CSharp6NameofTests.cs`, `CSharp6SpellingContractTests.cs`
- Create: `docs/coverage/csharp6-seam-proof-evidence.md`
- Modify: `CSNitrogen.slnx` (add the test project)

**Interfaces:**
- Consumes: the 32 CSharp5 grammar paths in `src/CSNitrogen.Syntax/nitrogen.json` (`CSharp5` entry).
- Produces: generated `CSharp6NoneProbeComposition` and `CSharp6AllProbeComposition` in `CSNitrogen.CSharp6Probes`; node kind names `InterpolatedRegularString`, `InterpolatedVerbatimString`, `Interpolation` (children `Expression`, optional `Width`, optional `Format`), `InterpolatedText`, `NullConditionalAccess`, `NameofExpression`, `ExceptionFilter`, `UsingStaticDirective`, `PropertyInitializer`, `ExpressionBody`, `IndexInitializer`. Task 2 reuses these names. No production factory change.

- [x] **Step 1:** Write `csharp6-reference-policy.json` in the shape of `csharp5-reference-policy.json`: primary PDF URL/bytes/SHA256, working text tag/commit/grammar.md SHA256, the PDFKit extraction SHA256 `b74337ad3d4151f370a1e591b09731044ae39981af163520afd7c8dfc160b70c`, `unicode` = frozen Unicode4 scalar approved 2026-11-04, `nameof` = Ambiguous approved 2026-11-04, `completion` = "partial".
- [x] **Step 2:** Add the none-probe (the CSharp5 module set) and `Source_additions_have_real_generated_owners` theory, one InlineData per addition, asserting the kind name exists in the all-probe and the none-probe rejects. Witnesses: `class C{string s=$"a{x}b";}` → `InterpolatedRegularString`; `$@"a{x}` + new-line + `b"` in a field initializer → `InterpolatedVerbatimString`; `class C{object o=a?.b;}` → `NullConditionalAccess`; `class C{string s=nameof(int);}` → `NameofExpression`; `class C{void M(){try{}catch(E e)when(e!=null){}}}` → `ExceptionFilter`; `using static System.Math;` → `UsingStaticDirective`; `class C{int P{get;}=1;}` → `PropertyInitializer`; `class C{int M()=>1;}` → `ExpressionBody`; `class C{object o=new D{[0]=1};}` → `IndexInitializer`. Run: `dotnet test tests/CSNitrogen.CSharp6.Tests --filter Source_additions` — Expected: FAIL (all-probe kinds absent).
- [x] **Step 3:** Add `Baseline_ordinary_identifiers_remain_available` on the none-probe: `class C{int when;int nameof;void M(){nameof(when);}}`, `class C{object o=a?b:c;}`, `class C{object o=a?.5:1;}`. Expected: PASS before and after the modules.
- [x] **Step 4:** Write `CSharp6InterpolationTests` RED cases on the all-probe, each asserting typed children and exact spans with and without a `/*𐐀*/` prefix: hole expression and text spans for `$"a{x}b"`; nested `$"{$"{x}"}"` and `$@"{$"{x}"}"`; width `$"{x,5}"` and `$"{x,-5:N2}"` (Width = `-5`, Format = `N2`); escapes `$"{{a}}"` produce no Interpolation; literal trivia `$"a /*x*/ b"` keeps text; hole trivia `$"{ x /*c*/ }"`; colon cases from Review Focus; rejections: new-line in regular text, unterminated `$"{x"`, `$"{}"`, lone `}` in text. Expected: FAIL.
- [x] **Step 5:** Implement `InterpolatedStrings.ngr` and any empty seam in a test copy of `Expressions.ngr`, using only existing Nitrogen syntax/token mechanisms. Run the interpolation tests. Expected: PASS, or the exact failing cases are retained and Step 9 applies.
- [x] **Step 6:** Write and implement `CSharp6NameofTests`: `nameof(int)`/`nameof(base)` → `NameofExpression` only; `nameof(x)`, `nameof(a.b)`, `nameof(List<int>.Count)` → one `Ambiguous` node whose two readings are `NameofExpression` and `InvocationExpression`, each spanning `nameof(...)`, with `p.Success&&!p.HasErrors`; `nameof(1+2)`/`nameof(x,y)` → `InvocationExpression` only. Expected: PASS, or retained RED and Step 9.
- [x] **Step 7:** Implement the remaining six probe modules, each in an empty seam of a test copy, and add per-module omission tests: removing one module from the all-probe rejects only its witness and keeps the other eight passing. Include the Review Focus `?.5`, `when`/`static` and span cases. Expected: PASS.
- [x] **Step 8:** Write `CSharp6SpellingContractTests`, parsing every witness above with `CSharpSyntaxTree.ParseText(source,new CSharpParseOptions(LanguageVersion.CSharp6))`, comparing acceptance and hole/expression spans; mark each disagreement as a named divergence row asserted in the test, and copy each into the evidence file. Expected: PASS.
- [x] **Step 9:** If Step 5 or 6 cannot pass without a core change, keep passing observation tests plus the failing desired-contract cases as `[Fact(Skip="core decision: <doc>")]`, write `docs/superpowers/specs/2026-11-04-csharp6-<topic>-decision.md` with the concrete mechanism and witnesses, and stop the dependent production work (spec staging: only that feature waits).
- [x] **Step 10:** Write `csharp6-seam-proof-evidence.md` (mechanism used, every witness, divergences, blockers). Run the full verification command. Expected: all pass; CSharp1–5 admission tests unchanged; generator output for production factories byte-identical (`git diff --stat src/CSNitrogen.Syntax` empty).
- [x] **Step 11:** Commit, then obtain one fresh whole-branch review before publishing the bounded slice.

Result (Task 1): 134 probe tests and the full solution (11,575) pass with no Nitrogen core change. Rulings: interpolated text lives in delimiter tokens; the nameof invocation reading is a NameofInvocation node; the parse wrapper must accept only the nameof Ambiguous tie. Task 2 obligations are listed in docs/coverage/csharp6-seam-proof-evidence.md.

## Task 2: Production capabilities and the Unicode4 selector

Consumes the reviewed Task 1 mechanism and node names. If Task 1 left a core decision, split into 2a (everything else) and 2b (the blocked feature after its decision).

**Files:** `src/CSNitrogen.Syntax/Grammars/` one module per addition (Task 1 names); `src/CSNitrogen.Syntax/nitrogen.json` (`CSharp6` entry and later entries gain the modules; `CSharp6`+ keep `GlobalTargetsCurrent.ngr`); `docs/coverage/features.json` (ten 6.0 entries: grammars, policies, fixtures, status `implemented`; semantic entries classified per the spec); `src/CSNitrogen/Parsing/UnicodePolicy.cs:10`; new `src/CSNitrogen/Parsing/CSharp6ContextPolicy.cs`; `tests/CSNitrogen.Tests/Syntax/CSharp6*Tests.cs`; predecessor contracts and gate JSON for pin refresh.

- [x] Write production RED cases: every Task 1 witness through `CSharpParser` with profile `CSharp6`, the same sources rejected or read as baseline under `CSharp5` with an unavailable-feature diagnostic naming the 6.0 feature, and await inside catch/finally accepted under `CSharp6` and still rejected under `CSharp5`.
- [x] Write the Unicode RED case: U+0220 and raw/escaped U+10400 identifiers accepted under `CSharp6`, resolved policy `unicode-4.0-scalar`.
- [x] Add the modules and nitrogen.json entries; change `UsesUnicode4` to `version.Major is 2 or 3 or 4 or 5 or 6`.
- [x] Add `CSharp6ContextPolicy` source-local checks with new `CSN` codes: property initializer only on an automatic property (all accessor bodies `;`, not abstract/extern, not in an interface); expression-bodied members not on constructors, finalizers or accessors; every policy that meets an `Ambiguous` nameof handles both readings.
- [x] Run focused tests, then refresh fifth/fourth/third/second pins in 5->4->3->2 order and run the full verification command. Expected: all pass; CSharp6 reports partial.
- [x] Commit; fresh branch review before integration.

Result (Task 2): production capabilities, Unicode4 for major 6, the sanctioned-nameof wrapper filter and CSN2060–CSN2062 are in place; full solution 11,631 passes; CSharp6 partial. Unavailable-feature diagnostics were not added (rejection under CSharp5 is an honest syntax failure, as for earlier versions). Evidence: docs/coverage/csharp6-production-evidence.md.

## Task 3: Sixth-edition conformance review

- [ ] Add `tools/extract-csharp6-standard-index.py` producing `docs/coverage/csharp6-standard-index.json` from the V6-standard grammar.md with `--check`; record productions, alternatives and provenance.
- [ ] Add `tools/compare-csharp6-pdf.py`: compare every grammar block and normative paragraph of the markdown with the PDFKit extraction; record each difference in `docs/coverage/csharp6-pdf-differences.json` with a disposition (PDF wins).
- [ ] Build the delta inventory, crosswalk, main-text differences and dispositions against admitted CSharp5 (`build-csharp6-*.py`, each with `--check`), and review every historical lexical/preprocessing/attribute reading listed in the spec, recording whether CSharp6 shares the Version5 reading or current compiler behavior.
- [ ] Link and execute every alternative on the sixth factory (`build-csharp6-alternative-matrix.py`), audit owners on the actual factory (`build-csharp6-owner-crosswalk.py`), and write `csharp6-normative-context-review.md` and `csharp6-production-review.md`.
- [ ] Full verification; commit; fresh review.

## Task 4: Independent CSharp6 admission

- [ ] Add `src/CSNitrogen/Profiles/CSharp6CoverageContract.cs` mirroring `CSharp5CoverageContract`: requires admitted CSharp5, exact ledger digest, source/Unicode identity with exclusions, pinned eligible inputs, the selected sixth factory, all reviewed records and `docs/coverage/csharp6-review-approval.json` (written by `tools/approve-csharp6-review.py`, reviewer named by the user, never by the agent).
- [ ] Tests: acceptance, predecessor dependency, factory identity, later/custom partial status, a drift family per pinned input, structural drift families, blank reviewer, unapproved record, missing input, input changed after approval.
- [ ] Add `CSharp6` to the catalog's admitted majors and both coverage-identity enumerations; refresh pins 6->5->4->3->2; update README status line and `docs/coverage/csharp6-reviewed-admission-evidence.md`.
- [ ] Full verification; commit; fresh review; the user records the approval before CSharp6 reports Complete.
