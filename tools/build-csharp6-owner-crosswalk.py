#!/usr/bin/env python3
"""Link every sixth-edition grammar rule to its owners in the actual CSharp6 factory.

A rule whose name (underscores as hyphens, lowercase) exists in the admitted CSharp5 index carries
the union of its CSharp5 crosswalk owners. Every other rule is listed below explicitly: an ECMA
renaming names its CSharp5 predecessors, and a sixth-edition addition names its sixth-module owners.
Owner existence is checked against the factory grammars by CSharp6OwnerCrosswalkTests; this record
is owner evidence, not normative equivalence or admission. --check compares with the committed file.
"""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
def read(path): return json.loads((ROOT / path).read_text())
def sha(path): return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()
INDEX, FIFTH_INDEX, FIFTH = 'docs/coverage/csharp6-standard-index.json', 'docs/coverage/csharp5-standard-index.json', 'docs/coverage/csharp5-standard-crosswalk.json'
index, fifth_index, fifth = read(INDEX), read(FIFTH_INDEX), read(FIFTH)
P6 = 'csharp6.source-context-checks'
I = 'CSharp.InterpolatedStrings/'
E = 'CSharp.Expressions.Expression.'
interp = [I + 'extension:' + E + n for n in ('InterpolatedRegularText', 'InterpolatedRegularString', 'InterpolatedVerbatimText', 'InterpolatedVerbatimString')]
NC = 'CSharp.NullConditional/extension:' + E
# name -> (CSharp5 predecessor names, sixth owners, policies, note)
EXPLICIT = {
 'default': (['keyword'], [], [], 'ECMA token rule factored out of keyword'),
 'null': (['null-literal'], [], [], 'ECMA token rule for the null literal'),
 'true': (['boolean-literal'], [], [], 'ECMA token rule for the boolean literal'),
 'false': (['boolean-literal'], [], [], 'ECMA token rule for the boolean literal'),
 'asterisk': (['operator-or-punctuator'], [], [], 'ECMA token rule factored out of operator_or_punctuator'),
 'slash': (['operator-or-punctuator'], [], [], 'ECMA token rule factored out of operator_or_punctuator'),
 'comma': (['dim-separators'], [], [], 'ECMA rank-specifier comma; replaces dim-separators'),
 'simple-identifier': (['identifier', 'available-identifier'], [], [], 'ECMA lexical identifier; contextual keywords are separated out'),
 'escaped-identifier': (['identifier'], [], [], 'the @ form of identifier'),
 'basic-identifier': (['identifier-or-keyword'], [], [], 'renamed identifier-or-keyword'),
 'underscore-character': (['connecting-character'], [], [], 'underscore and its \\u005F spelling'),
 'contextual-keyword': (['identifier'], [], [], 'contextual keywords remain identifiers in the factory'),
 'pp-kind': (['pp-directive'], [], [], 'ECMA preprocessing directive dispatch'),
 'pp-start': (['pp-directive'], [], [], 'directive start position and #'),
 'pp-whitespace': (['whitespace'], [], [], 'directive whitespace'),
 'pp-conditional-symbol': (['conditional-symbol'], [], [], 'renamed conditional-symbol'),
 'pp-line-indicator': (['line-indicator'], [], [], 'renamed line-indicator'),
 'pp-compilation-unit-name': (['file-name'], [], [], 'renamed file-name'),
 'pp-compilation-unit-name-character': (['file-name-character'], [], [], 'renamed file-name-character'),
 'pp-pragma-text': (['pragma-body'], [], [], 'ECMA leaves pragma text unstructured'),
 'nullable-value-type': (['nullable-type'], [], [], 'renamed nullable-type'),
 'parameter-mode-modifier': (['parameter-modifier'], [], [], 'ref/out factored out of parameter-modifier'),
 'finalizer-declaration': (['destructor-declaration'], [], [], 'renamed destructor-declaration'),
 'finalizer-body': (['destructor-body'], [], [], 'renamed destructor-body'),
 'integral-type-name': (['enum-base'], [], [], 'ECMA allows an integral type name in enum-base'),
 'unsafe-modifier': (['class-modifier'], [], [], 'unsafe factored out of each modifier list'),
 'exception-specifier': (['specific-catch-clause'], [], [], 'factored out of specific-catch-clause'),
 'await-expression': ([], ['CSharp.AwaitExpressions/extension:' + E + 'AwaitExpression'], ['csharp5.source-context-checks'], 'appendix-omitted in Version5, present in the sixth annex'),
 'interpolated-string-expression': ([], interp, [P6], 'sixth addition'),
 'interpolated-regular-string-expression': ([], interp[:2], [P6], 'sixth addition'),
 'interpolated-verbatim-string-expression': ([], interp[2:], [P6], 'sixth addition'),
 'regular-interpolation': ([], [I + 'rule:Interpolation', I + 'rule:InterpolationWidth'], [P6], 'sixth addition'),
 'verbatim-interpolation': ([], [I + 'rule:Interpolation', I + 'rule:InterpolationWidth'], [P6], 'sixth addition'),
 'interpolation-minimum-width': ([], [I + 'rule:InterpolationWidth'], [], 'sixth addition'),
 'interpolated-regular-string-start': ([], [I + 'rule:RegularOpen', I + 'rule:RegularWhole'], [], 'start delimiter inside the open/whole token'),
 'interpolated-regular-string-mid': ([], [I + 'rule:RegularOpen', I + 'rule:RegularMid', I + 'rule:RegularClose', I + 'rule:RegularWhole'], [], 'text inside the delimiter tokens'),
 'regular-interpolation-format': ([], [I + 'rule:InterpolationFormat', I + 'rule:RegularFormatText'], [P6], 'sixth addition'),
 'interpolated-regular-string-end': ([], [I + 'rule:RegularClose', I + 'rule:RegularWhole'], [], 'end delimiter inside the close/whole token'),
 'interpolated-regular-string-element': ([], [I + 'rule:RegularElement'], [], 'sixth addition'),
 'interpolated-regular-string-character': ([], [I + 'rule:RegularElement'], [], 'sixth addition'),
 'interpolated-verbatim-string-start': ([], [I + 'rule:VerbatimOpen', I + 'rule:VerbatimWhole'], [], 'start delimiter inside the open/whole token'),
 'interpolated-verbatim-string-mid': ([], [I + 'rule:VerbatimOpen', I + 'rule:VerbatimMid', I + 'rule:VerbatimClose', I + 'rule:VerbatimWhole'], [], 'text inside the delimiter tokens'),
 'verbatim-interpolation-format': ([], [I + 'rule:InterpolationFormat', I + 'rule:VerbatimFormatText'], [P6], 'sixth addition'),
 'interpolated-verbatim-string-end': ([], [I + 'rule:VerbatimClose', I + 'rule:VerbatimWhole'], [], 'end delimiter inside the close/whole token'),
 'interpolated-verbatim-string-element': ([], [I + 'rule:VerbatimElement'], [], 'sixth addition'),
 'interpolated-verbatim-string-character': ([], [I + 'rule:VerbatimElement'], [], 'sixth addition'),
 'open-brace-escape-sequence': ([], [I + 'rule:RegularElement', I + 'rule:VerbatimElement'], [], 'sixth addition'),
 'close-brace-escape-sequence': ([], [I + 'rule:RegularElement', I + 'rule:VerbatimElement'], [], 'sixth addition'),
 'null-conditional-member-access': ([], [NC + 'NullConditionalAccess', 'CSharp.Expressions/alternative:Expression.MemberExpression'], [], 'dependent accesses are ordinary postfix nodes'),
 'dependent-access': ([], ['CSharp.Expressions/alternative:Expression.MemberExpression', 'CSharp.Expressions/alternative:Expression.ElementExpression', 'CSharp.Expressions/alternative:Expression.InvocationExpression'], [], 'ordinary postfix nodes over the null-conditional node'),
 'null-conditional-projection-initializer': ([], [NC + 'NullConditionalAccess', 'CSharp.AnonymousTypes/rule:InferredMember'], ['generated-syntax-context-checks'], 'sixth addition'),
 'null-conditional-invocation-expression': ([], [NC + 'NullConditionalAccess', NC + 'NullConditionalElementAccess', 'CSharp.Expressions/alternative:Expression.InvocationExpression'], [], 'sixth addition'),
 'null-conditional-element-access': ([], [NC + 'NullConditionalElementAccess'], [], 'sixth addition'),
 'initializer-target': (['member-initializer'], ['CSharp.IndexInitializers/rule:IndexInitializer', 'CSharp.IndexInitializers/extension:CSharp.ObjectInitializers.InitializerExtension.IndexedObjectInitializer'], [], 'identifier target carried; index target added'),
 'nameof-expression': ([], ['CSharp.NameofExpressions/extension:' + E + 'NameofExpression', 'CSharp.NameofExpressions/extension:' + E + 'NameofInvocation'], [P6], 'Ambiguous with the invocation reading where both parse'),
 'named-entity': ([], ['CSharp.NameofExpressions/rule:NamedEntity', 'CSharp.NameofExpressions/rule:NamedEntityPart'], [], 'sixth addition'),
 'named-entity-target': ([], ['CSharp.NameofExpressions/rule:NamedEntityTarget'], [], 'sixth addition'),
 'exception-filter': ([], ['CSharp.ExceptionFilters/extension:CSharp.Statements.CatchFilter.ExceptionFilter', 'CSharp.Statements/rule:CatchFilter'], [], 'sixth addition'),
 'using-static-directive': (['using-namespace-directive'], ['CSharp.UsingStatic/extension:CSharp.CompilationUnits.UsingKind.UsingStaticMarker', 'CSharp.CompilationUnits/rule:UsingKind'], [], 'a using directive with the static marker'),
 'property-body': (['property-declaration'], ['CSharp.PropertyInitializers/extension:CSharp.Declarations.PropertyInitializerSeam.PropertyInitializer', 'CSharp.ExpressionBodies/extension:CSharp.Declarations.MemberDeclarationExtension.ExpressionBodiedProperty'], [P6], 'accessor block carried; initializer and expression body added'),
 'property-initializer': ([], ['CSharp.PropertyInitializers/extension:CSharp.Declarations.PropertyInitializerSeam.PropertyInitializer', 'CSharp.Declarations/rule:PropertyInitializerSeam'], [P6], 'sixth addition'),
 'indexer-body': (['indexer-declaration'], ['CSharp.ExpressionBodies/extension:CSharp.Declarations.MemberDeclarationExtension.ExpressionBodiedIndexer', 'CSharp.ExpressionBodies/rule:ExpressionClause'], [], 'accessor block carried; expression body added'),
}
CHANGED = {  # carried names whose sixth body adds alternatives owned by sixth modules
 'method-body': ['CSharp.ExpressionBodies/extension:CSharp.Declarations.MethodBodyExtension.ExpressionBody', 'CSharp.Declarations/rule:MethodBodyExtension'],
 'operator-body': ['CSharp.ExpressionBodies/extension:CSharp.Declarations.MethodBodyExtension.ExpressionBody'],
 'primary-no-array-creation-expression': interp + [NC + 'NullConditionalAccess', NC + 'NullConditionalElementAccess', 'CSharp.NameofExpressions/extension:' + E + 'NameofExpression'],
 'specific-catch-clause': ['CSharp.ExceptionFilters/extension:CSharp.Statements.CatchFilter.ExceptionFilter'],
 'general-catch-clause': ['CSharp.ExceptionFilters/extension:CSharp.Statements.CatchFilter.ExceptionFilter'],
 'using-directive': ['CSharp.UsingStatic/extension:CSharp.CompilationUnits.UsingKind.UsingStaticMarker'],
 'member-initializer': ['CSharp.IndexInitializers/rule:IndexInitializer'],
 'member-declarator': [NC + 'NullConditionalAccess'],
 'statement-expression': [NC + 'NullConditionalAccess', NC + 'NullConditionalElementAccess'],
}
names5 = {}
for p in fifth_index['productions']:
    names5.setdefault(p['name'], []).append(p['id'])
by_id = {r['standardId']: r for r in fifth['productions']}
additions5 = {}
for a in fifth['mainTextAdditions']:
    additions5.setdefault(a['name'], []).append(a)
def carried(name):
    targets, policies = set(), set()
    for occurrence in names5.get(name, []):
        targets |= set(by_id[occurrence]['targets']); policies |= set(by_id[occurrence]['policies'])
    for a in additions5.get(name, []):
        targets |= set(a['targets']); policies |= set(a['policies'])
    return targets, policies
rows = []
for p in index['productions']:
    name = p['name'].replace('_', '-').lower()
    if name in EXPLICIT:
        predecessors, owners, policies, note = EXPLICIT[name]
        for pred in predecessors:
            if pred not in names5: raise SystemExit(f'{name}: unknown CSharp5 predecessor {pred}')
        targets, inherited = set(owners), set(policies)
        for pred in predecessors:
            t, q = carried(pred); targets |= t; inherited |= q
        relation = 'renamed' if predecessors and not owners else 'sixth-addition' if not predecessors else 'extended'
    elif name in names5:
        if name in EXPLICIT: raise SystemExit('ambiguous mapping ' + name)
        predecessors, note = [name], ''
        targets, inherited = carried(name)
        relation = 'name-carried'
        if name in CHANGED:
            targets |= set(CHANGED[name]); inherited.add(P6); relation = 'name-carried-extended'
    else:
        raise SystemExit('Unmapped sixth rule: ' + p['name'])
    if not targets and not inherited:
        raise SystemExit('No owner or policy for ' + p['name'])
    rows.append(dict(standardId=p['id'], name=p['name'], grammar=p['grammar'], sourceBodySha256=p['sourceBodySha256'],
                     relation=relation, predecessors=[o for n in predecessors for o in names5.get(n, [])],
                     targets=sorted(targets), policies=sorted(inherited), note=note, reviewStatus='owner-candidate'))
unused = sorted(set(EXPLICIT) - {r['name'].replace('_', '-').lower() for r in rows}) + sorted(set(CHANGED) - set(names5))
if unused: raise SystemExit('Unused explicit mappings: ' + ', '.join(unused))
factory = next(l for l in read('src/CSNitrogen.Syntax/nitrogen.json')['languages'] if l['name'] == 'CSharp6')
dropped = sorted(n for n in names5 if n not in {r['name'].replace('_', '-').lower() for r in rows} and n not in {p for v in EXPLICIT.values() for p in v[0]})
result = dict(schemaVersion=1, profile='CSharp6', status='owner-indexed-unreviewed', primarySha256=index['primarySha256'],
 scope='Every sixth-edition grammar rule with its owners in the actual CSharp6 factory; owner existence is not normative equivalence or admission.',
 indexSha256=sha(INDEX), predecessorIndexSha256=sha(FIFTH_INDEX), predecessorCrosswalkSha256=sha(FIFTH),
 factory=dict(name='CSharp6', start=factory['start'], grammars=factory['grammars']),
 counts={r: sum(1 for x in rows if x['relation'] == r) for r in sorted({x['relation'] for x in rows})},
 fifthNamesNotInSixth=dropped, productions=rows)
output = json.dumps(result, indent=1, ensure_ascii=False) + '\n'
path = ROOT / 'docs/coverage/csharp6-standard-crosswalk.json'
if args.check:
    if not path.exists() or path.read_text() != output: raise SystemExit('Fresh owner crosswalk differs')
    print('current')
else:
    path.write_text(output); print(result['counts'], 'fifth-only', len(dropped))
