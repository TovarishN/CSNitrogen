"""Snapshot only Nitrogen sources needed for building; never write into the source checkout."""
import argparse, hashlib, json, shutil, subprocess
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('--source',type=Path,default=Path.home()/'work/Nitrogen')
args=parser.parse_args(); root=Path(__file__).resolve().parents[1]
source=args.source.resolve(); dest=root/'.dependencies/Nitrogen'
if source == dest or dest in source.parents:
    parser.error('Source must be outside the destination snapshot')
for name in ['Nitrogen.Runtime','Nitrogen.Generator','Nitrogen.Grammar']:
    target=dest/name
    if target.exists(): shutil.rmtree(target)
    shutil.copytree(source/name,target,ignore=shutil.ignore_patterns('bin','obj'))
shutil.copy2(source/'Directory.Build.props',dest/'Directory.Build.props')
hashes={str(p.relative_to(dest)):hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(dest.rglob('*')) if p.is_file() and 'bin' not in p.parts and 'obj' not in p.parts}
info={'revision':subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD'],text=True).strip(),
      'dirty':subprocess.check_output(['git','-C',str(source),'status','--short'],text=True).splitlines(),
      'source':str(source),'buildCopy':'.dependencies/Nitrogen','sdk':'10.0.100','sourceHashes':hashes}
(root/'docs/coverage/dependencies.json').write_text(json.dumps(info,indent=2)+'\n')
print('Dependency source snapshot prepared at',dest)
