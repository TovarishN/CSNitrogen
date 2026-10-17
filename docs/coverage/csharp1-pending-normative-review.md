# Whole pending C# 1 source review — 2026-10-17

This reviews exactly the 331 source occurrences pending after PR #14, in addition to the 31 earlier normalized type/name reviews. The pinned ECMA-334 first-edition reference and source hash in `csharp1-reference-policy.json` remain authoritative. Appendix A is informative; normative clauses govern conflicts (including void delegate returns and local-only stackalloc).

The four review dimensions mean normative RHS/prose correspondence; positive/negative finite alternative and boundary witnesses; syntactic ambiguity/owning context; and original UTF-16 spans plus source-located diagnostics. They do not mean universal malformed-input phase/recovery parity with Roslyn. Shared syntax categories preserve the already-approved semantic exclusions, including binding/denotation, constant/boolean conversion, writability, modifier compatibility, operator arity/types, using-resource initialization, unsafe context/handler legality and execution.

Independent domain audits examined every listed occurrence. The added closure suites fill their concrete witness gaps; existing recursive comparers inspect typed discriminants, children, optional/list fields, separators and spans. Inlined helper/list rows inherit their actual owner's structural evidence. Primitive Unicode category alternatives use independent original Unicode 3.0 records, including exhaustive BMP category comparisons. Generated Unicode tries are deterministic implementation helpers, not independent source-language features.

## Fixture groups

- **L**: `PendingLexicalClosureTests` (literal alternatives/negative boundaries, maximal munch, comments/newlines/EOF, directive whitespace/inactive grammar, condition precedence, filenames/messages/regions), plus `HistoricalUnicodeParityTests`, `HistoricalConditionalIdentityTests`, `PendingLexicalGapTests`, `ExactLineMappingTests`, and explicit `PreprocessingParityTests` historical/current differences. All escape widths/suffix casing and five historical newline spellings have witnesses; NEL and terminal Control-Z are explicit historical observations.
- **S**: `PendingStatementClosureTests` + `StatementConformanceTests/ParityTests` and recursive `StatementShapeAssert`: all statement alternatives, empty/nonempty lists, embedded versus full statements, dangling else, for-header categories, labels, all jumps, catch cardinality/order, resource forms, malformed lists/delimiters and declared semantic boundaries.
- **E**: `PendingExpressionClosureTests` + existing `CastConformanceParityTests`, `ArrayArgumentConformanceParityTests`, `UnsafeConformanceParityTests`, `ContextConformanceParityTests`, `SourceGrammarParityTests`, and recursive `ExpressionShapeAssert`/`UnsafeShapeAssert`. X1=primary/unary/cast operands; X2=is/as precedence and type operands; X3=all assignment operators/target spans; X4=conditional arms and missing operands; X5=semantic expression wrappers; X6=unsafe tails and existing fixed/membership; X7=stackalloc direct-local contexts and deferred handler restrictions; X8=existing array/initializer alternatives. The is/as postfix/tighter-operator cases are additionally recorded as explicit historical/reference phase observations.
- **D**: `PendingDeclarationClosureTests` + existing `DeclarationConformanceParityTests`, `AttributeConformanceTests/ParityTests`, `CompilationUnitConformanceTests/ParityTests`. Covers all five type headers/modifier owners, list/base/header/body alternatives, all 24 unary/binary operator forms (22 tokens), both conversions, member/parameter/accessor attributes, positional/named historical expression arguments, malformed lists/delimiters and declared semantic boundaries. Existing recursive declaration/parameter/compilation comparers assert actual fields and original spans.

## Per-occurrence correspondence

Each row below was reviewed against its normative clause and the cited group, with the corrected owner/policy identities in the hashed source crosswalk. The source crosswalk's review declarations remain review metadata; passing counts or structural catalog resolution alone do not establish this review.

| Source occurrence | Normative clause | Witness group / detail |
|---|---|---|
| `A:353:7:input` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:353:9:input-section` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:353:12:input-section-part` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:353:15:input-elements` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:353:18:input-element` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:353:23:new-line` | 9.3.1 | **L** comments-lines-and-input-transforms |
| `A:353:30:whitespace` | 9.3.3 | **L** comments-lines-and-input-transforms |
| `A:353:36:comment` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:353:39:single-line-comment` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:1:input-characters` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:354:4:input-character` | 9.3 | **L** comments-lines-and-input-transforms |
| `A:354:6:new-line-character` | 9.3.1 | **L** comments-lines-and-input-transforms |
| `A:354:11:delimited-comment` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:13:delimited-comment-characters` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:16:delimited-comment-character` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:19:not-asterisk` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:21:not-slash` | 9.3.2 | **L** comments-lines-and-input-transforms |
| `A:354:24:token` | 9.3,9.4,9.4.5 | **L** maximal-munch-at-source-boundaries |
| `A:354:33:unicode-character-escape-sequence` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:354:37:identifier` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:354:40:available-identifier` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:354:42:identifier-or-keyword` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:354:44:identifier-start-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:1:identifier-part-characters` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:4:identifier-part-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:10:letter-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:14:combining-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:17:decimal-digit-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:20:connecting-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:23:formatting-character` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:355:27:keyword` | 9.4.1-9.4.3 | **L** identifier-policy-existing-closure |
| `A:356:2:literal` | 9.4.4 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:9:boolean-literal` | 9.4.4.1 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:12:integer-literal` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:15:decimal-integer-literal` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:17:decimal-digits` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:20:decimal-digit` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:22:integer-type-suffix` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:24:hexadecimal-integer-literal` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:27:hex-digits` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:30:hex-digit` | 9.4.4.2 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:32:real-literal` | 9.4.4.3 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:37:exponent-part` | 9.4.4.3 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:40:sign` | 9.4.4.3 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:42:real-type-suffix` | 9.4.4.3 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:356:44:character-literal` | 9.4.4.4-9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:1:character` | 9.4.4.4-9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:6:single-character` | 9.4.4.4-9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:8:simple-escape-sequence` | 9.4.4.4-9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:10:hexadecimal-escape-sequence` | 9.4.4.4-9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:12:string-literal` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:15:regular-string-literal` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:17:regular-string-literal-characters` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:20:regular-string-literal-character` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:25:single-regular-string-literal-character` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:27:verbatim-string-literal` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:29:verbatim-string-literal-characters` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:32:verbatim-string-literal-character` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:35:single-verbatim-string-literal-character` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:37:quote-escape-sequence` | 9.4.4.5 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:39:null-literal` | 9.4.4.6 | **L** literal-alternatives-and-authored-spans, literal-negative-boundaries |
| `A:357:42:operator-or-punctuator` | 9.3,9.4,9.4.5 | **L** maximal-munch-at-source-boundaries |
| `A:358:2:pp-directive` | 9.5 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:8:pp-new-line` | 9.5.3 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:10:conditional-symbol` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:12:pp-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:14:pp-or-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:17:pp-and-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:20:pp-equality-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:24:pp-unary-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:27:pp-primary-expression` | 9.5.1-9.5.2 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:33:pp-declaration` | 9.5.3 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:36:pp-conditional` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:38:pp-if-section` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:41:pp-elif-sections` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:44:pp-elif-section` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:358:47:pp-else-section` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:1:pp-endif` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:3:conditional-section` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:6:skipped-section` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:9:skipped-section-part` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:12:skipped-characters` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:14:not-number-sign` | 9.5.4 | **L** directive-whitespace-comments-and-inactive-validation |
| `A:359:16:pp-line` | 9.5.7 | **L** filename-message-region-boundaries |
| `A:359:18:line-indicator` | 9.5.7 | **L** filename-message-region-boundaries |
| `A:359:22:file-name` | 9.5.7 | **L** filename-message-region-boundaries |
| `A:359:24:file-name-characters` | 9.5.7 | **L** filename-message-region-boundaries |
| `A:359:27:file-name-character` | 9.5.7 | **L** filename-message-region-boundaries |
| `A:359:29:pp-diagnostic` | 9.5.5 | **L** filename-message-region-boundaries |
| `A:359:32:pp-message` | 9.5.5 | **L** filename-message-region-boundaries |
| `A:359:34:pp-region` | 9.5.6 | **L** filename-message-region-boundaries |
| `A:359:36:pp-start-region` | 9.5.6 | **L** filename-message-region-boundaries |
| `A:359:38:pp-end-region` | 9.5.6 | **L** filename-message-region-boundaries |
| `A:361:14:variable-reference` | 12.4 | **E** X5 |
| `A:361:17:argument-list` | 14.4.1 | **E** X5, X8 |
| `A:361:20:argument` | 14.4.1 | **E** X5, X8 |
| `A:361:24:primary-expression` | 14.5 | **E** X1 |
| `A:361:27:primary-no-array-creation-expression` | 14.5 | **E** X1, X6 |
| `A:361:45:simple-name` | 14.5.2 | **E** X1 |
| `A:361:47:parenthesized-expression` | 14.5.3 | **E** X1 |
| `A:362:1:member-access` | 14.5.4 | **E** X1 |
| `A:362:7:invocation-expression` | 14.5.5 | **E** X1 |
| `A:362:9:element-access` | 14.5.6 | **E** X1, X8 |
| `A:362:11:expression-list` | 14.5.6 | **E** X1, X8 |
| `A:362:14:this-access` | 14.5.7 | **E** X1 |
| `A:362:16:base-access` | 14.5.8 | **E** X1 |
| `A:362:19:post-increment-expression` | 14.5.9 | **E** X3 |
| `A:362:21:post-decrement-expression` | 14.5.9 | **E** X3 |
| `A:362:23:object-creation-expression` | 14.5.10.1 | **E** X1 |
| `A:362:25:array-creation-expression` | 14.5.10.2 | **E** X8 |
| `A:362:28:delegate-creation-expression` | 14.5.10.3 | **E** X1 |
| `A:362:30:typeof-expression` | 14.5.11 | **E** X1 |
| `A:362:33:checked-expression` | 14.5.12 | **E** X1 |
| `A:362:35:unchecked-expression` | 14.5.12 | **E** X1 |
| `A:362:37:unary-expression` | 14.6, 14.6.5 | **E** X3, X6 |
| `A:362:47:pre-increment-expression` | 14.6, 14.6.5 | **E** X3 |
| `A:363:1:pre-decrement-expression` | 14.6, 14.6.5 | **E** X3 |
| `A:363:3:cast-expression` | 14.6.6 | **E** X1 |
| `A:363:5:multiplicative-expression` | 14.7 | **E** X2 |
| `A:363:10:additive-expression` | 14.7 | **E** X2 |
| `A:363:14:shift-expression` | 14.8 | **E** X2 |
| `A:363:18:relational-expression` | 14.9 | **E** X2 |
| `A:363:26:equality-expression` | 14.9 | **E** X2 |
| `A:363:30:and-expression` | 14.10 | **E** X2 |
| `A:363:33:exclusive-or-expression` | 14.10 | **E** X2 |
| `A:363:36:inclusive-or-expression` | 14.10 | **E** X2 |
| `A:363:39:conditional-and-expression` | 14.11 | **E** X2 |
| `A:363:42:conditional-or-expression` | 14.11 | **E** X2 |
| `A:363:45:conditional-expression` | 14.12 | **E** X4 |
| `A:363:48:assignment` | 14.13 | **E** X3 |
| `A:364:1:assignment-operator` | 14.13 | **E** X3 |
| `A:364:3:expression` | 14.14 | **E** X1, X3, X4 |
| `A:364:6:constant-expression` | 14.15 | **E** X5 |
| `A:364:8:boolean-expression` | 14.16 | **E** X4, X5 |
| `A:364:11:statement` | 15 introductory grammar; printed p179 / PDF193 | **S** Statement |
| `A:364:15:embedded-statement` | 15 introductory grammar; printed p179 / PDF193 | **S** StandardEmbeddedStatement enumerates all twelve normative embedded categories; declaration/label only occur via Statement and are excluded as direct bodies |
| `A:364:27:block` | 15.2; printed p181 / PDF195 | **S** Braces enclose Statement*, equivalent to optional nonempty statement-list |
| `A:364:29:statement-list` | 15.2.1; printed p181 / PDF195 | **S** Block |
| `A:364:32:empty-statement` | 15.3; printed pp181-182 / PDF195-196 | **S** The semicolon alternative in StandardEmbeddedStatement is an empty leaf |
| `A:364:34:labeled-statement` | 15.4; printed p182 / PDF196 | **S** Identifier colon Statement allows labels on any statement, including a declaration |
| `A:364:36:declaration-statement` | 15.5; printed p182 / PDF196 | **S** StandardStatement selects ConstantDeclaration semicolon or VariableDeclaration semicolon |
| `A:364:39:local-variable-declaration` | 15.5.1; printed pp182-183 / PDF196-197 | **S** VariableDeclaration is type plus one-or-more comma-separated VariableDeclarator; ConstantDeclaration belongs to the separate constant production |
| `A:364:41:local-variable-declarators` | 15.5.1; printed p183 / PDF197 | **S** VariableDeclaration |
| `A:364:44:local-variable-declarator` | 15.5.1; printed p183 / PDF197 | **S** Identifier with optional equals/VariableInitializer group matches both alternatives and preserves absent initializer explicitly |
| `A:365:1:local-variable-initializer` | 15.5.1 and 25.7 | **E** X7 |
| `A:365:4:local-constant-declaration` | 15.5.2; printed pp183-184 / PDF197-198 | **S** ConstantDeclaration is const, type and nonempty comma-separated declarators; StandardStatement adds the semicolon |
| `A:365:6:constant-declarators` | 15.5.2; printed p183 / PDF197 | **S** ConstantDeclaration |
| `A:365:9:constant-declarator` | 15.5.2; printed pp183-184 / PDF197-198 | **S** Identifier equals Expression matches constant-expression syntax; constancy/conversion is excluded semantic work |
| `A:365:11:expression-statement` | 15.6; printed p184 / PDF198 | **S** StatementExpression plus semicolon; CSN2001 narrows Expression roots to invocation, object creation, assignment and four increment/decrement forms |
| `A:365:13:statement-expression` | 15.6; printed p184 / PDF198 | **S** CSN2001 explicitly permits invocation/object creation/assignment/postfix increment/decrement; UnaryExpression only when authored prefix is ++ or -- |
| `A:365:21:selection-statement` | 15.7; printed p184 / PDF198 | **S** StandardEmbeddedStatement selects IfStatement or SwitchStatement for the two normative categories |
| `A:365:24:if-statement` | 15.7.1; printed pp184-185 / PDF198-199 | **S** IfStatement has mandatory parenthesized condition/body, optional else body |
| `A:365:27:boolean-expression` | 15.7.1 and 14.16; printed p184 / PDF198 | **S** The normative grammar is Expression; boolean conversion/operator true/constant evaluation belongs to excluded semantics |
| `A:365:29:switch-statement` | 15.7.2; printed pp185-188 / PDF199-202 | **S** SwitchStatement has expression and braced zero-or-more sections; CSN2006 enforces one immediate default per switch |
| `A:365:31:switch-block` | 15.7.2; printed pp185-186 / PDF199-200 | **S** Braced SwitchStatement |
| `A:365:33:switch-sections` | 15.7.2; printed pp185-186 / PDF199-200 | **S** SwitchStatement |
| `A:365:36:switch-section` | 15.7.2 and 15.2.1; printed pp185-186 / PDF199-200 | **S** SwitchSection |
| `A:365:38:switch-labels` | 15.7.2; printed pp185-186 / PDF199-200 | **S** SwitchSection |
| `A:365:41:switch-label` | 15.7.2; printed pp185-186 / PDF199-200 | **S** SwitchLabel alternatives are case Expression colon/default colon; CSN2006 enforces default cardinality by inspecting immediate labels; constant-expression denotation excluded |
| `A:365:44:iteration-statement` | 15.8; printed p188 / PDF202 | **S** StandardEmbeddedStatement enumerates while/do/for/foreach |
| `A:366:1:while-statement` | 15.8.1; printed pp188-189 / PDF202-203 | **S** WhileStatement requires while (Expression) EmbeddedStatement; boolean-expression syntax is Expression |
| `A:366:3:do-statement` | 15.8.2; printed p189 / PDF203 | **S** DoStatement requires do EmbeddedStatement while (Expression) semicolon |
| `A:366:5:for-statement` | 15.8.3; printed pp189-190 / PDF203-204 | **S** ForStatement has independently optional initializer/condition/iterator separated by mandatory semicolons and an embedded body; 18 header combinations compare option presence |
| `A:366:7:for-initializer` | 15.8.3; printed p190 / PDF204 | **S** Initializer choice is VariableDeclaration or StatementExpressionList, equivalent to normative local declaration or expression list |
| `A:366:10:for-condition` | 15.8.3; printed p190 / PDF204 | **S** Condition is optional Expression (the syntax of boolean-expression); it is neither a declaration nor statement-expression list |
| `A:366:12:for-iterator` | 15.8.3; printed p190 / PDF204 | **S** Iterator is optional StatementExpressionList only; declaration is not an alternative |
| `A:366:14:statement-expression-list` | 15.8.3; printed p190 / PDF204 | **S** StatementExpressionList |
| `A:366:17:foreach-statement` | 15.8.4; printed pp190-192 / PDF204-206 | **S** ForeachStatement has type identifier in Expression and EmbeddedStatement; member/conversion/readonly/enumeration behavior is excluded |
| `A:366:19:jump-statement` | 15.9; printed p192 / PDF206 | **S** JumpStatement alternatives cover break/continue/goto/return/throw with appropriate delimiters and optional expression forms |
| `A:366:25:break-statement` | 15.9.1; printed p193 / PDF207 | **S** JumpStatement break semicolon; placement/target/finally transfer is excluded control-flow analysis |
| `A:366:27:continue-statement` | 15.9.2; printed p194 / PDF208 | **S** JumpStatement continue semicolon; loop target/finally transfer is excluded control-flow analysis |
| `A:366:29:goto-statement` | 15.9.3; printed pp194-195 / PDF208-209 | **S** Goto supports identifier, case Expression, default then semicolon; label/case target resolution and constant conversions excluded |
| `A:366:33:return-statement` | 15.9.4; printed pp195-196 / PDF209-210 | **S** Return has optional Expression then semicolon; containing member type and finally prohibition are excluded semantic/control-flow checks |
| `A:366:35:throw-statement` | 15.9.5; printed pp196-197 / PDF210-211 | **S** Throw has optional Expression then semicolon; exception type and bare-throw catch context are excluded semantics |
| `A:366:37:try-statement` | 15.10; printed pp197-199 / PDF211-213 | **S** TryStatement requires block and catch clauses or finally or both; alternatives enforce general catch at most once, after specifics, finally last |
| `A:366:41:catch-clauses` | 15.10; printed p197 / PDF211 | **S** Handlers choice is specific+ optional general optional finally, or general optional finally, or finally |
| `A:366:44:specific-catch-clauses` | 15.10; printed p197 / PDF211 | **S** SpecificCatchClause+ owns the nonempty ordered repeated specific catches within TryStatement |
| `A:367:1:specific-catch-clause` | 15.10; printed pp197-198 / PDF211-212 | **S** SpecificCatchClause uses catch-( lookahead and CatchClause with TypeReference/optional identifier and Block |
| `A:367:3:general-catch-clause` | 15.10; printed p197 / PDF211 | **S** GeneralCatchClause uses catch-{ lookahead and CatchClause without declaration; TryStatement enforces final position among catches |
| `A:367:5:finally-clause` | 15.10; printed pp197-199 / PDF211-213 | **S** FinallyClause is finally Block; TryStatement positions it last |
| `A:367:7:checked-statement` | 15.11; printed pp199-200 / PDF213-214 | **S** CheckedStatement keyword union checked/unchecked followed by Block; keyword checked by shape helper |
| `A:367:9:unchecked-statement` | 15.11; printed pp199-200 / PDF213-214 | **S** CheckedStatement keyword union checked/unchecked followed by Block; keyword checked by shape helper |
| `A:367:11:lock-statement` | 15.12; printed p200 / PDF214 | **S** LockStatement is lock (Expression) EmbeddedStatement |
| `A:367:13:using-statement` | 15.13; printed pp200-201 / PDF214-215 | **S** UsingStatement is using (VariableDeclaration or Expression) EmbeddedStatement; both resource syntax alternatives represented |
| `A:367:15:resource-acquisition` | 15.13; printed pp200-201 / PDF214-215 | **S** UsingStatement |
| `A:367:18:compilation-unit` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:20:namespace-declaration` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:25:namespace-body` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:27:using-directives` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:30:using-directive` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:33:using-alias-directive` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:35:using-namespace-directive` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:37:namespace-member-declarations` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:367:40:namespace-member-declaration` | 16.1-16.5 | **D** owning header/list/payload; CompilationUnitConformanceParityTests.Ordered_root_and_namespace_projections_match |
| `A:368:1:type-declaration` | 16.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:8:class-declaration` | 17.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:10:class-modifiers` | 17.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:13:class-modifier` | 17.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:21:class-base` | 17.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:25:interface-type-list` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:28:class-body` | 17.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:30:class-member-declarations` | 17.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:33:class-member-declaration` | 17.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:45:constant-declaration` | 17.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:368:47:constant-modifiers` | 17.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:1:constant-modifier` | 17.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:7:constant-declarators` | 17.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:10:constant-declarator` | 17.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:12:field-declaration` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:14:field-modifiers` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:17:field-modifier` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:26:variable-declarators` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:29:variable-declarator` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:32:variable-initializer` | 17.4,17.4.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:35:method-declaration` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:37:method-header` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:369:39:method-modifiers` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:1:method-modifier` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:16:member-name` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:19:method-body` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:22:formal-parameter-list` | 17.5.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:26:fixed-parameters` | 17.5.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:29:fixed-parameter` | 17.5.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:31:parameter-modifier` | 17.5.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:34:parameter-array` | 17.5.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:36:property-declaration` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:370:38:property-modifiers` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:1:property-modifier` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:13:member-name` | 17.5 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:16:accessor-declarations` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:19:get-accessor-declaration` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:21:set-accessor-declaration` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:23:accessor-body` | 17.6,17.6.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:26:event-declaration` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:30:event-modifiers` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:33:event-modifier` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:45:event-accessor-declarations` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:371:48:add-accessor-declaration` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:1:remove-accessor-declaration` | 17.7,17.7.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:3:indexer-declaration` | 17.8 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:5:indexer-modifiers` | 17.8 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:8:indexer-modifier` | 17.8 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:19:indexer-declarator` | 17.8 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:22:operator-declaration` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:24:operator-modifiers` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:27:operator-modifier` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:31:operator-declarator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:35:unary-operator-declarator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:37:overloadable-unary-operator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:39:binary-operator-declarator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:41:overloadable-binary-operator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:43:conversion-operator-declarator` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:372:46:operator-body` | 17.9-17.9.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:1:constructor-declaration` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:3:constructor-modifiers` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:6:constructor-modifier` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:12:constructor-declarator` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:14:constructor-initializer` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:17:constructor-body` | 17.10,17.10.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:20:static-constructor-declaration` | 17.11 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:22:static-constructor-modifiers` | 17.11 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:25:static-constructor-body` | 17.11 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:28:destructor-declaration` | 17.12 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:30:destructor-body` | 17.12 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:34:struct-declaration` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:36:struct-modifiers` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:39:struct-modifier` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:373:45:struct-interfaces` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:374:1:struct-body` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:374:3:struct-member-declarations` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:374:6:struct-member-declaration` | 18.1-18.1.3,18.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:374:30:array-initializer` | 19.6 | **E** X8 |
| `A:374:33:variable-initializer-list` | 19.6 | **E** X8 |
| `A:374:36:variable-initializer` | 19.6 | **E** X8, X7 |
| `A:374:40:interface-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:374:43:interface-modifiers` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:1:interface-modifier` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:7:interface-base` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:9:interface-body` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:11:interface-member-declarations` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:14:interface-member-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:19:interface-method-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:21:interface-property-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:23:interface-accessors` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:28:interface-event-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:30:interface-indexer-declaration` | 20.1-20.2 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:33:enum-declaration` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:35:enum-base` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:37:enum-body` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:40:enum-modifiers` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:375:43:enum-modifier` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:1:enum-member-declarations` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:4:enum-member-declaration` | 21.1-21.3 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:8:delegate-declaration` | 22.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:11:delegate-modifiers` | 22.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:14:delegate-modifier` | 22.1 | **D** owning header/list/payload; DeclarationConformanceParityTests.Declaration_signatures_and_member_payloads_match |
| `A:376:21:global-attributes` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:23:global-attribute-sections` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:26:global-attribute-section` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:29:global-attribute-target-specifier` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:31:global-attribute-target` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:33:attributes` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:35:attribute-sections` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:38:attribute-section` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:376:41:attribute-target-specifier` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:1:attribute-target` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:10:attribute-list` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:13:attribute` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:15:attribute-name` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:17:attribute-arguments` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:21:positional-argument-list` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:24:positional-argument` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:26:named-argument-list` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:29:named-argument` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:31:attribute-argument-expression` | 24.2 | **D** owning header/list/payload; AttributeConformanceTests.Attribute_syntax_contexts |
| `A:377:34:embedded-statement` | 25.1 | **E** X6 |
| `A:377:37:unsafe-statement` | 25.1 | **E** X6 |
| `A:378:1:primary-no-array-creation-expression` | 25.5 | **E** X1, X6 |
| `A:378:6:unary-expression` | 25.5 | **E** X3, X6 |
| `A:378:10:pointer-indirection-expression` | 25.5.1 | **E** X6 |
| `A:378:12:pointer-member-access` | 25.5.2 | **E** X6 |
| `A:378:14:pointer-element-access` | 25.5.3 | **E** X6 |
| `A:378:16:addressof-expression` | 25.5.4 | **E** X6 |
| `A:378:18:sizeof-expression` | 25.5.8 | **E** X6 |
| `A:378:20:embedded-statement` | 25.6 | **E** X6 |
| `A:378:23:fixed-statement` | 25.6 | **E** X6 |
| `A:378:25:fixed-pointer-declarators` | 25.6 | **E** X6 |
| `A:378:28:fixed-pointer-declarator` | 25.6 | **E** X6 |
| `A:378:30:fixed-pointer-initializer` | 25.6 | **E** X6 |
| `A:378:33:variable-initializer` | 25.7 | **E** X7 |
| `A:378:37:stackalloc-initializer` | 25.7 | **E** X7 |

## Corrections and remaining release boundary

The continuation corrects historical lexical/directive discrepancies, exact logical line mappings, named-cast builtin followers, historical attribute expression arguments, unrecognized-target warnings, and postfix target categories. Modern compiler differences are preserved explicitly rather than filtered out. Source metadata now identifies type-header modifiers and list/condition/iterator/initializer owners accurately.

All 362 source rows are reviewed within the scoped recognition contract. Independent domain/code review found no remaining required source fixture or valid-syntax gap; final verification accompanies publication. The subsequent [canonical capability implementation](csharp1-canonical-coverage-evidence.md) closes the former 21-identity runtime gap and verifies the separate runtime-profile-contract gate. Named CSharp1 reports complete syntax coverage within these semantic exclusions. Later-version grammar completeness, arbitrary raw delimiters, executable custom factories and fine-grained omission remain separate work.
