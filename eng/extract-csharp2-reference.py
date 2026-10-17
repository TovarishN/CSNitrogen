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
all_lines = []
section = None
for page, item in enumerate(PdfReader(args.pdf).pages, 1):
    if not 465 <= page <= 494:
        continue
    for line, text in enumerate(item.extract_text().splitlines(), 1):
        if re.match(r"A\.\d", text.strip()): section=text.strip()
        all_lines.append((page,line,text,section))
        heading = re.fullmatch(r'\s*([a-z][a-z0-9-]*)(::?)(?:\s+one of)?\s*', text)
        if heading:
            name = heading.group(1)
            rows.append(dict(id=f'A:{page}:{line}:{name}', pdfPage=page,
                             line=line, name=name, kind='lexical' if heading.group(2)=='::' else 'syntactic',
                             status='indexed', annexSection=section))
coordinates={(p,l):i for i,(p,l,t,section) in enumerate(all_lines)}
for i,row in enumerate(rows):
    start=coordinates[(row['pdfPage'],row['line'])]+1
    end=coordinates[(rows[i+1]['pdfPage'],rows[i+1]['line'])] if i+1<len(rows) else len(all_lines)
    body=[]
    for page,line,text,section in all_lines[start:end]:
        text=text.strip()
        if re.match(r'A\.\d',text): break
        if not text or text.isdigit() or text in ['C# LANGUAGE SPECIFICATION','A   GRAMMAR']:continue
        body.append((page,line,text))
    row['endPdfPage'],row['endLine']=(body[-1][:2] if body else (row['pdfPage'],row['line']))
    row['bodyLines']=[b[2] for b in body]
args.output.write_text(json.dumps(dict(schemaVersion=1, profile='CSharp2',
    sourceSha256=EXPECTED, extractor='pypdf==6.10.0', annexAPdfPages=[465,494], scope='Reproducible Annex A occurrences and body bounds; indexing does not establish conformance review',
    productions=rows), indent=2)+'\n')
print(f'{len(rows)} indexed occurrences')
