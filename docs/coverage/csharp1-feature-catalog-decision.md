# Decision: coverage names versus selectable language capabilities

The source-corpus review and public feature-selection contract are different obligations. Completing the 331 pending historical source reviews does not repair the public resolver's feature identities.

## Original reproduced boundary

The C# 1 ledger contains 40 syntax/policy inventory items plus two semantic exclusions. `FeatureCatalog.Implemented` contains 19 canonical capabilities. Twenty-one historical `csharp-1.0.*` inventory names have no selectable `FeatureDefinition`, although their source grammar is recognized through canonical bundles. For example:

- `csharp-1.0.events`, `.classes`, `.delegates`, `.interfaces`, `.properties` and `.operator-overloading` all map to `Declarations.ngr`, currently owned by `baseline.members`.
- `csharp-1.0.preprocessor-directives` maps to several preprocessing policies rather than a removable grammar module.
- `csharp-1.0.literals` covers both lexical tokens and expression alternatives.

`ProfileResolver.Resolve("CSharp1").Profile.MissingFeatures` consequently reports those 21 names. `ResolveCustom` rejects them as unknown features. Selecting `baseline.members` supplies the whole declaration bundle; it cannot promise an events-only language. Runtime parse requests currently use generated named factories; custom metadata resolution is not yet an executable arbitrary grammar factory. All public profiles remain partial.

This is a representation and customization decision, not another missing historical source production. Simply registering the 21 names as independent features would imply removability that the closed shared rules do not provide.

## Approved: canonical bundles with explicit coverage mappings

Keep the stable `baseline.*` capability identities. Add explicit inventory-to-capability coverage mappings; an inventory name is a coverage/reporting identity, not automatically a selectable independent capability. Validate that every named-profile inventory requirement is supplied by its selected canonical capabilities and compatibility policies. Keep requested inventory names and resolved capability names separately visible.

Preserve explicit dependency validation for custom selections. Reject unsupported fine-grained selections with the required bundle and a clear explanation. Do not silently enable sibling language surfaces while claiming they were omitted. Named CSharp1 completeness may become true only after source review, corpus/dependency gates, coverage mapping and actual named-factory/policy membership agree. Later and arbitrary custom profiles stay partial until their own contracts pass.

Then design C# 2-6 additive feature modules using the same canonical capability contract. Split declaration/lexical bundles at the later customization stage where removability is an explicit requirement, with typed-view migration and omission tests.

## Alternative: granular modules before more versions

Refactor existing declaration, member, statement and lexical categories into independently extensible feature modules now. Define supported combinations, dependencies, conflicts, typed discriminants and migration compatibility before accepting the historical feature names as selections. Require paired full/omitted source acceptance tests for every advertised selectable surface. This advances fine-grained customization before C# 2-6, but changes generated syntax APIs and the module dependency graph.

## Decision implemented

The user selected the recommended canonical-bundle path with “keep” and “continue”. The original boundary above records the pre-change behavior. All forty CSharp1 syntax inventory identities now have immutable coverage mappings to the nineteen canonical capabilities and required policy groups. The twenty-one reporting-only aliases remain unavailable as independent custom selections; diagnostics identify the supplying bundles. Explicit dependency validation and existing positional constructors remain compatible.

Named CSharp1 completeness requires reviewed source obligations, verified requested inventory, valid mappings, required capabilities and policies, exclusive historical Unicode policy, exact configured factory membership/start and the actual generated factory's module/root witness. `ParseDocument.ProfileComplete` carries that resolved coverage result independently of whether the input is accepted. Later and custom profiles remain partial.

See [implementation and verification evidence](csharp1-canonical-coverage-evidence.md). Fine-grained module splitting and executable custom factories remain future design work.
