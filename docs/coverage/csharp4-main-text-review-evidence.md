# CSharp4 main-text review evidence

Base: merged PR44 c08efd84b81a14e1dc11bb230b9a7f171978c138, tree354469401fbc7e65e4017dd61ed69c1523f0a443. No production parser/grammar/admission behavior changes in this slice. CSharp4 remains partial.

The SHA-pinned external third/fourth DOCs produce268 normalized pre-appendix difference regions. Fresh comparison reproduces the exact source-line bounds from the inspected analysis. The repository stores bounds and hashes rather than source prose. Normalization is explicit, including LF identity, Word fields and TOC/blank/header exclusion; the comparison is not proof of normative equivalence.

The separate authored disposition table links every region to a source-inspection category and pins the normalized-context report. That report covers the458 occurrence domains, concrete fourth-version syntax changes and inherited normalization/semantic exclusions. The generic-follower and base-projection prose differences are explicitly retained as implemented syntax, not silently classified as semantics. Every other changed region is accounted for, including mixed named-argument/default/variance sections and changed semantic requirements that remain excluded.

Seventeen review integrity tests pass, including16 mutation families for source/index/region/disposition/count/definition/report/status drift. These validate record integrity, not the truth of semantic conclusions; fresh source review is a separate gate. No production RED claim is made. Full solution:6,162 normal plus3,774 oracle =9,936 passed, zero failures/skips. Command: `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false`.

Reproduce comparison: `python3 tools/compare-csharp4-main-text.py /path/to/pinned-third.doc /path/to/pinned-fourth.doc --check`. Independent appendix extraction, alternative matrix and owner crosswalk checks also reproduce unchanged results. Whole-source admission, publisher-byte authenticity, semantic compilation/runtime, complete later profiles and arbitrary runtime factories remain outside this evidence slice. Unrelated benchmarks remain preserved.
