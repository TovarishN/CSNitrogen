# CSharp7.2 non-trailing named index arguments

1. Verify that calls, constructor arguments and attribute constructor arguments already use the 7.2 release boundary. Compare ordinary, base and conditional indexing and index initializers.
2. Add a selectable 7.2 grammar module for ordinary and base indexers whose argument order includes a name followed by a positional value. Preserve the existing positional and trailing-named trees and retain ordered typed arguments for the new forms.
3. Use the nearest argument-list owner to enforce source-order and duplicate-name rules. Verify nested-owner isolation, earlier-profile rejection, and unchanged attribute member-assignment order. Parameter binding stays outside the parser.
4. Regenerate the profile projects, refresh admission pins, and run the full corpus in a disposable snapshot before publishing the candidate.
