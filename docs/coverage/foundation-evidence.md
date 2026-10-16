# Foundation evidence — 2026-10-16

Historical foundation/probe evidence. For the current implementation, see [C# 1 lexical/preprocessing evidence](csharp1-lexical-preprocessing-evidence.md).

This is a runnable foundation milestone, not complete C# 1–14 coverage. The only production grammar feature is empty class declarations with ASCII names, alongside empty compilation units. All 17 named profiles are partial. Production parsing uses the Nitrogen generator/runtime; Roslyn is confined to test projects.

## Verification

SDK: 10.0.100. Oracle: Microsoft.CodeAnalysis.CSharp 5.0.0, explicit language versions and .NET 10.0.0 reference assemblies. Nitrogen revision and actual copied source hashes are in `dependencies.json`; the source snapshot is built under `.dependencies/Nitrogen`.

Commands (from repository root):

```sh
dotnet build CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
dotnet test tests/CSNitrogen.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
dotnet test tests/CSNitrogen.Oracle.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

Final verification: solution build succeeded with 0 warnings and 0 errors; 58 normal tests and 29 oracle tests passed, 87 total, with no skipped tests. A passing known-limitation test establishes the limitation, not the missing capability.

The implementation was developed through missing-API/grammar RED runs and GREEN runs. Additional reproduced regressions covered hash text inside comments, reserved class names, unterminated comments, BOM, and CR/NEL/line-separator/paragraph-separator comment termination. Fixes remain in this consumer; the sibling Nitrogen source and pre-existing dirty state were preserved.

Audit before final verification found zero differences between copied dependency source hashes and the sibling originals, and unchanged sibling Git status. Neither production `.deps.json` listed Microsoft.CodeAnalysis. Build outputs were local; the sibling was not built.

## Implemented behavior

- Nitrogen `.ngr` compilation-unit parsing, generated typed class views, and authored identifier spans.
- An owning disposable parse API; recovered/error input is rejected and borrowed tree access expires on disposal.
- Seventeen explicit version profiles, including C# 7.1/7.2/7.3. CSharp7 means 7.0; all profiles currently share the foundation module.
- Deterministic feature validation: duplicates, unknown selections, missing requirements, declared conflicts, and catalog cycles fail with ordered issues.
- Requested inventory candidates, missing coverage, implemented module membership, and a SHA-256 coverage identity are separate. The identity includes inventory, dependency provenance, and the core grammar. Imported candidates remain visibly unreviewed.
- Roslyn comparisons for the foundation surface and selected expression shapes/spans; a compilation diagnostic proves the C# 8/9 target-typed-new version boundary.
- Test-only generated grammar compositions for generic calls and lambdas; membership changes accepted syntax.
- Test-only nested interpolation with expression children, whitespace-preserving text, and finite raw-string forms.

## Feasibility and coverage still open

The 205-entry feature inventory contains imported history candidates and baseline category entries. Only the foundation slice is verified; precise classification, dependencies, and boundaries require specification review. An inventory is not a completed parser.

Arbitrary raw-string delimiter matching is not demonstrated. Delimiter lengths 3/4/8 pass the finite probe; valid length 5 is retained as a counterexample accepted by Roslyn. No generated external-token matcher contract was found in the inspected grammar model/emitter. This is an unresolved approach, not proof of impossibility and not authorization to modify Nitrogen core.

Conditional preprocessing is not implemented. Reference tests pin directive-looking content inside strings/comments and malformed inactive branches; production rejects preprocessing requests honestly. The comment guard is limited to the currently supported production surface and must evolve before strings are added to production.

Full C# 1 declarations/statements/expressions, Unicode/escaped names, all later feature modules, external corpus validation, unavailable-feature identification beyond syntax failures, and the two complete-language customization demonstrations remain future milestones.

## Resource baseline

A separate VSTest testhost run with a 30-second hang timeout parsed balanced parentheses and rejected malformed tails at depths 16/64/128. One observed run reported approximately 0.177/0.139/21.672 ms and 33,888/47,904/128,392 allocated bytes, respectively. Order, JIT, language composition, and recovery affect these numbers; this is not a throughput claim.

## Review and decisions

A fresh reviewer found no important correctness issue in the bounded parser, and independently ran the existing 57 normal/29 oracle tests before the final coverage-metadata addition. The author then fixed the requested-coverage gap with a missing-property RED run and a 58-test GREEN suite. The final metadata change and final documentation did not receive a second reviewer pass.

Decisions made:

1. Work in the non-Git workspace; no Git initialization or commits. Cost: no commit-based recovery.
2. Build an isolated Nitrogen source snapshot rather than directly building sibling references. Cost: dependency refresh is explicit; source hashes record the actual inputs.
3. Keep raw-delimiter probes finite and test-only. Cost: later profiles remain partial while the arbitrary-delimiter gate is unresolved.
4. Treat requested-coverage metadata as a required completion fix despite the reviewer's minor severity. Cost if the inventory is wrong: requests remain unreviewed candidates; no completeness claim follows from them.

Deferred minor review findings:

- Dependency snapshot refresh is not transactional: an invalid source or interrupted refresh can replace a valid snapshot with an incomplete one. It needs source validation and staged publication; current build uses the verified prepared snapshot.
- Coverage-ledger tests do not yet validate its future requirement references/cycles. The runtime fixture-catalog tests do validate dependency integrity; the imported ledger currently has empty requirement lists and stays unreviewed.

## Continuation boundary

The approved foundation plan's runnable slice and selected probes are delivered. Its comprehensive inventory-review and arbitrary-lexing/preprocessing goals remain open. Do not treat the entire foundation plan or all fourteen grammars as complete.

Before later-version implementation expands, settle the lexical strategy using the retained raw-string counterexample and review the feature inventory. A separately scoped Nitrogen token-extension proposal is one option; a demonstrated grammar construction or source-preserving adapter is another. The C# 1 baseline plan must carry forward the proven C# trivia behavior and source-lifetime contract, and must include full preprocessing and its lexical boundaries. The next reviewable plan is [C# 1 lexical and preprocessing baseline](../superpowers/plans/2026-10-16-csharp1-lexical-preprocessing.md). It progresses the C# 1 subset independently of C# 11 raw delimiters. No new core interface or full-language implementation plan is silently approved by this report.
