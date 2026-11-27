# CSharp7.2 private protected source contract

ECMA-334 seventh edition §7.5.2 defines the accessibility as the presence of both `private` and `protected` in the member modifiers. The C# 7.2 proposal permits either order among the modifiers. The property and indexer accessor grammar in §15.7.3 and Annex A explicitly permits both two-token spellings. [Published standard](https://ecma-international.org/wp-content/uploads/ECMA-334_7th_edition_december_2023.pdf) · [C# 7.2 proposal](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-7.2/private-protected.md)

| Form | Roslyn 6.0 and 7.1 | Roslyn 7.2 and 7.3 | CSNitrogen 6.0 and 7.1 | CSNitrogen 7.2 and 7.3 |
|---|---|---|---|---|
| Class field or nested type with the modifier pair | Rejects | Accepts | Accepts | Accepts |
| Property or indexer accessor with the modifier pair | Rejects | Accepts | Rejects | Rejects |
| Ordinary protected internal field | Accepts | Accepts | Accepts | Accepts |

The executable checks use Roslyn 5.0.0. The CSNitrogen results come from an isolated comparison of the same sources. The compiler reports CS8059 under 6.0 and CS8302 under 7.1 for the new modifier pair. Both member orders, a member with an intervening `static` modifier, and both accessor orders were exercised. Roslyn also reports separate eligibility diagnostics for members of structs or static classes and rejects illegal modifier sets. [Executable witnesses](../../../tests/CSNitrogen.Oracle.Tests/CSharp72PrivateProtectedObservations.cs)

## Decision on the earlier profile boundary

**Approved on 2026-11-27.** Add either accessor spelling from CSharp7.2, then reject the newly introduced pair on ordinary members and nested types in all earlier profiles. That is a narrow correction to existing acceptance, including admitted profiles. Keep the existing modifier nodes and verify the unsupported-version diagnostic against the authored tokens. Review the changed historical behavior and refresh approval for any changed admission pins. General modifier-set validity and accessibility binding retain their separate scope.

**Compatibility option.** Add either accessor spelling from CSharp7.2 and enforce the pair boundary for CSharp7 and CSharp7.1. Retain the accepted modifier lists of the admitted CSharp1–6 profiles under their existing modifier-compatibility exclusions. Record the historical divergence for those earlier profiles.
