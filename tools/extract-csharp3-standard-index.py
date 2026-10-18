#!/usr/bin/env python3
"""Index grammar occurrence identities from the SHA-pinned C#3 Word DOC.

Development tool: requires macOS /usr/bin/textutil. No document is downloaded or
redistributed. --check compares a fresh extraction with the committed index.
UTF8 text is split at LF only; Word's U+2028 grammar separators remain in a line.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('doc', type=Path)
parser.add_argument('--check', action='store_true')
parser.add_argument('--check-crosswalk', type=Path, metavar='CROSSWALK',
                    help='also verify every candidate body identity and main-text heading reference')
args = parser.parse_args()
policy = json.loads((ROOT / 'docs/coverage/csharp3-reference-policy.json').read_text())
primary = policy['primary']
if hashlib.sha256(args.doc.read_bytes()).hexdigest() != primary['sha256']:
    raise SystemExit('Document does not match the pinned CSharp3 primary')
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
    if match[3] != ':' and match[2] not in {'explicit-anonymous-function-parameter-list', 'implicit-anonymous-function-parameter-list'}:
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
result = dict(schemaVersion=1, profile='CSharp3', status='indexed-unreviewed',
              source=primary, sourceSha256=primary['sha256'],
              extraction=dict(tool='macOS textutil -convert txt -stdout', lineSplit='LF only',
                              textSha256=hashlib.sha256(text.encode()).hexdigest(),
                              scope='Appendix B grammar occurrences and source-alternative identities; no printed-page or portable extraction claim'),
              counts={kind: sum(r['grammar'] == kind for r in rows) for kind in ('lexical', 'syntactic', 'unsafe')},
              alternativeCounts={kind: sum(len(r['alternatives']) for r in rows if r['grammar'] == kind) for kind in ('lexical', 'syntactic', 'unsafe')},
              productions=rows)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp3-standard-index.json'
if args.check:
    if not path.exists() or path.read_text() != output:
        raise SystemExit('Fresh extraction differs from the committed CSharp3 index')
else:
    path.write_text(output)
if args.check_crosswalk:
    crosswalk = json.loads(args.check_crosswalk.read_text())
    if crosswalk['sourceSha256'] != primary['sha256']:
        raise SystemExit('Crosswalk uses a different primary')
    candidates = crosswalk['productions']
    if len(candidates) != len(rows) or len({r['standardId'] for r in candidates}) != len(rows):
        raise SystemExit('Missing or duplicate crosswalk occurrence')
    indexed = {r['id']: r for r in rows}
    for candidate in candidates:
        heading = indexed.get(candidate['standardId'])
        if heading is None or candidate['sourceBodySha256'] != heading['sourceBodySha256']:
            raise SystemExit('Crosswalk source occurrence/body differs: ' + candidate['standardId'])
        finite = {a['id'] for a in heading['alternatives'] if a['kind'] == 'one-of-token'}
        links = candidate.get('finiteAlternativeWitnesses', [])
        for link in links:
            if link['alternativeId'] not in finite or link['caseId'] != link['alternativeId']:
                raise SystemExit('Crosswalk finite alternative differs: ' + candidate['standardId'])
        if 'lexicalProductionAlternativeEvidence' in crosswalk:
            expected_lexical = {a['id'] for a in heading['alternatives']
                                if heading['grammar'] == 'lexical' and a['kind'] == 'production'}
            lexical_links = candidate.get('lexicalProductionAlternativeWitnesses', [])
            if sorted(link['alternativeId'] for link in lexical_links) != sorted(expected_lexical):
                raise SystemExit('Crosswalk lexical production set differs: ' + candidate['standardId'])
            if any(link['caseId'] != link['alternativeId'] for link in lexical_links):
                raise SystemExit('Crosswalk lexical production case differs: ' + candidate['standardId'])
        production_domains = crosswalk.get('productionAlternativeEvidence', {})
        if set(production_domains) - {'expression', 'statement', 'unsafe', 'declaration'}:
            raise SystemExit('Crosswalk has an unknown production evidence domain')
        if any(link['domain'] not in production_domains for link in candidate.get('productionAlternativeWitnesses', [])):
            raise SystemExit('Crosswalk has an unregistered production link domain: ' + candidate['standardId'])
        for domain in production_domains:
            in_domain = ((domain == 'expression' and heading['grammar'] == 'syntactic' and heading['line'] <= 8622)
                         or (domain == 'statement' and heading['grammar'] == 'syntactic' and 8624 <= heading['line'] <= 8675)
                         or (domain == 'unsafe' and heading['grammar'] == 'unsafe')
                         or (domain == 'declaration' and heading['grammar'] == 'syntactic' and heading['line'] >= 8677))
            expected_production = {a['id'] for a in heading['alternatives'] if in_domain and a['kind'] == 'production'}
            production_links = [link for link in candidate.get('productionAlternativeWitnesses', []) if link['domain'] == domain]
            if sorted(link['alternativeId'] for link in production_links) != sorted(expected_production):
                raise SystemExit('Crosswalk production alternative set differs: ' + candidate['standardId'])
            if any(link['caseId'] != link['alternativeId'] for link in production_links):
                raise SystemExit('Crosswalk production case differs: ' + candidate['standardId'])
        markers = {a['id'] for a in heading['alternatives'] if a['kind'] == 'inherited-placeholder'}
        inherited = candidate.get('inheritedSourceAlternatives', [])
        if 'sourceInheritanceEvidence' in crosswalk:
            if sorted(link['alternativeId'] for link in inherited) != sorted(markers):
                raise SystemExit('Crosswalk inherited marker set differs: ' + candidate['standardId'])
            earlier = [a['id'] for row in rows if row['line'] < heading['line'] and row['name'] == heading['name']
                       for a in row['alternatives'] if a['kind'] != 'inherited-placeholder']
            for link in inherited:
                if link['status'] != 'source-mapped' or not earlier or sorted(link['sourceAlternativeIds']) != sorted(earlier):
                    raise SystemExit('Crosswalk inherited source set differs: ' + candidate['standardId'])
        pattern = re.compile(r'^[ \t]*' + re.escape(heading['name']) + r'(?::|\u2028)')
        main = [n+1 for n, line in enumerate(lines[:start]) if pattern.match(line)]
        if not main or candidate['mainTextOccurrences'] != main:
            raise SystemExit('Crosswalk main-text references differ: ' + candidate['standardId'])
    print(f'{len(candidates)} crosswalk occurrence bodies and main-text heading references checked')
print(f'{len(rows)} indexed occurrences; {len({r["name"] for r in rows})} unique names; status indexed-unreviewed')
