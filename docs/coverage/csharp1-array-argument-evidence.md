# C# 1 array/initializer/argument continuation — 2026-10-17

PR #5 was merged at its verified head after a fresh 1,923-test run. This continuation starts from merged main `eb2f6fc` and follows the approved source grammar plan Task 2. All version profiles remain partial. The [observation ledger](csharp1-array-argument-productions.json) maps the selected productions and interactions; it is not a whole-expression completion claim.

## Behavior corrected

The pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §14.5.6, excludes direct array creation from element-access targets. The shared Pratt expression grammar had admitted `new int[1][2]` as an element access on a new array. Four source fixtures reproduced the false acceptance before a six-line generated-syntax policy correction.

`CSN2005` rejects an ElementExpression whose immediate Target is SizedArrayExpression or InitializedArrayExpression. The selected source baseline applies this rule across profiles, with CSharp1 and CSharp14 fixtures. Raw grammar fragments require `CSharpSyntaxPolicy.Check` to enforce the restriction. Parentheses and member access change the immediate target and remain accepted: `(new int[1])[0]` and `new int[1].P[0]`. The rule does not inspect bound types. Tests assert exact authored diagnostic span and Syntax category.

Modern Roslyn CSharp1 accepts initialized direct indexing such as `new int[]{1}[0]`; this is recorded as a formal-reference discrepancy rather than weakening the baseline. Repeated explicit size brackets are diagnosed at compilation as CS0178, while missing sizes/initializers produce CS1586. Separate tests retain those phase/code observations. Named arguments, out declarations, in arguments and implicit arrays require CS8022 under the pinned CSharp1 compiler oracle.

## Typed evidence

The reusable test-only `ExpressionShapeAssert` was extracted from the previous cast fixtures, preserving their recursive checks. It now compares actual generated array element types, explicit dimension expressions and separators, omitted rank order and comma spans, optional initializers, nested element values, interior/trailing commas, argument values/ref-out modifiers and separators. The initializer's existing optional Group/List representation is inspected explicitly through the generated Items field; no production API migration was introduced. Unknown expression cases still fail.

Literal comparisons now read `LiteralExpressionExpression.Value` and assert the typed child span/text. This closes the deferred PR #5 literal projection detail while testing initializer literals and all prior cast literal forms.

New observations: 32 normal and 163 oracle cases. The oracle set includes 124 array shapes (112 element/dimension/rank combinations plus 12 boundary/nested/comment shapes), 10 call/creation argument shapes, four declaration/constructor contexts, four direct-indexing classifications, four later-version contexts, fourteen malformed syntax cases and three missing-size classifications. The same comparer runs over all 587 prior cast/operator fixtures.

Initial unsupported-comparer failures were test coverage under construction, not claimed production regressions. The actual production defect reproduced 4/4 RED in direct-target tests; corrected source acceptance and exact diagnostic checks pass. Compiler-phase assumptions were corrected and retained in the ledger. No sibling repository or Nitrogen core changes were made.

Full pre-review verification: **582 normal + 1,536 oracle = 2,118 tests pass**. Rank/dimension value matching, constant-expression requirements, ref/out writability, overload/constructor binding and execution remain excluded. Finite nesting/type/rank observations do not establish full expression or unsafe/context closure.

Decision: enforce the published standard's immediate-target restriction through the existing syntax-policy architecture. This intentionally diverges from compiler-accepted initialized direct indexing; the cost is documented and covered by explicit discrepancy tests.

## Fresh review

Fresh read-only review of `eb2f6fc..371212d` judged the checkpoint ready to merge with no Critical, Important or Minor findings. No additional fix pass or deferred minor resulted. The prior literal projection detail is closed.

Review boundaries remain semantic validity; complete statement/interface/namespace/unsafe closure; exhaustive malformed/recovery behavior and every intermediate profile. Extrapolating the finite fixtures would overstate those guarantees. The reviewer did not independently extract the historical PDF or run builds/hash checks under its read-only/no-network instructions. The author read the official reference text and ran the complete tests/dependency audit; these remain author execution evidence.
