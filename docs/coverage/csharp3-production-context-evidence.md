# CSharp3 expression, statement and unsafe production witnesses

Base: merged PR31 ca6d8f5b48afdec60397700e401deb08cb63fe2c, tree3d4096493e41b1a64d81e7bc6709372d39978cc6, matching tested featureef02255. This slice records241 expression/type,100 statement and58 unsafe production alternatives, with399 owning-context positive/contrast pairs. No production parser, grammar, API or admission change.

Each case names its exact source occurrence/alternative/hash. Expressions include type categories, invocation/member/array/creation forms, precedence families, lambda signatures and query clauses/continuations. Statements cover declarations, recursive lists, all selection/iteration/jump/handler and resource forms. Unsafe cases include modifier orders, pointers, fixed declarations/buffers and stack allocation. Named category denotation, constant/boolean values, jump targets, flow, unsafe applicability, modifier compatibility and other inherited semantic exclusions remain unclaimed. These are finite representative observations, not exhaustive input/depth proof.

Reviewed source normalizations are explicit: namespace generic optional-suffix typo, three user-approved generic member suffix corrections, ASCII subtraction in place of the printed en dash, and the fixed-buffer declarator comma established by the controlling main-text example. No new source-authority decision is introduced.

The first parser run passed398 cases and failed one negative expectation: `var =1;` is valid assignment syntax using an ordinary contextual identifier. The failed observation is retained as an explicitly accepted neighbor; `var x=;` supplies the malformed declaration contrast. The approved inference/binding boundary is preserved. All399 now pass, with exact generated declaration-name spans after a supplementary prefix. Focus-fragment spelling/offsets are authored fixture integrity checks, not claims of exact generated fragment nodes; other tree/diagnostic spans are bounds-checked.

All three independent domain checks first failed for absent evidence/method metadata and links. The linked contracts now require exact source sets, fixture hashes, attributed witness identity, context/range/disposition rules and pending overall status. Sixteen mutation families run against each of the three domains and require their specific invariant errors independently of fixture checksum errors. Witness method attribution/existence is metadata evidence; execution comes from the recorded test run.

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**2,869 normal +3,587 oracle =6,456 passed**, zero failures/skips. Fresh pinned-DOC extraction reproduces455 occurrences/1175 alternatives and exact production links; an invented production case link is rejected. All123 dependency pins match. Unrelated untracked benchmarks remain untouched.

All455 overall rows remain candidate/pending and CSharp3 remains partial. With these399 observations, all58 unsafe alternatives have representative witnesses and the remaining production obligations are the365 declaration alternatives. The separate source/factory/policy admission gate still follows their closure; inheritance source mapping and finite token observations retain their distinct evidence scope.

Independent read-only review found no actionable findings and approved integration. It checked all399 context pairs against the raw source alternatives and independently confirmed fixture hashes, case/link sets, authored coordinates, semantic boundaries and six normalization precedents. Runtime/fresh-DOC/dependency verification remains the recorded executor evidence; alternative-specific typed-tree structure and exhaustive depth are not claimed.
