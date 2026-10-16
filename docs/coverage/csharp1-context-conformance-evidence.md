# C# 1 context conformance continuation — 2026-10-16

PR #1 was already merged at `6149e53` when this continuation began. The clean checkout was fast-forwarded to that commit. Work continues the approved source plan's expression, statement, declaration and coverage obligations; no new architecture or Nitrogen core changes are included. **All profiles remain partial.**

## Corrected behavior and typed views

- Conditional false arms now parse full expressions. `a ? b : c = d` has a ConditionalExpression root whose false arm contains the assignment. The generated Pratt tail previously moved the assignment outside the conditional. `ConditionalExpressionExpression.False` now exposes a typed `ConditionalOperandNode`; use `.False.Value` for the expression. Root factories and parse ownership remain unchanged.
- General catch clauses must follow all typed catches. Grammar helper rules constrain ordering while preserving CatchClause nodes. Exception type ancestry and reachability are excluded semantic work.
- Properties, indexers and interface signatures reject repeated accessor kinds. Event accessor declarations require one add and one remove, in either order. Checks use generated accessor tokens and their nearest owning declaration, so state does not leak across declarations.
- Static constructors reject parameters and constructor initializers. Structs reject destructors. Explicit parameterless struct constructors and instance field initializers report unavailable before C# 10 and unsupported coverage thereafter; later syntax has not been enabled accidentally.
- Empty enums no longer admit a lone comma. Empty bodies and nonempty lists with a trailing comma remain supported.

## Historical reference and inventory

The immutable [ECMA-334 first edition, December 2001](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf) is recorded by URL/title/SHA256 in `csharp1-reference-policy.json` and embedded in CoverageIdentity. The PDF itself is not republished. Relevant clauses are §14.12 (conditional), §15.10 (try), §17.6.2/17.7 (accessors/events), §17.11 (static constructors), and §18.3.7–9 (struct differences).

The historical reference specifies Unicode 3.0. The implementation still uses recorded .NET 10 BMP categories and compiler comparisons. Historical category compatibility is explicitly open; modern-category evidence does not prove that older table. Full per-production/context closure also remains pending.

All 42 C# 1 inventory entries now have reviewed classification/module associations. Imported release entries remain implemented rather than verified: this is an inventory audit, not proof of complete support. Boxing/unboxing remains excluded semantic work; conversion declarations are classified as syntax with binding excluded. Semantic observations such as enum underlying-type validity and missing using-variable initializers are retained without treating every compiler error as syntax failure.

The [C# 10 struct proposal](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-10.0/parameterless-struct-constructors.md) identifies the later forms used for version checks. Its implementation is future work.

## Corpus and verification

The pinned Mono commit remains `13d1764537ce5bbe5a06bf133fb2db1a4b5d2131`. Added ICollection.cs, EventHandler.cs, AttributeUsageAttribute.cs and Version.cs. All six selected files parse successfully with both CSNitrogen and Roslyn CSharp1 syntax observations; original MIT notices and hashes remain in provenance. Total corpus: 808 lines. Binding/core-library compilation is not claimed.

Retained RED runs found 16 public context acceptance errors plus three conditional root-shape errors. An oracle classifier was extended for empty accessor lists (CS0548), separately from other semantic diagnostics. The historical identity resource also has a retained RED/green check. Source changes and builds are serialized.

Pre-review full suites passed **377 normal tests and 291 oracle tests**. These include 38 shared source contexts, three conditional child/span comparisons, nested declaration isolation, recovered declarations, and C# 1/9/10/14 availability categories. A fresh review and final clean build/tests follow before publication.

The previous production-map documentation minor is corrected. Remaining work includes exhaustive production obligations and historical Unicode policy; the C# 2–6 handoff gate has not passed. Arbitrary raw delimiters remain a later-version risk.

## Final review and verification

The fresh read-only review found no Critical or Important issue. One Minor test improvement is deferred: assert both assignment children and the inner false operand in nested conditionals, including target/value spans. The current regression checks root structure and outer false-arm kind/span.

Clean solution rebuild: zero warnings/errors. Full suites: **377 normal + 291 oracle tests passed**, zero failed/skipped. Commands and logs are `dotnet build CSNitrogen.slnx -t:Rebuild -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` (`pre-review-build.log`) and `dotnet test CSNitrogen.slnx --no-build` with the same properties and a 30-second hang timeout (`pre-review-tests.log`), under `.superpowers/sdd/csharp1-context-conformance/`.

All 118 recorded dependency hashes match both isolated and original Nitrogen source roots; original revision/dirty state is unchanged. All six corpus hashes and the historical specification hash match provenance. Production runtime manifests contain no Microsoft.CodeAnalysis package. The typed `.False.Value` prototype migration is intentional and documented; later grammar features remain unfinished.
