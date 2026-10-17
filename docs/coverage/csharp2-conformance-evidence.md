# CSharp2 conformance closure evidence

PR16 merged at f9d9494. Closure branch: codex/csharp2-conformance. The user approved the generated type-operand revision after the isolated Nitrogen prototype passed eight cases; see the decision and migration notes. The closure admits CSharp2 only after its independent source/factory/policy contract succeeds. CSharp1 retains its separate contract; CSharp3–14/custom remain partial.

## Verification

Final command:

```sh
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
```

Result: **1,197 normal +3,317 oracle =4,514 passing tests**, zero failures/skips and no warnings. Local full output: /tmp/csn-csharp2-closure-final.log. The merged baseline was 925+3,270=4,195 passing tests. New focused regressions first reproduced the Unicode, alias, iterator, declaration, directive and nullable failures; no failing source witness was discarded. The former A::F<int>(x) positive is retained as a historical negative with A::T.F<int>(x) as its valid neighbor.

Independent read-only audits covered lexical/preprocessor, expression/type, statement/iterator and declaration/attribute source domains. Subsequent independent reviews found and closed nullable-constructor scope and input-set/composition metadata guard gaps. Final re-review found no further actionable defects. The source audit tests parse actual Nitrogen grammar ASTs, resolve source owners and finite witness references, and validate factory membership independently of runtime admission. Source/metadata and runtime mutation suites cover missing/changed occurrences, bodies, reviews, targets, witnesses, source/grammar/review inputs and composition root/membership.

The pinned pypdf6.10.0 extractor reproduces the 409-occurrence source index byte-for-byte from the pinned ECMA PDF. Unicode4 table/grammar, contextual identifier grammar and global-target generators reproduce byte-for-byte. `git diff --check` passes. The primary source, Unicode data/license and fixed-buffer supplement pins are recorded in the reference policy and provenance files.

All123 isolated Nitrogen source hashes match the dependency manifest. All118 non-project artifacts match the original sibling; no Nitrogen core change was made. The sibling's pre-existing docs/editor-support.md modification remains unchanged. Production CSNitrogen and CSNitrogen.Syntax dependency manifests contain neither Roslyn nor Nitrogen.Grammar.

## Scope and compatibility

The source contract covers normalized generated syntax and declared source-only context policies, independently of whether an input is accepted. Binding/type/value legality, compiler lowering and execution remain excluded, as detailed in csharp2-conformance-review.md. Modern Roslyn is a secondary executable reference; historical discrepancies remain explicit. The generated IsExpression.Type/AsExpression.Type API and CSharp1 wrapper change are explicitly approved and documented in csharp2-type-operand-migration.md.

This evidence establishes the reviewed parser snapshot; it does not claim CSharp3–14 language completeness, arbitrary runtime custom composition or a semantic compiler.
