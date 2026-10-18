# CSharp3 inherited source-alternative links

Base: merged PR29 6ae9f0960b7f894893c92319b92c06247711d52d, tree2ddbb08d76285c0172d85c2e912f44fc6304a77b, matching tested feature67cf46a. This slice closes source traceability for the18 explicit unsafe ellipses, without grammar/parser/API or completeness-admission changes.

Each marker names every earlier non-placeholder alternative of the same source production, using exact independent occurrence/alternative IDs. The148 references include both B8625 base statements and the B8848 unsafe-statement extension when B8860 inherits embedded statements. Flattening earlier non-placeholder alternatives avoids treating an ellipsis as executable syntax and prevents recursive placeholder targets. Inherited targets retain their own pending witness/admission obligations.

The new contract test first failed for absent inheritance metadata. Eighteen separately declared origin cases now establish the expected original source occurrences; ten mutations require their exact source invariant errors for missing/invented markers, missing/duplicate/forward/other-family/placeholder targets, statuses/counts and premature promotion. The fresh-DOC checker independently rejects a disposable forward link.

Full verification:

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**2,286 normal +3,553 oracle =5,839 passed**, zero failures/skips. Fresh extraction reproduces455 occurrence bodies/main references and1175 alternatives, now checking inheritance links against that source. All123 dependency pins match; authored whitespace checks pass. Unrelated untracked benchmarks are preserved.

All455 overall rows stay candidate/pending and CSharp3 remains partial. The18 placeholders are now source-mapped;948 production alternatives still require representative owning-context witnesses or explicit reviewed normalization/exclusion evidence before the separate completeness contract. This is source-reference closure, not execution or admission of inherited alternatives.

The independent read-only review found no actionable findings. It inspected all18 source placeholders and confirmed the148 references, explicit origins, cycle/forward-target prevention and accurate source-mapping scope. The first review attempt failed at an account usage limit; after the user's continuation, the same review seat completed successfully. Runtime/fresh-DOC results remain the executor's recorded evidence; final admission remains pending.
