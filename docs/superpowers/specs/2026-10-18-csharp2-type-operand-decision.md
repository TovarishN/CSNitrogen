# Decision: context-specific type operands for CSharp2

Status: approved by the user in this session. This is a generated public syntax-view contract change beyond the previously approved additive grammar seams.

PR16 is merged. Source audit, Unicode4, directive, alias, iterator, declaration and source-admission work is implemented on codex/csharp2-conformance. CSharp2 completeness must stay disabled until the nullable operand ambiguity below is closed and all verification reruns.

## Concrete problem

Both are valid source syntax and require different treatment of the same nullable-looking substring:

```csharp
x is int ? (a) : b
b ? new int?(1) : null
```

The original TypeReference greedily consumes `int?` in the first expression. A global lookahead on NullableType fixes that expression but rejects the constructor in the second. An additive shorter TypeReference alternative cannot win: Nitrogen extensible prefix selection takes the longest match. Tests retain both outcomes; no case can be discarded to admit completeness.

## Recommended bounded change

Add an empty ConditionalTypeOperand extension point and a TypeOperatorOperand wrapper in Expressions. Only the is/as operand fields use this wrapper. NullableValues contributes the non-nullable operand spelling guarded by the following conditional expression; other type contexts keep the existing TypeReference parser.

The isolated prototype in /tmp/csn-type-operand-prototype demonstrates constructor and ordinary/parenthesized is/as witnesses using real generated Nitrogen parsing. The prototype is separate from production before approval. Final acceptance also covers explicitly nullable is/as operands, nested generic nullable arguments, coalescing, casts, original spans, omission compositions and CSharp1 regression behavior.

Generated API impact: IsExpressionExpression.Type and AsExpressionExpression.Type change from TypeReference to TypeOperatorOperandNode. Consumers that inspect the operand must unwrap its Value and discriminate the normalized alternative. Existing TypeReference and declaration type APIs remain. This affects the shared generated expression view API, including CSharp1 consumers; publish migration notes and version the parser package accordingly if a release is made. No Nitrogen core change or sibling-repository write is needed.

## Alternative

Extend Nitrogen core with context-specific parsing that preserves the existing generated operand API. This requires a separate core design, parity evidence and explicit authorization to modify the sibling Nitrogen repository. No existing core context facility was established during this review.

## Completion gate

After approval: implement the selected design, retain all regression witnesses, refresh the reviewed AST/source hashes and source records, run full normal/oracle and mutation suites, obtain independent review, and publish the closure PR. CSharp3–14/custom remain partial; semantic compilation remains excluded.
