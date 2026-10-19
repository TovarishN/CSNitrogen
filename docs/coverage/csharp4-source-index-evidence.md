# CSharp4 source inventory and inherited lexical closure

Base: merged PR38 2d1b122ca71c1da94fa29acce7388bec8eb518f0, tree4c1e4680ef55ecbe51a48221597c3868247e36b3, identical to tested54da6ff. The source, mirrored delivery and explicitly unverified publisher-byte equivalence remain pinned by csharp4-reference-policy.json. This stage does not admit CSharp4.

## Independent source inventory

The new extractor reads the SHA-pinned working Version4.0 DOC with macOS textutil and splits only at LF. U+2028 separates grammar alternatives within a Word paragraph. It records occurrence identities, exact body hashes, normalized alternative hashes, all main-text heading references and exact earlier source alternatives for explicit inheritance markers. It does not redistribute the document or claim portable extraction.

There are458 appendix occurrences /431 distinct names:100 lexical,320 syntactic,38 unsafe. The1,181 alternatives comprise370 lexical,736 syntactic,75 unsafe; by kind,955 productions,209 one-of tokens,17 inheritance markers. The markers have135 exact earlier alternative references. Every458 occurrence has a main-text heading reference. Both anonymous-function list headings now have printed colons, unlike Version3; no missing-colon exception is retained in the fourth extractor.

The candidate comparison with the independently admitted CSharp3 index uses grammar domain, heading name and within-family occurrence ordinal. It records437 identical bodies,15 changed bodies,6 additions and3 removed predecessor occurrences. Alternative hashes suggest explicit candidates only. Neither identical bodies nor matching hashes establish CSharp4 normative contexts, generated owners, fixture coverage or admission.

The changed bodies include source spelling/formatting corrections, dynamic class-type spelling, named arguments/defaults/variance, attribute constructor-argument names, and the consolidated unsafe embedded-statement extension. Removed duplicate struct array headings and a second unsafe embedded-statement occurrence are retained as removals. Named constructor arguments in attributes are a concrete pending implementation delta; this stage does not silently claim that Task2 covered them.

```
python3 tools/extract-csharp4-standard-index.py /tmp/csn-csharp4-spec.doc --check --compare-third
```

Fresh extraction and comparison reproduce the committed artifacts. Six deliberate mutations were rejected: missing occurrence, changed alternative hash, missing main reference, changed inherited origin, changed predecessor identity and removed comparison row. Original bytes were restored before verification. These are development-tool consistency checks, not the future runtime source-admission gate.

## Inherited corpus observations

All1,157 admitted CSharp3 production/finite cases are replayed through the actual named CSharp4 parser and generated factory. The inherited test classes explicitly retain CSharp3 source/case identities; they are compatibility evidence, not fabricated CSharp4 occurrence coverage. Typed anchors, authored ranges, directive arguments/effects, class names, issue codes, contrast inputs and normalization dispositions are retained.

The first run had1,142 passes and15 failures, all lexical boundary gaps. The unchanged failures remain identified by their CSharp3 case identities:

- B:8394:input/1: terminal Control-Z was rejected.
- B:8422:identifier-start-character/2: escaped leading underscore was accepted.
- B:8468:pp-directive/4; B:8489:pp-diagnostic/1–2; B:8490:pp-message/1–2; B:8492:pp-start-region/1; B:8493:pp-end-region/1: missing required directive/message whitespace was accepted or its required issue omitted.
- B:8484:conditional-section/2; B:8486:skipped-section-part/2: malformed disabled #line syntax was ignored.
- B:8495:line-indicator/1; B:8496:file-name/1; B:8497:file-name-characters/1: empty mapped file names were accepted.
- B:8495:line-indicator/2: literal decimal zero was rejected by the current-compiler Int32 policy.

The controlling Version4 source agrees with the inherited historical rules: terminal marker handling at main1220–1222; identifier-start alternatives at1263/1266; raw keyword versus escaped identifier recognition at1278; conditional-symbol grammar at1404; directive grammar/message whitespace and skipped sections in §2.5; literal line-indicator/file-name grammar at1491–1495. The frozen Unicode4 scalar decision remains explicit.

Narrow historical boundaries now include4 for terminal-marker normalization (and incremental fallback), literal directive grammar checks including disabled syntax, exact decimal/file-name #line parsing, escaped leading underscore, and escaped boolean spellings as conditional symbols. Later profiles retain their prior contracts. Source text and UTF16 lengths remain original; terminal deletion is represented by a masked space, preserving the established coordinate contract. Line mappings retain the inherited arbitrary-decimal normalization; no modern compiler/PDB equivalence is claimed.

Four additional witnesses were RED: an undefined escaped `true` symbol selected the wrong branch, disabled nullable/file-based directives were ignored, and incremental terminal-Control-Z handling disagreed with required normalization. All1,161 inherited/new cases now pass.

## Remaining scope and verification

Full solution:4,522 normal +3,681 oracle =8,203 passed, zero failures/skips. All125 dependency input hashes match. Fresh whole-branch review found no actionable issues and confirmed the inspected Version4 clauses support these bounded corrections. The review did not execute tests or mutations. Whole conformance/admission, named attribute implementation, original publisher-byte equivalence and semantic binding remain explicit pending/excluded work; unchanged named-index/variance/default/omission behavior is supported by the full regression run, not a second fresh grammar review. CSharp1/2/3 gates retain exact logic and refreshed source pins. CSharp4 stays partial. The rest of the historical context review, named attribute constructor syntax, explicit CSharp4 alternative-specific fixture links and independent source/factory/policy admission are pending. Hash matches are candidate evidence, not completion. No sibling dependency writes or unrelated benchmark edits.
