# CSharp3 alternative identity and finite witness verification

Base: merged PR28 f3167e5236f912f1b6f39ace9ae9df1167b3efd7, tree f349b1a3f642cbcffc16e6785196c594b4df94c8, matching tested feature e29db0b. This slice adds source-derived alternative hashes to the existing index and explicit finite case links to the crosswalk, without grammar/parser/API/admission changes.

The new alternative-index test first failed because alternative identities were absent. Fresh extraction now records1175 alternatives across455 occurrences, including18 inherited placeholders and eight continued right-hand sides kept intact. The209 selected-CSharp3 finite token cases passed on first execution, with paired positive/negative inputs and exact original ranges. The finite evidence test then failed for absent crosswalk metadata; exact case links and independently pinned source/fixture identities close that failure. Fourteen mutations require the corresponding source/case invariant errors, beyond merely detecting a changed checksum.

Full verification:

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

The initial full run passed2286 normal +3524 oracle =5810 tests. After strengthening mutation assertions and checking domain alternative counts, the final full run again passes **5810 tests**, zero failures/skips. Fresh extraction reproduces all455 source bodies/main references and all1175 alternative identities. A disposable invented finite case link is rejected by the fresh-DOC checker. Authored whitespace checks pass. The unrelated untracked benchmarks directory is preserved.

All455 overall source rows remain candidate/pending and CSharp3 stays partial. See csharp3-alternative-obligations.md for the948 production and18 inheritance obligations that remain before a separate completeness contract.

All123 dependency pins match. The independent read-only reviewer found no actionable findings, independently matched all1175 alternatives to the pinned text, inspected every209 fixture context/negative/range and confirmed unchanged prior source identities and overall pending status. Method attribution is not method-body hashing or execution proof; the recorded full suite supplies execution evidence. Remaining production/inheritance closure, semantic legality and final admission remain unclaimed.
