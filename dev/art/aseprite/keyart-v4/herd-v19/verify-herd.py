"""Read-only image/JSON validation; all drawings and boards authored in Aseprite."""
from pathlib import Path
from PIL import Image
import hashlib,json
out=Path(__file__).resolve().parent
root=Path('D:/Projetos/Cindars_Hope/cindars_hope')
sources=[('cow_keyart_v4_01.png','738837d43d27f97f85de34275e35bad1953e503c2af434d09f3bbf51546b9838'),('sheep_keyart_v4_01.png','01bbf4dd768431481de6692a64dc844a71688e9edc3b55a184d9bc8c92c1f732')]
for name,sha in sources: assert hashlib.sha256((root/'Assets/_Game/Art/Generated/World/animals'/name).read_bytes()).hexdigest()==sha
expected_tags=[('idle',0,1),('walk_down',2,5),('walk_up',6,9),('walk_side',10,13),('graze',14,16),('rest',17,18)]
report={'status':'OFFLINE_CANDIDATES_FOR_REVIEW','originalsUnchanged':dict(sources),'sourceUse':'Existing cow/sheep opened as references; these are original layered drawings, not resized originals. Goat is distinct new art.','canvas':[48,48],'sheet':[912,48],'supportBottomLeft':[24,8],'supportTopLeft':[24,40],'targetPPU':32,'assets':[],'unity':'NOT RUN: root owns integration','continuousPlayback':'NOT RUN by art worker; timed GIFs exported, poses/contact/static progression reviewed'}
for kind in ['cow','sheep','goat']:
 path=out/(kind+'_v19.png');im=Image.open(path).convert('RGBA');assert im.size==(912,48)
 data=json.loads((out/(kind+'_v19.json')).read_text());assert len(data['frames'])==19
 assert [(t['name'],t['from'],t['to']) for t in data['meta']['frameTags']]==expected_tags
 frames=[];durations=[400]*2+[180 if kind=='cow' else 160]*12+[220]*3+[650]*2
 for n,f in enumerate(data['frames']):
  assert f['frame']==dict(x=n*48,y=0,w=48,h=48) and not f['trimmed'] and not f['rotated'] and f['duration']==durations[n]
  # Crop is used solely to inspect an already exported frame; it is never saved as art.
  frame=im.crop((n*48,0,(n+1)*48,48));bounds=frame.getbbox();assert bounds and bounds[0]>0 and bounds[1]>0 and bounds[2]<48 and bounds[3]<48
  pixels=list(frame.getdata());assert all(p[3] in (0,255) for p in pixels)
  if n>=17: assert bounds[3]==40,'Rest must reach groundline40'
  if n<14: assert bounds[3]==40,'Walking/idle retains planted contact at y39'
  frames.append(dict(index=n,alphaBoundsExclusive=list(bounds),opaque=sum(p[3]==255 for p in pixels),partial=0,durationMs=durations[n]))
 gif=Image.open(out/(kind+'_v19.gif'));assert gif.n_frames==19
 for n,d in enumerate(durations):gif.seek(n);assert gif.info['duration']==d
 report['assets'].append(dict(name=kind+'_v19',sha256=hashlib.sha256(path.read_bytes()).hexdigest(),frames=frames,standingOpaqueHeightsUnits=[(f['alphaBoundsExclusive'][3]-f['alphaBoundsExclusive'][1])/32 for f in frames[:14]],layers=3))
(out/'manifest.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({a['name']:a['sha256'] for a in report['assets']},indent=2))
