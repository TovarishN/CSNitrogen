# CSharp5 scoped production review

The fresh whole-branch review covered `5385c7b..4f5e04a`. The initial reviewer invocation failed at an account usage limit without findings. After ordinary account usage became available, one replacement fresh reviewer completed the review; no second opinion or re-review was commissioned.

The reviewer independently ran 103 fifth-version tests and verified the 141 dependency inputs, admission-pin reproducibility and 12 recorded generator outputs. It found one Important syntax-acceptance defect, no Critical issues and no Minor issues. Its verdict was ready with fixes.

## Finding and author verification

`await x=>x` and `await async ()=>1` were accepted even though source line4430 requires `await unary-expression`. The shared third-version operand-category check did not include `AwaitExpression` among its unary parents. The author added three direct lambda/query rejection cases with exact UTF16 diagnostic spans and three parenthesized acceptance controls. The direct cases failed before the fix, while all three controls passed. Adding the missing parent category made all six pass. Parenthesized syntax acceptance makes no awaitability or type claim. The post-fix full solution gate passed 10,097 tests (6,254 main,3,788 oracle,55 scoped proof), with zero failures/skips.

The directed fourth/third/second admission pins were refreshed after the policy change. The pre-fix measurement artifacts retain their exact candidate revision; they are not measurements of this later policy edit.

## Rulings on declined review scope

- Nitrogen cache identity, reuse propagation, compiler analyses and context validation retain the separately reviewed PR94 evidence. This review verifies their CSNitrogen consumption. Cost if wrong: a core integration defect could survive both bounded reviews.
- Complete historical CSharp5 source admission remains Task3. The fifth profile stays explicitly incomplete and unreviewed. Cost if wrong: callers who ignore the incomplete status may rely on source behavior not yet established.
- Duplicate async modifiers, abstract/extern async combinations and async iterator/return compatibility remain explicit Task3 source-audit items; this milestone does not establish their conformance. Classify each source-local versus binding/type restriction before admission. Cost if wrong: the partial candidate can accept invalid combinations until that audit closes.
- Awaitability, task identity, binding, caller substitution, state machines and execution remain excluded semantic work. Cost if wrong: syntax acceptance can be mistaken for a compilable or executable program.
- Stable throughput parity, retained-memory bounds and live-editor latency remain unestablished. The measurements retain samples and allocation observations, with no such acceptance claim. Cost if wrong: a consumer may discover a performance limit absent from these measurements.

No deferred Minor findings.
