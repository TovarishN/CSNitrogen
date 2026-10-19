#!/usr/bin/env python3
"""Build/check explicit CSharp4 alternative-to-witness provenance, without copying old cases."""
import argparse
import hashlib
import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check',action='store_true')
args=parser.parse_args()
def read(path):return json.loads((ROOT/path).read_text())
def sha(path):return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
index=read('docs/coverage/csharp4-standard-index.json')
delta=read('docs/coverage/csharp4-source-delta-inventory.json')
if delta['indexSha256']!=sha('docs/coverage/csharp4-standard-index.json'):raise SystemExit('Candidate comparison uses another index')
specific=read('tests/Corpus/CSharp4/source-specific-cases.json')
if specific['sourceSha256']!=index['sourceSha256']:raise SystemExit('Source-specific cases use another source')
novel={c['id']:c for c in specific['cases']}
if len(novel)!=len(specific['cases']):raise SystemExit('Duplicate source-specific case')
files={domain:f'tests/Corpus/CSharp3/{domain}-production-cases.json' for domain in ['lexical','expression','statement','unsafe','declaration']}
files['finite']='tests/Corpus/CSharp3/finite-alternative-cases.json'
old={}
for domain,path in files.items():
 for case in read(path)['cases']:
  if case['id'] in old:raise SystemExit('Duplicate predecessor case identity')
  old[case['id']]=(domain,case)
links={c['alternativeId']:c for row in delta['productions'] for c in row['alternativeCandidates']}
cases=[]
for row in index['productions']:
 for alternative in row['alternatives']:
  if alternative['kind']=='inherited-placeholder':continue
  id=alternative['id'];candidates=links[id]['predecessorAlternativeCandidates']
  if candidates:
   if len(candidates)!=1 or candidates[0] not in old:raise SystemExit('Ambiguous or missing predecessor witness: '+id)
   witness=candidates[0];domain,case=old[witness]
   if case['sourceAlternativeSha256']!=alternative['sourceAlternativeSha256']:raise SystemExit('Predecessor alternative hash differs: '+id)
   kind='predecessor-corpus'
  else:
   if id not in novel:raise SystemExit('Missing source-specific witness: '+id)
   case=novel[id]
   if case['sourceAlternativeSha256']!=alternative['sourceAlternativeSha256']:raise SystemExit('Source-specific alternative hash differs: '+id)
   witness=id;domain='source-specific';kind='source-specific'
  cases.append(dict(id=id,sourceOccurrence=row['id'],sourceAlternativeSha256=alternative['sourceAlternativeSha256'],sourceKind=kind,domain=domain,witnessId=witness))
used={c['id'] for c in cases if c['sourceKind']=='source-specific'}
if used!=set(novel):raise SystemExit('Extra source-specific witness')
result=dict(schemaVersion=1,profile='CSharp4',status='source-witnesses-unreviewed',sourceSha256=index['sourceSha256'],
 scope='Representative source4 alternative identities linked to exact inherited or source-specific corpus cases. Execution, source context review and eventual admission are separate gates.',
 indexSha256=sha('docs/coverage/csharp4-standard-index.json'),deltaSha256=sha('docs/coverage/csharp4-source-delta-inventory.json'),
 predecessorCorpora={d:dict(path=p,sha256=sha(p)) for d,p in files.items()},
 sourceSpecific=dict(path='tests/Corpus/CSharp4/source-specific-cases.json',sha256=sha('tests/Corpus/CSharp4/source-specific-cases.json')),
 counts=dict(production=sum(a['kind']=='production' for r in index['productions'] for a in r['alternatives']),finite=sum(a['kind']=='one-of-token' for r in index['productions'] for a in r['alternatives']),predecessor=sum(c['sourceKind']=='predecessor-corpus' for c in cases),sourceSpecific=len(used)),cases=cases)
output=json.dumps(result,indent=2)+'\n'
p=ROOT/'tests/Corpus/CSharp4/alternative-witness-matrix.json'
if args.check:
 if not p.exists() or p.read_text()!=output:raise SystemExit('Fresh matrix differs from the committed matrix')
else:p.write_text(output)
print(len(cases),'alternative witnesses:',result['counts'])
