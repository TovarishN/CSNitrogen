#!/usr/bin/env python3
"""Record a named reviewer's approval of the exact CSharp5 review inputs.

Run this only after independently reviewing the CSharp5 admission inputs. It pins the
current hash of every reviewed input, regenerates the reviewed overlay (which is
`verified` only while this record matches) and refreshes the admission pins. Any later
change to a reviewed input invalidates the approval until a reviewer records it again.
"""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
# The inputs a reviewer approves; the overlay is derived from them and is not listed.
INPUTS = [
    'docs/coverage/csharp5-reference-policy.json',
    'docs/coverage/csharp5-standard-index.json',
    'docs/coverage/csharp5-source-delta-inventory.json',
    'docs/coverage/csharp5-standard-crosswalk.json',
    'docs/coverage/csharp5-main-text-differences.json',
    'docs/coverage/csharp5-main-text-dispositions.json',
    'docs/coverage/csharp5-normative-context-review.md',
    'docs/coverage/csharp4-reviewed-occurrences.json',
    'tests/Corpus/CSharp5/alternative-witness-matrix.json',
    'tests/Corpus/CSharp5/source-specific-cases.json',
]
APPROVAL = ROOT / 'docs/coverage/csharp5-review-approval.json'


def digests():
    return {path: hashlib.sha256((ROOT / path).read_bytes()).hexdigest() for path in INPUTS}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewer', required=True, help='the person who reviewed the inputs')
    parser.add_argument('--notes', default='', help='optional review notes')
    args = parser.parse_args()
    if not args.reviewer.strip():
        raise SystemExit('A named reviewer is required')
    record = dict(schemaVersion=1, profile='CSharp5', status='approved', reviewer=args.reviewer.strip(),
                  approvedOn=datetime.date.today().isoformat(), notes=args.notes,
                  scope='Independent review of the listed CSharp5 admission inputs. Any change to one of them invalidates this approval.',
                  inputs=digests())
    APPROVAL.write_text(json.dumps(record, indent=2) + '\n')
    for command in (['tools/build-csharp5-reviewed-occurrences.py'], ['eng/refresh-admission-pins.py']):
        subprocess.run([sys.executable, *command], cwd=ROOT, check=True)
    print(f'Recorded approval by {record["reviewer"]}; rebuild and run the tests before committing.')


if __name__ == '__main__':
    main()
