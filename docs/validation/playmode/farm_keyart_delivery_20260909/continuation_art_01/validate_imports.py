from pathlib import Path
from PIL import Image
import hashlib, json, re

root = Path(__file__).resolve().parents[5]
manifest = json.loads((root / 'art/farm-pixelart-review/organic-revision-assets.json').read_text(encoding='utf-8-sig'))
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest().upper()
rows = []
for item in manifest:
    asset = root / item.get('assetPath', item.get('asset'))
    raw = root / item.get('rawPath', item.get('raw'))
    metadata = Path(str(asset) + '.meta')
    meta = metadata.read_text()
    im = Image.open(asset)
    alpha = im.getchannel('A') if 'A' in im.getbands() else None
    platforms = []
    for block in re.findall(r'  - serializedVersion: 4\n(.*?)(?=  - serializedVersion: 4|  spriteSheet:)', meta, re.S):
        def value(key):
            match = re.search(key + r': (\S+)', block)
            return match.group(1) if match else None
        platforms.append(dict(target=value('buildTarget'), compression=value('textureCompression'), overridden=value('overridden'), maxTextureSize=value('maxTextureSize')))
    prompt = root / item['promptSummaryPath']
    rows.append(dict(id=item.get('id', asset.stem), asset=str(asset.relative_to(root)), raw=str(raw.relative_to(root)), assetSha256=sha(asset), rawSha256=sha(raw), bytesIdentical=sha(asset) == sha(raw), width=im.width, height=im.height, mode=im.mode, alphaExtrema=alpha.getextrema() if alpha else None, transparentPixels=alpha.histogram()[0] if alpha else 0, filterMode=int(re.search(r'filterMode: (\d+)',meta).group(1)), enableMipMap=int(re.search(r'enableMipMap: (\d+)',meta).group(1)), ppu=int(re.search(r'spritePixelsToUnits: (\d+)',meta).group(1)), metaSha256=sha(metadata), platforms=platforms, promptSummary=str(prompt.relative_to(root)), promptSummarySha256=sha(prompt)))
Path(__file__).with_name('import-validation.json').write_text(json.dumps(rows, indent=2), encoding='utf-8')
for row in rows:
    assert row['bytesIdentical'] and row['filterMode'] == 0 and row['enableMipMap'] == 0
    assert row['platforms'][0]['compression'] == '0'
print('Import technical PASS:', len(rows), 'assets; no artistic acceptance inferred.')
