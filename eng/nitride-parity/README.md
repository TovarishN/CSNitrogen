# Nitride parse parity

Compares [Nitride](https://github.com/TovarishN/Nitride)'s parse of a CSNitrogen profile with Nitrogen's generated parser for the same composition. Both sides parse the raw composed grammar from `CompilationUnit`; preprocessing, Unicode policies and source checks are not applied. Each file must agree on acceptance, and accepted files on every node's rule, UTF-8 span and parent. Alias rules are omitted on both sides, as Nitride's observer omits them.

Nitride needs scoped parse context support (its `docs/scoped-parse-context.md`). Build its CLI, then run:

```sh
dotnet build eng/nitride-parity -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false
dotnet eng/nitride-parity/bin/Debug/net10.0/NitrideParity.dll /path/to/nitride CSharp5 tests/Corpus/Mono2008/*.cs
```

The tool prints `{profile, files, same, differences}` and exits nonzero on any difference. Rejected files also compare the reported offset. Nitride does not count progress inside a lookahead toward that offset, while Nitrogen does, so a rejection inside CSharp5's async-header lookahead can report an earlier offset. Acceptance and trees are unaffected. This is a development comparison, not a conformance claim.
