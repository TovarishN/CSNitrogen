# CSharp7 numeric literal tree contract

C# 7.0 adds binary integers and separators between digits of numeric literals. C# 7.2 adds separators immediately after the binary and hexadecimal prefixes. The two additions need independently selectable grammar modules so a custom language can use the C# 7.0 forms without taking the 7.2 forms. The source contract leaves value conversion and overflow to semantic analysis. [Release observations](../../coverage/csharp7-release-boundary-evidence.md) · [C# 7.2 separator specification](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-7.2/leading-separator.md)

## Preserve the current token API (recommended)

Keep the existing `LiteralExpression.Value` property as `Nitrogen.Token`. A composition that includes the numeric extensions supplies an additional expression alternative for the new spellings. Each new numeric expression keeps the full literal source span and its numeric token. Existing literal visitors can continue to consume their current node and token types. Visitors and patterns covering every literal in a CSharp7 profile need to handle the new alternative as well.

The main compilation-unit parser will recognize the variants wherever expressions are permitted. The existing `CSharp.Expressions.Literal` fragment and quotation category keep their current spellings. Callers can use `Expression` for the new forms. Verify named-cast lookahead and document the fragment and quotation entry points with witnesses.

## Use one literal expression type

Add a transparent extensible choice inside the shared literal rule. This keeps `LiteralExpression` as the expression type for all literal spellings, including direct recognition through that shared rule. The generated `LiteralExpression.Value` property becomes `Nitrogen.SyntaxNode` across the CSharp1–14 compositions. Callers using this property as a `Token` would need to update their code and recompile. The probe still produces one token leaf for each numeric value.

A token rule that directly refers to this extension point fails in Nitrogen with `NGR0112`. The current generator supports extending syntax rules and emits a syntax node property for the shared choice, even when the extension body is transparent.

A third route uses a shared token rule that accepts all spellings and a host policy that rejects disallowed versions after parsing. That preserves both the type and the expression node, but grammar-only compositions would include the later numeric spellings even with the corresponding capability omitted.

## Proof and acceptance

An isolated grammar built with pinned Nitrogen revision `b3494870dfb5f187bae9065227d4ef7f742aa048` exercised both options. The additive alternative and the baseline each exposed `Token`, while the shared choice exposed `SyntaxNode`. Both extensions accepted `1_2` and `1_2.3_4` as one leaf, retained the exact source text, accepted the historical control `12.34`, and rejected `12_`. The baseline rejected both decorated spellings.

Production verification should run through compilation units, expression fragments and typed literal views. Use binary and decimal integers, hexadecimal integers, real fractions and exponents, suffixes, numeric member access, and malformed separator placement. Make each additive alternative exclusive to the spellings it introduces so ordinary literals retain their node identity. Verify the 7.0 and 7.2 transitions, inherited profiles and each custom module selection. The CSharp6 gate pins the shared composition and grammar resources, so a production candidate that changes those inputs requires renewed approval of its exact ledger digest.
