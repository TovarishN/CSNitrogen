# CSharp3 source audit continuation

Base: PR20 merged as `982296113e73861e84dd9f3fe7c375ef180c08c1`; its tree exactly matches tested feature head `bb78c393c38dcb0360a27143ead3cf3235ad37fa` (`57731a365408d40d955b1908e4c8860cfdcbbaea`). All 4,851 tests passed before merge.

## Fixed buffer element types

The pinned CSharp3 source §18.7.1 main7998 / Appendix8869 spells buffer-element-type as type. Prose main8002 requires denotation of one of twelve predefined primitive types. That does not require keyword spelling: `System.Int32`, a using alias to it, or an alias-qualified name can denote the same type. CSharp2 retains its narrower reviewed VS2005-supplement spelling policy.

Three named-element witnesses initially failed with CSN2010. CSharp3 now defers named element denotation to the semantic consumer while still rejecting known nonprimitive builtin types, pointer/nullable/array shapes, non-struct owners and static declarations. Generic names share the deferred named-type category; acceptance does not assert they resolve to an allowed primitive. No binding/type inference is implemented. Original generated type nodes and APIs remain unchanged.

The initial proposed `using I=int` fixture was invalid CSharp3 syntax. It was corrected to `using I=System.Int32`, and the modern-only form is retained as a negative witness. Independent Roslyn compilation accepts all three corrected named primitive examples. The normal source-delta suite passes all 29 cases.

## Licensed historical external corpus

The manifest at `tests/Corpus/Mono2008/provenance.json` pins Mono revision `d7a4c6473ed2cd096c9223519c378bf136912a97` (2008-12-30), exact source/license hashes and selection procedure. Before fetching or parsing, select the first three non-library test paths from each of seven groups: LINQ, initialization, implicit arrays, anonymous types, collection initializers, auto-properties and extension methods. Implicit arrays have only two, yielding twenty fixtures. All twenty were retained and accepted by Nitrogen CSharp3 and Roslyn5 CSharp3 syntax parsing on the first completed corpus run. This is finite syntax evidence; no historical compiler reproduction, dependency compilation or execution claim.

The original compiler license files and source notices are retained beside parse-only fixtures. Provenance/license integrity and every selected source are executable tests. The existing Mono2004/Mono2006 fixtures remain intact.

## Source-authority discrepancy

Comparison of the 454 CSharp3 indexed occurrences with the CSharp2 source identified three generic-member suffix omissions in both main text and Appendix B. Current CSharp2/CSharp3 factories and Roslyn's language modes accept all three complete witnesses with expected source spans. No generic-member behavior was changed. The concrete alternatives and source lines are in [the proposed authority decision](../superpowers/specs/2026-10-18-csharp3-generic-member-source-decision.md).

This slice does not promote any unreviewed source-index row or establish a full occurrence crosswalk. CSharp3 remains partial pending the authority choice, continued normative audit and separate fail-closed admission gate.

Review found that the initial corpus integrity test could miss duplicate selection entries or removed license entries. The same disposable-output mutation passed the original check and failed after hardening. The test now independently pins the whole manifest and asserts exact source/local/license paths; pristine output was restored before full verification. The reviewer confirmed resolution with no remaining actionable finding in this bounded slice and agreed the generic-member authority conflict needs an explicit disposition.

Validation: full `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` passed 1,442 normal + 3,446 oracle = 4,888 tests, zero failures/skips. Source-index `--check` reproduces 454 occurrences / 425 names, all 123 pinned dependency source hashes match, and `git diff --cached --check -- . ':!tests/Corpus/Mono2008/**'` passes for authored changes. The exact imported source/license bytes retain upstream whitespace, so the unrestricted whitespace check reports those original lines.
