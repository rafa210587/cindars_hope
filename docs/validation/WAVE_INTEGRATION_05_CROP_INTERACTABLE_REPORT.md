# WAVE_INTEGRATION_05 Crop Interactable Report

Date: 2026-06-08
Status: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED

## Summary

WAVE_INTEGRATION_05 code/runtime changes remain valid as a code-ready crop gameplay slice, but the direct `.unity` scene wiring was reverted after Unity detected a corrupted FarmScene YAML.

Implemented and preserved:
- Reused `FarmPlot` as the crop interactable runtime.
- Updated `FarmPlot` so inventory item `item_seed_carrot` can resolve to runtime seed `seed_carrot` through `SeedDataSO.SeedItem.Id`.
- Kept harvest on the real inventory path through `InventoryManager.AddItem(...)`.
- Added a temporary smoke hook in code with `TODO_INTEGRATION_NOT_FINAL`.
- Updated `CreateMvpFarmScene` so regenerated FarmScene plots can keep the same stamina and temporary smoke wiring.

Reverted by hotfix:
- `Assets/_Game/Scenes/FarmScene.unity`
- `Assets/_Game/Scenes/TownScene.unity`
- `Assets/_Game/Scenes/CaveScene.unity`

Reason: direct scene YAML wiring produced an invalid scene file.

Not implemented:
- No new ScriptableObjects.
- No new sprites, tiles, prefabs, or animator controllers.
- No new farm save system.
- No UI/HUD/shop/crafting/shipping/lake/tree/NPC changes.

## Runtime Path

Expected smoke loop after safe Unity scene wiring is reapplied to `FarmPlot_00`:

1. Interact with the plot.
2. `Raw -> TilledDry` via Arar solo.
3. `TilledDry -> TilledWet` via Molhar solo.
4. Plant `seed_carrot` resolved from `item_seed_carrot`.
5. `PlantedWet -> ReadyToHarvest` via temporary `Simular crescimento`.
6. Harvest grants `item_crop_carrot` through `InventoryManager.AddItem(...)`.

The temporary growth action is not final gameplay. It exists only because Play Mode smoke needs a complete local loop before the final equipment/tool/day-growth UX is validated.

## Scene corruption detected

Unity reported this while opening `Assets/_Game/Scenes/FarmScene.unity`:

```text
Unable to parse file Assets/_Game/Scenes/FarmScene.unity:
[Parser Failure at line 7780: Expect ':' between key and value within mapping]

Broken text PPtr in file Assets/_Game/Scenes/FarmScene.unity.
Local file identifier 910400010 doesn't exist.

Dangling components deleted by Unity:
- Transform FileID 910400011
- SpriteRenderer FileID 910400012
- BoxCollider2D FileID 910400013
- MonoBehaviour FileID 910400014
```

Classification:

```text
BLOCKED_AFTER_UNITY_OPEN_SCENE_PARSE
```

Resolution:
- Do not save the corrupted scene.
- Do not repair the YAML line manually.
- Restore scene LFS pointers to the WAVE_INTEGRATION_04 validated commit.
- Reapply WAVE05 scene wiring only through Unity Editor/Inspector or a safe Editor API generator.

## Design/Direction Compliance Matrix

| Direction source | Rule | Result |
|---|---|---|
| `FARM_DESIGN_DIRECTION_v1.3.md` | Reuse existing farm base. | PASS: `FarmPlot` reused. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Preserve crop states and watering/death concepts. | PASS: existing states and `DayStartedEvent` logic preserved. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Provide visible crop/soil state feedback. | PASS_CODE_READY: existing sprite/color visual states remain in code; scene wiring needs safe Unity reapplication. |
| `FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md` | Initial crop field belongs to Level 1 farm. | PASS_CODE_READY: current WAVE04 FarmScene foundation preserved; WAVE05 wiring reverted. |
| `FARM_DESIGN_DIRECTION_v1.3.md` | Do not treat Mana fruit as normal crop. | PASS: no Mana content added. |
| Save constraints | Save simple IDs/state only. | PASS: no new Unity refs added to save DTOs. |

## Validation

Assembly-CSharp:
- PASS reported before hotfix.
- Command: `dotnet build Assembly-CSharp.csproj --no-restore`
- Result: 0 warnings, 0 errors

Assembly-CSharp-Editor:
- PASS reported before hotfix.
- Command: `dotnet build Assembly-CSharp-Editor.csproj --no-restore`
- Result: 3 pre-existing warnings, 0 errors

Docs validation:
- EXPECTED_FAIL_LEGACY_ONLY reported before hotfix.
- Failures were pre-existing governance/doc issues around `spec_test_harness_editmode_playmode_quality_gate.md`, legacy recent validation report metadata, and two implemented specs citing amendments as canonical sources.

Unity validation:
- FAIL before hotfix due to scene parse corruption.
- After hotfix, human must reopen Unity and validate that `FarmScene.unity` loads from the restored WAVE04 pointer.

## WAVE_INTEGRATION_07 Gate

Inventory reward gate: PASS_CODE_READY.

Harvest is not feedback-only. The code path uses `SeedDataSO.HarvestItems` and `InventoryManager.AddItem(...)`, with `Seed_Cenoura` producing `item_crop_carrot` x2.

Scene/Play Mode gate: BLOCKED until WAVE05 wiring is reapplied safely and Play Mode checklist passes.

## Decision

- Can start WAVE_INTEGRATION_06: NO.
- Blocking issue: WAVE05 scene wiring was reverted after FarmScene YAML corruption.
- Required next action: reapply WAVE05 plot wiring via Unity Editor/Inspector or safe Editor API; then run human Play Mode checklist.
