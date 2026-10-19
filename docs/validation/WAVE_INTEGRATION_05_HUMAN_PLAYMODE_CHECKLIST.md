# WAVE_INTEGRATION_05 Human Play Mode Checklist

Date: 2026-06-08
Target scene: `Assets/_Game/Scenes/FarmScene.unity`
Status: CODE_READY_TO_REGENERATE

## Status Update (2026-06-08)

BLOCKED_PENDING_SCENE_REGENERATION -> CODE_READY_TO_REGENERATE

Before running this checklist:
1. Open Unity Editor
2. Run: CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene
3. Confirm FarmScene.unity loads without Console errors
4. Enter Play Mode
5. Execute checklist items below (especially FarmPlot_00 smoke loop)

## Current state after hotfix

The WAVE_INTEGRATION_05 code is preserved, but direct scene YAML wiring was reverted after Unity failed to parse `FarmScene.unity`.

Before this checklist can be executed, reapply the crop smoke wiring safely through one of these options:

1. Use Unity Editor/Inspector to wire `FarmPlot_00`.
2. Regenerate/update FarmScene through the safe Editor API path in `CreateMvpFarmScene`.
3. Use a Unity Editor script that opens and modifies the scene through Unity serialization APIs, not text patching.

Do not edit `.unity` YAML manually.

## Required setup before Play Mode

- Open `FarmScene`.
- Confirm no scene parse error.
- Confirm no Missing Script or broken PPtr errors.
- Confirm `FarmPlot_00` exists.
- Confirm `FarmPlot_00` has `FarmPlot`.
- Confirm `FarmPlot_00` has `_temporarySequentialSliceMode = true` only for WAVE05 smoke validation.
- Confirm `FarmPlot_01` to `FarmPlot_08` do not have temporary sequential mode enabled.
- Confirm plots have valid `InventoryManager`, `SeedDatabaseSO`, and preferably `StaminaManager` references.
- Enter Play Mode.
- Confirm no boot errors for missing scene databases/managers.
- Move player to `FarmPlot_00` in the initial 3x3 plot cluster.

## Crop Smoke Loop

Use the interaction key `E`.

1. Interact with `FarmPlot_00`.
2. Select `Arar solo`.
3. Reopen the plot and select `Molhar solo`.
4. Reopen the plot and select `Plantar Carrot Seed`.
5. Reopen the plot and select `Simular crescimento`.
6. Reopen the plot and select `Colher`.

Expected:
- The plot changes visual state after till/water/plant/ready/harvest.
- Harvest adds carrot crop inventory reward through the real `InventoryManager` path.
- No exception appears in Console.
- Player movement/interactions still work after closing the plot menu.

## Negative Checks

- Other farm zones remain navigable.
- Trees, lake, forage, shop, NPCs, portals, combat, cave, and HUD behavior are unchanged by this spec.
- `FarmPlot_01` to `FarmPlot_08` do not show `Simular crescimento` unless tool/day growth flow reaches valid states through normal gameplay.

## Result

PENDING.

## Residual Note

`FarmPlot_00` uses `_temporarySequentialSliceMode` only for this smoke test. This is not final farm gameplay and is marked as `TODO_INTEGRATION_NOT_FINAL` in code.
