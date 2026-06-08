# WAVE_INTEGRATION_05 Crop Interactable Decision

Date: 2026-06-08
Status: BUILD_VALIDATED_SCENE_WIRED_WITH_TEMP_SMOKE_HOOK

## Decision

Use `FarmPlot` as the crop interactable runtime for FarmScene. Do not create a parallel `FarmCropPlotInteractable`.

Rationale:
- `FarmPlot` already implements `IInteractable`.
- `FarmPlot` already owns soil states, watering, growth, harvest, save DTO capture/restore, and event publication.
- `FarmPlot.TryHarvest()` already grants real inventory rewards through `InventoryManager.AddItem(...)` and publishes `CropHarvestedEvent`.
- Creating a second interactable would duplicate farm gameplay state and risk diverging from save/runtime ownership.

## Integration Choice

Chosen strategy: `REUSE_EXISTING_FARMPLOT_RUNTIME`.

Additional bridge:
- `FarmPlot` now resolves inventory seed items through `SeedDataSO.SeedItem.Id`, so `item_seed_carrot` can plant runtime seed `seed_carrot`.
- FarmScene plots now bind `_staminaManager`.
- The FarmScene generator mirrors the same wiring.

Temporary validation hook:
- `FarmPlot_00` has `_temporarySequentialSliceMode` enabled for WAVE_INTEGRATION_05 smoke validation only.
- The hook is marked in code as `TODO_INTEGRATION_NOT_FINAL`.
- It bypasses missing equipped hoe/watering-can flow and adds a menu action to simulate crop growth to ready state.
- Harvest still uses the real `SeedDataSO` and `InventoryManager` path.

## Design/Direction Compliance Matrix

| Direction source | Rule | Implementation |
|---|---|---|
| `FARM_DESIGN_DIRECTION_v1.3.md` | Preserve existing farm base; do not reimplement from scratch. | Reused `FarmPlot`; no new farm system created. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Roadmap 1 needs dry/wet/planted/ready feedback. | Existing `FarmPlot.UpdateVisual()` remains the visual state adapter for scene plots. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Crops must progress by water/day and can die after dry days. | Existing `DayStartedEvent` processing, dry/wet states, and death threshold preserved. |
| `FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md` | Initial field is a Level 1 farm zone. | Existing `FarmPlots` inside the WAVE_INTEGRATION_04 crop field area were reused. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Mana fruit is not a common crop. | WAVE05 uses `seed_carrot`; no Mana crop/content added. |
| Save rules in existing farm runtime | Save persists simple IDs/state, not Unity refs. | Existing `FarmPlotSaveData` path preserved; no new save DTO created. |

## Gates

WAVE_INTEGRATION_07 gate from inventory reward perspective: PASS.

Reason: harvest grants real inventory items through `InventoryManager.AddItem(harvestItem.Id, amount)` using `Seed_Cenoura.HarvestItems -> item_crop_carrot`.

Residual gate: Play Mode remains pending human Unity validation.
