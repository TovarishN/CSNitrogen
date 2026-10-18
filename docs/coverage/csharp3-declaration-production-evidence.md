# CSharp3 declaration production-alternative observations

Base: merged PR32 acd62c43398487ba739883d98e4e050f0b15698a, tree2bb5bc6ded3c3c917025ec775b2ffbf53af94a4b, matching tested feature5894215. This slice adds365 declaration production cases, completing the representative production matrix. No grammar, parser, public API or completeness-admission behavior changes.

Each source alternative has a pinned owning compilation unit and malformed contrast. The cases cover namespace/import order, type families, attributed generic parameters, all constraint combinations, member families, finite modifiers, accessors, constructor/finalizer forms, operator tokens, repeated array occurrences, interface/enum/delegate forms and attribute targets/arguments. Existing binding exclusions remain explicit: modifier compatibility/applicability, named type/category/constraint identity, operator type/pairing legality, constant values, attribute identity/target applicability and enum underlying-type legality. `enum E:char` deliberately records that inherited syntax boundary. The interface method witness includes a generic parameter list and does not introduce a source correction. Existing ordinary-method normalization remains in the reviewed declaration source report.

The first run passed all365 new declaration cases and the prior399 contexts:764 production theories total. Exact generated declaration-name anchors are checked after a supplementary prefix; focus fragments have authored spelling/range integrity checks, and other node/diagnostic spans are bounds-checked. No alternative-specific typed-tree or exhaustive-depth claim is made.

The declaration evidence first failed for missing metadata, attributed method and links. The whole-source matrix separately failed while those365 identities were absent. Exact links now complete all1175 alternative identities:209 finite tokens,948 production alternatives and18 source-mapped inheritance markers. The matrix requires one correctly classified evidence entry per identity; six mutation cases reject missing/duplicate identities, missing markers, unknown domains, wrong evidence classes and wrong case IDs. Sixteen production mutation families now run across all four expression/statement/unsafe/declaration domains with specific invariant-error assertions. Attributed method existence remains metadata evidence; recorded suite runs establish execution.

The fresh-DOC checker now handles the declaration domain. A new direct probe first demonstrated that an extra unknown-domain link was ignored, then the checker was extended to reject unknown/unregistered production domains. The same probe now fails with its exact domain error, alongside the independent matrix mutation. This remains a source checker, not a runtime admission mechanism.

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**3,234 normal +3,595 oracle =6,829 passed**, zero failures/skips. Fresh pinned-DOC extraction reproduces455 source bodies/main references and1175 alternatives with complete source-link sets. All123 dependency hashes match. Unrelated untracked benchmarks are preserved.

All455 overall rows remain candidate/pending and CSharp3 remains partial. Every production alternative now has representative owning-context or reviewed normalization/binding-boundary observations. Exhaustive inputs, binding/type/flow compilation and runtime behavior remain excluded. The separate fail-closed source/factory/policy admission contract is the next task; this evidence matrix alone does not admit the profile.

Independent read-only review approved this bounded slice with no actionable findings. It checked all365 source/contrast pairs, source hashes and authored UTF16 coordinates, confirmed exactly365 new links across149 occurrences, and independently confirmed the complete1175-identity evidence classification. Runtime/fresh-conversion/dependency execution remains the executor evidence above; admission remains separate.
