# CSharp2 shared-seam feasibility

Test-only probes demonstrate an unpopulated extensible rule referenced by an optional typed field. Adding a module supplies the generic parameter alternative; omitting it rejects generic headers while retaining ordinary Header.Name and absent optional parameters. Generic parameter items remain typed and ordered. Existing ordinary QualifiedName.Parts is not modified.

Empty literals fail grammar parsing (NGR0001). A lookahead-only alternative is nullable and rejected (NGR0113), even if its predicates are contradictory. Neither workaround is needed: an empty extensible rule composes successfully in the pinned generator. No Nitrogen source changes were made.

The header extension tests failed two acceptance cases before adding the extension and passed six cases afterward. Additional follower/closer fixtures failed five cases before implementing the probe. The final seventeen cases cover optional headers, malformed tails, generic names, nested closers, equality/parenthesis followers, relational fallback and shift retention. `F<G<int>(x)` is explicitly retained as valid `F < G<int>(x)` syntax rather than filtered out as malformed.

Full solution: **816 normal + 3,245 oracle = 4,061 passed**, zero failures/skips. This is seam feasibility, not production CSharp2 coverage. Detailed typed delimiter, full follower-set and production context tests remain required by the generics slice.
