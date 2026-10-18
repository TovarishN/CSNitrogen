"""Contextual identifier spellings from pinned Unicode category data.
ECMA-334 fourth edition §9.4.3: these terminals are identifiers, permitting @,
Unicode escapes and ignored formatting. The selected Unicode policy validates
accepted spellings after generated parsing.
"""
import json,re
from pathlib import Path
root=Path(__file__).resolve().parents[1]
def ranges(values):
 result=[];lo=prev=values[0]
 for x in values[1:]+[0x110000]:
  if x==prev+1:prev=x;continue
  a=f"'\\u{lo:04x}'";b=f"'\\u{prev:04x}'"
  result.append(a if lo==prev else a+'..'+b);lo=prev=x
 return '['+' '.join(result)+']'
formats=set()
for file in ['historical-unicode-data.json','csharp2-unicode-data.json']:
 formats.update(json.loads((root/'docs/coverage'/file).read_text())['format'])
formats.update(json.loads((root/'tests/Corpus/attribute-target-formatting.json').read_text())['format'])
def spans(values):
 result=[];lo=prev=values[0]
 for x in values[1:]+[0x110000]:
  if x==prev+1:prev=x;continue
  result.append((lo,prev));lo=prev=x
 return result
bmp=sorted(n for n in formats if n<=65535)
supplementary=sorted(n for n in formats if n>65535)
lines=['// BEGIN GENERATED CONTEXTUAL IDENTIFIERS',
 '// A member of `values`, written directly or as a \\u or \\U0000 escape.',
 'function Escaped(values: Grammar.Class) : Grammar.Rule = values / "\\\\u" HexIn(4, values) / "\\\\U0000" HexIn(4, values);',
 '// A supplementary code point n, as its UTF-16 surrogate pair or a \\U escape.',
 'function Supplementary(n: Grammar.Int) : Grammar.Rule =',
 '  let s = n - 65536;',
 '  [(55296 + s / 1024)] [(56320 + s % 1024)] / "\\\\U" HexIn(4, [(n / 65536)]) HexIn(4, [(n % 65536)]);',
 '// Each supplementary code point from low through high.',
 'function SupplementaryIn(low: Grammar.Int, high: Grammar.Int) : Grammar.Rule = choice n in low .. high { Supplementary(n) };',
 '// The identifier spelling of `word`: escapes and ignored formatting allowed, no identifier character after it.',
 'function Contextual(word: Grammar.Text) : Grammar.Rule =',
 '  "@"? (seq c in word { Escaped([c]) ContextualFormat* }) !(IdPart / ScalarPart / UnicodePart);',
 'token ContextualFormat = Escaped('+ranges(bmp)+') / '+' / '.join(f'SupplementaryIn({lo}, {hi})' for lo,hi in spans(supplementary))+';']
for word in ['add','alias','get','global','partial','remove','set','value','where','yield']:
 lines.append(f'token Contextual{word.title()} = Contextual("{word}");')
lines.append('// END GENERATED CONTEXTUAL IDENTIFIERS')
p=root/'src/CSNitrogen.Syntax/Grammars/Lexical.ngr';s=p.read_text();s=re.sub(r'// BEGIN GENERATED CONTEXTUAL IDENTIFIERS.*?// END GENERATED CONTEXTUAL IDENTIFIERS\n?', '',s,flags=re.S);s=s[:s.rfind('}')]+ '\n'.join(lines)+'\n}\n';p.write_text(s)
