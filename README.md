# CSNitrogen

C# syntax experiments and versioned grammar foundations built with Nitrogen.

**Every version profile is partial.** Production Nitrogen modules now implement a C# 1 source-syntax baseline: names/types, expressions, statements, attributes, declarations, namespaces, and optional unsafe syntax, with source-preserving preprocessing. C# 2–14 feature syntax remains future work. This is a parser, not a C# semantic compiler.

## Build and test

Requires the .NET SDK 10.0.100, the .NET 10.0.0 reference pack (oracle tests), and the pinned [Nitrogen dependency](https://github.com/TovarishN/Nitrogen). For a fresh clone, obtain the recorded revision in an ignored directory:

```sh
git clone https://github.com/TovarishN/Nitrogen.git .dependencies/NitrogenSource
git -C .dependencies/NitrogenSource checkout --detach f5bf525e3e2723a8fe39e127f800f6376345feb1
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

Named profiles are `CSharp1` through `CSharp14`, plus `CSharp7_1`, `CSharp7_2`, and `CSharp7_3`. `CSharp7` means 7.0. Each currently selects the nine baseline grammar modules in `src/CSNitrogen.Syntax/nitrogen.json`; differing names do not imply later syntax is implemented. `CSharp1SafeComposition` is a generated test composition omitting `Unsafe.ngr`.

`ProfileResolver.ResolveCustom` validates feature requirements/conflicts and gives deterministic module selections. It does not build arbitrary runtime grammars or execute a custom profile through `CSharpParser`. Implemented metadata separates source grammar families, lexical categories, trivia, preprocessing and unsafe syntax. Full language coverage remains unfinished.

## Composition and capability evidence

Test-only `.ngr` probes demonstrate adding generic-call and lambda syntax by module membership, expression precedence, nested interpolation with expression nodes, and selected raw-string forms. They are deliberately separate from production grammar coverage.

The finite raw-string probe supports delimiters of lengths 3, 4, and 8. A valid delimiter of length 5 is retained as an unsupported counterexample. Arbitrary raw delimiters and full production interpolation syntax remain unresolved later-version gates. C# 1 conditional compilation is now implemented; modern directive effects remain unsupported.

See [the approved design](docs/superpowers/specs/2026-10-16-csharp-versioned-grammars-design.md), [foundation plan](docs/superpowers/plans/2026-10-16-csharp-grammar-foundation.md), [feature inventory](docs/coverage/features.json), and [evidence report](docs/coverage/foundation-evidence.md). Inventory entries imported from the official release history remain unreviewed until checked against the relevant specifications; imported entries are not coverage claims.

Named profile metadata also exposes `RequestedFeatures`, `MissingFeatures`, and `CoverageIdentity`. Requests imported from the history remain candidates pending specification review. The identity hashes the embedded inventory, dependency record, composition configuration, all grammar modules, and production parser/preprocessing/profile sources; it does not claim those inputs are complete.


The parser's `Tree.Text` and `MaskedText` contain a source-preserving preprocessing buffer. Use `OriginalText` for authored slices at the same offsets. `Directives`, `InactiveSpans`, `LineMappings`, and `MapLocation(offset)` expose preprocessing observations. Mapping line/character numbers are zero-based; an absent mapped file means the original document identity.

See the [C# 1 lexical/preprocessing evidence](docs/coverage/csharp1-lexical-preprocessing-evidence.md) for lexical tests and Unicode scope. See the [source grammar evidence](docs/coverage/csharp1-source-grammar-evidence.md) for the current milestone, oracle discrepancies and remaining gates.

Prototype API migration: `Root.Members` replaces `Root.Types` to represent namespaces and all type declarations. `Classes` returns global classes only. Generated views remain borrowed from the owning result.
