#!/usr/bin/env python3
"""Disposition every difference found by compare-csharp6-prose.py, in both directions.

Each working-text-only block and each PDF-only span gets one reviewed category; the PDF governs every
difference. Grammar blocks are syntax-bearing: their rule bodies must be verified rule by rule against
Annex A (compare-csharp6-pdf.py). A difference without a disposition, or a disposition for a difference that
no longer exists, fails the build. --check compares with the committed record.
"""
import argparse, hashlib, json
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
SRC = 'docs/coverage/csharp6-main-text-differences.json'
diff = json.loads((ROOT / SRC).read_text())
CATEGORIES = {
 'grammar-comment-layout': 'ANTLR block whose trailing comments and negated character sets the PDF prints in another position; every rule body is verified against Annex A by compare-csharp6-pdf.py',
 'code-comment-layout': 'informative C# or XML example whose comments, quote marks or line order the PDF prints differently; no syntax rule depends on it',
 'code-output-digits': 'program output lines made only of digits, removed together with PDF page numbers; informative',
 'table-layout': 'converted table whose cells, repeated headers or minus glyphs the PDF lays out differently; operator results, member forms, format strings or documentation tags, not syntax rules',
 'informative-edit': 'informative example text edited after the PDF (a variable name or an explanatory phrase); the PDF governs',
 'pdf-layout': 'PDF-only layout text: a repeated heading, a list bullet, the adoption notice or a page fragment; no working-text counterpart is expected',
}
W = {
 'lexical-structure.md:387': 'grammar-comment-layout', 'lexical-structure.md:713': 'grammar-comment-layout', 'lexical-structure.md:789': 'grammar-comment-layout',
 'lexical-structure.md:932': 'grammar-comment-layout', 'lexical-structure.md:1025': 'grammar-comment-layout', 'grammar.md:11': 'grammar-comment-layout',
 'grammar.md:522': 'grammar-comment-layout',
 'conversions.md:23': 'code-comment-layout', 'expressions.md:1517': 'code-comment-layout', 'expressions.md:2870': 'code-comment-layout',
 'classes.md:215': 'code-comment-layout', 'classes.md:1608': 'code-comment-layout', 'attributes.md:105': 'code-comment-layout',
 'documentation-comments.md:214': 'code-comment-layout', 'documentation-comments.md:575': 'code-comment-layout',
 'expressions.md:1542': 'informative-edit', 'expressions.md:3892': 'informative-edit',
 **{f'expressions.md:{n}': 'table-layout' for n in (3362, 3482, 3612, 3632, 3778, 3788, 3798, 3808)},
 'standard-library.md:475': 'table-layout', 'standard-library.md:664': 'table-layout', 'documentation-comments.md:69': 'table-layout',
 **{k: 'code-output-digits' for k in ('expressions.md:4888', 'expressions.md:4965', 'expressions.md:4989', 'expressions.md:5017', 'expressions.md:5080', 'classes.md:1315', 'structs.md:288', 'structs.md:337')},
}
P = {
 'foreword.md:3': 'pdf-layout', 'scope.md:1': 'pdf-layout', 'expressions.md:793': 'pdf-layout', 'expressions.md:813': 'pdf-layout', 'bibliography.md:1': 'pdf-layout',
 'documentation-comments.md:112': 'pdf-layout', 'documentation-comments.md:232': 'pdf-layout', 'expressions.md:3828': 'pdf-layout',
 **{k: 'grammar-comment-layout' for k in ('lexical-structure.md:467', 'lexical-structure.md:469', 'lexical-structure.md:740', 'lexical-structure.md:829', 'lexical-structure.md:965', 'lexical-structure.md:967', 'lexical-structure.md:1032', 'grammar.md:524')},
 **{k: 'code-comment-layout' for k in ('conversions.md:39', 'expressions.md:1544', 'expressions.md:2915', 'classes.md:228', 'classes.md:1645', 'attributes.md:111', 'documentation-comments.md:230', 'documentation-comments.md:587')},
 'expressions.md:3922': 'informative-edit',
 **{f'expressions.md:{n}': 'table-layout' for n in (3373, 3493, 3622, 3642, 3818)},
 'standard-library.md:647': 'table-layout', 'standard-library.md:789': 'table-layout', 'documentation-comments.md:90': 'table-layout',
}
rows, used_w, used_p = [], set(), set()
for r in diff['workingTextOnly']:
    key = f"{r['file']}:{r['line']}"
    if key not in W: raise SystemExit('No disposition for working-text block ' + key)
    used_w.add(key); cat = W[key]; bearing = r['kind'] == 'grammar'
    if bearing and cat != 'grammar-comment-layout': raise SystemExit(key + ': a grammar block needs grammar-comment-layout')
    rows.append(dict(direction='working-text-only', at=key, kind=r['kind'], sha256=r['sha256'], disposition=cat, syntaxBearing=bearing,
                     verifiedBy='tools/compare-csharp6-pdf.py' if bearing else None, reason=CATEGORIES[cat]))
for r in diff['pdfOnly']:
    key = r['before']
    if key not in P: raise SystemExit('No disposition for PDF-only text before ' + key)
    used_p.add(key); cat = P[key]; bearing = cat == 'grammar-comment-layout'
    rows.append(dict(direction='pdf-only', at=key, kind='pdf', sha256=r['sha256'], disposition=cat, syntaxBearing=bearing,
                     verifiedBy='tools/compare-csharp6-pdf.py' if bearing else None, reason=CATEGORIES[cat]))
stale = sorted(set(W) - used_w) + sorted(set(P) - used_p)
if stale: raise SystemExit('Stale dispositions: ' + ', '.join(stale))
record = dict(schemaVersion=2, profile='CSharp6', differencesSha256=hashlib.sha256((ROOT / SRC).read_bytes()).hexdigest(), categories=CATEGORIES,
              counts=dict(blocks=diff['counts']['blocks'], aligned=diff['counts']['aligned'], workingTextOnly=len(diff['workingTextOnly']), pdfOnly=len(diff['pdfOnly']),
                          syntaxBearing=sum(r['syntaxBearing'] for r in rows)), dispositions=rows)
text = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
path = ROOT / 'docs/coverage/csharp6-main-text-dispositions.json'
if args.check:
    if path.read_text(encoding='utf-8') != text: raise SystemExit('csharp6-main-text-dispositions.json is stale')
    print('current')
else:
    path.write_text(text, encoding='utf-8'); print(record['counts'])
