# CSharp2 initial source-delta audit

This is an initial pending obligation map, not a closed normative review. The primary reference is ECMA-334 fourth edition (June 2006), pinned in csharp2-reference-policy.json. All extracted Annex A occurrences remain pending in csharp2-standard-index.json; duplicate headings retain distinct PDF-page/line identities. Annex A is informative; each owner still needs normative prose and positive/negative, ambiguity/context and spans/diagnostics review before completion.

Reproduce extraction with `python3 eng/extract-csharp2-reference.py reference.pdf output.json` in an environment with pypdf. The script rejects PDF hash drift. The downloaded reference is external scratch, not a committed binary.

| Obligation | Primary clauses | Intended owner / current state |
| --- | --- | --- |
| Constructed, qualified generic names; type parameters/attributes/constraints; generic class/struct/interface/delegate/method headers; unbound typeof | 10.8, 14.5, 25 | csharp2.generics; pending |
| Generic expression disambiguation | 9.2.3 | generics lookahead: retain arguments only before (, ), ], :, ;, comma, dot, ?, ==, !=; type contexts exempt; pending |
| Nested closers and right shifts | 9.4.5 | generics delimiters; right shift and assignment require adjacent > plus > or >=; preserve existing token behavior; pending |
| Nullable types, coalescing | 14.13, 28 | csharp2.nullable-values; lifted operators/conversions excluded; pending |
| Anonymous methods | 14.5.15, 21 | csharp2.anonymous-methods; conversions/capture execution excluded; pending |
| Yield statements and iterator syntax | 15.14, 26 | csharp2.iterators; state-machine lowering/type validity excluded; pending |
| Partial types, static classes, accessor modifiers | 17.1, 17.6, 18, 20 | csharp2.declaration-forms; merged declaration/type/member semantics excluded; pending |
| Extern aliases and alias-qualified names | 10.8, 16 | csharp2.alias-qualification; alias binding excluded; pending |
| Typed default values | 14.5.5 | csharp2.default-values; constant/type interpretation excluded; pending |
| Unicode 4.0 identifiers/whitespace/formatting | 2, 9.3–9.4 | pinned historical category policy needed; current .NET data remains unverified |
| Pragma payloads and line-indicator implementation-defined forms | 9.5.7–9.5.8 | preprocessing policy; unknown pragma interpretation cannot cause compile-time error; pending |
| Attribute/context and unsafe generic pointer-member changes | 24, 27 | existing policies plus additive typed alternatives; pending |
| Covariance, contravariance, delegate inference/conversions | conversion/member binding clauses | semantic exclusions; imported inventory classification needs correction without erasing history |
| Microsoft-specific CSharp2 surfaces, including fixed buffers | not found as fixed-buffer productions in fourth-edition annex | Pinned Microsoft VS2005 supplement p959 confirms fixed buffers; initial grammar/context fixtures implemented, normative closure still pending |

The first edition's Unicode 3.0 contract remains unchanged. New source classification must preserve explicit compiler phase differences. Normalization, symbol lookup, overload resolution, constraint satisfaction and execution are not implemented by syntax recognition.

Implementation continuation: generic/nullable/default/anonymous/iterator/declaration/alias and unsafe/fixed-buffer bundles now have finite typed, omission and oracle witnesses. Fourth-edition CSharp2 #line digit ranges/identifier indicators and opaque pragma handling are implemented with explicit current-Roslyn differences. Source rows remain pending; historical Unicode4.0 data, full lexical/prose review and universal context closure are not promoted.
