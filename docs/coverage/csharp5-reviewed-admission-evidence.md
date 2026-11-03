# Reviewed CSharp5 normalized syntax admission

This independent admission builds on admitted CSharp4 (merged PR45 lineage) and the CSharp5 Task3 inputs merged in PR51–53. Earlier CSharp5 evidence documents record their partial stages; this admission supersedes their completion-state observations while retaining their measured behavior and limits.

## What is admitted

Only the named CSharp5 snapshot reports Complete/ProfileComplete. It requires the exact 32-module factory, the compilation-unit root, the reviewed source/owner/addition records, the selected policies and pinned production/dependency/evidence inputs. Source acceptance is independent: accepted and rejected documents share the admitted profile contract. CSharp1–4 stay admitted. CSharp6–14, the 7.x minors and arbitrary custom selections remain partial.

The primary is the Microsoft-hosted Version5.0 DOCX `6f12b6a3…`. Unicode4 scalar continuity remains the user's approved contract. The inherited CSharp4 exclusions carry forward: semantic binding and inference, general modifier compatibility and finite modifier sets, accessibility, explicit pointer type-argument legality, variance safety, constantness/default legality, and control flow/runtime. Two fifth-version semantic exclusions are added: awaitability, task types and async return types; and caller-info substitution. Duplicate `async` modifiers and the Version5 abstract/extern event-initializer sentence fall under the modifier-compatibility exclusion.

## Independent guard and reviewed records

`CSharp5CoverageContract` requires an actually admitted CSharp4, the exact approved ledger digest, and source/Unicode identity with both exclusions present. It also requires exact eligible-input membership and byte hashes (90 pinned inputs) and the selected 32-module factory configuration. On top of these it checks:

- all 458 occurrences and their reviewed overlay;
- each appendix owner, policy and mapping equal to its admitted CSharp4 row;
- the six main-text async/await additions with their omitted appendix occurrences and line hashes;
- 1164 linked witnesses (1161 through the CSharp4 matrix, 3 source-specific);
- 17 inheritance markers/135 references;
- 707 main-text dispositions and the pinned context report.

`CSharpParser.MatchesFifthFactory` separately checks the actual generated module identities and compilation-unit root provenance. The oracle audit derives owners from the parsed grammar resources of the actual factory.

The index (`indexed-unreviewed`), owner crosswalk (`owner-indexed-unreviewed`) and witness matrix (`source-witnesses-unreviewed`) never award their own approval. The reviewed overlay records each occurrence's basis: 455 carry the admitted CSharp4 review, the 3 changed bodies and the 6 additions cite `csharp5-normative-context-review.md`. The overlay binds the crosswalk, dispositions, matrix, predecessor overlay and report by hash.

Hash dependencies remain directed. The fifth ledger excludes CSharp2–5 contract code and requires admitted4; the fourth, third and second ledgers pin the fifth contract. Refresh with `eng/refresh-admission-pins.py` in the order 5, 4, 3, 2. A mutation of fifth contract code invalidates CSharp4 admission, and therefore CSharp5. No predecessor gate logic is weakened.

The `csharp-5.0.asynchronous-methods` release-history row becomes reviewed/verified. Caller info attributes and the foreach-variable change remain semantic exclusions, not implemented compiler features.

## Changes made while closing Task3

- `CSN2053` rejects async methods whose body is a semicolon (main6913). The historical partial-method check admits `async` from version5.
- Version5 shares the Version4 historical lexical/preprocessing/attribute readings its source repeats: Control-Z, directive whitespace, `#line` and later-directive effects, escaped underscore starts, conditional boolean spellings and the attribute-argument category. CSharp6+ keep current-compiler compatibility.

## Verification

`dotnet test CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` passes 11,419 tests: 7,481 main, 3,883 oracle and 55 scoped proof, with zero failures or skips. Of the focused contract tests, 48 cover the fifth guard: acceptance, predecessor dependency, actual factory identity, later/custom partial status, 44 drift/mutation families and directed contract pins. Completion assertions and both independent coverage-identity input enumerations were updated to include CSharp5. All extraction, comparison, disposition, matrix, crosswalk and overlay tools reproduce their artifacts with `--check`; the admission pins are current. These are finite source/structural/integrity observations, not exhaustive inputs, arbitrary-depth guarantees or a historical semantic compiler.

A fresh whole-branch review precedes authorized integration, as for CSharp4.
