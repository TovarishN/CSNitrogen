"""Emit finite spelling rules for the two global attribute target identifiers.

Identifier equality ignores @, Unicode escapes and formatting characters.
BMP formatting membership is checked independently against .NET/Roslyn tests.
"""
import json
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
path = root / 'src/CSNitrogen.Syntax/Grammars/Attributes.ngr'
q = json.dumps


def spellings(c):
    digits = ''.join("['a' 'A']" if d == 'a' else "['b' 'B']" if d == 'b' else
                     "['c' 'C']" if d == 'c' else "['d' 'D']" if d == 'd' else
                     "['e' 'E']" if d == 'e' else "['f' 'F']" if d == 'f' else q(d)
                     for d in f'{ord(c):04x}')
    return '(' + q(c) + ' / "\\\\u" ' + digits + ' / "\\\\U0000" ' + digits + ')'


format_data = json.loads((root / 'tests/Corpus/attribute-target-formatting.json').read_text())
formats = [chr(n) for n in format_data['format']]
format_rule = ' token GlobalTargetFormat = ' + ' / '.join(spellings(c) for c in formats) + ';'
words = [' '.join(spellings(c) + ' GlobalTargetFormat*' for c in word) for word in ('assembly', 'module')]
global_rule = ' token GlobalAttributeTarget = "@"? (' + ' / '.join(words) + ') !(CSharp.Lexical.IdPart / CSharp.Lexical.UnicodePart);'
keywords = re.findall(r'"([a-z]+)"', (root / 'docs/coverage/reserved-keywords.txt').read_text())
keyword_rule = ' syntax AttributeTarget = CSharp.Lexical.Identifier / ' + ' / '.join(q(w) for w in keywords) + ';'
block = '// BEGIN GENERATED ATTRIBUTE TARGETS\n' + format_rule + '\n' + global_rule + '\n' + keyword_rule + '\n // END GENERATED ATTRIBUTE TARGETS'
source = path.read_text()
if '// BEGIN GENERATED ATTRIBUTE TARGETS' in source:
    source = re.sub(r'// BEGIN GENERATED ATTRIBUTE TARGETS.*?// END GENERATED ATTRIBUTE TARGETS', lambda _: block, source, flags=re.S)
else:
    source = source.replace('{', '{\n ' + block, 1)
path.write_text(source)
