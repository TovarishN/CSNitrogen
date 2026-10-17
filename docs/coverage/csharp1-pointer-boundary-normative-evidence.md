# C# 1 root type and pointer boundary review — 2026-10-17

PR #13 was merged as `1c24f34` after a fresh 3,181-test run. This approved Task 8 continuation reviews four remaining type-union/pointer/unmanaged source occurrences. No grammar or accepted-source behavior changes. CSharp1 remains partial.

## Normative recognition contract

The SHA-pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §11 (printed p. 89) and §25.2 (printed pp. 323–324), supplies the contract. The safe type union contains value/reference contributions; the unsafe extension adds pointer types. A pointer is an unmanaged referent followed by a star, or void followed by a star. The unmanaged alias contributes ordinary type syntax, while its denotation excludes references and structs containing reference fields.

Historical unmanaged referents comprise thirteen simple types, enums, pointers and recursively unmanaged structs. This yields a useful normal form for every historical valid pointer spelling: a builtin or qualified-name base followed by one or more stars, with the separate void-star case; array rank suffixes can then form arrays of those pointers. A valid pointer cannot have an array referent. Repeated stars cover recursively pointer-valued referents, including void pointers. Checking whether a named type denotes an unmanaged enum/struct is semantic and remains excluded.

The actual grammar corresponds structurally:

- Safe NamesTypes.TypeBase admits builtin/named spellings; TypeReference adds the ordered optional array-rank suffix.
- Unsafe.PointerTypeName recursively appends a star to TypeBase; VoidPointerTypeName supplies the void-star seed. These extend the same TypeReference union.
- PointerNonArrayType represents the same valid pointer normal form with a typed base and a nonempty star list; PointerCastType adds ranks for pointer-containing casts.
- The unmanaged-type alias maps to the existing type syntax, without introducing a semantic classifier, new entrypoint or binding/type-checking subsystem.

These are recognition/contributing-production closures for historical valid syntax, with explicitly excluded semantic checks. They do not promise that every semantically invalid source receives a syntax tree or the same diagnostic phase as modern Roslyn. The earlier name/type category closures use the same normalization boundary; no module, feature, policy or global completion state is promoted here.

## Decision on the recorded phase difference

The retained [parent-type observations](csharp1-parent-type-normative-evidence.md) show that Roslyn 5.0 parses `int[]*`, `N.T[,]*` and `int*[]*` and reports CS8500 as a warning, while CSNitrogen rejects them. First-edition §25.2 excludes reference referents; the spellings therefore do not demonstrate a missing historical valid pointer form. The decision is to preserve the existing recognition and explicitly limit the closure claim to historical valid-type recognition plus the documented semantic boundary. No new grammar/API is warranted solely to match a modern syntax tree for these historically invalid referents.

The diagnostic-phase difference remains a recorded limitation. It is not silently turned into a Roslyn syntax error, hidden by fixture selection, or claimed fixed. Universal invalid-input recovery/phase parity is not established by these four rows. If a consumer later requires typed trees for such historically invalid forms, that is a separate explicit capability with its own representation and tests.

This does not make the parser a historical unmanaged validator: `string*`, `object*`, a pointer to a named class and a pointer to a struct containing an object field remain accepted as broad named/builtin syntax. Modern Roslyn warns with CS8500 for these fixtures; historical referent legality remains semantic. Likewise pointer declarations outside an unsafe context are recognized by the full composition, while the compiler reports CS0214. Safe module omission excludes the pointer syntax itself, independently of whether an unsafe keyword occurs in source.

## Executed observations

`PointerBoundaryNormativeTests` contributes **174** observations:

- **146** compiler-backed valid historical type forms: thirteen primitive referents, a declared enum, a nested unmanaged struct and the special void seed; one to three stars and bare/single/ordered-double rank suffixes, plus comment/escaped-name variants. Each source compares field, method-return, parameter and actual cast structure against explicit CSharp1 Roslyn; direct full type/cast fragments compare the alternative typed representations and full consumption. The same raw type fragment is rejected by the real configured safe factory, with no Unsafe nodes. These finite observations support the recursive grammar proof, not exhaustive star/rank depth.
- **17** compiler-backed allocation element forms cover thirteen primitives, enum, nested unmanaged struct, int-pointer and void-pointer referents. Typed element/size children and physical spans are compared. This is type-operand evidence, not whole stackalloc-family or memory-behavior closure.
- **4** managed-referent cases retain syntax acceptance and the compiler's CS8500 warning without inferring unmanaged denotation.
- **2** unsafe-context cases retain parser syntax acceptance and compiler CS0214 observations.
- **5** malformed direct type fragments require full consumption and remain rejected.

Existing ParentTypeNormativeTests, TypeConformanceTests/ParityTests and UnsafeConformanceTests/ParityTests retain the independent source-context, malformed/span, array-pointer difference and module-omission evidence. Cast, fixed, sizeof, stackalloc, statement and other source-context families are not globally promoted by this operand matrix.

The initial run passed all 174 parser observations and failed only the added four-row metadata closure test, before changing review declarations. No production fix is claimed. The exact four source identities are `A:360:5:type`, `A:377:39:type`, `A:377:43:pointer-type` and `A:377:46:unmanaged-type`. Proposed declarations bring the source crosswalk to **31 verified, 331 pending**, subject to independent review. Review states remain human metadata; the structural audit cannot authenticate a review.

## Verification

Focused tests covering PointerBoundaryNormativeTests, ParentTypeNormativeTests and both audit classes passed **573 observations**. The full solution command `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s` passed **706 normal + 2,650 oracle = 3,356 tests**, zero failed/skipped. The new count is 174 parser/phase observations plus one metadata closure test.

All 118 source hashes match both original and isolated Nitrogen; all six corpus hashes match provenance. Sibling Nitrogen remains at `f5bf525e3e2723a8fe39e127f800f6376345feb1` with only its preexisting modified `docs/editor-support.md`. Production dependency manifests still exclude Roslyn and Nitrogen.Grammar. The PDF hash was rechecked against the recorded pin. No grammar, parser policy, composition, dependency, public API or accepted-source change is included. Crosswalk bytes remain bound by CoverageIdentity. `git diff --check` passes.

## Independent review

Fresh read-only review of `1c24f34..422e770` inspected the full seven-file diff, approved design/Task 8, actual grammar/helpers, historical §25.2, identity integration and supplied execution ledger/log. It found no Critical, Important or Minor issues and accepted the four closures within their documented historical valid-recognition/contributing-production scope. No required valid historical syntax capability was identified as missing in that scope. No fix pass or deferred minors resulted.

The reviewer explicitly accepted the retained array-pointer phase difference as disclosed historically invalid-referent behavior, without certifying universal invalid-source trees, recovery or phase parity. It declined semantic denotation/binding/conversions/unsafe legality/layout/execution; whole cast/statement/interface/placement/fixed/sizeof/stackalloc/unsafe-interaction closure; unbounded nesting performance or exhaustive depths; independent execution/reproducibility; and independent source/PDF/corpus/sibling/dependency verification. It read the 706-normal/2,650-oracle passing log without rerunning it. Execution and preservation checks remain author-run evidence; finite structural observations do not establish resource behavior at arbitrary depths.
