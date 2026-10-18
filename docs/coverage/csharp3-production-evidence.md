# CSharp3 bounded production syntax evidence

Base: merged PR 18, 662ca83. Date: 2026-10-18.

The public CSharp3–14 factories now include additive Nitrogen modules for lambdas, queries, object/collection initializers, anonymous creation, implicit arrays and extension receivers. Source-local policies handle anonymous/query operand categories, raw query-word reservation, inferred anonymous members, collection scalar categories, extension receiver placement, historical partial methods and read/write automatic properties. No LINQ translation, delegate binding, type inference, member synthesis or partial-method pairing/erasure is claimed.

CSharp1/CSharp2 remain reviewed complete. CSharp3–14/custom profiles remain partial. Newly mapped CSharp3 inventory entries are **implemented, unreviewed**, not verified historical completeness. The lock inventory duplicate maps to existing baseline statements; implicit arrays have an explicit source-only inventory identity.

## Composition and compatibility

- CSharp3+ configuration explicitly selects six new modules. Queries and lambdas have independent omission tests; queries have no lambda capability dependency. Object and collection initializers form one cohesive module because nested initializer syntax shares ownership. Anonymous types and implicit arrays are separately removable.
- `FixedParameter.Receiver` is a new optional empty extension seam. Existing `Attributes`, `Mode`, `Type` and `Name` fields retain their meanings; CSharp1/CSharp2 receiver absence and ref/name observations are tested. New CSharp3 typed fields and list nodes preserve authored delimiters and UTF16 spans.
- Initialized object creation uses `TypeBase`, preserving the earlier explicitly typed array creation owner. New creation forms are included in primary suffix checks. Implicit arrays retain direct-array element-access restrictions.
- The shared partial modifier recognizes a declaration keyword only before class/struct/interface/void. A type named `partial` remains usable as a return type. A regression exposed and corrected the previous contextual-name over-consumption in CSharp2.
- Historical iterator source checks apply to CSharp3 and recognize lambda body boundaries; ordinary return/out/yield restrictions remain in force. No iterator execution claim is made.

The CSharp1 authored-production/policy ledger now includes the empty receiver seam and the two delegated source-policy owners. The CSharp2 ledger pins all current grammar/source inputs again, including the new files; its approval hash changes with the reviewed ledger. Exact earlier factory membership, source occurrence crosswalks and admission validation logic are preserved. Drift mutation tests still run as part of the full suite.

## Evidence and review

Public-parser tests cover version 2/3 boundaries, positive/malformed neighbors, independent omission, later partial factories, typed children, list separators/ranks, lambda body/assignment ownership, receiver fields, supplementary-prefix UTF16 spans and inherited contexts. Separate Roslyn 5.0.0 CSharp3 observations read the authored production fixtures without referencing the normal test assembly.

Observed test-first failures included 24 missing capability/production cases,16 missing construction cases,12 declaration/context cases,3 inherited iterator holes,3 primary suffix cases,1 pointer-array receiver and 2 escaped query spellings, followed by 2 direct null-coalescing operand category witnesses. Positive and negative pre-existing behavior remained in those runs; failures were not discarded. The initial escaped-query fixture accidentally decoded its escape in the authoring script; the literal spelling was repaired and the two expected policy failures were observed before the source fix.

Independent whole-branch review identified the primary-suffix omissions and pointer-array receiver over-rejection. Both were reproduced and fixed, then re-reviewed with no remaining actionable findings. Roslyn source-local CS0201 evidence also caught anonymous creation incorrectly allowed as a standalone statement; initialized ordinary object creation remains valid there.

Raw query words are reserved inside queries, including member names. Escaped spellings remain identifiers under clause 2.4.1; they are not normalized into keywords. Roslyn's CSharp3 syntax observations agree on the retained raw-word and escaped-word witnesses. No historical/modern query discrepancy is asserted from those examples.

Final verification:

```sh
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
```

**1,384 normal + 3,416 oracle = 4,800 passed, zero failed/skipped.** The merged baseline was 4,616 tests. `git diff --check` passes. All 123 dependency source hashes match the pinned snapshot. Production generated code was rebuilt from changed `.ngr` inputs during verification. Dependency snapshot and sibling repositories were not edited.

## Remaining admission work

The [Unicode contract decision](../superpowers/specs/2026-10-18-csharp3-unicode-contract-decision.md) is required before selecting a frozen CSharp3 lexical policy. Executable observations show the current BMP placeholder differs from both historical candidates. Full historical source indexing/crosswalk, lexical/directive review, representative licensed external corpus and a separate fail-closed CSharp3 gate remain pending. The current production feature milestone does not satisfy or bypass those obligations.
