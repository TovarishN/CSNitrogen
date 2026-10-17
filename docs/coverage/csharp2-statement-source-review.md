# CSharp2 statement source audit

Read-only review of ECMA-334 fourth edition (June 2006), local `/tmp/csn-ecma334-4.pdf`; SHA-256 independently read: `de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652`. Extracted text `/tmp/csn-ecma334-4.txt`. PDF pages below are one-based physical pages, matching the index. Annex A is informative; normative clause 15, iterator clause 26.1, and clauses 27.6–27.7 govern. No repository modifications, build, tests, oracle executions, or parser executions performed. Witness outcomes below are predictions from grammar/policy inspection, not reproduced failures.

## Findings

1. **Missing new iterator placement policy (§15.14, printed 249–250 / PDF271–272).** `Iterators.ngr:3–6` adds both forms to all EmbeddedStatement positions. Neither `CSharpSyntaxPolicy` nor `CSharpDeclarationPolicy` contains a yield/iterator ancestry check. The standard forbids either form outside method/operator/accessor bodies, inside anonymous methods, and inside finally. It additionally forbids yield-return anywhere in a try statement with catch clauses. Concrete anticipated false acceptances:
   - `class C { C() { yield break; } }` (constructor owner).
   - `class C { void M() { object d = delegate { yield break; }; } }` (anonymous method).
   - `class C { object M() { try {} finally { yield break; } } }` (finally).
   - `class C { object M() { try { yield return 1; } catch {} } }`.
   - `class C { object M() { try {} catch { yield return 1; } } }`.
   These witnesses isolate source containment, independent of iterator return-type binding. Use `System.Collections.IEnumerable` instead of object when constructing compiler-backed fixtures. A correct policy must preserve yield-break in try/catch, yield-return in try/finally's try block, yield as an ordinary identifier, and function-boundary resets. Row `A:481:15:yield-statement`; linked rows `A:480:37:try-statement`, `A:480:47:specific-catch-clause`, `A:480:49:general-catch-clause`, `A:481:2:finally-clause`; owner `csharp2.iterators` / `Iterators.ngr` + syntax policy. Existing CSharp2ExpressionStatementTests cover one positive method, malformed missing expression, version omission, and identifier assignment; they do not cover these source restrictions.

2. **Iterator-block declaration/content restrictions unimplemented (§26.1, printed 415–416 / PDF437–438).** No explicit iterator production is required: the standard says iterators are ordinary blocks distinguished by yield. However it prohibits ref/out parameters on an iterator member, an ordinary return in the iterator block, and an unsafe context within the iterator. Concrete anticipated false acceptances:
   - `class C { System.Collections.IEnumerable M(ref int x) { yield break; } }`.
   - `class C { System.Collections.IEnumerable M(out int x) { x=0; yield break; } }`.
   - `class C { System.Collections.IEnumerable M() { yield break; return null; } }`.
   - `class C { System.Collections.IEnumerable M() { unsafe {} yield break; } }`.
   These restrictions are determined from declaration and block syntax; return-interface validity, yield conversions, generated state machines, capture, lifetimes and execution are excluded. Classification caveat: pre-existing CSharp1 documents explicitly defer unsafe legality; if that general boundary is carried into CSharp2, the unsafe bullet needs an explicit reviewed exclusion rather than an implementation task. Ordinary-return/ref-out/yield-owner restrictions are newly introduced CSharp2 obligations and cannot be inferred from first-edition fixtures. Row `A:481:15:yield-statement`, linked `A:480:33:return-statement`, unsafe `A:493:42:unsafe-statement`; owner `csharp2.iterators` / syntax/declaration policy.

3. **Inherited stackalloc handler restriction remains deferred, not newly introduced syntax loss (§27.7, printed439–440 / PDF461–462).** Normative prose forbids stackalloc initializers inside catch/finally. Current policy (`CSharpSyntaxPolicy` stackalloc block) enforces direct local-variable initializer and excludes fields, returns, parenthesized expressions etc, but does not inspect catch/finally. Witness `class C { unsafe void M() { try {} catch { int* p=stackalloc int[1]; } } }`, and corresponding finally form. Row `A:494:47:stackalloc-initializer`, linked duplicate `A:494:43:local-variable-initializer`. This is explicitly deferred in existing first-edition normative evidence (unsafe/handler legality); preserve as a reviewed exclusion unless the parent review deliberately changes scope. Do not call it closed merely because direct-local tests pass.

## Inherited boundaries, not additional syntax defects

`PendingStatementClosureTests` explicitly accepts break without a loop as a semantic/control-flow boundary. First-edition normative evidence likewise defers break/continue/goto target and finally transfers, return-member/finally restrictions, throw rethrow containment, using initialization, catch class-type denotation, constant/boolean conversion, and unsafe legality. Do not reclassify these as new CSharp2 syntax gaps without a scope decision. Label duplicate/target resolution, switch duplicate case values, governing types, exhaustiveness/reachability/definite assignment and runtime exception/disposal/iterator behavior are excluded here.

`Unsafe.ngr` fixed statement has required pointer spelling, at least one initialized declarator, comma separators and EmbeddedStatement body; `GenericUnsafe.ngr` extends the pointer element names for generic/alias types. Fixed initializer normalization to Expression covers address-of syntax; addressability/movability/capture/readonly/pinning remain excluded. No concrete additional fixed statement spelling gap was established by this review. Fixed buffers are a separate Microsoft supplement, not inferred from §27.6.

## Statement row source-to-owner correspondence

The following **51** A.2.5 occurrences are independently correlated against this edition's Annex A (PDF478 lines13ff through PDF481 line17), normative clause boundaries and current grammar. This is a read-only source correspondence, not a declaration that the four coverage dimensions pass. Most test evidence remains explicitly first-edition or CSharp1-profile evidence; CSharp2 must retain pending status until its own selected-profile witnesses/shape/span/diagnostic review exists.

`S` = `src/CSNitrogen.Syntax/Grammars/Statements.ngr`; `P` = `src/CSNitrogen/Parsing/CSharpSyntaxPolicy.cs`; `I` = `src/CSNitrogen.Syntax/Grammars/Iterators.ngr`. Lists/wrappers normalized into fields are described rather than claimed as missing productions.

| Exact row ID | Normative clause; PDF bounds | Grammar/policy owner |
|---|---|---|
| `A:478:13:statement` | 15; 247 | S StandardStatement / EmbeddedStatement |
| `A:478:17:embedded-statement` | 15; 247 | S StandardStatement / EmbeddedStatement |
| `A:478:30:block` | 15.2/15.2.1; 249 | S Block.Statements Statement* |
| `A:478:32:statement-list` | 15.2/15.2.1; 249 | S Block.Statements Statement* |
| `A:478:35:empty-statement` | 15.3; 249–250 | S StandardEmbeddedStatement semicolon |
| `A:478:37:labeled-statement` | 15.4; 250 | S LabeledStatement |
| `A:478:39:declaration-statement` | 15.5; 250–251 | S StandardStatement declaration choices |
| `A:478:42:local-variable-declaration` | 15.5.1; 251 | S VariableDeclaration / VariableDeclarator / VariableInitializer |
| `A:478:44:local-variable-declarators` | 15.5.1; 251 | S VariableDeclaration / VariableDeclarator / VariableInitializer |
| `A:478:47:local-variable-declarator` | 15.5.1; 251 | S VariableDeclaration / VariableDeclarator / VariableInitializer |
| `A:479:2:local-variable-initializer` | 15.5.1; 251 | S VariableDeclaration / VariableDeclarator / VariableInitializer |
| `A:479:5:local-constant-declaration` | 15.5.2; 251–252 | S ConstantDeclaration / ConstantDeclarator |
| `A:479:7:constant-declarators` | 15.5.2; 251–252 | S ConstantDeclaration / ConstantDeclarator |
| `A:479:10:constant-declarator` | 15.5.2; 251–252 | S ConstantDeclaration / ConstantDeclarator |
| `A:479:12:expression-statement` | 15.6; 252 | S ExpressionStatement / StatementExpression; P CSN2001 |
| `A:479:14:statement-expression` | 15.6; 252 | S ExpressionStatement / StatementExpression; P CSN2001 |
| `A:479:22:selection-statement` | 15.7; 252 | S StandardEmbeddedStatement selection choices |
| `A:479:25:if-statement` | 15.7.1; 252–253 | S IfStatement |
| `A:479:28:switch-statement` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:30:switch-block` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:32:switch-sections` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:35:switch-section` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:37:switch-labels` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:40:switch-label` | 15.7.2; 253–256 | S SwitchStatement / SwitchSection / SwitchLabel; P CSN2006 |
| `A:479:43:iteration-statement` | 15.8; 256 | S StandardEmbeddedStatement loop choices |
| `A:479:48:while-statement` | 15.8.1; 256–257 | S WhileStatement |
| `A:480:2:do-statement` | 15.8.2; 257 | S DoStatement |
| `A:480:4:for-statement` | 15.8.3; 257–258 | S ForStatement / StatementExpressionList; P CSN2001 |
| `A:480:6:for-initializer` | 15.8.3; 257–258 | S ForStatement / StatementExpressionList; P CSN2001 |
| `A:480:9:for-condition` | 15.8.3; 257–258 | S ForStatement / StatementExpressionList; P CSN2001 |
| `A:480:11:for-iterator` | 15.8.3; 257–258 | S ForStatement / StatementExpressionList; P CSN2001 |
| `A:480:13:statement-expression-list` | 15.8.3; 257–258 | S ForStatement / StatementExpressionList; P CSN2001 |
| `A:480:17:foreach-statement` | 15.8.4; 258–261 | S ForeachStatement |
| `A:480:19:jump-statement` | 15.9; 261–262 | S JumpStatement |
| `A:480:25:break-statement` | 15.9.1; 262 | S JumpStatement break alternative |
| `A:480:27:continue-statement` | 15.9.2; 263 | S JumpStatement continue alternative |
| `A:480:29:goto-statement` | 15.9.3; 263–264 | S JumpStatement goto alternatives |
| `A:480:33:return-statement` | 15.9.4; 264–265 | S JumpStatement return alternative |
| `A:480:35:throw-statement` | 15.9.5; 265 | S JumpStatement throw alternative |
| `A:480:37:try-statement` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:480:40:catch-clauses` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:480:44:specific-catch-clauses` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:480:47:specific-catch-clause` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:480:49:general-catch-clause` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:481:2:finally-clause` | 15.10; 266–268 | S TryStatement / CatchClause / SpecificCatchClause / GeneralCatchClause / FinallyClause |
| `A:481:4:checked-statement` | 15.11; 268 | S CheckedStatement keyword choice |
| `A:481:6:unchecked-statement` | 15.11; 268 | S CheckedStatement keyword choice |
| `A:481:8:lock-statement` | 15.12; 269 | S LockStatement |
| `A:481:10:using-statement` | 15.13; 269–271 | S UsingStatement Resource choice |
| `A:481:12:resource-acquisition` | 15.13; 269–271 | S UsingStatement Resource choice |
| `A:481:15:yield-statement` | 15.14; 271–272; 26.1 437–438 | I YieldReturnStatement / YieldBreakStatement; missing context policy |

## Unsafe cross-link row bounds

- `A:493:42:unsafe-statement`: Annex PDF493 line42; Unsafe.ngr UnsafeEmbeddedStatement / UnsafeBlock; §27.1 or §27.6 as appropriate.
- `A:494:29:embedded-statement`: Annex PDF494 line29; Unsafe.ngr UnsafeEmbeddedStatement / UnsafeBlock; §27.1 or §27.6 as appropriate.
- `A:494:32:fixed-statement`: Annex PDF494 line32; §27.6 PDF458–461, Unsafe.ngr FixedStatement / FixedDeclarator / expression initializer.
- `A:494:34:fixed-pointer-declarators`: Annex PDF494 line34; §27.6 PDF458–461, Unsafe.ngr FixedStatement / FixedDeclarator / expression initializer.
- `A:494:38:fixed-pointer-declarator`: Annex PDF494 line38; §27.6 PDF458–461, Unsafe.ngr FixedStatement / FixedDeclarator / expression initializer.
- `A:494:40:fixed-pointer-initializer`: Annex PDF494 line40; §27.6 PDF458–461, Unsafe.ngr FixedStatement / FixedDeclarator / expression initializer.
- `A:494:43:local-variable-initializer`: Annex PDF494 line43; §27.7 PDF461–462, Unsafe.ngr StackAllocExpression + direct-local policy.
- `A:494:47:stackalloc-initializer`: Annex PDF494 line47; §27.7 PDF461–462, Unsafe.ngr StackAllocExpression + direct-local policy.
