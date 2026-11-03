#!/usr/bin/env python3
"""Build/check the CSharp5 reviewed-occurrence overlay from pinned review inputs.

Body-identical appendix occurrences carry the admitted CSharp4 review; changed bodies and
main-text additions cite the Version5 review report. Every row binds the crosswalk,
dispositions, witness matrix and report it was reviewed against. --check compares bytes.
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
REPORT = 'docs/coverage/csharp5-normative-context-review.md'
crosswalk_path = 'docs/coverage/csharp5-standard-crosswalk.json'
dispositions_path = 'docs/coverage/csharp5-main-text-dispositions.json'
matrix_path = 'tests/Corpus/CSharp5/alternative-witness-matrix.json'
crosswalk = read(crosswalk_path)
fourth = read('docs/coverage/csharp4-reviewed-occurrences.json')
if fourth['status'] != 'verified':
    raise SystemExit('Predecessor overlay is not verified')
previous = {r['standardId']: r for r in fourth['productions']}
DIMENSIONS = {'normative-prose': 'verified', 'positive-negative': 'verified', 'ambiguity-context': 'verified', 'spans-diagnostics': 'verified'}
source = crosswalk['sourceSha256']
rows = []
for row in crosswalk['productions']:
    prior = previous[row['predecessorId']]
    if prior['status'] != 'verified':
        raise SystemExit('Unverified predecessor review: ' + row['predecessorId'])
    carried = row['bodyRelation'] == 'body-identical'
    rows.append(dict(standardId=row['standardId'], sourceBodySha256=row['sourceBodySha256'], status='verified',
                     basis='predecessor-review-carried' if carried else 'version5-source-review',
                     predecessorId=row['predecessorId'], reviews=DIMENSIONS,
                     sourceReview=dict(domain=prior['sourceReview']['domain'], report=REPORT, occurrenceId=row['standardId'],
                                       sourceSha256=source, status='source-inspected')))
DOMAINS = {'unary-expression': 'expression', 'await-expression': 'expression', 'lambda-expression': 'expression',
           'anonymous-method-expression': 'expression', 'statement-expression': 'statement', 'method-modifier': 'declaration'}
additions = [dict(id=a['id'], sourceLineSha256=a['sourceLineSha256'], status='verified', basis='version5-main-text-review',
                  reviews=DIMENSIONS, sourceReview=dict(domain=DOMAINS[a['name']], report=REPORT, occurrenceId=a['id'],
                                                         sourceSha256=source, status='source-inspected'))
             for a in crosswalk['mainTextAdditions']]
result = dict(schemaVersion=1, profile='CSharp5', status='verified', sourceSha256=source,
 scope='Reviewed normalized Version5 source occurrence and main-text addition dispositions; admission additionally requires the exact factory/policy/source/evidence ledger. Inherited CSharp4 exclusions remain, including general modifier compatibility; awaitability, task types, async return types, caller substitution and execution are semantic exclusions. No semantic compilation/runtime claim.',
 ownerCrosswalkSha256=sha(crosswalk_path), contextReportSha256=sha(REPORT), mainTextDispositionsSha256=sha(dispositions_path),
 alternativeMatrixSha256=sha(matrix_path), predecessorOverlaySha256=sha('docs/coverage/csharp4-reviewed-occurrences.json'),
 productions=rows, mainTextAdditions=additions)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp5-reviewed-occurrences.json'
if args.check:
    if not path.exists() or path.read_text() != output: raise SystemExit('Fresh reviewed overlay differs')
else: path.write_text(output)
print(len(rows), 'reviewed occurrences;', sum(r['basis'] != 'predecessor-review-carried' for r in rows), 'Version5 source reviews;', len(additions), 'main-text additions')
