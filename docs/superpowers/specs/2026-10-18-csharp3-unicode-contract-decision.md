# CSharp3 historical Unicode contract decision

Status: decision required before whole-source admission. Production syntax additions can ship with CSharp3 still partial. This document does not authorize a Unicode policy change.

## Why a contract decision is needed

The pinned Microsoft Version 3.0 document, identifier clause 2.4.2, refers readers to Unicode Version 3.0 for character classes. Its wording is informational rather than an explicit frozen table identity. The reviewed CSharp2 contract uses the fourth-edition ECMA source and frozen Unicode 4.0 scalar tables. Interpreting the CSharp3 reference literally would narrow the identifier repertoire compared with the preceding reviewed version; inheriting Unicode 4.0 instead requires an explicit source-precedence decision.

The current partial CSharp3 implementation uses host Unicode categories over BMP characters. It implements neither candidate historical contract. Keeping that placeholder would make source admission depend on the runtime's Unicode tables and continue rejecting supplementary letters accepted by CSharp2. It must not be promoted to reviewed completeness.

Primary evidence: [pinned Version 3.0 source](../../coverage/csharp3-reference-policy.json), original extracted identifier text at physical line 1253 using `split('\n')`; [CSharp2 reference policy](../../coverage/csharp2-reference-policy.json). The DOC SHA256 is ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1. This decision does not claim to have reproduced the original Microsoft C#3 compiler.

## Concrete alternatives

| Contract | Consequence | Recommendation |
| --- | --- | --- |
| Pin Unicode 4.0 scalar tables for CSharp3 | Preserve the reviewed CSharp2 character repertoire, including supplementary letters. Record explicitly that the DOC's Version 3.0 reference is informational and the lexical table authority remains the reviewed CSharp2 source/data. | Recommended: stable, reproducible repertoire without a backwards step between named versions. |
| Pin Unicode 3.0 tables for CSharp3 | Interpret the DOC's reference as the exact historical table authority. Reject letters introduced after Unicode 3.0 even when CSharp2 accepts them; record that deliberate non-monotonic source contract. | Appropriate if literal edition fidelity has priority over repertoire continuity. |

Both alternatives keep CSharp3's own lexical grammar, newline rules and source diagnostics. Selecting Unicode 3.0 tables would not copy the entire CSharp1 policy. Both require matching frozen identifier/format/space classification in parsing, preprocessing and trivia; current-host tables are not an admission option. No generated syntax API or semantic compiler change is proposed.

## Executable observations before the choice

`tests/CSNitrogen.Tests/Lexing/CSharp3UnicodeDecisionTests.cs` records actual public parser observations, rather than inferring them from a table name:

| Authored witness | CSharp1 now | CSharp2 now | Partial CSharp3 now | Unicode 3.0 candidate | Unicode 4.0 candidate |
| --- | --- | --- | --- | --- | --- |
| Class named U+0220 | Reject | Accept | Accept | Reject | Accept |
| Class named U+10400, literal or `\U00010400` | Reject | Accept | Reject | Reject | Accept |
| U+205F before `class` | Reject | Accept | Accept | Reject | Accept |

A separate retained observation shows the partial profile currently accepts an escaped initial underscore that the preceding source policies reject. Escape spelling, raw keyword recognition, normalization and newline rules still need their own source-local review regardless of the table choice. Choosing a table does not itself close those obligations.

## Work after the decision

Implement the selected frozen table contract and source-specific lexical/directive differences with paired positive/negative/span tests. Then finish the historical occurrence crosswalk and licensed external corpus review before adding a separate fail-closed CSharp3 admission gate. CSharp3 completeness remains false until all of those checks pass. Later version profiles keep their explicit partial status and need their own historical review.
