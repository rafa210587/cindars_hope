"""Root-only material integration after shared Assets release and candidate review."""
import hashlib
import json
import shutil
from pathlib import Path

root = Path.cwd()
base = root / 'dev/art/aseprite/keyart-v4/contact-v16'
art = base / 'art'
targets = {
 'Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs': [('WorldSpriteLibrary.Ground("ground_grass_keyart_v2")', 'WorldSpriteLibrary.Ground("ground_grass_contact_v16")')],
 'Assets/_Game/Scripts/Editor/Art/FarmPerimeterVisualComposer.cs': [('WorldSpriteLibrary.Ground("ground_grass_keyart_v2")', 'WorldSpriteLibrary.Ground("ground_grass_contact_v16")')],
 'Assets/_Game/Scripts/Editor/Art/FarmPathPalette.cs': [('ground_path_aseprite_v1.png', 'ground_path_contact_v16.png')],
 'Assets/_Game/Scripts/Editor/GeneratedSpriteImporter.cs': [(': p.StartsWith(HousesModularRoot) ? HousesModularPpu : Ppu;', ': p == "Assets/_Game/Art/Generated/World/tiles/ground_grass_contact_v16.png" ? 314f / (1254f / 128f)\n                : p == "Assets/_Game/Art/Generated/World/tiles/ground_path_contact_v16.png" ? 32f\n                : p.StartsWith(HousesModularRoot) ? HousesModularPpu : Ppu;')]
}
prepared=[]
for relative, edits in targets.items():
 p=root/relative; data=p.read_bytes(); text=data.decode('utf-8-sig'); newline='\r\n' if b'\r\n' in data else '\n'; text=text.replace('\r\n','\n')
 for old,new in edits:
  assert text.count(old)==1, f'Changed or ambiguous anchor: {relative}: {old}'
  text=text.replace(old,new)
 encoded=text.replace('\n',newline).encode('utf-8'); encoded=(b'\xef\xbb\xbf'+encoded) if data.startswith(b'\xef\xbb\xbf') else encoded
 prepared.append((p,data,encoded))
sources=[art/'ground_grass_contact_v16.png',art/'ground_path_contact_v16.png']
assert all(p.is_file() for p in sources), 'Missing reviewed Aseprite output'
backup=base/'material-integration-backup'; backup.mkdir(exist_ok=False)
manifest=[]
for p,old,new in prepared:
 (backup/(p.name+'.before')).write_bytes(old)
 manifest.append({'path':str(p),'before':hashlib.sha256(old).hexdigest(),'after':hashlib.sha256(new).hexdigest()})
for source in sources:
 dest=root/'Assets/_Game/Art/Generated/World/tiles'/source.name
 assert not dest.exists(), f'Refusing overwrite: {dest}'
for p,old,new in prepared:
 assert p.read_bytes()==old, f'Concurrent edit: {p}'
for source in sources: shutil.copyfile(source,root/'Assets/_Game/Art/Generated/World/tiles'/source.name)
for p,old,new in prepared: p.write_bytes(new)
(backup/'hashes.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('Material candidates and scoped import/wiring integrated; Unity not run.')
