"""Read-only raster and export metadata audit; no image editing."""
from pathlib import Path
from PIL import Image
import hashlib,json
out=Path(__file__).resolve().parent
src=out.parent/'boat-v17/boat_contact_v17.png'
source_hash='0a3d58b1a30142b3dc701245598c3184881f86e3e728f7e8a7a7eaf546859167'
assert hashlib.sha256(src.read_bytes()).hexdigest()==source_hash
report={'status':'OFFLINE_CANDIDATES_FROZEN','sourceBoatSha256':source_hash,'assets':[],'unity':'NOT RUN: root owns integration','continuousPlayback':'NOT RUN by art worker; GIFs exported and static poses inspected'}
for name,w,h,count,durations in [('boat_v18',76,68,4,[1250]*4),('chicken_v18',32,32,19,[400]*2+[140]*12+[180]*3+[600]*2)]:
 data=json.loads((out/(name+'.json')).read_text()); im=Image.open(out/(name+'.png')).convert('RGBA')
 assert im.size==(w*count,h) and len(data['frames'])==count
 frames=[]
 for i,f in enumerate(data['frames']):
  assert f['duration']==durations[i] and not f['trimmed'] and not f['rotated']
  assert f['frame']==dict(x=i*w,y=0,w=w,h=h)
  opaque=partial=0
  for y in range(h):
   for x in range(w):
    a=im.getpixel((i*w+x,y))[3]; opaque+=a==255; partial+=0<a<255
  assert partial==0
  frames.append(dict(index=i,durationMs=durations[i],opaque=opaque,partial=partial))
 if name=='boat_v18':
  base=Image.open(src).convert('RGBA')
  for i,dy in enumerate([0,-1,0,1]):
   for y in range(h):
    for x in range(w):
     expected=base.getpixel((x,y-dy)) if 0<=y-dy<h else (0,0,0,0)
     assert im.getpixel((i*w+x,y))==expected
  support=[35,24]
 else: support=[16,6]
 gif=Image.open(out/(name+'.gif')); assert gif.n_frames==count
 for i,d in enumerate(durations): gif.seek(i); assert gif.info['duration']==d
 report['assets'].append(dict(name=name,canvas=[w,h],sheet=[w*count,h],frameCount=count,frames=frames,tags=data['meta']['frameTags'],supportBottomLeft=support,sha256=hashlib.sha256((out/(name+'.png')).read_bytes()).hexdigest()))
(out/'manifest.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({x['name']:x['sha256'] for x in report['assets']},indent=2))
