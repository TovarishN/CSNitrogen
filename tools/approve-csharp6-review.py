#!/usr/bin/env python3
"""Record a named reviewer's approval of the exact CSharp6 admission ledger."""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
GATE = ROOT / "docs/coverage/csharp6-coverage-gate.json"
APPROVAL = ROOT / "docs/coverage/csharp6-review-approval.json"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reviewer", required=True)
    parser.add_argument("--ledger-sha256", required=True, help="digest of the exact ledger the reviewer inspected")
    parser.add_argument("--notes", default="")
    args = parser.parse_args()
    reviewer = args.reviewer.strip()
    if not reviewer:
        parser.error("A named reviewer is required")

    ledger = GATE.read_bytes()
    digest = hashlib.sha256(ledger).hexdigest()
    if digest != args.ledger_sha256:
        parser.error("The supplied digest does not match the current ledger")
    subprocess.run([sys.executable, "eng/refresh-admission-pins.py", "--check"], cwd=ROOT, check=True)
    if GATE.read_bytes() != ledger:
        parser.error("The ledger changed while checking its inputs")

    record = dict(
        schemaVersion=1,
        profile="CSharp6",
        status="approved",
        reviewer=reviewer,
        approvedOn=datetime.date.today().isoformat(),
        ledgerSha256=digest,
        notes=args.notes,
    )
    APPROVAL.write_text(json.dumps(record, indent=2) + "\n", encoding="utf-8")
    print(f"Recorded CSharp6 approval by {reviewer}. Rebuild and test before committing.")


if __name__ == "__main__":
    main()
