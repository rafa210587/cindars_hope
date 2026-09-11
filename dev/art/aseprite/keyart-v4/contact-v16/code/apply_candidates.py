"""Root executes --apply from repo root after releasing Farm source ownership. Never launches Unity."""
from pathlib import Path
import argparse,hashlib,json,datetime
parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--apply',action='store_true')
args=parser.parse_args()
if not args.apply: parser.error('No writes; explicit --apply required after coordination.')
here=Path(__file__).resolve().parent;root=Path.cwd()
manifest=json.loads((here/'manifest.json').read_text())
sha=lambda b:hashlib.sha256(b).hexdigest()
prepared=[]
for item in manifest:
    target=root/item['path'];candidate=here/item['candidate']
    original=target.read_bytes() if target.exists() else None
    if (sha(original) if original is not None else None)!=item['before']:
        raise RuntimeError('Live file changed; review/rebase required: '+str(target))
    new=candidate.read_bytes()
    if sha(new)!=item['after']:raise RuntimeError('Candidate changed after freeze: '+str(candidate))
    prepared.append((target,original,new))
backup=here/('backup-'+datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%S%fZ'))
backup.mkdir()
for target,old,new in prepared:
    if old is not None:(backup/target.name).write_bytes(old)
(backup/'manifest.json').write_text(json.dumps(manifest,indent=2))
for target,old,new in prepared:
    if (target.read_bytes() if target.exists() else None)!=old:
        raise RuntimeError('Concurrent edit detected immediately before write: '+str(target))
for target,old,new in prepared:
    target.parent.mkdir(parents=True,exist_ok=True)
    if old is None:
        with target.open('xb') as f:f.write(new)
    else:target.write_bytes(new)
print('Applied reviewed candidates. Backups: '+str(backup))
print('No scene regeneration, Unity compilation or tests executed.')
