# WAVE_INTEGRATION_05 — Scene Corruption Hotfix Report

Date: 2026-06-08
Status: HOTFIX_APPLIED_CODE_READY_FARMSCENE_REVERTED_TO_WAVE03

## Summary

Unity could not open `Assets/_Game/Scenes/FarmScene.unity` after the scene integration work. Restoring the WAVE_INTEGRATION_04 pointer was not sufficient: Unity still reported the same broken local file identifiers `910400010..910400014`. Therefore the FarmScene was restored to the WAVE_INTEGRATION_03 baseline pointer, before the WAVE04 foundation marker layer.

WAVE05 code changes were preserved.

## Error class

```text
SCENE_YAML_CORRUPTION_FROM_DIRECT_TEXT_PATCH_OR_PRIOR_SCENE_PATCH
```

Unity reported a parser failure in `FarmScene.unity`, broken local file references, and dangling components. This is treated as scene serialization corruption, not as the primary C# logic bug.

## Scenes restored

| File | Restored source | Reason |
|---|---|---|
| `Assets/_Game/Scenes/FarmScene.unity` | `3169116654e242deec997f9dca2813bd2db6131b` | WAVE04 FarmScene pointer still reproduced the broken `910400010..014` errors; reverted to WAVE03 baseline |
| `Assets/_Game/Scenes/TownScene.unity` | `a7418a03b016fb1f3f2c21de1f59881ba8ad6117` | Reverted because WAVE05 should not alter TownScene |
| `Assets/_Game/Scenes/CaveScene.unity` | `a7418a03b016fb1f3f2c21de1f59881ba8ad6117` | Reverted because WAVE05 should not alter CaveScene |

## Code preserved

| File | Reason |
|---|---|
| `Assets/_Game/Scripts/Farm/FarmPlot.cs` | Reuses FarmPlot runtime, fixes item-to-seed resolution, keeps harvest on real InventoryManager path |
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Provides safe Unity Editor API path for regenerating FarmScene plot/foundation wiring |

## Current decision

```text
WAVE_INTEGRATION_04_STATUS: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED
WAVE_INTEGRATION_05_STATUS: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED
Can start WAVE_INTEGRATION_06: NO
Human Play Mode validation needed: YES
Scene wiring needed: YES, via Unity Editor/API only
```

## Required next action

Do not edit `.unity` files by text patch.

First validate that the restored WAVE03 FarmScene opens cleanly.

Then reapply WAVE04 foundation zones and WAVE05 crop smoke wiring through Unity Editor/Inspector, `CreateMvpFarmScene`, or a safe Editor utility that modifies the scene via Unity serialization APIs.
