# CSharp6 bounded seam proof

Test-only evidence for the sixth-edition additions (decision: [2026-11-04-csharp6-source-contract-decision.md](../superpowers/specs/2026-11-04-csharp6-source-contract-decision.md), sources: [csharp6-reference-policy.json](csharp6-reference-policy.json)). No production grammar, factory, profile or admission changes. CSharp6 remains partial.

## Composition

`tests/CSNitrogen.CSharp6.Tests` composes the real 32-module CSharp5 factory under the separate generated namespace `CSNitrogen.CSharp6Probes`. Four grammars are test copies with empty seams only. Every other input is the unchanged production file:

- `Statements.ngr`: `CatchClause.Filter:CatchFilter?`; the general-clause lookahead becomes `&("catch" !"(")` so `catch when(...)` is general.
- `Declarations.ngr`: `MethodBody` gains `/ MethodBodyExtension`; `PropertyDeclaration` gains `Initializer:PropertyInitializerSeam?`; both `MemberDeclaration` body choices gain `/ MemberDeclarationExtension`.
- `CompilationUnits.ngr`: `UsingDirective` gains `Static:UsingKind?` after `using`.
- `ObjectInitializers.ngr`: `Initializer` gains `/ InitializerExtension`.

Existing fields keep their names and generated types (for example `MemberInitializerNode.Name` stays a `Token`). Eight independent modules fill the seams: InterpolatedStrings, NullConditional, NameofExpressions, ExceptionFilters, UsingStatic, PropertyInitializers, ExpressionBodies, IndexInitializers. `CSharp6NoneProbe` is the seamed baseline, `CSharp6AllProbe` adds all eight, and `CSharp6Without<Module>Probe` omits one module at a time.

## Mechanisms

- **Interpolated strings, no core change.** Text lives inside the delimiter tokens `$"…{`, `}…{`, `}…"` (and `$@"` forms), so trivia skipping never reaches it. Only hole contents are ordinary syntax. Holes are a separated list `(Interpolation; Mid)+`. An `Interpolation` has a typed `Expression`, an optional `InterpolationWidth` and an optional `InterpolationFormat`. The `CSharp.InterpolatedStrings.Verbatim` context flag selects regular or verbatim format text for the innermost string. Strings without holes are `InterpolatedRegularText`/`InterpolatedVerbatimText`. Nesting in either direction, `{{`/`}}` escapes, widths, formats, trivia inside holes and literal comment text in strings are witnessed with exact UTF16 spans, with and without a supplementary-character prefix. Hole starts match Roslyn's `InterpolationSyntax.Expression`.
- **Null-conditional.** `?` `.` identifier and `?` `[` arguments `]` are postfix alternatives at precedence 180. Dependent member, element and invocation accesses are ordinary postfix nodes over them. `a?.5:1` keeps the conditional reading.
- **nameof.** `NameofExpression` (`nameof` `(` named entity `)`) is a prefix alternative. Nitrogen chooses the longest prefix alternative before postfix operators, so it can never tie with `InvocationExpression` over the `nameof` name. Where the named-entity reading parses, a second prefix alternative `NameofInvocation` (Target identifier, Args) ties with it, and Nitrogen yields one `Ambiguous` node holding both. Invocation-only spellings (`nameof(1+2)`, `nameof(x,y)`, `nameof()`) keep `InvocationExpression`, and entity-only spellings (`nameof(int)`, `nameof(base)`) give a lone `NameofExpression`. Nitrogen reports every Ambiguous node as an error diagnostic. The probe accepts only Ambiguous errors whose children are exactly NameofExpression and NameofInvocation; Task 2 moves this filter into the parse wrapper.
- **Exception filters, using static, property initializers, expression bodies, index initializers** fill the seams above. Index initializers use a separate `IndexedObjectInitializer` that requires at least one `[…] =` member, so lists without one keep the baseline `ObjectInitializer`. This follows the CSharp4 named-index precedent. Expression-bodied properties and indexers are separate member alternatives, so `PropertyDeclaration.Accessors` is unchanged.

## Source-local obligations for Task 2

These are accepted by the grammar here and must become source-local checks:
- An unparenthesized conditional in a hole, for example `$"{a?b:c}"`. The standard's unnested colon starts a format and leaves `a?b` incomplete; Roslyn also rejects it.
- Property initializers on non-automatic, abstract, extern or interface properties.
- Expression bodies on constructors, finalizers and accessors, which reach `MethodBody` through the shared seam.
- The anonymous-object projection check must admit `a?.b` (null_conditional_projection_initializer).

## Roslyn observations (Microsoft.CodeAnalysis.CSharp 5.0.0, LanguageVersion.CSharp6)

24 spellings are accepted by both, 6 are rejected by both, and 5 disagree. In each disagreement the standard governs:

| Spelling | Standard | Roslyn | Reason |
| --- | --- | --- | --- |
| `$"{x:N⏎}"` | accept | reject | hole contents are tokens, so the new-line is whitespace |
| `nameof(int)` | accept | reject | `named_entity_target` includes `predefined_type` |
| `@$"{x}"` | reject | accept | only `$@` exists in the sixth edition; Roslyn defers the version check |
| `using static A=B;` | reject | accept | `using_static_directive` has no alias; Roslyn reports it at binding |
| `$"{x:}"` | reject | accept | a format needs at least one element |

`$"{⏎x⏎}"` is accepted by both.

## Verification

`dotnet test CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` passes 11,570 tests: 7,502 main, 3,884 oracle, 55 CSharp5 proof and 129 CSharp6 proof, with zero failures or skips. Production sources under `src/` are unchanged, so CSharp1–5 admission pins are untouched. These are finite witnesses, not exhaustive inputs or arbitrary-depth guarantees.
