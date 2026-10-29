# Scoped context runtime experiment

THROWAWAY internal prototype, not a Nitrogen feature or CSharp5 implementation. User authorized this experiment on2026-10-29. The concrete production design is [the scoped-context proposal](../../docs/superpowers/specs/2026-10-29-scoped-parse-context-design.md).

## Reproduce

Base Nitrogen commit: `ed69ac8aaa3bbc7c8d44c97cf969d1b6ddd89ea2` (0.19.0 checkout inspected2026-10-29). Patch SHA256: `8d4c321644347e655d390e532cf69310428adeed545fbf24df9af5bf32b0a630`.

Create a separate clean clone of Nitrogen, check out that exact commit, and apply `2026-10-29-context-spike.patch` with `git apply`. Do not apply to an unrelated dirty checkout. In that clone:

```sh
dotnet test probes/ContextSpike/ContextSpike.csproj -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

This restores the test packages and runs the small harness. The patch changes five runtime files, adds one internal partial implementation and a two-file test harness. It introduces no public context API, grammar surface, generator support, dependency repin or installed tool change. The test harness borrows the existing Nitrogen.Tests friend assembly name to exercise internal state; it is separate from the full Nitrogen test project and links only its14 existing ParserStateTests. Do not treat this harness as the full Nitrogen suite.

Verified here using .NET SDK11.0.100-preview.2.26159.112, targeting net10.0. Preview SDK informational message remains; no compiler warnings in the final replay. The patch was applied to a second clean clone and the test command reproduced22 passed, zero failures/skips.

## Observed evidence

- Scoped helper plus context-blind caches: five failing cache/reuse witnesses, two passing scope/isolation checks. See initial-cache-red.txt.
- Context added to memo and reuse lookup; targeted descendant-carry mutation then failed one of eight context tests. See descendant-carry-red.txt. A moved enclosing graft must retain its descendants' own contexts; the enclosing environment is insufficient.
- Full patch: eight context witnesses plus14 existing parser primitive tests pass. See fresh-replay-green.txt, reproduced from the saved patch in a clean clone.

The generic synthetic language has marker a/n and identical payload x; its two owners depend on the marker. It exercises real ParserState/ExtensibleRules memoization and Language.ParseIncremental/Reparse. It is not a C# parser. Tests cover same-position success/failure entries, high context bits and growth, nested overrides/restoration on failure/exception, parallel parse isolation, equal-length marker edits with unaffected reuse, moved grafts and descendant carry-over over repeated reparses. They compare full/reparsed node kinds, spans and parents.

## Limits and next gates

The spike does not prove generated grammar integration, CSharp5 owner/field compatibility, scoped grammar extraction/diagnostics, lookahead/recovery correctness, quotation behavior or performance. It adds no language to nitrogen.json because it introduces no authored DSL or generated grammar; the harness is low-level runtime test code. The full Nitrogen suite and CSharp1–4 admission were not rerun for this isolated experiment, and CSNitrogen production/dependency inputs are unchanged. These remain mandatory before production integration.

Existing parser APIs and ParseMark's public shape are preserved. Memo/reuse entries include the environment, at an anticipated memory cost; no throughput or allocation benchmark was performed. Do not describe the scoped mechanism as production-ready from22 tests.

## Design calls

The designer selected an initial64-Boolean environment with collision-free per-Language identities and fail-closed capacity, rather than arbitrary mutable state. Internal scoped calls restore in finally; generated wrappers must be transparent and must not add tree nodes/captures. Full incoming context keys both caches, including negative memo entries; descendant context survives graft carry-over. Standalone parses/quotes start false and non-default categories need explicit scoped start rules. These are proposed additive contracts, not previously chosen syntax by the user.

The app's managed worktree tool rejected the sibling sourceRoot because it is outside this chat's registered folders. Experiments used temporary local clones instead; the original Nitrogen checkout and its four untracked design drafts were preserved. Unrelated CSNitrogen benchmarks were also untouched. No sandbox auto-approval review rejection occurred.

## Independent review

The concrete design builder check found no important unanswered planning questions. A separate fresh-context code reviewer inspected the complete CSNitrogen branch through43d6dbc and the patched runtime/test sources, checksum and RED/GREEN logs. No Critical/Important findings within this bounded scope; approved for draft publication pending concrete production-design approval. Review execution was read-only and did not independently rerun tests.

Deferred minor: MemoTable's XML summary still describes a context-free24-byte cache and ReuseTable's summary omits context in the identity. The implementation and this report include context correctly; update those internal comments when preparing production work.

Reviewer set-aside boundaries remain gates, not inferred successes: public declarations/binding/capacity/generated wrappers; actual CSharp5 owners/fields/spelling/quotations/admission; lookahead/recovery/full compatibility; performance/allocation; unrelated benchmark/draft preservation; and revalidation of historical reference observations. The implementer checked the original sibling's unchanged status in-session, but the reviewer did not independently inspect it or the historical witnesses. No production-readiness claim follows from this review.
