# WAVE_INTEGRATION_05 Crop Interactable Report

Date: 2026-06-08
Status: BUILD_VALIDATED_SCENE_WIRED_WITH_TEMP_SMOKE_HOOK

## Summary

FarmScene now has a first crop gameplay slice connected to real scene objects through the existing `FarmPlot` runtime.

Implemented:
- Reused `FarmPlot` as the scene interactable.
- Connected all 9 FarmScene plots to the scene `StaminaManager`.
- Kept `FarmPlotRegistry` and `FarmSceneRuntimeReferenceInstaller` scene references intact.
- Updated `CreateMvpFarmScene` so regenerated FarmScene plots keep the same stamina and temporary smoke wiring.
- Fixed seed planting resolution so inventory item `item_seed_carrot` maps to runtime seed `seed_carrot`.
- Enabled `_temporarySequentialSliceMode` on `FarmPlot_00` only, for WAVE05 smoke validation.

Not implemented:
- No new ScriptableObjects.
- No new sprites, tiles, prefabs, or animator controllers.
- No new farm save system.
- No UI/HUD/shop/crafting/shipping/lake/tree/NPC changes.

## Runtime Path

Expected smoke loop on `FarmPlot_00`:

1. Interact with the plot.
2. `Raw -> TilledDry` via Arar solo.
3. `TilledDry -> TilledWet` via Molhar solo.
4. Plant `seed_carrot` resolved from `item_seed_carrot`.
5. `PlantedWet -> ReadyToHarvest` via temporary `Simular crescimento`.
6. Harvest grants `item_crop_carrot` through `InventoryManager.AddItem(...)`.

The temporary growth action is not final gameplay. It exists only because Play Mode smoke needs a complete local loop before the final equipment/tool/day-growth UX is validated.

## Design/Direction Compliance Matrix

| Direction source | Rule | Result |
|---|---|---|
| `FARM_DESIGN_DIRECTION_v1.3.md` | Reuse existing farm base. | PASS: `FarmPlot` reused. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Preserve crop states and watering/death concepts. | PASS: existing states and `DayStartedEvent` logic preserved. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Provide visible crop/soil state feedback. | PASS_STRUCTURAL: existing sprite/color visual states remain wired. |
| `FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md` | Initial crop field belongs to Level 1 farm. | PASS: current FarmScene 3x3 plot cluster reused in crop field area. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Do not treat Mana fruit as normal crop. | PASS: no Mana content added. |
| Save constraints | Save simple IDs/state only. | PASS: no new Unity refs added to save DTOs. |

## Validation

Assembly-CSharp:
- PASS
- Command: `dotnet build Assembly-CSharp.csproj --no-restore`
- Result: 0 warnings, 0 errors

Assembly-CSharp-Editor:
- PASS
- Command: `dotnet build Assembly-CSharp-Editor.csproj --no-restore`
- Result: 3 pre-existing warnings, 0 errors
- Warnings:
  - `CreateEnemyActionsAndSets.ActionEntry.MinRange` CS0649
  - `CreateEnemyActionsAndSets.ActionEntry.RequiresLos` CS0649
  - `CSharpProjectPostprocessor.OnGeneratedCSProject` UNT0006

Static scene checks:
- FarmPlot names in FarmScene: 9
- Temporary sequential mode enabled: 1
- Temporary seed fields: 9
- FarmScene plot stamina refs updated to scene `StaminaManager`.
- FarmPlot seed/stamina/temp blocks: 9

Unity validation: NOT RUN
Reason: local Unity Editor Play Mode was not launched in this Codex turn.
Command attempted: not attempted
Residual risk: Unity compile/import and Play Mode interaction behavior not validated locally.

Docs validation:
- EXPECTED_FAIL_LEGACY_ONLY
- Command: `powershell -ExecutionPolicy Bypass -File tools/docs/validate_docs.ps1`
- Result: exit code 1
- Failures were pre-existing governance/doc issues around `spec_test_harness_editmode_playmode_quality_gate.md`, legacy recent validation report metadata, and two implemented specs citing amendments as canonical sources.

## WAVE_INTEGRATION_07 Gate

Inventory reward gate: PASS.

Harvest is not feedback-only. The path uses `SeedDataSO.HarvestItems` and `InventoryManager.AddItem(...)`, with `Seed_Cenoura` producing `item_crop_carrot` x2.

Play Mode gate: PENDING HUMAN.
