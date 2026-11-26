# CSharp7 release boundary observations

The release checks compile the same witnesses under `CSharp6`, `CSharp7`, `CSharp7_1`, `CSharp7_2`, `CSharp7_3` and `CSharp8`. They use the repository's Roslyn 5.0.0 oracle and .NET 10.0.0 reference pack. A first accepted release below means that the witness has no compiler errors at that release or any of the later settings tested. These observations help date syntax and distinguish binding rules. The published seventh edition governs the CSharp7.3 syntax contract. [Executable witnesses](../../tests/CSNitrogen.Oracle.Tests/CSharp7ReleaseObservationTests.cs) · [Source review](csharp7-main-text-review.md)

| First release in the probe | Representative observations |
|---|---|
| 6.0 | Typed `default(int)` is an inherited control. |
| 7.0 | Binary literals, digit separators, out variable declarations, guarded type patterns, tuples and deconstruction, local functions, ref locals and returns, expression-bodied constructors and accessors, and throw expressions. |
| 7.1 | Untyped `default`, use of inferred tuple names, and the sampled type pattern on a type parameter. |
| 7.2 | `in` parameter and `ref readonly` return, `readonly ref struct`, conditional `ref`, `private protected`, positional arguments after in-position named arguments, leading separators after a base prefix, and `stackalloc` as a `Span<T>` local initializer or either result of its conditional. |
| 7.3 | `stackalloc` array initializer, ref reassignment, `unmanaged` constraint, tuple equality, and the sampled field initializer with an out variable. Several reuse existing source forms and become valid through contextual or binding changes. |
| 8.0 | `stackalloc` in a call argument. |

A digit separator at the end of a literal is rejected in every setting tested. The backing-field attribute target reports warning `CS8371` before 7.3 and that warning disappears at 7.3. The `stackalloc` assignments in the CSharp7.3 witnesses have no parse errors and report `CS8353` when compiled. Keep the placement check from ECMA-334 §12.8.21 separate from the compiler's escape analysis.

Microsoft's release-history summary lists stack allocation array initializers under both 7.2 and 7.3. The dedicated initializer proposal places them in 7.3 and the compiler rejects that witness at 7.2. Conditional and direct `Span<T>` local initialization without the array initializer already work at 7.2. [Microsoft release history](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-version-history#c-version-73) · [C# 7.2 span safety proposal](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-7.2/span-safety.md) · [C# 7.3 initializer proposal](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-7.3/stackalloc-array-initializers.md)
