# CSharp2 declaration / compilation / attribute / generic-header source audit

Read-only source/code audit on 2026-10-18. No build or test was run and no repository file was changed. All proposed behavior witnesses below are source/code deductions, not newly executed parser observations. Primary: ECMA-334 fourth edition June 2006, SHA256 de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652, `/tmp/csn-ecma334-4.pdf` and text extraction. Supplement: Microsoft VS2005 CSharp PDF SHA256 27ce1c2eac588d95f370b16741f35b1593564ddebaa9a46ad40b6c04faf8e6d3, PDF p959, read directly with pypdf. Source coordinates below are physical 1-based PDF page / extracted line. These coordinates preserve occurrences, including both variable-initializer productions and unsafe modifier extensions. No first-edition coverage label was treated as fourth-edition proof.

## Findings requiring explicit closure or classification

1. **Historical attribute argument grammar incorrectly receives a modern restriction in CSharp2.** §24.2 PDF pp398–400 and A:491:46:attribute-argument-expression specify expression. `CSharpAttributePolicy.Check` rejects direct AssignmentExpression whenever `!UnicodePolicy.IsHistorical(version)`; IsHistorical is only version 1. Thus CSharp2 `[A((x)=1)] class C {}` or `[A(P=(x=1))] class C {}` reaches modern CSN2006. Parentheses around an assignment expression can change its outer node and expose additional inconsistency. Use a direct assignment with parenthesized left operand to avoid the named-argument syntax ambiguity. Attribute constant/type validity is excluded; the syntactic expression production itself is the issue. The positional-before-named check is supported by §24.2. Existing AttributeConformanceTests exercise CSharp1/CSharp14, not a fourth-edition CSharp2 exception fixture. Modern Roslyn may reject expressions here; it cannot decide the historical production.

2. **Required unknown-attribute-target warning is absent for CSharp2; global target grammar is narrower than Annex/prose.** §24.2 PDF p400 says implementations can accept other target specifiers but must warn on unrecognized ones. A:491:7:global-attribute-target and A:491:20:attribute-target are both identifier | keyword. Current ordinary AttributeTarget accepts that family, but CSharpAttributePolicy recognition/warning is inside IsHistorical(version), so `[vendor:A] class C {}` has no required recognition warning in CSharp2. Current GlobalTargetsCurrent only recognizes assembly/module, so `[vendor:A]` alone cannot be represented as an otherwise valid global attribute; with a following class it becomes a type attribute instead. This is a global-vs-type classification gap; namespace/class follows can be ambiguous, so use `[vendor:A]` alone for an unambiguous global witness. Standard global target is assembly; module is permitted as implementation-defined extension, not standardized by this edition. Standard ordinary targets include **typevar**, added since first edition: `[typevar:A] T` is legitimate on a generic parameter. Do not copy the CSharp1 warning list without adding typevar. Attribute target applicability to symbol kinds is excluded here, but target recognition and required warnings are source-local.

3. **Accessor cardinality policy misses the override exception and source-local restrictions.** §17.6.2 PDF p327, A:485:18–32, inherited by indexers §17.8. CSharpDeclarationPolicy rejects one accessibility-marked accessor unless there are two accessors, regardless of override. Normative prose limits the two-accessor requirement to declarations with no override. Candidate accepted-source witness: `class B { public virtual int P { get { return 0; } protected set {} } } class C:B { public override int P { protected set {} } }`. Matching base accessors is semantic and must be excluded from parser policy; source syntax should not reject the override form outright. Conversely explicit interface accessors cannot have accessor-modifiers (`class C { int I.P { get { return 0; } private set {} } }`); current policy only checks counts. Declared accessibility ordering is locally knowable: `class C { private int P { get { return 0; } private set {} } }`, `class C { internal int P { get { return 0; } protected set {} } }`. Current grammar accepts these and policy does not check ordering. Interface grammar correctly has no accessor modifier slot. These restrictions can be recorded as source-local compile-time context checks, distinct from name binding/override lookup. Existing CSharp2DeclarationAliasTests test modifier spelling, two marked accessors and public-accessor rejection; no override exception or accessibility-order fixtures.

4. **Shared Modifier* is not a per-production grammar implementation.** All type declarations and every MemberDeclaration share StandardModifier, plus unsafe and partial extensions. The Annex uses distinct modifier sets for class/struct/interface/enum/delegate, fields/constants/methods/properties/events/indexers/operators/constructors/finalizers/static constructors. Examples showing a structural-set mismatch: `readonly class C {}`, `static struct S {}`, `unsafe enum E { A }`, `class C { virtual int f; }`, `class C { static int this[int i] { get { return 0; } } }`, `class C { public ~C(){} }`. §17.8 explicitly forbids static indexers. Unsafe A.3 adds unsafe to class/struct/interface/delegate and specified members, **not enum or constant modifiers**. Partial owner/order validation exists and correctly rejects partial enum/delegate/member, repeated partial, and ordinary modifiers after partial. Existing first-edition evidence deliberately classifies modifier compatibility as semantic and retains parser acceptance. This audit does not silently reverse that contract: split grammar-set membership (finite tokens, no binding) from compatibility/duplicate/accessibility rules, and explicitly classify any retained over-approximation before claiming Annex closure. Static class source-local restrictions §17.1.1.3 PDF p287 (no abstract/sealed, base list, operators, protected members or instance constructors/members) likewise have no dedicated implementation except enabling static since v2; classify context scope separately from static-type usage resolution.

5. **Indexer ref/out exclusion is absent.** §17.8 PDF p337 says at least one parameter and forbids ref/out. IndexerParameterList enforces nonempty and params-last but reuses FixedParameter with ref/out. Candidate `class C { int this[ref int i] { get { return 0; } } }` and interface equivalent are syntax-only/context-local omissions without binding. The parameter array's required single-dimensional type (§17.5.1.4) is not enforced (`params int[,] a`); this is type-shape legality, and existing evidence explicitly places params/indexer type legality outside scope. Record that boundary rather than promote it as implemented.

6. **Constraint production is intentionally over-approximated and partial ordering checks do not close it.** §25.7 PDF pp427–429 and A:492:17–41. Constraint permits class/struct/new()/any TypeReference in an unrestricted comma list. Policy checks class/struct first and new() last, and requires an owning type-parameter-list. It does not reject `class C<T> where T:struct,new(){}`, repeated `where T`, or invalid primary/secondary type shapes. Two constructor constraints are actually caught because the first new() is not last, so do NOT report that as missed. Duplicate where and struct/new prohibition are source-local compile-time constraints, but declaration-space resolution, duplicate names after normalization, class/interface/type-parameter classification, sealed/object/Array/Delegate/Enum/ValueType constraint validity, cycle detection, and matching the named type parameter are semantic exclusions. Pure grammar-shape mismatch witness: `class C<T> where T:int[] {}` or `where T:int?`; do not require type binding to see these do not match class-type/interface-type/type-parameter productions. Existing CSharp2GenericsTests cover new-before-class, class/struct after another item, constraint without type parameters, malformed headers and variance; no struct,new or duplicate-clause fixtures. Type argument and generic expression syntax is owned by another auditor.

7. **Attribute names do not implement full type-name syntax.** §24.2 / A:491:29 uses type-name, including namespace alias qualifiers in fourth edition §16.7 and constructed type-name syntax. Attributes.Attribute uses only QualifiedName (identifier-dot sequence). `[global::N.A] class C {}` and `[A::N.Attr] class C {}` are legal syntactic alias-qualified attribute names but absent from the grammar. Generic attribute usage such as `[A<int>]` also has syntactic type-name shape; generic attribute class legality is a semantic exclusion (§24.1 prohibits generic attribute classes), so distinguish this negative semantic example from positive alias-qualified attributes. CSharp2 alias fixtures cover using/type/cast/typeof/expression seams, not attribute names.

8. **Fixed buffer supplement has explicit limits beyond owner/static checks.** Microsoft PDF p959 expressly says only instance fields of structs in unsafe context, vectors only with count, and enumerates bool/byte/char/short/int/long/sbyte/ushort/uint/ulong/float/double element types. FixedBuffers.ngr enforces one bracketed expression per declarator, requires count, supports lists and attributes, and CSharpDeclarationPolicy enforces struct owner and not static. It accepts arbitrary TypeReference and does not check unsafe context: `struct S { fixed int x[5]; }` is a missing source-local unsafe context check (unless unsafe context is explicitly excluded globally); `unsafe struct S { fixed decimal x[5]; }` is an explicit supplement element-kind boundary, not a binding-dependent unknown type case. Existing CSharp2UnsafeAdditionsTests cover struct owner, static, omitted count, positive public int list; no unsafe-context or finite element-set witnesses. Do not infer fixed-size-buffer syntax from ECMA A.3: no fixed-buffer declaration production appears there. Supplement is essential.

## Positive ownership and existing witnesses

- Compilation-unit/namespace order §16.1–16.4, A:472:36 and A:481:18–43: CompilationUnits.ngr + AliasQualification.ngr implement externs before usings, globals before namespace/type members, namespace-scoped externs, optional namespace trailing semicolon. CSharpCompilationUnitPolicy rejects assembly/module targets attached to namespace-level types after declarations. CSharp2DeclarationAliasTests contains root and namespace externs, using-before-extern and trailing extern negatives, malformed `extern alias;`, and malformed `A:::T`.
- Classes/structs/interfaces/delegates generic headers §17.1/18.1/20.1/22.1 and §25.1–25.4: Declarations.ngr + Generics.ngr + DeclarationForms.ngr; type parameter attributes, nonempty generic parameter list, optional constraints after base/parameter list. CSharp2GenericsTests contains class/struct/interface/delegate/method examples and physical spans. Generic type/member name disambiguation owned by other auditor.
- Accessors/property/indexer/event shapes: Declarations.ngr + CSharpDeclarationPolicy provide no duplicate get/set, both add/remove, no event duplicate accessor, accessor modifiers on at most one accessor, interface semicolon accessors. CSharp1 declaration suites are inherited grammar evidence but explicitly parse CSharp1; they are not fourth-edition CSharp2 regression execution.
- Struct body reuses ClassMember and policy rejects destructor, explicit parameterless constructor (later feature), instance field initializer (later feature), parameterized/static initializer constructor forms. Constructor-name resolution, operator arity/types, enum underlying type validity and general modifier compatibility are explicitly excluded in existing docs/coverage/csharp1-declaration-conformance-evidence.md. Enum integral base mismatch (BuiltinType accepts bool/float/string/object) is consequently a known over-approximation to classify, not new proof of a parser regression.
- Arrays A.2.8 map to NamesTypes.TypeReference/ArrayRank, Expressions.ArrayInitializer/ArrayElement and Statements.VariableInitializer; generic parameter as non-array element is structurally covered by identifier name syntax. Both variable-initializer source occurrences must remain separate IDs. Ragged/rank/type/value compatibility and initializer size requirements are semantic exclusions.
- Clause 23 exception classes and clause 24 attribute construction/retrieval/reserved attributes are semantic/runtime content without additional in-scope source grammar families. §25.1–25.4 identity/scope/base/interface unification/variance of constructed interfaces and §25.7 satisfaction/lookup/conversions remain excluded.

## Source inventory bounds and owners

The following occurrence inventory is generated from the pinned PDF extraction and cross-checked against the supplied candidate index. Each bound starts at its heading and ends before the next grammar heading (including `: one of` headings absent from the candidate extractor), Annex section title, or page boundary when a production does not continue. Page headers/footers inside extracted ranges are not grammar content. Full primary clauses and alternatives were read in `/tmp/csn-ecma334-4.txt`; inventory rows are references, not independent evidence of passing behavior. Owner family D=Declarations/DeclarationForms/CSharpDeclarationPolicy; C=CompilationUnits/AliasQualification/CSharpCompilationUnitPolicy; A=Attributes/GlobalTargetsCurrent/CSharpAttributePolicy; G=Generics/CSharpDeclarationPolicy; R=NamesTypes/Expressions/Statements; U=Unsafe modifier extension. Table status is reviewed-source pending implementation closure/classification for findings above, never blanket complete.

| Source ID | Heading/alternative bound | Owner |
|---|---|---|
| A:472:36:compilation-unit | 472:36–472:38 | C |
| A:481:18:namespace-declaration | 481:18–481:19 | C |
| A:481:20:qualified-identifier | 481:20–481:22 | C |
| A:481:23:namespace-body | 481:23–481:24 | C |
| A:481:25:extern-alias-directives | 481:25–481:27 | C |
| A:481:28:extern-alias-directive | 481:28–481:29 | C |
| A:481:30:using-directives | 481:30–481:32 | C |
| A:481:33:using-directive | 481:33–481:35 | C |
| A:481:36:using-alias-directive | 481:36–481:37 | C |
| A:481:38:using-namespace-directive | 481:38–481:39 | C |
| A:481:40:namespace-member-declarations | 481:40–481:42 | C |
| A:481:43:namespace-member-declaration | 481:43–481:45 | C |
| A:482:2:type-declaration | 482:2–482:8 | C |
| A:482:9:qualified-alias-member | 482:9–482:12 | C |
| A:482:14:class-declaration | 482:14–482:16 | D |
| A:482:17:class-modifiers | 482:17–482:19 | D |
| A:482:20:class-modifier | 482:20–482:29 | D |
| A:482:30:class-base | 482:30–482:33 | D |
| A:482:34:interface-type-list | 482:34–482:36 | D |
| A:482:37:class-body | 482:37–482:38 | D |
| A:482:39:class-member-declarations | 482:39–482:41 | D |
| A:483:2:class-member-declaration | 483:2–483:13 | D |
| A:483:14:constant-declaration | 483:14–483:16 | D |
| A:483:17:constant-modifiers | 483:17–483:19 | D |
| A:483:20:constant-modifier | 483:20–483:25 | D |
| A:483:26:constant-declarators | 483:26–483:28 | D |
| A:483:29:constant-declarator | 483:29–483:30 | D |
| A:483:31:field-declaration | 483:31–483:32 | D |
| A:483:33:field-modifiers | 483:33–483:35 | D |
| A:483:36:field-modifier | 483:36–483:45 | D |
| A:483:46:variable-declarators | 483:46–483:48 | D |
| A:483:49:variable-declarator | 483:49–483:51 | D |
| A:484:2:variable-initializer | 484:2–484:4 | D |
| A:484:5:method-declaration | 484:5–484:6 | D |
| A:484:7:method-header | 484:7–484:9 | D |
| A:484:10:method-modifiers | 484:10–484:12 | D |
| A:484:13:method-modifier | 484:13–484:24 | D |
| A:484:25:return-type | 484:25–484:27 | D |
| A:484:28:member-name | 484:28–484:30 | D |
| A:484:31:method-body | 484:31–484:33 | D |
| A:484:34:formal-parameter-list | 484:34–484:38 | D |
| A:484:39:fixed-parameters | 484:39–484:41 | D |
| A:484:42:fixed-parameter | 484:42–484:43 | D |
| A:484:44:parameter-modifier | 484:44–484:46 | D |
| A:484:47:parameter-array | 484:47–484:48 | D |
| A:484:49:property-declaration | 484:49–484:50 | D |
| A:485:2:property-modifiers | 485:2–485:4 | D |
| A:485:5:property-modifier | 485:5–485:17 | D |
| A:485:18:accessor-declarations | 485:18–485:20 | D |
| A:485:21:get-accessor-declaration | 485:21–485:22 | D |
| A:485:23:set-accessor-declaration | 485:23–485:24 | D |
| A:485:25:accessor-modifier | 485:25–485:31 | D |
| A:485:32:accessor-body | 485:32–485:34 | D |
| A:485:35:event-declaration | 485:35–485:38 | D |
| A:485:39:event-modifiers | 485:39–485:41 | D |
| A:485:42:event-modifier | 485:42–485:54 | D |
| A:486:2:event-accessor-declarations | 486:2–486:4 | D |
| A:486:5:add-accessor-declaration | 486:5–486:6 | D |
| A:486:7:remove-accessor-declaration | 486:7–486:8 | D |
| A:486:9:indexer-declaration | 486:9–486:10 | D |
| A:486:11:indexer-modifiers | 486:11–486:13 | D |
| A:486:14:indexer-modifier | 486:14–486:24 | D |
| A:486:25:indexer-declarator | 486:25–486:27 | D |
| A:486:28:operator-declaration | 486:28–486:29 | D |
| A:486:30:operator-modifiers | 486:30–486:32 | D |
| A:486:33:operator-modifier | 486:33–486:36 | D |
| A:486:37:operator-declarator | 486:37–486:40 | D |
| A:486:41:unary-operator-declarator | 486:41–486:42 | D |
| A:486:45:binary-operator-declarator | 486:45–486:46 | D |
| A:487:7:conversion-operator-declarator | 487:7–487:9 | D |
| A:487:10:operator-body | 487:10–487:12 | D |
| A:487:13:constructor-declaration | 487:13–487:14 | D |
| A:487:15:constructor-modifiers | 487:15–487:17 | D |
| A:487:18:constructor-modifier | 487:18–487:24 | D |
| A:487:25:constructor-declarator | 487:25–487:26 | D |
| A:487:27:constructor-initializer | 487:27–487:29 | D |
| A:487:30:constructor-body | 487:30–487:32 | D |
| A:487:33:static-constructor-declaration | 487:33–487:34 | D |
| A:487:35:static-constructor-modifiers | 487:35–487:37 | D |
| A:487:38:static-constructor-body | 487:38–487:40 | D |
| A:487:41:finalizer-declaration | 487:41–487:42 | D |
| A:487:43:finalizer-body | 487:43–487:45 | D |
| A:487:47:struct-declaration | 487:47–487:49 | D |
| A:488:2:struct-modifiers | 488:2–488:4 | D |
| A:488:5:struct-modifier | 488:5–488:11 | D |
| A:488:12:struct-interfaces | 488:12–488:13 | D |
| A:488:14:struct-body | 488:14–488:15 | D |
| A:488:16:struct-member-declarations | 488:16–488:18 | D |
| A:488:19:struct-member-declaration | 488:19–488:30 | D |
| A:488:32:array-type | 488:32–488:33 | R |
| A:488:34:non-array-type | 488:34–488:39 | R |
| A:488:40:rank-specifiers | 488:40–488:42 | R |
| A:488:43:rank-specifier | 488:43–488:44 | R |
| A:488:45:dim-separators | 488:45–488:47 | R |
| A:488:48:array-initializer | 488:48–488:50 | R |
| A:489:2:variable-initializer-list | 489:2–489:4 | R |
| A:489:5:variable-initializer | 489:5–489:8 | R |
| A:489:10:interface-declaration | 489:10–489:13 | D |
| A:489:14:interface-modifiers | 489:14–489:16 | D |
| A:489:17:interface-modifier | 489:17–489:23 | D |
| A:489:24:interface-base | 489:24–489:25 | D |
| A:489:26:interface-body | 489:26–489:27 | D |
| A:489:28:interface-member-declarations | 489:28–489:30 | D |
| A:489:31:interface-member-declaration | 489:31–489:35 | D |
| A:489:36:interface-method-declaration | 489:36–489:39 | D |
| A:489:40:interface-property-declaration | 489:40–489:41 | D |
| A:489:42:interface-accessors | 489:42–489:46 | D |
| A:489:47:interface-event-declaration | 489:47–489:48 | D |
| A:489:49:interface-indexer-declaration | 489:49–489:51 | D |
| A:490:3:enum-declaration | 490:3–490:4 | D |
| A:490:5:enum-base | 490:5–490:6 | D |
| A:490:7:enum-body | 490:7–490:9 | D |
| A:490:10:enum-modifiers | 490:10–490:12 | D |
| A:490:13:enum-modifier | 490:13–490:18 | D |
| A:490:19:enum-member-declarations | 490:19–490:21 | D |
| A:490:22:enum-member-declaration | 490:22–490:25 | D |
| A:490:27:delegate-declaration | 490:27–490:29 | D |
| A:490:30:delegate-modifiers | 490:30–490:32 | D |
| A:490:33:delegate-modifier | 490:33–490:40 | D |
| A:490:42:global-attributes | 490:42–490:43 | A |
| A:490:44:global-attribute-sections | 490:44–490:46 | A |
| A:491:2:global-attribute-section | 491:2–491:4 | A |
| A:491:5:global-attribute-target-specifier | 491:5–491:6 | A |
| A:491:7:global-attribute-target | 491:7–491:9 | A |
| A:491:10:attributes | 491:10–491:11 | A |
| A:491:12:attribute-sections | 491:12–491:14 | A |
| A:491:15:attribute-section | 491:15–491:17 | A |
| A:491:18:attribute-target-specifier | 491:18–491:19 | A |
| A:491:20:attribute-target | 491:20–491:22 | A |
| A:491:23:attribute-list | 491:23–491:26 | A |
| A:491:27:attribute | 491:27–491:28 | A |
| A:491:29:attribute-name | 491:29–491:30 | A |
| A:491:31:attribute-arguments | 491:31–491:34 | A |
| A:491:35:positional-argument-list | 491:35–491:37 | A |
| A:491:38:positional-argument | 491:38–491:39 | A |
| A:491:40:named-argument-list | 491:40–491:43 | A |
| A:491:44:named-argument | 491:44–491:45 | A |
| A:491:46:attribute-argument-expression | 491:46–491:48 | A |
| A:492:3:type-parameter-list | 492:3–492:4 | G |
| A:492:5:type-parameters | 492:5–492:7 | G |
| A:492:8:type-parameter | 492:8–492:9 | G |
| A:492:17:type-parameter-constraints-clauses | 492:17–492:19 | G |
| A:492:20:type-parameter-constraints-clause | 492:20–492:21 | G |
| A:492:22:type-parameter-constraints | 492:22–492:29 | G |
| A:492:30:primary-constraint | 492:30–492:33 | G |
| A:492:34:secondary-constraints | 492:34–492:39 | G |
| A:492:40:constructor-constraint | 492:40–492:42 | G |
| A:492:44:class-modifier | 492:44–492:46 | U |
| A:492:47:struct-modifier | 492:47–492:49 | U |
| A:493:2:interface-modifier | 493:2–493:4 | U |
| A:493:5:delegate-modifier | 493:5–493:7 | U |
| A:493:8:field-modifier | 493:8–493:10 | U |
| A:493:11:method-modifier | 493:11–493:13 | U |
| A:493:14:property-modifier | 493:14–493:16 | U |
| A:493:17:event-modifier | 493:17–493:19 | U |
| A:493:20:indexer-modifier | 493:20–493:22 | U |
| A:493:23:operator-modifier | 493:23–493:25 | U |
| A:493:26:constructor-modifier | 493:26–493:28 | U |
| A:493:29:finalizer-declaration | 493:29–493:31 | U |
| A:493:32:static-constructor-modifiers | 493:32–493:38 | U |

Candidate extractor omissions (one-of headings; add independent source records rather than pretending existing IDs cover them):

- `A:486:43:overloadable-unary-operator` — overloadable-unary-operator: one of; D owner, §17.9.
- `A:487:2:overloadable-binary-operator` — overloadable-binary-operator: one of; D owner, §17.9.

Supplement source record proposal: `MSVS2005:959:Fixed-Size-Buffers` (page-bounded prose and examples), owner FixedBuffers.ngr/CSharpDeclarationPolicy, finite primitive element set and unsafe struct instance/vector/count restrictions; present witnesses CSharp2UnsafeAdditionsTests.
