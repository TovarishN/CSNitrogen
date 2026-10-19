#!/usr/bin/env python3
"""Index grammar occurrence identities from the SHA-pinned C#4 Word DOC.

Development tool: requires macOS /usr/bin/textutil. No document is downloaded or
redistributed. --check compares a fresh extraction with the committed index.
UTF8 text is split at LF only; Word's U+2028 grammar separators remain in a line.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('doc', type=Path)
parser.add_argument('--check', action='store_true')
parser.add_argument('--compare-third', action='store_true', help='also write/check the candidate occurrence-scoped comparison with the admitted CSharp3 index')
args = parser.parse_args()
policy = json.loads((ROOT / 'docs/coverage/csharp4-reference-policy.json').read_text())
primary = policy['primary']
if hashlib.sha256(args.doc.read_bytes()).hexdigest() != primary['sha256']:
    raise SystemExit('Document does not match the pinned CSharp4 primary')
text = subprocess.check_output(['/usr/bin/textutil', '-convert', 'txt', '-stdout', str(args.doc)]).decode('utf-8')
lines = text.split('\n')
start = next(i for i, line in enumerate(lines) if line == 'Grammar')
end = next(i for i in range(start, len(lines)) if lines[i].strip('\f') == 'References')
def source_alternatives(body, occurrence):
    fragments = '\n'.join(body).split('\u2028')
    one_of = 'one of' in fragments[0]
    if one_of:
        alternatives = ' '.join(fragments[1:]).split()
    else:
        alternatives = []
        for fragment in fragments[1:]:
            if not fragment.strip():
                continue
            # Word indents long right-hand sides with two tabs. These continue
            # the preceding alternative rather than introduce another choice.
            if fragment.startswith('\t\t'):
                if not alternatives:
                    raise SystemExit('Continuation has no alternative: ' + occurrence)
                alternatives[-1] += ' ' + fragment.strip()
            else:
                alternatives.append(fragment.strip())
    result = []
    for ordinal, alternative in enumerate(alternatives, 1):
        canonical = ' '.join(alternative.split())
        kind = 'one-of-token' if one_of else 'inherited-placeholder' if canonical in ('...', '…') else 'production'
        result.append(dict(id=f'{occurrence}/{ordinal}', ordinal=ordinal, kind=kind,
                           sourceAlternativeSha256=hashlib.sha256(canonical.encode()).hexdigest()))
    if not result:
        raise SystemExit('Source occurrence has no alternatives: ' + occurrence)
    return result

section = None
rows = []
heading_pattern = re.compile(r'^([ \t]*)([a-z][a-z0-9-]*)(:|\u2028)')
for i in range(start + 1, end):
    line = lines[i]
    if line in ('Lexical grammar', 'Syntactic grammar', 'Grammar extensions for unsafe code'):
        section = {'Lexical grammar': 'lexical', 'Syntactic grammar': 'syntactic',
                   'Grammar extensions for unsafe code': 'unsafe'}[line]
    match = heading_pattern.match(line)
    if not match:
        continue
    if match[3] != ':':
        raise SystemExit('Unreviewed heading without a source colon: ' + match[2])
    if section is None:
        raise SystemExit('Grammar occurrence has no source section')
    # Preserve authored continuation lines; prose/headings are not copied.
    body = [line]
    j = i + 1
    while j < end and lines[j].startswith((' ', '\t')) and lines[j].strip() and not heading_pattern.match(lines[j]):
        body.append(lines[j])
        j += 1
    rows.append(dict(id=f'B:{i+1}:{match[2]}', name=match[2], line=i+1,
                     grammar=section, headingStyle=('indented-colon' if match[1] else 'colon') if match[3] == ':' else 'missing-source-colon', sourceBodySha256=hashlib.sha256('\n'.join(body).encode()).hexdigest(),
                     alternatives=source_alternatives(body, f'B:{i+1}:{match[2]}')))
for row in rows:
    pattern = re.compile(r'^[ \t]*' + re.escape(row['name']) + r'(?::|\u2028)')
    row['mainTextOccurrences'] = [n+1 for n, line in enumerate(lines[:start]) if pattern.match(line)]
    if not row['mainTextOccurrences']:
        raise SystemExit('Missing main-text heading: ' + row['id'])
    for alternative in row['alternatives']:
        if alternative['kind'] == 'inherited-placeholder':
            origins = [a['id'] for previous in rows if previous['line'] < row['line'] and previous['name'] == row['name']
                       for a in previous['alternatives'] if a['kind'] != 'inherited-placeholder']
            if not origins:
                raise SystemExit('Inheritance marker has no earlier source: ' + alternative['id'])
            alternative['earlierSourceAlternativeIds'] = origins
result = dict(schemaVersion=1, profile='CSharp4', status='indexed-unreviewed',
              source=primary, sourceSha256=primary['sha256'],
              extraction=dict(tool='macOS textutil -convert txt -stdout', lineSplit='LF only',
                              textSha256=hashlib.sha256(text.encode()).hexdigest(),
                              scope='Appendix B grammar occurrences and source-alternative identities; no printed-page or portable extraction claim'),
              counts={kind: sum(r['grammar'] == kind for r in rows) for kind in ('lexical', 'syntactic', 'unsafe')},
              alternativeCounts={kind: sum(len(r['alternatives']) for r in rows if r['grammar'] == kind) for kind in ('lexical', 'syntactic', 'unsafe')},
              productions=rows)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp4-standard-index.json'
if args.check:
    if not path.exists() or path.read_text() != output:
        raise SystemExit('Fresh extraction differs from the committed CSharp4 index')
else:
    path.write_text(output)
print(f'{len(rows)} indexed occurrences; {len({r["name"] for r in rows})} unique names; status indexed-unreviewed')

if args.compare_third:
    third_path = ROOT / 'docs/coverage/csharp3-standard-index.json'
    third_bytes = third_path.read_bytes()
    third_gate = json.loads((ROOT / 'docs/coverage/csharp3-coverage-gate.json').read_text())
    if hashlib.sha256(third_bytes).hexdigest() != third_gate['indexSha256']:
        raise SystemExit('Predecessor index differs from its admitted gate')
    third = json.loads(third_bytes)
    def key_rows(productions):
        counts = Counter()
        keyed = {}
        for row in productions:
            family = (row['grammar'], row['name'])
            counts[family] += 1
            keyed[(*family, counts[family])] = row
        return keyed
    old = key_rows(third['productions'])
    current = key_rows(rows)
    comparison = []
    relations = Counter()
    for key, row in current.items():
        previous = old.get(key)
        relation = 'added' if previous is None else 'body-identical' if previous['sourceBodySha256'] == row['sourceBodySha256'] else 'body-changed'
        relations[relation] += 1
        candidates = []
        for alternative in row['alternatives']:
            matching = [] if previous is None else [a['id'] for a in previous['alternatives']
                if a['kind'] == alternative['kind'] and a['sourceAlternativeSha256'] == alternative['sourceAlternativeSha256']]
            candidates.append(dict(alternativeId=alternative['id'], predecessorAlternativeCandidates=matching))
        comparison.append(dict(standardId=row['id'], familyOccurrence=key[2],
            predecessorId=None if previous is None else previous['id'], bodyRelation=relation,
            alternativeCandidates=candidates))
    removed = [row['id'] for key, row in old.items() if key not in current]
    delta = dict(schemaVersion=1, profile='CSharp4', status='candidate-unreviewed',
        sourceSha256=primary['sha256'], indexSha256=hashlib.sha256(output.encode()).hexdigest(),
        predecessor=dict(profile='CSharp3', sourceSha256=third['sourceSha256'], indexSha256=hashlib.sha256(third_bytes).hexdigest()),
        scope='Comparison by grammar domain, heading name and within-family occurrence ordinal. Exact normalized alternative hashes suggest candidates, not CSharp4 owners, corpus coverage, normative equivalence or source admission.',
        counts={**dict(relations), 'removedPredecessorOccurrences':len(removed)},
        removedPredecessorOccurrences=removed, productions=comparison)
    delta_path = ROOT / 'docs/coverage/csharp4-source-delta-inventory.json'
    delta_output = json.dumps(delta, indent=2) + '\n'
    if args.check:
        if not delta_path.exists() or delta_path.read_text() != delta_output:
            raise SystemExit('Fresh comparison differs from the committed CSharp4 candidate inventory')
    else:
        delta_path.write_text(delta_output)
    print('Candidate predecessor comparison:', dict(relations), 'removed:', len(removed))
