# CSharp3 unsafe source disposition

Primary: Microsoft C# Language Specification Version3.0 DOC, SHA256 `ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1`. Scope: all39 unsafe Appendix B occurrences B8835–8873 and controlling §18. This report records normalized syntax/policy inspection and finite selected-factory evidence. All overall rows remain candidate/pending and CSharp3 remains partial.

## Modifier and statement extensions: B8835–8849

Unsafe.UnsafeModifier extends the shared declaration modifier vocabulary; declaration/finalizer/static-constructor owners preserve the printed unsafe alternatives. The reviewed inherited contract explicitly excludes general modifier compatibility and per-production finite modifier-set restrictions, and does not claim a distinct generated modifier-list node for each source family. UnsafeBlock requires a block and extends EmbeddedStatement, excluding a bare expression/declaration body. Unsafe context legality, type/address safety and handler legality remain deferred semantic boundaries. Iterator blocks retain their explicit separate prohibition on an unsafe block.

## Pointer types and expressions: B8850–8859

Unsafe extends TypeBase with pointer and void-pointer forms; GenericUnsafe supplies generic/alias pointer element names and sizeof alternatives. PointerCastType preserves star sequences and optional array ranks. Managed/unmanaged type identity, pointer value/target validity, indirection/addressability, readonly/movable storage, conversions, comparisons and arithmetic are excluded. Pointer element access shares ElementExpression with array access because the target's identity requires binding; its type-specific rank legality is semantic. Pointer members preserve the previously approved generic suffix correction and the historical generic-follower table. Primary/unary category and direct-array-creation suffix restrictions remain source syntax checks.

SizeOfExtendedType and ordinary builtin sizeof forms preserve the type operand. Source inspection exposed an ordered-choice defect: AdditionalSizeofType accepted a plain generic/alias prefix before a star, preventing the later PointerCastType branch from running. Putting PointerCastType first corrects sizeof(G<int>*) and sizeof(A::N.G<int>*) without changing node/property contracts. Both second/third profiles have raw tree/range, malformed-neighbor and pinned Roslyn syntax observations; plain non-pointer generic/alias node owners remain unchanged. sizeof result, managed-type validity and compile-time values remain excluded.

## Fixed statements and buffers: B8860–8871

FixedStatement requires a pointer type, one or more initialized comma declarators and an embedded body. The initializer normalizes address-of variable-reference or expression spelling; addressability/movability, readonly pin variables, capture/pinning/runtime behavior and unsafe-context legality are excluded. Missing pointer stars, initializer/count/separator and invalid embedded-body neighbors remain syntax obligations.

CSharp3 fixed-size-buffer syntax uses buffer-element-type as type (main7998/B8869). Main8003 restricts its denotation to the listed primitive types. The reviewed profile checks struct owner, instance declaration, one explicit vector count, known primitive keyword kinds and known invalid array/nullable/pointer shapes; rankless named/alias/generic names are preserved for deferred denotation. This deliberately differs from CSharp2's narrower reviewed spelling contract. The missing comma in fixed-size-buffer-declarators is normalized using the controlling main8007 comma-separated example, retained by a selected-factory witness. Attributes/modifier/list ownership and authored counts remain represented; constant size values and field layout are semantic exclusions.

## Stack allocation: B8872–8873

StackAllocExpression is an initializer-shaped expression owner, with source policy requiring a direct local variable initializer. Field, return and parenthesized forms are rejected. Size expressions and element spelling remain syntax; unmanaged identity, negative/range/size values, lifetime, allocation behavior and inherited catch/finally/unsafe legality are excluded. No modern target-typed stack allocation forms are added.

## Finite evidence and admission boundary

CSharp3UnsafeSourceTests executes source-defined accepted/malformed pointer, sizeof, unsafe block, fixed statement/buffer and stack allocation neighbors in the actual CSharp3 factory, preserving all node/diagnostic coordinate bounds. GenericSizeofPointerConformanceTests checks both CSharp2/CSharp3 pointer operand ranges and retained non-pointer owners, with malformed neighbors. GenericSizeofPointerReferenceTests supplies independent pinned current-compiler syntax observations, without historical compiler or compilation-validity claims. CSharp3SourceDeltaTests and InheritedClosureTests retain fixed-buffer named denotation and controlling comma-example witnesses.

The crosswalk links exact39 occurrences to this report and finite witness groups; hash/source-set/method-reference mutations fail closed. Attributed-method existence alone does not prove execution. Finite groups are not per-alternative closure or arbitrary-depth proof. Whole-source admission remains pending until explicit remaining witness obligations and the separate source/factory/policy completeness gate close.
