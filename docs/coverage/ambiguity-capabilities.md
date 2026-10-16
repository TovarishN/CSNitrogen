# Expression and composition capability evidence

Historical foundation/probe evidence. For the current implementation, see [C# 1 lexical/preprocessing evidence](csharp1-lexical-preprocessing-evidence.md).

Test grammars: `ExpressionProbe.ngr`, `GenericProbe.ngr`, and `LambdaProbe.ngr`. Generated compositions share a namespace and preserve separate membership.

Observed syntax roots are checked against Roslyn for addition/multiplication, parentheses, casts, relational comparison, shifts, contextual names, generic calls including nested generic closers, and simple lambdas. The selected cases have matching root kinds and source spans. Precedence tests additionally require a multiplication child under addition.

The base composition rejects `F<T>(x)` and `F<G<int>>(x)` in these probes; the generic composition accepts them and includes `Probe.Generic`. The lambda composition accepts `x => x`; the base composition rejects it. `from + select` remains an ordinary identifier expression with and without the lambda extension. Generic keywords such as `int` are handled as types rather than ordinary expression identifiers. This distinction was necessary to prevent the probe from incorrectly interpreting `F<G<int>>(x)` as relational/shift syntax.

These results do not establish arbitrary C# syntactic disambiguation, query parsing, dangling-else behavior, statement/declaration disambiguation, or binding-based interpretation. Those remain in the baseline/later feature inventory.

Depth cases at 16/64/128 parse balanced parentheses and reject malformed trailing operators. VSTest runs them in its testhost process with a 30-second hang timeout. They emit elapsed time and per-thread allocated-byte observations as a baseline, not a performance guarantee. Allocation measurements include language composition and recovery; they are not steady-state parser throughput.
