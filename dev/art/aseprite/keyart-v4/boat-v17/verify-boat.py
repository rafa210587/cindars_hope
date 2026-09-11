"""Read-only image inspection; artwork authored entirely by Aseprite Lua."""
from pathlib import Path
from PIL import Image
import hashlib,json
root=Path('D:/Projetos/Cindars_Hope/cindars_hope')
out=Path(__file__).resolve().parent
source=root/'Assets/_Game/Art/Generated/World/props/boat_keyart_v4.png'
expected='c0f86c0f0422c7a9ff869d986ed96ef4c75d662ad7cfee917b58a636c40c53ee'
assert hashlib.sha256(source.read_bytes()).hexdigest()==expected
base=Image.open(source).convert('RGBA'); candidate=Image.open(out/'boat_contact_v17.png').convert('RGBA')
assert base.size==candidate.size==(76,68)
added=changed=0
for y in range(68):
 for x in range(76):
  before,after=base.getpixel((x,y)),candidate.getpixel((x,y))
  assert after[3] in (0,255)
  if after!=before:
   changed+=1
   assert 1<=x<=12 and 35<=y<=55
   added+=before[3]==0 and after[3]==255
report=dict(status='CANDIDATE_REVIEWED_OFFLINE',source=str(source.relative_to(root)),sourceSha256=expected,sourceUnchanged=True,candidate='boat_contact_v17.png',candidateSha256=hashlib.sha256((out/'boat_contact_v17.png').read_bytes()).hexdigest(),canvas=[76,68],note='Source actually76x68; no resize to initial68x68 estimate',colorMode='RGBA',layers=3,frames=1,supportBottomLeft=[35,24],supportTopLeft=[35,44],normalizedPivot=[35/76,24/68],existingImportAndScale='Preserve existing PPU/scale; source effective18.3px/u; no gameplay footprint change',baselineAlphaBoundsExclusive=list(base.getbbox()),candidateAlphaBoundsExclusive=list(candidate.getbbox()),changedPixels=changed,newOpaquePixels=added,partialAlpha=0,unchangedOutside='x1..12,y35..55 top-left coordinates',review='Opened4x comparison and native1x on light and dark backgrounds. Stern rim now closes; lower hull reconstructed; other pixels unchanged.',unity='NOT RUN: root owns integration')
(out/'manifest.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
