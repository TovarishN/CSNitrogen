# C# 1 Source Grammar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans for the preserved native execution method. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand the verified lexical/preprocessing foundation into a C# 1 syntax baseline for regular source files, retaining typed trees and explicit partial coverage until the production ledger is verified.

**Architecture:** Separate `.ngr` modules define names/types, expression categories, statements, attributes, declarations, compilation units, and unsafe extensions. Existing generated composition factories select their membership; the public parse API retains ownership and preprocessing metadata. Syntax conformance is compared with Roslyn syntax observations; binding/type/constant-expression validity remains outside scope.

**Tech Stack:** Existing .NET 10 solution, pinned Nitrogen snapshot, xUnit, Roslyn 5.0.0 oracle.

**Spec:** [Approved design](../specs/2026-10-16-csharp-versioned-grammars-design.md).

## Global Constraints

- Roslyn is a test oracle, not the production parser or a replacement for Nitrogen grammars.
- No sibling repository changes are included.
- Support regular C# source files first; script mode is excluded.
- Preserve original text and UTF-16 source coordinates.
- The initial product does not implement C# name binding, type inference, overload resolution, semantic validity, lowering, code generation, or execution.
- Repository initialization and commits are deferred until requested.
- Never use an unpinned `latest` or `preview` policy for these profiles.

C# 2–14 syntax is still future work. Older syntax that is also a valid spelling in a newer language must not be rejected merely by keyword text: `var`, `async`, and `nameof` may be ordinary identifiers in C# 1. Semantic-only feature differences are not parser failures.

## Review Focus

1. Casts versus parentheses must follow C# syntactic disambiguation, not a first-reading assumption: Task 2.
2. An arbitrary expression must not become a statement expression merely because the expression module accepts it: Task 3.
3. Interface members must not inherit class-only bodies/default members accidentally: Task 5.
4. Using directives, global attributes, namespace declarations, and type members must respect their source contexts: Tasks 4/6.
5. Unsafe syntax must be selectable by module membership and leave no pointer/fixed/stackalloc leakage when omitted: Task 7.

## Contracts and generated-view migration

Create `NamesTypes.ngr`, `Expressions.ngr`, `Statements.ngr`, `Attributes.ngr`, `Declarations.ngr`, `CompilationUnits.ngr`, and `Unsafe.ngr` under `src/CSNitrogen.Syntax/Grammars`.

Required extensible categories are `TypeReference`, `Expression`, `Statement`, `EmbeddedStatement`, `ClassMember`, `TypeDeclaration`, and `NamespaceMember`. Dedicated categories constrain parameter lists, accessors, interface members, assignment targets where syntactically specified, and statement expressions. Rules that the C# specification classifies semantically must not be replaced by guessed syntax restrictions.

The existing empty-class `CompilationUnitNode.Types` prototype cannot represent namespaces and nonclass types. Migrate the generated root to `CompilationUnitNode.Members` with typed member alternatives; expose `ParseDocument.Classes` as a read-only projection for the current top-level-class use case, and update examples/tests together. Document this prototype API change explicitly. Factories (`CSharp1Composition.CreateBuilder` and `.Start`), parse-result ownership, physical spans, and preprocessing metadata remain the integration contract.

Every task below follows RED → minimal `.ngr` implementation → focused GREEN → existing regression suite. Run focused tests with `dotnet test tests/CSNitrogen.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --filter FullyQualifiedName~<TaskTestClass>`. Confirm the intended missing production/shape failure before implementing. Run corresponding oracle tests with the same build flags under `tests/CSNitrogen.Oracle.Tests`.

## Task 1: Names and type syntax

**Create:** `NamesTypes.ngr`; `Syntax/NamesTypesTests.cs`; `NamesTypesParityTests.cs`.

- [ ] Review every applicable C# 1 name/type production against the specification and ledger before mapping it. Use non-generic qualified names and C# 1 built-in types; keep `void` restricted to contexts where syntax permits it.
- [ ] Write fixtures for `A`, `A.B`, `int`, `object`, `int[]`, `A[,]`, and jagged/multidimensional combinations. Reject missing name segments, incomplete rank specifiers, and generics. Test contextual identifier spellings as ordinary names.
- [ ] Implement named and nonpointer array type categories with typed rank/name children. Keep pointer types in Task 7. A starting non-generic name production is:

```text
syntax QualifiedName = Parts:(CSharp.Lexical.Identifier; ".")+;
```

- [ ] Compare full consumption, selected types, rank count, identifier spelling, and spans against Roslyn. Do not infer what a name denotes. Record semantic-only differences separately.

## Task 2: Expressions and syntactic disambiguation

**Create:** `Expressions.ngr`; `Syntax/ExpressionTests.cs`; `ExpressionParityTests.cs`.

- [ ] Build precedence/associativity fixtures before implementing each operator family: `a+b*c`, `a-b-c`, `a=b=c`, `a?b:c?d:e`, shifts versus relational comparison, prefix/postfix increments, casts, and parenthesized expressions. Preserve a tree-shape expectation for every case.
- [ ] Add C# 1 primary forms: literals, names, `this`, base/member access, calls, indexing, object/array creation and array initializers, `typeof`, checked/unchecked expressions. Add unary, multiplicative, additive, shifts, relational/type-testing, equality, bitwise/logical, conditional, and assignment families using declared precedence.
- [ ] Keep argument modifiers `ref`/`out`; exclude named/optional arguments, lambdas, anonymous methods, query expressions, nullable operators, and later object/collection initializer syntax. Do not forbid an ordinary method named `nameof`.
- [ ] Implement cast lookahead from the specification's C# 1 disambiguation rule. Test `(T)x`, `(x)+y`, `(int)-x`, and nested parentheses against Roslyn; record cases requiring meaning rather than guessing a bound type.
- [ ] Assert source spans and operator-tree structure, including malformed tails and missing operands. Do not claim that syntactic assignment targets are writable or that expressions have C# types.

## Task 3: Statements, blocks, and exception syntax

**Create:** `Statements.ngr`; `Syntax/StatementTests.cs`; `StatementParityTests.cs`.

- [ ] Write statement-context fixtures for blocks, empty statements, local declarations/constants, permitted statement expressions, `if/else`, `switch`, loops, jumps, try/catch/finally, checked/unchecked blocks, lock, and using statements. Reject `a+b;` while accepting `F();`, assignments, increments/decrements, and object creation.
- [ ] Implement `Statement` versus `EmbeddedStatement` distinctions. A local declaration must not become an unbraced embedded statement where C# disallows it. Keep labels/goto forms and dangling-else association explicit.
- [ ] Test `for` empty clauses, multiple initializers/iterators, `foreach` declarations, switch label grouping, catch types/identifiers, bare catch, and `try` without an allowed handler. Do not add catch filters or using declarations from later versions.
- [ ] Compare syntax shapes/diagnostics with Roslyn. Case-label constancy and throw/catch type validity are semantic checks and remain outside parser acceptance.

## Task 4: Attributes and parameter contexts

**Create:** `Attributes.ngr`; shared declaration helper rules; `Syntax/AttributeParameterTests.cs`; oracle counterparts.

- [ ] Write fixtures for attribute lists, qualified attribute names, positional/named attribute arguments, global `assembly`/`module` targets, and target specifiers. Named attribute arguments are C# 1 even though named method arguments are later syntax.
- [ ] Implement parameter lists with attributes, C# 1 `ref`/`out`, and terminal `params` arrays; test each declaration context's allowed parameter categories. Exclude `in`, default values, and extension-method `this` parameters.
- [ ] Include malformed bracket/argument lists, invalid placement of global attributes, and later-feature lookalikes. Attribute binding, constructor existence, and target applicability remain semantic and are not guessed.
- [ ] Run focused tests and expression/statement regressions before expanding declarations.

## Task 5: Type declarations and members

**Create:** `Declarations.ngr`; `Syntax/DeclarationTests.cs`; `DeclarationParityTests.cs`.

- [ ] Inventory and map C# 1 classes, structs, interfaces, enums, and delegates. Add class/struct fields/constants, methods, constructors, destructors, properties, events, indexers, operators, conversions, nested types, explicit interface member names, and constructor initializers.
- [ ] Write typed-tree fixtures for a class containing one member of each kind, a struct implementing an interface, enum values with optional initializer/trailing comma, and a delegate with attributes and parameter modifiers.
- [ ] Separate interface member syntax from class/struct members: C# 1 interface signatures/accessors are not default implementations. Keep accessor-level accessibility, generics, partial declarations, expression bodies, records, primary constructors, and default interface bodies out of this profile.
- [ ] Add malformed signatures and member/context boundaries. Method/type-name resolution, constructor-name matching, overload legality, modifier compatibility, and operator type/arity semantics are classified using the oracle before adding any syntax-only rejection.
- [ ] Run every earlier syntax suite and oracle observations; preserve all discrepancies in the coverage report.

## Task 6: Compilation units, namespaces, and public typed root

**Create:** `CompilationUnits.ngr`; `Syntax/CompilationUnitTests.cs`; `CompilationUnitParityTests.cs`. **Modify:** `Core.ngr`, configured memberships, `ParseDocument`, README, existing generated-view/pipeline tests.

- [ ] Write whole-file fixtures combining using directives/aliases, global attributes, namespace bodies, top-level types, nested namespaces, and empty files. Test invalid using placement, incomplete qualified names, and later file-scoped/global/static-using forms.
- [ ] Build one complete public parse path through preprocessing and every C# 1 module. Migrate root members and the class convenience projection as specified above, with tests proving authored spans survive directives and inactive regions.
- [ ] Compare counts, member kinds, representative nested fields, full consumption, and useful malformed-input diagnostics against Roslyn. Successful examples must exercise actual typed generated views rather than string searches.
- [ ] Verify recovered files remain rejected and borrowed node access still expires when the owning result is disposed. Keep every profile partial until its ledger obligations are satisfied.

## Task 7: Unsafe syntax as an explicit extension

**Create:** `Unsafe.ngr`; `Syntax/UnsafeCompositionTests.cs`; oracle counterparts; one configured safe subset for tests.

- [ ] Write paired positive/negative membership fixtures for pointer types, pointer dereference/address-of, `->`, unsafe blocks/modifiers, fixed statements, `sizeof`, and C# 1 `stackalloc` forms. Check the historical `sizeof` distinction between built-in and user-defined types using syntax versus compilation observations.
- [ ] Implement only unsafe-dependent productions in the extension module. Keep syntactic built-in `sizeof` available where required without conflating semantic unsafe-context checks. No pointer/body representation outside the shared type/expression categories is introduced.
- [ ] Exclude later fixed-size buffers and stackalloc initializers. Test class/member signatures containing pointer types and combined expression forms.
- [ ] Demonstrate omitted unsafe syntax with actual module lists and paired source acceptance. This is a focused composition proof, not the completed later customization milestone.

## Task 8: C# 1 acceptance, corpus, and continuation

**Modify:** `docs/coverage/features.json`, current evidence, README, coverage identity registrations, and profile metadata.

- [ ] Map every applicable C# 1 syntax production and declared policy to implemented modules, references, positive/negative fixtures, and status. Remaining blocked/unimplemented entries keep CSharp1 partial; counts of passing samples alone cannot mark it complete.
- [ ] Add a small pinned external source corpus with source revision/path/license provenance. Keep corpus results distinct from targeted grammar conformance and compiler semantic diagnostics.
- [ ] Run the full solution build and both suites, including bounded malformed/deep tests and the isolated dependency audit. Verify production dependencies contain no Roslyn compiler and sibling Nitrogen source/state remain unchanged.
- [ ] Update coverage identity inputs for every new grammar and policy. Retain exact explicit profile versions, current preprocessing regressions, and any oracle discrepancies with their authored inputs.
- [ ] Once the C# 1 ledger gate passes, prepare the C# 2–6 feature-module plan. Keep the unresolved arbitrary raw-delimiter gate visible for later versions.

## Handoff

Approved and executed as the partial source-grammar milestone. The C# 1 completion gate remains open; evidence and remaining production closure are in ../../coverage/csharp1-source-grammar-evidence.md. Native execution remains selected. It includes the necessary typed-root prototype migration explicitly. Approval does not authorize a Nitrogen core change or C# semantic compiler implementation.
