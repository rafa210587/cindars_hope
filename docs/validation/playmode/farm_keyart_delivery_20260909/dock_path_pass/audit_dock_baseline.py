"""Read-only projection of baseline sprite alpha against canonical lake polygon."""
import argparse,json
from pathlib import Path
import numpy as np
from PIL import Image
out=Path(__file__).resolve().parent
manifest=json.loads((out/'baseline-inputs.json').read_text(encoding='utf-8-sig'))
parser=argparse.ArgumentParser()
parser.add_argument('--inventory',type=Path,default=out/'baseline-scale-inventory.json')
parser.add_argument('--output',type=Path,default=out/'dock-baseline-water-review.json')
args=parser.parse_args()
inventory=json.loads(args.inventory.read_text(encoding='utf-8-sig'))
dock=next(m for m in inventory['measurements'] if m['path']=='FishingSpot/Visual_FishingDock')
im=Image.open(Path(manifest['backupRoot'])/dock['sprite']).convert('RGBA')
alpha=np.asarray(im)[:,:,3]
# Copied from the immutable baseline FarmSceneSpatialContract.Lake. Polygon is collision contour,
# not every painted shore pixel. The image has no semantic hull layer: ROI below is explicitly approximate.
lake=np.array([(6,-16),(8,-10),(12,-7),(21,-6),(27,-8),(28,-12),(27,-17),(22,-18),(13,-18),(8,-17)],dtype=float)
ys,xs=np.nonzero(alpha>=128)
wx=dock['position'][0]+(xs+.5-im.width*.5)*dock['lossyScale'][0]/dock['pixelsPerUnit']
wy=dock['position'][1]+(im.height-ys-.5)*dock['lossyScale'][1]/dock['pixelsPerUnit']
inside=np.zeros(xs.size,dtype=bool)
for a,b in zip(lake,np.roll(lake,-1,axis=0)):
    if a[1]==b[1]: continue
    crossing=((a[1]>wy)!=(b[1]>wy)) & (wx<(b[0]-a[0])*(wy-a[1])/(b[1]-a[1])+a[0])
    inside ^= crossing
def region(label,mask):
    return dict(label=label,opaqueSamples=int(mask.sum()),waterSamples=int((mask&inside).sum()),waterPercent=float(100*(mask&inside).sum()/mask.sum()),worldBounds=[float(wx[mask].min()),float(wy[mask].min()),float(wx[mask].max()),float(wy[mask].max())])
result=dict(sceneSha256=inventory['sceneSha256'],textureSize=[im.width,im.height],method='Serialized transforms, 128PPU, importer BottomCenter alignment7; alpha>=128 pixel centers vs canonical visual Lake polygon. No raster modifications. Hull ROI is approximate and not an acceptance gate.',regions=[region('whole dock+boat sprite',np.ones(xs.size,dtype=bool)),region('approximate visible boat hull ROI x800..1150/y650..900',(xs>=800)&(xs<=1150)&(ys>=650)&(ys<=900))],lakePolygon=lake.tolist(),playerHeightWorld=1.1875,dockOpaqueHeightToPlayer=dock['heightRelativeToPlayer'])
args.output.write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
