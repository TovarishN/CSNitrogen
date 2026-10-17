# CSharp2 Additive Grammars Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans inline; continue all authorized tasks without intermediate approval checkpoints.

**Goal:** Implement source-grounded C#2 syntax as additive Nitrogen modules while preserving CSharp1 and honest partial coverage.

**Architecture:** Audit the pinned fourth edition first. Prove typed extension seams before production generics, then add the remaining bundles with explicit versioned capability selection. Keep completeness closed until an independent CSharp2 source/factory/policy gate exists.

**Tech Stack:** Existing .NET10/C#14, pinned Nitrogen generator/runtime, xUnit and test-only Roslyn.

**Spec:** docs/superpowers/specs/2026-10-17-csharp2-additive-grammars-design.md (user approved).

## Global Constraints

- No sibling edits, parser delegation, runtime arbitrary custom factories or semantic compiler.
- Retain existing ordinary typed field contracts, explicit dependencies and CSharp1 source/policy gates.
- CSharp2 and later/custom profiles remain partial during staging.
- Syntax is authored in .ngr; policies only enforce syntax/context restrictions.
- Stop for a demonstrated mechanism limitation requiring core or public contract redesign.

## Review Focus

- Generic-versus-relational follower selection and nested closers must retain shift behavior.
- Empty extension fallbacks must not swallow malformed syntax or change baseline typed trees.
- New alternatives must participate in assignment/postfix/cast/context policies.
- Named profiles must select capabilities by minimum version, not all implemented capabilities.
- Metadata completion cannot be inferred from finite oracle/corpus success.

## Task 1: Historical reference and delta audit

Files: create docs/coverage/csharp2-reference-policy.json and csharp2-delta-audit.md; add reproducible fourth-edition extraction support in eng only as needed.

- [x] Download the fourth-edition PDF to ignored reference storage; record SHA256/title/URL and extraction command. Inspect relevant normative prose/grammar and compare with first-edition contract.
- [x] Record named deltas, lexical/directive differences, semantic exclusions and module ownership candidates. Mark all implementation/review obligations pending.
- [x] Verify hash/extraction reproducibility and commit the audit with this plan. No production coverage claims.

## Task 2: Extension seam feasibility

Files: tests/CSNitrogen.Tests/Grammars/CSharp2SeamProbe.ngr and Syntax/CSharp2SeamTests.cs; use existing test-only generator integration.

Consumes: pinned Nitrogen generator and .ngr extensible syntax. Produces: executable baseline/loaded/omitted seam witnesses and typed compatibility observations.

- [ ] Write a failing test expecting generic-header syntax through an optional typed seam, plus baseline identifier projection and omitted-feature rejection.
- [ ] Observe failure before adding the probe extension; then implement optional extension/fallback and generic argument/declaration witnesses in test grammar.
- [ ] Prove ordinary typed fields, malformed fallback rejection, nested generic closers, relational followers and shifts under paired compositions. If the pinned mechanism cannot satisfy the design, retain the evidence and report the concrete limitation.
- [ ] Run focused tests and full existing suite; commit the independent feasibility result.

## Task 3: Production generics vertical slice

Files: src/CSNitrogen.Syntax/Grammars/Generics.ngr and narrow seams in NamesTypes/Declarations/Expressions/Attributes; nitrogen.json; FeatureCatalog/ProfileResolver and syntax policies; tests normal/oracle CSharp2GenericsTests; features.json.

Consumes: proven seams and source disambiguation rules. Produces: csharp2.generics capability, real CSharp2 compilation-unit parsing and typed parameters/arguments/constraints.

- [ ] Write public failing generics/constraint/method/type/typeof fixtures and preceding-version/omission checks; run RED.
- [ ] Introduce shared typed seams and additive alternatives; preserve ordinary QualifiedName.Parts and existing declaration projections. Include full syntactic generic follower rules and delimiter spans.
- [ ] Add minimum-version selection without changing FeatureDefinition positional constructor compatibility; register production module in CSharp2+ factories only. Explicit custom dependency validation remains unchanged.
- [ ] Integrate new syntax alternatives with cast/postfix/assignment/context policies; exercise typed trees and authored spans against matching Roslyn version with classified discrepancies.
- [ ] Run focused and full CSharp1 regression gates; update pending coverage evidence and commit.

## Task 4: Remaining audited CSharp2 deltas

Files: additive NullableValues/AnonymousMethods/Iterators/DeclarationForms/AliasQualification/DefaultValues grammar modules, audited extra modules/policies, configuration/catalog and corresponding normal/oracle tests.

Consumes: baseline + required earlier bundles. Produces: source-owned capability mappings for implemented deltas; partial named profiles with honest remaining inventory.

- [ ] For each bundle, write positive/malformed/preceding-version/omission/typed-span failing fixtures; observe RED before production changes.
- [ ] Implement the smallest module and policy integration; preserve contextual identifier usage and later-only rejection.
- [ ] Audit source-derived additions such as fixed buffers and pragma policy separately, retaining semantic exclusions and unsupported witnesses.
- [ ] Run focused tests per bundle and full gates before each coherent commit; record measured evidence.

## Task 5: Whole milestone review and publication

Files: source-review/coverage evidence, README and reviewed CSharp2 gate only if all normative obligations actually close.

- [ ] Review all source obligations, policies, version availability, actual factories and dependency/corpus identities. Keep CSharp2 partial if any obligation remains pending.
- [ ] Run full solution tests and dependency/corpus preservation checks; obtain one fresh independent whole-branch review and reproduce/fix material findings.
- [ ] Commit/push on the current codex branch, publish a reviewable PR with explicit implemented/remaining scope, attach it and verify remote head/clean state.

Ruling: user approval plus the standing instruction to continue until a real decision authorizes inline execution; no additional plan/execution-method approval is needed. Existing clean codex branch is retained in the authorized workspace rather than creating another checkout.
