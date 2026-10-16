# C# 1 source grammar evidence — 2026-10-16

Status: implemented source-syntax milestone, **partial language coverage**. C# 2–14 feature modules have not been implemented. All 17 public profiles remain partial. No semantic C# compiler is claimed.

Production `.ngr` modules now cover non-generic names/types, expression precedence and casts, statements, attributes/parameters, type/member declarations, namespaces/compilation units, and optional unsafe extensions. The owning public API uses the generated compilation-unit root. `Root.Members` replaces the prototype `Root.Types`; `Classes` is a convenience projection of global classes only. Syntax-context policy restricts statement expressions and C# 1 stackalloc placement, and rejects unsupported static classes/auto-properties without treating ordinary contextual identifiers as new features.

## Verification scope

Normal tests exercise generated views, ownership, preprocessing, syntax forms, safe/unsafe membership, nested blocks/parentheses through depth 64, and malformed input. Oracle tests compare root kinds, selected operator child shapes, spans, source contexts, and the 88-case fixture matrix. This is targeted evidence, not proof that all applicable productions or combinations conform. Pre-review full results: 273 normal tests and 195 oracle tests passed. Final verification and review outcomes are appended below.

Two external files from Mono, pinned to commit `13d1764537ce5bbe5a06bf133fb2db1a4b5d2131`, parse successfully: `Boolean.cs` and `EventArgs.cs`. Original license notices and source hashes are retained in `tests/Corpus/Mono2004/provenance.json`. These files redefine core library types; corpus acceptance is syntax evidence, not successful semantic compilation.

## Reference and oracle limits

The [specification grammar](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/grammar), [expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions), and [attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/attributes) are current references and include later additions. Implemented productions are restricted to the C# 1 scope and checked with the pinned Roslyn CSharp1 option. A current annex alone is not a frozen historical specification.

Roslyn 5.0.0 accepts `a ?? b` with LanguageVersion.CSharp1 and declared object operands without a version diagnostic. This known oracle discrepancy is asserted explicitly in SourceMatrixTests; Nitrogen rejects it because null-coalescing was introduced in C# 2. No generic override of oracle failures is used. C# 1 `nameof` is an ordinary method identifier in the expression wrapper.

Binding, type validity, modifier compatibility, constant-expression evaluation, overload/operator arity, constructor-name matching, attribute target applicability and control-flow legality remain outside this parser. Modern Unicode category tables are recorded; historical Unicode freezing and supplementary identifiers remain unresolved. Compiler intrinsic identifiers are reserved by the earlier lexical policy but intrinsic expressions are not implemented.

## Coverage gate and continuation

See [production map](csharp1-source-production-map.md) and `features.json`. New source families are marked implemented, not exhaustively verified. Imported C# 1 release-history entries still require individual specification review and context/production closure. CSharp1 therefore does not pass the completion gate. The plan's conditional C# 2–6 handoff is deferred until that gate passes. Raw delimiter generality remains a later-version blocker.

The focused customization proof is the generated `CSharp1SafeComposition` with `Unsafe.ngr` omitted; paired fixtures reject pointers/fixed/stackalloc while retaining safe syntax and built-in sizeof. ResolveCustom currently validates/selects metadata; it does not execute arbitrary runtime language compositions.

CoverageIdentity includes composition configuration, all production grammar/source resources, inventory, and dependency identity. Nitrogen core changes, sibling edits and Git initialization remain outside this milestone.

## Fresh review

One fresh read-only review found four Important issues, all reproduced through both public-parser fixtures and independent oracle observations: casts before sizeof, predefined-type member access, stackalloc placement, and lone-comma empty array initializers. The single fix pass adds the missing grammar forms, requires at least one element before a trailing array comma, and checks stackalloc ancestry against a direct local initializer. The generated optimizer inlines VariableInitializer, so the context check observes the initializer group under VariableDeclarator. Paired assignment/return/argument/field/parenthesized negatives preserve the C# 1 restriction.

A minor production-map extraction artifact is deferred: the generated listing includes `module` and duplicate `CSharp` entries. Family status is explicitly implemented, not exhaustive verification. No semantic or later-version scope was added by review.

## Final verification

- `dotnet build CSNitrogen.slnx -t:Rebuild -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false`: succeeded, zero warnings/errors.
- Full normal suite (`dotnet test tests/CSNitrogen.Tests --no-build` with the same build properties and 30-second hang timeout): **287 passed**, zero failed/skipped.
- Full oracle suite (`dotnet test tests/CSNitrogen.Oracle.Tests --no-build` with the same properties/timeout): **215 passed**, zero failed/skipped. Includes 100 matrix cases (88 original plus 12 review contexts), 21 expression root/shape cases and both external corpus files.
- All 118 pinned Nitrogen source files match both isolated copy and original source hashes. Original revision and dirty state remain unchanged (` M docs/editor-support.md` only).
- Both corpus hashes match provenance; license notices are preserved.
- Production runtime dependency manifests contain no Microsoft.CodeAnalysis dependency. Roslyn remains in the oracle test project; Nitrogen's source generator uses compiler APIs only at build time.

Logs are retained locally under `.superpowers/sdd/csharp1-source-grammar/`: `final-build.log`, `final-normal.log`, `final-oracle.log`, `review-red.log`, `review-oracle-red.log`, and `review-green.log`. At this milestone there was no Git repository; publication was subsequently authorized and main was published at 7359d6f. Destructive cleanup remains deferred.

Next required work is C# 1 conformance closure: independently review each imported baseline feature, add per-production negative/context/tree-shape evidence, and resolve attribute argument restrictions and historical reference policy. Passing sample counts do not close those obligations. The source milestone provides executable baseline modules and an unsafe omission proof; the broader C# 1–14/customization request remains unfinished.
