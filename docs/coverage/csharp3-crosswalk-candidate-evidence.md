# CSharp3 occurrence-owner crosswalk candidates

Base: merged PR21 `8880165ef7633ac9f224f8c417cf85805ae6397e`. On 2026-10-18 the user approved preserving the three generic-member suffix forms as explicit historical source corrections. The reference policy and crosswalk record exactly Appendix B8542,8548,8856. Existing generated grammar/API behavior is retained.

The crosswalk has one candidate for each of the 454 source occurrence identities (425 distinct names). Existing CSharp2 mappings supply explicitly labeled candidate origins; renamed destructors, shared anonymous-function signatures, new construction/query forms and fixed-buffer productions have authored owners in the selected CSharp3 factory. Different list/wrapper/source categories may normalize to shared generated owners. Every row retains its Appendix B body hash and all corresponding main-text heading line references. Main-text heading references are source locations, not claims that all associated normative prose has been reviewed.

Independent oracle checks parse the real Nitrogen grammar ASTs, collect actual selected factory owners and resolve every candidate target/policy. They reject missing/duplicate occurrences, altered body identity, invalid main references, unknown/duplicate owners, unknown policies, missing/unapproved generic correction declarations, wrong correction occurrence sets and premature reviewed status. The runtime coverage identity includes the embedded candidate crosswalk; drop/mutation checks cover this new identity input. The initial resource-wiring run failed all ten tests with the missing resource; the embedded resource and identity wiring close that failure. No CSharp3 admission is enabled.

Reproduction:

```
python3 tools/extract-csharp3-standard-index.py /path/to/pinned.doc --check --check-crosswalk docs/coverage/csharp3-standard-crosswalk.json
```

The SHA-checked original document reproduces all 454 body identities and main-text heading reference lists. A disposable crosswalk whose first main reference was changed to zero is rejected. The DOC and extracted document text are not redistributed.

The retained inherited source corpus now executes all 163 cases in the actual CSharp3 factory, with original UTF16 tree/diagnostic bounds. The first run found six expected edition differences: lambdas, initializers and automatic properties become accepted; three target spellings absent from the third-edition grammar become rejected. Exact per-source overrides retain these observations without weakening other expectations. A separate 64-case closure suite executes inherited generic followers, nullable/conditional and alias-cast ambiguity, iterator ownership, declaration restrictions, historical lexical forms and malformed neighbors in CSharp3. Verbatim conditional-symbol spelling remains deliberately rejected under CSharp3's own production.

Three additional source normalizations have direct controlling text in the same primary: main1789–1793 expressly makes qualified type arguments optional despite printed `type-argument-listop`; main7068 demonstrates an ordinary nongeneric interface method despite the missing `opt` at7082; main8006–8007 demonstrates comma-separated fixed-buffer declarators despite the separator omission at7999. The crosswalk records these interpretations and the closure suite retains witnesses. They use explicit source examples/prose; they do not expand the user's bounded generic-member correction to unrelated forms. Fixed-buffer primitive denotation is at main8003; the preceding audit report's line reference was corrected.

Every candidate remains `reviewStatus: pending`. This slice does not establish a normative occurrence crosswalk, executable witness coverage for every production alternative, or a complete language. Full source-domain dispositions and a separate fail-closed CSharp3 gate remain required. CSharp1/CSharp2 source admission logic stays unchanged, with reviewed source hashes refreshed for coverage identity wiring.

Validation: the full solution command used in the preceding audit passed 1,669 normal + 3,460 oracle = 5,129 tests, zero failures/skips. All 123 dependency sources match their pins, the source extraction/crosswalk check reproduces, and the authored whitespace check passes. Independent whole-branch review found no blocking correctness issue; the noted stale approval prose was refreshed as part of recording the user's decision.
