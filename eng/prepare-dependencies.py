"""Copy Nitrogen build sources without modifying the source checkout.

Default mode records a new authoring snapshot. --restore-pinned restores exactly the
reviewed revision/hashes without changing the admission manifest; --verify checks it.
"""
import argparse
import hashlib
import json
import shutil
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source', type=Path, default=Path.home()/'work/Nitrogen')
mode = parser.add_mutually_exclusive_group()
mode.add_argument('--restore-pinned', action='store_true')
mode.add_argument('--verify', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
source = args.source.resolve()
destination = root/'.dependencies/Nitrogen'
dest = destination.resolve()
manifest = root/'docs/coverage/dependencies.json'
modules = ['Nitrogen.Runtime', 'Nitrogen.Generator', 'Nitrogen.Grammar']

def hashes(directory):
    files = [directory/'Directory.Build.props']
    for name in modules:
        files.extend(p for p in (directory/name).rglob('*') if p.is_file() and 'bin' not in p.relative_to(directory).parts and 'obj' not in p.relative_to(directory).parts)
    return {p.relative_to(directory).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(files)}

def require_hashes(actual, expected):
    if actual != expected:
        changed = sorted(k for k in actual.keys() | expected.keys() if actual.get(k) != expected.get(k))
        raise SystemExit('Pinned dependency source mismatch: '+', '.join(changed))

if args.verify:
    expected = json.loads(manifest.read_text())
    require_hashes(hashes(dest), expected['sourceHashes'])
    print('Verified', len(expected['sourceHashes']), 'pinned dependency source files at', expected['revision'])
    raise SystemExit(0)

if dest != destination or (dest/'.git').exists():
    parser.error('Destination must be an isolated snapshot directory, not a symlink or Git checkout')
if source == dest or dest in source.parents or source in dest.parents:
    parser.error('Source and destination snapshot must be separate directories')
revision = subprocess.check_output(['git', '-C', str(source), 'rev-parse', 'HEAD'], text=True).strip()
source_hashes = hashes(source)
if args.restore_pinned:
    expected = json.loads(manifest.read_text())
    if revision != expected['revision']:
        raise SystemExit('Expected pinned revision '+expected['revision']+', got '+revision)
    require_hashes(source_hashes, expected['sourceHashes'])
for name in modules:
    target = dest/name
    if target.is_symlink():
        target.unlink()
    elif target.exists():
        shutil.rmtree(target)
    shutil.copytree(source/name, target, ignore=shutil.ignore_patterns('bin', 'obj'))
props = dest/'Directory.Build.props'
if props.is_symlink():
    props.unlink()
shutil.copy2(source/'Directory.Build.props', props)
actual = hashes(dest)
require_hashes(actual, source_hashes)
if args.restore_pinned:
    print('Restored', len(actual), 'pinned sources without changing', manifest)
else:
    info = {'revision': revision,
            'dirty': subprocess.check_output(['git', '-C', str(source), 'status', '--short'], text=True).splitlines(),
            'source': str(source), 'buildCopy': '.dependencies/Nitrogen', 'sdk': '10.0.100', 'sourceHashes': actual}
    manifest.write_text(json.dumps(info, indent=2)+'\n')
    print('Dependency source snapshot prepared at', dest)
