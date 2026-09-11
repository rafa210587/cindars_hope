"""Stage12 facade depth fix: candidate only, preserving all live source files."""
from pathlib import Path
import difflib,json,re
root=Path(__file__).resolve().parents[5]
out=Path(__file__).resolve().parent
helperpath='Assets/_Game/Scripts/Editor/Art/FarmHouseDoorArtAuthoring.cs'
creatorpath='Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs'
helper=(root/helperpath).read_text(encoding='utf-8-sig')
creator=(root/creatorpath).read_text(encoding='utf-8-sig')
addition='''        internal static Sprite FacadeAtDoorDepth()
        {
            var source = WorldSpriteLibrary.Building("farmhouse_opening_keyart_v4");
            if (source == null || source.texture.width != 212 || source.texture.height != 205)
                throw new InvalidOperationException("Farm facade depth requires the unchanged212x205 opening source.");
            // Ground sorting follows the leaf support(y165 from top), while the creator still
            // aligns source stair support(122,13). All rendered pixels retain their world positions.
            return PersistSprite(source, "farmhouse_opening_depth_v4", new Rect(0f, 0f, 212f, 205f),
                new Vector2(122f / 212f, 40f / 205f), "building", source.pixelsPerUnit);
        }

'''
needle='        internal static void Configure('
assert helper.count(needle)==1
changedhelper=helper.replace(needle,addition+needle)
changedhelper=changedhelper.replace('private static Sprite PersistSprite(Sprite source, string name, Rect rect, Vector2 pivot)',
    'private static Sprite PersistSprite(Sprite source, string name, Rect rect, Vector2 pivot,\n            string category = "props", float pixelsPerUnit = 32f)')
changedhelper=changedhelper.replace('"Assets/_Game/Art/Generated/World/props/" + name + ".asset"','"Assets/_Game/Art/Generated/World/" + category + "/" + name + ".asset"')
changedhelper=changedhelper.replace('if (sprite.texture != source.texture || sprite.rect != rect ||',
    'if (sprite.texture != source.texture || sprite.rect != rect || !Mathf.Approximately(sprite.pixelsPerUnit, pixelsPerUnit) ||')
changedhelper=changedhelper.replace('Sprite.Create(source.texture, rect, pivot, 32f, 0, SpriteMeshType.FullRect)',
    'Sprite.Create(source.texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect)')
assert helper!=changedhelper
needle='var houseRoofSprite = WorldSpriteLibrary.Building("farmhouse_opening_keyart_v4");'
assert creator.count(needle)==1
changedcreator=creator.replace(needle,'var houseRoofSprite = FarmHouseDoorArtAuthoring.FacadeAtDoorDepth();')
needle='''            roofSr.sortingOrder = 20;
            TrySetSortingLayer(roofSr, "Roof", 20);'''
assert changedcreator.count(needle)==1
changedcreator=changedcreator.replace(needle,'''            roofSr.sortingOrder = 0;
            TrySetSortingLayer(roofSr, "World", 0);''')
patch=''
for path,before,after in [(helperpath,helper,changedhelper),(creatorpath,creator,changedcreator)]:
    patch+=''.join(difflib.unified_diff(before.splitlines(True),after.splitlines(True),fromfile='a/'+path,tofile='b/'+path))
(out/'farm-facade-depth.patch').write_text(patch,encoding='utf-8')
(out/'FarmHouseDoorArtAuthoring.facade-depth.cs.txt').write_text(changedhelper,encoding='utf-8')

# Geometry of AlignSpriteSupport with unchanged PPU/scale: compare all pixel centers.
ppu=128.0; source_pixel_world=9.65/177; scale=source_pixel_world*ppu
support=(122.,13.); world_support=(15.3,10.125)
oldpivot=(106.,0.); newpivot=(122.,40.)
def world(px,pivot):
    local_support=tuple((support[i]-pivot[i])/ppu for i in (0,1))
    origin=tuple(world_support[i]-local_support[i]*scale for i in (0,1))
    return tuple(origin[i]+(px[i]-pivot[i])/ppu*scale for i in (0,1))
max_error=0.0
for y in range(205):
    for x in range(212):
        before=world((x+.5,y+.5),oldpivot); after=world((x+.5,y+.5),newpivot)
        max_error=max(max_error,*(abs(before[i]-after[i]) for i in (0,1)))
assert max_error<1e-12
pivotworld=world(newpivot,newpivot)
assert abs(pivotworld[1]-(10.125+27*source_pixel_world))<1e-12
assert 'reveal.ConfigureFeetOccupancy(revealTrigger);' in changedcreator
assert creator.split('            // Trigger de revelacao do telhado.',1)[1]==changedcreator.split('            // Trigger de revelacao do telhado.',1)[1]
assert changedhelper.split('        internal static void Configure(',1)[1].split('        private static Sprite PersistSprite',1)[0]==helper.split('        internal static void Configure(',1)[1].split('        private static Sprite PersistSprite',1)[0]
report=dict(status='STATIC_PASS',pixelCentersChecked=212*205,maximumWorldError=max_error,
    facadePivotWorld=pivotworld,leafGroundY=10.125+27*source_pixel_world,
    sourcePpuPreserved=True,scalePreserved=True,revealCodeUnchanged=True,leafThresholdCodeUnchanged=True,
    unity='NOT RUN: candidate only')
(out/'facade-depth-geometry.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
