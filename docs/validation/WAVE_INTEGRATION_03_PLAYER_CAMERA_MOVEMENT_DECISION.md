# WAVE INTEGRATION 03 - Player, Camera e Movement Decision

## Status

VALIDATED

## Target scene

| Field | Value |
|---|---|
| TargetScene | FarmScene |
| TargetScenePath | `Assets/_Game/Scenes/FarmScene.unity` |
| Classification | ACTIVE_TARGET |
| Source decision | `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE.md` |
| Scene modification policy | No direct scene/prefab edit in this spec; existing scene wiring is reused. |

## Player strategy

REUSE_EXISTING_PLAYER

Evidence:
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `Assets/_Game/Scenes/FarmScene.unity` contains `CindarsHope.Player.PlayerController` on `Player`.
- The scene assigns `PlayerDataSO`, `Rigidbody2D`, and `PlayerNeedsBalanceSO`.

## Movement strategy

REUSE_EXISTING_MOVEMENT

Evidence:
- `PlayerController` reads WASD/arrow keyboard fallback when the Input System define is not enabled.
- `PlayerController` uses `Rigidbody2D.MovePosition` in `FixedUpdate`.
- `FarmScene` has `Rigidbody2D` on `Player` with gravity scale `0` and rotation constraints.

## Camera strategy

REUSE_EXISTING_CAMERA

Evidence:
- `Assets/_Game/Scripts/Camera/CameraFollow2D.cs`
- `FarmScene` `Main Camera` has `CameraFollow2D` with `_target` assigned to the `Player` transform.
- Camera is orthographic and uses snap-on-start follow behavior.

## Spawn strategy

REUSE_EXISTING_SCENE_SPAWN_INSTALLER

Evidence:
- `Assets/_Game/Scripts/SceneManagement/SceneSpawnPoint.cs`
- `Assets/_Game/Scripts/SceneManagement/SceneSpawnInstaller.cs`
- `FarmScene` has `Spawn_farm_default`, `Spawn_farm_from_town`, and `Spawn_farm_from_cave`.
- `FarmScene` `SceneSpawnInstaller` references the player transform, all three spawn points, and `_defaultSpawnId: farm_default`.

## Scale/sorting strategy

Use `FarmScaleContract` as the validation reference:
- Tile size: `32px`
- Player visual target: `32x48px`
- Player footbox target: bottom-centered `16px` height
- Sorting method: `Y_Foot`
- Camera target range: 20-24 tiles wide, 12-14 tiles tall

Current structural scene evidence:
- Player collider exists and is non-trigger.
- Scene has `Bounds` object with top/bottom/left/right colliders.
- Camera orthographic size is `8.5`; this must be visually reviewed in Play Mode against the scale contract.

## Scene modification policy

No `.unity`, `.prefab`, `.asset`, `Packages`, or `ProjectSettings` changes were required. The baseline is already present in `FarmScene`, so this spec documents and validates the existing wiring instead of creating parallel scripts or touching scene YAML.

## Human Unity actions required

| Action | Reason | Required before |
|---|---|---|
| Open `Assets/_Game/Scenes/FarmScene.unity`. | Confirm scene opens cleanly in the current Unity editor. | WAVE_INTEGRATION_04 |
| Press Play and validate player/camera movement. | Play Mode visual behavior cannot be proven by C# build/static YAML checks. | WAVE_INTEGRATION_04 |
| Confirm no red Console errors. | Required visual integration gate. | WAVE_INTEGRATION_04 |
| Confirm player remains inside bounds and visible. | Validate collision and framing. | WAVE_INTEGRATION_04 |
