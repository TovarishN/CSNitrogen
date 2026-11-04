#!/usr/bin/env python3
"""Compare every main-text paragraph of the sixth-edition working text with the pinned PDF.

Inputs: the V6-standard standard/ directory, the SHA-pinned PDF and its pinned PDFKit text. Each
markdown paragraph outside code blocks (prose, notes, list items, table rows and headings) is reduced
to letters and digits after removing markdown markup, and must occur in the PDF body text reduced the
same way, with page headers, footers and page numbers removed. Grammar blocks are compared by
compare-csharp6-pdf.py. Unmatched paragraphs are written with their location for disposition
(csharp6-main-text-dispositions.json); the PDF wins every difference. --check compares with the record.
"""
import argparse, hashlib, json, re
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs/coverage/csharp6-main-text-differences.json'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('standard', type=Path)
parser.add_argument('pdf', type=Path)
parser.add_argument('text', type=Path)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
policy = json.loads((ROOT / 'docs/coverage/csharp6-reference-policy.json').read_text())
if hashlib.sha256(args.pdf.read_bytes()).hexdigest() != policy['primary']['sha256']: raise SystemExit('PDF does not match the pinned primary')
raw = args.text.read_bytes()
if hashlib.sha256(raw).hexdigest() != policy['extraction']['textSha256']: raise SystemExit('Text extraction does not match the pinned extraction')
if hashlib.sha256((args.standard / 'grammar.md').read_bytes()).hexdigest() != policy['workingText']['grammarSha256']: raise SystemExit('Working text does not match the pinned tag')

import unicodedata
def reduce(text):
    text = unicodedata.normalize('NFKC', text)
    text = text.replace('‘', "'").replace('’', "'").replace('“', '"').replace('”', '"')
    return ''.join(c for c in text.lower() if c.isalnum())

HEADER = re.compile(r'^(ECMA-334|\d+|Chapter \d+ .*|[A-E] (Grammar|Portability issues|Standard library|Documentation comments|Bibliography)|© Ecma International \d+.*)$')
pdf_lines = raw.decode('utf-8').split('\n')
body_start = 0  # whole document: foreword and introduction precede Chapter 1
pdf = reduce('\n'.join(l for l in pdf_lines[body_start:] if not HEADER.match(l.strip())))

def strip_markdown(text):
    text = re.sub(r'<!--.*?-->', '', text, flags=re.S)
    text = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', text)        # links keep their text
    text = re.sub(r'^\s*>\s?', '', text, flags=re.M)              # note/example quote markers
    text = re.sub(r'^\s*#+\s*', '', text, flags=re.M)             # heading markers
    text = re.sub(r'^\s*(?:[-*]|\d+\.)\s+', '', text, flags=re.M) # list markers
    return text.replace('\\', '')

records, total = [], 0
for chapter in sorted(p for p in args.standard.glob('*.md') if p.name not in ('grammar.md', 'README.md')):
    lines = chapter.read_text(encoding='utf-8').split('\n')
    fenced, block, start = False, [], 0
    def flush(end):
        global total
        text = '\n'.join(block).strip()
        if not text: return
        # A '>' block inside a note can hold code; drop fenced code within quotes too.
        text = re.sub(r'```.*?```', '', re.sub(r'^\s*>\s?', '', text, flags=re.M), flags=re.S)
        reduced = reduce(strip_markdown(text))
        if not reduced: return
        total += 1
        if reduced not in pdf:
            records.append(dict(file=chapter.name, line=start + 1, sha256=hashlib.sha256(text.encode()).hexdigest(), text=text))
    for n, line in enumerate(lines):
        if line.lstrip('> ').startswith('```'):
            fenced = not fenced
            if fenced: flush(n); block = []
            else: start = n + 1
            continue
        if fenced: continue
        if not line.strip():
            flush(n); block = []; start = n + 1
        elif re.match(r'^\s*(>\s*)*(?:[-*]|\d+\.)\s+', line) and block:
            flush(n); block = [line]; start = n   # each list item is compared on its own
        else:
            if not block: start = n
            block.append(line)
    flush(len(lines))
record = dict(schemaVersion=1, profile='CSharp6', primarySha256=policy['primary']['sha256'], extractionSha256=policy['extraction']['textSha256'],
              workingText=dict(tag=policy['workingText']['tag'], commit=policy['workingText']['commit']),
              scope='Every main-text paragraph outside code blocks, reduced to letters and digits, located in the reduced PDF body text; grammar blocks are compared separately.',
              counts=dict(paragraphs=total, matched=total - len(records), unmatched=len(records)), unmatched=records)
text = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
if args.check:
    if OUT.read_text(encoding='utf-8') != text: raise SystemExit('csharp6-main-text-differences.json is stale')
    print('current')
else:
    OUT.write_text(text, encoding='utf-8'); print(record['counts'])
