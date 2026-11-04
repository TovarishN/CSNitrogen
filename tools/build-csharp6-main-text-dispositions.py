#!/usr/bin/env python3
"""Disposition every main-text paragraph that compare-csharp6-prose.py could not locate in the PDF.

Each unmatched paragraph gets one reviewed disposition; the PDF governs every difference. A paragraph
with no disposition, or a disposition left for a paragraph that now matches, fails the build.
--check compares with the committed record.
"""
import argparse, hashlib, json
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
SRC = 'docs/coverage/csharp6-main-text-differences.json'
diff = json.loads((ROOT / SRC).read_text())
LAYOUT = 'PDF lays this out differently (heading split across lines, or table cells read in another order); content reviewed as equivalent and not syntax-bearing'
TABLE = 'HTML table converted from Word; the PDF reads its cells in another order. Semantic or library content (operator results, member forms, format strings, documentation tags), not syntax-bearing'
DISPOSITIONS = {
 ('bibliography.md', 1): ('annex-heading-layout', LAYOUT),
 ('documentation-comments.md', 1): ('annex-heading-layout', LAYOUT),
 ('portability-issues.md', 1): ('annex-heading-layout', LAYOUT),
 ('standard-library.md', 1): ('annex-heading-layout', LAYOUT),
 ('documentation-comments.md', 69): ('table-layout', TABLE),
 ('expressions.md', 419): ('table-layout', TABLE),
 ('expressions.md', 3215): ('table-layout', TABLE),
 ('expressions.md', 3348): ('table-layout', TABLE),
 ('expressions.md', 3479): ('table-layout', TABLE),
 ('expressions.md', 3609): ('table-layout', TABLE),
 ('expressions.md', 3775): ('table-layout', TABLE),
 ('standard-library.md', 474): ('table-layout', TABLE),
 ('standard-library.md', 647): ('table-layout', TABLE),
 ('standard-library.md', 663): ('table-layout', TABLE),
 ('introduction.md', 23): ('markdown-comment', 'Inside an HTML comment of the working text (an unpublished copyright notice); not part of the published standard'),
 ('introduction.md', 27): ('markdown-comment', 'Inside an HTML comment of the working text (an unpublished copyright notice); not part of the published standard'),
 ('expressions.md', 1541): ('working-text-only-informative', 'An explanatory sentence inside an informative example (§11.7.6.2) added after the PDF; the PDF governs and does not contain it. No syntax rule depends on it'),
}
rows, seen = [], set()
for r in diff['unmatched']:
    key = (r['file'], r['line'])
    if key not in DISPOSITIONS: raise SystemExit(f'No disposition for {key}')
    seen.add(key); kind, reason = DISPOSITIONS[key]
    rows.append(dict(file=r['file'], line=r['line'], sha256=r['sha256'], disposition=kind, syntaxBearing=False, reason=reason))
stale = sorted(set(DISPOSITIONS) - seen)
if stale: raise SystemExit(f'Stale dispositions: {stale}')
record = dict(schemaVersion=1, profile='CSharp6', differencesSha256=hashlib.sha256((ROOT / SRC).read_bytes()).hexdigest(),
              counts=dict(paragraphs=diff['counts']['paragraphs'], matched=diff['counts']['matched'], dispositioned=len(rows), syntaxBearing=sum(r['syntaxBearing'] for r in rows)),
              dispositions=rows)
text = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
path = ROOT / 'docs/coverage/csharp6-main-text-dispositions.json'
if args.check:
    if path.read_text(encoding='utf-8') != text: raise SystemExit('csharp6-main-text-dispositions.json is stale')
    print('current')
else:
    path.write_text(text, encoding='utf-8'); print(record['counts'])
