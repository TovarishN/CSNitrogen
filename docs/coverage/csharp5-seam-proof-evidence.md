# CSharp5 bounded typed seam proof

Status: test-only observation proof; CSharp5 remains partial. Production grammar, Unicode selector and admission are unchanged. Unicode4 scalar continuity was approved by the user on 2026-10-23.

## Inputs and scope

The source and extraction are pinned in csharp5-reference-policy.json. Main4430–4437 controls await and its contextual restrictions; main5174–5175 controls anonymous async prefixes, main7800–7802 async function owners/ref-out. The appendix9841–9842 omits prefixes present in the controlling main text. Caller-information substitution at main8767/8856–8876 remains semantic work. This proof does not close those source obligations.

The isolated CSNitrogen.CSharp5.Tests assembly composes the real30 CSharp4 grammar paths, substituting bounded copies of AnonymousMethods and Lambdas with optional Async markers. A separate assembly is required because RegisteredGrammarInputs registers an absolute input path once per compilation and rejects two generated namespaces for that path. No production grammar is copied or changed to bypass admission.

Two independent modules add async function markers/modifiers and a global await expression. This is deliberately an insufficient candidate, retained to expose the missing contextual behavior. It is not registered as a production language in nitrogen.json.

## Observed progression

- Initial eight seam tests: five new-language witnesses failed, three ordinary baseline witnesses passed. After the bounded extensions: eight passed. Logs: /tmp/csn-fifth-seam-red.log and /tmp/csn-fifth-seam-green.log (session-local, not durable evidence).
- Corrected desired context-owner test plus pinned Roslyn reference: three failures and thirteen passes, /tmp/csn-fifth-context-corrected-red.log. All three failures were ordinary invocations misclassified as AwaitExpression by the global extension. This is an unresolved desired contract, not a passing language implementation.
- The committed observation harness asserts that limitation explicitly. Reference observations use Microsoft.CodeAnalysis.CSharp5.0.0 with LanguageVersion.CSharp5; they are independent syntax observations, not the normative authority or semantic acceptance.
- The nested non-async delegate inside an async method still receives AwaitExpression in the reference parser. Source policy must reject that use separately. Resetting lexical recognition at every anonymous function would therefore be wrong.
- Verbatim @await and Unicode-escaped \u0061wait calls inside async methods remain InvocationExpression in the reference. The global prototype's shared Contextual helper produces AwaitExpression for both. These two mismatches are pinned as observations too; keyword-spelling behavior is unproved and must be fixed before production.

| Authored location for raw await(M()) | Reference owner | Global prototype |
| --- | --- | --- |
| Ordinary method | InvocationExpression | AwaitExpression |
| Async method | AwaitExpression | AwaitExpression |
| Mixed-file ordinary method | InvocationExpression | AwaitExpression |
| Mixed-file async method | AwaitExpression | AwaitExpression |
| Ordinary lambda | InvocationExpression | AwaitExpression |
| Async lambda | AwaitExpression | AwaitExpression |
| Non-async delegate within async method | AwaitExpression; use prohibited by source policy | AwaitExpression |
| Async delegate within ordinary method | AwaitExpression | AwaitExpression |

Typed tests retain Lambda.Parameters/Body as SyntaxNode; AnonymousMethod.Parameters as Optional<AnonymousParametersNode> and Body as BlockNode. Async is an additive Optional<AsyncMarkerNode>. Await.Operand is ExpressionNode; unary precedence and original UTF16 spans, including a supplementary prefix, are asserted. Removing either module preserves fourth-version baseline witnesses and removes its own additions. Ordinary async return types, names, lambda parameters and identifier suffixes remain baseline syntax in the bounded witnesses.

## Verification

The isolated proof passes38 tests, zero failures/skips. Full solution: 10,026 passed (6,200 ordinary + 3,788 oracle + 38 fifth-proof), zero failures/skips, using dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false. All125 pinned dependency files match. Four grammar generators leave tracked production files unchanged; all four CSharp4 index/matrix/crosswalk/main-text check tools pass. The missing temporary CSharp3 DOC was restored from its pinned publisher URL before the main-text check. An existing CS8602 warning remains in CSharp3InheritedAlternativeTests.cs:81. Passing observation tests mean the limitations remain reproducible; they do not satisfy the desired contextual contract.

## Required decision

See ../superpowers/specs/2026-10-23-csharp5-await-context-decision.md. Production Task2 and source-admission Task3 remain blocked on a scoped mechanism/API direction and subsequent implementation evidence.

## Review and execution rulings

Fresh whole-branch review (gpt-6-astra/high, a158cc5..91d9ace) found no Critical/Important issues and approved this bounded proof for integration. It did not approve production CSharp5. The reviewer inspected code and generated contracts read-only; verification execution above was performed by the implementer.

Deferred minors: paired await(M()) cases do not directly assert Operand (M()) (the separate unary test uses await M(x)); independent omission checks do not directly reassert surviving typed fields. Both must be considered in the production context acceptance work.

Rulings made during execution, with costs if wrong:

1. Use an isolated test assembly because input-path registration prevents a second namespace in the existing oracle assembly; cost: an extra build/test assembly.
2. Retain passing observations after the genuine context RED rather than ship wrong typed owners; cost: production is delayed for the decision.
3. Correct the nested non-async delegate lexical expectation from invocation to await based on source/reference evidence; cost: revisiting the nested context boundary if later evidence differs.
4. Retain escaped/verbatim mismatches explicitly without claiming the helper is suitable; cost: spelling-specific work remains before production.

Reviewer set-aside rulings:

- Production context correctness remains unresolved because no production factory changes; cost: delayed production support.
- Complete spelling/suffix/nesting coverage remains a future acceptance gate because this proof is bounded; cost: more production cases.
- Catch/finally/lock/unsafe/ref-out/query enforcement remains future source-local validation; cost: source closure delayed.
- Memoization/recovery/incremental/core APIs require design and proof because no core implementation ships; cost: additional core work.
- Unicode selector/source inventory/admission remain Tasks2/3 because contract approval alone does not implement them; cost: CSharp5 stays partial.
- Awaitability/task identity/caller substitution/state machines remain semantic exclusions; cost: separate semantic implementation.
- Reviewer verification was code-only by assignment; implementer ran10026 tests and input checks in-session; cost: no second independent execution.

The active plan workspace remains while Tasks2/3 await a mechanism decision. No broader task completion is claimed.
