"""Offline candidate audit, not a substitute for Unity physics execution."""
from pathlib import Path
import json,re
out=Path(__file__).resolve().parent
root=out.parents[4]
candidate=(out/'FarmPhysicalRouteProbe.cs').read_text()
source=(root/'Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs').read_text(encoding='utf-8-sig')
checks=[]
def check(name,condition):
    assert condition,name
    checks.append(name)
names=re.findall(r'"(\w+)"',candidate.split('SettlementSolidNames =',1)[1].split('};',1)[0])
expected=['PottingSupplies','HouseSupplies','HouseSuppliesSmall','GreenhouseSupplies','FieldNorthCrate','FieldNorthSmallCrate','FieldSouthCrate','CheeseSupplies','ShedSupplies','HouseWestBarrel_A','HouseWestBarrel_B','HouseEastBarrel_A','HouseEastBarrel_B','FountainWestBarrel','FountainEastBarrel','PastureTrough','CoopTrough','BarnHayStack','BarnHayStackSmall']
check('Exact19 unique props (9crates/6barrels/2troughs/2hay)',names==expected and len(set(names))==19)
check('Each prop has a current authoring call',all('"'+name+'"' in (root/'Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs').read_text() for name in names))
check('Specific collider identity is required','if (overlaps[i] == expected) check.overlapsExpected = true;' in candidate and 'check.status = check.overlapsExpected ? "PASS" : "FAIL";' in candidate)
check('Missing/disabled/trigger/masked colliders fail',all(s in candidate for s in ['candidates.Length != 1','colliders.Length != 1','!check.enabled','!check.active','!check.layerIncluded','!expected.attachedRigidbody.simulated']))
check('Local evidence obtained before exterior BFS',candidate.index('EvaluateAuthoredAffordances(player,')<candidate.index('queue.Enqueue(Vector2Int.zero)'))
check('Exterior BFS targets unchanged',candidate.split('private static Dictionary<string, Vector2> Targets',1)[1]==source.split('private static Dictionary<string, Vector2> Targets',1)[1])
check('Expected counts feed overall result','authoredSolidChecks.Count == 24' in candidate and 'authoredApproachChecks.Count == 4' in candidate)
check('Live tile grid covers every touched solid tile','grid.WorldToTile(bounds.max.x - 0.0001f' in candidate and 'grid.IsTillableAtWorldPosition(point.x, point.y)' in candidate)
check('No non-arable mutation in probe','RegisterBlocked' not in candidate)
check('Actual collider/filter used for casts and overlap',all(s in candidate for s in ['playerCollider.OverlapCollider(filter, overlaps)','CanReachExact(player, playerCollider, filter, overlaps, hits, from, to)','Physics2D.GetLayerCollisionMask(collider.gameObject.layer)']))
check('Furniture trigger distance is bounded',all(s in candidate for s in ['check.triggerDistance > 0.45f','triggers[0].ClosestPoint(to)','!triggers[0].enabled']))
geometry=[]
for name,foot,basebottom,approach in [('Bed',14,14.08,12.8),('Chest',15.1,15.14,13.85)]:
    bodytop=approach+1.125
    triggerdistance=(foot+0.1-2/2)-approach
    check(name+' offline full-body front clearance',bodytop<basebottom)
    check(name+' offline trigger selection distance',0<=triggerdistance<=.45)
    geometry.append(dict(name=name,approachY=approach,bodyTop=bodytop,baseBottom=basebottom,gap=round(basebottom-bodytop,5),triggerDistance=round(triggerdistance,5)))
report=dict(status='STATIC_PASS',checks=checks,geometry=geometry,unity='NOT RUN: candidate only; root owns Unity')
(out/'affordance-probes-static-report.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
