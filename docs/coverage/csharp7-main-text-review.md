# CSharp7.3 prose alignment review

Reviewed all 26 working-text blocks and 9 PDF spans in the [comparison report](csharp7-main-text-differences.json), SHA-256 `afb657153538290fcd602ee1d68784998b94d2121669953faf66072065ec3d77`. The published PDF governs the syntax contract. The comparison uses a lossy normalization and ordered search, so this review covers the listed candidates. The working-text column identifies a chapter block by its start line. The PDF column uses the report's anchor after each span.

| Working-text blocks | PDF span before | Disposition |
|---|---|---|
| `terms-and-definitions.md:48` | `general-description.md:1` | The compile-time warning definition matches apart from the last character of its final word in the PDF extraction. No parse rule follows from this definition. |
| `expressions.md:3150` | — | The working text introduces the two allowed `stackalloc` contexts with a sentence absent on PDF page 225 (file page 253). The PDF retains the numbered list and the restriction that follows it. Use the list and restriction for the syntax contract below. |
| — | `expressions.md:832`, `expressions.md:852` | Each unmatched span is one `o` from the nested list in the type-inference section. Type inference itself is outside parser coverage. |
| `expressions.md:5109`, `expressions.md:5188`, `expressions.md:5226`, `expressions.md:5268`, `expressions.md:5331`, `classes.md:1347`, `structs.md:330`, `structs.md:378` | — | These informative example output blocks contain only digits and line breaks. The PDF normalizer drops standalone numerals with the page numbers. |
| `grammar.md:11`, `grammar.md:544`, `grammar.md:546`, `grammar.md:2676`, `grammar.md:2678`, `grammar.md:2760` | `portability-issues.md:13` | The search skips the Annex A grammar blocks and their intervening headings and boundary marker. The separate [annex comparison](csharp7-pdf-differences.json) verifies all 489 rule bodies. |
| `portability-issues.md:1`, `portability-issues.md:3`, `portability-issues.md:5`, `portability-issues.md:7`, `portability-issues.md:9`, `portability-issues.md:11` | `portability-issues.md:13` | The same span ends with the introduction to the list in informative Annex B. The search resynchronizes at its first numbered item. |
| `standard-library.md:587` | `standard-library.md:589` | The PDF spells the interface name `IFormattable`. The working paragraph omits the second `t`. The difference concerns a library contract. |
| `standard-library.md:609`, `standard-library.md:798` | `standard-library.md:781`, `standard-library.md:923` | Format-specifier tables differ in repeated page headings, HTML list tags and isolated cell text. The differences concern runtime formatting. The PDF remains the reference for those cells. |
| — | `documentation-comments.md:113` | The unmatched `<code>` is part of the §D.3.3 heading. The tag and its example appear in both texts. |
| `bibliography.md:1` | `bibliography.md:3` | The working heading includes the annex letter. The extracted PDF heading consists of the bibliography title. |

## Stack allocation in CSharp7.3

Under §12.8.21, the containing expression must be a local variable initializer or the right side of a simple assignment used as an expression statement. Within the containing expression, `stackalloc` may occupy the whole expression or one of the two result operands of a conditional expression that occupies the whole expression. The positive and negative witness pairs should exercise both hosts, both conditional operands, and nested uses that the C# 8 extension later permits. §23.9 supplies the separate unsafe result rules. Type conversion, allocation size and safe context require their own scope analysis. [Published seventh edition, file page 253](https://ecma-international.org/wp-content/uploads/ECMA-334_7th_edition_december_2023.pdf#page=253)
