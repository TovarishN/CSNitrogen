#!/usr/bin/env python3
"""Build/check a CSharp4 owner candidate; normative approval and admission stay separate."""
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
index_path = 'docs/coverage/csharp4-standard-index.json'
delta_path = 'docs/coverage/csharp4-source-delta-inventory.json'
old_path = 'docs/coverage/csharp3-standard-crosswalk.json'
index, delta, old = read(index_path), read(delta_path), read(old_path)
assert delta['indexSha256'] == sha(index_path)
assert delta['sourceSha256'] == index['sourceSha256']
assert old['status'] == 'verified'
previous = {r['standardId']: r for r in old['productions']}
links = {r['standardId']: r for r in delta['productions']}
name_slot = 'CSharp.NamedArguments/extension:CSharp.Expressions.ArgumentName.NamedArgumentName'
default = 'CSharp.OptionalParameters/extension:CSharp.Attributes.ParameterDefault.DefaultValue'
variance = ['CSharp.Variance/extension:CSharp.Generics.VarianceAnnotation.Covariant',
            'CSharp.Variance/extension:CSharp.Generics.VarianceAnnotation.Contravariant']
parameters = 'CSharp.Generics/extension:CSharp.Declarations.TypeParameters.GenericParameters'
new = {
 'argument-name': [name_slot],
 'argument-value': ['CSharp.Expressions/rule:Argument'],
 'default-argument': [default],
 'variant-type-parameter-list': [parameters, 'CSharp.Generics/rule:TypeParameter'],
 'variant-type-parameters': [parameters, 'CSharp.Generics/rule:TypeParameter'],
 'variance-annotation': variance,
}
followers = ['CSharp.GenericFollowersFourth/extension:CSharp.Generics.AdditionalGenericFollower.BitOrGenericFollower',
             'CSharp.GenericFollowersFourth/extension:CSharp.Generics.AdditionalGenericFollower.BitXorGenericFollower']
extra = {
 'simple-name': followers,
 'pointer-member-access': followers,
 'argument': [name_slot],
 'element-access': ['CSharp.NamedArguments/extension:CSharp.Expressions.Expression.NamedElementExpression', 'CSharp.NamedArguments/rule:NamedIndexArguments', 'CSharp.NamedArguments/rule:NamedIndexArgument', 'CSharp.NamedArguments/rule:PositionalIndexArgument'],
 'base-access': ['CSharp.NamedArguments/extension:CSharp.Expressions.Expression.NamedBaseIndexExpression', 'CSharp.NamedArguments/rule:NamedIndexArguments', 'CSharp.Generics/extension:CSharp.Expressions.Expression.GenericBaseMemberExpression'],
 'member-access': followers + ['CSharp.Generics/extension:CSharp.Expressions.Expression.GenericMemberExpression', 'CSharp.Generics/extension:CSharp.Expressions.Expression.GenericPredefinedMemberExpression'],
 'fixed-parameter': [default],
 'interface-declaration': variance,
 'delegate-declaration': variance,
 'positional-argument': [name_slot],
}
context = set(new) | {'argument', 'element-access', 'base-access', 'fixed-parameter', 'interface-declaration', 'delegate-declaration', 'positional-argument'}
rows = []
for h in index['productions']:
 link = links[h['id']]
 prior = previous.get(link['predecessorId'])
 if prior is None and h['name'] not in new: raise SystemExit('Unmapped source occurrence: ' + h['id'])
 targets = list(prior['targets']) if prior else new[h['name']]
 targets = sorted(set(targets + extra.get(h['name'], [])))
 policies = list(prior['policies']) if prior else []
 if h['name'] in context: policies.append('csharp4.source-context-checks')
 row = dict(standardId=h['id'], sourceBodySha256=h['sourceBodySha256'], grammar=h['grammar'],
            mainTextOccurrences=h['mainTextOccurrences'], targets=targets, policies=sorted(set(policies)),
            mapping=prior['mapping'] if prior else 'normalized', reviewStatus='owner-candidate',
            predecessorId=link['predecessorId'], bodyRelation=link['bodyRelation'])
 if prior and 'sourceCorrection' in prior: row['sourceCorrection'] = prior['sourceCorrection']
 rows.append(row)
factory = next(l for l in read('src/CSNitrogen.Syntax/nitrogen.json')['languages'] if l['name'] == 'CSharp4')
result = dict(schemaVersion=1, profile='CSharp4', status='owner-indexed-unreviewed', sourceSha256=index['sourceSha256'],
 scope='Every source occurrence has candidate selected-factory owners/policies. Predecessor provenance and owner existence are not normative equivalence, whole context closure or source admission.',
 indexSha256=sha(index_path), deltaSha256=sha(delta_path), predecessorCrosswalkSha256=sha(old_path),
 factory=dict(name='CSharp4', start=factory['start'], grammars=factory['grammars']),
 sourceCorrections={'approved-generic-member-suffix': dict(status='approved-predecessor-continuity',
  decision=old['sourceCorrections']['approved-generic-member-suffix']['decision'],
  occurrences=[r['standardId'] for r in rows if 'sourceCorrection' in r])},
 productions=rows)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp4-standard-crosswalk.json'
if args.check:
 if not path.exists() or path.read_text() != output: raise SystemExit('Fresh owner crosswalk differs')
else: path.write_text(output)
print(len(rows), 'owner candidates;', sum(r['predecessorId'] is not None for r in rows), 'predecessor links; normative review pending')
