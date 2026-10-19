# CSharp4 source and typed seam proof

Base: merged PR34 eb187de5199d9908ca45b925cd5d47b2c624f86b, tree3086e67e7821c3c7d11508da97e08ff0a667807f, matching tested featured175aea. CSharp1/2/3 are admitted; CSharp4 is still partial. This slice changes only test grammars, test registration, source/provenance metadata and investigation documents.

## Working source and decision

The working DOC identifies Version4.0 and Microsoft1999–2010 authorship notice; it is delivered by the CSharp in Depth mirror. SHA256 cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7,2501632 bytes. It has not been compared with publisher-hosted bytes. The external original is not redistributed. The metadata pins source/extracted-text hashes and accurately identifies mirrored delivery. Inspection covers bounded argument/index/default/variance clauses, not a whole-source occurrence review.

The document repeats a Unicode3.0 reference. The user explicitly approved frozen Unicode4 scalar continuity for CSharp4 on2026-10-19, preserving identifiers from admitted CSharp2/3. Current named CSharp4 still uses its prior host-current BMP policy until production continuation. Read-only preflight over9 programs/18 profile observations records raw/escaped U+10400 accepted by CSharp3 and rejected by current CSharp4, while U+0220 is accepted by both. Named/default/variance forms are not yet implemented; dynamic remains valid identifier/type-name spelling without claiming dispatch semantics. The observation artifact is a historical baseline, not a future expected-output oracle.

## Executable seams

The oracle test generator reuses all26 real CSharp3 inputs under CSNitrogen.Oracle.Tests.CSharp4Probes. Only Expressions,Attributes and Generics have bounded test copies adding empty ArgumentName,ParameterDefault and VarianceAnnotation slots. The other23 inputs are unchanged production files. The input manifest records baseline/probe hashes. These are generated test factories, not arbitrary runtime custom factories.

Independent named-argument/default/variance modules fill those slots. Existing Argument.Mode/Value, FixedParameter.Attributes/Receiver/Mode/Type/Name and TypeParameter.Attributes/Name fields retain their meanings in absent/present probe observations. Named index access adds new NamedElementExpression/NamedBaseIndexExpression alternatives requiring at least one named argument. Original positional ElementExpression.Indices and BaseIndexExpression.Indices keep their expression-list types and owners. NamedIndexArguments has Leading entries with separate Value expressions/Comma tokens and then named entries. No production API or grammar is changed by this proof.

All17 initial positive cases failed before module extensions, then passed. Expanded tests include20 valid owning compilation units,14 malformed neighbors, independently omitted features, ordinary absent fields, original positional index owners, alias/conditional colon distinctions, exact typed children/ranges after supplementary trivia, and depth40 named calls. Twenty independent Roslyn5.0.0 syntax observations under LanguageVersion.CSharp4 compare named identifier, default expression and variance child spans. These are syntax-tree observations, not compilation or historical compiler equivalence.

The first test compilation used xUnit collection assertions on Nitrogen SyntaxList; those assertions were corrected to its Count API. Initial typed-view text assertions used object ToString; authored source is now read from node spans. The expanded first pass retained two findings: grouped positional-prefix capture included its comma, now represented explicitly as PositionalIndexArgument.Value/Comma; bare A::T is not a valid expression, so A::T.M supplies the alias ambiguity neighbor and bare A::T remains an explicit rejection. No failed source observation was silently removed.

Raw grammar acceptance of six source-invalid cases is deliberately retained as future policy work: positional after named arguments, required after optional parameters, ref/out defaults, and variance on class/method type parameters. Named-index ordering/ref-out restrictions are already expressed by the new named-index grammar. Array/pointer target denotation, named parameter lookup, default conversions/values, inference, variance safety, interop, dynamic dispatch and runtime behavior remain semantic exclusions.

## Verification

```
dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
```

**3,261 normal +3,681 oracle =6,942 passed**, zero failures/skips, including all76 new seam/reference cases. All26 selected production grammar files match the merged base byte-for-byte. All123 dependency pins match. Fresh macOS textutil extraction reproduces the pinned working-DOC text hash; no portable extraction claim. Unrelated benchmarks are preserved.

This proves bounded syntax composition and typed seam feasibility. CSharp4 production implementation, source-local checks, feature mappings, approved Unicode policy application and whole-source admission remain separate continuation stages. CSharp1/2/3 gates are unchanged.

Fresh whole-branch review found no production defect. Two validation gaps were regraded as necessary evidence for the declared review focus: eight direct array/base alias/conditional index witnesses now check original and named owners plus exact expression spans, and each omission checks both surviving capabilities. These are test-only evidence additions; no RED behavior failure is claimed. The final full run above includes them. Production policy/Unicode/mapping/admission, binding semantics, publisher-byte authenticity and runtime outcomes were outside this bounded review; those limits stand as described above.
