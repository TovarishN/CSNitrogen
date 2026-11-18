# CSharp6 production review

This maps each sixth-edition capability in the CSharp6 factory to its grammar, typed owners, checks and evidence. CSharp6 stays partial until Task 4.

| Capability | Grammar | Owners (selected) | Checks | Evidence |
| --- | --- | --- | --- | --- |
| `csharp6.interpolated-strings` | InterpolatedStrings.ngr | `InterpolatedRegularText`, `InterpolatedRegularString`, `InterpolatedVerbatimText`, `InterpolatedVerbatimString`, `Interpolation`, `InterpolationWidth`, `InterpolationFormat` | CSN2062 | seam proof, production, matrix (`interpolated_*`, `*_Interpolation_Format`) |
| `csharp6.null-conditional` | NullConditional.ngr | `NullConditionalAccess`, `NullConditionalElementAccess` | primary/unary kind lists (CSN2013, CSN2015); projection initializer in anonymous objects | production, matrix (`null_conditional_*`, `dependent_access`) |
| `csharp6.nameof` | NameofExpressions.ngr | `NameofExpression`, `NameofInvocation`, `NamedEntity` | wrapper accepts only the NameofExpression/NameofInvocation tie; CSN2001, CSN2013 and CSN2015 accept it | seam proof, production, matrix |
| `csharp6.exception-filters` | ExceptionFilters.ngr, `Statements.CatchFilter` | `ExceptionFilter` | grammar order (filter-only catch is specific) | production, matrix (`specific_catch_clause`, `exception_filter`) |
| `csharp6.using-static` | UsingStatic.ngr, `CompilationUnits.UsingKind` | `UsingStaticMarker` | grammar (no alias) | production, matrix |
| `csharp6.property-initializers` | PropertyInitializers.ngr, `Declarations.PropertyInitializerSeam` | `PropertyInitializer` | CSN2060, CSN2063 | production, matrix |
| `csharp6.expression-bodies` | ExpressionBodies.ngr, `Declarations.MethodBodyExtension` / `MemberDeclarationExtension` | `ExpressionBody`, `ExpressionBodiedProperty`, `ExpressionBodiedIndexer` | CSN2061; CSN2053 adjusted | production, matrix (`method_body`, `property_body`, `indexer_body`, `operator_body`) |
| `csharp6.index-initializers` | IndexInitializers.ngr, `ObjectInitializers.InitializerExtension` | `IndexedObjectInitializer`, `IndexInitializer` | — | production, matrix (`initializer_target`) |
| `csharp6.enum-base-type-names` | EnumBaseTypeNames.ngr | `NamedBaseEnumDeclaration`, `NamedEnumBase` | CSN2012 and the partial-modifier check cover the new owner | production, matrix (`enum_base`, `integral_type_name`) |
| `csharp6.await-in-catch-finally` | — | — | CSN2050 restricted to version 5 | production |
| `csharp6.read-only-auto-properties` | — | — | CSN2032 accepts a lone `get;` from 6 | production |

## Factory and lexical contract

- The CSharp6 factory is the CSharp5 factory plus the nine modules above. It selects `GlobalTargetsThird.ngr` and frozen Unicode4 scalars (`unicode-4.0-scalar`, `CSharpTrivia.SkipUnicode4`).
- CSharp7 and later factories include the sixth modules but keep current-compiler lexical behaviour, and they are not reviewed.
- The empty seams added to `Statements`, `Declarations`, `CompilationUnits` and `ObjectInitializers` are filled in no CSharp1–5 factory. Those profiles remain admitted, and their pins are refreshed in the order 5→4→3→2.

## Typed-view stability

No existing generated property changes type. The seams add optional fields (`CatchClause.Filter`, `PropertyDeclaration.Initializer`, `UsingDirective.Static`) and new owners only. `MemberInitializerNode.Name` remains a `Token`, and `PropertyDeclaration.Accessors` is unchanged.

## Known limitations

- `await x;` as a statement in an async body reads as a declaration with type `await` (CSN2051). This predates C# 6.
- Expression-bodied members skip the same modifier checks that block-bodied members skip; that is the inherited modifier-compatibility exclusion.
- A semicolon-only, non-abstract indexer is accepted in every profile (see the normative context review).
- `\U0000005F` is accepted as an identifier start, although the sixth `Underscore_Character` lists only `_`/`_`.
