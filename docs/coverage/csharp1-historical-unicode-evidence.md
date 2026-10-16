# C# 1 historical Unicode continuation — 2026-10-16

PR #2 merged after fresh full suites at its reviewed head; main was synchronized before this continuation. The existing Nitrogen module/policy architecture now implements pinned Unicode 3.0 **BMP category behavior for CSharp1**. Other partial profiles retain the pinned current .NET 10 policy. All language profiles remain partial: this closes a category-compatibility obligation, not every C# 1 syntax obligation.

## Implementation

The [original Unicode 3.0 data](https://www.unicode.org/Public/3.0-Update/UnicodeData-3.0.0.txt) and accompanying Unicode redistribution notice are retained under `sources/Unicode3`, with SHA256 provenance. `generate-historical-unicode.py` expands First/Last records and produces immutable ranges for identifier start/part, formatting and space categories. Generation and license provenance are reproducible.

The shared Identifier token grammar admits the union of historical/current BMP categories. `CSharpSyntaxPolicy` validates Identifier nodes against the requested policy; raw grammar fragments must also apply that policy. This preserves the existing typed identifier Tokens. Global targets use independently selected GlobalTargetsHistorical/GlobalTargetsCurrent modules: exactly one is configured per named language. `GlobalAttributeSectionNode.Target` is now a typed GlobalAttributeTargetNode with an alternative containing its Token. Named profile metadata lists actual module membership and Unicode policy; custom metadata retains the current default and still does not execute arbitrary profiles.

CSharp1 source trivia, directive scanning, conditional symbol validation/identity, expression whitespace and line-directive arguments all use the same historical categories. Historical whitespace that is an identifier part in the current table is normalized to equal-length ASCII space outside literals/comments before parsing. OriginalText and all UTF-16 positions are preserved; literal/comment contents remain unchanged. This does not add C# binding, NFC validation or supplementary identifier support.

## Observations and verification

Unicode 3.0 has two identifier starts and eleven identifier parts absent from the current table. Conversely, many currently assigned characters were not historical identifier characters. Historical space/format classifications also differ. Roslyn's CSharp1 option still uses current Unicode categories, so these differences are recorded explicitly rather than treating modern Roslyn as a historical table oracle.

An independent C# reader consumes the original UnicodeData records and verifies every BMP code unit's identifier-start/part behavior through Nitrogen grammar plus policy. A public-parser fixture checks every BMP code unit in an otherwise empty class body against the historical whitespace category. All historical formatting characters are tested as raw characters and short/long Unicode escapes for global target ownership and preprocessing symbol identity. Existing current-category and current-target oracle loops remain in the suite under the current policy. Additional boundary fixtures ensure keyword prefixes cannot consume the beginning of a longer identifier; grammar lookahead guards ref/out/modifiers, and name/type alternatives consume complete identifiers.

The original [2001 C# specification](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §9.4, permits escaped keyword spellings as identifiers; normalization must not reinterpret them as keyword tokens. The exhaustive tests caught and removed such an overrestriction during implementation.

The deferred conditional-expression tests now recurse through both arms, nested false arms, assignments, targets and values, comparing authored spans with Roslyn. All three generation scripts were checked for deterministic/idempotent output, including unchanged current Unicode snapshot bytes.

Source-category tests: all 16 initial historical public pipeline fixtures first failed, then passed. The independent historical exhaustive run passed all six initial observations in five seconds; full final counts and fresh review outcomes are appended below. Historical BMP category parity does not prove exhaustive C# 1 production coverage or semantic identifier equality in a binder.

Final pre-review verification: 411 normal tests and 298 independent oracle tests passed (709 total). Historical boundary fixtures passed 33/33 after reproducing failures; constant/event/jump keyword alternatives also have complete-identifier guards. All three generators reproduce byte-identical outputs, the current Unicode snapshot remains unchanged, and all 118 recorded Nitrogen source hashes match both the original source and the isolated build copy.
