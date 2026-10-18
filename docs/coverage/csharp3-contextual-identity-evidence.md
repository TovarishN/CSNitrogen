# Selected Unicode contextual identity correction

Base: PR24, `7cb1d40a5f8d6abecdd88d1917dc79e530b6ed00`. The continuing CSharp3 lexical audit found that checking identifier character validity alone did not enforce contextual identifier identity. Unicode3 classifies U+180B–U+180D as Cf; Unicode4 classifies them as Mn. The intentional generated lexical superset therefore parsed a spelling such as `get<U+180B>` as a contextual token in both versions, although Unicode4 normalization must retain the mark. The selected Unicode policy now compares the normalized identity with the contextual token's required word and reports CSN2023 on the entire token when they differ. No grammar, dependency or generated API changes were needed.

The paired regression run observed nine failures and three passes before the correction: both CSharp2 and CSharp3 incorrectly accepted the retained combining marks, and the diagnostic was absent. The correction passed those tests. Expanded evidence covers raw/escaped forms, partial/where/set/alias/add/remove/yield, ordinary identifier/type names, actual formatting characters, exact diagnostic span/category, and the reverse U+17B4/U+17B5 transition. Eighteen normal tests and three pinned Roslyn observations execute these boundaries. Roslyn's current Unicode categories differ deliberately from the frozen CSharp1 contract; it is an observation oracle, not the authority for historical Unicode3 identity.

Full verification command:

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**1,776 normal + 3,471 oracle = 5,247 passed, zero failures/skips.** The first full run caught six fail-closed source-audit failures because the new diagnostic was absent from the CSharp1 declared code inventory. That inventory and the CSharp2 source hash were updated; admission logic was unchanged. All454 SHA-pinned source/crosswalk bodies and main-text heading references reproduce, all123 dependency hashes match, and both lexical generators reproduce exact bytes. The independent whole-branch review found no actionable findings; the two reverse-transition cases were added afterward as focused coverage without further implementation changes. Authored whitespace checks pass.

CSharp3 remains partial. All454 overall crosswalk rows remain candidate/pending; the lexical report and finite contextual witness links do not constitute whole-source admission. Continue expression/type, statement/iterator and declaration/unsafe dispositions and alternative-specific obligations before the separate completeness gate.
