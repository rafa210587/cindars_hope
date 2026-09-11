# Stage12 facade depth correction — candidate only

2026-09-10. `farm-facade-depth.patch` changes only FarmHouseDoorArtAuthoring and three renderer lines in CreateFarmWalkInHouse. Read-only patch applicability PASS with `--ignore-space-change`. No Assets file, PNG, scene, collider, reveal state or runtime source changed by this slice.

## Finding and contract

Opened actual stage12 `motion/house_approach.png` and `inside_revealed.png`: the farmer is fully covered by the whole Roof-layer facade outside, then becomes visible with the interior. Moving the reveal threshold would mask this sorting defect.

The current scene's Player SpriteRenderer uses sorting layer ID3141592653 (World), order0 and Pivot. Player is a root transform without a containing SortingGroup. Captured camera metadata confirms CustomAxis(0,1,0); CameraTransparencySort2D restores the same axis.

The candidate persists a Sprite asset `World/building/farmhouse_opening_depth_v4.asset` referencing the existing212×205texture. Its bottom-left pixel pivot is(122,40). It preserves the source128PPU; existing door sprites continue using their32PPU default. The helper validates texture, rect, pivot and PPU on reuse. No new image/canvas or asset class is introduced.

With unchanged stair alignment(122,13), the facade sort pivot becomes:

`Y = HouseMinY + (40 - 13) × (HomesteadRoofTargetWidth /177) =11.597033898305085`

This equals DoorGroundY. Sprite pivot changes are cancelled by AlignSpriteSupport: for any source pixel p, `world(p)=stairWorld+(p-stairPixel)×sourcePixelWorldSize`. The formula does not depend on pivot. Native alpha bounds, visible footprint, world corners and scale therefore remain unchanged. Source importer uses alignment7(bottom-center); its serialized custom-pivot field is inactive.

The facade becomes World/order0/Pivot. Player feet south of its pivot sort in front; feet north sort behind until the unchanged useful-interior reveal. Leaf remains World/order0, excluded from both reveal lists; Threshold remains World/order-1 in the exterior list. All existing visibility configuration remains byte-for-byte unchanged by the patch.

## Evidence and residual checks

- `prepare-facade-depth-candidate.py` compared all43,460pixel centers before/after: maximum world error3.56e-15, floating-point roundoff only. Source PPU/scale, reveal code and leaf/threshold configuration preservation asserted. Report: `facade-depth-geometry.json`.
- Existing FarmSceneComposition validation measures visual width, uniform scale and building/player ratio, all preserved. No inspected Farm composition test requires the facade's old Roof layer/order.
- Regeneration must use the new helper so the persisted pivot asset exists. A scene saved before regeneration still has the old renderer settings.
- Inspect the next actual outside/closed, opening, inside and exiting captures. A small transition band between leaf support and useful-interior inset is intentionally unrevealed; this patch does not advance reveal.
- Facade and Leaf share the sameY/order. Their normal pixels are separated by the transparent aperture; the open leaf's few pixels below that aperture can tie with stair pixels. Confirm those exposed edge pixels in the animation capture. No tie-break workaround is included in this minimal patch.
- World sorting may change overlap with foreground props compared with the former always-front Roof layer; regional capture should confirm those contacts too.

Unity validation: NOT RUN
Reason: Root owns the coordinated Unity window; requested read-only analysis and candidates outside Assets.
Command attempted: None.
Residual risk: Actual render sorting and visual acceptance await root's regenerated scene capture.
