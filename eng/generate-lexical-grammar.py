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
lines += [
 '// The UTF-16 surrogate pairs of the code points in `plane` whose low 16 bits are in `values`.',
 'function Pairs(plane: Grammar.Int, values: Grammar.Class) : Grammar.Rule =',
 '  choice h in 0 .. 63 {',
 '    let part = values & [(h * 1024) .. (h * 1024 + 1023)];',
 '    let low = part - h * 1024 + 56320;',
 '    where part != [] : [(55296 + (plane - 1) * 64 + h)] low',
 '  };',
 '// Those code points as a \\U escape when `escaped` is 1, otherwise as surrogate pairs.',
 'function Plane(escaped: Grammar.Int, plane: Grammar.Int, values: Grammar.Class) : Grammar.Rule =',
 '  if escaped == 1 then "\\\\U" HexIn(4, [(plane)]) HexIn(4, values) else Pairs(plane, values);']
# Each identifier set is a superset of the start set, so a part rule lists only what it adds.
start=set(data['start']);start_supplementary={n for n in csharp2['start'] if n>65535}
def planes(values):return ' / '.join(f'Plane(escaped, {p}, {ranges([n&65535 for n in values if n>>16==p])})' for p in sorted({n>>16 for n in values}))
for label,key in [('Start','start'),('Part','part')]:
 supplementary={n for n in csharp2[key] if n>65535}
 lines.append(f'// Supplementary identifier {key} characters, escaped or as surrogate pairs.')
 body=planes(supplementary) if key=='start' else 'StartPlanes(escaped) / '+planes(supplementary-start_supplementary)
 lines.append(f'function {label}Planes(escaped: Grammar.Int) : Grammar.Rule = {body};')
for label,key in [('Start','start'),('Part','part')]:
 lines.append(f'token Id{label} = {ranges(data[key])};' if key=='start' else f'token IdPart = IdStart / {ranges(sorted(set(data[key])-start))};')
 lines.append(f'token Scalar{label} = {label}Planes(0);')
 lines.append(f'token Unicode{label} = "\\\\u" HexIn(4, Id{label}) / "\\\\U0000" HexIn(4, Id{label}) / {label}Planes(1);')
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
