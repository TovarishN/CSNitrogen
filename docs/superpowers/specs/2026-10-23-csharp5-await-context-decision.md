# CSharp5 await context mechanism decision

Status: proposed; user decision required before sibling-core work or a different public tree contract. Unicode4 scalar continuity is already approved and is not reopened.

## Concrete contract failure

The same tokens await(M()) must yield InvocationExpression in an ordinary method and AwaitExpression with operand (M()) in an async method, including both methods in one file. The bounded global extension fails three of eight context-owner witnesses. The passing observation/reference harness and typed seam evidence are in docs/coverage/csharp5-seam-proof-evidence.md and tests/CSNitrogen.CSharp5.Tests. No production factory has changed.

Microsoft Version5 main4430–4437 distinguishes contextual recognition from permitted usage. In particular, the pinned reference still recognizes raw await as an operator in a non-async delegate nested within an async method; the source forbids its use there separately. Catch/finally/lock/unsafe, query placement and ref/out restrictions need source-local validation. They cannot be handled by a whole-file keyword switch. @await and Unicode-escaped await must retain invocation identity in the observed async witnesses; the existing shared Contextual helper is insufficient.

## Recommended direction: scoped Nitrogen parsing context

Authorize a separately reviewable Nitrogen design and prototype, then integrate only after its gates pass. Keep the existing generated Parameters/Body/signature property types and real InvocationExpression/AwaitExpression owners. Express lexical async context at grammar boundaries and keep nearest-function/use restrictions distinct. Context must participate correctly in memoization, incremental reuse, recovery and nested re-entry; no ambient global mutable flag, source rewrite or handwritten C# parser.

This authorizes work in the sibling Nitrogen repository, which the current CSNitrogen scope does not cover. It does not approve an unspecified public core API: write and review the concrete design before implementation, returning for a decision if it requires a breaking API or altered tree contract. Cost: reusable core design/runtime/generator work plus dependency repinning and predecessor gates; delivery takes longer than adding a local syntax alternative. Benefit: composition can preserve the existing C# tree contract without cloning the full expression/statement graph. No claim is made that the current experiment proves all existing Nitrogen alternatives impossible.

Acceptance must reproduce mixed ordinary/async files, methods/lambdas/delegates and nested lexical-versus-legality boundaries; verbatim/escaped spelling and suffixes; typed owners/fields/UTF16 origins; omission; recovery; and incremental edits that enter/leave async context without stale memoized trees. CSharp1–4 admission must survive. Task2 remains gated until that evidence exists.

## Alternative: investigate a separate async grammar family

Keep Nitrogen core unchanged and prototype context-specific expression/statement/function rules in CSNitrogen. This can require duplicating a recursive grammar graph and introducing generated async node families or wrappers; preserving the old public property types is not established. Cost: substantially more grammar variants, owner mappings and migration surface. Choose this if avoiding core work outweighs those costs; any resulting public API/tree change requires a concrete follow-up approval, not blanket acceptance now.

## Decision requested

Approve the recommended scoped-context design/prototype in Nitrogen, or choose investigation of the separate async grammar family in CSNitrogen. Neither option admits CSharp5 or licenses treating uniform wrong trees as complete.
