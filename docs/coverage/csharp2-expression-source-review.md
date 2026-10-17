# CSharp2 names/types/expressions normative source review

Read-only review against ECMA-334 fourth edition, SHA256 de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652. No repository changes and no builds/tests. Findings are source/static-analysis results, not executable failure claims. Snapshot candidate index had 397 pending headings. All dispositions below remain pending until root executes CSharp2-context witnesses and resolves findings.

## Source bounds and normalization

* A2.1: A:472:39:namespace-name, A:472:41:type-name, A:472:43:namespace-or-type-name. Compilation-unit A:472:36 belongs to compilation domain; its embedded names use these owners. Normative §10.8, §16.3 alias qualification, §25.5.
* A2.2: A:473:3:type through A:474:20:delegate-type inclusive: value-type, struct-type, simple-type, numeric-type, integral-type, floating-point-type, enum-type, nullable-type, non-nullable-value-type, reference-type, class-type, interface-type, array-type, non-array-type, rank-specifiers, rank-specifier, dim-separators, delegate-type. Normative §11.1, §11.2, §11.4, §19, §25.1.1, §27.2.
* A2.3: A:474:24:variable-reference; normalized to Expression. Classifying expressions as variables, writable storage and definite assignment is excluded binding/value analysis, not a parser rejection.
* A2.4: A:474:28:argument-list through A:478:9:boolean-expression inclusive. This includes every A475–477 row and the first three A478 rows. Normative §14, §9.2.3, §11.4, §25.5.1, §25.6.3.
* Additive generic type-list rows A:492:10:type-argument-list, A:492:12:type-arguments, A:492:15:type-argument; §25.5.1. Generic declaration parameters/constraints owned by declarations domain.
* Unsafe expression rows A:494:19:pointer-indirection-expression, A:494:21:pointer-member-access, A:494:23:pointer-element-access, A:494:25:addressof-expression, A:494:27:sizeof-expression; normative §27.4–27.5. Pointer conversion validity and addresses are excluded. Fixed/stackalloc syntax belongs adjacent domain; expression recognition/context policies are noted below.

**Candidate extraction gaps:** the 397-heading index omits `predefined-type: one of` on PDF475 and `assignment-operator: one of` on PDF478. Both are source grammar productions, even if inlined in BuiltinType/AssignmentOperator. Supply stable source identities and account for finite alternatives. There is no clause28 in this edition: nullable source is §11.4; OCR running header `27 28BUnsafe code` is not a clause28 heading.

### Existing normalized owners

Names/types: NamesTypes.QualifiedName, BuiltinType, ArrayRank, TypeBase, TypeReference and ReturnType; Generics.GenericQualifiedName, GenericNamePart, TypeArgumentList; AliasQualification.AliasQualifiedType; NullableValues.NullableType and NullableElement. Numeric/enum/class/interface/delegate/type-parameter subcategories collapse to name/builtin syntax because category resolution requires binding. Keep type-parameter identity as an identifier syntax category, without enforcing its scope through binding.

Arguments/lists/initializers: Expressions.Argument, Arguments, ArrayInitializer, ArrayElement, SizedArray, InitializedArray; Statements.VariableInitializer. `ref`/`out` argument writability and parameter matching excluded. Normalized owners retain original lists/separators.

Primary expressions: Expressions.Expression alternatives; GenericNameExpression, GenericBaseMemberExpression, GenericPredefinedMemberExpression, GenericMemberExpression; AliasNameExpression helper (see finding E1); GenericUnsafe.GenericPointerMemberExpression; DefaultValueExpression; AnonymousMethodExpression. Object/delegate creation share ObjectCreationExpression because identifying the delegate type requires binding. `typeof` ordinary/void uses TypeOfExpression/ReturnType; unbound dimensions use Generics.UnboundName/UnboundPart/UnboundTypeOfExpression. UnboundName requires at least one dimension; ordinary names route TypeOfExpression, matching §14.5.11 preference. No mixed bound and unbound dimensions are permitted by unbound-type-name grammar.

Unary/casts/operators: Expressions.Expression Pratt alternatives, CastHead, DefiniteCastType, AfterNamedCast; additive GenericCast/GenericArrayCast, AliasCast/AliasArrayCast, NullableCast, Unsafe.PointerCastHead. NullableValues.NullCoalescingExpression at65 lies below conditional-or70 and above conditional60, right associative. ConditionalOperand resets the right-arm Pratt floor to full expression. Assignment operator is finite spelling list; assignment target policy enforces unary-expression syntactic category only. Constant-expression and boolean-expression normalize Expression; constness/type checks excluded.

Unsafe: Unsafe.PointerIndirectionExpression/AddressExpression/PointerMemberExpression/SizeOfExtendedExpression and regular ElementExpression/arithmetic/postfix alternatives; GenericUnsafe adds pointer member type arguments, pointer casts and sizeof names. Numeric pointer conversions use existing cast grammar; no new conversion operator syntax. Unmanaged type validity, pointer-target identity, null/address/dangling behavior, conversions and numeric size result excluded.

### Required generated policies

CSharpSyntaxPolicy currently handles assignment target unary category CSN2013, primary suffix target CSN2015, direct array-creation element access CSN2005, is/as tighter-operator escapes CSN2016, statement-expression shape CSN2001, stackalloc direct-local initializer CSN2004, and adjacent identifier boundaries CSN2002. Its new primary/unary lists explicitly contain generic/alias/default/anonymous forms. Preserve these category checks when changing aliases; unknown new alternatives must not bypass them. Type and unsafe semantics remain excluded. A2.4 syntax itself does not justify writability, captured-variable legality, overload resolution, constant values or control-flow analysis.

## Findings requiring work or explicit resolution

### E1. Alias-qualified names have incorrect standalone-expression and cast category

§14.5.4 PDF191 and Annex A475 give `qualified-alias-member . identifier type-argument-list?` as a member-access alternative. A qualified-alias-member alone is not a primary expression/simple-name production. Current AliasQualification.Expression adds standalone AliasNameExpression and CSharpSyntaxPolicy lists it as primary/unary without a surrounding-member restriction.

§14.6.6 PDF219 first cast test says a parenthesized sequence which is correct type grammar but not expression grammar starts a cast, independent of follower. Consequently `(A::T)-x`, `(A::T)+x`, `(A::T)*p`, `(A::T)&x`, and their `A::G<int>` variants must have AliasCast heads and unary operands. Existing AliasCast requires AfterNamedCast, excluding these followers; the permissive AliasNameExpression can instead create parenthesized target+binary operator. That loses normative typed shape even if Accepted is true. Existing alias cast witness only `(A::N.T)x` does not expose this.

Suggested fix: treat AliasQualifiedType cast as definite; prevent freestanding AliasNameExpression helper from satisfying final primary-expression category unless it is the immediate prefix of ordinary/generic member access. A context policy can preserve existing helper typed API if needed. Positives `A::T.M()`, `A::G<int>.M<U>()`, `(A::T)-x`; negatives standalone `return A::T;`, `(A::T)` and direct `A::T()`; assert exact cast head/operand spans. Roslyn may accept standalone alias names as a broad parse tree then reject by semantic diagnostics, so syntax-oracle differences need classification rather than blindly following modern trees.

### E2. Nullable/conditional ambiguity has no CSharp2 owning witness

Run `class C { object M(object x) { return x is int ? a : b; } }` and `... return x as T ? a : b; ...` and assert ConditionalExpression with nonnullable type operand; compare nullable RHS forms `x is int?`, `x as int?`, `(x as int?) ?? y`, nullable arrays and generic/alias nullable names. NullableTypeName is inserted in TypeBase and may greedily swallow the ternary question mark; no current test covers this seam. This is a high-risk static concern, not a confirmed failure. Historical §14.9.10 PDF233–234 explicitly describes `is` nullable RHS: do not reject historical nullable is syntax simply because modern Roslyn diagnoses it. §11.4 rejects explicit reference keyword nullable forms and repeated/array/pointer nullable syntax; named reference identity is binding and remains excluded.

### E3. §9.2.3 finite follower table and historical/modern differences need explicit evidence

GenericFollower statically matches exactly `( ) ] : ; , . ? == !=` and excludes `??` with the negative predicate on `?`. Need positive selection witnesses for every follower in a syntactically owning context, plus rejection/fallback for `+ - * & < > >> >>= ?? =` and EOF. Current tests cover invocation, nested closers and selected `+`/shift fallbacks only; tests accepting `F<G<int>(x)` do not assert its relational shape. `F<T> ?? x` is not a retained generic-name under the historical table; current Roslyn may differ and must be classified. Type grammar `x is C<T> + z` does retain type args; should parse its type then get a syntactic tighter-operator issue per existing CSN2016. Pointer member type arguments use the same table.

### E4. Missing context-specific negative/span observations

Existing tests cover CSharp2 generics argument/parameter/separator/delimiter spans thoroughly, nullable/anonymous/default whole-node spans, right coalescing association and anonymous modes. They do not cover all new expression leaf spans and error anchors, nested unbound generic dimension commas/angles, nullable token/bracket order, alias cast category above, CSharp2 inherited baseline expression witnesses or complete finite generic follower table. Add one parameterized CSharp2 source/fragment matrix spanning inherited operators, array ranks/initializers, calls/ref/out/base/this/typeof/checked, with malformed full consumption and exact original spans after comments and preprocessing. CSharp1 test/source-crosswalk success is owner evidence only; rerun in CSharp2 context with required policies.

## Existing evidence and limits

* CSharp2GenericsTests: generic declaration/type/method/base/predefined/member names, qualified generic cast and array cast, unbound `typeof(N.List<>)`, malformed args/constraints, argument spans, preceding-version and module omission. Oracle CSharp2GenericsParityTests has list item/separator/delimiter and constraint spans.
* CSharp2ExpressionStatementTests: nullable arrays/casts; anonymous omitted/empty/ref/out signatures; typed default; malformed `int??`, `int*?`, `int[]?`, `string?`; malformed coalescing and anonymous attrs/params; version gating. CSharp2TypedViewsTests checks coalescing right association and modes/body. CSharp2AddedSyntaxParityTests checks whole nullable/coalescing/anonymous/default node spans.
* CSharp2UnsafeAdditionsTests: generic/alias pointer cast, generic pointer access and sizeof; module omission; malformed generic pointer member. Existing CSharp1 UnsafeConformanceTests/CastConformanceTests/TypeConformanceTests and oracle equivalents provide inherited owner candidates, not CSharp2-context proof.
* CSharp2CompositionTests: 8/32/64 nested type arguments and missing closer; module omission CSharp.Generics/NullableValues/AnonymousMethods/DefaultValues/AliasQualification/GenericUnsafe.
* No tests were executed in this read-only review; no pending source row should be promoted based solely on this document.

## Excluded normative obligations

§11.4 underlying-type identity, HasValue/Value/storage/default constructor/conversion semantics; §14 expression classifications requiring binding, overload resolution and execution; §25.5 constraints/substitution/accessibility/open-vs-closed runtime behavior; §25.6 type inference and delegate compatibility; §27.4 numeric/pointer conversion semantics and §27.5 pointer identity/addresses/results. Syntactic type lists, token grammar, ambiguity choice, original spans and the primary/unary categories remain in scope. Repeating baseline `static` or accepting a method-group spelling does not prove CSharp2 conversion semantics.
