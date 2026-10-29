# CSharp5 scoped grammar proof

The global-await evidence in csharp5-seam-proof-evidence.md is historical and remains an explicit rejected candidate. The user approved the concrete scoped parsing-context design2026-10-29. Nitrogen PR94 implements that design, with a fresh whole-branch review and a verified fix pass; the latest core full gate is2398 tests and a clean solution build. The core PR merged as b3494870dfb5f187bae9065227d4ef7f742aa048 after all CI gates passed.

The isolated actual-grammar assembly uses26 real baseline modules plus test copies of Core, Declarations, AnonymousMethods and Lambdas. The async-functions and await-expressions modules remain independently removable. It is not a standalone handwritten parser or a production CSharp5 admission guard.

Desired ordinary/async owners first failed5 of18 targeted checks against the old global probe. Scoped branches then passed41 checks, including the two previously deferred operand/omission field assertions. Raw/escaped/verbatim/format spellings add10 pinned Roslyn5 syntax comparisons. Two additional header witnesses failed because setting Await at an entire method declaration misclassified await(M()) in attributes/default values. A separate Boolean AsyncMethod flag now selects only MethodDeclaration.Body's Await scope; all55 checks pass with SDK10.0.100 against the reviewed latest core.

The body matrix retains three InvocationExpression and five AwaitExpression owners, exact(M()) operands and original UTF16 positions. Parameters/Body property types and direct parents survive independent module omission. Adding/removing an async header matches a full parse and reuses unaffected nodes. Method attributes/default expressions retain InvocationExpression while the method body has AwaitExpression. Only raw async acts as a modifier; only raw await acts as the operator in these syntax witnesses. Verbatim, Unicode-escaped and formatting-character spellings retain their tested identifier owners.

The source document and text extraction were independently reacquired and matched their existing pinned SHA256 values. Main4431 scopes await expressions to async bodies;4432-4437 and7801 supply later source-local legality obligations. The nested non-async anonymous-function case remains lexically AwaitExpression, with legality to be rejected separately. Syntax observations do not establish awaitability, task identity, binding, caller-information substitution or execution.

Commands:

```sh
dotnet test tests/CSNitrogen.CSharp5.Tests/CSNitrogen.CSharp5.Tests.csproj -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -p:NitrogenRoot=/path/to/reviewed/Nitrogen
```

The production stage pins141 clean sources from that merge, registers the two independent capabilities, applies the approved Unicode4 scalar selector to5, and enforces source-local await/ref/out/query/identifier boundaries. The full restored-snapshot gate passes6248 main tests,3788 oracle tests and55 scoped proofs. The inherited raw-await identifier reservation is checked separately from nearest-function await legality. Exact predecessor helper/policy inventories include the explicitly inactive fifth policy; no predecessor source alternatives or admission checks were removed. Pinned restoration preserves manifest bytes across source locations. Deterministic generator and measurement evidence is recorded with the production stage. CSharp5 and later/custom profiles remain partial until independent source/factory/policy/evidence closure.
