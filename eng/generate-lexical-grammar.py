"""Generate explicit BMP identifier classes/escape tries from pinned runtime category data."""
import json,sys,re
from pathlib import Path
root=Path(__file__).resolve().parents[1]
data=json.loads(Path(sys.argv[1]).read_text())
original_data=dict(data)
historical=json.loads((root/'docs/coverage/historical-unicode-data.json').read_text())
for key in ['start','part']:data[key]=sorted(set(data[key])|set(historical[key]))
q=lambda x:json.dumps(x)
def cls(chars):
 return '['+' '.join("'"+c+"'" for c in sorted(set(chars)))+']'
def digit(n):
 s=f'{n:x}';return cls(s+s.upper())
def ranges(values):
 result=[];lo=prev=values[0]
 for x in values[1:]+[0x10001]:
  if x==prev+1:prev=x;continue
  a=f"'\\u{lo:04x}'";b=f"'\\u{prev:04x}'"
  result.append(a if lo==prev else a+'..'+b);lo=prev=x
 return '['+' '.join(result)+']'
lines=['// Generated BMP identifier superset from pinned current and Unicode 3.0 tables; apply the selected profile Unicode policy.', 'syntax module CSharp.Lexical {']
for label,key in [('Start','start'),('Part','part')]:
 values=data[key];lines.append(f'token Id{label} = {ranges(values)};')
 groups={}
 for x in values:groups.setdefault(x>>8,{}).setdefault((x>>4)&15,[]).append(x&15)
 names=[]
 for high,lowgroups in sorted(groups.items()):
  name=f'Esc{label}{high:02x}';names.append(name)
  tail=' / '.join(digit(mid)+' '+cls(''.join(f'{last:x}'+f'{last:X}' for last in lastvalues)) for mid,lastvalues in sorted(lowgroups.items()))
  lines.append(f'token {name} = {digit(high>>4)} {digit(high&15)} ({tail});')
 lines.append(f'token Esc{label}4 = '+' / '.join(names)+';')
 lines.append(f'token Unicode{label} = "\\\\u" Esc{label}4 / "\\\\U0000" Esc{label}4;')
words=re.search(r'except (.*?);', (root/'src/CSNitrogen.Syntax/Grammars/Core.ngr').read_text())
if words: reserved=words[1]
else: reserved=(root/'docs/coverage/reserved-keywords.txt').read_text().strip()
(root/'docs/coverage/reserved-keywords.txt').write_text(reserved+'\n')
lines.append('token Identifier = "@"? (IdStart / UnicodeStart) (IdPart / UnicodePart)* except '+reserved+';')
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
 'token Character = "\'" ([^ \'\\\'\' \'\\\\\' \'\\r\' \'\\n\' \'\\u0085\' \'\\u2028\' \'\\u2029\'] / "\\\\" (SimpleEscape / Escape16)) "\'";',
 'token RegularString = "\\\"" ([^ \'"\' \'\\\\\' \'\\r\' \'\\n\' \'\\u0085\' \'\\u2028\' \'\\u2029\'] / "\\\\" (SimpleEscape / Escape16 / Escape32))* "\\\"";',
 'token VerbatimString = "@\\\"" ("\\\"\\\"" / !"\\\"" .)* "\\\"";',
 'token ConditionalSymbol = (IdStart / UnicodeStart) (IdPart / UnicodePart)* except \"true\" \"false\";', 'syntax ConditionalSymbolFragment = Value:ConditionalSymbol;', 'syntax IdentifierFragment = Value:Identifier;', 'syntax IntegerFragment = Value:Integer;',
 'syntax RealFragment = Value:Real;', 'syntax CharacterFragment = Value:Character;',
 'syntax StringFragment = Value:(VerbatimString / RegularString);','}']
(root/'src/CSNitrogen.Syntax/Grammars/Lexical.ngr').write_text('\n'.join(lines)+'\n')
(root/'docs/coverage/unicode-data.json').write_text(json.dumps(original_data,separators=(',',':'))+'\n')
print('Generated',len(lines),'lexical grammar lines')
