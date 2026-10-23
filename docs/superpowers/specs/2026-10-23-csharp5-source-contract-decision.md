# CSharp5 source contract and Unicode decision

Status: decision required before CSharp5 production lexical/grammar work. CSharp4 admission is merged as PR46; CSharp5 remains partial. This proposal records source investigation, not an approved fifth-version contract.

## Publisher source

Microsoft Download Center item7029 identifies C# Language Specification5.0 and delivers CSharp Language Specification.docx directly from download.microsoft.com. Discovery: https://www.microsoft.com/en-us/download/details.aspx?id=7029 . Direct source: https://download.microsoft.com/download/0/b/d/0bda894f-2ccd-4c2c-b5a7-4eb1171962e5/CSharp%20Language%20Specification.docx . Downloaded bytes SHA256 6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519,791626 bytes. The document header says Version5.0 and Microsoft1999–2012. Its current download listing date is not the document's language-version date.

Local macOS textutil extraction SHA256 9e1d368ec4216701c3eec65d599e0ced499841df0eff778febd520cb52fa051e. Line references below split only at LF, preserving Word U+2028 within paragraphs. The external document is not redistributed.

## Required frozen Unicode choice

Main1501 and reference10131 again cite Unicode3.0. Reviewed CSharp2/3/4 use frozen Unicode4 scalar categories; their continuity choices were explicitly approved. Following the repeated Unicode3 reference literally at5 would reject some identifiers already accepted by those profiles, including U+0220 and U+10400. The current partial5 instead selects host-current BMP categories through UnicodePolicy.UsesUnicode4, which covers2/3/4 only. That source selector is verified; no new fifth-version parser execution is claimed by this document.

Recommendation: retain frozen Unicode4 scalar continuity for CSharp5, treating the repeated Unicode3 reference as informational, as already approved separately for3 and4. This preserves existing identifiers and avoids a host-dependent lexical contract. Alternative: literal frozen Unicode3, knowingly accepting the backward identifier contraction. This is a source-authority and compatibility decision, not a parser implementation convenience. No Unicode5 policy changes will be made before the answer.

## Bounded implementation direction after the decision

Use admitted CSharp4 as the baseline and retain independent capabilities. Main4430–4437 defines await unary-expression, permits it only inside async functions, and forbids it in nested non-async anonymous functions, catch/finally, lock and unsafe contexts. Main5174–5175 adds optional async to lambdas and anonymous methods; main7800–7802 defines async method/anonymous-function owners and ref/out restrictions. Return task identity, awaiter pattern lookup, inferred types, state machines and execution remain semantic work.

The appendix is stale for at least the anonymous-function summaries: main9841–9842 omits the async prefix present in main5174–5175. Await-expression appears in the main grammar at4430. The fifth-version index must retain this discrepancy and source-defined main-text additions explicitly; copying the appendix alone is insufficient. Review all remaining source differences before admission.

A test-only typed seam proof precedes production grammar edits. Preserve existing parameter/body/signature property types, ordinary async/await identifiers outside their source-defined contexts, contextual identifier boundaries, module omission and original UTF16 ranges. A real public property-type or Nitrogen core change returns for a separate concrete decision. No such change is currently proposed.

Caller-info attributes at main8767/8856–8876 use existing attribute/default-parameter spelling; their compiler-supplied argument values are semantic behavior. Review the release-history inventory classification rather than claiming that syntax recognition implements caller-information substitution. CSharp5 and later/custom profiles remain partial until their own independent source/factory/policy/evidence admission.
