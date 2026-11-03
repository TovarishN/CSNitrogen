# CSharp6 source contract and Unicode decision

Status: user approved frozen Unicode4 scalar continuity for CSharp6 on 2026-11-04. CSharp5 admission is merged as PR55; CSharp6 remains partial. Production grammar changes follow a bounded test-only seam proof and review, as for CSharp4 and CSharp5.

## Publisher source

C# 6 has no Microsoft-published language specification. Its standard is ECMA-334 6th edition (June 2022), published by Ecma International: https://ecma-international.org/wp-content/uploads/ECMA-334_6th_edition_june_2022.pdf Downloaded 2026-11-04: 6,025,981 bytes, SHA256 `4b859dfce21f9935f234b48af55c6a9de36e2eda1eb58d3b565e5aab2b44922f`, 639 pages. Its metadata title is "ECMA-334, 6th edition, June 2022", produced by Microsoft Word 2019 and created 2022-05-12; the title page reads "ECMA-334 6th Edition / June 2022 C# Language Specification". The hash freezes those bytes as the authority. A local macOS PDFKit text extraction has SHA256 `b74337ad3d4151f370a1e591b09731044ae39981af163520afd7c8dfc160b70c`; extraction output depends on the tool, so the extraction is a review aid and is pinned separately from the PDF.

The working text is the ECMA TC49-TG2 source repository, tag `V6-standard` of https://github.com/dotnet/csharpstandard , commit `721ef2964efeb5b4b803e856a00094dc4fe21bb7` ("Finalize V6. (#611)", 2022-08-11). Its 30 `standard/*.md` files are markdown with ANTLR grammar blocks; `standard/grammar.md` at that commit has SHA256 `909c7c563d0034df3ed74395f7777933df776301551d7e7b38f3966d81f35d83`. Line references below are to that commit. The tag commit (2022-08-11) is three months later than the PDF's creation date (2022-05-12), so the two texts may differ and cannot be assumed equivalent. The repository is extraction input only: before admission, its normative text is compared against the frozen PDF and any difference is recorded and dispositioned, just as the CSharp5 appendix was reviewed against the main text. Neither source is redistributed.

The Microsoft Learn specification pages are not used: they track later drafts (C# 7+) and are not frozen.

## Unicode choice

Unlike Version2–5, which cite Unicode 3.0, the sixth edition's normative reference (normative-references.md:13) is the undated "The Unicode Standard". Identifier rules (lexical-structure.md:385) follow Unicode Standard Annex 15 with the traditional underscore, escape and `@` exceptions; identifiers not in Normalization Form C are implementation-defined with no diagnostic required (lexical-structure.md:484).

Decision: frozen Unicode4 scalar continuity, the contract already approved for CSharp2–5. `UnicodePolicy.UsesUnicode4` extends to major 6, and the resolved policy becomes `unicode-4.0-scalar`, replacing the current partial profile's host-current BMP selection. The undated reference is recorded as informational. Every identifier admitted by CSharp2–5 remains accepted and the lexical contract does not depend on the host runtime. NFC is recorded as implementation-defined and is not checked. Rejected alternative: host-current Unicode, which follows the undated reference more literally but makes acceptance vary with the .NET runtime.

The Version4/5 historical lexical, preprocessing and attribute readings (Control-Z, directive whitespace, `#line` effects, escaped underscore starts, conditional boolean spellings, attribute-argument category) do not automatically carry forward. CSharp6 currently uses current-compiler compatibility for them; Task 3 reviews each against the sixth-edition text and records whether CSharp6 shares the historical reading or the current one.

## Source-defined additions

Inventory entries from `docs/coverage/features.json`, mapped to sixth-edition grammar:

- String interpolation: `interpolated_string_expression` and its regular/verbatim forms, §11.7.3, grammar.md:724–815. Six lexical rules are context sensitive (expressions.md:1316–1323): Mid, Format and End are recognised only inside an interpolated string, Format only when its colon is not nested in any bracket, and interpolations nest arbitrarily. Regular interpolated text excludes new-line characters; verbatim text does not.
- Null-conditional operator: `null_conditional_member_access`, `dependent_access`, `null_conditional_element_access`, `null_conditional_invocation_expression` and `null_conditional_projection_initializer`, §11.7.7/§11.7.9/§11.7.11, grammar.md:855–897. `a?.5:1` must keep its conditional-expression reading.
- `nameof`: `nameof_expression`/`named_entity`, §11.7.20, grammar.md:1044–1050. `nameof` is not a keyword; expressions.md:2854 makes the choice between a nameof expression and an invocation of a simple name `nameof` depend on name lookup. That choice is binding, not syntax (see below).
- Exception filters: `exception_filter` `'when' '(' boolean_expression ')'` on catch clauses, §12.11, grammar.md:1555–1580; `when` stays contextual.
- Using static: `using_static_directive`, §13.5.4, grammar.md:1659–1662; `static` follows `using`, distinct from using-alias and using-namespace directives.
- Auto-property initializers: `property_initializer` after an accessor block, §14.7.1, grammar.md:1917–1923. Whether the property is automatic (and so may have an initializer) is a source-local check over accessor bodies, not a separate production.
- Expression-bodied members: `'=>' expression ';'` bodies for methods (including the separate null-conditional-invocation alternative), properties, indexers, operators and conversion operators, grammar.md:1857–1862, 1917–1919, 2012–2015, 2058–2062. Constructors, finalizers and accessors are not expression-bodied in C# 6.
- Dictionary (index) initializers: `initializer_target` `'[' argument_list ']'` in object initializers, §11.7.15.3, grammar.md:924–945.
- Await in catch/finally: §11.8.8.1 (expressions.md:3116–3121) drops the catch/finally restriction while keeping nested non-async functions, lock blocks, expression-tree conversions and unsafe contexts. The existing check at `src/CSNitrogen/Parsing/CSharp5ContextPolicy.cs:89` applies only to major 5, so the policy split is already in place and must be verified, not rewritten.
- Default values for getter-only properties: `{ get; }` already parses under the CSharp5 grammar; the sixth-edition change is the property initializer above plus semantic read-only assignment. No separate production.

Classified semantic or tooling, outside parser scope: compiler-as-a-service (Roslyn), the "draft specification online" inventory line, interpolated string conversions to `IFormattable`/`FormattableString`, format evaluation, null-conditional lifting and evaluation, `nameof` value computation, static member import lookup, filter evaluation order and read-only property assignment. Inherited CSharp5 semantic exclusions carry forward.

## nameof is binding-dependent

The standard resolves `nameof(x)` by name lookup, so a parser cannot always choose one reading. Contract (user decision, 2026-11-04):
- Where only the `named_entity` reading parses (for example `nameof(int)` or `nameof(base)`, whose arguments are not expressions), produce a nameof expression.
- Where only the invocation reading parses, produce an invocation.
- Where both parse (for example `nameof(x)`, `nameof(a.b)`), produce Nitrogen's existing `SyntaxKinds.Ambiguous` node holding both the nameof-expression and invocation readings, each typed, with original spans. The parse is accepted and reports no issue; neither reading is presented as a resolved semantic choice. Policies that walk parents already pass through Ambiguous (`CSharpCompilationUnitPolicy.cs:26`); every other policy and typed view that meets the node must be checked to handle both readings.

This follows the design rule to preserve binding ambiguities rather than claim them. The seam proof must show Nitrogen yields this node for the dual spellings without a core change; if it cannot, a concrete core proposal comes back before nameof production work. Rejected alternative: always a nameof node with an assumption flag, which gives a wrong tree for files that declare a `nameof` member.

## Bounded implementation direction

Use admitted CSharp5 as the baseline and keep independent capabilities: one feature module per inventory entry where its version boundary or selection needs it, with explicit requirements. Additions must not change existing generated property types or original UTF16 coordinates; a real public property-type or Nitrogen core change returns for a separate concrete decision.

String interpolation is the first feasibility gate. The CSharp1 lexical design flagged nested interpolation and context-sensitive token boundaries as unproven in this consumer, and the CSharp5 await context already needed Nitrogen core PR94. A test-only probe must show, before any production edit, that the existing Nitrogen mechanisms can: tokenize Mid/Format/End only inside their string; nest interpolated strings inside interpolations, including verbatim inside regular and the reverse; distinguish a format colon from `?:`, `::` and named-argument colons inside brackets; reject new-lines in regular interpolated text; handle `{{`/`}}` escapes; and give every hole a typed expression child with original spans, including after supplementary-character trivia. A regex that consumes the whole string without typed holes is not acceptable. If composition cannot express this, the probe keeps passing observation tests plus the failing desired-contract cases, and a concrete Nitrogen core proposal comes back before dependent work.

The same probe covers `?.` against `?` followed by a numeric literal, and the nameof contract above. Exception filters, using static, property initializers, expression bodies and index initializers are additive productions expected to fit existing seams; they are proved in the same test-only composition but are not feasibility gates.

## Oracle

Roslyn observations use the existing pinned `Microsoft.CodeAnalysis.CSharp` test package with `LanguageVersion.CSharp6`, as CSharp5 used `LanguageVersion.CSharp5`; no new package is added. A current compiler run at version 6 is an observation, not the authority: where it disagrees with the sixth-edition text (for example new-lines inside regular interpolation holes, or format-colon recognition), the standard wins, and the disagreement is recorded as a documented oracle divergence with its witness, not treated as a test failure. (Builder's call, following the design's documented-discrepancy rule.)

## Staging

1. Task 1: hash the PDF, record the reference policy and Unicode approval, and run the test-only seam proof described above, against the real 32-module CSharp5 factory under a separate generated namespace. Focused proof, Roslyn CSharp6 syntax observations and full solution pass; fresh branch review before merge.
2. Task 2: production capabilities and the Unicode4 selector change for major 6, with RED cases first; refresh predecessor pins in 5->4->3->2 order. CSharp6 stays partial. If the interpolation gate needs a Nitrogen core change, only string interpolation waits for that decision: the other additions proceed as Task 2a, and interpolation follows as Task 2b once the mechanism is settled. Tasks 3 and 4 need both. (Builder's call.)
3. Task 3: index the sixth-edition grammar independently, compare it with the PDF and with admitted CSharp5, disposition every main-text difference, link and execute every alternative on the sixth factory, audit owners and review the historical lexical readings.
4. Task 4: an independent fail-closed `CSharp6CoverageContract` requiring admitted CSharp5, the exact source/factory/policy/evidence inputs and a recorded reviewer approval, as for CSharp5. CSharp7+, the 7.x minors and custom selections stay partial.

No CSharp6 admission is claimed by this document or by the bounded proof.
