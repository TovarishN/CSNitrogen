# CSharp7 throw expressions

1. Test 7.0 versus 6.0 for the four allowed parent roles in ECMA-334 §12.16. Include a historical throw statement control.
2. Add the optional `CSharp.ThrowExpressions` module. Parse the throw operand at the precedence floor that includes coalescing and stops before the conditional operator. Preserve the typed expression and the existing throw statement.
3. Enforce the standard's direct parent roles in the host syntax policy. Test precedence, both conditional operands, coalescing, expression-bodied lambdas and members, and prohibited roles including parenthesized throws and arguments. Keep exception conversion outside the syntax policy.
4. Regenerate the named profiles, verify module selection and dependency pins, and run the full suite in a disposable copy before publishing the exact admission candidate.
