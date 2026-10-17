"""Generate explicit UTF-16 identifier classes, and escapes limited to them, from pinned runtime category data."""
import json,sys,re
from pathlib import Path
root=Path(__file__).resolve().parents[1]
data=json.loads(Path(sys.argv[1]).read_text())
original_data=dict(data)
csharp2=json.loads((root/'docs/coverage/csharp2-unicode-data.json').read_text())
historical=json.loads((root/'docs/coverage/historical-unicode-data.json').read_text())
for key in ['start','part']:data[key]=sorted(set(data[key])|set(historical[key])|{n for n in csharp2[key] if n<=65535})
q=lambda x:json.dumps(x)
def ranges(values):
 result=[];lo=prev=values[0]
 for x in values[1:]+[0x10001]:
  if x==prev+1:prev=x;continue
  a=f"'\\u{lo:04x}'";b=f"'\\u{prev:04x}'"
  result.append(a if lo==prev else a+'..'+b);lo=prev=x
 return '['+' '.join(result)+']'
lines=['// Generated identifier superset from pinned current, Unicode 3.0 and Unicode 4.0 tables; apply the selected profile Unicode policy.', 'syntax module CSharp.Lexical {',
 '// The hex digit d, in either case (Nitrogen grammar functions are evaluated when the grammar compiles).',
 'function HexDigit(d: Grammar.Int) : Grammar.Rule = if d < 10 then [char(48 + d)] else [char(55 + d) char(87 + d)];',
 '// Exactly `width` hex digits, in either case, whose value is a member of `values`.',
 'function HexIn(width: Grammar.Int, values: Grammar.Class) : Grammar.Rule =',
 '  if width == 0 then ()',
 '  else choice d in 0 .. 15 {',
 '    let size = 16 ^ (width - 1);',
 '    let low = d * size;',
 '    let part = values & [low .. (low + size - 1)];',
 '    where part != [] : HexDigit(d) HexIn(width - 1, part - low)',
 '  };']
for label,key in [('Start','start'),('Part','part')]:
 values=data[key];lines.append(f'token Id{label} = {ranges(values)};')
 supplementary=[n for n in csharp2[key] if n>65535]
 pairs={}
 for n in supplementary:
  high=0xd800+((n-0x10000)>>10);low=0xdc00+((n-0x10000)&1023)
  pairs.setdefault(high,[]).append(low)
 raw=' / '.join(ranges([h])+' '+ranges(v) for h,v in sorted(pairs.items()))
 lines.append(f'token Scalar{label} = {raw};')
 escapes=[f'"\\\\u" HexIn(4, Id{label})',f'"\\\\U0000" HexIn(4, Id{label})']
 for plane in range(1,17):
  values=[n&65535 for n in supplementary if n>>16==plane]
  if values:
   prefix=' '.join(("['"+c+"' '"+c.upper()+"']") if c in 'abcdef' else q(c) for c in f'{plane:04x}')
   escapes.append('"\\\\U" '+prefix+f' HexIn(4, {ranges(values)})')
 lines.append(f'token Unicode{label} = '+ ' / '.join(escapes)+';')
words=re.search(r'except (.*?);', (root/'src/CSNitrogen.Syntax/Grammars/Core.ngr').read_text())
if words: reserved=words[1]
else: reserved=(root/'docs/coverage/reserved-keywords.txt').read_text().strip()
(root/'docs/coverage/reserved-keywords.txt').write_text(reserved+'\n')
lines.append('token Identifier = "@"? (IdStart / ScalarStart / UnicodeStart) (IdPart / ScalarPart / UnicodePart)* except '+reserved+';')
lines += [
 'token Hex = [\'0\'..\'9\' \'a\'..\'f\' \'A\'..\'F\'];',
 'token Digits = [\'0\'..\'9\']+;',
 'token Integer = ("0" [\'x\' \'X\'] Hex+ / Digits) ([\'u\' \'U\'] [\'l\' \'L\']? / [\'l\' \'L\'] [\'u\' \'U\']?)?;',
 'token Exponent = [\'e\' \'E\'] [\'+\' \'-\']? Digits;',
 'token RealSuffix = [\'f\' \'F\' \'d\' \'D\' \'m\' \'M\'];',
 'token Real = (Digits? "." Digits Exponent? RealSuffix? / Digits Exponent RealSuffix? / Digits RealSuffix);',
 'token SimpleEscape = [\'\\\'\' \'"\' \'\\\\\' \'0\' \'a\' \'b\' \'f\' \'n\' \'r\' \'t\' \'v\'];',
 'token Escape16 = "u" Hex Hex Hex Hex / "x" Hex Hex? Hex? Hex? / "U0000" Hex Hex Hex Hex;',
 'token Escape32 = "U" ("000" Hex Hex Hex Hex Hex / "0010" Hex Hex Hex Hex);',
 'token Character = "\'" ([^ \'\\\'\' \'\\\\\' \'\\r\' \'\\n\' \'\\u2028\' \'\\u2029\'] / "\\\\" (SimpleEscape / Escape16)) "\'";',
 'token RegularString = "\\\"" ([^ \'"\' \'\\\\\' \'\\r\' \'\\n\' \'\\u2028\' \'\\u2029\'] / "\\\\" (SimpleEscape / Escape16 / Escape32))* "\\\"";',
 'token VerbatimString = "@\\\"" ("\\\"\\\"" / !"\\\"" .)* "\\\"";',
 'token ConditionalSymbol = "@"? (IdStart / ScalarStart / UnicodeStart) (IdPart / ScalarPart / UnicodePart)* except \"true\" \"false\";', 'syntax ConditionalSymbolFragment = Value:ConditionalSymbol;', 'syntax IdentifierFragment = Value:Identifier;', 'syntax IntegerFragment = Value:Integer;',
 'syntax RealFragment = Value:Real;', 'syntax CharacterFragment = Value:Character;',
 'syntax StringFragment = Value:(VerbatimString / RegularString);','}']
(root/'src/CSNitrogen.Syntax/Grammars/Lexical.ngr').write_text('\n'.join(lines)+'\n')
(root/'docs/coverage/unicode-data.json').write_text(json.dumps(original_data,separators=(',',':'))+'\n')
print('Generated',len(lines),'lexical grammar lines')

import runpy
runpy.run_path(str(root/'eng/generate-contextual-identifiers.py'))
