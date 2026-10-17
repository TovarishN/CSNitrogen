# Canonical Coverage Mappings Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans inline. Execute all tasks without intermediate approval checkpoints.

**Goal:** Keep canonical capability bundles, explicitly map historical coverage identities to them, and make named-profile coverage/completeness consistent with the actual factory and policy contract.

**Architecture:** Add immutable inventory-to-capability/policy mappings to the existing hashed feature ledger. Resolve named coverage through those mappings; custom selections remain canonical and dependency-explicit. Validate source-review state and exact named factory/policy membership before CSharp1 completeness; later/custom profiles stay partial.

**Tech Stack:** C# 14/.NET 10, Nitrogen generated factories, System.Text.Json, xUnit. No new packages or Nitrogen core changes.

**Spec:** docs/coverage/csharp1-feature-catalog-decision.md, recommended path selected by the user's “keep” and “continue”.

## Global Constraints

- Retain baseline.* and foundation.empty-classes capability identities and explicit dependency validation.
- Coverage IDs are reporting identities; do not register them as independently selectable/removable syntax.
- Preserve original source coordinates, typed syntax APIs, semantic exclusions and historical/current differences.
- No arbitrary custom executable factory or grammar module split in this change.
- Keep later named profiles and custom metadata resolutions partial. CSharp1 may report complete only with verified source/coverage/factory/policy agreement.
- No sibling Nitrogen edits; update the existing PR only after full tests and independent review.

## Review Focus

Missing/duplicate/empty mappings; unknown capability or policy; missing mapped capabilities; factory/policy drift; source-review or feature-status regression. Do not equate inventory IDs with selected capability IDs, or infer fine-grained removability from shared grammar files.

## Tasks

### 1. Explicit mappings and coverage evaluation

- [x] Add failing public tests for CSharp1's forty requested inventory identities being covered by nineteen canonical capabilities, and contextual rejection of a coverage-only custom selection.
- [x] Add a CoverageMapping record and immutable mappings loaded from the hashed features ledger. Map all forty first-edition syntax entries explicitly; later entries remain unmapped.
- [x] Add capability policy declarations without changing existing positional record constructors/dependency validation. Evaluate required capabilities/policies and named factory agreement; reject malformed mappings fail closed.
- [x] Verify focused tests and mutation cases; commit this coherent capability contract with the coupled completeness changes.

### 2. Named completeness and gate consistency

- [x] Add failing tests for CSharp1 complete status, later/custom partial status, rejected source versus profile completeness, and source/factory/policy/mapping drift.
- [x] Read reviewed source metadata and generated factory composition from existing embedded resources. Implement checked completeness for CSharp1 only; carry the resolved Complete value into ParseDocument without re-resolving it.
- [x] Close runtime-profile-contract metadata only when actual mappings and factory/policy agreement pass. Update prior characterization tests to explicit negative mutations and preserve anti-bypass tests.
- [x] Run full solution tests; preserve dependency/corpus hashes and production dependency boundaries.

### 3. Evidence, review and publication

- [x] Update the approved decision, README and living coverage evidence to distinguish complete CSharp1 syntax coverage from semantic validity and later/custom completeness.
- [x] Request one fresh independent code/contract review. Fix important findings with reproductions and rerun affected/full checks.
- [x] Commit/push to the current continuation branch, update PR #15 title/body around final scope, verify remote head and clean checkout.

## Builder rulings

Coverage mappings live in features.json to reuse the existing identity binding. Mapping policy requirements use the gate's declared policy-group identities; version-specific Unicode policy remains separately selected by named profile. Custom feature catalogs supplied explicitly by callers keep their existing validation behavior. Completeness describes the reviewed syntax profile independently of whether a particular input is accepted.

Execution ruling: tasks 1 and 2 share embedded mappings, source/factory validation and public completeness assertions, so they are committed together as one reviewable contract. Final verification: 4,044 tests passed; independent review completed and both robustness findings fixed.
