"""Contextual identifier spellings from pinned Unicode category data.
ECMA-334 fourth edition §9.4.3: these terminals are identifiers, permitting @,
Unicode escapes and ignored formatting. The selected Unicode policy validates
accepted spellings after generated parsing.
"""
import json,re
from pathlib import Path
root=Path(__file__).resolve().parents[1]
q=json.dumps
def spelling(c):
 digits=' '.join("['"+d+"' '"+d.upper()+"']" if d in 'abcdef' else q(d) for d in f'{ord(c):04x}')
 return '('+q(c)+' / "\\\\u" '+digits+' / "\\\\U0000" '+digits+')'
formats=set()
for file in ['historical-unicode-data.json','csharp2-unicode-data.json']:
 formats.update(json.loads((root/'docs/coverage'/file).read_text())['format'])
formats.update(json.loads((root/'tests/Corpus/attribute-target-formatting.json').read_text())['format'])
forms=[]
for n in sorted(formats):
 if n<=65535:forms.append(spelling(chr(n)))
 else:
  high=0xd800+((n-0x10000)>>10);low=0xdc00+((n-0x10000)&1023)
  forms.append(f"['\\u{high:04x}'] ['\\u{low:04x}']")
  hexes=' '.join("['"+d+"' '"+d.upper()+"']" if d in 'abcdef' else q(d) for d in f'{n:08x}')
  forms.append('"\\\\U" '+hexes)
lines=['// BEGIN GENERATED CONTEXTUAL IDENTIFIERS','token ContextualFormat = '+' / '.join(forms)+';']
for word in ['add','alias','get','global','partial','remove','set','value','where','yield']:
 body=' '.join(spelling(c)+' ContextualFormat*' for c in word)
 lines.append(f'token Contextual{word.title()} = "@"? ({body}) !(IdPart / ScalarPart / UnicodePart);')
lines.append('// END GENERATED CONTEXTUAL IDENTIFIERS')
p=root/'src/CSNitrogen.Syntax/Grammars/Lexical.ngr';s=p.read_text();s=re.sub(r'// BEGIN GENERATED CONTEXTUAL IDENTIFIERS.*?// END GENERATED CONTEXTUAL IDENTIFIERS\n?', '',s,flags=re.S);s=s[:s.rfind('}')]+ '\n'.join(lines)+'\n}\n';p.write_text(s)
