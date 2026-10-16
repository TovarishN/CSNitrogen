# C# 1 names/types conformance continuation — 2026-10-16

This checkpoint adds production-level evidence for the existing `CSharp.NamesTypes` module, following Task 1 of the approved source grammar plan. It changes no production grammar or parser policy. All profiles remain partial. The [production ledger](csharp1-type-productions.json) maps all ten authored productions/categories/alternatives to positive, negative and typed-structure observations.

## Scope and independent observations

The normative reference remains the pinned [ECMA-334 first edition, December 2001](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf): §10.8 names, §11.1.3 built-in types, §17.5 return contexts and §19.1 array syntax. This scope tests nonpointer source type spellings, their physical structure and restricted `void` contexts; it does not resolve names or type validity.

`TypeConformanceTests` adds 76 direct observations: all fifteen non-void builtins; raw qualified/escaped/contextual names; typed name, element and rank children; malformed/full-consumption cases; distinct `ReturnType` alternatives; comments and authored coordinates; owning source contexts; and safe-composition rejection of pointers. The existing full/unsafe composition tests remain intact. No handwritten replacement parser or sibling Nitrogen change is introduced.

`TypeConformanceParityTests` adds 488 independent Roslyn observations. Twenty-two built-in/named stems combine with no rank, four single ranks, and sixteen ordered double ranks; three additional escaped/comment cases give 465 source shapes. Each compares the actual typed base/name selection, identifier spelling and spans, rank ordering, rank/comma counts and physical spans. Ten malformed fragments, three later-version availability contexts and ten `void`/array-context observations complete the fixture set. CSharp1 is explicit in every oracle parse; generic, alias-qualified and nullable type contexts require CS8022 during compilation. Binding errors for undeclared fixture names are deliberately not a parser gate.

## Classified oracle differences

- Roslyn's standalone type fragment accepts `A[1]` without diagnostics. The enclosing declaration yields CS0270. The historical declaration grammar permits comma-only rank notation; CSNitrogen rejects a dimension expression in this context.
- Roslyn's syntax tree accepts `void` fields and properties, then compilation reports CS0670 and CS0547. CSNitrogen's existing grammar rejects these non-return type contexts. Separate tests assert the diagnostic phase and code, rather than relabeling ordinary binding failures as syntax errors.

The initial characterization run had three failing oracle assumptions (array dimensions, void field, void property); the corrected classification tests all passed without changing production behavior. An earlier test compile error was corrected before recording runtime evidence. This is characterization/conformance evidence, not a manufactured RED→GREEN production fix.

## Verification and remaining gates

Full pre-review verification: **491 normal + 786 oracle = 1,277 tests pass**, including all prior historical Unicode and source regressions. The new names/types fixtures contribute 564 observations. Finite rank combinations are representative coverage, not exhaustive rank length/depth or all consuming contexts. The baseline inventory remains `implemented`, and no C# 1 completion gate is promoted by a passing sample count. Pointer productions belong to Unsafe; casts/array creation/operator trees and broader declaration/statement context closure remain subsequent work.

Decisions: record compiler-deferred void errors explicitly (risk if misclassified: overstated phase parity); distinguish standalone array-fragment acceptance from legal declaration ranks (risk if conflated: invalid type acceptance).
