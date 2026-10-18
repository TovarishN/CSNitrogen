# CSharp3 historical lexical and source-delta evidence

The user approved the frozen Unicode4 scalar contract on 2026-10-18. CSharp3 now selects the same pinned data as reviewed CSharp2 for identifiers, trivia and preprocessing boundaries. This replaces the host-dependent BMP placeholder. The CSharp3 DOC's informational Unicode3 reference is subordinate to that explicit decision; CSharp3's own lexical productions remain authoritative for their syntax.

The SHA-pinned Microsoft C#3 DOC is indexed by `tools/extract-csharp3-standard-index.py`. A fresh macOS textutil extraction reproduces 454 Appendix B occurrences / 425 names, including the two source headings lacking a colon. Occurrence identities and body hashes are retained; every occurrence is still indexed-unreviewed. The index does not establish normative coverage.

Source-specific differences are implemented independently of the reviewed CSharp2 contract:

- Conditional symbols follow §2.5.1's identifier-or-keyword alternative, excluding verbatim `@` prefixes. Escaped/format-normalized boolean spellings remain symbol names rather than becoming literal booleans.
- `#line` uses decimal digits or raw `default`/`hidden`, with a nonempty optional filename; CSharp2's implementation-defined identifier alternative is not inherited. Directive grammar is checked in inactive regions. Historical separators, terminal Control-Z and Unicode4 whitespace retain original UTF16 positions.
- §7.5.4.2 adds `}` as a generic follower through a separate module and an empty extension seam. CSharp2 retains its reviewed finite follower table. The CSharp3 capability explicitly requires generics, and can be independently omitted from custom compositions.
- §17's attribute targets exclude unknown names and `typevar`. Reserved `event`/`return` require raw keyword spelling (§2.4.1); contextual identifier targets retain normalization. A separate frozen global-target module closes the current-Unicode token bypass. Full attribute argument expressions remain source syntax; binding and constant validity are excluded.
- Escaped contextual `partial` modifiers use normalized identifier identity in the existing declaration checks.

Regression witnesses reside in CSharp3UnicodeContractTests, CSharp3HistoricalDirectiveTests, CSharp3SourceDeltaTests and CSharp3SourceIndexTests. The reserved attribute target cases first failed 3/3, then passed after the CSharp3-only spelling guard. Earlier Unicode, directive, source-delta and contextual-partial failures are retained as tests. Coverage identity includes both embedded CSharp3 authority resources; independent drop/mutation cases verify their contribution.

Validation: `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false` passed 1,432 normal + 3,419 oracle tests = 4,851, with zero failures/skips. Both versioned global-target generators reproduce their committed bytes, the source-index `--check` succeeds, all 123 pinned Nitrogen dependency source hashes match, and `git diff --check` passes. A fresh whole-diff reviewer confirmed the reserved-keyword fix and found no remaining actionable issue in this slice.

CSharp1/CSharp2 admission stays intact. CSharp3 remains partial: full normative occurrence crosswalk, licensed external corpus and a separate fail-closed completeness gate remain pending. Later/custom profiles remain partial. No semantic compilation or portable DOC extraction is claimed.
