# CSharp6 production capabilities

Task 2 of [the CSharp6 plan](../superpowers/plans/2026-11-04-csharp6-grammars.md). It moves the reviewed seam proof ([csharp6-seam-proof-evidence.md](csharp6-seam-proof-evidence.md)) into production. CSharp6 stays partial: every sixth-edition syntax inventory entry is implemented, but no independent admission guard exists (Task 4).

## Factory and catalog

- The empty seams from the proof are now in production `Statements.ngr`, `Declarations.ngr`, `CompilationUnits.ngr` and `ObjectInitializers.ngr`. CSharp1–5 fill none of them, so their behaviour and module lists are unchanged. The new rules are recorded in the CSharp1 and CSharp2 source gates, following the CSharp5 precedent.
- Eight modules (`InterpolatedStrings`, `NullConditional`, `NameofExpressions`, `ExceptionFilters`, `UsingStatic`, `PropertyInitializers`, `ExpressionBodies`, `IndexInitializers`) join the CSharp6 factory and every later factory, including the 7.x minors. The Nitride projects are regenerated.
- `FeatureCatalog` gains ten `csharp6.*` capabilities. Two of them are policy-only: `await-in-catch-finally` and `read-only-auto-properties`. Each of the ten 6.0 syntax entries in `features.json` maps to them and has status `implemented`.

## Lexical contract

CSharp6 uses frozen Unicode4 scalars (`UnicodePolicy.UsesUnicode4` now includes major 6, and the factory uses `CSharpTrivia.SkipUnicode4`). The resolved policy is `unicode-4.0-scalar`.

## Parse wrapper and policies

- `CSharpParser` drops Nitrogen's Ambiguous diagnostic only when the node is the sanctioned tie of `NameofExpression` and `NameofInvocation` and the profile is major 6 or later. Any other tie still reports `NG3`.
- `CSharp6ContextPolicy` (ECMA-334 6th edition) adds three checks:
  - **CSN2060:** a property initializer needs an automatically implemented property that is neither abstract nor extern (§14.7.4).
  - **CSN2061:** constructors, finalizers and accessors have no expression bodies in C# 6.
  - **CSN2062:** an unparenthesized conditional in an interpolation expression or width is rejected, because its colon would begin the format (§11.7.3).
- Existing checks updated for sixth syntax:
  - The primary and unary kind lists (CSN2013, CSN2015) include the null-conditional, nameof and interpolated kinds and the sanctioned nameof node.
  - The statement-expression check (CSN2001) accepts the sanctioned nameof node.
  - The inferred anonymous-member check accepts `a?.b` (the null-conditional projection initializer).
  - From version 6, the automatic-property rule (CSN2032) also accepts a lone `get;`.
  - CSN2053 no longer treats an expression body as a semicolon body.
  - The await catch/finally restriction (CSN2050) was already limited to version 5.

## Known limitation, not changed here

In an async body, the existing factory reads `await x;` as a declaration of `x` with type `await` and reports CSN2051. The CSharp5 tests use `await(M());`, and so do the sixth tests. This predates CSharp6.

## Verification

`dotnet test CSNitrogen.slnx -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` passes 11,631 tests (7,558 main, 3,884 oracle, 55 CSharp5 proof, 134 CSharp6 proof) with zero failures or skips. `CSharp6ProductionTests` adds 56. CSharp1–5 remain admitted with pins refreshed 5→4→3→2. `eng/refresh-admission-pins.py --check` and `eng/generate-nitride-projects.py --check` reproduce.
