# WAVE INTEGRATION 04 - FarmScene Layout Decision

## Status

BUILD_VALIDATED_SCENE_WIRED

## Target FarmScene

| Field | Value |
|---|---|
| TargetScene | FarmScene |
| TargetScenePath | `Assets/_Game/Scenes/FarmScene.unity` |
| Decision | USE_EXISTING_FARMSCENE |
| Source | WAVE_INTEGRATION_03 player/camera/movement report |

## Scene file modification record

| Field | Value |
|---|---|
| Scene file to modify | `Assets/_Game/Scenes/FarmScene.unity` |
| Reason | WAVE_INTEGRATION_04 is the first authorized visual FarmScene rebuild foundation spec. |
| Expected changes | Add a `FarmSceneFoundationZones` root with visual zone markers for future gameplay hookups. Preserve existing player, camera, spawns, farm plots, trees, lake, portals, bounds, and runtime references. |
| Backup/restore strategy | Git diff and isolated commit. Revert only this commit/file if Unity visual inspection finds scene serialization damage. Static YAML checks also verify no duplicate fileIDs and expected marker count. |

## Strategy

REUSE_EXISTING_FARMSCENE_WITH_FOUNDATION_ZONE_MARKERS

The scene already had a playable baseline from prior waves:
- Player, movement, camera follow, spawns, and bounds from WAVE_INTEGRATION_03.
- Existing `FarmPlots` 3x3 crop area.
- Existing lake/fishing spot and lake edge triggers.
- Existing tree/resource area.
- Existing Town and Cave portals.

The missing part for this spec was a clear foundation layer for future hookups. A new lightweight `FarmSceneZoneMarker` component and visual marker layer were added instead of implementing gameplay.

## Preservation rules

| Area | Decision |
|---|---|
| Player/camera | Preserve existing objects and serialized references. |
| Gameplay systems | Do not implement new crop/resource/shipping gameplay. |
| Existing farm runtime | Reuse existing `FarmPlot`, `FishingSpot`, `TreeNode`, `ScenePortal`, and `SceneSpawnInstaller` objects. |
| Scene generator | Update `CreateMvpFarmScene` so regenerated FarmScene keeps the same foundation markers. |
| Other scenes | Do not modify. |
| Packages/ProjectSettings | Do not modify. |
