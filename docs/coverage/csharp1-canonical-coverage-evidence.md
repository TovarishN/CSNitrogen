# CSharp1 canonical coverage contract — 2026-10-17

The approved canonical-bundle decision is implemented. All forty CSharp1 syntax/policy inventory identities map explicitly to nineteen selectable canonical capabilities and required policies. Twenty-one historical aliases remain reporting identities; a custom selection of an alias returns `coverage-only-feature` and identifies the supplying bundle. Dependencies must be selected explicitly. `CoverageMappings` and its inner lists are immutable.

Named CSharp1 now reports complete reviewed syntax coverage. This requires verified source obligations and inventory status, valid nonempty mappings with known unique targets, all mapped capabilities/policies, exclusive historical Unicode policy, exact configured grammar membership/start, and the actual generated factory's module membership and compilation-unit root witness. Source guards retain all 362 pinned heading occurrences, their four review dimensions, exact source identity, owners and policy-group ownership. The oracle coverage audit evaluates supplied mutated metadata through the same runtime contract rather than trusting status labels.

`ParseDocument.ProfileComplete` carries the resolved profile result. A rejected source can therefore have complete profile coverage while `Accepted` is false. Binding, type/value validity, semantic safety, execution and universal recovery/diagnostic-phase parity remain excluded. Later named profiles and custom metadata resolutions remain partial; this does not implement arbitrary custom executable factories or fine-grained module removal. Existing positional feature constructors remain compatible.

## Verification

Public behavior, mapping/audit mutations and policy guards were observed failing before their fixes. The final policy review reproduced two failures: simultaneous historical/current Unicode policies and swapped policy-group source lists with an unchanged union. Both regressions pass after exclusive Unicode and exact per-group ownership checks. The focused contract suite passes 41 tests.

The final full command was:

```sh
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s
```

Result: **799 normal + 3,245 oracle = 4,044 passed**, zero failures/skips and no compiler warnings. `git diff --check` passes. Independent read-only review checked mappings, factory/root guards, immutable exposure, constructor compatibility, later/custom boundaries and completeness propagation. Its two robustness findings were reproduced, fixed and rechecked with no remaining source findings.

All 123 recorded isolated Nitrogen build-copy hashes match. The 118 source artifacts (excluding project/build configuration) match in both original and isolated trees, including all 115 C# sources. The original checkout has advanced independently to `b8bae6283874dcb49cb7cefacf25ab27e43d802e`; its root build properties differ from the older recorded snapshot and its existing documentation modification remains outside this change. The isolated build remains pinned to the recorded dependency manifest. All six external corpus hashes match provenance. Both production dependency manifests exclude Microsoft.CodeAnalysis and Nitrogen.Grammar. No sibling repository edits were performed.
