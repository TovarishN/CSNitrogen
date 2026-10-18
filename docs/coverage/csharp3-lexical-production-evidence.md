# CSharp3 lexical production-alternative observations

Base: merged PR30 a6c0e55429adc04f73dcad9c613e17155f8ba77d, tree c21ec69dcb907760a9e17e244d609871c3bd03db, matching tested feature7412e2b. This slice adds alternative-specific observations for all184 lexical production alternatives. No parser, grammar, public API or completeness gate changes.

The pinned corpus has246 positive inputs and184 contrasts, each linked to an exact primary-source alternative/hash. Owning contexts exercise literal/trivia forms, raw/escaped Unicode4 identifiers, all frozen Zs scalars, supplementary characters, newline forms, disabled text and directive structures/effects. A supplementary-character prefix shifts the exact class-name anchor checks in original UTF16 coordinates. Other node, diagnostic, directive and inactive spans are bounds-checked; these checks do not establish their exact offsets or require a nonempty inactive-span collection. Original source and masked length are preserved. These finite observations are not exhaustive category/input proof, binding/type checking or semantic compilation.

Explicit dispositions distinguish syntax observations, frozen Unicode policy, input normalization, diagnostic effects and opaque pragma payloads. An active #error intentionally yields CSN1202 and a rejected document. Warning/pragma information yields CSN1203. Opaque warning payload cases retain malformed warning lists with a warning under the already reviewed normalization; they do not claim structured pragma-body conformance. Accepted contrasts are permitted only for the exact declared opaque payload case. No new exclusion or language-policy decision is introduced.

The184 parser theories passed on their first run. The independent evidence test then failed for missing evidence metadata, attributed witness metadata and alternative links. Populating the exact crosswalk links made it pass. Fourteen mutations require their specific invariant errors beyond fixture checksums: missing/duplicate cases, source hashes, anchors, dispositions, pragma/error boundaries, case links, evidence/method identity and premature admission. Method existence/attribution is metadata evidence; execution is established by the recorded suite.

Verification:

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**2,470 normal +3,568 oracle =6,038 passed**, zero failures/skips. Fresh pinned-DOC extraction reproduces455 occurrence bodies/main references and1175 alternative identities; its crosswalk check now verifies lexical production link sets and rejects an invented case link. All123 dependency hashes match. Unrelated untracked benchmarks are preserved.

All455 overall rows remain candidate/pending and CSharp3 remains partial. Finite209 token observations,184 lexical production observations and18 source-mapped inheritance markers are separate evidence classes. The remaining764 production alternatives (706 syntactic and58 unsafe) need their own representative cases or explicit reviewed normalization/exclusion evidence before the separate source/factory/policy admission contract.

Independent read-only review found no blocking correctness issues and confirmed all184 alternative hashes/links,246 positive contexts and fixture identity. Its minor evidence-wording finding is corrected above: only class-name anchors have exact expected offsets, while the other listed spans have bounded-range checks. Exhaustive inputs/depth, semantic and historical compiler equivalence, structured pragma validation and final admission remain unclaimed.
