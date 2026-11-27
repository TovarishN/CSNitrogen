# CSharp7.2 private protected

1. Verify the missing accessor spellings and excess acceptance of the member pair with pre-change public-parser witnesses.
2. Extend `AccessorModifiers` with a distinct two-token alternative in either order. Keep existing modifier nodes and the protected-internal accessor alternative.
3. Apply the pre-7.2 declaration check to one modifier list at a time. Check the pair even when non-access modifiers intervene. Report the later token and preserve existing general modifier-set exclusions.
4. Register the grammar and coverage capability, regenerate the profile projects, and verify the original and new modifier cases, declaration isolation, and CSharp6 admission in a disposable snapshot.
