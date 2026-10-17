# C# 2 additive grammar design

Status: approved by the user on 2026-10-17. Builds on merged PR15 (`81da89d`) and the approved canonical coverage contract. This is the first bounded milestone in the C# 2–6 release group, not an implementation or CSharp2 completeness claim.

## Intent and boundaries

Add real Nitrogen-authored C# 2 syntax with typed views, original-source spans, explicit module membership and version checks. Keep the reviewed CSharp1 contract intact. Reuse the owning parse API, preprocessing, canonical capability mappings and test-only Roslyn oracle. No sibling Nitrogen edits, handwritten C# parser, new semantic compiler, arbitrary runtime factory or general plugin system.

The release-history ledger is a candidate list, not the complete normative delta. Pin ECMA-334 fourth edition (June 2006) by URL and SHA before implementing, extract its grammar/prose obligations and compare against the pinned first edition. Record intervening historical differences explicitly. The historical C# 2 reference contract, including Unicode/category and directive changes, must be settled from the pinned source before any later completeness promotion; do not inherit the current .NET policy and call it historical parity. Roslyn LanguageVersion.CSharp2 is a secondary executable reference; classify modern/historical discrepancies and semantic diagnostics separately.

Sources checked while preparing this design:

- https://ecma-international.org/wp-content/uploads/ECMA-334_4th_edition_june_2006.pdf
- https://github.com/dotnet/csharplang/blob/main/Language-Version-History.md
- Existing CSharp1 source crosswalk, coverage gate and canonical capability decision in this repository.

## Capability contract

Retain the nineteen baseline capabilities and their coverage identities. Introduce the following additive canonical bundles; exact IDs below are proposed stable identities. Report dependencies explicitly and reject missing ones. Inventory mappings can cover syntax portions only, with separate documented semantic exclusions. Names are not promises of independently removable subfeatures inside a bundle.

| Canonical bundle | Surface and dependency intent |
| --- | --- |
| `csharp2.generics` | Constructed and qualified generic types, type parameters and constraints on class/struct/interface/delegate/method declarations, explicit generic method names and invocation disambiguation, unbound generic names in typeof. Requires baseline names/types, expressions and members. |
| `csharp2.nullable-values` | Nullable type suffix and null-coalescing expression, including interaction with arrays, pointers, casts and conditional expressions. Requires baseline names/types and expressions; does not require generic source syntax merely because Nullable<T> exists semantically. |
| `csharp2.anonymous-methods` | delegate expressions with omitted or explicit parameter lists and block bodies. Requires baseline expressions, attributes/parameters and statements. No lambda syntax. |
| `csharp2.iterators` | Contextual yield return and yield break statements. Requires baseline statements and expressions. Iterator lowering and return-type validity excluded. |
| `csharp2.declaration-forms` | Partial class/struct/interface headers, static class syntax and accessor accessibility. Requires baseline members and attributes. Partial methods and partial member forms from later versions excluded. |
| `csharp2.alias-qualification` | Extern aliases, alias-qualified names including global::, and their allowed qualified type/name and using positions. Requires baseline names/types and compilation units. Existing using-name aliases remain baseline syntax. |
| `csharp2.default-values` | Typed default(T) expression. Requires baseline expressions and names/types. Untyped default literal is excluded. |

The audit must identify additional normative deltas absent from release-history headings (for example fixed buffers or pragma directives). Implement syntax deltas within this milestone in a separately named additive bundle or explicit policy mapping, with the same acceptance gates. Do not silently drop them or declare the seven rows exhaustive. A materially new mechanism, public API break or Nitrogen core change requires a revised design; routine source-derived syntax additions within this contract are builder decisions.

Review existing covariance/contravariance, delegate inference and method-group-conversion inventory classifications. Their binding/conversion behavior is excluded; shared name/expression recognition is not proof of those semantics. Do not confuse C# 2 delegate compatibility with later generic variance declarations using in/out type parameters.

## Grammar architecture and typed compatibility

Production modules live beside the existing grammars and are selected by generated composition configuration. New bundles extend the existing extensible TypeBase/TypeReference, CastHead, Expression, Statement, Modifier, TypeDeclaration and member surfaces where suitable. Split modules by necessary extension boundaries rather than copying a complete C# grammar for each version.

Closed rules currently block generic qualified names, type/method headers, accessor headers and extern-alias placement. Introduce narrowly scoped shared extension seams before feature alternatives. Preserve existing non-generic typed field names and their meaning, CSharp1 tree observations and owning-lifetime rules. Add typed alternatives/optional fields for the new syntax; never store generic parameter lists, constraints or anonymous bodies as opaque text. Preserve the existing QualifiedName.Parts projection for ordinary names. New generic/alias-qualified forms may have separate typed alternatives instead of changing ordinary identifier parts into lossy strings.

Before choosing exact seam syntax, prove it against the pinned generator in a test-only composition: baseline with the seam alone must produce the old tree, extension-loaded composition must select the new form without shadowing, and omission must reject the new form. If an optional extension needs an empty fallback, prove ordering, full consumption and malformed cases explicitly. Do not assume Nitrogen supports rule replacement or unpopulated extension points. If the existing mechanisms cannot express the required seams while preserving the stated compatibility, report the concrete limitation before changing core or generated public contracts.

Generic names need typed type-argument lists and individual closing delimiter spans. Generic invocation selection follows the pinned syntactic disambiguation rules rather than first-success parsing. Preserve relational/shift expressions and distinguish nested generic closers from >> and >>= operators. Cast classification must recognize nullable/generic/alias-qualified forms through typed categories rather than string heuristics. Update existing syntax policies by explicit alternative handling; new node kinds must not silently bypass lvalue, postfix-target, cast or declaration-context checks.

## Profiles and version behavior

CSharp1 keeps its original factory membership and historical Unicode policy; shared seam additions must preserve its accepted syntax and coverage review. If shared source changes, refresh coverage identity and rerun source gates rather than treating the old review as automatic proof.

CSharp2 and later named factories include implemented additive modules and matching policies. Each capability is advertised only once its source, typed, boundary and omission checks pass. During staging, CSharp2 remains partial with honest missing inventory; CSharp3–14 and custom resolutions stay partial. Keep custom resolution as metadata, and use generated test compositions for omission witnesses. Runtime arbitrary custom execution remains deferred.

Do not reuse CSharp1's hardcoded Complete exception for CSharp2. A separate CSharp2 source/coverage/factory/policy contract is required before promotion. CSharp1 regressions are blockers throughout. A C# 2 feature already parseable as baseline syntax (static modifiers or method-group names) needs a scoped availability/context diagnostic only where the syntax feature can be identified; semantic conversions remain excluded.

## Representative acceptance observations

Each source runs in its owning fragment and a real compilation unit. Assert typed children, ordered lists, delimiters, original UTF-16 spans and policy diagnostic anchors.

| Witness | Required observation |
| --- | --- |
| class Box<T> where T : class, new() { T value; } | Typed parameter/constraint order and constructor constraint; reject in CSharp1 and without generics. |
| class C { object M() { return F<G<int>>(x); } } | Generic method type arguments and nested closers; pair with a < b and a >> b expressions. Add follower-token disambiguation cases from the pinned prose. |
| class C { int?[] values; int M(int? x) { return x ?? 0; } } | Nullable element before array rank, right-associative coalescing and spans; paired omission tests. |
| class C { object d = delegate(int x) { return x; }; } | Typed anonymous parameter and block; also delegate { } and delegate() { }; conversion validity excluded. |
| class C { object M() { yield return 1; yield break; } } | Two typed yield alternatives; semantic return-type validity excluded. yield used as an ordinary identifier remains accepted in appropriate contexts. |
| partial class C { int P { get { return 1; } private set { } } } | Typed contextual partial header and accessor access modifier; malformed accessor combinations classified against syntax/prose exclusions. |
| extern alias A; class C { A::N.T x; global::N.T y; } | Extern directive before using/member regions and typed alias qualification; alias binding excluded. |
| class C { int M() { return default(int); } } | Typed default operand; reject default literal and other later-only syntax under CSharp2. |

Malformed fixtures include empty/missing generic arguments, bad constraints/order, missing generic closers, ambiguous relational followers, missing coalescing operand, incomplete delegate parameter/body, incomplete yield, misplaced alias directives, invalid alias separators and later-only generic variance/lambda/query syntax. Assert deterministic full consumption/rejection and authored diagnostics, with bounded nesting/deep-malformed cases. Contextual words partial/yield/where remain usable as identifiers according to the pinned lexical/context rules.

## Delivery and acceptance gates

1. Pin the historical reference and audit normative deltas, semantic exclusions, historical lexical/directive policy and source production owners. Keep new entries pending until reviewed; no metadata-only completeness promotion.
2. Prove shared seams and the highest-risk generic/relational/shift/cast ambiguities. Run existing CSharp1 typed/source/policy gates. If a required mechanism fails, retain the failing witness and resolve the design before broad implementation.
3. Implement generic types/declarations/methods as the first runnable vertical slice, through public CSharp2 parsing and typed views. Add explicit capability dependencies and inventory mappings; preserve CSharp1 rejection and generic-omission tests.
4. Add remaining audited C# 2 syntax bundles in bounded increments. Each increment includes positive/malformed/context/source-span, preceding-version and omission tests, explicit oracle discrepancies and unchanged baseline gates.
5. Audit the whole C# 2 source contract, actual factory and policies; extend completeness only after a separate reviewed CSharp2 gate. Pin representative external C# 2 corpus provenance and preserve unsupported cases. Finite corpus success is regression evidence, not normative proof.

Use the existing .NET10 and dependency snapshot. Production remains free of Roslyn and Nitrogen.Grammar runtime dependencies. Run focused tests per increment and full solution gates before publication. Record results and source/dependency preservation. Builder may choose helper/module filenames, internal decomposition and test organization within these contracts; material changes to syntax scope, typed compatibility or execution model return for design review.
