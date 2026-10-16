# C# 1 statement/control-tree continuation — 2026-10-17

PR #6 was merged at its verified head after a fresh 2,118-test run. This continuation starts from merged main `bec12c9` and follows the approved source grammar plan Task 3. All profiles remain partial. The [production-family ledger](csharp1-statement-productions.json) maps all 32 authored statement productions/categories/alternatives to finite observations; mapping is not exhaustive conformance proof.

## Production correction and preserved scope

The pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §15.7.2, limits each switch to one default label. The existing grammar accepted repeated defaults in one section or separate sections. Three public source cases reproduced false acceptance before adding `CSN2006` to the generated-syntax policy. The checker traverses only the current switch's direct sections/labels, unwraps the generated choice group and compares the exact keyword. Nested and subsequent switches keep independent state. Tests assert the offending keyword's authored span, Syntax category and accumulation of multiple diagnostics. CSharp1 and CSharp14 baseline selections are checked.

Missing using-resource initializers were initially mistaken for an additional parser defect. Full regression testing exposed the original context corpus and earlier report's deliberate semantic exclusion. The proposed initializer check was removed; the original corpus remains unchanged. CS0210 is observed during compilation while syntax is accepted, preserving the established parser contract. AST-inspectability alone does not override that agreed boundary. No CSN2007 policy is emitted.

## Independent typed observations

The test-only `StatementShapeAssert` follows actual generated statement/embedded wrappers and child accessors. It compares ordered block statements, conditional/else association, loop initializer/condition/iterator/body fields, foreach types/identifiers/sources, switch section and label grouping, declarations/initializers/separators, labels/jump spellings, checked/lock/using bodies, catch declarations and finally blocks. It delegates expression children to the existing recursive expression comparer. Exception-handler collection stops at each immediate handler instead of counting handlers in nested bodies. Unknown statement forms fail explicitly.

New observations: 38 normal and 139 oracle tests (177 total). The oracle set has 116 recursive shapes: 72 nested body combinations, 18 for-header combinations and 26 selected switch/exception/declaration/comment/newline cases. Six compiler-deferred context classifications, nine statement-eligibility observations, five malformed syntax cases, two empty-section classifications and one modern pattern reinterpretation complete the set.

Roslyn reports duplicate default labels as CS0152 during compilation. Empty final switch sections parse without syntax errors but produce CS8070; the historical grammar requires a nonempty statement list and rejects them. CS1023/CS0201 are selected embedded/statement-expression eligibility observations. Modern Roslyn interprets `case F():` as a later CasePatternSwitchLabelSyntax and reports CSharp1 availability CS8022. The historical constant-expression grammar recognizes the invocation expression; its nonconstant value is excluded semantic work. This is recorded separately, not coerced into tree-shape parity or presented as a valid executable program.

## Verification and remaining gates

Initial expectations had six rejecting cases; three duplicate-default cases were actual production defects and three initializer expectations conflicted with the preserved semantic boundary. Initial policy tracing also caught the generated SwitchLabel choice group before reaching GREEN. Oracle assumptions about empty sections and invocation-shaped cases were corrected explicitly. Failures and scope corrections remain in the execution ledger; no passing-only history is claimed.

Full pre-review verification: **620 normal + 1,675 oracle = 2,295 tests pass**. No grammar module, generated API or Nitrogen core changes were necessary. Binding, IDisposable conversions/read-only resource semantics, case constant evaluation/equality, reachability, label/jump target resolution and executable correctness remain excluded. Finite nesting, recovery and profile observations do not establish complete C# 1 conformance.

Decisions: add selected default-keyword cardinality without evaluating case values; preserve historical case-expression recognition despite modern pattern reinterpretation; preserve the earlier using-initializer semantic exclusion. The costs are a narrower policy than raw grammar for defaults and accepted syntax that still needs semantic validation for resource/constant/jump validity.

## Fresh review

Fresh read-only review of `bec12c9..b612fca` judged the checkpoint ready to merge with no Critical, Important or Minor findings. No additional fix pass or deferred minors resulted.

Review limits remain missing using initializers and other semantic validity; unchanged cast/interface/source-placement/unsafe families; exhaustive recovery and full C#1 closure. Extrapolating these fixtures would overstate conformance or executable validity. The reviewer did not execute tests/hash checks; the complete author run and audit supply evidence for 2,295 tests and 118 original/isolated Nitrogen dependency hashes. The original corpus and sibling dirty status remain unchanged; production runtime manifests contain no Roslyn.
