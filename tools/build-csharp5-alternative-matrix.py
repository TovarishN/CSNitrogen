#!/usr/bin/env python3
"""Build/check explicit CSharp5 alternative-to-witness provenance through the admitted CSharp4 matrix."""
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
index=read('docs/coverage/csharp5-standard-index.json')
delta=read('docs/coverage/csharp5-source-delta-inventory.json')
if delta['indexSha256']!=sha('docs/coverage/csharp5-standard-index.json'):raise SystemExit('Candidate comparison uses another index')
fourth_path='tests/Corpus/CSharp4/alternative-witness-matrix.json'
fourth=read(fourth_path)
if fourth['indexSha256']!=delta['predecessor']['indexSha256']:raise SystemExit('Predecessor matrix uses another CSharp4 index')
inherited={c['id']:c for c in fourth['cases']}
specific_path='tests/Corpus/CSharp5/source-specific-cases.json'
specific=read(specific_path)
if specific['sourceSha256']!=index['sourceSha256']:raise SystemExit('Source-specific cases use another source')
novel={c['id']:c for c in specific['cases']}
if len(novel)!=len(specific['cases']):raise SystemExit('Duplicate source-specific case')
links={c['alternativeId']:c for row in delta['productions'] for c in row['alternativeCandidates']}
cases=[]
for row in index['productions']:
 for alternative in row['alternatives']:
  if alternative['kind']=='inherited-placeholder':continue
  id=alternative['id'];candidates=links[id]['predecessorAlternativeCandidates']
  if candidates:
   if len(candidates)!=1 or candidates[0] not in inherited:raise SystemExit('Ambiguous or missing predecessor witness: '+id)
   previous=inherited[candidates[0]]
   if previous['sourceAlternativeSha256']!=alternative['sourceAlternativeSha256']:raise SystemExit('Predecessor alternative hash differs: '+id)
   # Fourth source-specific witnesses keep their own corpus; inherited third corpora keep theirs.
   domain='fourth-source-specific' if previous['domain']=='source-specific' else previous['domain']
   cases.append(dict(id=id,sourceOccurrence=row['id'],sourceAlternativeSha256=alternative['sourceAlternativeSha256'],
                     sourceKind='predecessor-matrix',predecessorAlternative=candidates[0],domain=domain,witnessId=previous['witnessId']))
  else:
   if id not in novel:raise SystemExit('Missing source-specific witness: '+id)
   if novel[id]['sourceAlternativeSha256']!=alternative['sourceAlternativeSha256']:raise SystemExit('Source-specific alternative hash differs: '+id)
   cases.append(dict(id=id,sourceOccurrence=row['id'],sourceAlternativeSha256=alternative['sourceAlternativeSha256'],
                     sourceKind='source-specific',predecessorAlternative=None,domain='source-specific',witnessId=id))
used={c['id'] for c in cases if c['sourceKind']=='source-specific'}
if used!=set(novel):raise SystemExit('Extra source-specific witness')
corpora=dict(fourth['predecessorCorpora'])
corpora['fourth-source-specific']=fourth['sourceSpecific']
result=dict(schemaVersion=1,profile='CSharp5',status='source-witnesses-unreviewed',sourceSha256=index['sourceSha256'],
 scope='Version5 alternative identities linked through the admitted CSharp4 matrix to exact inherited corpus cases, or to Version5 source-specific cases. Execution on the CSharp5 factory, context review and eventual admission are separate gates.',
 indexSha256=sha('docs/coverage/csharp5-standard-index.json'),deltaSha256=sha('docs/coverage/csharp5-source-delta-inventory.json'),
 predecessorMatrix=dict(path=fourth_path,sha256=sha(fourth_path)),predecessorCorpora=corpora,
 sourceSpecific=dict(path=specific_path,sha256=sha(specific_path)),
 counts=dict(production=sum(a['kind']=='production' for r in index['productions'] for a in r['alternatives']),
             finite=sum(a['kind']=='one-of-token' for r in index['productions'] for a in r['alternatives']),
             predecessor=sum(c['sourceKind']=='predecessor-matrix' for c in cases),sourceSpecific=len(used)),cases=cases)
output=json.dumps(result,indent=2)+'\n'
p=ROOT/'tests/Corpus/CSharp5/alternative-witness-matrix.json'
if args.check:
 if not p.exists() or p.read_text()!=output:raise SystemExit('Fresh matrix differs from the committed matrix')
else:p.write_text(output)
print(len(cases),'alternative witnesses:',result['counts'])
