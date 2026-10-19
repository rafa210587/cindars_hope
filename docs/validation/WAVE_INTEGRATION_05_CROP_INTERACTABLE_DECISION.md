# WAVE_INTEGRATION_05 Crop Interactable Decision

Date: 2026-06-08
Status: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED

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
- `CreateMvpFarmScene` can bind FarmScene plots to `_staminaManager` when the scene is regenerated through Unity Editor APIs.
- `CreateMvpFarmScene` can enable `_temporarySequentialSliceMode` only for `FarmPlot_00` during the WAVE_INTEGRATION_05 smoke hook.

## Scene corruption hotfix decision

The direct FarmScene YAML wiring from WAVE_INTEGRATION_05 was reverted after Unity reported:

```text
Unable to parse file Assets/_Game/Scenes/FarmScene.unity:
Parser Failure at line 7780: Expect ':' between key and value within mapping
Broken text PPtr local file identifier 910400010 doesn't exist
Dangling Transform/SpriteRenderer/BoxCollider2D/MonoBehaviour components 910400011..910400014
```

Decision:
- Restore `FarmScene.unity`, `TownScene.unity`, and `CaveScene.unity` to the WAVE_INTEGRATION_04 validated LFS pointers.
- Preserve code/runtime changes in `FarmPlot.cs` and `CreateMvpFarmScene.cs`.
- Do not edit `.unity` YAML by text patch for this wiring.
- Reapply scene wiring only via Unity Editor/Inspector or safe Editor API generation.

## Temporary validation hook

- The hook remains in code and is marked as `TODO_INTEGRATION_NOT_FINAL`.
- It is not considered final gameplay.
- It should only be scene-wired for smoke validation through Unity Editor/Inspector or generator flow.
- Final tool/seed/watering/day-growth UX remains deferred.

## Design/Direction Compliance Matrix

| Direction source | Rule | Implementation |
|---|---|---|
| `FARM_DESIGN_DIRECTION_v1.3.md` | Preserve existing farm base; do not reimplement from scratch. | Reused `FarmPlot`; no new farm system created. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Roadmap 1 needs dry/wet/planted/ready feedback. | Existing `FarmPlot.UpdateVisual()` remains the visual state adapter for scene plots. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Crops must progress by water/day and can die after dry days. | Existing `DayStartedEvent` processing, dry/wet states, and death threshold preserved. |
| `FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md` | Initial field is a Level 1 farm zone. | Existing `FarmPlot` runtime and crop field intent are preserved; scene wiring must be reapplied safely. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Mana fruit is not a common crop. | WAVE05 uses `seed_carrot`; no Mana crop/content added. |
| Save rules in existing farm runtime | Save persists simple IDs/state, not Unity refs. | Existing `FarmPlotSaveData` path preserved; no new save DTO created. |

## Gates

WAVE_INTEGRATION_06 gate: BLOCKED until WAVE_INTEGRATION_05 scene wiring is reapplied safely and human Play Mode checklist passes.

WAVE_INTEGRATION_07 gate from inventory reward perspective: PASS_CODE_READY.

Reason: harvest code grants real inventory items through `InventoryManager.AddItem(harvestItem.Id, amount)` using `Seed_Cenoura.HarvestItems -> item_crop_carrot`, but the scene wiring is currently reverted and requires Unity validation.

Residual gate: Play Mode remains pending human Unity validation.
