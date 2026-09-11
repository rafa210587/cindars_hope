"""Read-only raster validation; all pixel authoring and boards are Aseprite outputs."""
from pathlib import Path
from PIL import Image
import hashlib,json
root=Path(__file__).resolve().parents[6]
out=Path(__file__).resolve().parent
root=Path('D:/Projetos/Cindars_Hope/cindars_hope')
sources=[root/'Assets/_Game/Art/Generated/World/tiles/ground_path_aseprite_v1.png',root/'Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png']
expected=['dd33c3a632985a8e1ad8038bbb22d2263ee40c1149fca785a89e34ed74e9ee89','6b5419e57a843071d26f1b2f58fc1f4d079d170e5baf4278945add3231299e4a']
for p,h in zip(sources,expected): assert hashlib.sha256(p.read_bytes()).hexdigest()==h
items=[]
for name,baseline,ppu in [('ground_path_contact_v16',sources[0],32),('ground_grass_contact_v16',out/'grass-nearest-baseline.png',314/(1254/128)),('ground_grass_contact_v16_fullres_comparison',sources[1],128)]:
 p=out/(name+'.png'); img=Image.open(p).convert('RGBA'); base=Image.open(baseline).convert('RGBA')
 assert img.size==base.size
 pixels=list(img.getdata()); original=list(base.getdata()); w,h=img.size
 assert all(a[3]==b[3] for a,b in zip(pixels,original))
 edge=4 if w!=1254 else 16
 assert all(img.getpixel((x,y))==base.getpixel((x,y)) for y in range(h) for x in range(w) if min(x,y,w-1-x,h-1-y)<edge)
 items.append(dict(name=name,canvas=[w,h],rgba=True,opaque=sum(p[3]==255 for p in pixels),partial=sum(0<p[3]<255 for p in pixels),changedPixels=sum(a!=b for a,b in zip(pixels,original)),edgePixelsPreserved=edge,ppu=ppu,worldWidth=w/ppu,screenPixelsPerSourcePixel=(480/17)/ppu,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
barn_source=root/'Assets/_Game/Art/Generated/World/building/barn_keyart_v4.png'
barn_hash='56f6652658a34a17c224137fe120027033983d8965cb2009d640faaac01ef774'
assert hashlib.sha256(barn_source.read_bytes()).hexdigest()==barn_hash
barn=Image.open(out/'barn_contact_v16.png').convert('RGBA'); before=Image.open(barn_source).convert('RGBA')
assert barn.size==before.size==(125,166)
changed=[]
for y in range(166):
 for x in range(125):
  a,b=barn.getpixel((x,y)),before.getpixel((x,y))
  assert a[3]==b[3]
  if a!=b: changed.append((x,y)); assert 29<=x<=94 and 65<=y<=81
items.append(dict(name='barn_contact_v16',canvas=[125,166],changedPixels=len(changed),alphaIdentical=True,opaque=sum(p[3]==255 for p in barn.getdata()),partial=sum(0<p[3]<255 for p in barn.getdata()),sourceSha256=barn_hash,sha256=hashlib.sha256((out/'barn_contact_v16.png').read_bytes()).hexdigest(),importPolicy='Preserve existing barn importer PPU/pivot and scene scale; effective18sourcepx/u',screenPixelsPerSourcePixel=(480/17)/18,decision='Optional local sample: effect subtle, root visual review required'))
report=dict(status='CANDIDATES_FOR_VISUAL_REVIEW',sources=[dict(path=str(p.relative_to(root)),sha256=h,unchanged=True) for p,h in zip(sources+[barn_source],expected+[barn_hash])],assets=items,board='offline-camera-density-board.png',boardNotes='Offline nearest-sampled comparison, top grass includes existing FarmGround tint. Not Unity rendering proof. Full source cleanup retained as comparison.',native='Aseprite layered files reopened;2layers each',unity='NOT RUN: root owns integration')
(out/'manifest.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
