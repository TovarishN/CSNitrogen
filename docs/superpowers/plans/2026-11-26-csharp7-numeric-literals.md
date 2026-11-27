# CSharp7 numeric literal implementation

The [approved tree contract](../specs/2026-11-26-csharp7-numeric-syntax-decision.md) uses additive expression nodes.

1. Add compilation-unit witnesses in `CSharp7NumericLiteralTests.cs` for the 7.0 and 7.2 version boundaries, exact spans, casts, malformed suffixes and separators, and the existing literal node. Run the focused test before implementing the new grammar.
2. Extend the Pratt expression point from `NumericLiterals.ngr` and `LeadingDigitSeparators.ngr`. Match only the spellings introduced by each extension. Keep the numeric text in one token and verify exact expression kinds and the standalone expression and literal entry points.
3. Register independent capabilities in `FeatureCatalog.cs`, map the source inventory in `features.json`, enable the modules in the named Nitrogen profiles, and regenerate the Nitride projects. Verify compositions with and without the 7.2 extension.
4. Refresh admission pins and run the full suite in a disposable snapshot with a verification-only CSharp6 approval. Publish the exact candidate ledger for review with the real approval record intact.
