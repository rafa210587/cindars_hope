"""Read-only before/after serialized scene identity audit."""
import argparse,hashlib,json,re
from pathlib import Path
out=Path(__file__).resolve().parent
root=out.parents[4]
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def value(b,k):
    m=re.search(r'^  '+re.escape(k)+r': (.*)$',b,re.M)
    return m[1] if m else ''
def ref(b,k):
    m=re.search(r'fileID: (-?\d+)',value(b,k))
    return int(m[1]) if m else 0
def inventory(path):
    text=path.read_text(encoding='utf-8-sig')
    blocks={int(i):(int(t),b) for t,i,b in re.findall(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)',text,re.S)}
    gos={i:value(b,'m_Name') for i,(t,b) in blocks.items() if t==1}
    trans={i:b for i,(t,b) in blocks.items() if t==4}
    bygo={ref(b,'m_GameObject'):i for i,b in trans.items()}
    sprites={ref(b,'m_GameObject'):b for i,(t,b) in blocks.items() if t==212}
    def name(i):
        b=trans[i]; n=gos[ref(b,'m_GameObject')]; p=ref(b,'m_Father')
        return name(p)+'/'+n if p else n
    trees={}; exterior=[]
    for i,(t,b) in blocks.items():
        go=ref(b,'m_GameObject')
        if go not in bygo: continue
        tr=trans[bygo[go]]; path=name(bygo[go])
        if '_treeIndex:' in b:
            index=value(b,'_treeIndex')
            if index in trees: raise ValueError('Duplicate TreeIndex '+index)
            sr=sprites.get(go,'')
            trees[index]=dict(path=path,position=value(tr,'m_LocalPosition'),scale=value(tr,'m_LocalScale'),sprite=value(sr,'m_Sprite'),treeData=value(b,'_treeData'))
        if 'FarmPerimeterVisuals' in path:
            exterior.append(dict(path=path,componentType=t,treeNode='_treeIndex:' in b))
    return dict(trees=trees,exterior=exterior)
parser=argparse.ArgumentParser()
parser.add_argument('--after',type=Path,default=out/'regen_01/FarmScene.unity.snapshot')
args=parser.parse_args()
before=inventory(out/'before_gameplay/FarmScene.unity.snapshot')
after=inventory(args.after)
idsame=set(before['trees'])==set(after['trees'])
changes=[dict(index=i,before=before['trees'][i],after=after['trees'][i]) for i in before['trees'] if i in after['trees'] and before['trees'][i]!=after['trees'][i]]
external=after['exterior']
result=dict(beforeSceneSha256=sha(out/'before_gameplay/FarmScene.unity.snapshot'),afterSceneSha256=sha(args.after),beforeTreeCount=len(before['trees']),afterTreeCount=len(after['trees']),sameIds=idsame,changedTreeCount=len(changes),changes=changes,exteriorComponentCount=len(external),exteriorColliderCount=sum(x['componentType'] in (58,60,61,66,68,70,197,256) for x in external),exteriorTreeNodeCount=sum(x['treeNode'] for x in external),exteriorComponentTypes=sorted(set(x['componentType'] for x in external)))
(out/'scene-identity-review.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k!='changes'},indent=2))
