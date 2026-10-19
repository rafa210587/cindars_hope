# WAVE INTEGRATION 04+05 Spatial Reconciliation Audit

Date: 2026-06-08
Status: AUDIT_COMPLETE

## Target Scene
Assets/_Game/Scenes/FarmScene.unity
Current state: WAVE03 BASELINE (FarmScene reverted after YAML corruption in WAVE04+05)

## Scene History
- WAVE03 baseline: commit 3169116 (player, camera, portals, spawn points, bounds)
- WAVE04: Added FarmSceneFoundationZones with 11 zone markers — scene YAML became corrupted
- WAVE04 hotfix: Reverted to WAVE03 baseline (WAVE04 pointer also reproduced corruption errors)
- WAVE05: FarmPlot.cs + CreateMvpFarmScene.cs code preserved; scene still WAVE03

## Code Preserved from WAVE04+05
- FarmPlot.cs: WAVE05 crop interactable logic (seed resolution, harvest, stamina, smoke hook)
- CreateMvpFarmScene.cs: WAVE04 zone creation + WAVE05 plot wiring

## WAVE05 Objects State
- FarmCropPlotInteractable: NOT APPLICABLE — FarmPlot reused as crop interactable
- FarmCropPlotVisualController: NOT APPLICABLE — FarmPlot.UpdateVisual() is the visual controller
- FarmPlot instances in scene: ABSENT (scene is WAVE03 baseline; generator must be run)
- Crop interactable code: PRESERVED in FarmPlot.cs
- Reward/fallback strategy: PRESERVED (InventoryManager.AddItem path, seed_carrot smoke hook)

## Layout Problems Identified in WAVE04 Layout (now reverted)
1. TREES_SCATTERED: 4 of 19 trees at negative X (-8.2, -6.4, -8.4, -7.6), outside ResourceTrees zone
2. CROPFIELD_NEAR_PORTALS: FarmPlots parent at (-4.75, -1) adjacent to cave portal at (-5.5, 0)
3. HOUSE_OFF_SCREEN: HouseEntrance zone at (-12.4, -5.6), near bounds edge
4. ZONE_TREE_MISMATCH: Zone_ResourceTrees at (7.5, 1.2) did not cover west trees
5. BOUNDS_OVERSIZED: 40x34 world units with sparse content; farm feels empty

## Classification

| Object/Zone | Status | Problem | Action |
|---|---|---|---|
| FarmPlot_00..08 | MISSING | Scene reverted | Human must run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene |
| Zone_PlayerSpawn | MISSING | Scene reverted | Code correct; human must regenerate |
| Zone_CropField | MISSING | Scene reverted + position updated | Code fixed (moved to 1, -2.25); human must regenerate |
| Zone_ResourceTrees | MISSING | Scene reverted + position updated | Code fixed with tree consolidation (9.5, 1.0); human must regenerate |
| Zone_ResourceRocks | MISSING | Scene reverted | Code correct; human must regenerate |
| Zone_Forage | MISSING | Scene reverted + position updated | Code fixed (moved to -8, -2); human must regenerate |
| Zone_LakeFishing | MISSING | Scene reverted | Code correct; aligns with FishingSpot at (7.8, -2.8) |
| Zone_ShippingSellpoint | MISSING | Scene reverted + position updated | Code fixed (moved to 3.5, 7.5); human must regenerate |
| Zone_Construction | MISSING | Scene reverted + position updated | Code fixed (moved to -1, 9); human must regenerate |
| Zone_HouseEntrance | MISSING | Scene reverted + position fixed | Code fixed (moved from -12.4, -5.6 to -5, -7); human must regenerate |
| Zone_TownExit | MISSING | Scene reverted | Code correct; aligns with existing portal at (-8.25, -4.75) |
| Zone_CaveEntrance | MISSING | Scene reverted | Code correct; aligns with existing portal at (-5.5, 0) |
| Player | PRESENT | WAVE03 baseline intact | OK |
| Camera | PRESENT | WAVE03 baseline intact | OK |
| Portals | PRESENT | WAVE03 baseline intact | OK |
| Bounds | PRESENT | WAVE03 baseline intact | Will be replaced when generator runs (28x22) |
| Trees 0-2, 8-18 in WAVE03 | PRESENT | Old positions (scattered) | Generator will recreate consolidated east cluster |
| Trees 3-7 in WAVE03 | PRESENT | Were on west side | Generator will move to east cluster |

## Next Action
Human must open Unity Editor and run: CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene
This will regenerate the ENTIRE FarmScene with the corrected layout.
Do NOT manually edit FarmScene.unity YAML.
