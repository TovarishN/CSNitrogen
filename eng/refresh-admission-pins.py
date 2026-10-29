"""Refresh existing CSharp4 -> 3 -> 2 input hashes, preserving source-review data.

Uses evaluated MSBuild resource identities and current source bytes. This records inputs;
it does not review their behavior or promote admission status. Run the predecessor guards
and full corpus after reviewing the changed sources. --check verifies reproducibility.
"""
import argparse
import hashlib
import json
import re
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
result = subprocess.check_output(['dotnet', 'msbuild', str(root/'src/CSNitrogen/CSNitrogen.csproj'), '-getItem:EmbeddedResource'], cwd=root, text=True)
items = json.loads(result)['Items']['EmbeddedResource']
resources = {}
for item in items:
    name = item['LogicalName']
    if name in resources:
        raise ValueError('Duplicate resource identity: '+name)
    resources[name] = Path(item['FullPath'])
virtual = {}
def read(path):
    return virtual[path] if path in virtual else path.read_bytes()
def digest(path):
    return hashlib.sha256(read(path)).hexdigest()
def eligible(name, version):
    source = name.startswith('CSNitrogen.source.')
    excluded = {f'CSNitrogen.source.Profiles/CSharp{v}CoverageContract.cs' for v in range(2, version+1)}
    if source and name not in excluded:
        return True
    if name.startswith('CSNitrogen.grammar.'):
        return True
    if version == 2:
        return name.startswith('CSNitrogen.review.')
    if name in {'CSNitrogen.coverage.json', 'CSNitrogen.dependencies.json'}:
        return True
    prefixes = ['csharp3-review.', 'csharp3-evidence.'] if version == 3 else ['csharp4-artifact.', 'csharp4-review.', 'csharp4-evidence.', 'csharp3-evidence.']
    return any(name.startswith('CSNitrogen.'+prefix) for prefix in prefixes)

for version in (4, 3, 2):
    gate_path = root/f'docs/coverage/csharp{version}-coverage-gate.json'
    gate = json.loads(read(gate_path))
    if gate['profile'] != f'CSharp{version}':
        raise ValueError('Unexpected gate profile')
    gate['reviewInputs'] = {name: digest(path) for name, path in sorted(resources.items()) if eligible(name, version)}
    virtual[gate_path] = (json.dumps(gate, indent=2)+'\n').encode()
    contract_path = root/f'src/CSNitrogen/Profiles/CSharp{version}CoverageContract.cs'
    code, count = re.subn(r'(ApprovedLedgerSha256\s*=\s*")[0-9a-f]{64}(";)', lambda m: m[1]+digest(gate_path)+m[2], read(contract_path).decode())
    if count != 1:
        raise ValueError('Expected one approved ledger literal')
    virtual[contract_path] = code.encode()

changed = [path for path, content in virtual.items() if path.read_bytes() != content]
for path in changed:
    print(('would update ' if args.check else 'updated ')+str(path.relative_to(root)))
    if not args.check:
        path.write_bytes(virtual[path])
if args.check and changed:
    raise SystemExit(1)
