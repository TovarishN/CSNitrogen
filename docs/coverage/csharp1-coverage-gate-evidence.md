# C# 1 production/coverage gate — 2026-10-17

PR #10 was merged at its verified head after a fresh 2,695-test run. This checkpoint starts from merged main `d2d1805` and follows approved source-grammar plan Task 8. It makes the existing completion contract auditable; it does not declare C# 1 complete or begin later-version implementation.

## Current gate result

The checked-in [catalog](csharp1-coverage-gate.json) is structurally valid and the completion gate remains **open**:

| Input | Current result |
|---|---|
| Configured CSharp1 grammars | 10 module/file mappings match actual Nitrogen ASTs |
| Authored declarations and named alternatives | 641 exact entries, including 446 generated lexical fragments |
| Policy-source diagnostic literals | 27 source/code identities in 8 files, mapped through 5 policy groups |
| Applicable syntax inventory entries | 40: 11 verified, 29 implemented |
| Reviewed semantic exclusions | Constant-expression evaluation; boxing/unboxing |
| Explicit closure obligations | Historical-standard crosswalk; independent production/context review |

There are 49 unsatisfied review states: 10 module groups and 8 policy groups remain observed, 29 syntax inventory entries remain implemented, and the two closure obligations remain pending. These are overlapping evidence states, not 49 demonstrated parser defects. No passing-test count promotes them.

The 641 entries are the consumer's authored Nitrogen declarations/alternatives, including internal generated helpers and a legacy compatibility alias. They are **not** 641 independently reviewed ECMA productions. Cataloging implemented rules cannot detect a standard production that has never been implemented. That is why the separate historical crosswalk remains mandatory.

## What the audit checks

`CSharp1CoverageGateAudit` is test-only. It reads the production assembly's embedded configuration, inventory, grammar and source resources. The pinned Nitrogen `GrammarParser` produces the actual `.ngr` ASTs; declarations, extensible-rule alternatives and cross-module extension alternatives have distinct catalog identities. No grammar-name regex extraction is used.

Roslyn syntax trees identify CSN diagnostic string literals in embedded C# source. A policy identity is **source path plus code**, preserving existing CSN2005/CSN2006 reuse across different policies. This is a conservative static code catalog, not a proof that every literal is emitted or every branch is tested. Dynamic Nitrogen NG diagnostics are parser diagnostics, not invented CSN policy entries; existing malformed-input fixtures remain their evidence.

The audit requires exact module, production, policy-source and code sets, validates policy-group/source associations, and checks positive/negative fixture links against real Fact/Theory declarations in copied test sources. Inventory grammar/policy mappings and fixture files must resolve. Reviewed semantic classifications require explicit exclusion reasons and references; unknown classifications, statuses, duplicate entries, missing obligations and a fabricated profileComplete override fail validation. Evidence links establish traceability, not the truth of every test's assertion. Fixture execution and historical conformance judgment remain separate work.

Closure status is declarative review metadata. The audit does not perform semantic binding or automatically prove a declaration marked verified. The production `ProfileComplete` behavior remains false. No public completion-audit API is introduced.

## Corrections and coverage identity

The audit reproduced an existing ledger gap: the imported C# 1 preprocessor-directives entry had neither grammar nor policy mapping. It now explicitly maps to `csharp1-preprocessing`, with the existing conditional/directive-effect fixtures. Its status stays implemented. A mistaken authored fixture method name was corrected after the link audit rejected it.

The new gate ledger is embedded and included in `FeatureCatalog.CoverageIdentity`. A regression recomputes the identity from named inputs and proves that omitting the ledger changes it. The identity covers the ledger declarations and production inputs; it does not hash test bodies, execution logs or unembedded reports. Parser grammar, accepted-source behavior, profile memberships and Nitrogen core remain unchanged.

## Execution evidence

The first helper build needed a missing Roslyn extension import. The behavioral RED run then failed on the missing embedded ledger and the invalid current catalog (unmapped preprocessing entry plus the mistaken fixture link). Four further RED cases exposed missing audit checks for wrong policy-source paths, ungrouped sources, unknown policy groups and unreviewed semantic classification. The corrected audit, metadata mapping and identity integration pass all 25 focused observations. Three of those exercise the previously untested Core legacy alias with full-consumption positive/negative input; the others check catalog validity, mutation rejection and completion boundaries.

Run the audit with:

```sh
dotnet test tests/CSNitrogen.Oracle.Tests --filter FullyQualifiedName~CSharp1CoverageGateTests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --logger 'console;verbosity=detailed'
```

A green test run means the current catalog is valid and correctly remains partial. Inspect the reported states; it does not mean the completion gate has passed.

## Next closure work

1. Build the crosswalk from the pinned ECMA-334 first-edition productions to implemented grammar families, compatibility policies and justified semantic/sugar equivalences. Record missing or unsupported syntax explicitly.
2. Close the crosswalk's positive/negative, ambiguity, source-context, authored-span and diagnostic obligations. Promote individual module/policy/feature review states only when their evidence supports it.
3. Re-run full build/tests, bounded malformed/deep fixtures, pinned corpus and dependency audits. Prepare the C# 2–6 feature-module plan only after the C# 1 gate passes.

The six-file pinned external corpus is retained as finite regression evidence. A larger corpus can improve confidence; it cannot substitute for the crosswalk. Historical Unicode policies after CSharp1 and arbitrary raw-delimiter support belong to later profiles. Semantic evaluation, binding, lowering and execution remain outside the approved parser scope.

## Full verification

Full solution build/test command: `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s`. Result: **706 normal + 2,014 oracle = 2,720 passed**, zero failed/skipped. This includes 25 new audit/identity/legacy-alias observations, existing bounded nested/malformed/directive-expression cases and the external corpus regressions. The audit's host C# source inspection uses explicit CSharp14 with pinned Roslyn 5.0.0; the audited language profile remains CSharp1.

All 118 original/isolated Nitrogen source hashes match. Original revision remains `f5bf525e3e2723a8fe39e127f800f6376345feb1`, with only preexisting ` M docs/editor-support.md` dirty state. All six external corpus hashes match provenance. Production dependency manifests exclude both Microsoft.CodeAnalysis and the Nitrogen.Grammar bootstrap parser; the latter was added only to the oracle test project for authoritative `.ngr` AST inspection. Original corpora, parser grammar, composition membership and accepted-source behavior are unchanged.

## Fresh review

Fresh read-only review of `d2d1805..cfce8e2` found no Critical, Important or Minor issues and judged the bounded structural-audit checkpoint ready to merge. It inspected the complete diff, approved plan/design, execution rulings and saved full-test log. No fix pass was needed.

Review limits are retained: exhaustive historical completeness, unchanged-family assertion/context closure and human review authenticity are not proved by this catalog. Later Unicode, larger corpus work and C# 2–14 remain separate. The reviewer did not execute tests/audits; the 2,720-pass run and dependency/corpus/sibling checks remain author-run evidence. Task 8 and CSharp1 completion remain open.

## Historical-source continuation

After PR #11, the [standard crosswalk checkpoint](csharp1-standard-crosswalk-evidence.md) adds an independent 362-occurrence Appendix A index and candidate owner mappings. Its aggregate pending-production blocker brings the evaluator to 50 overlapping blocker reports; the earlier 49-state result above records the pre-crosswalk checkpoint. All source rows remain unverified, and both closure obligations remain pending.
