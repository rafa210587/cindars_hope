# WAVE_INTEGRATION_05 Human Play Mode Checklist

Date: 2026-06-08
Target scene: `Assets/_Game/Scenes/FarmScene.unity`

## Setup

- Open `FarmScene`.
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

## Residual Note

`FarmPlot_00` uses `_temporarySequentialSliceMode` for this smoke test. This is not final farm gameplay and is marked as `TODO_INTEGRATION_NOT_FINAL` in code.
