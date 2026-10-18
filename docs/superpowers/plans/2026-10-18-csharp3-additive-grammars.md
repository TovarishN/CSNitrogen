# CSharp3 Additive Grammar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prove and then implement bounded CSharp3 syntax additions without weakening reviewed CSharp1/CSharp2 contracts.

**Architecture:** Shared real Nitrogen modules with additive feature compositions and explicit context policies. First use test-only registration of the 19 production grammar inputs in a separate generated namespace; production remains unchanged until that proof and source design are reviewed.

**Tech Stack:** .NET10/C#14, pinned Nitrogen snapshot, test-only Roslyn5.0.0, historical Microsoft C#3 DOC pinned by SHA256.

**Spec:** docs/superpowers/specs/2026-10-18-csharp3-additive-grammars-design.md

## Global Constraints

No sibling Nitrogen writes, handwritten C# parser, semantic compiler or arbitrary runtime custom factory. Preserve existing typed field names/meanings and original UTF16 spans. CSharp1/CSharp2 completeness must remain true; CSharp3+ remain partial until separate source admission. Any material API/core mechanism change requires a concrete revised design decision. Record failing witnesses rather than dropping them.

## Task1: historical source and seam proof

- [x] Confirm PR17 merge and merged 4514-test baseline.
- [x] Pin official Microsoft C#3 DOC and inspect source clauses 7.5.10,7.14,7.15,8.5.1,10.2.7,10.6.9,10.7.3.
- [x] Inventory source-only syntax additions; record implicit arrays absent from history and lock duplicate; separate var inference and expression trees from syntax claims.
- [x] Add failing lambda/query/initializer/anonymous/array fixtures against merged CSharp2 grammar.
- [x] Add test-only .ngr compositions under tests/CSNitrogen.Tests/Grammars/CSharp3 and narrowly include them in the normal test generator namespace CSNitrogen.Tests.CSharp3Probes.
- [x] Prove signatures, precedence, expression/block lambda bodies, typed query headers/clauses/continuations and initializer choices in real compilation units; assert typed children and spans.
- [x] Prove independent omission (queries without lambdas, lambdas without queries), contextual names and ordinary baseline shapes.
- [x] Record raw Pratt/category over-acceptance as explicit future policy obligations; add malformed/deep witnesses and compare selected observations with pinned Roslyn CSharp3.
- [x] Run focused/full tests; independent review; publish source/design/proof evidence. This task alone does not promote feature status.

## Task2: lambda and query production slices

- [x] Implement lambda signatures/body nodes through Expressions extension, explicit introduction3.0 and required baseline capabilities; positive/negative/version/omission/span gates.
- [x] Add source-category checks for low-precedence anonymous functions without inferring delegate types; preserve earlier composition observations.
- [x] Implement query clause/binding/body/continuation module independently from lambdas; prove contextual disambiguation against historical source and query omission; no LINQ binding/translation claim.
- [x] Add CSharp3+ explicit factory membership/feature mappings, keeping profiles partial and refreshing reviewed CSharp1/CSharp2 source hashes only after regression review.

## Task3: construction and declaration slices

- [x] Add object/collection initializer, anonymous creation and implicit-array modules; typed lists/delimiters, nested initializer/category and inferred-member restrictions.
- [x] Prove optional extension receiver seam without changing existing FixedParameter fields/absent tree observations; enforce historical receiver context restrictions.
- [x] Add partial-method and auto-property source policies using existing spelling/tree ownership; retain var/type and method-pair binding exclusions.
- [x] Run canonical capability requirements, preceding-version and omission checks; update inventory with exact mappings and source evidence.

## Task4: historical conformance and integration

- [ ] Audit full CSharp3 lexical/Unicode/directive/source grammar contract and historical/modern differences; index exact source occurrences with reviewed owners and licensed external corpus.
- [ ] Add a separate fail-closed source/factory/policy completeness gate only after all obligations close.
- [ ] Run full solution, generated source reproduction, dependency/runtime-boundary checks and independent review; publish reviewable PRs.

Task1 evidence: [source and seam proof](../../coverage/csharp3-seam-proof-evidence.md). Tasks2–3 are implemented; [production evidence](../../coverage/csharp3-production-evidence.md) records tests/review and remaining Task4 admission work. A [Unicode contract decision](../specs/2026-10-18-csharp3-unicode-contract-decision.md) is required before historical lexical admission.
