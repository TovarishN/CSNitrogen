# CSharp3 additive grammar continuation

Status: proposed bounded continuation of the approved C#1–14 architecture. Feasibility probes precede production edits. Base: merged PR17, 7d542a4. No new Nitrogen core change or breaking generated API is authorized by this document.

## Historical source and scope

Primary: Microsoft C# Language Specification Version3.0, official download linked from Microsoft Learn compiler language options. File CSharp Language Specification.doc, SHA256 ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1 (2,384,896 bytes). The title identifies Version3.0; Word metadata is historical provenance, not a coverage claim. Download URL https://download.microsoft.com/download/3/8/8/388e7205-bc10-4226-b2a8-75351c669b09/CSharp%20Language%20Specification.doc . ECMA editions do not correspond one-to-one with C# versions; the fifth edition covers a later language and is not the C#3 primary.

Generated syntax and source policies remain the product. Expression-tree construction, type inference, overloads, anonymous-type synthesis, extension lookup, query translation/LINQ binding, partial-method erasure and runtime execution remain semantic exclusions. Retain CSharp1/CSharp2 completeness and original UTF16 spans; CSharp3+ remain partial until a separate whole-source contract is reviewed.

## Candidate capabilities

| Capability | Ownership and boundary |
| --- | --- |
| csharp3.lambdas | Add typed signature/body alternatives to Expressions. Explicit/ref/out, implicit single/list/empty parameters; expression/block bodies, assignment precedence/right associativity. No async, attributes, default parameters, static lambdas or return-type syntax from later versions. |
| csharp3.object-initializers | New longer object-creation alternative, typed member/value/nested initializer lists; keep existing ObjectCreationExpression fields unchanged. |
| csharp3.collection-initializers | Typed element lists, scalar non-assignment elements and nested nonempty expression lists; require object-creation owner, preserve commas/braces. Determine module split/dependency from probes; do not fabricate empty object-vs-collection distinction. |
| csharp3.anonymous-types | Typed explicit/inferred member declarators. Inferred simple/member names need source-category policy; anonymous type identity/synthesis excluded. |
| csharp3.implicit-arrays | Typed ranks plus array initializer and original delimiters; element type inference excluded. This syntax is missing from the release-heading inventory and must be audited explicitly. |
| csharp3.queries | Typed from/let/where/join/join-into/orderby/select/group/continuation structure. No dependency on lambda syntax or LINQ binding solely because translation uses them. Contextual-name/ambiguity source review required. |
| csharp3.extension-receivers | Prove an optional empty receiver seam in FixedParameter preserving Mode/Type/Name and absent baseline observations. Add this receiver syntax and source-local context checks; no extension lookup. |
| csharp3.partial-methods | Existing partial modifier/member spelling plus scoped context policy; void, modifier ordering and out/explicit-interface syntax restrictions. No erasure/paired declaration matching. |
| csharp3.auto-properties | Existing property/accessor tree plus version/context policy; historical C#3 read/write restrictions, no later getter-only/property initializer feature. No backing-field generation. |

Implicitly typed local `var` syntax reuses named type/local declaration syntax. Distinguishing a contextual inference marker from a bound type named var requires binding; preserve older valid identifiers and do not reject every `var x = value` under CSharp1/2. Record inference outside parser scope. The history's CSharp3 lock-statement entry is a duplicate/misattribution: lock is already in the reviewed CSharp1 baseline and requires no new grammar.

## First proof milestone

Use test-only generated compositions over the real 19 CSharp2 modules under a distinct test namespace. Probe lambda precedence/signatures, initializer/anonymous/implicit-array typed shapes, query headers/continuations and independent module omission. Baseline fixtures, positive/malformed neighbors, UTF16 spans and full compilation-unit wrapping are mandatory. Retain any over-acceptance found by raw Pratt parsing as a policy obligation; do not mark a feature implemented from a successful fragment alone. No production .ngr or factory membership changes in this proof milestone.

## Production continuation after proof review

Implement bounded feature bundles with failing tests first, explicit introduction3.0/dependencies and preceding-version/omission observations. Keep contextual identifiers as identifiers and settle raw/escaped/@ spelling against historical prose. Avoid changing existing typed field names/meanings. A missing Nitrogen facility, material public API break or execution-model change returns for a revised decision, with a concrete reproduction.

Before completeness: audit the entire historical source, Unicode/lexical/directive differences and modern Roslyn discrepancies; pin source occurrences, reviewed owners, code/factory/policy identities and representative licensed external sources. CSharp3 completeness is not part of the initial additive slice.
