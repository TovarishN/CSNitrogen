# Decision needed: complete historical #line mapping

ECMA-334 first edition §9.5.7 defines a numeric line-indicator as decimal-digits, optionally followed by a nonempty quoted filename, with no stated Int32/PDB cap. Existing LineMapping.LogicalLine and MappedLocation.LogicalLine are Int32. DirectiveEffects currently errors on #line 2147483648 and warns/ignores #line 16707566. Reproducing tests are PendingLexicalGapTests.Current_line_mapping_limit_is_exposed_for_the_pending_API_decision. Both observations remain unchanged by the independent grammar/lexical fixes.

Recommended design: retain Int32 physical offsets/physical line positions, add arbitrary-precision logical mapping metadata and an exact mapping query using BigInteger (BCL, no external dependency). Preserve raw directive spelling and original UTF-16 positions. Keep the current Int32 query as an explicitly checked conversion, with a Try form; never silently ignore a valid large directive or wrap its line value. Define one-based directive numbers separately from zero-based legacy display coordinates, including the grammar's zero spelling. Existing callers for normal-sized positive mappings retain their results. Tests must cover leading zeros, zero, Int32 edge, PDB edge and much larger values, reset/filename propagation and physical offsets.

Alternative: retain bounded Int32/PDB mapping and record exact logical mapping outside that range as unsupported coverage. Then the location-directive obligation remains open and CSharp1 cannot be declared complete for the pinned standard.

This affects the public preprocessing/mapping contract; it is not a routine grammar-production correction. The user must select the compatibility/API policy before implementation and before whole-corpus completion can be claimed.
