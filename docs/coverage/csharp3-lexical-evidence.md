# CSharp3 lexical disposition verification

Base PR22 merged `9c81807bde014b4f70c94354187df3d424d351fb`; its tree `d6e88ef0de37df430ca36d0b66fbdb977161a726` matches tested feature `abf4369`. This continuation records the source-domain lexical disposition without changing parser/grammar acceptance or admitting CSharp3 completeness.

The [source report](csharp3-lexical-source-review.md) covers the primary's100 lexical occurrences, with exact crosswalk occurrence/source identities and finite selected-CSharp3 witness groups. All454 overall candidate review statuses remain pending. Literal/trivia tests execute63 cases; directive tests26. Report/source-set/witness-reference integrity checks include six mutations. The initial report test failed for the missing embedded report; resource/coverage identity wiring closes that failure. CSharp3 reports use their own resource prefix rather than entering CSharp2's reviewed report set.

Final full verification:

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**1,758 normal + 3,468 oracle = 5,226 passed, zero failures/skips.** The pinned-DOC index/crosswalk command reproduces all454 body identities and main-text heading references. All123 Nitrogen dependency source hashes match. Authored whitespace check passes. An independent whole-branch reviewer found no actionable issue, confirmed the100 lexical links and hashes, and confirmed the opaque pragma boundary is explicitly limited. Witness method existence alone is not execution evidence; the recorded full suite run executes the retained groups.

An initial test invocation was not executed because automatic approval review hit an account usage limit. After the user requested continuation, the same escalated command succeeded; no approval bypass was used.

Integration: concurrent PR23 merged as `d90d5f2` while this slice was under review. Its Lexical.ngr grammar-function refactor and both generator changes are preserved. The sole merge conflict was the CSharp2 approved-ledger hash; the combined source snapshot was rehashed without changing gate logic. Both lexical generators reproduce the upstream bytes, and the complete integrated suite again passes all5,226 tests. The dependency snapshot is unchanged.

Remaining Task4 work: normative expression/type, statement/iterator and declaration/unsafe domain dispositions, traceable alternative-specific witness obligations, then a separate fail-closed CSharp3 source/factory/policy admission contract and its review. This evidence does not substitute for those steps.
