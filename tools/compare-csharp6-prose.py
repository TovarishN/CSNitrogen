#!/usr/bin/env python3
"""Align a pinned edition’s working text with the pinned PDF, block by block, in document order.

Inputs: the standard/ directory, the SHA-pinned PDF and its pinned PDFKit text. Every block of
the working text (prose paragraph, list item, note, table, heading, code block and grammar block, with
markdown and HTML markup removed, outside HTML comments) is reduced to letters, digits and operator
characters after Unicode NFKC normalization. The PDF body (from the Foreword, with page headers, footers,
page numbers and list bullets removed) is reduced the same way. Blocks are located in order: each must
occur after the end of the previous match. A block not found there is reported as working-text-only; PDF
text between consecutive matches is reported as PDF-only. The report lists alignment candidates for review against the PDF. --check compares with the record.
"""
import argparse, hashlib, html, json, re, unicodedata
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('standard', type=Path)
parser.add_argument('pdf', type=Path)
parser.add_argument('text', type=Path)
parser.add_argument('--edition', choices=(6, 7), default=6, type=int,
                    help='ECMA-334 edition, defaults to the sixth')
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
stem = f'csharp{args.edition}'
profile = 'CSharp6' if args.edition == 6 else 'CSharp7_3'
OUT = ROOT / f'docs/coverage/{stem}-main-text-differences.json'
policy = json.loads((ROOT / f'docs/coverage/{stem}-reference-policy.json').read_text())
if hashlib.sha256(args.pdf.read_bytes()).hexdigest() != policy['primary']['sha256']: raise SystemExit('PDF does not match the pinned primary')
raw = args.text.read_bytes()
if hashlib.sha256(raw).hexdigest() != policy['extraction']['textSha256']: raise SystemExit('Text extraction does not match the pinned extraction')
if hashlib.sha256((args.standard / 'grammar.md').read_bytes()).hexdigest() != policy['workingText']['grammarSha256']: raise SystemExit('Working text does not match the pinned tag')

ORDER = ['foreword', 'introduction', 'scope', 'normative-references', 'terms-and-definitions', 'general-description', 'conformance',
         'lexical-structure', 'basic-concepts', 'types', 'variables', 'conversions', 'expressions', 'statements', 'namespaces', 'classes',
         'structs', 'arrays', 'interfaces', 'enums', 'delegates', 'exceptions', 'attributes', 'unsafe-code', 'grammar',
         'portability-issues', 'standard-library', 'documentation-comments', 'bibliography']
if args.edition == 7:
    ORDER.insert(ORDER.index('expressions'), 'patterns')
chapters = {p.stem for p in args.standard.glob('*.md') if p.name != 'README.md'}
if chapters != set(ORDER):
    raise SystemExit(f'Chapter inventory differs: missing={sorted(set(ORDER) - chapters)}, extra={sorted(chapters - set(ORDER))}')

KEEP = set('<>=!?&|^~+-*/%')  # '.' and ',' vary with list punctuation; '?' still separates ?. from .
def reduce(text):
    text = unicodedata.normalize('NFKC', text).replace('‘', "'").replace('’', "'").replace('“', '"').replace('”', '"')
    return ''.join(c for c in text.lower() if c.isalnum() or c in KEEP)

header_text = r'(?:Chapter \d+ .+|[A-E] (?:Grammar|Portability issues|Standard library|Documentation comments|Bibliography)|Table of Contents)'
page_headers = '' if args.edition == 6 else rf'|{header_text} ECMA-334|ECMA-334 {header_text}'
HEADER = re.compile(r'^(ECMA-334|[0-9ivxlc]+|\d+(\.\d+)+|Chapter \d+ .*|[A-E] (Grammar|Portability issues|Standard library|Documentation comments|Bibliography)|© Ecma International \d+.*' + page_headers + r')$')
pdf_lines = raw.decode('utf-8').split('\n')
start = next(i for i, l in enumerate(pdf_lines) if l.strip() == 'Foreword' and i > 700)
# List bullets and the dot after a top-level chapter number ("1. Scope") are PDF layout.
# Ordered-list numbers (and a top-level chapter number, "1. Scope") and bullets ("•", "o") are PDF layout.
kept = [re.sub(r'^(?:\d+\.|[•◦▪]|o(?=\s+[A-Za-z]))\s+', '', l.strip()) for l in pdf_lines[start:] if not HEADER.match(l.strip())]
pdf = reduce('\n'.join(kept))

def strip_markup(text, kind):
    if kind != 'prose': return text
    table = re.search(r'^[\s>]*\|?[\s:|-]*-{3,}[\s:|-]*$', text, flags=re.M) is not None   # a markdown table rule row
    text = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', text)          # links keep their text
    text = re.sub(r'^\s*(>\s?)+', '', text, flags=re.M)             # note/example quote markers
    text = re.sub(r'^\s*#\s+\d+\s+', '', text, flags=re.M)         # top-level chapter number (PDF prints "1." as layout)
    text = re.sub(r'^(\s*#+\s*)Annex ([A-E]) ', r'\1\2 ', text, flags=re.M)  # the PDF prints "A. Grammar"
    text = re.sub(r'^\s*#+\s*', '', text, flags=re.M)               # heading markers
    text = re.sub(r'^\s*(?:[-*]|\d+\.)\s+', '', text, flags=re.M)   # list markers
    text = re.sub(r'^[\s|:-]+$', '', text, flags=re.M)              # markdown table rule rows
    # Emphasis markers are removed outside code spans only; code spans keep their operators.
    parts = re.split(r'(`[^`]*`)', text)
    # HTML tags and entities (converted tables) are handled outside code spans only; only real tag names count as tags.
    TAG = re.compile(r'</?(?:table|tr|td|th|p|code|br|em|strong|b|i|sub|sup|span|div|thead|tbody)\b[^<>]*>', re.I)
    text = ''.join(p[1:-1] if p.startswith('`') and p.endswith('`') and len(p) > 1 else html.unescape(TAG.sub(' ', p)).replace('\\*', '\x00').replace('*', '').replace('\x00', '*') for p in parts)
    if table:   # cell separators go; an escaped \| inside a cell is a literal operator character
        text = re.sub(r'(?<![\\|])\|(?!\|)', ' ', text).replace('\\|', '|')
    return text.replace('\\', '')

QUOTE = re.compile(r'^\s*(>\s?)*')
def blocks(path):
    """Yield (line, kind, text): paragraphs, list items, code and grammar blocks, outside HTML comments."""
    source = re.sub(r'<!--.*?-->', lambda m: '\n' * m.group(0).count('\n'), path.read_text(encoding='utf-8'), flags=re.S)
    lines = source.split('\n'); i = 0; block, first = [], 0
    while i < len(lines):
        line = lines[i]; bare = QUOTE.sub('', line)
        if bare.startswith('```'):
            if block: yield first + 1, 'prose', '\n'.join(block)
            block = []
            kind = 'grammar' if bare.strip() == '```ANTLR' else 'code'
            body, j = [], i + 1
            while j < len(lines) and not QUOTE.sub('', lines[j]).startswith('```'):
                body.append(QUOTE.sub('', lines[j])); j += 1
            yield i + 1, kind, '\n'.join(body); i = j + 1; continue
        if not bare.strip():
            if block: yield first + 1, 'prose', '\n'.join(block)
            block = []
        elif re.match(r'(?:[-*]|\d+\.)\s+', bare.lstrip()) and block:
            yield first + 1, 'prose', '\n'.join(block); block = [line]; first = i
        else:
            if not block: first = i
            block.append(line)
        i += 1
    if block: yield first + 1, 'prose', '\n'.join(block)

WINDOW = 4000
ANCHOR = 200
working, pdf_only, pointer, total = [], [], 0, 0
for name in ORDER:
    path = args.standard / (name + '.md')
    for line, kind, text in blocks(path):
        reduced = reduce(strip_markup(text, kind))
        if not reduced: continue
        total += 1
        at = pdf.find(reduced, pointer, pointer + WINDOW + len(reduced))  # a nearby match only; a far one would skip text
        if at < 0 and len(reduced) >= ANCHOR: at = pdf.find(reduced, pointer)  # a long block is a safe anchor to resynchronize
        if at < 0:
            working.append(dict(file=path.name, line=line, kind=kind, sha256=hashlib.sha256(text.encode()).hexdigest(), text=text)); continue
        if at > pointer:
            gap = pdf[pointer:at]
            pdf_only.append(dict(before=f'{path.name}:{line}', sha256=hashlib.sha256(gap.encode()).hexdigest(), reduced=gap))
        pointer = at + len(reduced)
if pointer < len(pdf): pdf_only.append(dict(before='end', sha256=hashlib.sha256(pdf[pointer:].encode()).hexdigest(), reduced=pdf[pointer:]))
if args.edition == 7:
    for candidate in working:
        candidate['sourceCharacters'] = len(candidate.pop('text'))
    for candidate in pdf_only:
        candidate['normalizedCharacters'] = len(candidate.pop('reduced'))
record = dict(schemaVersion=2 if args.edition == 6 else 3, profile=profile, primarySha256=policy['primary']['sha256'], extractionSha256=policy['extraction']['textSha256'],
              workingText={k: policy['workingText'][k] for k in ('tag' if args.edition == 6 else 'branch', 'commit')},
              scope='Every working-text block (prose, notes, list items, tables, headings, code and grammar) reduced to letters, digits and operator characters and aligned in document order with the reduced PDF body; text on either side without a counterpart is reported.',
              counts=dict(blocks=total, aligned=total - len(working), workingTextOnly=len(working), pdfOnly=len(pdf_only)),
              workingTextOnly=working, pdfOnly=pdf_only)
text = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
if args.check:
    if OUT.read_text(encoding='utf-8') != text: raise SystemExit(f'{OUT.name} is stale')
    print('current')
else:
    OUT.write_text(text, encoding='utf-8'); print(record['counts'])
