# CSharp6 normative context review

This review covers the sixth-edition main text (`V6-standard`, checked against the pinned PDF) and its source-local rules for the C# 6 additions. Each rule is assigned to the grammar, to a CSNitrogen check, or to a recorded exclusion. CSharp6 stays partial until the admission guard (Task 4) and a recorded approval exist.

## Main text against the PDF

`tools/compare-csharp6-prose.py` aligns the whole working text with the PDF in document order, chapter by chapter from the Foreword to Annex E.

- **What is compared:** every block, meaning prose paragraphs, notes, list items (one at a time), tables, headings, code blocks and grammar blocks. Text inside HTML comments is left out.
- **Reduction:** after Unicode NFKC normalization, each block is reduced to letters, digits and operator characters (`< > = ! ? & | ^ ~ + - * / %`). Markdown and HTML markup is removed outside code spans, entities are decoded, and escaped `\|` stays a literal. The PDF body is reduced the same way, with page headers, footers, page numbers, bare term numbers, list numbers and bullets removed.
- **Alignment:** each block must be found close after the previous match, so a far-away match is never used. A block of 200 or more characters may search further ahead to resynchronize.
- **Both directions:** a block with no match is reported as working-text-only. PDF text between two consecutive matches is reported as PDF-only, so a sentence missing from the working text shows up as PDF-only text.

The result is in [csharp6-main-text-differences.json](csharp6-main-text-differences.json): 7,588 of 7,624 blocks align, 36 blocks are working-text-only and 33 spans are PDF-only. Each one has a category in [csharp6-main-text-dispositions.json](csharp6-main-text-dispositions.json). Apart from the program-output digits, each working-text-only block pairs with a nearby PDF span of almost the same text.

| Category | Working-text-only | PDF-only |
| --- | --- | --- |
| Grammar comment layout: trailing comments and negated sets printed elsewhere; every rule body is verified against Annex A, and these are the only syntax-bearing rows | 7 | 8 |
| Code example layout: comments, quote marks or line order differ in informative examples | 8 | 8 |
| Program output digits, removed together with page numbers | 8 | 0 |
| Table layout: floating-point result tables, format strings, documentation tags | 11 | 8 |
| Informative edit after the PDF: an explanatory phrase in §11.7.6.2; the delegate example's variable `list`, which the PDF calls `delList` | 2 | 1 |
| PDF layout: repeated headings, bullets, the adoption notice, page fragments | 0 | 8 |

Mutation probes run on a copy of the working text are all reported:
- removing "shall be the last `catch` clause" (statements.md:1294) shows up as PDF-only text;
- `'=>'` → `'='` in `method_body`;
- `'?' '.'` → `'.'` in `null_conditional_member_access`;
- "is deleted" → "is not deleted" for Control-Z.

The reduction does not separate `.` from `,`, since list punctuation varies between the two texts. `?.` still differs from `.` through the `?`.

## Source-local rules for the sixth additions

Rules come from the "shall", "compile-time error" and "is an error" sentences of §11.7.3, §11.7.7, §11.7.9, §11.7.11, §11.7.15.3, §11.7.20, §11.8.8, §12.11, §13.5.4, §14.6.1, §14.7.1, §14.7.4, §14.9, §14.10.1 and §19.2.

| Rule | Text | CSNitrogen |
| --- | --- | --- |
| Format colon only when unnested in an interpolation | §11.7.3 context table | CSN2062: an unparenthesized conditional in a hole expression or width |
| Interpolated text excludes new-line; holes are tokens | §11.7.3 grammar | grammar (delimiter tokens); new-lines in holes accepted (recorded Roslyn divergence) |
| Width converts implicitly to `int` | expressions.md:1342 | semantic exclusion (conversion) |
| `?.` / `?[` target is not an unconstrained type parameter | expressions.md:1571, 1910 | semantic exclusion (typing) |
| `null_conditional_invocation_expression` preferred in a method body | classes.md:1922 | single tree: the invocation over the null-conditional node; the choice changes no shape |
| Initializer target names an accessible member, or an indexer evaluated once | expressions.md:2115–2127 | semantic exclusion (binding, evaluation) |
| A `nameof` method group has no type arguments; the target is not `dynamic` | expressions.md:2858 | semantic exclusion (binding); both readings kept under Ambiguous |
| `await` contexts: async body, not in a nested non-async function, lock body, expression tree or unsafe; catch/finally allowed | expressions.md:3116–3121 | CSN2050 (catch/finally allowed from 6); expression-tree conversion is semantic |
| `await` is not an identifier in an async function | expressions.md:3125 | CSN2051 |
| Exception type derives from `System.Exception` | statements.md:1288 | semantic exclusion |
| At most one general catch, and it comes last; a filter-only catch is specific | statements.md:1294, grammar | grammar (`TryStatement` handler order; filter-only catch is specific). The prose at statements.md:1294 calls a clause with neither type nor name general, which would include `catch when(f)`; the grammar (`specific_catch_clause : 'catch' exception_filter block`) and Roslyn make it specific, and CSNitrogen follows the grammar |
| Later catch type not subsumed by an earlier unfiltered one | statements.md:1298 | semantic exclusion (type relations) |
| A `null_conditional_invocation_expression` occurs only as a statement expression, an anonymous-function body or a method body | expressions.md:1803, statements.md:427 | CSN2001 accepts `a?.b();` (an invocation over the null-conditional node) and rejects `a?.b;`; lambda and method bodies take any expression |
| `null_conditional_invocation_expression` preferred in an anonymous-function body | expressions.md:4716 | single tree, as for method bodies |
| `using static` names a type; no alias form | §13.5.4 grammar | grammar (`UsingStaticMarker` excludes `=`); type kind is semantic |
| An async method has no semicolon body | classes.md:1951 | CSN2053 (an expression body is not a semicolon body) |
| An auto-property is non-abstract, non-extern, with a get accessor | classes.md:3359 | CSN2032 (lone `get;` allowed from 6); CSN2060 for initializers on non-auto, abstract or extern properties; CSN2063 for struct instance initializers |
| Expression bodies only for methods, operators, properties and indexers | §14.6.1, §14.7.1, §14.9, §14.10.1 grammar | grammar for properties and indexers; CSN2061 for constructors, finalizers and accessors in C# 6 |
| `static` not permitted on an indexer; operator `public static`; no modifiers on operator parameters; duplicate modifiers | classes.md:3930, 4175–4179 | inherited exclusion: general modifier compatibility (CSharp4/5); expression-bodied members behave like block-bodied ones |
| A type-name enum base resolves to an integral type | §19.2 | grammar (`NamedBaseEnumDeclaration`); resolution is semantic |

## Inherited gap noted, not changed

classes.md:3969 says a non-abstract, non-extern indexer with semicolon accessors is an error. CSNitrogen accepts `class C{int this[int i]{get;}}` in every profile, including the admitted CSharp1–5, which have the same rule. This review records it and does not change predecessor behaviour.

## Readings carried from earlier versions

The Version5 lexical, preprocessing, attribute and iterator readings are reviewed in [csharp6-historical-reading-review.md](csharp6-historical-reading-review.md). The exclusions inherited from CSharp4/5 carry forward unchanged: semantic binding and inference, general modifier compatibility and finite modifier sets, accessibility, explicit pointer type-argument legality, variance safety, constantness and default legality, control flow and runtime, awaitability and task types, and caller-info substitution.
