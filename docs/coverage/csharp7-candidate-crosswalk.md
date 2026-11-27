# CSharp7 annex crosswalk

The [annex crosswalk](csharp7-candidate-crosswalk.json) links the sixth- and seventh-edition rule and alternative IDs by name and normalized text. It matches 945 alternatives in unchanged bodies and 130 inside changed bodies. For unchanged bodies, each rule pair links its alternatives by ordinal. For changed bodies, the report lists every matching alternative pair explicitly. The remaining 146 seventh-edition alternatives need separate review. An identical spelling can still acquire a different meaning when it refers to a changed rule or when the surrounding prose changes.

The sixth-edition `local_variable_declarator` and `local_variable_declarators` names disappear because §13.6.2 refactors local declarations into explicit, inferred and ref forms. The explicit declarator retains the old optional-initializer shape and the explicit declarator list retains the comma-separated repetition. Their replacement rules need their own witnesses alongside the inferred and ref forms. [Seventh-edition source contract](csharp7-source-contract.md)

Run `python3 tools/build-csharp7-candidate-crosswalk.py --check` to reproduce the crosswalk against the pinned indices.
