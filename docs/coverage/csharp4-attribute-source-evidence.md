# CSharp4 attribute source closure

Base: merged PR39 5d13294daab3140a6d1393eaa1dd8561eb1a7e5d, treed862b881938ef373886cc9d0c03b2200165d2687. CSharp4 remains partial; its independent index and candidate comparison are not source admission. Mirrored source delivery and unverified publisher-byte equivalence remain explicit.

## Controlling source and production change

Version4 main7855 and appendix9152 add argument-nameopt to an attribute positional-argument. Main7920 compiles the positional list P as an object creation new T(P), so source-local constructor argument order and repeated logical labels follow the existing4 argument rules. Member named arguments remain identifier = expression at7857. Main7859 requires constructor/positional arguments before named member arguments. Constructor labels and member assignments are different lists even if both use the same identifier.

PositionalAttributeArgument gains an optional Name view using the existing Expressions.ArgumentName extension point; its Value expression property retains its type and meaning. The independently removable csharp4.named-arguments capability fills that point. CSharp1/2/3 leave Name absent and retain original positional/member values. No new core mechanism, runtime arbitrary factory or existing property-type change is introduced.

CSharp4ContextPolicy keeps constructor label ordering and normalized duplicate-name state per Attribute owner. Nested invocation argument lists have their own owners; member Name= assignments do not enter the constructor-name set. The existing attribute policy still requires all constructor arguments before member assignments. Ref/out and trailing commas remain outside the attribute positional expression grammar. Parameter/member lookup, constantness, conversions, overload resolution and attribute/type legality remain semantic exclusions.

The inspected4 target grammar at7844/7849 retains the third-version categories. CSharp4 now uses that recognized-target policy, including strict raw reserved event/return spellings and excluding typevar/unknown names. Applicability remains a semantic exclusion, as in reviewed3; this does not claim to reject every target on a wrong declaration kind. Main7858 remains expression, so assignment-shaped values are normalized syntax even where constantness/writability makes them semantically invalid. The prior current-compiler non-assignment rejection is retained only for5+ pending their own review. Predecessor policies and admission logic are unchanged; source hashes are refreshed in directed order.

## Retained observations and verification

The initial30 source cases had19 failures and11 passes: missing constructor labels, target recognition and assignment-expression boundary differences. After the bounded change, all30 passed. Twenty additional structural observations prove:

- Separate constructor/member/invocation typed owners, Name and Value children, exact UTF16 spans after supplementary trivia.
- Absent Name and preserved Value/member fields on actual CSharp1/2/3 parses.
- Per-attribute and nested-call state, exact rejected source pieces, malformed/incomplete neighbors.
- Omitting named arguments removes constructor labels while preserving ordinary attribute syntax/defaults/variance; labels survive omission of either other capability.
- Selected later factories expose the addition while remaining partial.

Three independent Roslyn5.0.0 ParseText observations with LanguageVersion.CSharp4 agree on constructor NameColon, member NameEquals and expression child spans, including an ordinary positional prefix, identical constructor/member names, verbatim labels, alias qualification and conditional colons. They establish syntax observations, not compilation, historical compiler equivalence, constantness or binding.

Full solution:4,572 normal +3,684 oracle =8,256 passed, zero failures/skips. Command: `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false`. The four grammar generators reproduce identical bytes; all125 dependency pins match. Fresh source4 extraction/candidate checks reproduce458 occurrences/1181 alternatives. Fresh whole-branch review found no actionable issues. It inspected changed implementation/generated accessors and controlling local clauses without executing tests. Whole fourth-version admission, semantic binding/constantness/applicability/runtime validity, original publisher bytes, complete later versions and arbitrary custom runtime factories remain excluded/pending. Unchanged index/variance/default policies were inspected only for interaction; their full regression evidence is executor-owned. Whole CSharp4 context/alternative closure and independent admission remain separate work. Unrelated benchmarks are preserved.
