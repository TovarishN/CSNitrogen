# CSharp2 conformance closure

Approved scope: finish the existing additive CSharp2 design after merging PR16.

1. Preserve the merged 4,195-test baseline and source/dependency pins.
2. Pin Unicode 4.0; add generated scalar identifier categories and profile-specific lexical/preprocessor handling, with regressions before changes.
3. Review fourth-edition lexical, type/expression, statement/iterator, declaration/attribute and unsafe productions against normative prose; fix concrete grammar/context gaps with typed witnesses. Independent audits are read-only.
4. Produce a reproducible full source index and crosswalk with explicit ownership, exclusions, witnesses and review dimensions.
5. Admit CSharp2 completeness only through exact source, factory, module, feature, policy and witness guards; mutation-test fail-closed behavior. Later versions and custom profiles remain partial.
6. Run full verification and independent review; update evidence and publish an attached PR.

Keep binding, type validity, constant evaluation and execution semantics out of the syntax claim. Preserve CSharp1 behavior and all dependency/sibling state. Any newly required core/public-contract redesign is a decision gate.

## Approved type-operand revision

The user approved the generated operand wrapper after the isolated eight-case prototype. Add ConditionalTypeOperand + TypeOperatorOperand in Expressions; populate the conditional-specific non-nullable operand from NullableValues, preserving all other type contexts. Add an adjacent nullable-mark token, typed/span/omission witnesses and generated API migration notes. Refresh both source catalogs and the pinned CSharp2 source/factory ledger, then rerun complete verification and independent review.
