#!/usr/bin/env python3
"""Cross-reference sixth- and seventh-edition annex rules and alternative identities."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path


COVERAGE = Path(__file__).resolve().parents[1] / 'docs' / 'coverage'
OUTPUT = COVERAGE / 'csharp7-candidate-crosswalk.json'


def inputs(edition):
    raw = (COVERAGE / f'csharp{edition}-standard-index.json').read_bytes()
    rules = json.loads(raw)['productions']
    indexed = {rule['name']: rule for rule in rules}
    if len(indexed) != len(rules):
        raise SystemExit(f'Duplicate rule names in edition {edition}')
    return indexed, hashlib.sha256(raw).hexdigest()


def alternatives(rule):
    index = {item['sourceAlternativeSha256']: item['id'] for item in rule['alternatives']}
    if len(index) != len(rule['alternatives']):
        raise SystemExit(f"Duplicate alternative fingerprints in {rule['id']}")
    return index


def build():
    sixth, sixth_digest = inputs(6)
    seventh, seventh_digest = inputs(7)
    checked_delta = json.loads((COVERAGE / 'csharp7-annex-candidate-delta.json').read_bytes())
    same = []
    changed = []
    added = []
    removed = []
    totals = Counter()

    for name, rule in seventh.items():
        previous = sixth.get(name)
        current = alternatives(rule)
        if previous is None:
            added.append([rule['id'], rule['section'], list(current.values())])
            totals['newAlternative'] += len(current)
            continue

        old = alternatives(previous)
        if previous['sourceBodySha256'] == rule['sourceBodySha256']:
            if list(old) != list(current):
                raise SystemExit(f'Matching rule body has different alternative order: {name}')
            same.append([previous['id'], rule['id']])
            totals['identicalRuleAlternative'] += len(current)
            continue

        shared = [[old[digest], identifier] for digest, identifier in current.items() if digest in old]
        earlier_only = [identifier for digest, identifier in old.items() if digest not in current]
        later_only = [identifier for digest, identifier in current.items() if digest not in old]
        changed.append({
            'from': previous['id'], 'to': rule['id'], 'section': rule['section'],
            'matchingAlternatives': shared, 'sixthOnlyAlternatives': earlier_only,
            'seventhOnlyAlternatives': later_only,
        })
        totals['matchingChangedRuleAlternative'] += len(shared)
        totals['sixthOnlyAlternative'] += len(earlier_only)
        totals['newAlternative'] += len(later_only)

    for name, rule in sixth.items():
        if name not in seventh:
            earlier = alternatives(rule)
            removed.append([rule['id'], rule['section'], list(earlier.values())])
            totals['sixthOnlyAlternative'] += len(earlier)

    def rule_name(rule_id):
        return rule_id.rsplit(':', 1)[-1]

    counts = {'same': len(same), 'changed': len(changed), 'added': len(added), 'removed': len(removed)}
    if len(seventh) != checked_delta['totalRules'] or counts['same'] != checked_delta['same'] or any(
        sorted(expected) != sorted(actual) for expected, actual in (
            (checked_delta['changed'], [rule_name(row['to']) for row in changed]),
            (checked_delta['added'], [rule_name(row[0]) for row in added]),
            (checked_delta['removed'], [rule_name(row[0]) for row in removed]),
        )
    ):
        raise SystemExit('Computed rule relations disagree with the pinned annex delta')
    shared_count = totals['identicalRuleAlternative'] + totals['matchingChangedRuleAlternative']
    if (shared_count + totals['newAlternative'] != checked_delta['totalAlternatives'] or
            shared_count + totals['sixthOnlyAlternative'] != sum(len(rule['alternatives']) for rule in sixth.values())):
        raise SystemExit('Alternative counts disagree with the source indices')

    return {
        'schemaVersion': 1,
        'sixthIndexSha256': sixth_digest,
        'seventhIndexSha256': seventh_digest,
        'scope': 'Text identity within same-name annex rules. Semantic equivalence and implementation ownership require review.',
        'ruleCounts': counts,
        'alternativeCounts': dict(totals),
        'identicalBodyColumns': ['sixthRuleId', 'seventhRuleId'],
        'editionOnlyColumns': ['ruleId', 'section', 'alternativeIds'],
        'identicalBodies': same,
        'changedBodies': changed,
        'seventhOnlyNames': added,
        'sixthOnlyNames': removed,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    record = build()
    result = json.dumps(record, indent=1, ensure_ascii=False) + '\n'
    if args.check:
        if OUTPUT.read_text(encoding='utf-8') != result:
            raise SystemExit(f'{OUTPUT.name} is stale')
    else:
        OUTPUT.write_text(result, encoding='utf-8')
    print(record['ruleCounts'], record['alternativeCounts'])


if __name__ == '__main__':
    main()
