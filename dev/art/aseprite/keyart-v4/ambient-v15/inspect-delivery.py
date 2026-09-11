"""Read-only raster/metadata inspection. All art authoring/export is performed in Aseprite."""
from pathlib import Path
from PIL import Image
import json,hashlib
folder=Path(__file__).resolve().parent
root=folder.parents[4]
items=[]
sources={'fountain_ambient_v15':'fountain_keyart_v3','cascade_ambient_v15':'river_cascade_keyart_v4'}
for name,count,size,duration in [('fountain_ambient_v15',5,(170,159),140),('cascade_ambient_v15',5,(68,56),140),('fish_jump_v15',8,(48,48),120)]:
    data=json.loads((folder/(name+'.json')).read_text())
    assert len(data['frames'])==count
    im=Image.open(folder/(name+'.png')).convert('RGBA')
    assert im.size==(size[0]*count,size[1])
    frames=[];pixels=[]
    for i,frame in enumerate(data['frames']):
        assert not frame['trimmed'] and not frame['rotated'] and frame['duration']==duration
        assert frame['frame']==dict(x=i*size[0],y=0,w=size[0],h=size[1])
        cell=im.crop((i*size[0],0,(i+1)*size[0],size[1])) # read-only inspection; never written
        alpha=cell.getchannel('A');values=list(alpha.getdata());pixels.append(list(cell.getdata()))
        frames.append(dict(index=i,durationMs=duration,opaque=sum(v==255 for v in values),partial=sum(0<v<255 for v in values),transparent=sum(v==0 for v in values),alphaBoundsTopLeft=alpha.getbbox()))
    diffs=[sum(a!=b for a,b in zip(pixels[i],pixels[(i+1)%count])) for i in range(count)]
    assert all(d>0 for d in diffs)
    if name.startswith('fish'): assert frames[-1]['opaque']==0 and frames[-1]['partial']==0
    gif=Image.open(folder/(name+'.gif')); gifdurations=[]
    for f in range(gif.n_frames): gif.seek(f);gifdurations.append(gif.info['duration'])
    assert gif.n_frames==count and gifdurations==[duration]*count
    item=dict(name=name,canvas=list(size),frameCount=count,sheetSize=list(im.size),durationMs=duration,loopSeconds=count*duration/1000,frames=frames,changedPixelsToNext=diffs,gifDurations=gifdurations,sha256=hashlib.sha256((folder/(name+'.png')).read_bytes()).hexdigest().upper())
    if name in sources:
        source=root/('Assets/_Game/Art/Generated/World/props/'+sources[name]+'.png')
        item['source']=str(source.relative_to(root));item['sourceSha256']=hashlib.sha256(source.read_bytes()).hexdigest().upper()
        item['pivotPolicy']='Preserve existing scene support; canvas and alpha are invariant.'
    else:item['contactTopLeft']=[24,34];item['pivotBottomLeft']=[24,14];item['normalizedPivot']=[.5,14/48];item['suggestedPPU']=18.3;item['idle']='Native Unity clip owns 20-second transparent interval; not baked into GIF.'
    items.append(item)
manifest=dict(status='CANDIDATE_STATIC_AND_METADATA_PASS',aseprite='1.3.18.5-x64',assets=items,continuousPlayback='NOT RUN by art worker; exported GIF timing and static pose progression reviewed',unity='NOT RUN: root owns integration')
(folder/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps([dict(name=a['name'],canvas=a['canvas'],frames=a['frameCount'],duration=a['durationMs'],changes=a['changedPixelsToNext']) for a in items],indent=2))
