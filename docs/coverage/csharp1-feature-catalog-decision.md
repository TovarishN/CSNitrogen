# Decision: coverage names versus selectable language capabilities

The source-corpus review and public feature-selection contract are different obligations. Completing the 331 pending historical source reviews does not repair the public resolver's feature identities.

## Reproduced boundary

The C# 1 ledger contains 40 syntax/policy inventory items plus two semantic exclusions. `FeatureCatalog.Implemented` contains 19 canonical capabilities. Twenty-one historical `csharp-1.0.*` inventory names have no selectable `FeatureDefinition`, although their source grammar is recognized through canonical bundles. For example:

- `csharp-1.0.events`, `.classes`, `.delegates`, `.interfaces`, `.properties` and `.operator-overloading` all map to `Declarations.ngr`, currently owned by `baseline.members`.
- `csharp-1.0.preprocessor-directives` maps to several preprocessing policies rather than a removable grammar module.
- `csharp-1.0.literals` covers both lexical tokens and expression alternatives.

`ProfileResolver.Resolve("CSharp1").Profile.MissingFeatures` consequently reports those 21 names. `ResolveCustom` rejects them as unknown features. Selecting `baseline.members` supplies the whole declaration bundle; it cannot promise an events-only language. Runtime parse requests currently use generated named factories; custom metadata resolution is not yet an executable arbitrary grammar factory. All public profiles remain partial.

This is a representation and customization decision, not another missing historical source production. Simply registering the 21 names as independent features would imply removability that the closed shared rules do not provide.

## Recommended: canonical bundles with explicit coverage mappings

Keep the stable `baseline.*` capability identities. Add explicit inventory-to-capability coverage mappings; an inventory name is a coverage/reporting identity, not automatically a selectable independent capability. Validate that every named-profile inventory requirement is supplied by its selected canonical capabilities and compatibility policies. Keep requested inventory names and resolved capability names separately visible.

Preserve explicit dependency validation for custom selections. Reject unsupported fine-grained selections with the required bundle and a clear explanation. Do not silently enable sibling language surfaces while claiming they were omitted. Named CSharp1 completeness may become true only after source review, corpus/dependency gates, coverage mapping and actual named-factory/policy membership agree. Later and arbitrary custom profiles stay partial until their own contracts pass.

Then design C# 2-6 additive feature modules using the same canonical capability contract. Split declaration/lexical bundles at the later customization stage where removability is an explicit requirement, with typed-view migration and omission tests.

## Alternative: granular modules before more versions

Refactor existing declaration, member, statement and lexical categories into independently extensible feature modules now. Define supported combinations, dependencies, conflicts, typed discriminants and migration compatibility before accepting the historical feature names as selections. Require paired full/omitted source acceptance tests for every advertised selectable surface. This advances fine-grained customization before C# 2-6, but changes generated syntax APIs and the module dependency graph.

## Choice required

Choose canonical bundles with honest coverage mappings now (recommended), or prioritize the granular-module refactor before adding versions. The current corpus work does not silently choose either public contract. The runtime-profile-contract gate remains pending until this decision is implemented and verified.
