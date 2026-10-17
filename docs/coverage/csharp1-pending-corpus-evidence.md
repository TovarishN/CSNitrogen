# Pending C# 1 corpus continuation — 2026-10-17

PR #14 was merged at `0095070fbf7fbfee2cc268cc665507e0b86f9cbb`. This continuation is on `test/csharp1-pending-corpus`. All profiles remain partial; the existing crosswalk stays at 31 verified and 331 pending productions. No review obligations are promoted here.

The first-edition lexical/directive corrections include terminal Control-Z handling with stable UTF-16 offsets; historical newline handling excluding NEL; raw NEL in historical regular string/character literals; literal versus escaped underscore at identifier start; required directive argument whitespace; nonempty mapped filenames; rejection of historical `#line hidden`; and lexical validation of inactive historical line directives. Current-profile NEL and directive behavior remains covered separately. Shared lexical grammar is a superset with version policy checks.

Assignment target syntax now requires a unary-expression root. Binary and conditional roots fail CSN2013 at the authored target span. Parenthesized expressions, invocations and increment targets remain admitted as grammar forms; writability is excluded semantic work. Current-profile raw NEL literals report CSN2021. Both codes are included in the coverage diagnostic inventory.

The focused production regression run failed 15 cases before changes and passed all 25 afterward. The initial full run exposed nine modern assumptions in historical fixtures. Those cases were preserved through explicit version selection or historical-versus-current difference tests, including oracle disagreement. The final full run passed **731 normal + 2650 oracle = 3381 tests**, without skips, using `dotnet test CSNitrogen.slnx --no-restore -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false --blame-hang-timeout 30s`. Lexical generation was rerun from the pinned category data and `git diff --check` passed.

The three independent read-only audits stopped at an account usage limit before delivering complete audits; their partial findings informed reproductions but do not constitute final review. Remaining crosswalk corrections and expression/declaration/statement audits are unfinished. No complete-corpus or fresh independent review claim is made.

A public mapping-contract decision is required before the numeric location-directive obligation can close. See [the concrete proposal](csharp1-line-mapping-decision.md). Existing Int32/PDB limit behavior is retained and characterized by tests pending that decision.
