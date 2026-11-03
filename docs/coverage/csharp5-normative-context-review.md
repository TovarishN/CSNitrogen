# CSharp5 normative context review

Sources: the pinned Version5.0 DOCX `6f12b6a3…` (text `9e1d368e…`) and the admitted Version4.0 DOC `cd82c06f…`, both extracted with macOS `textutil` and split at LF only. No document text is redistributed. This review is source inspection for Task3; CSharp5 remains partial and unadmitted.

## Appendix occurrences

`tools/extract-csharp5-standard-index.py --compare-fourth` indexes 458 Appendix B occurrences (431 names) and compares them with the admitted CSharp4 index by grammar domain, heading and within-family ordinal. 455 bodies are identical, none are added or removed, and 3 differ:

- `new-line` and `new-line-character`: Version4 prints NEL as U+2085. Version5 prints U+0085, the reading CSNitrogen has implemented since CSharp2.
- `pointer-member-access`: Version5 adds `type-argument-listopt`, which the existing CSharp2 generic pointer-member alternative already implements.

The appendix contains no async or await production. Version5 adds them only in the main text (4430, 5174–5175, 6891), so the summary appendix alone cannot establish fifth-version coverage.

## Main-text differences

`tools/compare-csharp5-main-text.py` finds 707 normalized difference regions before the appendix. `tools/build-csharp5-main-text-dispositions.py` assigns every region a disposition:

| Disposition | Regions |
|---|---:|
| field-and-layout-normalization | 589 |
| retained-semantic-or-editorial-boundary | 100 |
| async-functions | 6 |
| await-expressions | 5 |
| caller-info-attributes | 3 |
| generic-member-transcription | 2 |
| preprocessing-trailing-comment | 1 |
| modifier-compatibility | 1 |

Version4 is a DOC with unresolved cross-reference fields; Version5 is a DOCX with resolved section numbers, so most regions differ only in `§ <ref>` versus `§2.4`, Word `XE` index fields or table layout. The builder classifies those mechanically and fails if any other region lacks a reviewed disposition.

## Parser-affecting findings

- **Semicolon-bodied async methods (main6913).** "If the method-body consists of a semicolon, the declaration may not include the async modifier." Abstract, extern, partial-defining and bodyless async methods are now rejected at the modifier with `CSN2053`. Async anonymous functions always have bodies.
- **Async partial methods (main6891, 6519–6526).** `async` is a method modifier, and partial methods do not exclude it. The historical partial-method check (`CSN2032`) now admits `AsyncModifier` from version5, so an implementing `async partial void M(){}` is accepted. The requirement that defining and implementing parts carry the same modifiers spans declarations; it remains excluded declaration-merging semantics.
- **#endregion comments (main1656).** Verified for CSharp4 and CSharp5: region text is pp-message, which already accepts the comment.
- **Await placement (main4430–4437).** The source list (nested non-async anonymous functions, catch/finally, lock blocks, unsafe contexts, translated query positions) matches the existing `CSN2050` policy exactly.

## Rulings

- Duplicate `async` modifiers and the new abstract/extern event-initializer sentence (main7310) are modifier-compatibility rules. General modifier compatibility is an inherited explicit exclusion of the normalized contract; extending it only for these spellings would make the boundary inconsistent. Cost if wrong: the partial candidate accepts these invalid combinations.
- Async return types (void/Task/Task<T>), async iterators and task-returning evaluation depend on type identity, so they remain semantic. Cost if wrong: syntax acceptance can be mistaken for a valid async signature.
- Caller info attributes change only compiler substitution; attribute syntax is unchanged.

## Remaining Task3 work

The source-alternative witness matrix, the owner crosswalk against the actual 32-module CSharp5 factory, the reviewed-occurrence overlay and the independent fail-closed CSharp5 admission guard remain. The index, delta inventory, difference index and dispositions are mechanical or inspected inputs; none of them awards admission.
