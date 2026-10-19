# CSharp4 source contract and bounded seam proof

Status: user approved Unicode4 scalar continuity on2026-10-19; bounded seam proof in progress. The standing continuation authorizes source investigation and test-only feasibility work; no new production grammar/profile/Unicode behavior is claimed.

## Working historical source

The available DOC identifies itself as C# Language Specification Version4.0, with Microsoft1999–2010 notice and Word2010 metadata. Delivery is the historical CSharp in Depth mirror, not a verified current Microsoft download. URL https://csharpindepth.com/Files/CSharp%204%20Specification.doc; SHA256 cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7,2501632 bytes. The hash freezes those bytes; it does not prove equivalence to an original publisher-hosted copy. The original document is an external local source, not redistributed. Source/provenance metadata is in docs/coverage/csharp4-reference-policy.json. Whole-source authority and admission remain separate work.

Inspected source additions: argument-name and argument-value at main3163–3167/B8854; fixed-parameter default-argument at main6123–6135/B9047–9048; variant type parameters and in/out annotations at main7292–7304/B9112–9114. Dynamic dispatch/type meaning, embedded interop, variance safety, default-value conversion/evaluation, named parameter lookup/overload resolution and compiler/runtime effects remain semantic exclusions.

## Required Unicode choice

The document still references Unicode3.0 at main1271 and reference9196. Reviewed CSharp2/3 use frozen Unicode4 scalar data; CSharp3's continuity was explicitly approved. Literal Unicode3 at CSharp4 would reject names admitted by those profiles, including U+0220 and U+10400. Existing partial CSharp4 currently uses host-current BMP policy: a read-only probe accepts U+0220 but rejects raw/escaped U+10400 with CSN2020, while CSharp3 accepts both. Host-current BMP is not a frozen historical contract.

Recommendation: carry frozen Unicode4 scalar continuity into CSharp4 and treat this repeated Unicode3 citation as informational, exactly as the approved CSharp3 contract. Alternative: literal frozen Unicode3 BMP, accepting the backward identifier contraction. This is a source-authority/compatibility choice, not a parser implementation convenience. The user approved the recommendation on2026-10-19. Production changes still follow the bounded proof and implementation review.

## Test-only proof design

Use the real26-module CSharp3 baseline in the oracle test compilation under a separate generated namespace. Copy only Expressions,Attributes and Generics for bounded optional seams; all other grammar inputs are the unchanged production files. Keep existing Argument.Mode/Value, FixedParameter.Attributes/Receiver/Mode/Type/Name and TypeParameter.Attributes/Name meanings, adding only absent-capable Name/Default/Variance captures. No operand wrapper, Nitrogen core change, handwritten parser or runtime arbitrary custom factory is proposed.

Independent named-argument, optional-parameter and variance modules fill these empty seams. Test valid/malformed owning compilation units, independently omit each module, retain ordinary baseline fields/absence and verify exact authored spans after supplementary trivia. Raw grammar acceptance of source-local ordering/ref-default/variance-owner restrictions is recorded as a future policy obligation, not conformance. Attribute named arguments remain equals-based and separate from colon invocation names. Alias qualification and conditional-expression colons must retain their baseline meaning.

The existing partial CSharp4 parser rejects named arguments, optional parameters and interface/delegate variance, while accepting a type spelled dynamic as an ordinary name. The committed read-only baseline observations distinguish these syntax gaps from dynamic binding semantics. Production implementation, feature inventory promotion and whole-source admission follow only after the source contract and bounded proof are settled.

Named index access has a separate bounded seam: main3652/B8863 and main3693/B8865 replace expression lists with argument lists; main3653 forbids ref/out. The proof adds NamedElementExpression and NamedBaseIndexExpression alternatives requiring at least one named argument. Their Arguments node has positional Leading entries with separate Value expressions and Comma tokens, followed by named entries. Original ElementExpression.Indices and BaseIndexExpression.Indices keep their expression-list types and owning nodes for positional-only syntax. No existing property type is changed. Named-index applicability to an array/pointer target remains binding-dependent where the target's type is unknown.
