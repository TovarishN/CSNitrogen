# Generated is/as operand migration

The user approved this bounded generated API change in 2026-10-18-csharp2-type-operand-decision.md. It applies to the shared expression views, including CSharp1 consumers.

`IsExpressionExpression.Type` and `AsExpressionExpression.Type` now return `TypeOperatorOperandNode`, rather than a direct `TypeReference` view. Its `Value` is the parsed operand. Ordinary cases retain the original TypeReference child and authored span. A conditional-specific non-nullable operand is represented by `NonNullableConditionalOperandConditionalTypeOperand`; its `Type` field owns the source type spelling.

Consumers that used the operand's source text/span can use `expression.Type.Value`. Consumers that switch on type alternatives must first distinguish the conditional-specific operand, then inspect its `Type` or unwrap the ordinary TypeReference. Code depending on exact CSharp1 tree nesting must account for the new wrapper. Declaration type views and TypeReference remain unchanged.

The scope is necessary because `x is T ? (a) : b` and `b ? new T?(a) : null` have identical suffixes after `T?` but require different ownership of the question mark. The scoped is/as rule chooses the correct operand without restricting nullable constructors. Nullable punctuation uses a token guard so `int? ? a : b` distinguishes separated conditional punctuation from adjacent `??`.

No Nitrogen core changes or runtime handwritten C# parser were introduced. A future package release must disclose this generated API change and use appropriate versioning.
