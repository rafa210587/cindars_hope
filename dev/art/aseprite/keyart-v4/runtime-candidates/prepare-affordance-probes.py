from pathlib import Path
import difflib
root=Path(__file__).resolve().parents[5]
out=Path(__file__).resolve().parent
path='Assets/_Game/Scripts/Editor/Validation/FarmPhysicalRouteProbe.cs'
source=(root/path).read_text(encoding='utf-8-sig')
changed=source
needle='            public List<PhysicsCheck> landmarkPhysicsChecks = new List<PhysicsCheck>();'
assert changed.count(needle)==1
changed=changed.replace(needle,needle+'''
            public List<AuthoredSolidCheck> authoredSolidChecks = new List<AuthoredSolidCheck>();
            public List<AuthoredApproachCheck> authoredApproachChecks = new List<AuthoredApproachCheck>();
            public List<PhysicsCheck> authoredTillingChecks = new List<PhysicsCheck>();''')
needle='''                var grid = DomainManagerRegistry.Get<FarmTileGrid>();
                if (grid == null) throw new InvalidOperationException("Farm tile grid was not installed.");'''
assert changed.count(needle)==1
changed=changed.replace(needle,'')
anchor='                var targets = Targets(player, evidence);'
assert changed.count(anchor)==1
changed=changed.replace(anchor,needle+'''
                EvaluateAuthoredAffordances(player, collider, filter, overlaps, hits, grid, evidence);
'''+anchor)
needle='                evidence.status = evidence.routes.All(route => route.status == "PASS") &&'
assert changed.count(needle)==1
changed=changed.replace(needle,needle+'''
                    evidence.authoredSolidChecks.Count == 24 && evidence.authoredSolidChecks.All(check => check.status == "PASS") &&
                    evidence.authoredApproachChecks.Count == 4 && evidence.authoredApproachChecks.All(check => check.status == "PASS") &&
                    evidence.authoredTillingChecks.Count > 0 && evidence.authoredTillingChecks.All(check => check.status == "PASS") &&''')
needle='        private static void EvaluateLandmarkPhysics('
assert changed.count(needle)==1
changed=changed.replace(needle,(out/'affordance-probe-methods.cs.txt').read_text()+needle)
changed=changed.replace('An approach, dock-water check or natural boundary sweep failed; inspect individual evidence.',
    'An approach, named authored solid, live tilling tile, dock-water check or boundary sweep failed; inspect individual evidence.')
(out/'FarmPhysicalRouteProbe.cs').write_text(changed,encoding='utf-8')
(out/'FarmPhysicalRouteProbe.patch').write_text(''.join(difflib.unified_diff(source.splitlines(True),changed.splitlines(True),fromfile='a/'+path,tofile='b/'+path)),encoding='utf-8')
print('Candidate only: FarmPhysicalRouteProbe.cs + FarmPhysicalRouteProbe.patch')
