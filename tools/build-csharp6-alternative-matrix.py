#!/usr/bin/env python3
"""Link every sixth-edition alternative to an explicit witness executed on the CSharp6 factory.

A name-carried rule with the same number of alternatives as its single CSharp5 occurrence links each
alternative by position to the CSharp5 matrix case, reusing its exact witness. Every other alternative has
a sixth-specific witness from tools/csharp6_witnesses.py. Execution is in CSharp6AlternativeMatrixTests;
this record links witnesses and does not review their context. --check compares with the committed file.
"""
import argparse, hashlib, json, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from csharp6_witnesses import WITNESSES
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
def read(p): return json.loads((ROOT / p).read_text())
def sha(p): return hashlib.sha256((ROOT / p).read_bytes()).hexdigest()
INDEX, CROSS, FIFTH_INDEX, FIFTH = 'docs/coverage/csharp6-standard-index.json', 'docs/coverage/csharp6-standard-crosswalk.json', 'docs/coverage/csharp5-standard-index.json', 'tests/Corpus/CSharp5/alternative-witness-matrix.json'
index, cross, fifth_index, fifth = read(INDEX), read(CROSS), read(FIFTH_INDEX), read(FIFTH)
relation = {r['standardId']: r['relation'] for r in cross['productions']}
by5 = {}
for p in fifth_index['productions']: by5.setdefault(p['name'], []).append(p)
cases5 = {c['id']: c for c in fifth['cases']}
cases, specific, used = [], [], set()
for p in index['productions']:
    name = p['name'].replace('_', '-').lower(); preds = by5.get(name, [])
    positional = relation[p['id']] == 'name-carried' and len(preds) == 1 and len(preds[0]['alternatives']) == len(p['alternatives']) \
        and all(a['id'] in cases5 for a in preds[0]['alternatives'])
    if positional and p['name'] not in WITNESSES:
        for alt, old in zip(p['alternatives'], preds[0]['alternatives']):
            c = cases5[old['id']]
            cases.append(dict(id=alt['id'], sourceAlternativeSha256=alt['sourceAlternativeSha256'], sourceKind='predecessor-positional',
                              predecessorAlternative=old['id'], domain=c['domain'], witnessId=c['witnessId']))
        continue
    witnesses = WITNESSES.get(p['name'])
    if witnesses is None: raise SystemExit('No witness for ' + p['name'])
    if len(witnesses) != len(p['alternatives']): raise SystemExit(f"{p['name']}: {len(witnesses)} witnesses for {len(p['alternatives'])} alternatives")
    used.add(p['name'])
    for alt, (source, focus) in zip(p['alternatives'], witnesses):
        start = source.find(focus)
        if start < 0: raise SystemExit(f"{alt['id']}: focus not in source")
        specific.append(dict(id=alt['id'], sourceAlternativeSha256=alt['sourceAlternativeSha256'], alternative=alt['text'], source=source, focus=focus, focusStart=start))
        cases.append(dict(id=alt['id'], sourceAlternativeSha256=alt['sourceAlternativeSha256'], sourceKind='sixth-specific', domain='sixth-specific', witnessId=alt['id']))
unused = sorted(set(WITNESSES) - used)
if unused: raise SystemExit('Unused witnesses: ' + ', '.join(unused))
def write(rel, obj):
    text = json.dumps(obj, indent=1, ensure_ascii=False) + '\n'; path = ROOT / rel
    if args.check:
        if not path.exists() or path.read_text(encoding='utf-8') != text: raise SystemExit(rel + ' is stale')
    else: path.write_text(text, encoding='utf-8')
write('tests/Corpus/CSharp6/source-specific-cases.json', dict(schemaVersion=1, profile='CSharp6', primarySha256=index['primarySha256'],
      scope='Sixth-specific representative positives, one per alternative without a positional CSharp5 witness; not exhaustive inputs or semantic compilation.', cases=specific))
counts = {k: sum(1 for c in cases if c['sourceKind'] == k) for k in ('predecessor-positional', 'sixth-specific')}
write('tests/Corpus/CSharp6/alternative-witness-matrix.json', dict(schemaVersion=1, profile='CSharp6', status='source-witnesses-unreviewed', primarySha256=index['primarySha256'],
      scope='Every sixth-edition alternative linked to an explicit witness executed on the CSharp6 factory; positional CSharp5 links reuse exact cases. Context review is separate.',
      indexSha256=sha(INDEX), crosswalkSha256=sha(CROSS), predecessorMatrix=dict(path=FIFTH, sha256=sha(FIFTH)),
      sourceSpecific=dict(path='tests/Corpus/CSharp6/source-specific-cases.json', sha256=sha('tests/Corpus/CSharp6/source-specific-cases.json')),
      counts=dict(counts, total=len(cases)), cases=cases))
print('current' if args.check else counts, len(cases))
