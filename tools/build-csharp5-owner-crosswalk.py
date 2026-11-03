#!/usr/bin/env python3
"""Build/check a CSharp5 owner candidate; normative approval and admission stay separate.

Appendix occurrences carry their admitted CSharp4 owners forward. Version5 adds async
and await only in the main text, so those productions are recorded separately with
their main-text line, the appendix occurrence that omits them and their owners.
Requires macOS textutil and the pinned Version5 DOCX; --check compares with the artifact.
"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('fifth_docx', type=Path)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
def read(path): return json.loads((ROOT / path).read_text())
def sha(path): return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()
index_path = 'docs/coverage/csharp5-standard-index.json'
delta_path = 'docs/coverage/csharp5-source-delta-inventory.json'
old_path = 'docs/coverage/csharp4-standard-crosswalk.json'
index, delta, old = read(index_path), read(delta_path), read(old_path)
assert delta['indexSha256'] == sha(index_path)
assert delta['sourceSha256'] == index['sourceSha256']
if hashlib.sha256(args.fifth_docx.read_bytes()).hexdigest() != index['sourceSha256']:
    raise SystemExit('Document does not match the pinned CSharp5 primary')
lines = subprocess.check_output(['/usr/bin/textutil', '-convert', 'txt', '-stdout', str(args.fifth_docx)]).decode('utf-8').split('\n')
previous = {r['standardId']: r for r in old['productions']}
links = {r['standardId']: r for r in delta['productions']}
rows = []
for h in index['productions']:
    link = links[h['id']]
    prior = previous.get(link['predecessorId'])
    if prior is None:
        raise SystemExit('Unmapped source occurrence: ' + h['id'])
    row = dict(standardId=h['id'], sourceBodySha256=h['sourceBodySha256'], grammar=h['grammar'],
               mainTextOccurrences=h['mainTextOccurrences'], targets=prior['targets'], policies=prior['policies'],
               mapping=prior['mapping'], reviewStatus='owner-candidate',
               predecessorId=link['predecessorId'], bodyRelation=link['bodyRelation'])
    if 'sourceCorrection' in prior: row['sourceCorrection'] = prior['sourceCorrection']
    rows.append(row)
await_owner = 'CSharp.AwaitExpressions/extension:CSharp.Expressions.Expression.AwaitExpression'
async_marker = 'CSharp.AsyncFunctions/extension:CSharp.AnonymousMethods.AsyncMarker.AsyncFunction'
policy = ['csharp5.source-context-checks']
additions_spec = [
    (4368, 'unary-expression', 'B:9825:unary-expression', [await_owner]),
    (4430, 'await-expression', None, [await_owner]),
    (5174, 'lambda-expression', 'B:9841:lambda-expression', [async_marker, 'CSharp.Lambdas/extension:CSharp.Expressions.Expression.LambdaExpression']),
    (5175, 'anonymous-method-expression', 'B:9842:anonymous-method-expression', [async_marker, 'CSharp.AnonymousMethods/extension:CSharp.Expressions.Expression.AnonymousMethodExpression']),
    (5728, 'statement-expression', 'B:9892:statement-expression', [await_owner, 'CSharp.Statements/rule:StatementExpression']),
    (6891, 'method-modifier', 'B:9975:method-modifier', ['CSharp.AsyncFunctions/extension:CSharp.Declarations.Modifier.AsyncModifier', 'CSharp.AsyncFunctions/extension:CSharp.Declarations.AsyncMemberStart.AsyncHeader', 'CSharp.Declarations/rule:MethodDeclaration']),
]
known = {h['id']: h for h in index['productions']}
additions = []
for line, name, appendix, targets in additions_spec:
    text = lines[line - 1]
    if not text.startswith(name + ':'):
        raise SystemExit(f'Main-text line {line} is not {name}')
    if appendix is not None and (appendix not in known or known[appendix]['name'] != name):
        raise SystemExit('Wrong appendix occurrence for ' + name)
    additions.append(dict(id=f'M:{line}:{name}', name=name, mainTextLine=line,
                          sourceLineSha256=hashlib.sha256(text.encode()).hexdigest(),
                          appendixOccurrence=appendix, targets=sorted(targets), policies=policy,
                          mapping='normalized', reviewStatus='owner-candidate'))
factory = next(l for l in read('src/CSNitrogen.Syntax/nitrogen.json')['languages'] if l['name'] == 'CSharp5')
result = dict(schemaVersion=1, profile='CSharp5', status='owner-indexed-unreviewed', sourceSha256=index['sourceSha256'],
 scope='Every Version5 appendix occurrence carries its admitted CSharp4 owners; main-text-only async/await productions list their selected-factory owners. Owner existence is not normative equivalence, whole context closure or source admission.',
 indexSha256=sha(index_path), deltaSha256=sha(delta_path), predecessorCrosswalkSha256=sha(old_path),
 factory=dict(name='CSharp5', start=factory['start'], grammars=factory['grammars']),
 sourceCorrections=old['sourceCorrections'],
 productions=rows, mainTextAdditions=additions)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp5-standard-crosswalk.json'
if args.check:
    if not path.exists() or path.read_text() != output: raise SystemExit('Fresh owner crosswalk differs')
else: path.write_text(output)
print(len(rows), 'owner candidates;', len(additions), 'main-text additions; normative review pending')
