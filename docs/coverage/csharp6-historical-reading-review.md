# CSharp6 sixth-edition conformance review (in progress)

Task 3 of [the CSharp6 plan](../superpowers/plans/2026-11-04-csharp6-grammars.md). This is review evidence, not admission: CSharp6 stays partial.

## Source index and PDF agreement

- `tools/extract-csharp6-standard-index.py` indexes every ANTLR rule of the pinned `V6-standard` `grammar.md`: 435 rules (94 lexical, 325 syntactic, 16 unsafe) with 1,114 top-level alternatives. Each rule is linked to the main-text chapter block that defines it. All 435 bodies equal their chapter blocks. The count is confirmed by an independent scan.
- `tools/compare-csharp6-pdf.py` parses Annex A of the SHA-pinned PDF through the pinned PDFKit extraction. All 435 rule bodies agree with the working text after whitespace, comment and typographic-quote normalization. There are no rules found only in the PDF, so no difference needs a PDF-wins disposition ([csharp6-pdf-differences.json](csharp6-pdf-differences.json)). Main-text prose outside the grammar blocks is not compared by this tool.

## Owner crosswalk

`tools/build-csharp6-owner-crosswalk.py` gives every rule its owners in the actual CSharp6 factory ([csharp6-standard-crosswalk.json](csharp6-standard-crosswalk.json)):

| Relation | Rules |
| --- | --- |
| name-carried (same name as a CSharp5 rule; inherits its owners) | 364 |
| name-carried-extended (carried, plus sixth-module owners for added alternatives) | 9 |
| renamed (an ECMA renaming of named CSharp5 predecessors, e.g. `finalizer_declaration` for `destructor-declaration`) | 27 |
| extended (carried predecessor owners plus sixth owners, e.g. `initializer_target`, `property_body`) | 4 |
| sixth-addition | 31 |

The 48 CSharp5 names that are absent are mostly plural list rules that ECMA writes as `x*`/`x+`; they are listed in the record. `CSharp6OwnerCrosswalkTests` resolves every owner against the factory's grammar sources with Nitrogen's grammar parser. Owner existence is not normative equivalence.

The crosswalk test found a Task 2 defect. The CSharp6 factory still selected `GlobalTargetsCurrent.ngr`, whose global-target format characters come from current Unicode (for example U+0604 and U+061C), although CSharp6 follows the approved frozen Unicode4 contract. CSharp6 now selects `GlobalTargetsThird.ngr`, as CSharp3–5 do, and `ProfileResolver` maps it. `Global_attribute_targets_follow_the_frozen_Unicode4_reading` pins this: `[as؄sembly:A]` behaves as in CSharp5.

## Historical readings

These are the Version4/5 readings that CSharp5 kept and that CSharp6 had left at current-compiler behaviour. Each is decided from the sixth-edition text and pinned by `CSharp6HistoricalReadingTests`:

| Reading | Sixth-edition text | CSharp6 |
| --- | --- | --- |
| Trailing Control-Z deleted | lexical-structure.md:177 | kept |
| Whitespace required after `#if`, `#elif`, `#define`, `#undef`, `#line` and message directives | `PP_Whitespace` in the preprocessing grammar | kept (CSN1001) |
| Skipped directives must be lexically correct (`#nullable`, a malformed `#line` in a false section) | lexical-structure.md:1223, 1268 | kept |
| Exact `#line` indicator forms | `PP_Line_Indicator` | kept |
| Attribute targets: non-standard names warned; `assembly`/`module` only in global sections | attributes.md:260, 275 | kept; `typevar` is standardized again, as in the second edition |
| Attribute arguments: `attribute_argument_expression : expression` | grammar.md `attribute_argument_expression` | kept (no CSN2006; constantness is semantic) |
| Yield restrictions (CSN2022) | statements.md:1653 and §13.15 | kept |
| Escaped `_` as an identifier start | `Underscore_Character` includes `_` | allowed (current reading) |

CSharp7 and later are not reviewed and keep current-compiler behaviour. The CSharp5 test's later-profile probe now uses CSharp7.

## Executed alternative matrix

`tools/build-csharp6-alternative-matrix.py` links each of the 1,114 sixth alternatives to an explicit witness ([alternative-witness-matrix.json](../../tests/Corpus/CSharp6/alternative-witness-matrix.json)). `CSharp6AlternativeMatrixTests` executes every witness on the CSharp6 factory.

- **710 positional links.** These rules are carried by name and have the same number of alternatives as their single CSharp5 occurrence, so each alternative reuses the exact CSharp5 matrix case in order. They run through the same inherited lexical, finite and production checks.
- **404 sixth-specific witnesses** ([source-specific-cases.json](../../tests/Corpus/CSharp6/source-specific-cases.json), authored in `tools/csharp6_witnesses.py`). They cover every new, renamed or restructured rule. Each is a compilation unit, prefixed with a supplementary-character comment and a new line, that must be accepted. A node or directive record must cover its focus text at the recorded offset.

The matrix found two problems:
- **enum_base.** It is `':' integral_type | ':' integral_type_name` (§19.2), and no release-history entry covers it. The CSharp6 factory rejected type-name bases. The new module `EnumBaseTypeNames.ngr` (capability `csharp6.enum-base-type-names`, in CSharp6 and later factories) adds a separate `NamedBaseEnumDeclaration` owner, so keyword bases keep the baseline `EnumDeclaration`. Whether the name resolves to an integral type is semantic. Roslyn at version 6 parses the same spellings without diagnostics.
- **Identifier_Start_Character.** The inherited CSharp5 case asserted that `\u005f` is rejected; the sixth edition allows it, so this rule uses sixth-specific witnesses.

Positional links rely on the CSharp5 alternative order of a same-named rule. That is a link, not a normative equivalence review.

## Remaining

Task 4: the independent CSharp6 admission guard and its recorded approval.
