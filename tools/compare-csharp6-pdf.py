#!/usr/bin/env python3
"""Compare every indexed grammar rule with its pinned ECMA-334 PDF.

Inputs: the PDF (SHA-pinned in csharp6-reference-policy.json) and a macOS PDFKit text extraction of
it (also SHA-pinned; produce it with PDFDocument.string). Annex A of the PDF is parsed into rules
and each rule body, whitespace- and comment-normalized, is compared with the edition's indexed working-text body. The PDF wins every difference; each one is written with
its disposition. --check compares a fresh comparison with the committed record.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('pdf', type=Path)
parser.add_argument('text', type=Path, help='PDFKit text extraction of the PDF')
parser.add_argument('--edition', choices=(6, 7), default=6, type=int,
                    help='ECMA-334 edition, defaults to the sixth')
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
stem = f'csharp{args.edition}'
profile = 'CSharp6' if args.edition == 6 else 'CSharp7_3'
OUT = ROOT / f'docs/coverage/{stem}-pdf-differences.json'
policy = json.loads((ROOT / f'docs/coverage/{stem}-reference-policy.json').read_text())
if hashlib.sha256(args.pdf.read_bytes()).hexdigest() != policy['primary']['sha256']:
    raise SystemExit('PDF does not match the pinned primary')
raw = args.text.read_bytes()
if hashlib.sha256(raw).hexdigest() != policy['extraction']['textSha256']:
    raise SystemExit('Text extraction does not match the pinned PDFKit extraction')
index = json.loads((ROOT / f'docs/coverage/{stem}-standard-index.json').read_text())
lines = raw.decode('utf-8').split('\n')
start = next(i for i, l in enumerate(lines) if l == 'A.2 Lexical grammar' and i > 1000)
end_heading = 'B Portability issues' if args.edition == 6 else 'B. Portability issues'
end = next(i for i in range(start, len(lines)) if lines[i] == end_heading)
page_headers = '' if args.edition == 6 else r'|ECMA-334 A Grammar|A Grammar ECMA-334'
NOISE = re.compile(r'^(ECMA-334|\d+|A Grammar|A\.\d+(\.\d+)? .*|© Ecma International \d+ ?\d*' + page_headers + r')$')
annex = []
for l in lines[start:end]:
    if NOISE.match(l.strip()): continue
    # PDF layout wraps a trailing '//' comment onto the next line; a wrapped tail is prose, not grammar.
    if annex and '//' in annex[-1] and annex[-1].split('//')[0].strip() and re.fullmatch(r"[A-Za-z_][A-Za-z_ ,.]*", l.strip()) and not l.strip().startswith(('fragment',)):
        continue
    annex.append(l)

def strip_comment(line):
    out, quote, i = [], None, 0
    while i < len(line):
        c = line[i]
        if quote:
            out.append(c)
            if c == '\\' and i + 1 < len(line): out.append(line[i + 1]); i += 2; continue
            if c == quote: quote = None
        elif c == "'": quote = "'"; out.append(c)
        elif c == '[': quote = ']'; out.append(c)
        elif line.startswith('//', i): break
        else: out.append(c)
        i += 1
    return ''.join(out).rstrip()

def norm(text):
    # Typographic quotes and spacing from PDF layout are not grammar differences.
    text = text.replace('‘', "'").replace('’', "'").replace(' ', ' ')
    return ''.join(text.split())

pdf = {}
i = 0
while i < len(annex):
    text = strip_comment(annex[i]).strip()
    one = re.match(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*:(.*);$', text)
    head = re.match(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)$', text)
    colon = re.match(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*:$', text)
    if colon:
        body = []; i += 1
        while i < len(annex):
            t = strip_comment(annex[i]).strip()
            if t == ';' or (t.endswith(';') and not t.endswith("';'")):
                body.append(t[:-1]); break
            body.append(t); i += 1
        pdf.setdefault(colon.group(2), []).append(' '.join(body).strip()); i += 1; continue
    if one:
        pdf.setdefault(one.group(2), []).append(one.group(3)); i += 1; continue
    nxt = i + 1
    while head and nxt < len(annex) and strip_comment(annex[nxt]).strip() == '' and annex[nxt].strip().startswith('//'): nxt += 1
    if head and nxt < len(annex) and strip_comment(annex[nxt]).strip().startswith(':'):
        body = []; i = nxt
        while i < len(annex):
            t = strip_comment(annex[i]).strip()
            if t == ';' or (t.endswith(';') and not t.endswith("';'")):
                body.append(t[:-1]); break
            body.append(t); i += 1
        joined = ' '.join(body).strip()
        pdf.setdefault(head.group(2), []).append(joined[1:] if joined.startswith(':') else joined)
    i += 1

differences, agreed = [], 0
names = {p['name'] for p in index['productions']}
for p in index['productions']:
    working = ' | '.join(a['text'] for a in p['alternatives'])
    bodies = pdf.get(p['name'], [])
    if len(bodies) == 1 and norm(bodies[0]) == norm(working):
        agreed += 1; continue
    differences.append({'rule': p['name'], 'indexId': p['id'], 'working': working, 'pdf': bodies,
        'disposition': 'missing-from-pdf-annex' if not bodies else 'duplicated-in-pdf-annex' if len(bodies) > 1 else 'body-differs',
        'resolution': 'pending-review'})
extra = sorted(set(pdf) - names)
record = {
    'schemaVersion': 1, 'profile': profile, 'primarySha256': policy['primary']['sha256'],
    'extractionSha256': policy['extraction']['textSha256'], 'indexSha256': hashlib.sha256((ROOT / f'docs/coverage/{stem}-standard-index.json').read_bytes()).hexdigest(),
    'scope': 'Annex A rule bodies, whitespace/comment/typographic-quote normalized; main-text prose is reviewed separately',
    'counts': {'indexed': len(index['productions']), 'pdfRules': len(pdf), 'agreed': agreed, 'differences': len(differences), 'pdfOnly': len(extra)},
    'pdfOnly': extra, 'differences': differences,
}
text = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
if args.check:
    if OUT.read_text(encoding='utf-8') != text: raise SystemExit(f'{OUT.name} is stale')
    print('current')
else:
    OUT.write_text(text, encoding='utf-8'); print(record['counts'])
