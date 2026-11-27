# CSharp7.1 default literal implementation

1. Verify the release boundary and the existing typed default and switch label cases through the public parser.
2. Add `CSharp.DefaultLiterals` with a distinct token-valued expression node. Reserve a following parenthesis for the typed grammar, including across comments. Treat the new node as a primary expression in the syntax policy.
3. Register the capability starting at CSharp7.1 and regenerate the Nitride project definitions. Verify node spans, casts, direct composition and inheritance.
4. Refresh the shared pins and run the full suite with a disposable approval fixture. Put the exact ledger forward for approval.
