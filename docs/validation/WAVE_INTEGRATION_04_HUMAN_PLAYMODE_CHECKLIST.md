# WAVE INTEGRATION 04 - Human Play Mode Checklist

## Status Update (2026-06-08)

BLOCKED_PENDING_SCENE_REGENERATION -> CODE_READY_TO_REGENERATE

Before running this checklist:
1. Open Unity Editor
2. Run: CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene
3. Confirm no Console errors
4. Then execute the checklist below

## Play Mode Checklist — WAVE_INTEGRATION_04+05 Reconciliation

Precondition: CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene has been run in Unity Editor.

### Scene Opens
- [ ] FarmScene opens without Unity Console errors
- [ ] No missing script warnings
- [ ] No broken PPtr references

### Layout Readability
- [ ] Player visible at center
- [ ] CropField recognizable as distinct area (3x3 brown plots, center of map)
- [ ] Trees clustered visibly on east side (no trees on west)
- [ ] LakeFishing zone visible on east-center
- [ ] Cave portal visible on left
- [ ] Town portal visible on bottom-left
- [ ] Zone markers do not overlap player spawn area
- [ ] Debug overlay (if present) does not cover crop field

### Player Movement
- [ ] Player moves in all 4 directions
- [ ] Camera follows player
- [ ] Player does not clip through bounds

### Crop Interaction (WAVE05 smoke loop on FarmPlot_00)
- [ ] Walk player to FarmPlot_00 (first/closest plot)
- [ ] Interact -> see option "Arar solo"
- [ ] Plot changes to TilledDry state (color/visual change)
- [ ] Interact -> see option "Molhar solo"
- [ ] Plot changes to TilledWet state
- [ ] Interact -> see option "Plantar" with seed_carrot
- [ ] Plot changes to Planted state
- [ ] Interact -> see option "Simular crescimento" (temporary smoke hook)
- [ ] Plot changes to ReadyToHarvest state
- [ ] Interact -> "Harvest" -> item_crop_carrot appears in inventory (or debug log)
- [ ] No Unity Console errors during full smoke loop

### Zone Navigation
- [ ] Walk to town portal area (southwest) — zone visible
- [ ] Walk to cave portal area (left) — zone visible
- [ ] Walk toward east — trees visible in cluster
- [ ] Lake area visible on east-center

### Stop Play Mode
- [ ] Exit Play Mode without corrupting scene
- [ ] FarmScene.unity unchanged after Play Mode exit

## Previous Status (archived)

Status was PENDING

## Preconditions

- Unity opens without red Console errors.
- WAVE_INTEGRATION_03 player/camera baseline is complete.
- Target FarmScene is `Assets/_Game/Scenes/FarmScene.unity`.
- `FarmSceneFoundationZones` exists in the scene hierarchy.

## Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open FarmScene | Scene opens without missing script warnings |  |  |
| Inspect hierarchy | `FarmSceneFoundationZones` exists with 11 `Zone_*` children |  |  |
| Press Play | Game starts |  |  |
| Player visible | Player appears in FarmScene near `farm_default` |  |  |
| Move player | Player moves inside farm |  |  |
| Camera | Camera frames/follows player |  |  |
| Farm bounds | Player does not leave expected playable area |  |  |
| Crop area | Crop area is visually identifiable around `FarmPlots` |  |  |
| Resource areas | Tree, rock, and forage zones are visually identifiable |  |  |
| Lake area | Lake/fishing area is visible or marked |  |  |
| Shipping area | Shipping/sellpoint zone is visible or marked |  |  |
| House/town/cave markers | House entrance, Town exit, and Cave entrance markers exist |  |  |
| Stop Play | Scene does not retain unwanted Play Mode changes |  |  |

## Result

PENDING

## Bugs found

TBD

## Can start WAVE_INTEGRATION_05

YES after this checklist passes or the human explicitly accepts the residual Play Mode risk.
