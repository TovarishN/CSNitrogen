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
args = parser.parse_args()
policy = json.loads((ROOT / 'docs/coverage/csharp3-reference-policy.json').read_text())
primary = policy['primary']
if hashlib.sha256(args.doc.read_bytes()).hexdigest() != primary['sha256']:
    raise SystemExit('Document does not match the pinned CSharp3 primary')
text = subprocess.check_output(['/usr/bin/textutil', '-convert', 'txt', '-stdout', str(args.doc)]).decode('utf-8')
lines = text.split('\n')
start = next(i for i, line in enumerate(lines) if line == 'Grammar')
end = next(i for i in range(start, len(lines)) if lines[i].strip('\f') == 'References')
section = None
rows = []
for i in range(start + 1, end):
    line = lines[i]
    if line in ('Lexical grammar', 'Syntactic grammar', 'Grammar extensions for unsafe code'):
        section = {'Lexical grammar': 'lexical', 'Syntactic grammar': 'syntactic',
                   'Grammar extensions for unsafe code': 'unsafe'}[line]
    match = re.match(r'^([a-z][a-z0-9-]*)(:|\u2028)', line)
    if not match:
        continue
    if match[2] != ':' and match[1] not in {'explicit-anonymous-function-parameter-list', 'implicit-anonymous-function-parameter-list'}:
        raise SystemExit('Unreviewed heading without a source colon: ' + match[1])
    if section is None:
        raise SystemExit('Grammar occurrence has no source section')
    # Preserve authored continuation lines; prose/headings are not copied.
    body = [line]
    j = i + 1
    while j < end and lines[j].startswith((' ', '\t')) and lines[j].strip():
        body.append(lines[j])
        j += 1
    rows.append(dict(id=f'B:{i+1}:{match[1]}', name=match[1], line=i+1,
                     grammar=section, headingStyle='colon' if match[2] == ':' else 'missing-source-colon', sourceBodySha256=hashlib.sha256('\n'.join(body).encode()).hexdigest()))
result = dict(schemaVersion=1, profile='CSharp3', status='indexed-unreviewed',
              source=primary, sourceSha256=primary['sha256'],
              extraction=dict(tool='macOS textutil -convert txt -stdout', lineSplit='LF only',
                              textSha256=hashlib.sha256(text.encode()).hexdigest(),
                              scope='Appendix B grammar occurrences; no printed-page or portable extraction claim'),
              counts={kind: sum(r['grammar'] == kind for r in rows) for kind in ('lexical', 'syntactic', 'unsafe')},
              productions=rows)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp3-standard-index.json'
if args.check:
    if not path.exists() or path.read_text() != output:
        raise SystemExit('Fresh extraction differs from the committed CSharp3 index')
else:
    path.write_text(output)
print(f'{len(rows)} indexed occurrences; {len({r["name"] for r in rows})} unique names; status indexed-unreviewed')
