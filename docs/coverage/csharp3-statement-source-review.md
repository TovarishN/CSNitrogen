# CSharp3 statement and iterator source disposition

Primary: Microsoft C# Language Specification Version3.0 DOC, SHA256 `ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1`. Scope: all52 Appendix B statement occurrences B8624–8675, controlling §8 and iterator declaration restriction §10.14/main6617. Unsafe statements/stack allocation remain in the separate unsafe domain. This is source inspection with finite selected-CSharp3 evidence; overall crosswalk entries stay candidate/pending and CSharp3 stays partial.

## Statement categories, declarations and expressions: B8624–8640

Statements.Statement selects declaration, label or embedded form; EmbeddedStatement excludes declarations and labels, as main4618–4620 requires. Blocks own optional statement lists. A label prefixes a complete statement and cannot be the final bare token before a closing brace; an empty statement can supply its body. Duplicate label identity, scope/target resolution and reachability are excluded binding/control-flow work.

VariableDeclaration/VariableDeclarator/VariableInitializer and ConstantDeclaration/ConstantDeclarator preserve comma lists, optional variable initializers and required constant initializers. Array initializers are a local-variable initializer alternative. CSharp3 adds `var` to local-variable-type, also used by foreach; existing identifier type spelling recognizes it. Whether `var` names a declared type or requests inference requires binding. The implicit-type restrictions, best-common-type inference and declaration-space/self-reference checks are explicitly deferred with that identity decision; parser acceptance of a name is not inferred-type validation. General constant-expression value/type checks remain excluded.

StatementExpression normalizes the Pratt expression then CSN2001 limits it to invocation, object creation, assignment or pre/post increment/decrement. An initialized object creation retains the object-creation category, while anonymous object and implicit array creation do not become statement expressions. A parenthesized invocation is itself a parenthesized-expression and is not admitted as an invocation statement. The same category check applies to every expression in for initializer/iterator lists.

## Selection and iteration: B8641–8657

IfStatement chooses an optional else after its embedded body; main4742 requires the nearest eligible if owner. A selected-factory shape witness verifies the inner else and original ranges. SwitchStatement preserves zero or more sections; each section has one or more labels and a nonempty statement list. Multiple default labels are rejected by source policy; duplicate case values, governing types, constant evaluation and no-fall-through analysis are excluded semantic/control-flow checks.

While, do, for and foreach preserve their delimiters and embedded bodies. Do requires its final semicolon. For admits optional variable-declaration or statement-expression-list initializer, optional condition and optional iterator list. Foreach admits local-variable-type plus identifier, in and source expression. Boolean conversion, enumerator pattern lookup, collection/iteration variable types, readonly enforcement, loop execution and control-flow legality remain excluded. Syntax witnesses retain the empty forms and malformed separator/embedded-body neighbors.

## Jumps, handlers and resource statements: B8658–8674

JumpStatement owns the five keyword families and their required/optional expressions/labels. Break/continue/goto targets, return value compatibility/member/finally legality and rethrow containment retain the reviewed baseline semantic/control-flow exclusions. An accepted bare break or unresolved goto witness demonstrates this boundary; it is not a compiler-validity claim.

TryStatement requires catches or finally. Specific catch clauses may be followed by one general catch, and that general catch must be last, per main4978. Catch/Finally owners require blocks. Exception type denotation/inheritance and unreachable typed catches are binding checks. Checked/unchecked statements require blocks. Lock and using accept embedded bodies; using resource acquisition is a declaration or expression. Lock reference identity, disposal compatibility, readonly resources, required resource initialization and all resource/runtime effects retain the baseline deferred semantic boundary.

## Iterator context: B8675 and linked block/jump/handler owners

Iterators adds contextual yield return with expression and yield break, retaining yield as an ordinary identifier outside those forms (main5070). Main5072–5075 forbids either form outside method/operator/accessor bodies, inside anonymous functions and finally, and forbids yield return anywhere in a try containing catches. Existing CSN2022 syntax ancestry checks apply to actual CSharp3 trees. Yield break remains permitted in try/catch; yield return remains permitted in the try block of try/finally. A nested try's catches do not poison a later yield outside that try.

Main4657–4658 and6617 additionally forbid ordinary return, an explicit unsafe context and ref/out parameters in an iterator member. Function boundaries prevent ordinary returns/ref parameters in nested anonymous functions from being mistaken for members of the outer iterator. Lambdas are anonymous-function boundaries and cannot themselves contain yields. Constructors, destructors and event accessors are not iterator owners. Getter accessor bodies remain supported. Iterator interface/element identity, conversions, capture/state machines, enumeration and disposal execution remain excluded semantic work.

## Finite evidence and admission boundary

CSharp3StatementSourceTests retains accepted/malformed statement neighbors, iterator owner/mode/context cases, a nearest-else shape/range witness and an exact CSN2022 diagnostic range after a supplementary-character comment. All node/diagnostic ranges are bounded by original source coordinates in its acceptance/rejection matrix. CSharp3InheritedClosureTests separately reruns the reviewed second-version context cases in the actual third factory. Existing construction/statement category compiler observations retain the initialized versus anonymous/array creation distinction.

The crosswalk links all52 exact source occurrences to the embedded report and finite witness groups, with report hash/source-set/method-reference mutation checks. Attributed-method existence alone does not prove execution; recorded test runs provide that evidence. These common groups are not alternative-specific closure, arbitrary input/depth proof or semantic compilation. Declaration/unsafe dispositions and explicit remaining alternative obligations precede any separate CSharp3 source/factory/policy completeness gate.
