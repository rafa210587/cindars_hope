# WAVE INTEGRATION 03 - Human Unity Wiring Instructions

## Target scene

`Assets/_Game/Scenes/FarmScene.unity`

## Current structural wiring

The scene already contains the required baseline wiring:

| Object | Components | Required references |
|---|---|---|
| `Player` | `PlayerController`, `Rigidbody2D`, `BoxCollider2D`, `SpriteRenderer`, `InteractionSystem` | `PlayerController._rigidbody` assigned to the same `Rigidbody2D`; `PlayerDataSO` assigned |
| `Main Camera` | `Camera`, `CameraFollow2D` | `CameraFollow2D._target` assigned to `Player` transform |
| `SpawnPoints` | `SceneSpawnInstaller` | `_playerTransform` assigned to `Player`; `_defaultSpawnId` is `farm_default` |
| `Spawn_farm_default` | `SceneSpawnPoint` | `_spawnId` is `farm_default` |
| `Bounds` | four child boundary objects with `BoxCollider2D` | Top/bottom/left/right boundary colliders present |

## Step-by-step validation

1. Open `Assets/_Game/Scenes/FarmScene.unity`.
2. Select `Player`.
3. Confirm `PlayerController`, `Rigidbody2D`, and `BoxCollider2D` are enabled.
4. Confirm `Rigidbody2D.gravityScale` is `0`.
5. Select `Main Camera`.
6. Confirm `CameraFollow2D._target` points to `Player`.
7. Select `SpawnPoints`.
8. Confirm `SceneSpawnInstaller._playerTransform` points to `Player`.
9. Confirm `_defaultSpawnId` is `farm_default`.
10. Press Play.
11. Move with WASD and arrow keys.
12. Confirm the camera snaps/follows the player.
13. Confirm the player does not leave the basic `Bounds` area.
14. Stop Play and confirm no scene changes are left dirty unexpectedly.

## Validation checklist

Use `docs/validation/WAVE_INTEGRATION_03_HUMAN_PLAYMODE_CHECKLIST.md`.
