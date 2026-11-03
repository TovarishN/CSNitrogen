#!/usr/bin/env python3
"""Record dispositions for every C#4->C#5 main-text difference region; no admission is inferred.

Requires macOS textutil and the pinned C#4 DOC and C#5 DOCX. Regions whose texts are
equal after cross-reference, index-field, quote/dash and layout normalization are
classified mechanically; every other region carries an explicit reviewed disposition
below, checked for completeness. --check compares with the committed artifact.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('fourth_doc', type=Path)
p.add_argument('fifth_docx', type=Path)
p.add_argument('--check', action='store_true')
a = p.parse_args()
FOURTH = 'cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7'
FIFTH = '6f12b6a362d12c401902c57f4f9baf648d12ab399f6d33ce558f72ad20cb8519'
def sha(b): return hashlib.sha256(b).hexdigest()

DEFINITIONS = {
    'async-functions': 'The async method modifier and anonymous-function asyncopt markers are implemented through independently selected capabilities. Source-local ref/out formals (CSN2052), semicolon-bodied async methods (CSN2053) and async partial methods are checked. Task types, return-type validity, evaluation and cross-part partial-method modifier matching remain excluded semantics.',
    'await-expressions': 'await-expression is a unary-expression and statement-expression in async function bodies only. Placement restrictions (CSN2050), raw await identifiers (CSN2051) and direct lambda/query operands (CSN2030) are checked; awaitability, task identity and suspension remain excluded semantics.',
    'generic-member-transcription': 'Main text now prints type-argument-listopt for member-access and pointer-member-access, matching the existing generic member and generic pointer-member syntax admitted since CSharp2.',
    'preprocessing-trailing-comment': 'Adds #endregion to the directives that may end with a single-line comment. The existing pp-message reading of region text already accepts that comment.',
    'modifier-compatibility': 'A modifier/initializer combination rule for events. General modifier compatibility remains an inherited explicit exclusion of the normalized contract.',
    'caller-info-attributes': 'Caller info attributes change compiler argument substitution and #line effects only; attribute syntax is unchanged. Substitution remains an excluded semantic.',
    'field-and-layout-normalization': 'Equal after normalizing resolved versus unresolved cross-reference fields, Word index (XE) and DOCPROPERTY fields, typographic quotes/dashes, NFKC and table/paragraph layout.',
    'retained-semantic-or-editorial-boundary': 'Inspected difference adds no normalized token/owner category: wording, examples, tables of contents or changes to binding, conversions, inference, definite assignment, expansions, events, constructors, runtime or compiler behavior remain within the inherited exclusions.',
}
REVIEWED = {
    'async-functions': ['D:351', 'D:352', 'D:493', 'D:496', 'D:498', 'D:569'],
    'await-expressions': ['D:008', 'D:307', 'D:319', 'D:354', 'D:387'],
    'generic-member-transcription': ['D:241', 'D:667'],
    'preprocessing-trailing-comment': ['D:032'],
    'modifier-compatibility': ['D:529'],
    'caller-info-attributes': ['D:036', 'D:645', 'D:650'],
}
SEMANTIC_OR_EDITORIAL = [
    'D:001', 'D:002', 'D:003', 'D:007', 'D:041', 'D:050', 'D:053', 'D:055', 'D:062', 'D:063', 'D:078',
    'D:099', 'D:100', 'D:101', 'D:102', 'D:121', 'D:125', 'D:126', 'D:128', 'D:166', 'D:167', 'D:168',
    'D:169', 'D:184', 'D:185', 'D:186', 'D:194', 'D:198', 'D:213', 'D:214', 'D:215', 'D:216', 'D:217',
    'D:218', 'D:228', 'D:239', 'D:249', 'D:296', 'D:297', 'D:299', 'D:313', 'D:332', 'D:350', 'D:353',
    'D:355', 'D:365', 'D:375', 'D:377', 'D:378', 'D:399', 'D:400', 'D:401', 'D:402', 'D:403', 'D:407',
    'D:408', 'D:409', 'D:411', 'D:416', 'D:418', 'D:424', 'D:448', 'D:450', 'D:509', 'D:519', 'D:531',
    'D:532', 'D:533', 'D:538', 'D:555', 'D:556', 'D:578', 'D:587', 'D:606', 'D:619', 'D:624', 'D:625',
    'D:646', 'D:663', 'D:684', 'D:687', 'D:688', 'D:689', 'D:690', 'D:691', 'D:692', 'D:693', 'D:694',
    'D:695', 'D:696', 'D:697', 'D:698', 'D:699', 'D:700', 'D:701', 'D:702', 'D:703', 'D:704', 'D:705',
    'D:706',
]
REVIEWED['retained-semantic-or-editorial-boundary'] = SEMANTIC_OR_EDITORIAL

def paragraphs(path, expected):
    if sha(path.read_bytes()) != expected:
        raise SystemExit('Wrong pinned document: ' + str(path))
    raw = subprocess.check_output(['/usr/bin/textutil', '-convert', 'txt', '-stdout', str(path)]).decode('utf-8')
    lines = raw.split('\n'); end = lines.index('Grammar'); rows = {}
    for n, line in enumerate(lines[:end], 1):
        if not line.strip() or 'PAGEREF' in line or line.strip().startswith('Copyright'):
            continue
        line = re.sub(r'REF [A-Za-z0-9_]+(?: \\[rwhn])*(?: [0-9]+(?:\.[0-9]+)*)?', '<ref>', line)
        line = re.sub(r'STYLEREF [^ ]+(?: \\[a-z]+)*(?: [0-9]+(?:\.[0-9]+)*)?', '<style>', line)
        rows[n] = ' '.join(line.split())
    return rows

def loose(text):
    text = unicodedata.normalize('NFKC', text)
    text = re.sub(r'XE "[^"]*"', '', text)
    text = re.sub(r'DOCPROPERTY \w+ \\\* MERGEFORMAT', '', text)
    text = re.sub(r'§+ ?(<ref>|\d+(\.\d+)*)', '§', text)
    text = re.sub(r'<ref>|<style>', '', text)
    for x, y in [('“', '"'), ('”', '"'), ('’', "'"), ('‘', "'"), ('–', '-'), ('—', '-')]:
        text = text.replace(x, y)
    return re.sub(r'\W+', '', text).lower()

old = paragraphs(a.fourth_doc, FOURTH); new = paragraphs(a.fifth_docx, FIFTH)
index_bytes = (ROOT / 'docs/coverage/csharp5-main-text-differences.json').read_bytes()
index = json.loads(index_bytes)
def text(rows, segment):
    if segment['startLine'] is None or segment['endLine'] is None:
        return ''
    return ' '.join(v for k, v in sorted(rows.items()) if segment['startLine'] <= k <= segment['endLine'])
reviewed = {i: c for c, ids in REVIEWED.items() for i in ids}
regions = []
for region in index['regions']:
    mechanical = loose(text(old, region['fourth'])) == loose(text(new, region['fifth']))
    if mechanical and region['id'] in reviewed:
        raise SystemExit('Reviewed disposition for a mechanically equal region: ' + region['id'])
    if not mechanical and region['id'] not in reviewed:
        raise SystemExit('Substantive region without a reviewed disposition: ' + region['id'])
    disposition = 'field-and-layout-normalization' if mechanical else reviewed.pop(region['id'])
    regions.append(dict(id=region['id'], fourthNormalizedSha256=region['fourth']['normalizedSha256'],
                        fifthNormalizedSha256=region['fifth']['normalizedSha256'], disposition=disposition))
if reviewed:
    raise SystemExit('Reviewed dispositions for unknown regions: ' + ', '.join(sorted(reviewed)))
report = ROOT / 'docs/coverage/csharp5-normative-context-review.md'
result = dict(schemaVersion=1, profile='CSharp5', status='source-inspected', sourceSha256=FIFTH,
              differenceIndexSha256=sha(index_bytes),
              scope='Disposition of every normalized main-text difference region, with semantic exclusions retained. This does not establish whole-source admission, publisher-byte authenticity or semantic compilation.',
              report='docs/coverage/csharp5-normative-context-review.md', reportSha256=sha(report.read_bytes()),
              dispositions=DEFINITIONS,
              counts={c: sum(r['disposition'] == c for r in regions) for c in DEFINITIONS},
              regions=regions)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp5-main-text-dispositions.json'
if a.check:
    if not path.exists() or path.read_text() != output:
        raise SystemExit('Fresh dispositions differ from the committed artifact')
else:
    path.write_text(output)
print(result['counts'])
