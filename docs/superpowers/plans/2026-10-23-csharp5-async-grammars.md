# CSharp5 Async Grammar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this continuation inline. Steps use checkbox syntax for tracking.

**Goal:** Add source-defined async functions and await expressions through typed Nitrogen grammar composition, preserving contextual identifier meaning and prior APIs.

**Architecture:** Start with the admitted30-module CSharp4 factory. Prove optional anonymous-function markers and method modifiers in test-only copies of AnonymousMethods/Lambdas, plus a separate await expression module. Explicitly test the same await token sequence inside/outside async functions before selecting a production mechanism. Existing APIs and source positions are binding requirements; a required core mechanism or breaking public property type returns for a concrete decision.

**Tech Stack:** .NET10, pinned Nitrogen snapshot, Roslyn5.0.0 syntax observations, Microsoft Version5 DOCX6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519.

**Spec:** docs/superpowers/specs/2026-10-23-csharp5-source-contract-decision.md. Unicode4 scalar continuity approved2026-10-23.

## Global Constraints

Preserve CSharp1–4 admission and unrelated benchmarks. No sibling Nitrogen writes, handwritten C# parser or semantic/compiler/runtime claim. Preserve existing parameter/body/signature generated property types and original UTF16 coordinates. No fifth-version source admission from a bounded proof. Keep later/custom profiles partial. Record actual source/appendix disagreements rather than copying stale summaries.

## Review Focus

- `await(M())` outside async must retain InvocationExpression, while the same spelling inside async must have AwaitExpression with the authored operand.
- Ordinary async/await identifiers, escaped/verbatim spellings and supplementary/escaped identifier suffixes must not become accidental keywords.
- Async function boundaries must distinguish methods/lambdas/anonymous methods and nested non-async functions; a global keyword switch cannot classify mixed files.
- Optional marker captures must preserve old Parameters/Body fields, absence and typed owners when omitted independently from await expressions.
- Catch/finally/lock/unsafe/ref-out restrictions and query placement need source-local dispositions; awaitability/task identity/state machines remain semantic exclusions.

## Task 1: approved source and test-only typed seam proof

**Files:** docs/coverage/csharp5-reference-policy.json; tests/CSNitrogen.CSharp5.Tests/Grammars/*.ngr; tests/CSNitrogen.CSharp5.Tests/CSharp5GrammarSeamTests.cs; tests/CSNitrogen.CSharp5.Tests/CSNitrogen.CSharp5.Tests.csproj; CSNitrogen.slnx; docs/coverage/csharp5-seam-proof-evidence.md.

**Interfaces:** consumes exact CSharp4 composition grammar paths and the approved Unicode contract; produces generated CSharp5NoneProbe/AllProbe factories in CSNitrogen.CSharp5Probes. Existing parameter/body/signature captures retain types; Async marker is additive/optional. No production factory change.

- [x] Record approval and source clauses4430–4437,5174–5175,7800–7802; retain the appendix omission explicitly.
- [x] Add the30-module none-probe and assert desired async/await witnesses before adding extensions. Expected: source-defined additions fail while ordinary baseline syntax passes.
- [x] Add optional marker seams in the two test copies and independent function/await modules; assert typed children, absent fields, original spans and independent omission. Expected: grammar construction passes without a core/API change, or exact blocking diagnostic is retained.
- [x] Test paired `await(M())` invocation versus await expression in ordinary/async methods, including mixed files and nested functions. Expected: each authored context yields the right typed owner; a uniform wrong tree is a failed contract, not acceptance evidence. Result: desired contract RED in three ordinary contexts; observation harness and decision proposal retained as the explicit fallback below.
- [x] If existing composition cannot express that distinction, retain a passing observation harness plus the desired-contract RED evidence and write a concrete API/core/source decision proposal before dependent production work.
- [x] Run focused proof, independent syntax observations and full solution; verify predecessor admission and generator/dependency stability. Commit and obtain one fresh whole-branch review before publishing/merging the bounded slice.

## Task 2: production async capabilities and approved Unicode5 selector

Consumes the reviewed Task1 mechanism. Produces independently selected fifth capabilities and source-local policy. This task cannot start while a Task1 core/API decision is unresolved.

- [x] Write production RED cases for methods/anonymous functions/await, contextual identities, mixed files, nested function boundaries and restriction/span/incremental behavior.
- [x] Apply only the proved seams/mechanism; register real grammar files in production nitrogen.json and canonical capabilities with explicit dependencies. Preserve predecessor fields and CSharp1–4 admission.
- [x] Extend the frozen Unicode4 selector to5 under the approved contract, with scalar/format/escaped/contextual boundary witnesses and historical lexical/directive replay.
- [x] Classify caller-info compiler substitution as semantic where source-supported; syntax recognition alone must not claim implementation.
- [x] Refresh fourth/third/second input pins in directed order, verify full gates/generators/dependency inputs and obtain fresh branch review before authorized integration. CSharp5 remains partial.

## Task 3: fifth historical conformance and separate source admission

- [ ] Independently index appendix occurrences and explicit controlling main-text additions omitted by the summary; retain source provenance and every normalization/exclusion.
- [ ] Review all fourth-to-fifth main-text/context differences; link and execute every source alternative and inheritance marker with positive/negative/span/context evidence.
- [ ] Audit actual selected-factory owners, policies, omissions and ordinary contextual identifiers; resolve real source/API/core decisions with concrete witnesses.
- [ ] Only after closure implement an independent fail-closed CSharp5 admission guard requiring admitted4, exact source/factory/policy/evidence inputs and later/custom partial status. Full verification and fresh review precede authorized merge.
