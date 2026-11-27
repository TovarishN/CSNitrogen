# CSharp7 expression-bodied members and event accessors

ECMA-334 seventh edition covers expression bodies for instance constructors, static constructors, finalizers, and property and indexer accessors. Its event-accessor productions in §15.8.1 and Annex A still require a `block` after `add` and `remove`. Both locations in the published PDF agree with the pinned committee text on those productions. Microsoft's programming guide shows expression-bodied event accessors. [Published seventh edition](https://ecma-international.org/wp-content/uploads/ECMA-334_7th_edition_december_2023.pdf) · [Pinned event grammar](https://github.com/dotnet/csharpstandard/blob/02d1a90122ece425cff575f47f197fb396be73d4/standard/classes.md#1581-general) · [Microsoft event examples](https://learn.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members#events)

## Observed version boundary

A Roslyn 5.0.0 compilation probe tested both language settings against equivalent CSNitrogen compilation-unit parses. The table shows compilation success for Roslyn and syntax acceptance for CSNitrogen. Roslyn emits CS8059 for each unavailable expression body in these C# 6 witnesses.

| Source form | Roslyn 6.0 | Roslyn 7.0 | CSNitrogen 6.0 | CSNitrogen 7.0 |
|---|---|---|---|---|
| Expression-bodied method or property | Accepts | Accepts | Accepts | Accepts |
| Instance constructor, with or without a constructor initializer | Rejects | Accepts | Rejects | Accepts |
| Static constructor or finalizer | Rejects | Accepts | Rejects | Accepts |
| Property or indexer `get` and `set` | Rejects | Accepts | Rejects | Accepts |
| Event `add` and `remove`, both expressed with arrows | Rejects | Accepts | Rejects | Rejects |
| Mixed block and arrow event accessors, with either accessor first | Rejects | Accepts | Rejects | Rejects |
| Event accessors, both expressed as blocks | Accepts | Accepts | Accepts | Accepts |

For the event witnesses, a class field `System.Action h` provides storage. The event type is `System.Action`. The arrow expressions use `h += value` for `add` and `h -= value` for `remove`. The block controls wrap the same statements in braces. [Executable witnesses](../../../tests/CSNitrogen.Oracle.Tests/CSharp7ExpressionBodyObservations.cs)

## Source decision

**Recommended.** Include expression-bodied event accessors from C# 7.0 as an explicit extension to the published grammar, consistent with the compiler and Microsoft examples. Keep the block-based event tree and its typed `EventAccessor.Body` property. Use additive syntax for events containing at least one arrow accessor, including mixed accessor bodies in either order. Production tests must also exercise the existing accessor-pair constraints and CSharp6 rejection.

**Alternative.** Retain the published block-only grammar. Record the Roslyn divergence and leave the arrow accessor spelling outside the CSharp7 syntax contract.
