"""Read-only serialized-scene scale inventory; writes evidence JSON, never assets."""
import argparse, hashlib, json, re
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[5]
out = Path(__file__).resolve().parent
manifest = json.loads((out / 'baseline-inputs.json').read_text(encoding='utf-8-sig'))
parser = argparse.ArgumentParser()
parser.add_argument('--scene', type=Path, default=out / 'before_gameplay/FarmScene.unity.snapshot')
parser.add_argument('--asset-root', type=Path, default=Path(manifest['backupRoot']))
parser.add_argument('--output', type=Path, default=out / 'baseline-scale-inventory.json')
args = parser.parse_args()
backup = args.asset_root
scene = args.scene.read_text(encoding='utf-8-sig')
blocks = {int(i): (int(t), b) for t, i, b in re.findall(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', scene, re.S)}
def scalar(b, key, default=''):
    m = re.search(r'^  '+re.escape(key)+r': (.*)$', b, re.M)
    return m.group(1) if m else default
def ref(b, key):
    m = re.search(r'fileID: (-?\d+)', scalar(b, key))
    return int(m.group(1)) if m else 0
def vec(b, key, default=(0,0,0)):
    s = scalar(b, key)
    return tuple(float(x) for x in re.findall(r'[xyzw]: ([\d.eE+-]+)', s)) or default
gos = {i:scalar(b,'m_Name') for i,(t,b) in blocks.items() if t==1}
trans = {i:b for i,(t,b) in blocks.items() if t==4}
bygo = {ref(b,'m_GameObject'):i for i,b in trans.items()}
def world(i):
    b = trans[i]; p=vec(b,'m_LocalPosition'); s=vec(b,'m_LocalScale',(1,1,1)); parent=ref(b,'m_Father')
    q=vec(b,'m_LocalRotation',(0,0,0,1))
    if any(abs(v)>0.0001 for v in q[:3]): raise ValueError('Rotated target requires Unity measurement')
    if parent:
        pp,ss=world(parent); p=tuple(pp[n]+p[n]*ss[n] for n in range(3)); s=tuple(s[n]*ss[n] for n in range(3))
    return p,s
def name(i):
    b=trans[i]; n=gos[ref(b,'m_GameObject')]; parent=ref(b,'m_Father')
    return name(parent)+'/'+n if parent else n
assets={}
for f in backup.glob('Assets/_Game/Art/Generated/World/**/*.png.meta'):
    txt=f.read_text(encoding='utf-8-sig'); g=re.search(r'^guid: (\w+)',txt,re.M)
    if g: assets[g[1]]=(Path(str(f)[:-5]),txt)
measurements=[]; colliders=[]
for i,(t,b) in blocks.items():
    if t not in (212,61): continue
    go=ref(b,'m_GameObject')
    if go not in bygo: continue
    ti=bygo[go]; path=name(ti)
    if not re.search(r'FishingSpot|Bridge_01|FarmHouse/(Roof|Door|Wall_)|Coop_01|Barn_01|Station_CheesePress|Station_WineBarrel|Greenhouse|CraftingStation_|Visual_CentralWell|FonteAnya|ShippingBin_|^Bounds/',path): continue
    pos,scale=world(ti)
    if t==61:
        size=vec(b,'m_Size'); off=vec(b,'m_Offset')
        colliders.append(dict(path=path,trigger=scalar(b,'m_IsTrigger'),sizeWorld=[size[n]*abs(scale[n]) for n in range(2)],centerWorld=[pos[n]+off[n]*scale[n] for n in range(2)],enabled=scalar(b,'m_Enabled')))
        continue
    if scalar(b,'m_DrawMode','0') != '0': continue
    g=re.search(r'guid: (\w+)',scalar(b,'m_Sprite'))
    if not g or g[1] not in assets: continue
    png,meta=assets[g[1]]; ppu=float(re.search(r'spritePixelsToUnits: ([\d.]+)',meta)[1]); im=Image.open(png)
    opaque=im.convert('RGBA').getchannel('A').point(lambda a:255 if a>=13 else 0).getbbox()
    full=[im.width/ppu*abs(scale[0]),im.height/ppu*abs(scale[1])]
    visible=[(opaque[2]-opaque[0])/ppu*abs(scale[0]),(opaque[3]-opaque[1])/ppu*abs(scale[1])] if opaque else [0,0]
    measurements.append(dict(path=path,sprite=str(png.relative_to(backup)).replace('\\','/'),spriteGuid=g[1],position=pos,lossyScale=scale,fullRectWorldSize=full,alpha05WorldSize=visible,alpha05PixelBounds=opaque,textureSize=[im.width,im.height],pixelsPerUnit=ppu,heightRelativeToPlayer=visible[1]/1.1875,note='Serialized single-sprite estimate; runtime metadata is authoritative.'))
result=dict(sceneSha256=hashlib.sha256(args.scene.read_bytes()).hexdigest().upper(),playerHeight=1.1875,measurements=measurements,boxColliders=colliders)
args.output.write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({'sprites':len(measurements),'colliders':len(colliders),'measurements':[{k:x[k] for k in ('path','alpha05WorldSize','heightRelativeToPlayer')} for x in measurements]},indent=2))

