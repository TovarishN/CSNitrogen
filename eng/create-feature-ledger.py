"""Create a conservative inventory from the checked-in official history snapshot.
Entries are candidates: specification review is required before they can be verified.
"""
import hashlib, json, re
from pathlib import Path
root = Path(__file__).resolve().parents[1]
source = root / 'docs/coverage/sources/Language-Version-History.md'
url = 'https://github.com/dotnet/csharplang/blob/main/Language-Version-History.md'
items = []
version = None
semantic_terms = ['conversion', 'overload resolution', 'natural type', 'definite assignment',
    'caller info', 'callerargument', 'async main', 'inferred tuple', 'expression trees',
    'expression` trees', 'intptr', 'auto-default', 'lock` object', 'implicit conversions',
    'source generator', 'compiler-as-a-service', 'reference assemblies', 'draft specification',
    'foreach loop was', 'dispose in foreach', 'foreach over string', 'localsinit',
    'method-level asyncmethodbuilder', 'module initializer', 'embedded interop',
    'generic math', 'first-class `span`', 'target-typed conditional', 'covariant return',
    'relaxing shift operator', 'pattern match `span', 'expression` trees']
for number, line in enumerate(source.read_text().splitlines(), 1):
    heading = re.match(r'# .*?C# (\d+)(?:\.(\d+))?', line)
    if heading:
        version = f'{int(heading[1])}.{int(heading[2] or 0)}'
        continue
    if not version or int(version.split('.')[0]) > 14 or not re.match(r'-\s', line):
        continue
    text = re.sub(r'^-\s*', '', line).strip()
    first = re.match(r'\[([^\]]+)\]\(([^)]+)\)', text)
    title, reference = (first[1], first[2]) if first else (text.split(': ')[0], url)
    plain = title.replace('`', '')
    slug = re.sub(r'[^a-z0-9]+', '-', plain.lower()).strip('-')
    semantic = any(term in title.lower() for term in semantic_terms)
    items.append(dict(id=f'csharp-{version}.{slug}', title=plain, introduced=version,
        classification='semantic-or-tooling' if semantic else 'syntax-or-policy',
        reviewed=False, reference=reference, inventoryLine=number,
        requires=[], conflicts=[], grammars=[], policies=[], fixtures=[], status='unimplemented'))
# Baseline categories fill gaps in the history's intentionally short v1 overview.
for category in ['unicode-identifiers','escaped-identifiers','numeric-literals','character-literals',
                 'regular-strings','verbatim-strings','trivia','conditional-compilation','location-directives',
                 'diagnostic-directives','names-and-types','attributes','namespace-declarations',
                 'members','statements','constant-expressions','statement-expressions']:
    items.append(dict(id='baseline.'+category,title=category,introduced='1.0',classification='syntax-or-policy',
        reviewed=False,reference='https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/grammar',
        requires=[],conflicts=[],grammars=[],policies=[],fixtures=[],status='unimplemented'))
items.append(dict(id='foundation.empty-classes',title='Empty classes with ASCII names',introduced='1.0',
    classification='syntax-or-policy',reviewed=True,
    reference='https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes',
    requires=[],conflicts=[],grammars=['Grammars/Core.ngr'],policies=[],
    fixtures=['tests/CSNitrogen.Tests/GeneratedSyntaxTests.cs'],status='verified'))
result=dict(source=url,sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
    note='Inventory candidates require feature-spec review. No profile is complete; history headings alone do not establish precise syntax boundaries.',
    features=sorted(items,key=lambda x:x['id']))
(root/'docs/coverage/features.json').write_text(json.dumps(result,indent=2)+'\n')
print(f'Wrote {len(items)} coverage entries')
