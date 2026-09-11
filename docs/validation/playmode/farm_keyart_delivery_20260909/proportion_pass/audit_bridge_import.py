"""Inspect existing PNG and importer metadata without modifying either."""
import hashlib,json,re
from pathlib import Path
from PIL import Image
out=Path(__file__).resolve().parent
root=out.parents[4]
asset=root/'Assets/_Game/Art/Generated/World/props/bridge_keyart_v2.png'
raw=root/'art/farm-pixelart-review/raw/bridge-keyart-v2.png'
meta=Path(str(asset)+'.meta').read_text(encoding='utf-8-sig')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
def integer(k): return int(re.search(r'^\s*'+k+r': (\d+)',meta,re.M)[1])
platforms=[]
for target,body in re.findall(r'buildTarget: (\w+)\n(.*?)(?=\n  - serializedVersion:|\n  spriteSheet:)',meta,re.S):
    platforms.append(dict(target=target,maxTextureSize=int(re.search(r'maxTextureSize: (\d+)',body)[1]),compression=int(re.search(r'textureCompression: (\d+)',body)[1]),overridden=int(re.search(r'overridden: (\d+)',body)[1])))
im=Image.open(asset)
default=next(p for p in platforms if p['target']=='DefaultTexturePlatform')
result=dict(asset=str(asset.relative_to(root)).replace('\\','/'),raw=str(raw.relative_to(root)).replace('\\','/'),assetSha256=sha(asset),rawSha256=sha(raw),byteExact=sha(asset)==sha(raw),metaSha256=sha(Path(str(asset)+'.meta')),dimensions=[im.width,im.height],mode=im.mode,alphaExtrema=im.getchannel('A').getextrema(),filterMode=integer('filterMode'),enableMipMap=integer('enableMipMap'),pixelsPerUnit=integer('spritePixelsToUnits'),serializedLegacyRootMaxTextureSize=integer('maxTextureSize'),platforms=platforms,scope='Serialized importer + raw PNG verification. DefaultTexturePlatform max4096; legacy top-level max2048 retained by Unity. Generator explicitly sets TextureImporter.maxTextureSize=4096 then SaveAndReimport. Runtime imported texture width was not separately queried.')
result['pass']=result['byteExact'] and result['filterMode']==0 and result['enableMipMap']==0 and result['pixelsPerUnit']==128 and default['maxTextureSize']==4096 and default['compression']==0 and all(p['overridden']==0 for p in platforms) and result['alphaExtrema']==(0,255)
(out/'bridge-import-review.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
