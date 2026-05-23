# REF — CAVE VISUAL FIXES

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `docs/specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md`

---

# SPEC: Cave Visual Fixes (FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME + FIX_CAMERA_FOLLOW)

**Status**: Implementado Completo  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant Fixes**: FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME_v1.0 + FIX_CAVE_CAMERA_FOLLOW_AND_VISIBLE_ENEMIES_v1.0 + FIX_GLOBAL_CAMERA_FOLLOW_MVP_v1.0 + FIX_CAVE_EXITS_AND_SPARSE_RESOURCES_v1.0

---

## Summary

Visual materialization of procedural caves, camera smooth follow, and enemy visibility with fallback sprites.

## Scope

- ✅ CaveRuntimeMaterializer (floors, walls, exits visually)
- ✅ CameraFollow2D (smooth follow on all scenes)
- ✅ Enemy spawning with fallback sprite color
- ✅ Exit portal materialization
- ✅ Resource node visual representation
- ✅ Deterministic spawn order (by distance)

## Architecture

### Materialization
- **Floors/Walls**: Tiles from prefab or fallback GameObject
- **Exits**: Portal GameObjects with interactive components
- **Resources**: ResourceNode GameObjects with visual feedback
- **Enemies**: Spawned with sprite or fallback color

### Camera System
- **CameraFollow2D**: Smooth damped follow, rebindable target
- **RebindTarget()**: Set new follow target (e.g., on level load)
- **SnapToTarget()**: Immediate teleport to target (level change)
- **Consistency**: Used in Farm, Town, and Cave scenes

### Fallbacks
- **Sprite Missing**: Use builtin UI sprite (editor-only)
- **Tileset Prefab Missing**: Create plain colored GameObject
- **Enemy Sprite Missing**: Use dark red color (0.5, 0.1, 0.1)
- **Database Empty**: Create dummy data

### Spawn Order
- **Deterministic**: Enemies ordered by distance to entrance
- **Visual Feedback**: Closer enemies more visible
- **Consistency**: Same order on replay (snapshot)

## Key Files

- `Assets/_Game/Scripts/Camera/CameraFollow2D.cs` — Camera following
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` — Visuals
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawner.cs` — Enemy visuals
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` — Setup
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` — Setup
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs` — Setup

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Floors visible in cave | ✅ |
| 2 | Walls visible in cave | ✅ |
| 3 | Exits visible and interactive | ✅ |
| 4 | Resources visible in cave | ✅ |
| 5 | Enemies visible with sprite/fallback | ✅ |
| 6 | Camera follows player smoothly | ✅ |
| 7 | Camera snaps on scene change | ✅ |
| 8 | Farm/Town camera works | ✅ |
| 9 | Enemy spawn order deterministic | ✅ |
| 10 | No missing reference errors | ✅ |

## Pending

- Sprite polish (final art)
- Animation on enemy spawn
- Tile animation (floor variation)
- Camera damping tuning

## Next Steps

Refer to Cave procedural runtime and boss gates.



