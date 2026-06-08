# WAVE_INTEGRATION_05 — Scene Corruption Hotfix Report

Date: 2026-06-08
Status: HOTFIX_APPLIED_CODE_READY_SCENE_REVERTED

## Summary

Unity could not open `Assets/_Game/Scenes/FarmScene.unity` after WAVE_INTEGRATION_05 scene wiring. The scene changes were reverted to the last validated WAVE_INTEGRATION_04 LFS pointers. WAVE05 code changes were preserved.

## Error class

```text
SCENE_YAML_CORRUPTION_FROM_DIRECT_TEXT_PATCH
```

Unity reported a parser failure in `FarmScene.unity`, broken local file references, and dangling components. This is treated as scene serialization corruption, not as the primary C# logic bug.

## Scenes restored

| File | Restored source |
|---|---|
| `Assets/_Game/Scenes/FarmScene.unity` | `a7418a03b016fb1f3f2c21de1f59881ba8ad6117` |
| `Assets/_Game/Scenes/TownScene.unity` | `a7418a03b016fb1f3f2c21de1f59881ba8ad6117` |
| `Assets/_Game/Scenes/CaveScene.unity` | `a7418a03b016fb1f3f2c21de1f59881ba8ad6117` |

## Code preserved

| File | Reason |
|---|---|
| `Assets/_Game/Scripts/Farm/FarmPlot.cs` | Reuses FarmPlot runtime, fixes item-to-seed resolution, keeps harvest on real InventoryManager path |
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Provides safe Unity Editor API path for regenerating FarmScene plot wiring |

## Current decision

```text
WAVE_INTEGRATION_05_STATUS: BUILD_VALIDATED_CODE_READY_SCENE_REVERTED
Can start WAVE_INTEGRATION_06: NO
Human Play Mode validation needed: YES
Scene wiring needed: YES, via Unity Editor/API only
```

## Required next action

Do not edit `.unity` files by text patch. Reapply WAVE05 scene wiring through Unity Editor/Inspector, `CreateMvpFarmScene`, or a safe Editor utility that modifies the scene via Unity serialization APIs.
