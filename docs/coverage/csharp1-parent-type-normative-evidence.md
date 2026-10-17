# C# 1 parent type, array and return review — 2026-10-17

PR #12 was merged as `815b43a` after a fresh **2,838-test** run. This approved Task 8 continuation reviews sixteen additional type-family source occurrences. It preserves the earlier eleven helper closures; no module, policy, feature or profile completion state is promoted.

## Normative grammar correspondence

The pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf) is the authority. The implementation normalizes the type grammar into named/builtin/pointer bases and an ordered rank suffix. It does not expose every standard helper as a distinct start rule or infer a bound type's meaning.

| Source occurrences | Normative clauses | Implemented recognition and scope |
|---|---|---|
| value-type, struct-type, enum-type | §11.1, printed pp. 89–90 | named type spellings or the simple-keyword alternatives contribute to the normalized TypeBase union; whether a name denotes a struct/enum remains binding |
| simple-type, numeric-type, integral-type, floating-point-type | §11.1, printed pp. 89–91 | the explicit thirteen simple keywords are the union of nine integral spellings, float/double, decimal and bool; these are inlined into BuiltinType, rather than separately callable numeric-category parsers |
| reference-type, class-type, interface-type, delegate-type | §11.2, printed pp. 94–95 | class names and object/string, interface names, delegate names and ranked types contribute to the normalized type union; name denotation is semantic |
| two array-type and two non-array-type summaries | §11.2 and §19.1, printed pp. 95 and 279 | TypeBase supplies the unranked base, including selected Unsafe pointer extensions; a nonempty TypeReference.Ranks suffix supplies array syntax. With no suffix, the same owner represents a non-array spelling. Array element/rank structure remains typed and ordered |
| return-type | §17.5, printed p. 230 | ReturnType is a TypeReference or the dedicated void alternative; ordinary type fields do not gain void from the return wrapper |

Flattening the safe type alternatives gives the fifteen builtin spellings (thirteen simple types plus object/string), qualified names, and ranked forms. The implementation intentionally shares these spellings across standard semantic categories: a named type is not guessed to be a class, struct, enum, interface or delegate. The reviewed contributing alternatives are recognized in the owning type union; this does **not** introduce independent simple-type/numeric-type/class-type parser entrypoints or a semantic classifier. The crosswalk records this normalization explicitly.

The array rule's structural equivalence is a non-array base followed by one or more ranks; zero ranks belongs to the broader type owner. The leftmost rank describes the outer array, and remaining ranks describe nested element arrays (§19.1). Observations compare the source-ordered suffix with Roslyn's ArrayTypeSyntax, including arrays of pointers. Pointer referent legality and the broad root type/pointer/unmanaged productions remain open, as described below.

## Execution observations

`ParentTypeNormativeTests.Type_base_and_rank_children_match_in_each_declaration_and_typeof_owner` contributes **324** typed observations: all fifteen builtins; three named stems with bare, single and ordered double rank suffixes; four pointer stems with the same suffixes; and nine owners (field, local, parameter, method return, property, indexer, delegate return, interface return, typeof). It compares the selected TypeSyntax, base/qualified-name/pointer structure, stars, ranks, comma counts and physical spans against explicit Roslyn CSharp1 syntax. For the same source, actual configured safe composition membership rejects pointers, admits shared safe types and contains no Unsafe nodes, including local-declaration ambiguity checks through the syntax policy.

Five compiler-backed named-kind fixtures show identical named syntax for declared class, struct, enum, interface and delegate types without implementing name binding. Four void fixtures assert the dedicated return wrapper in method/delegate/interface/typeof owners and the absence of a TypeReference at that position. Five later/compiler-deferred cases retain named diagnostic classification: nullable/generic/alias-qualified availability (CS8022), void fields (CS0670) and sized declaration ranks (CS0270). Existing type/name/rank/cast/unsafe suites remain their independent evidence; these new observations do not close those whole families.

No grammar or parser policy change was required. A test-only build error (the custom separated list is not an IEnumerable accepted by Assert.Single) was corrected to use its actual Count/index API before runtime evidence. The metadata RED run then passed all **342** parser observations and failed only the new sixteen-row closure assertion, before changing any review declarations. A passing sample count alone is not the promotion basis; the normative grammar correspondence and independent review are required as well.

## Preserved pointer/array oracle difference

The initial four-case characterization incorrectly assumed that every pointer-to-array spelling must produce a modern compiler error. Two cases failed that assumption. Pinned Roslyn 5.0 parses `int[]*`, `N.T[,]*` and `int*[]*` without syntax errors and reports **CS8500 as a warning**; the unresolved `N.T` additionally gives CS0246. The historical §25.2 prose requires an unmanaged referent and excludes references from it. CSNitrogen rejects these array-pointer spellings in its current pointer grammar. `void[]` produces syntax errors in both.

These are retained as explicit phase observations, not silently relabeled as Roslyn syntax errors or used as a compiler-error admission gate. Pointer-to-array acceptance/diagnostic parity is unresolved; the broad root `type` occurrences, `pointer-type` and `unmanaged-type` rows stay observed with this note. Their syntax/semantic boundary needs a separate decision and evidence before closure. This checkpoint recognizes the normalized safe categories, array base/rank shape and return wrapper; it does not certify every recursively unsafe or invalid type spelling. In particular it does not promote the pointer family, module or global completion obligation.

## Completion boundary

The sixteen exact source occurrences are recorded as verified review declarations in the [crosswalk](csharp1-standard-crosswalk.json): eleven safe contributing type-category occurrences, two array-type occurrences, two non-array-type occurrences and return-type. Together with the earlier helper review, this yields **27 verified and 335 pending** occurrences. Review declarations remain human metadata; the audit checks structure and exact identity, not the authenticity of a review. Parent pointer/root-type closure, cast/statement/interface/source-placement/unsafe family closure, bindings, conversions, layout and execution remain outside this result. CSharp1 remains partial.

## Verification

Focused command filtering ParentTypeNormativeTests, CSharp1StandardCrosswalkTests and CSharp1CoverageGateTests passed **398 observations**. Full solution command `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s` passed **706 normal + 2,475 oracle = 3,181 tests**, zero failed/skipped and no build warning/error output. New observations total 343 (342 parser/phase cases plus one metadata closure test).

All 118 source hashes match both the original and isolated Nitrogen; all six corpus hashes match provenance. Original Nitrogen remains at `f5bf525e3e2723a8fe39e127f800f6376345feb1` with its preexisting modified `docs/editor-support.md`. Production manifests exclude Roslyn and Nitrogen.Grammar. No grammar, parser policy, composition membership, dependency or public API change is included. The changed crosswalk bytes are already an input to CoverageIdentity. `git diff --check` passes.

## Independent review

Fresh read-only review of `815b43a..d995527` found no Critical, Important or Minor issues requiring changes and judged the branch ready as an explicitly limited normalized-production review. It inspected the complete six-file diff, new/existing fixtures and assertion helpers, unchanged NamesTypes/Unsafe grammar, approved design/Task 8 and relevant normative text. It accepted all sixteen closures as contributing alternatives and structural wrappers, without certifying independent subcategory parser APIs or semantic type classifiers.

The reviewer specifically assessed the array-pointer discrepancy. Such referents violate the historical unmanaged restriction and therefore do not demonstrate a missing valid historical array spelling. The limited array/non-array structural closures can stand while recursive pointer/type phase parity remains unresolved. No fix pass or deferred minors resulted.

It did not certify execution results, dependency/source/corpus hashes or sibling state, and did not independently inspect execution logs. It declined full recursive pointer/root-type parity, unmanaged classification, exact diagnostic placement, semantic binding/conversions/layout/execution, whole cast/statement/interface/placement/unsafe context families, or module/policy/feature/CSharp1 completion. Execution and preservation checks above remain author-run evidence; the production and completion limits remain explicit.

## Subsequent boundary decision

The [root-type/pointer continuation](csharp1-pointer-boundary-normative-evidence.md) separately reviews the four source rows left open here. It retains the array-pointer phase difference and closes historical valid-recognition contributions, rather than asserting semantic validation or universal invalid-input tree/phase parity. The counts and pending-row statements above remain the record of the parent-type checkpoint.
