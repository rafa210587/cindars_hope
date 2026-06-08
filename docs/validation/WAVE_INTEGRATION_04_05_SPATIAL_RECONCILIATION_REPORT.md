# WAVE INTEGRATION 04+05 Spatial Reconciliation Report

Date: 2026-06-08
Status: CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

## Summary

Spatial and functional reconciliation of WAVE_INTEGRATION_04 and WAVE_INTEGRATION_05 after FarmScene YAML corruption and revert to WAVE03 baseline.

WAVE_INTEGRATION_04 zone marker layer and WAVE_INTEGRATION_05 crop wiring were lost when FarmScene.unity was reverted. The code (CreateMvpFarmScene.cs and FarmPlot.cs) was preserved.

This reconciliation:
1. Redesigned the farm layout to fix readability problems that caused the original report ("spread/randomized, zones not readable")
2. Updated CreateMvpFarmScene.cs with the corrected layout
3. Confirmed WAVE05 FarmPlot.cs code remains intact
4. Documented what a human must do to apply the safe wiring

## Layout Problems Found

| Problem | Evidence | Resolution |
|---|---|---|
| Trees 3-6 on west side of map | Positions (-8.2, 4.6) (-6.4, 3.2) (-8.4, 1.2) (-7.6, -3.1) overlap cave zone | Moved all 19 trees to east cluster (x: 6-13) |
| CropField adjacent to cave portal | FarmPlots parent (-4.75, -1) next to cave portal (-5.5, 0) | Moved FarmPlots parent to (1, -1.5) |
| HouseEntrance near bounds | (-12.4, -5.6) barely inside 40x34 bounds | Moved to (-5, -7) — visible southwest |
| Zone_ResourceTrees/trees mismatch | Zone at (7.5, 1.2) but trees on both sides of map | Zone now covers all consolidated trees at (9.5, 1.0) size 10x13 |
| Bounds too large (40x34) | Content was sparse at edges | Reduced to 28x22 |

## WAVE04 Status After Reconciliation
Status: CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

Preserved:
- All 11 zone markers defined in CreateMvpFarmScene.CreateFarmSceneFoundationZones()
- All zone stable IDs intact
- FarmSceneZoneMarker.cs unchanged

Changed:
- Zone positions updated for better spatial separation
- Tree positions consolidated to east cluster
- Bounds reduced from 40x34 to 28x22
- FarmPlots moved away from portals

Not in scene yet: requires human to run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene

## WAVE05 Status After Reconciliation
Status: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED

Preserved:
- FarmPlot.cs: seed resolution via SeedDataSO.SeedItem.Id
- FarmPlot.cs: harvest via InventoryManager.AddItem(harvestItem.Id, amount)
- FarmPlot.cs: _temporarySequentialSliceMode for FarmPlot_00 smoke hook
- CreateMvpFarmScene.cs: passes staminaManager to FarmPlot, enables smoke hook on FarmPlot_00
- No new crop system created (decision: REUSE_EXISTING_FARMPLOT_RUNTIME)

Crop objects in scene: ABSENT (scene reverted). Will be present after human runs generator.
Crop objects preserved in code: YES.
Duplicate crop system: NONE.
Orphaned crop plots outside CropField: NONE (generator creates them within CropField parent).

## Design Compliance
- No new crop system created (WAVE05 decision honored)
- No parallel FarmCropPlotInteractable (FarmPlot reused)
- No Unity YAML edited directly
- WAVE06 gate: still BLOCKED until human Play Mode checklist passes

## What Still Requires Human Unity Action

| Action | Why |
|---|---|
| Run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene | Regenerates FarmScene with corrected layout |
| Confirm Console: no errors | Validates YAML, missing scripts, broken refs |
| Run Play Mode: walk player, interact with FarmPlot_00 | Validates WAVE05 smoke loop |
| Confirm zones are visually readable | Validates spatial reconciliation goal |

## Post-WAVE05 Spatial Reconciliation

Problem visual: Trees scattered across west+east, crop field near cave portal, zones unreadable.
Status before: SCENE_REVERTED_TO_WAVE03 (both WAVE04 and WAVE05 wiring absent from scene).
Layout new: Trees east cluster (x:6-13), CropField center (1, -2.25), zones distinct and aligned.
WAVE05 objects preserved: FarmPlot.cs seed/harvest/stamina/smoke code intact.
Crop plots repositioned: Will be at (1, -1.5) parent + 3x3 grid — inside Zone_CropField at (1, -2.25).
References corrected: CreateMvpFarmScene wires _seedDatabase, _staminaManager, _inventoryManager on each FarmPlot.
Duplicates removed: N/A — no duplicate plots exist (code path creates exactly 9).
Human action needed: Run generator in Unity Editor.
New status: CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED.

## Validation

Assembly-CSharp: PASS (exit code 0)
Assembly-CSharp-Editor: PASS (exit code 0)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing governance failures only)
Unity validation: NOT RUN — YAML editing forbidden; requires human Unity Editor action
Scene changed: NO — FarmScene.unity not modified by this reconciliation
