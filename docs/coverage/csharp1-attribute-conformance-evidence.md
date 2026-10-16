# C# 1 attribute conformance continuation — 2026-10-16

The baseline was published publicly at [TovarishN/CSNitrogen](https://github.com/TovarishN/CSNitrogen), initial main commit `7359d6f`. This continuation implements remaining attribute checks from the approved source-grammar plan, Tasks 4/8. All profiles remain partial and no semantic C# compiler is claimed.

## Corrected behavior

- Positional arguments must precede named arguments within each attribute. Ordering state resets for the next attribute, including another attribute in the same section.
- Attribute argument values require a non-assignment expression at their root. Parenthesized and nested assignments still parse where the grammar permits them; whether they can be attribute constants remains semantic work.
- Attribute targets admit the C# 1 reserved keyword syntax as well as identifiers. Attribute target applicability is excluded: for example Roslyn warns and ignores an assembly target on a method.
- Escaped `@assembly`, `@module`, and Unicode-escaped global target names now produce compilation-unit global attributes instead of silently attaching the section to the following class. Formatting characters ignored by identifier equality, including their Unicode spellings, preserve global ownership. Longer identifiers such as `assemblyExtra` remain ordinary declaration targets.

`CSharpAttributePolicy` checks generated typed argument nodes, source preorder, and each containing Attribute node. It does not parse C# text or inspect bound values. Raw grammar starts must be followed by `CSharpSyntaxPolicy.Check`; the owning public API already applies it. Argument-order and expression-root diagnostics retain the offending authored spans.

`eng/generate-attribute-targets.py` generates finite global-target spelling rules and the standard C# 1 keyword target alternatives in Attributes.ngr. Pinned BMP formatting membership lives in `tests/Corpus/attribute-target-formatting.json` and is compared exactly with .NET 10 category data. Target ownership is separately compared with Roslyn syntax. Generation is deterministic and idempotent. The Nitrogen core remains unchanged.

## Reference and oracle classification

The [attribute specification, §23.3](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/attributes) defines positional-before-named ordering, a non_assignment_expression for attribute values, keyword target syntax and identifier spelling equality. Its modern constructor argument-name syntax is excluded from the C# 1 baseline.

Pinned Roslyn reports argument ordering as compilation diagnostic CS1016, even when syntax-tree diagnostics are empty. The new oracle tests classify that explicitly alongside CSharp1 feature errors. Roslyn 5.0.0 parses direct assignment expression arguments without a syntax/version error; those two named fixtures assert its assignment-expression tree while CSNitrogen enforces the specification's root restriction. This discrepancy is explicit, rather than treating arbitrary compiler diagnostics as parser failures. Parenthesized assignment and attribute target applicability remain outside semantic validation.

## Verification

Retained RED evidence: six public-parser acceptance discrepancies; three global typed-owner discrepancies after a clean regeneration. A stale-artifact observation from an input edited during a build was discarded; subsequent checks use serialized source changes/builds and a clean rebuild.

Pre-review full suites: **325 normal tests and 246 oracle tests passed**. Includes 25 shared attribute cases, five typed global-owner cases, all BMP formatting characters in raw/short-escape/long-escape forms, target-prefix boundaries and malformed/recovered attribute input. Final verification and review results are appended after the review pass.

CoverageIdentity automatically includes the new production policy and modified grammar through existing embedded resource globs. The attribute inventory entry remains implemented, with these obligations recorded as targeted evidence; exhaustive C# 1 production closure and later C# 2–14 modules remain pending. A passing attribute fixture suite does not close the whole-language gate.

## Final review and verification

One fresh read-only review found no Critical or Important issue. One Minor documentation update is deferred: the source-production map should add the two conformance fixture classes and the two new argument policies; this report and the feature ledger already record them.

A clean `dotnet build CSNitrogen.slnx -t:Rebuild -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` succeeded with zero warnings/errors. Full `dotnet test CSNitrogen.slnx --no-build -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s` passed **327 normal tests + 246 oracle tests**, zero failed/skipped. Logs: `.superpowers/sdd/csharp1-attribute-conformance/pre-review-build.log` and `pre-review-tests.log`.

All 118 recorded Nitrogen source hashes still match both isolated snapshot and sibling source. Sibling dirty state remains ` M docs/editor-support.md`; no sibling files were changed. Production runtime dependency manifests contain no Microsoft.CodeAnalysis package.
