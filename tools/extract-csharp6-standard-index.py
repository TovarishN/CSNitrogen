#!/usr/bin/env python3
"""Index ANTLR grammar rules from the pinned ECMA-334 6th edition working text.

Input: the standard/ directory of dotnet/csharpstandard tag V6-standard (commit pinned in
csharp6-reference-policy.json). Nothing is downloaded or redistributed. Every rule of the
consolidated grammar (grammar.md) is indexed with its top-level alternatives and source hashes,
and linked to the main-text chapter block that defines it. --check compares a fresh extraction with
the committed index.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs/coverage/csharp6-standard-index.json'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('standard', type=Path, help='path to the V6-standard standard/ directory')
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
policy = json.loads((ROOT / 'docs/coverage/csharp6-reference-policy.json').read_text())
work = policy['workingText']
grammar_bytes = (args.standard / 'grammar.md').read_bytes()
if hashlib.sha256(grammar_bytes).hexdigest() != work['grammarSha256']:
    raise SystemExit('grammar.md does not match the pinned V6-standard working text')

def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()

def strip_comment(line):
    out, quote = [], None
    i = 0
    while i < len(line):
        c = line[i]
        if quote:
            out.append(c)
            if c == '\\' and i + 1 < len(line):
                out.append(line[i + 1]); i += 2; continue
            if c == quote: quote = None
        elif c in "'[":
            quote = ']' if c == '[' else "'"; out.append(c)
        elif line.startswith('//', i):
            break
        else:
            out.append(c)
        i += 1
    return ''.join(out).rstrip()

def alternatives(body):
    """Split a rule body (text after ':' up to ';') at top-level '|'."""
    parts, depth, quote, current = [], 0, None, []
    i = 0
    while i < len(body):
        c = body[i]
        if quote:
            current.append(c)
            if c == '\\' and i + 1 < len(body):
                current.append(body[i + 1]); i += 2; continue
            if c == quote: quote = None
        elif c == "'": quote = "'"; current.append(c)
        elif c == '[': quote = ']'; current.append(c)
        elif c == '(': depth += 1; current.append(c)
        elif c == ')': depth -= 1; current.append(c)
        elif c == '|' and depth == 0:
            parts.append(''.join(current)); current = []
        else: current.append(c)
        i += 1
    parts.append(''.join(current))
    return [' '.join(p.split()) for p in parts]

RULE = re.compile(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*$')
def rules_in(lines, first_line=1):
    """Yield (name, fragment, line, body, section) for each ANTLR rule in ANTLR-fenced blocks."""
    fenced, section, i = False, None, 0
    while i < len(lines):
        raw = lines[i]
        if raw.startswith('```'):
            fenced = raw.strip() == '```ANTLR' if not fenced else False
            i += 1; continue
        if fenced:
            m = re.match(r'^// Source: (§\S+)', raw)
            if m: section = m.group(1)
            one = re.match(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*:(.*);\s*$', strip_comment(raw))
            if one:
                yield one.group(2), bool(one.group(1)), i + first_line, one.group(3).strip(), section
                i += 1; continue
            colon = re.match(r'^(fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*:\s*$', strip_comment(raw))
            if colon:
                # name and ':' on one line, body on the following lines
                start, body = i, []
                i += 1
                while i < len(lines):
                    text = strip_comment(lines[i]).strip()
                    if text == ';' or text.endswith(';') and not text.endswith("';'"):
                        body.append(text[:-1]); break
                    body.append(text); i += 1
                yield colon.group(2), bool(colon.group(1)), start + first_line, ' '.join(body).strip(), section
                i += 1; continue
            m = RULE.match(strip_comment(raw))
            nxt = i + 1
            while m and nxt < len(lines) and strip_comment(lines[nxt]).strip() == '' and lines[nxt].strip().startswith('//'):
                nxt += 1  # a comment line between the rule name and its ':'
            if m and nxt < len(lines) and strip_comment(lines[nxt]).lstrip().startswith(':'):
                start, body = i, []
                i = nxt
                while i < len(lines):
                    text = strip_comment(lines[i]).strip()
                    if text == ';' or text.endswith(';') and not text.endswith("';'"):
                        body.append(text[:-1]); break
                    body.append(text); i += 1
                joined = ' '.join(body).strip()
                if joined.startswith(':'): joined = joined[1:]
                yield m.group(2), bool(m.group(1)), start + first_line, joined, section
        i += 1

grammar_lines = grammar_bytes.decode('utf-8').split('\n')
def headings(lines):
    fenced, found = False, []
    for n, raw in enumerate(lines, 1):
        if raw.startswith('```'):
            fenced = raw.strip() == '```ANTLR' if not fenced else False; continue
        if fenced and re.match(r'^(fragment\s+)?[A-Za-z_][A-Za-z0-9_]*\s*(:.*)?$', strip_comment(raw)) and strip_comment(raw).strip():
            found.append((n, re.match(r'^(?:fragment\s+)?([A-Za-z_][A-Za-z0-9_]*)', raw).group(1)))
    return found
main = {}
for chapter in sorted(p for p in args.standard.glob('*.md') if p.name != 'grammar.md'):
    for name, _, line, body, _ in rules_in(chapter.read_text(encoding='utf-8').split('\n')):
        main.setdefault(name, []).append({'file': chapter.name, 'line': line, 'bodySha256': sha(' '.join(body.split()))})
productions, counts = [], {'lexical': 0, 'syntactic': 0, 'unsafe': 0}
seen = set()
for name, fragment, line, body, section in rules_in(grammar_lines):
    if name in seen:
        raise SystemExit('Duplicate grammar rule: ' + name)
    seen.add(name)
    lexical = fragment or name[0].isupper()
    alts = alternatives(body)
    kind = 'unsafe' if section and section.startswith('§22') else 'lexical' if lexical else 'syntactic'
    counts[kind] += 1
    normalized = ' '.join(body.split())
    occurrences = main.get(name, [])
    productions.append({
        'id': f'A:{line}:{name}', 'name': name, 'line': line, 'grammar': kind, 'fragment': fragment, 'section': section,
        'sourceBodySha256': sha(normalized),
        'alternatives': [{'id': f'A:{line}:{name}/{n}', 'ordinal': n, 'text': a, 'sourceAlternativeSha256': sha(a)} for n, a in enumerate(alts, 1)],
        'mainTextOccurrences': occurrences,
        'mainTextAgrees': bool(occurrences) and all(o['bodySha256'] == sha(normalized) for o in occurrences),
    })
indexed = {p['name'] for p in productions}
unindexed = sorted({name for _, name in headings(grammar_lines)} - indexed)
if unindexed:
    raise SystemExit('Fenced rule headings not indexed: ' + ', '.join(unindexed))
index = {
    'schemaVersion': 1, 'profile': 'CSharp6', 'status': 'indexed-unreviewed',
    'source': {'repository': work['repository'], 'tag': work['tag'], 'commit': work['commit'], 'grammarSha256': work['grammarSha256']},
    'primarySha256': policy['primary']['sha256'],
    'extraction': {'tool': 'tools/extract-csharp6-standard-index.py', 'scope': 'Every ANTLR rule in grammar.md with top-level alternatives, linked to main-text chapter blocks; the PDF comparison is separate'},
    'counts': counts,
    'alternativeCounts': {k: sum(len(p['alternatives']) for p in productions if p['grammar'] == k) for k in counts},
    'mainTextDisagreements': sorted(p['name'] for p in productions if p['mainTextOccurrences'] and not p['mainTextAgrees']),
    'withoutMainText': sorted(p['name'] for p in productions if not p['mainTextOccurrences']),
    'productions': productions,
}
text = json.dumps(index, indent=1, ensure_ascii=False) + '\n'
if args.check:
    if OUT.read_text(encoding='utf-8') != text:
        raise SystemExit('csharp6-standard-index.json is stale')
    print('current')
else:
    OUT.write_text(text, encoding='utf-8')
    print(counts, index['alternativeCounts'], 'disagree', len(index['mainTextDisagreements']), 'no-main', len(index['withoutMainText']))
