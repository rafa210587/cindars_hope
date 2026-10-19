"""Produce method-bounded patches outside Assets; never apply them to live source."""
from pathlib import Path
import difflib

root=Path(__file__).resolve().parents[5]
out=Path(__file__).resolve().parent
creator='Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs'
source=(root/creator).read_text(encoding='utf-8-sig')

def method(text, signature):
    start=text.index('        '+signature)
    brace=text.index('{',start); depth=1; end=brace+1
    while depth:
        depth += (text[end]=='{')-(text[end]=='}'); end+=1
    return start,end,text[start:end]

start,end,old=method(source,'private static void CreateFarmWalkInHouse()')
new=old.replace('float doorY = -hh;', '''float doorY = FarmHouseDoorArtAuthoring.DoorGroundY - cy;
            float northY = hh;
            float interiorHeight = northY - doorY;
            float interiorCenterY = (northY + doorY) * 0.5f;''')
new=new.replace('WorldSpriteLibrary.Ground("ground_soil")','WorldSpriteLibrary.Building("wall_plank")')
new=new.replace('WorldSpriteLibrary.Building("farmhouse_keyart_v4")','WorldSpriteLibrary.Building("farmhouse_opening_keyart_v4")')
new=new.replace('floorSr.size = new Vector2(w, h);','''floorSr.size = new Vector2(w, interiorHeight);
            // Tiled bounds are pivot-dependent; align the rendered floor to the useful room.
            var floorPivot = floorSr.sprite.pivot / floorSr.sprite.rect.size;
            floor.transform.localPosition = new Vector3((floorPivot.x - 0.5f) * w,
                interiorCenterY + (floorPivot.y - 0.5f) * interiorHeight, 0f);''')
new=new.replace('new Vector3(-hw, 0f, 0f), new Vector2(FarmWallThickness, h)','new Vector3(-hw, interiorCenterY, 0f), new Vector2(FarmWallThickness, interiorHeight)')
new=new.replace('new Vector3( hw, 0f, 0f), new Vector2(FarmWallThickness, h)','new Vector3( hw, interiorCenterY, 0f), new Vector2(FarmWallThickness, interiorHeight)')
new=new.replace('new Vector3(0f, -doorY, 0f)','new Vector3(0f, northY, 0f)')
new=new.replace('CreateFarmHouseDoor(house.transform, doorY);','var door = CreateFarmHouseDoor(house.transform, doorY);')
bed_start=new.index('            // Moveis DENTRO'); bed_end=new.index('            // BedLetter',bed_start)
new=new[:bed_start]+'''            // Furniture roots retain interaction identity; only their visuals are scaled.
            FarmHouseInteriorAuthoring.CreateFurniture<CindarsHope.World.BedInteractable>(
                house.transform, "Bed", WorldSpriteLibrary.Interior("bed"),
                new Vector2(12.7f, 14f), 2.2f, new Vector2(1.75f, 1.4f), 0.08f);

'''+new[bed_end:]
new=new.replace('new Vector3(-hw + 2.6f, hh - 1.1f, 0f)','new Vector3(14.35f - cx, 14.2f - cy, 0f)')
chest_start=new.index('            // FarmHouseChest'); chest_end=new.index('            // Exterior/telhado:',chest_start)
new=new[:chest_start]+'''            FarmHouseInteriorAuthoring.CreateFurniture<CindarsHope.World.FarmHouseChestInteractable>(
                house.transform, "FarmHouseChest", AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/chest_closed.png"),
                new Vector2(18.1f, 15.1f), 1.1f, new Vector2(1f, 0.48f), 0.04f);

'''+new[chest_end:]
new=new.replace('revealObj.transform.localPosition = Vector3.zero;','revealObj.transform.localPosition = new Vector3(0f, interiorCenterY, 0f);')
new=new.replace('revealTrigger.size = new Vector2(w, h);','revealTrigger.size = new Vector2(w - FarmWallThickness, interiorHeight - FarmWallThickness);')
new=new.replace('''            foreach (var interiorRenderer in house.GetComponentsInChildren<SpriteRenderer>())
                if (interiorRenderer != roofSr) interiorRenderers.Add(interiorRenderer);
            reveal.Configure(new[] { roofSr }, interiorRenderers: interiorRenderers.ToArray());''','''            var exteriorRenderers = new List<SpriteRenderer> { roofSr };
            foreach (var interiorRenderer in house.GetComponentsInChildren<SpriteRenderer>())
            {
                if (interiorRenderer == roofSr) continue;
                if (interiorRenderer.transform.IsChildOf(door.transform))
                {
                    // Leaf remains visible from either side, controlled only by its current pose.
                    if (interiorRenderer.gameObject.name == "Threshold") exteriorRenderers.Add(interiorRenderer);
                    continue;
                }
                interiorRenderers.Add(interiorRenderer);
            }
            reveal.Configure(exteriorRenderers.ToArray(), interiorRenderers: interiorRenderers.ToArray());
            reveal.ConfigureFeetOccupancy(revealTrigger);''')
new=new.replace('// Porta ao sul: doorY = -hh.','// The physical door meets the leaf; the approved exterior staircase stays below it.')
changed=source[:start]+new+source[end:]
start,end,old_door=method(changed,'private static void CreateFarmHouseDoor(Transform house, float doorY)')
new_door='''        private static HouseDoorInteractable CreateFarmHouseDoor(Transform house, float doorY)
        {
            var door = new GameObject("Door");
            door.transform.SetParent(house, false);
            door.transform.localPosition = new Vector3(0f, doorY, 0f);
            CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.TryAssignLayer(
                door, CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers.WorldSolid);
            var threshold = new GameObject("Threshold");
            threshold.transform.SetParent(door.transform, false);
            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(door.transform, false);
            var blocker = door.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(FarmDoorGapWidth, FarmWallThickness);
            var interact = door.AddComponent<BoxCollider2D>();
            interact.isTrigger = true;
            interact.size = new Vector2(FarmDoorGapWidth + 0.6f, FarmWallThickness + 2f);
            var interactable = door.AddComponent<HouseDoorInteractable>();
            FarmHouseDoorArtAuthoring.Configure(interactable, leaf.transform, threshold.transform, blocker);
            EditorUtility.SetDirty(interactable);
            return interactable;
        }'''
changed=changed[:start]+new_door+changed[end:]
patch=''.join(difflib.unified_diff(source.splitlines(True),changed.splitlines(True),fromfile='a/'+creator,tofile='b/'+creator))
(out/'farm-house-methods.patch').write_text(patch,encoding='utf-8')
(out/'CreateFarmWalkInHouse.method.cs.txt').write_text(new+'\n',encoding='utf-8')
(out/'CreateFarmHouseDoor.method.cs.txt').write_text(new_door+'\n',encoding='utf-8')
print('Candidate only:',out/'farm-house-methods.patch')
