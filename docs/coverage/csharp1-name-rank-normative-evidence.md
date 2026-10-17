# C# 1 normative name/rank review — 2026-10-17

This extends open PR #12 from `72c3c2c`; it does not merge that PR. The review closes eleven helper-production occurrences in the [historical crosswalk](csharp1-standard-crosswalk.json), while parent type/array/return categories, declaration/expression context families and CSharp1 remain partial.

## Normative correspondence

The SHA-pinned first-edition [ECMA-334](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), not Appendix A alone, is the authority. The following clauses/RHSs were compared with the authored grammars:

| Standard occurrences | Normative location | Recognition correspondence and boundary |
|---|---|---|
| `namespace-name`, `type-name`, `namespace-or-type-name` | §10.8, printed pp. 83–84 (PDF pp. 97–98) | one or more identifiers separated by dots becomes `NamesTypes.QualifiedName.Parts`; resolving namespace/type meaning is explicitly semantic |
| `qualified-identifier` | §16.2, printed p. 203 (PDF p. 217) | same identifier/dot sequence in `CompilationUnits.NamespaceDeclaration.Name`; lexical spelling/trivia and physical coordinates preserved |
| two occurrences each of `rank-specifiers`, `rank-specifier`, `dim-separators` | §11.2, printed p. 95 (PDF p. 109), and §19.1, printed p. 279 (PDF p. 293) | `ArrayRank` is brackets with an optional nonempty comma sequence. The nonempty suffix of `TypeReference.Ranks` is the repeated rank-specifiers helper; the owning type permits no suffix for non-array types |
| `predefined-type` | §14.5.4, printed p. 140 (PDF p. 154) | exact fifteen-keyword `NamesTypes.BuiltinType` set used by `Expressions.PredefinedMemberExpression`; `void` excluded and named/escaped spellings stay named expressions |

The rank helper equivalence is structural: `","*` implements the standard optional `dim-separators` (`","+`); a nonempty list of `ArrayRank` implements `rank-specifiers`. The broader `TypeReference` is not itself a standalone nonempty rank-list production. Parent `array-type` rows remain pending because element categories and their owning/unsafe contexts need separate closure. `return-type`, other semantic type wrappers, binding/conversions and actual array values remain outside this promotion.

`QualifiedName` is an identifier list with one-or-more elements and `.` separators, matching both normative left-recursive name definitions without admitting empty segments or reserved unescaped keyword segments. The independent historical Unicode/identifier evidence remains a dependency; this review does not reopen or promote that module/policy.

## Evidence contract

The earlier type checkpoint supplies 76 direct and 488 oracle observations, including the exact builtin set, raw qualified/escaped/contextual identifier spellings, rank order/count/comma coordinates, malformed/full-consumption fragments and source trivia. See [type evidence](csharp1-type-conformance-evidence.md), `TypeConformanceTests` and `TypeConformanceParityTests`. These finite execution observations support the grammar correspondence; they are not a claim of exhaustive rank length, name depth or every C# consuming context.

New `NameRankNormativeTests` compares raw name tokens and authored spans with explicit Roslyn CSharp1 syntax in using-import, alias, namespace, base-list and field contexts. Broken names are rejected in each owner and report physical syntax locations inside the malformed segment. Every normative predefined keyword exercises the actual typed member-access alternative and its type/name spans; `void`, `Void` and `@int` distinguish the excluded keyword from ordinary names. Malformed declaration ranks after a `#line` mapping retain physical diagnostic locations. Binding and member existence are not accepted-source gates.

The added metadata test first fails because no rows have yet been closed. New parser tests characterize existing behavior; no parser fix is claimed unless one is reproduced. Passing execution alone is not the reason for promotion: the normative grammar correspondence and review are both required.

Eleven rows receive verified review declarations with exact normative clauses and named fixture links in their notes. The audit continues to treat these as human metadata. No feature/module/policy status is promoted. The remaining 351 source occurrences and both global obligations still block completion.

## Characterization decisions

The initial run passed 74 parser observations and failed two diagnostic assumptions, alongside the expected unclosed-metadata failure. In the field spelling `N. f;`, trivia allows `N.f` to be consumed as one type name, so a missing field identifier is diagnosed at `;`. In `N.T[, f;`, the unclosed rank is diagnosed at the following `f` token. The corrected tests assert these physical syntax locations; neither case justifies a production change. Twelve additional source cases compare ordered rank and comma spans in field, parameter, method-return and typeof owners. Parent type/array/return productions remain pending despite these helper observations.

## Verification

Focused run: **143 passed** (88 new parser/context observations, one new metadata-boundary observation, and the existing 54 audit observations). Full solution command `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s`: **706 normal + 2,132 oracle = 2,838 passed**, zero failed/skipped.

No grammar, parser policy, production dependency or composition change was needed. All 118 pinned hashes match both original and isolated Nitrogen, and all six corpus hashes match provenance. Production dependency manifests still exclude Roslyn and Nitrogen.Grammar. The sibling retains only its preexisting documentation modification. The crosswalk byte changes participate in the existing CoverageIdentity; no new public API or resource kind was introduced.
