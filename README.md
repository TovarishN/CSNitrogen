# CSNitrogen

C# syntax experiments and versioned grammar foundations built with Nitrogen.

**CSharp1, CSharp2 and CSharp3 have reviewed normalized syntax coverage; CSharp4–14 and custom profiles remain partial.** Production Nitrogen modules implement names/types, expressions, statements, attributes, declarations, namespaces, unsafe syntax and the C# 2 additions, with source-preserving preprocessing and historical Unicode policies. This is a parser, not a C# semantic compiler.

## Build and test

Requires the .NET SDK 10.0.100, the .NET 10.0.0 reference pack (oracle tests), and the pinned [Nitrogen dependency](https://github.com/TovarishN/Nitrogen). For a fresh clone, obtain the recorded revision in an ignored directory:

```sh
git clone https://github.com/TovarishN/Nitrogen.git .dependencies/NitrogenSource
git -C .dependencies/NitrogenSource checkout --detach 6149713ea5348e3fff61d032c441fc6d6cf733e2
python3 eng/prepare-dependencies.py --source .dependencies/NitrogenSource
dotnet build CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
dotnet test tests/CSNitrogen.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
dotnet test tests/CSNitrogen.Oracle.Tests -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

For an existing Nitrogen checkout, pass its path to `--source` instead. Preparation copies build sources and records their hashes without modifying that checkout.

`NuGetAudit=false` makes the commands independent of the remote vulnerability-metadata endpoint; it does not disable dependency restore. The local snapshot identity and source hashes are recorded in `docs/coverage/dependencies.json`. An explicit `-p:NitrogenRoot=/path/to/buildable/snapshot` overrides the snapshot location; use an isolated copy if that path must remain unchanged.

## Owning parse API

```csharp
using CSNitrogen.Parsing;

using var document = CSharpParser.Parse(
    new ParseRequest("class Example {}", "CSharp14", "example.cs", []));
Console.WriteLine(document.Accepted);        // true for this supported input
Console.WriteLine(document.ProfileComplete); // false
Console.WriteLine(document.Classes[0].Name); // Example
```

The result owns the pooled Nitrogen tree. Borrowed nodes and typed views expire when it is disposed. Access `Root` when the parse is accepted; an error tree may not have a compilation-unit root. The generated views are syntax types; the API does not bind or type-check C# programs.

Named profiles are `CSharp1` through `CSharp14`, plus `CSharp7_1`, `CSharp7_2`, and `CSharp7_3`. `CSharp7` means 7.0. CSharp2 selects the baseline plus the implemented C# 2 modules; CSharp3 and later also select the bounded C# 3 additions in `src/CSNitrogen.Syntax/nitrogen.json`; CSharp3 has a separately reviewed normalized syntax contract, while differing later names do not imply complete C#4–14 syntax coverage. `CSharp1SafeComposition` is a generated test composition omitting `Unsafe.ngr`.

`ProfileResolver.ResolveCustom` validates feature requirements/conflicts and gives deterministic module selections. It does not build arbitrary runtime grammars or execute a custom profile through `CSharpParser`. Implemented metadata separates source grammar families, lexical categories, trivia, preprocessing and unsafe syntax. Custom resolutions describe canonical capability bundles and do not advertise independently removable inventory entries.

## Composition and capability evidence

Test-only `.ngr` probes demonstrate adding generic-call and lambda syntax by module membership, expression precedence, nested interpolation with expression nodes, and selected raw-string forms. They are deliberately separate from production grammar coverage.

The finite raw-string probe supports delimiters of lengths 3, 4, and 8. A valid delimiter of length 5 is retained as an unsupported counterexample. Arbitrary raw delimiters and full production interpolation syntax remain unresolved later-version gates. C# 1 conditional compilation is implemented. C# 2 pragma payloads are preserved as opaque implementation-defined information with a warning; compiler warning/optimization effects are not applied. Other later directive effects remain unsupported.

See [the approved design](docs/superpowers/specs/2026-10-16-csharp-versioned-grammars-design.md), [foundation plan](docs/superpowers/plans/2026-10-16-csharp-grammar-foundation.md), [feature inventory](docs/coverage/features.json), and [evidence report](docs/coverage/foundation-evidence.md). Inventory entries imported from the official release history remain unreviewed until checked against the relevant specifications; imported entries are not coverage claims.

Named profile metadata also exposes `RequestedFeatures`, `MissingFeatures`, and `CoverageIdentity`. Requests imported from the history remain candidates pending specification review. The identity hashes the embedded inventory, dependency record, composition configuration, all grammar modules, and production parser/preprocessing/profile sources; it does not claim those inputs are complete.


The parser's `Tree.Text` and `MaskedText` contain a source-preserving preprocessing buffer. Use `OriginalText` for authored slices at the same offsets. `Directives`, `InactiveSpans`, `LineMappings`, and `MapLocation(offset)` expose preprocessing observations. Mapping line/character numbers are zero-based; an absent mapped file means the original document identity.

See the [C# 1 lexical/preprocessing evidence](docs/coverage/csharp1-lexical-preprocessing-evidence.md) for lexical tests and Unicode scope. See the [source grammar evidence](docs/coverage/csharp1-source-grammar-evidence.md) for the current milestone, oracle discrepancies and remaining gates.

Prototype API migration: `Root.Members` replaces `Root.Types` to represent namespaces and all type declarations. `Classes` returns global classes only. Generated views remain borrowed from the owning result.

The [attribute conformance continuation](docs/coverage/csharp1-attribute-conformance-evidence.md) records the latest C# 1 fixes and oracle classifications. The initial baseline is published at [TovarishN/CSNitrogen](https://github.com/TovarishN/CSNitrogen).

The [context conformance continuation](docs/coverage/csharp1-context-conformance-evidence.md) records catch/accessor/constructor checks, corrected conditional tree shapes, the historical reference policy and expanded corpus. Prototype generated-view change: conditional false arms use `.False.Value`.

CSharp1 selects pinned Unicode 3.0 BMP categories, CSharp2 selects pinned Unicode 4.0 scalar categories, and later partial profiles retain current .NET BMP categories. Raw identifier grammar is a union and requires `CSharpSyntaxPolicy.Check(tree, version)` when bypassing the public parser. See [historical Unicode evidence](docs/coverage/csharp1-historical-unicode-evidence.md). Global attribute targets now expose a typed alternative containing their Token.

The [names/types conformance continuation](docs/coverage/csharp1-type-conformance-evidence.md) maps every NamesTypes production to direct fixtures and independent typed child/span comparisons. Compiler-deferred void and standalone array-rank oracle differences are recorded explicitly.

The [cast/expression conformance continuation](docs/coverage/csharp1-cast-conformance-evidence.md) compares recursive cast operands, operator grouping and authored spans. It adds finite malformed and safe-module observations without claiming full expression coverage.

The [array/argument continuation](docs/coverage/csharp1-array-argument-evidence.md) checks dimensions, rank order, nested initializer elements, ref/out arguments and separator spans. `CSN2005` rejects direct array-creation element targets in the standard baseline; parenthesize the creation before indexing. Roslyn acceptance of initialized direct indexing is recorded as a reference discrepancy.

The [statement/control-tree continuation](docs/coverage/csharp1-statement-conformance-evidence.md) checks nested bodies, dangling else, loop headers, switch sections and exception handlers. `CSN2006` rejects duplicate default labels per switch. Missing using-resource initializers and other semantic validity remain excluded.

The [declaration/member continuation](docs/coverage/csharp1-declaration-conformance-evidence.md) compares selected typed headers, nested member order, signatures, parameters, accessors and bodies against Roslyn. The added observations require no grammar changes; constructor-name matching, modifier compatibility and operator type/arity checks remain semantic exclusions.

The [compilation-unit/namespace continuation](docs/coverage/csharp1-compilation-unit-conformance-evidence.md) checks ordered typed roots, namespaces, using aliases, global attributes, masked-source spans and borrowed-view disposal. `CSN2012` rejects global attribute targets misplaced on source-level types; local member target applicability remains excluded.

The [unsafe-composition continuation](docs/coverage/csharp1-unsafe-conformance-evidence.md) checks pointer/fixed/stackalloc views and paired module omission. The configured safe factory differs by exactly `CSharp.Unsafe`; built-in `sizeof` and ordinary shared operators remain available. This demonstrates syntax membership, while semantic safety and runtime customization remain separate work.

The [C# 1 coverage gate](docs/coverage/csharp1-coverage-gate-evidence.md) now audits the configured grammar catalog, policy mappings, fixture links and explicit completion obligations. Run `dotnet test tests/CSNitrogen.Oracle.Tests --filter FullyQualifiedName~CSharp1CoverageGateTests`. The [historical standard crosswalk](docs/coverage/csharp1-standard-crosswalk-evidence.md) now preserves 362 independently extracted grammar-heading occurrences and their candidate owners. The [root-type/pointer review](docs/coverage/csharp1-pointer-boundary-normative-evidence.md) records an earlier 31-occurrence milestone; its historical valid-syntax scope and invalid-referent phase differences remain explicit.

The [whole pending C# 1 source review](docs/coverage/csharp1-pending-normative-review.md) closes the remaining 331 occurrences: all 362 now have scoped normative/context/span review. Historical lexical/directive and Pratt-category discrepancies are corrected, and [exact logical line mapping](docs/coverage/csharp1-line-mapping-decision.md) uses `BigInteger` with checked legacy access. The approved [canonical capability contract](docs/coverage/csharp1-feature-catalog-decision.md) maps all 40 historical syntax inventory identities to 19 canonical capabilities and required policies. `FeatureCatalog.CoverageMappings` exposes these reporting mappings; selecting an inventory-only name produces `coverage-only-feature` with the required bundle. `CSharp1` reports `Complete` and `ProfileComplete` only when source, mappings, factory and policies agree. Completeness describes syntax coverage independently of input acceptance and excludes binding/type/value semantics. CSharp4–14/custom profiles remain partial. See [verification evidence](docs/coverage/csharp1-canonical-coverage-evidence.md).

The [C# 2 conformance closure](docs/coverage/csharp2-conformance-review.md) reviews all 409 fourth-edition Annex A occurrences and the separate fixed-buffer supplement. CSharp2 completeness requires the pinned source/crosswalk/code/factory/policy gate; capability mapping alone is insufficient. [Verification evidence](docs/coverage/csharp2-conformance-evidence.md) records the actual commands and limits. The [generated is/as operand API migration](docs/coverage/csharp2-type-operand-migration.md) is an approved compatibility change. Custom resolutions remain metadata only.

The [CSharp3 historical source and seam proof](docs/coverage/csharp3-seam-proof-evidence.md) pins the original Version 3.0 specification and exercises test-only additive lambda, query and construction grammars over the CSharp2 baseline. It records known source-policy gaps and the production continuation plan; named profiles remain unchanged.

The [CSharp3 production continuation](docs/coverage/csharp3-production-evidence.md) adds lambda/query/construction/receiver modules and historical declaration context policies. The Unicode4 contract and three generic-member corrections were approved. The [reviewed CSharp3 admission](docs/coverage/csharp3-reviewed-admission-evidence.md) closes455 source occurrences and the1175-alternative representative matrix through a separate pinned source/factory/policy/evidence gate. Named CSharp3 now reports completeness independently of document acceptance; semantic compilation remains excluded.
