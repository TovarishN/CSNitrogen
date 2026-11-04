# CSharp6 normative context review

This review covers the sixth-edition main text (`V6-standard`, checked against the pinned PDF) and its source-local rules for the C# 6 additions. Each rule is assigned to the grammar, to a CSNitrogen check, or to a recorded exclusion. CSharp6 stays partial until the admission guard (Task 4) and a recorded approval exist.

## Main text against the PDF

`tools/compare-csharp6-prose.py` reduces every main-text paragraph outside code blocks to letters and digits and looks for it in the PDF body. That covers prose, notes, list items (one at a time), table rows and headings. The reduction follows Unicode NFKC normalization and removes markdown markup and the PDF's page headers, footers and page numbers. The result is in [csharp6-main-text-differences.json](csharp6-main-text-differences.json).

- 6,362 of 6,379 paragraphs are found in the PDF.
- The other 17 are dispositioned in [csharp6-main-text-dispositions.json](csharp6-main-text-dispositions.json), and none bears on syntax:

  | Disposition | Count |
  | --- | --- |
  | Annex headings set on separate lines | 4 |
  | Converted HTML/markdown tables whose cells the PDF reads in another order (operator-result, member-form, format-string and documentation-tag tables) | 10 |
  | Text inside a working-text HTML comment | 2 |
  | Explanatory sentence in an informative example (§11.7.6.2), added after the PDF; the PDF governs | 1 |

Grammar blocks are compared separately: all 436 Annex A rules agree ([csharp6-pdf-differences.json](csharp6-pdf-differences.json)).

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
| At most one general catch, and it comes last; a filter-only catch is specific | statements.md:1294, grammar | grammar (`TryStatement` handler order; filter-only catch is specific) |
| Later catch type not subsumed by an earlier unfiltered one | statements.md:1298 | semantic exclusion (type relations) |
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
