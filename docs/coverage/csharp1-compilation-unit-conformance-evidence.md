# C# 1 compilation-unit/namespace continuation — 2026-10-17

PR #8 was merged at its verified head after a fresh 2,366-test run. This checkpoint starts from merged main `3beffc0` and follows approved source-grammar plan Task 6. The owning root/member API already exists; this step checks its integration rather than migrating it again. All profiles remain partial.

## Reproduced placement correction

The pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §16.1, orders using directives, global attributes and namespace/type declarations. Namespace bodies (§16.2) do not include global attributes. Independent Roslyn CSharp1 observations report syntax diagnostic CS1730 for assembly/module attribute targets after a source-level type or inside a namespace body. Our grammar could reinterpret these as local attributes on the following type and accept them.

`CSharpCompilationUnitPolicy` adds CSN2012 for those misplaced targets. It visits generated namespace-member wrappers, selects only the five type alternatives and examines their attribute sections. Targets use the existing profile-specific Unicode identifier normalization, including escaped spellings, verbatim identifiers and ignored formatting characters. Each diagnostic covers the offending authored target token. Recovery guards skip incomplete structures; the result remains rejected by parser diagnostics. The raw grammar remains broader than the public parser, which requires syntax policy.

This does not check attribute applicability. Local member, parameter and nested-type targets remain accepted, with selected Roslyn CS0657 compilation warnings recorded separately. A file containing only global attributes is valid syntax; an initial negative expectation for that case was corrected. Missing attribute classes, namespace/type resolution, alias collisions and declaration meaning remain semantic work.

## Typed integration observations

The new root comparer traverses actual `Root.Usings`, `Root.Attributes` and `Root.Members`, then follows `NamespaceMemberBody.Value` and concrete type alternatives. It compares ordered namespace member kinds/spans, nested namespace names/usings, alias name/equals/qualified target spans, global attribute target/items, all five source type alternatives and selected class/struct fields/method bodies and nested types. Root class-projection order, spans and node identity are checked against the global class members. It reuses parameter/attribute/expression/statement assertions; extracting the shared attribute-items comparer preserves the distinct global and local section layouts.

The whole-file matrix crosses three using prefixes, three global-attribute prefixes and six member arrangements (54 cases), plus eight comment/escaped-name/repeated/nested-namespace and payload sources. Six conditional/newline cases compare active trees and #line mapped locations with Roslyn while inactive malformed text stays masked; physical UTF16 positions are retained. Ninety placement cases cross six target spellings, five source type kinds and three source contexts. Selected malformed/later source-context rejections and local applicability classifications complete the oracle evidence.

Normal tests cover both CSharp1 and CSharp14 placement policy, legal global ownership and preserved local applicability, multiple independent target diagnostics, masked-source diagnostic positions, malformed/trailing input rejection, and borrowed root/namespace/list/name/projected-class access after disposal. Read-only snapshots such as original source strings remain owned metadata; borrowed syntax views expire with the parse result.

This is finite integration evidence, not exhaustive declaration-tree equivalence, semantic compilation or complete C# 1 conformance. Declaration/header details outside the selected payloads remain in the preceding checkpoint's bounded evidence. Unsafe membership/recovery depth and all later feature modules remain open.

## Execution evidence

The initial 91-case oracle run had three failures: the mistaken attributes-only expectation and two genuine source-placement false acceptances. A diagnostic probe confirmed CS1730 syntax ownership for both. The first expanded matrix had a string-format fixture construction error; after fixing literal braces, all 90 independent placement observations failed for false acceptance. Nine normal checks also failed before the policy. After the correction, 178 oracle and 24 normal focused observations passed; additional masked/recovered/local-applicability observations were then added. Failures and probes are retained in the local execution ledger, rather than presenting fixture errors as production RED/GREEN evidence.

Full verification and fresh review are recorded below after execution. CoverageIdentity automatically includes the new policy through its existing production-source resource glob; no Nitrogen core or sibling change is required.

## Verification

Full solution build/test command: `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s`. Result: **648 normal + 1,929 oracle = 2,577 passed**, zero failed/skipped. This adds 28 normal and 183 oracle observations (211 total): 62 whole-file shapes, 90 misplaced-target syntax classifications, 22 malformed/later source contexts, three local-target applicability observations and six conditional/newline integrations comprise the oracle additions.

All 118 pinned dependency hashes match both the original and isolated Nitrogen copies. Original sibling dirty status is unchanged (` M docs/editor-support.md` only). All six external corpus hashes match provenance; original context/attribute corpora are unchanged. Both production runtime dependency manifests exclude Microsoft.CodeAnalysis. Nitrogen core and production grammar/API remain unchanged; the new policy is included in the coverage identity.

Decision: reject source-level global attribute misplacement while preserving local applicability as semantic validation. The cost is accepted local syntax that may still warn or fail compilation. This checkpoint does not close exhaustive recovery, alias/name resolution, semantic compilation or whole-language conformance. Next is the explicit unsafe-composition revalidation, followed by the remaining C# 1 production/coverage gate.

## Fresh review

Fresh read-only review of `3beffc0..8c9ec0e` found no Critical, Important or Minor issues and judged this checkpoint ready to merge. It inspected the complete diff, approved Task 6/spec, saved full-run log and relevant generated parser/view integration. No fix pass was needed.

Review limits are retained: execution and dependency/corpus/sibling audits were author-run, not independently rerun by the reviewer. Cast/statement-expression/interface families rely on preceding evidence; this step does not claim exhaustive revalidation. Unsafe omission/leakage remains the next continuation. Semantic attribute applicability, binding, alias resolution and compilation success remain excluded. Exhaustive recovery/resource limits and complete C# 1–14 conformance remain open. The cost is finite scoped evidence and accepted syntax that can still fail semantic compilation; no profile status is promoted.
