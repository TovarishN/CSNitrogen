# CSharp3 historical source and grammar seam proof

Date: 2026-10-18. Base: merged PR17, `7d542a438b569a07c2ab5905151c36a03c5ebe06`.

This milestone proves bounded test-only Nitrogen compositions over the real CSharp2 modules. It does not add CSharp3 production syntax or change the public factory, completeness gates, dependency snapshot or existing generated APIs. CSharp3–14 and custom profiles remain partial. The [design](../superpowers/specs/2026-10-18-csharp3-additive-grammars-design.md) and [continuation plan](../superpowers/plans/2026-10-18-csharp3-additive-grammars.md) distinguish the proof from production implementation and whole-source admission.

## Source and inventory decisions

The [reference policy](csharp3-reference-policy.json) pins Microsoft's historical Version3.0 Word document by URL, SHA256 and byte count. Local macOS `textutil -convert txt` extraction was used for inspection only; no portable source occurrence index or full historical audit is claimed. The source binary is not redistributed here.

| Source clauses | Candidate syntax / obligation |
| --- | --- |
| 7.14 | Lambda signatures, expression/block bodies, assignment precedence and source-category restrictions |
| 7.15 / 7.15.1 | Query clauses, typed/untyped bindings, continuations, contextual words and ambiguity |
| 7.5.10.1–4 / 7.5.10.6 | Object/collection initializers, implicit arrays and anonymous creation |
| 8.5.1 | `var` shares named-type spelling; inference/type-name distinction needs binding |
| 10.6.9 | Extension receiver syntax and source-local restrictions; lookup excluded |
| 10.2.7 / 10.6.8 | Partial method syntax/context; pairing and erasure excluded |
| 10.7.3 | Historical auto-property context; backing-field generation excluded |

Implicit arrays are genuine CSharp3 syntax absent from the imported release-heading inventory. The inventory's CSharp3 lock statement is already owned by CSharp1 and is a duplicate/misattribution. Neither issue licenses a silent catalog completeness promotion. Historical Unicode/lexical/directive review remains pending.

## Executable proof

The test project registers the source `.ngr` inputs again under `CSNitrogen.Tests.CSharp3Probes`, alongside five test-only composition files. `Base.ngr` selects the exact 19 production CSharp2 modules; the initializer, lambda and query compositions add their own generated modules. Production source files are not copied or edited. The independent Roslyn project reads the authored inline fixtures as data and does not reference the normal test assembly.

The initial 20 positive compilation-unit fixtures all failed against the merged CSharp2 grammar before the probe extensions. Permanent paired baseline rejection now covers all 22 candidates. These include an escaped query identifier and a query selecting an anonymous object containing a lambda. Successful raw parsing alone does not satisfy source policy.

The seam suite checks:

- Explicit/ref/out, implicit list/single/empty lambda signatures; expression/block bodies; arithmetic, nested lambda and assignment ownership through generated views; bounded depth 8/32 valid and missing-body cases.
- Object arguments/types, initializer member children, nested/braced collection elements, anonymous named/inferred members, multidimensional implicit-array ranks, list separators and original spans. Existing grouped optional list fields are unwrapped through their syntax nodes; choice items retain their generated alternative views.
- Typed query bindings, clause lists, ordering directions and continuation names, independent query/lambda inclusion, individual module omission with ordinary baseline acceptance, and contextual words outside queries.
- Original UTF16 positions after a supplementary Unicode character in a comment, with `Span.Start` compared to the authored string's ordinal `IndexOf`.
- Seventeen malformed neighbors, plus the four retained raw over-acceptance obligations below.

## Known raw grammar policy gaps

| Raw accepted witness | Required production restriction |
| --- | --- |
| `1+x=>x` | Anonymous-function expression operand category |
| `new {a+b}` | Inferred anonymous members restricted to simple/member names |
| `new C{1,x=2}` | Scalar collection elements must be non-assignment expressions |
| `from x in from select x` | Historical query-context keyword reservation |

The historical query prose reserves all 13 query contextual words anywhere inside a query and permits `@` identifier escapes. The raw skeleton deliberately lacks that context policy. The escaped witness is a narrow observation, not full Unicode-escape/context conformance. These gaps must close before production feature admission; no failing witness was dropped.

## Independent reference phases

Roslyn5.0.0 `LanguageVersion.CSharp3` observes the 22 positives and 17 malformed neighbors. Five malformed forms parse without syntax errors and are rejected at compilation by specific source-local/version diagnostics:

| Witness | Deferred diagnostic |
| --- | --- |
| `(int x,y)=>x` | CS0748 inconsistent parameter typing |
| `(ref x)=>x` | CS8024 later simple lambda parameter modifiers |
| `(int x=1)=>x` | CS8024 later lambda optional parameters |
| `(params int[] xs)=>xs` | CS8024 later lambda params arrays |
| `new C{{}}` | CS1920 empty element initializer |

Tests explicitly assert those phase distinctions. Unknown-name, delegate-conversion and other binding diagnostics do not count as source rejection. Four additional resolvable examples check CSharp2 feature diagnostics separately from CSharp3 parsing. Roslyn observations are supporting evidence, not the historical normative contract.

## Verification and review

Final verification command:

```sh
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
```

Result: **1,256 normal + 3,360 oracle = 4,616 passed; zero failed or skipped.** This includes 59 seam tests and 43 phase-classified reference checks. `git diff --check` passed.

 The merged baseline was 1197 normal  + 3317 oracle  = 4514 passing tests. The initial red seam run was 20 failures, zero passes; the expanded focused run before the final two candidates was 57 passing seam tests and 41 passing reference tests.

Independent read-only review identified missing typed initializer/span evidence and lambda precedence ownership evidence. Both were added and re-reviewed with no remaining actionable findings. Reviewer did not run builds/tests; execution evidence comes from the commands below.

Production continuation starts with bounded lambda/query modules and explicit source policies, version/omission tests and reviewed source/factory identity refreshes. Extension receivers, partial methods and auto-properties are planned source seams, not proven or implemented by these expression probes.
