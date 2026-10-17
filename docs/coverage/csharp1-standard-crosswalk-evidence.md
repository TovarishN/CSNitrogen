# C# 1 historical grammar crosswalk — 2026-10-17

This Task 8 continuation starts from PR #11 merged as `3e2ed67`. Its premerge full run passed 706 normal and 2,014 oracle tests. It supplies the independent source inventory missing from the authored-rule catalog. **CSharp1 remains partial.**

## Source inventory and mapping boundary

The first-edition ECMA-334 PDF matches the SHA-256 recorded in [reference policy](csharp1-reference-policy.json): `ac93571b47aa424a31316a30ac7c9b99424b7347b81469847b738f9af8b51c39`. Its informative Appendix A summarizes the lexical, syntactic and unsafe grammars. PDF pages 353–378 correspond to printed pages 339–364.

The [independent index](csharp1-standard-index.json) contains **362 heading occurrences, representing 346 distinct names**. It is extracted from the PDF without reading Nitrogen rules. Each identity preserves the PDF page, printed line and heading name, so repeated array/constant/boolean summaries and unsafe extensions are not silently deduplicated. The extractor recognizes both `:` and `::`, `one of` headings and trailing heading whitespace. Page-level counts and a golden heading-identity digest make extraction drift visible. It stores locations and names, not the grammar right-hand sides or PDF.

The [candidate crosswalk](csharp1-standard-crosswalk.json) nominates exact authored rules/alternatives or policy groups for every occurrence. These targets resolve against the configured Nitrogen-AST catalog. This proves structural traceability, **not** that every standard alternative is implemented or equivalent. Some helpers are inlined, Pratt categories share `Expression`, and semantic distinctions share syntax. Examples:

| Source production | Current owner | Pending distinction |
|---|---|---|
| `namespace-name`, `type-name` | `NamesTypes.QualifiedName` | binding determines what the name denotes |
| precedence tiers | corresponding `Expressions.Expression` alternatives | operand context, associativity and ambiguity review |
| `constant-expression`, `boolean-expression` | `Expressions.Expression` | evaluation/conversion semantics excluded; syntax context still reviewed |
| `delegate-creation-expression` | object-creation syntax | binding identifies delegate construction |
| `pointer-element-access` | shared element-access syntax | array/pointer meaning is semantic; safe indexing survives Unsafe omission |
| builtin/extended `sizeof` | safe expression plus Unsafe extension | module boundary versus semantic unsafe context |
| directive/comment productions | scanner/preprocessor/trivia policies | inactive text, line structure and diagnostic behavior |
| attribute target productions | historical target recognizer/attribute tokens | normative target prose, including implementation-defined targets |

Every row remains observed, with four pending review dimensions: normative prose, positive/negative evidence, ambiguity/source context, and authored spans/diagnostics. Existing gate module/policy fixture links provide family-level evidence only. This checkpoint does not claim 362 independent conformance tests. An owner nomination does not prove that missing syntax has been ruled out. A future discovered gap can be marked `unresolved` with a reason while preserving its source occurrence.

Appendix A is expressly informative. The normative chapters and prose constraints remain authoritative; the source inventory cannot substitute for their review. No module, policy or feature status is promoted, and both historical-crosswalk/context obligations remain pending. Semantic binding, constant evaluation, lowering and execution remain outside the approved parser scope.

## Audit and identity

The test-only standard audit validates the pinned source metadata, golden heading set/locations, exact crosswalk occurrence set, exact authored targets and policy-group identities, mapping strategies, explanations and review states. A verified row cannot retain pending review dimensions. Missing, duplicated, invented or malformed entries fail closed. Unresolved rows remain completion blockers rather than disappearing through exclusions.

It feeds the existing completion evaluator. Promoting the older module/policy/feature/obligation metadata alone cannot bypass the 362 unclosed source occurrences. The current evaluator has 50 overlapping blocker reports: the previous 49 states plus one aggregate standard-crosswalk report. These counts are neither defect counts nor evidence of complete syntax support. Review declarations remain human metadata; automatic validation cannot authenticate a review or prove its conclusions.

Both JSON inputs are embedded and bound into `CoverageIdentity`. Omission or byte changes alter the identity. Test sources, review reports and execution logs are not hashed by that production identity. No grammar, composition membership, accepted-source behavior, public completion API or Nitrogen dependency changes are introduced.

## Reproduction

With development-only `pypdf` installed and a local copy of the pinned PDF:

```sh
python3 tools/extract-csharp1-standard-index.py /path/to/ECMA-334_1st_edition_december_2001.pdf --check
```

The tool checks the PDF hash before extraction. Normal builds/tests use the committed index and do not require a PDF, Python, pypdf or network access. The golden heading fingerprint is independently anchored in the test audit; changing it requires reviewing a fresh pinned-PDF extraction.

```sh
dotnet test tests/CSNitrogen.Oracle.Tests --filter 'FullyQualifiedName~CSharp1StandardCrosswalkTests|FullyQualifiedName~CSharp1CoverageGateTests' -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
```

The first two tests were observed failing on missing embedded resources and absent completion integration. After implementation, 54 focused observations pass, including 29 new source/crosswalk/identity observations and the existing 25 gate checks. Drift mutations exercise missing/duplicate/invented headings and mappings, wrong source/location/target/policy, unknown status/strategy/schema, missing review dimensions and unreviewed promotion. Separate cases retain repeated/unsafe identities, preserve explicitly unresolved syntax, and prevent old-metadata promotion from bypassing the crosswalk.

## Next closure work

Review each nominated mapping against the normative RHS and prose, then attach and assess production-specific positive/negative, ambiguity/context, span and diagnostic observations. Correct actual grammar/policy gaps with reproducing tests. Promote only evidence-supported rows and associated module/policy/features; retain explicit semantic exclusions. The C# 2–6 plan remains conditional on the C# 1 gate passing. The pinned six-file corpus is finite regression evidence, and later Unicode/raw-delimiter work remains separate.

## Verification

The full solution command above passed **706 normal + 2,043 oracle = 2,749 tests**, zero failed/skipped. This includes the unchanged pinned corpus and bounded malformed/nested/preprocessing regressions. Full-test output has no build warnings/errors. Whitespace formatting exited successfully with a generic workspace-load warning; the subsequent full build/test run is the authoritative execution evidence.

All 118 source hashes match both the original and isolated Nitrogen copies. The sibling remains at `f5bf525e3e2723a8fe39e127f800f6376345feb1` with only its preexisting modified `docs/editor-support.md`. All six pinned Mono corpus hashes match provenance. Production manifests exclude Microsoft.CodeAnalysis and Nitrogen.Grammar. The extractor reproduced the committed index from the verified PDF and rejected an unpinned input before parsing. `git diff --check` passes.
