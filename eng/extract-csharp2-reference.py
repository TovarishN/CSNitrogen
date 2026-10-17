"""Extract candidate Annex A headings; requires pypdf and the pinned local PDF.

This records occurrences, not reviewed source coverage. Do not merge duplicates.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path
from pypdf import PdfReader, __version__ as pypdf_version

EXPECTED = 'de596c178a4a80a8d86e91877544025822ec0749d8c04ecca25af60c75937652'
parser = argparse.ArgumentParser()
parser.add_argument('pdf', type=Path)
parser.add_argument('output', type=Path)
args = parser.parse_args()
if pypdf_version != '6.10.0':
    parser.error('Use pypdf==6.10.0 to preserve candidate page/line coordinates')
if hashlib.sha256(args.pdf.read_bytes()).hexdigest() != EXPECTED:
    parser.error('PDF hash differs from pinned fourth edition')
rows = []
for page, item in enumerate(PdfReader(args.pdf).pages, 1):
    if not 465 <= page <= 494:
        continue
    for line, text in enumerate(item.extract_text().splitlines(), 1):
        heading = re.fullmatch(r'\s*([a-z][a-z0-9-]*)(::?)\s*', text)
        if heading:
            name = heading.group(1)
            rows.append(dict(id=f'A:{page}:{line}:{name}', pdfPage=page,
                             line=line, name=name, kind='lexical' if heading.group(2)=='::' else 'syntactic',
                             status='pending'))
args.output.write_text(json.dumps(dict(schemaVersion=1, profile='CSharp2',
    sourceSha256=EXPECTED, extractor='pypdf==6.10.0', annexAPdfPages=[465,494], scope='Candidate Annex A headings; verify source bounds and prose before coverage promotion',
    productions=rows), indent=2)+'\n')
print(f'{len(rows)} pending candidate occurrences')
