#!/usr/bin/env python3
"""Index normalized main-text differences between pinned C#3/4 DOCs; no approval is inferred.

Requires macOS textutil. External source text is not redistributed. LF line
identity is preserved; U+2028 inside Word paragraphs is normalized as whitespace.
"""
import argparse
import difflib
import hashlib
import json
from pathlib import Path
import re
import subprocess
ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser(description=__doc__)
p.add_argument('third_doc', type=Path)
p.add_argument('fourth_doc', type=Path)
p.add_argument('--check', action='store_true')
a = p.parse_args()
def sha(b): return hashlib.sha256(b).hexdigest()
third = 'ec96847c92a357626c7bd868cd3782b975c309eb82cd70c0509e8873231912d1'
fourth = 'cd82c06f12b83836f888d127ed59216c3505c4fc23a67c59c0fc3f46a087faf7'
def paragraphs(path, expected):
 if sha(path.read_bytes()) != expected: raise SystemExit('Wrong pinned DOC: ' + str(path))
 raw = subprocess.check_output(['/usr/bin/textutil', '-convert', 'txt', '-stdout', str(path)]).decode('utf-8')
 lines = raw.split('\n'); end = lines.index('Grammar'); rows = []
 for n, line in enumerate(lines[:end], 1):
  if not line.strip() or 'PAGEREF' in line or line.strip().startswith('Copyright'): continue
  line = re.sub(r'REF [A-Za-z0-9_]+(?: \\[rwhn])*(?: [0-9]+(?:\.[0-9]+)*)?', '<ref>', line)
  line = re.sub(r'STYLEREF [^ ]+(?: \\[a-z]+)*(?: [0-9]+(?:\.[0-9]+)*)?', '<style>', line)
  rows.append((n, ' '.join(line.split())))
 return rows, sha(raw.encode())
old, old_sha = paragraphs(a.third_doc, third)
new, new_sha = paragraphs(a.fourth_doc, fourth)
s = difflib.SequenceMatcher(None, [x[1] for x in old], [x[1] for x in new], autojunk=False)
regions = []
for op, i, j, k, l in s.get_opcodes():
 if op == 'equal': continue
 def segment(rows, start, end):
  return dict(startLine=rows[start][0] if start < len(rows) else None,
              endLine=rows[end-1][0] if end > start else None,
              paragraphCount=end-start, normalizedSha256=sha('\n'.join(x[1] for x in rows[start:end]).encode()))
 regions.append(dict(id=f'D:{len(regions)+1:03}', kind=op, third=segment(old,i,j), fourth=segment(new,k,l)))
result = dict(schemaVersion=1, profile='CSharp4', status='difference-index-unreviewed',
 thirdSourceSha256=third, sourceSha256=fourth, thirdTextSha256=old_sha, fourthTextSha256=new_sha,
 normalization='LF line identities; before Appendix Grammar; omit blank/TOC PAGEREF/Copyright header lines; normalize REF/STYLEREF fields and whitespace. This comparison is an analysis aid, not normative equivalence or admission.',
 regions=regions)
output = json.dumps(result, indent=2) + '\n'
path = ROOT / 'docs/coverage/csharp4-main-text-differences.json'
if a.check:
 if not path.exists() or path.read_text() != output: raise SystemExit('Fresh main-text comparison differs')
else: path.write_text(output)
print(len(regions), 'main-text difference regions; normative dispositions separate')
