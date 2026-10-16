# C# 1 lexical and preprocessing milestone — 2026-10-16

Status: implemented subset; reviewed fixes and final verification recorded below. All C# version profiles remain partial. Full types, declarations, statements, and expressions are not implemented.

## What is executable

`Grammars/Lexical.ngr` contains real Nitrogen identifier, integer, real, character, regular-string, and verbatim-string categories. `Core.ngr` consumes the shared identifier token, accepting Greek names, `@class`, and valid Unicode escapes. Generated lexical-fragment rules are available for consumers and tests; literals are not yet admitted as class members or general source expressions.

Identifier tables and escaped-code-point tries are generated from checked-in BMP category data, recorded in `unicode-data.json`. All 65,536 BMP start/part classifications and all generated four-digit escaped start/part spellings are compared with pinned Roslyn 5.0.0. The tables are fixed build inputs, not rediscovered at parse time. Supplementary identifier characters are outside this compiler-compatible subset; no claim is made about a historical Unicode-version freeze. `eng/GenerateUnicode` records category data, and `eng/generate-lexical-grammar.py` emits the grammar. Runtime snapshot and SDK remain unchanged.

`ConditionalPreprocessor.Process` scans active lexical states and conditional activity together. It supports definitions, restricted boolean expressions, nested `#if/#elif/#else/#endif`, regions, error/warning directives, and classic `#line` mappings. It masks directives and inactive code with equal-length spaces, retaining all C# newline characters and physical UTF-16 positions. It does not invoke or evaluate C# code.

The public `CSharpParser.Parse` runs preprocessing before Nitrogen parsing. `OriginalText` contains authored text; `MaskedText` and `Tree.Text` contain the coordinate-preserving parse buffer. `Directives`, `InactiveSpans`, and `LineMappings` remain available independently of borrowed syntax-node lifetime. `MapLocation` exposes physical and logical zero-based locations, optional mapped file, and hidden state. The result still owns/disposes the Nitrogen tree.

Known later-version directives produce unavailable-feature diagnostics before their introduction, and unsupported-coverage diagnostics afterward. They are not silently accepted as implemented. Inactive later-version effects are ignored as in Roslyn. These policies do not constitute full support for modern preprocessing or modern strings.

## Tests and corrected discrepancies

- Real `.ngr` fragment fixtures for Unicode/escaped identifiers, C# 1 number syntax, escape bounds, characters, and regular/verbatim strings; later binary/separated numbers, interpolation/raw/UTF-8 strings, and `\e` are excluded from C# 1.
- Active verbatim strings/comments retain directive-looking content. Inactive malformed strings do not hide `#endif`.
- Conditional evaluation uses explicit precedence and validates every operand, including otherwise-short-circuited malformed expressions. Nesting beyond 256 reports an error.
- Definitions normalize Unicode escapes/remove format characters; caller symbol lists are not mutated. Reserved C# keywords can be conditional symbols. Literal `true`/`false` definitions are excluded; escaped boolean spellings are admitted as definitions, while decoded boolean spellings evaluate as literals in expressions, matching the pinned oracle.
- Masking preserves CR/LF/CRLF/NEL/LS/PS and all UTF-16 offsets, including a leading BOM and supplementary text. A BOM on a masked directive line becomes a space at the same offset; its authored character remains in OriginalText.
- Conditional/region structure, directive placement, trailing arguments, unknown directives, diagnostic severity, and line mapping are tested through the public pipeline and Roslyn observations.
- The oracle exposed inactive `#line`/`#nullable` effects, the PDB line-number limit, Unicode directive-name boundaries, and compiler-reserved intrinsic identifiers. These failures were retained and fixed. A PDB-overflow line directive issues a warning and leaves mapping unchanged, after its remaining arguments are validated; malformed filename/suffix syntax still rejects the file.
- Coverage hashes now include all production grammar and preprocessing/profile/parser source resources with sorted, length-delimited identities. Tests prove enumeration-order independence and input-content sensitivity.

See the source-grounded [lexical/preprocessing specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure). Current specification references plus explicit Roslyn C# 1 observations support this bounded subset; they do not verify every historical feature distinction.

## Coverage and limits

Ten baseline ledger entries were reviewed for this subset and mapped to their grammar/policy/fixtures. The remaining imported release-history entries remain unreviewed. Ledger requirement references/cycles now have an integrity test. Production module membership is Core plus Lexical; host preprocessing policy is registered separately from syntax membership.

Arbitrary raw-string delimiters remain an unresolved later-version gate. Modern directive effects (`#pragma`, `#nullable`, enhanced `#line`, and file-based directives) remain unsupported at or above their introduction; no Nitrogen sibling changes are authorized or made by this milestone.

Re-running `eng/create-feature-ledger.py` imports the history inventory; preserve reviewed entries when refreshing it. A follow-up should make this refresh merge reviewed metadata automatically. Dependency snapshot refresh remains nontransactional, as noted in the prior review.

## Verification commands

```sh
dotnet build CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
dotnet test tests/CSNitrogen.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
dotnet test tests/CSNitrogen.Oracle.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

Final counts, review findings, and isolation audit are appended after verification. Logs and RED/GREEN evidence are under `.superpowers/sdd/csharp1-lexical-preprocessing`.


## Fresh review and fix pass

The fresh reviewer found two Important cases: escaped boolean-symbol spellings were rejected after decoding, and oversized line numbers returned before filename validation. Both have retained RED fixtures and corrected behavior, with defined/undefined escaped booleans, malformed/valid oversized filenames, and active-class/mapped-location oracle observations. The review also noted imprecise BOM wording; the description above now distinguishes offsets from character preservation.

The author additionally reproduced and excluded the compiler intrinsic identifier spellings `__arglist`, `__makeref`, `__reftype`, and `__refvalue`. Generated escape-trie equivalence, which the reviewer had not independently checked, now has exhaustive BMP execution coverage. These checks do not replace later semantic validation or full source-grammar acceptance.

The next reviewable implementation plan is [C# 1 source grammar](../superpowers/plans/2026-10-16-csharp1-source-grammar.md), including explicit typed-root migration, all source categories, unsafe composition, and a corpus gate.

## Final verification

Solution build: succeeded, zero warnings/errors. Normal suite: 180/180 passed. Oracle suite: 84/84 passed, including exhaustive escaped BMP identifier tests. Total: 264 passed, zero failures/skips. Both suites used a 30-second hang timeout in their separate VSTest testhost processes.

The copied Nitrogen source hashes and original dirty state were rechecked and unchanged. Neither production dependency manifest contains Microsoft.CodeAnalysis. No Git initialization, commits, sibling source edits, merges, or publication occurred.

Decisions carried forward: fixed BMP grammar tables avoid a new scanner API but exclude supplementary identifier support; directive edge behavior follows the pinned oracle without claiming universal historical behavior; escaped boolean definitions and decoded boolean expression classification are distinct. The review's BOM note was addressed by documenting offset preservation precisely. Deferred tooling limitations remain nontransactional dependency snapshot refresh and history-inventory refresh overwriting reviewed metadata unless preserved manually.
