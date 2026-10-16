# C# 1 cast and expression-tree continuation — 2026-10-17

This tests/evidence checkpoint follows Task 2 of the approved source grammar plan. It changes no production grammar or parser policy. All profiles remain partial. The [observation ledger](csharp1-cast-productions.json) records selected cast productions and operator interactions; this is a subset of expression coverage, not a full expression production ledger.

## Scope and comparison

The pinned [ECMA-334 first edition](https://ecma-international.org/wp-content/uploads/ECMA-334_1st_edition_december_2001.pdf), §14.6.6, disambiguates a parenthesized type using token grammar and its following token, without resolving identifiers. The existing NamedCast/DefiniteCast/PointerCastHead selection is tested directly. `is`/`as` following a parenthesized name stay outside a cast; built-in casts can take unary operands that would instead be binary/postfix syntax after a parenthesized name.

`CastConformanceParityTests` adds 587 independent Roslyn observations through the owning public parse API. Cast shapes include 22 type spellings (all fifteen non-void builtins, names, arrays and pointer forms) crossed with nine operands, plus 42 boundary/comment/literal cases: 240 cast/disambiguation shapes. Every ordered pair of eighteen binary operators gives 324 precedence/associativity cases with a leading cast. Six assignment/conditional interactions and seventeen malformed sources complete the set.

The recursive comparer checks every visited expression kind and physical span, selected cast-head alternative and type span, nested unary/postfix operands, binary left/right grouping, assignment target/value/operator, both conditional arms, names, member targets, calls/arguments and indices. Unknown comparer cases fail explicitly. Type or conversion legality, assignability and symbol resolution are excluded; syntax acceptance is not executable validity.

`CastConformanceTests` adds 59 direct observations: eight typed cast-head selections, nine name/parenthesis disambiguations, seventeen malformed/full-consumption cases, eighteen missing binary operands and seven safe/full module comparisons. Pointer casts and pointer unary operators require Unsafe. `(T)*p` and `(T)&x` remain legal safe binary expressions with parenthesized names, so keyword/punctuation text alone does not determine unsafe membership.

## Characterization outcomes

Initial execution passed 431 oracle and 37 normal fixtures; an earlier missing test import was corrected before runtime evidence. Expanded coverage caught one mistaken test expectation: `(T[1])x` does not produce a cast-type dimension diagnostic. Roslyn parses the parenthesized element access then reports stray `x` as a syntax error. The case was retained as ordinary malformed-source parity. All 646 final new observations pass with no production changes; no RED→GREEN product fix is claimed.

Full pre-review verification: **550 normal + 1,373 oracle = 1,923 tests pass**. The fixture report covers finite depth and selected interactions. Array creation/initializers, remaining primary forms, general argument contexts, full unsafe production internals and broader declaration/statement conformance remain pending. The baseline inventory stays implemented and no overall profile completion gate is promoted.

Decision: compare syntax and spans, excluding semantic conversion/assignability errors consistently with the parser scope; treating these accepted fixtures as executable programs would overstate the evidence.

## Fresh review

Fresh read-only review of `5e118e8..d6f126d` judged the checkpoint ready to merge with no Critical or Important findings. One deferred minor: literal expressions are compared by kind, expression span and source text; the generated `Value` child projection/span/text is not separately asserted. This does not block the finite cast/grouping evidence, but literal-child closure remains pending.

Review boundaries: statement/interface/declaration/source restrictions (Tasks 3–6); array creation/initializers, general argument modifiers and other primary forms; full pointer/fixed/stackalloc internals and unsafe contexts; binding/conversions/assignability/execution. Extrapolating these fixtures would overstate conformance or executable validity. The reviewer inspected source under a no-run instruction; the two complete author runs supply execution evidence. Prior PRs #3/#4 are now merged to main; this checkpoint targets main directly.
