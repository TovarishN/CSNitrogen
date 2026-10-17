#!/usr/bin/env python3
"""Extract Appendix A heading locations from the supplied SHA-pinned ECMA PDF.

Development tool only; requires pypdf. Does not download or redistribute the PDF.
Use --check to compare a fresh extraction with the committed index.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
from pypdf import PdfReader

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('pdf', type=Path)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
reference = json.loads((ROOT / 'docs/coverage/csharp1-reference-policy.json').read_text())['primary']
if hashlib.sha256(args.pdf.read_bytes()).hexdigest() != reference['sha256']:
    raise SystemExit('PDF does not match the pinned first edition')
reader = PdfReader(args.pdf)
productions = []
counts = []
for page in range(353, 379):
    # The PDF embeds printed line numbers after the text. Single/double colons
    # distinguish syntactic/lexical definitions; "one of" is part of a heading.
    text = reader.pages[page - 1].extract_text(extraction_mode='plain')
    heads = list(re.finditer(r'^([a-z][a-z-]*)(:{1,2})\s*(?:one of)?\s*(\d+)\s*$', text, re.M))
    counts.append(len(heads))
    for match in heads:
        name, colons, line = match.groups()
        productions.append(dict(id=f'A:{page}:{int(line)}:{name}', name=name,
                                pdfPage=page, printedPage=page - 14, line=int(line),
                                grammar='lexical' if colons == '::' else ('unsafe' if page == 378 or page == 377 and int(line) >= 34 else 'syntactic')))
# Reviewed page-level counts make extraction drift visible, including one-of
# headings, duplicated summaries and the two unsafe embedded-statement entries.
if counts != [9,14,8,15,16,15,18,13,12,17,14,14,16,17,18,11,12,11,11,15,15,13,15,14,15,14]:
    raise SystemExit(f'Unexpected heading extraction: {counts}')
index = dict(schemaVersion=1, source=reference, scope='Informative Appendix A heading occurrences, PDF pages 353-378; normative prose/context review is separate.', productions=productions)
# One row per heading keeps source locations reviewable without reproducing RHSs.
rendered = json.dumps({k:v for k,v in index.items() if k != 'productions'}, indent=2)[:-2] + ',\n  "productions": [\n'
rendered += ',\n'.join('    ' + json.dumps(p, separators=(',', ':')) for p in productions) + '\n  ]\n}\n'
target = ROOT / 'docs/coverage/csharp1-standard-index.json'
if args.check:
    if target.read_text() != rendered:
        raise SystemExit('Committed source index differs from pinned PDF extraction')
else:
    target.write_text(rendered)
print(f'{len(productions)} occurrences; {len(set(p["name"] for p in productions))} distinct names; pinned PDF verified')
