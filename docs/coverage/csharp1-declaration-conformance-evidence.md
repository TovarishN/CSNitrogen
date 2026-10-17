# C# 1 declaration/member continuation — 2026-10-17

PR #7 was merged at its reviewed head after a fresh 2,295-test run. This continuation starts from merged main `1f4b67e` and follows the approved source grammar plan Task 5 with Task 4 parameter interactions. All profiles remain partial.

## Selected typed observations

`DeclarationConformanceParityTests` adds **71 oracle observations**: 47 shape cases, 20 rejecting cases and four explicit semantic-boundary classifications. Thirty shape cases cross six parameter signatures with class methods, constructors, interface methods, delegates and explicit interface methods. Seventeen selected sources exercise the remaining member/type contexts, empty and nonempty enums, comment gaps and attribute targets/trailing commas. These fixtures establish syntax structure, not valid executable programs: unresolved symbols, invalid modifier combinations and ref/out argument aliasing may still require semantic validation.

The comparisons locate a unique generated concrete kind within each Roslyn declaration's source extent. Generated owning fields supply parameter lists, type/base names, field declarators/initializers, constructor initializers, bodies and accessor lists. Ordered member/accessor spans, attributes, modifier tokens, enum values/separators/trailing comma and explicit interface name extents are checked. Class/struct/interface headers, delegates, event fields/accessors, constants, methods, constructors/destructors, properties, indexers, operators/conversions and nested types receive selected observations. `ParameterShapeAssert` inspects fixed/ref/out/params alternatives, attributes and their expression arguments, array element/rank fields, comma positions, names and physical spans. Expression and statement children use the existing recursive comparers; the statement declaration helper is exposed internally for field/event reuse without changing behavior.

This is a bounded field comparison, not an exhaustive recursive declaration-tree equivalence proof. Concrete declarations are found by kind and extent; every enclosing alias/choice discriminant is not independently compared. Namespace/root projection and preprocessing interactions are the next continuation, not closed by this work.

## Acceptance boundaries

Malformed declarations/parameter lists, nonterminal params, empty indexers, incomplete enum values and constructor arguments are rejected. Existing accessor cardinality/event-pair policy remains checked. Selected class-only interface bodies/fields/constructors and later default/in/extension parameters, generic methods, accessor modifiers, partial declarations and expression bodies are rejected. Roslyn compilation errors are required for each negative source; the tests deliberately do not claim that every rejection is a Roslyn parse diagnostic or the same diagnostic category.

The established semantic boundary is preserved explicitly: wrong constructor names (`CS1520`), operator arity (`CS1534`), enum underlying-type validity (`CS1008`) and modifier compatibility (`CS0107`) remain compiler observations while parser acceptance stays true. A diagnostic's presence in Roslyn's compilation observations does not imply that it arose only after parsing. No new syntax policy is inferred from these diagnostics. Existing static-constructor/accessor/struct policies remain unchanged.

## Corrections and verification

No product defect was reproduced, so this continuation changes tests and coverage evidence only. Initial helper failures wrongly assumed interface events used class event fields, destructors owned a generated parameter-list node, and an optional single trailing enum comma owned an extra group. The helper was corrected to reflect the actual generated API. A later helper build caught ambiguous Optional imports and token-versus-node separator types. These failed runs are retained locally; they are not presented as product RED/GREEN evidence.

Full pre-review solution test/build: **620 normal + 1,746 oracle = 2,366 pass**, zero failed/skipped. Both original and isolated Nitrogen copies match all 118 pinned source hashes; original sibling dirty state remains ` M docs/editor-support.md`. Production dependency manifests contain no Microsoft.CodeAnalysis. Production source/grammar, original context corpus and pinned external corpus are unchanged.

Decision: retain this step as characterization instead of inventing a production correction; the cost is finite evidence rather than exhaustive declaration closure. Binding, constructor/member resolution, modifier compatibility, operator arity/types, attribute target applicability, params/indexer type legality and executable validity remain excluded. C# 1 production/context closure, unsafe membership revalidation, later-version feature modules and arbitrary raw delimiters remain outstanding.
