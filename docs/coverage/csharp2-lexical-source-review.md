# Fourth-edition lexical/preprocessing read-only source review

Reviewed local evolving Unicode4 changes without builds/tests or repository edits. Primary source ECMA-334 fourth edition SHA256 de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652, extracted using pinned pypdf6.10.0. Findings are static/source observations, not executed failures. All source rows remain pending until root verifies.

## Normative bounds and owners

§9.1–9.2 source/lexical/syntactic conventions; §9.2.3 generic token ambiguity is covered by expression review. §9.3 lexical longest element and source input/newlines/comments/whitespace; §9.4.1 Unicode escapes; §9.4.2 identifiers; §9.4.3 keywords/contextual identifiers; §9.4.4 literals; §9.4.5 operators; §9.5 directives and §9.5.1–9.5.8 symbol/expression/declaration/conditional/diagnostic/line/region/pragma rules. Clause2 PDF? declares Unicode4.0 (text lines888–889); §9.4.2's note referring Unicode3.0 is an outdated nonnormative reference, not permission to substitute Unicode3 tables.

Annex lexical rows run A465:input through A472:pp-pragma-text. Shared owner normalization: CSharpTrivia/DirectiveScanner/SourceLines for source, whitespace/comments/newlines; Lexical.ngr generated tokens and UnicodePolicy for token grammar/profile categories; ConditionalExpression/ConditionalSymbols/ConditionalPreprocessor for preprocessing expression and source masking; DirectiveEffects for diagnostic, line, region, pragma and feature availability. Existing CSharp1 coverage maps are owner candidates only, requiring CSharp2 source-context and span witnesses.

Unicode4 generator reads pinned UnicodeData-4.0.0.txt SHA77818a7e04c563a6a628e4f10be60323bf13f08e9fb208bffce11057b008ad7e and expands First/Last ranges. Categories map Lu/Ll/Lt/Lm/Lo/Nl plus literal underscore to start; these plus Nd/Pc/Mn/Mc/Cf to part; Cf to identity removal; Zs to spaces. This matches §9.3.3 and §9.4.2. TryNormalize scalar handling and generated raw surrogate-pair alternatives are suitable mechanisms. Non-NFC behavior is implementation-defined (§9.4.2 PDF93), with no required diagnostic: document the chosen behavior rather than inventing rejection. Binding identifier equivalence is excluded except where needed for parsing contextual words, attribute targets and conditional symbols.

## Findings

### L1. Historical policy switches often still mean version1, although fourth edition requires the same behavior

* ConditionalPreprocessor strips trailing U+001A only under IsHistorical(version1). Fourth §9.3.1 PDF89 mandates deletion of a final Control-Z. CSharp2 `class C{}\u001a` must accept with original spans and only the final marker removed/masked. Internal Control-Z remains invalid.
* DirectiveScanner argument whitespace is enforced only for IsHistorical(version1) or pragma2. §9.5 grammar requires whitespace after if/elif/define/undef/line, and before a nonempty error/warning/region/endregion message. Check `#if(true)\nclass C{}\n#endif`, `#if!false`, `#warning!`; these must diagnose in CSharp2. Ordinary keyword-boundary checks do not substitute for directive lexical whitespace.
* ConditionalExpression turns normalized true/false into literals for every non-CSharp1 profile. Fourth §9.4.1 and §9.4.2 say escapes do not form keywords; §9.5.1 conditional-symbol includes identifier. Thus `#if tr\\u0075e` without define is false; defining it makes it true. Likewise formatting spellings `tr\u200Cue` are identifiers whose identity equals true, not literal keyword tokens. Existing HistoricalConditionalIdentityTests exercise version1 only. Extend historical edition2 behavior explicitly and keep modern discrepancy documented.
* ConditionalSymbol token omits optional @ and ConditionalSymbols.TryNormalize uses verbatim=false. §9.5.1 `conditional-symbol ::= identifier | any keyword except true/false`; identifier admits @. Primary grammar therefore admits `#define @class` and `#if @class`, and @true is an identifier spelling, not raw true keyword. Compare reference diagnostically, classify modern behavior; do not inherit rejection without primary-source accounting.

### L2. Supplementary generated escape prefix uses lowercase-only hex for plane14

eng/generate-lexical-grammar.py emits `"\\U000e" HexIn(...)` for plane14. Hex digits are case insensitive (§9.4.1 + hex-digit), so `class C\\U000E0100 {}` must recognize the same Mn part as lowercase `\\U000e0100`. Prefix hex letters need case-insensitive matching too. Test raw/uppercase/lowercase escape supplementary part categories: U+104A0 Nd, U+1D165 Mc, U+E0001 Cf and U+E0100 Mn; start/part restrictions differ. Raw scalar and escaped scalar must preserve UTF16 source-span lengths (2 versus10 code units). Isolated surrogates and >10FFFF escapes remain invalid.

### L3. Keyword adjacency remains BMP/current-category based

CSharpSyntaxPolicy.IsIdentifierPart(char) uses current char categories and checks the first code unit at an adjacent leaf boundary. Raw supplementary high surrogates are false; a keyword prefix may split `return𐐀;` into return + Identifier without CSN2002. Check `class C{object M(){return𐐀;}}` (historically this is an identifier statement, not return statement and cannot be accepted), `class𐐀{}` (single identifier cannot introduce class), `int𐐀;` in a declaration-capable context, plus supplementary at both ends of names. Use selected scalar Unicode4 categories and preserve one original span. Existing `_version` char helpers in DirectiveScanner are harmless for valid literal directive names only if concatenated invalid names still reject; assert #if𐐀 rejection rather than reading it as if with an argument.

### L4. Contextual identifiers must recognize normalized spellings

§9.4.3 PDF94 explicitly names add, alias, get, global, partial, remove, set, value, where, yield and states these are identifiers, which can use @ prefixes and Unicode escapes. Grammars largely use raw literals, and policy Text comparisons assume raw words. Required witnesses include `@partial class C {}`, `part\\u0069al class C {}`, `class C<T> wh\\u0065re T:class {}`, `extern @alias A;`, property `@get`/escaped set, event escaped add/remove and `@yield return 1;`. Their ordinary identifier role must remain accepted where context permits. Matching terminal grammar and context policies must share selected identifier equality; new token wrappers should retain typed fields/spans. This is a normative historical-versus-current compiler discrepancy if Roslyn refuses these spellings; the primary note is explicit.

### L5. Attribute target normalization and arguments still inherit modern profile policy

Fourth §24.2 PDF397–399 and Annex491 require attribute targets to use identifier equality (§9.4.2), including @/escapes/Cf removal. Current CSharp2 composition selects GlobalTargetsCurrent.ngr, which is generated from current BMP Cf and TargetPart. It lacks Unicode4 supplementary Cf between assembly letters; e.g `[assem\\U000E0001bly:A] class C{}` should normalize assembly and be global. Current TargetFormat erroneously treats U+180E as formatting although Unicode4 categorizes it Zs. Its negative boundary excludes only TargetPart and UnicodePart, not raw ScalarPart. A selected Unicode4 module or broad identifier-token + normalization/context policy is needed; do not reuse current Cf data as Unicode4 proof. Validate normalized global scope and exact targets/spans after comments/preprocessing.

CSharpAttributePolicy historically permits assignment argument expressions and warns on unrecognized targets only for version1. Fourth §24.2 grammar retains `attribute-argument-expression ::= expression`, so `[A(x+=1)] class C{}` and named `X=y=1` are recognized syntax; value/constant validity is excluded. Unknown target warning applies to edition2 too; the standardized target list adds typevar. Do not simply extend the version1 list unchanged. Global module is only a Microsoft supplement; ECMA standard global name is assembly. Document module as the existing compatibility extension, not an ECMA literal alternative.

### L6. Newline source discrepancy must be explicit

§9.3.1 PDF89 and Annex465 print “Next line character (U+2085)”, while §9.3.2/PDF90 new-line-character prints U+0085, Unicode names identify U+0085 as NEL, and U+2085 is subscript five. Current implementation selects U+0085 (SourceLines, trivia, literal rejection), which is a reasonable compatibility choice but requires an explicit source-typo discrepancy, rather than claiming literal agreement with both source productions. Test NEL splitting comments/directive lines, raw NEL rejection in char/regular strings and preservation in verbatim strings. U+2028/U+2029, CR/LF/CRLF remain finite required alternatives. Virtual final newline behavior can preserve original text length while accepting final directives without newline.

## Existing implementation observations and evidence needs

* CSharp2UnicodeTests currently witnesses raw/escaped U+10400 identifiers, raw supplementary part, U+180E whitespace and supplementary conditional symbols. Negative examples test U+1C90 modern-only, escaped initial underscore, isolated high surrogate and out-of-range escape. Add all supplementary category branches above, hex prefix case, Cf normalization and keyword adjacency.
* TryNormalize disallows escaped underscore in initial position for versions1/2, consistent with grammar's literal-only initial underscore alternative. It validates all identifier characters under selected historical categories and removes Cf after validation. Raw later Unicode scalars accepted in superset must fail selected Unicode4 policy.
* Lexical regular strings/characters exclude CR/LF/LS/PS in grammar; CSharp2 UnicodePolicy explicitly rejects raw NEL afterwards. Strings support >FFFF escape32; chars only escape16. Literal numeric ranges/conversion/rounding are excluded semantics; syntactic suffixes/digits/exponent signs and escape bounds remain in scope.
* Right-shift and shift-assignment remain contiguous lexemes in authored Expressions tokens. Fourth §9.4.5 explicitly forbids any character between component > and > or >=, so `a > > b` and `a >/*x*/> b` must reject while nested generic closers may have trivia. Do not change shift token rules merely because the normative grammar lists two tokens.
* DirectiveEffects fourth-edition unknown #line identifiers warn with previous mapping retained; identifier equality is used for default, decimal mapping is unbounded and preserves exact BigInteger. Modern hidden behavior is a documented implementation-defined line-indicator choice. Unknown #pragma is warning/no compiler effects, consistent with §9.5.8 obligation to permit/ignore unrecognized pragma.
* Inactive directive lexical grammar still matters. Unknown directives currently error; version1 applies #line and later-directive lexical diagnostics even in inactive regions, version2 often skips through active-only conditions. Review inactive malformed #line, #nullable, invalid #pragma prefix and region boundary fixtures against fourth lexical grammar; do not silently suppress syntax validation merely because effects are inactive.
* CSharp2 normal/oracle source fixtures need inherited literals/comments/directives + Unicode4 original UTF16 spans and diagnostic anchors. Existing CSharp1 suites provide source owner evidence only. No tests executed here.

## Extractor validation

Directly scanned PDF465–494 using pinned pypdf6.10.0 and expanded regex accepting optional `one of`. Existing397 heading rows become409, with exactly12 omissions:

A:468:3:keyword
A:468:39:decimal-digit
A:468:42:integer-type-suffix
A:469:2:hex-digit
A:469:12:sign
A:469:15:real-type-suffix
A:469:26:simple-escape-sequence
A:470:18:operator-or-punctuator
A:475:29:predefined-type
A:478:2:assignment-operator
A:486:43:overloadable-unary-operator
A:487:2:overloadable-binary-operator

These finite alternatives must be mapped even when normalized/inlined. No source statuses were promoted and the extractor itself was not edited in this review.
