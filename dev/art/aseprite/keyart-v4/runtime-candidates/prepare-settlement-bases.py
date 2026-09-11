"""Produce optional settlement base changes outside Assets, leaving gates/plant source untouched."""
from pathlib import Path
import difflib
root=Path(__file__).resolve().parents[5]
out=Path(__file__).resolve().parent
path='Assets/_Game/Scripts/Editor/Art/FarmSettlementVisualComposer.cs'
source=(root/path).read_text(encoding='utf-8-sig')
old='''            Place(parent, name, WorldSpriteLibrary.Prop(asset), foot, height, false);'''
new='''            Place(parent, name, WorldSpriteLibrary.Prop(asset), foot, height, false);
            // Only reviewed solid objects receive bases; plant/stone/fence calls remain unchanged.
            Vector2 size;
            switch (asset)
            {
                case "crate": size = new Vector2(50f / 64f, 20f / 64f) * height; break;
                case "feeding_trough": size = new Vector2(90f / 64f, 16f / 64f) * height; break;
                case "hay_bale": size = new Vector2(48f / 64f, 23f / 64f) * height; break;
                default: return;
            }
            CreateMeasuredPropBase(parent, name, foot, size, height * 0.02f);'''
assert source.count(old)==1
changed=source.replace(old,new)
begin=changed.index('        private static void PlaceKeyartBarrel(')
end=changed.index('        private static void CreateGardenPatch(',begin)
section=changed[begin:end]
needle='            renderer.sortingLayerName = "World";'
assert section.count(needle)==1
section=section.replace(needle,needle+'''
            // Reviewed barrel21x25 opaque silhouette; contact base excludes its upper rim.
            CreateMeasuredPropBase(parent, name, foot, new Vector2(19f / 25f, 7f / 25f) * height, height * 0.02f);''')
changed=changed[:begin]+section+changed[end:]
helper='''        private static void CreateMeasuredPropBase(Transform parent, string name, Vector2 foot,
            Vector2 size, float bottomInset)
        {
            var solid = new GameObject("Solid_" + name);
            solid.transform.SetParent(parent, false);
            solid.transform.position = foot;
            solid.transform.localScale = new Vector3(1f / parent.lossyScale.x, 1f / parent.lossyScale.y, 1f);
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                solid, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.WorldSolid);
            var collider = solid.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.offset = new Vector2(0f, bottomInset + size.y * 0.5f);
        }

'''
needle='        private static void Foliage('
assert changed.count(needle)==1
changed=changed.replace(needle,helper+needle)
(out/'farm-settlement-bases.patch').write_text(''.join(difflib.unified_diff(source.splitlines(True),changed.splitlines(True),fromfile='a/'+path,tofile='b/'+path)),encoding='utf-8')
print('Candidate only:',out/'farm-settlement-bases.patch')
