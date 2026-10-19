from pathlib import Path
import difflib,hashlib,json
root=Path(__file__).resolve().parents[5]
out=Path(__file__).resolve().parent
plannerPath='Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs'
landscapePath='Assets/_Game/Scripts/Editor/Art/FarmLandscapeVisualComposer.cs'
planner=(root/plannerPath).read_text(encoding='utf-8-sig')
landscape=(root/landscapePath).read_text(encoding='utf-8-sig')
changes=[
 ('new Vector2(6.6f,9.2f), new Vector2(6.95f,6.7f), new Vector2(6.85f,4.4f)',
  'new Vector2(6.6f,9.2f), new Vector2(7.15f,7.05f), new Vector2(6.85f,4.4f)'),
 ('new Vector2(7.4f,3f), new Vector2(11f,3.25f), new Vector2(15f,3.6f)',
  'new Vector2(7.4f,3f), new Vector2(9.8f,3.5f), new Vector2(14.3f,3.75f)'),
 ('new[] { 1f, 0.85f, 0.75f, 1.05f, 0.85f, 0.95f, 1.1f, 0.8f, 0.85f, 0.9f }',
  'new[] { 1f, 0.85f, 0.75f, 1.05f, 0.85f, 0.95f, 1.1f, 0.8f, 0.85f, 0.875f, 0.9f }'),
 ('new Vector2(5.6f,-14.45f), new Vector2(7.3f,-12.8f), new Vector2(9.1f,-8f), new Vector2(6.9f,-4.35f)',
  'new Vector2(5.6f,-14.45f), new Vector2(7.3f,-12.8f), new Vector2(9.1f,-8f),\n                new Vector2(8.15f,-5.9f), new Vector2(6.9f,-4.35f)')]
changedPlanner=planner
for old,new in changes:
    assert changedPlanner.count(old)==1,old
    changedPlanner=changedPlanner.replace(old,new)

anchor='''        private static Sprite CliffColumn(Sprite source, int column)'''
assert landscape.count(anchor)==1
# Append one visual-only garden pass to the existing landscape entry point.
end=landscape.index(anchor)
before=landscape[:end]
tail='''            }
        }
'''
assert before.endswith(tail)
before=before[:-len(tail)]+'''            }
            CreateClearingPlantMasses(parent);
        }
'''
helper='''        private static void CreateClearingPlantMasses(Transform parent)
        {
            var bush = WorldSpriteLibrary.Foliage("undergrowth_keyart_v4");
            var flowers = WorldSpriteLibrary.Foliage("wildflowers_keyart_v4");
            // Unequal masses on the quiet sides of the well/fountain and inner forest edge.
            var centers = new[] { new Vector2(-9.8f, 12.8f), new Vector2(-4.5f, 16.3f),
                new Vector2(-32.8f, 6.6f), new Vector2(-24.8f, -0.4f),
                new Vector2(-34.6f, 0.5f), new Vector2(5.5f, -18.2f) };
            var offsets = new[] { Vector2.zero, new Vector2(-0.85f, 0.2f),
                new Vector2(0.7f, -0.12f), new Vector2(-1.05f, -0.28f),
                new Vector2(0.1f, 0.62f), new Vector2(1.35f, 0.22f) };
            for (var group = 0; group < centers.Length; group++)
            for (var part = 0; part < offsets.Length; part++)
            {
                var leafy = part < 2;
                var flip = group % 2 != 0;
                var offset = offsets[part];
                if (flip) offset.x = -offset.x;
                var point = centers[group] + offset;
                var height = leafy ? (part == 0 ? 2.15f : 1.45f) : 1.02f + (group + part) % 3 * 0.12f;
                // Measured opaque bounds relative to the source contact. Include a margin and
                // densely sample the entire silhouette, preserving all existing rejection buffers.
                var left = (leafy ? -34f / 55f : -20f / 26f) * height;
                var right = (leafy ? 22f / 55f : 13f / 26f) * height;
                var minX = (flip ? -right : left) - 0.2f;
                var maxX = (flip ? -left : right) + 0.2f;
                var columns = Mathf.CeilToInt((maxX - minX) / 0.25f);
                var rows = Mathf.CeilToInt((height + 0.4f) / 0.25f);
                var safe = true;
                for (var x = 0; x <= columns && safe; x++)
                for (var y = 0; y <= rows && safe; y++)
                {
                    var sample = point + new Vector2(Mathf.Lerp(minX, maxX, (float)x / columns),
                        Mathf.Lerp(-0.2f, height + 0.2f, (float)y / rows));
                    if (FarmDecorationPlanner.IsForbiddenCell(sample) ||
                        Vector2.Distance(sample, FarmSettlementPhysicsContract.WellClearingApproach) < 1.4f) safe = false;
                }
                if (!safe) continue;
                Place(parent, "Visual_ClearingPlantMass_" + group + "_" + part,
                    leafy ? bush : flowers, point, height, leafy ? 55f / 74f : 26f / 36f,
                    leafy ? new Vector2(43f / 84f, 9f / 74f) : new Vector2(21f / 43f, 6f / 36f),
                    flip, 5, Color.white);
            }
        }

'''
changedLandscape=before+helper+landscape[end:]
manifest=[]
patch=''
for path,old,new in [(plannerPath,planner,changedPlanner),(landscapePath,landscape,changedLandscape)]:
    filename=Path(path).name
    (out/(filename+'.txt')).write_text(new,encoding='utf-8')
    patch+=''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile='a/'+path,tofile='b/'+path))
    manifest.append(dict(path=path,originalSha256=hashlib.sha256((root/path).read_bytes()).hexdigest().upper(),candidate=filename+'.txt'))
(out/'visual-round14.patch').write_text(patch,encoding='utf-8')
(out/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps(manifest,indent=2))
