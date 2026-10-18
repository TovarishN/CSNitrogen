# CSharp3 generic-member source authority decision

Status: approved 2026-10-18. The user selected preservation of all three forms as bounded source corrections. Existing shared grammar/API acceptance is retained; full CSharp3 admission remains pending.

The approved Unicode4 implementation is merged in PR20. Continued Task4 source comparison found three CSharp3 primary grammar productions that omit generic argument suffixes accepted by reviewed CSharp2. The omission occurs in the main text as well as Appendix B; it is not an extraction artifact.

Primary: Microsoft Version3.0 DOC, SHA256 `ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1`. Physical line numbers below use the reproducible macOS LF-only extraction recorded by `docs/coverage/csharp3-standard-index.json`.

| Source | Literal grammar | Existing accepted witness |
| --- | --- | --- |
| §7.5.4 member-access, main3338 / Appendix8542 | The qualified-alias-member alternative ends with identifier, without optional type arguments. | `A::T.M<int>()` |
| §7.5.8 base-access, main3507 / Appendix8548 | `base . identifier`, without optional type arguments. | `base.M<int>()` |
| §18.5.2 pointer-member-access, main7872 / Appendix8856 | `primary-expression -> identifier`, without optional type arguments. | `p->M<int>()` |

The source prose points in the other direction: §7.5.4 (main3341) describes generic member access generally; §7.5.8 (3509) equates base access with a casted-this member access; §18.5.2 (7874,7880) equates pointer access with dereference/member access. Those descriptions support a correction, but do not add the omitted suffixes to the printed productions. This is a source-authority choice rather than a missing grammar implementation.

Executable observations in `CSharp3GenericMemberDecisionTests` retain three complete compilable units, including alias and unsafe declarations. All three have zero Roslyn5 CSharp2/CSharp3 compilation errors. Both selected Nitrogen factories accept each unit with the corresponding generic AST owner and exact source span. The CSharp2 source/gate remains reviewed; CSharp3 remains partial. Modern Roslyn is secondary evidence and does not by itself override the historical primary.

## Alternatives

1. **Preserve the three generic-member forms (recommended).** Record a bounded correction to the CSharp3 printed productions, supported by their member-access equivalences, reviewed CSharp2 grammar and compiler observations. Keep current shared Nitrogen grammar/API behavior. Crosswalk these specific source occurrences with an explicit correction disposition, rather than claiming literal correspondence.
2. **Follow the three printed productions literally.** Add a CSharp3-specific rejection policy for these generic-member owners/receiver shapes. CSharp2 continues accepting them. Record the deliberate narrowing between named versions and the discrepancy from Roslyn's CSharp3 mode. No semantic compiler or shared core change is needed.

The approval applies to these three forms only. Record their exact source occurrences as corrected in the CSharp3 crosswalk. Other historical discrepancies still require their own evidence; do not admit CSharp3 as complete until all source obligations close.
