# REF — CAVE PROCEDURAL RUNTIME PR141 153

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `.specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md`

---

# SPEC: Cave Procedural Runtime (PR-141 to PR-153)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-141 to PR-153

---

## Summary

Procedural cave generation engine, runtime materialization of floors/walls/exits, resource nodes, and save/load persistence.

## Scope

- ✅ CaveGenerator (BSP room-based generation)
- ✅ CaveGenerationConfig (tunable parameters)
- ✅ CaveRuntimeMaterializer (visuals + gameobjects)
- ✅ ResourceNode runtime component
- ✅ CaveExitPortal navigation
- ✅ CaveRunManager seed lifecycle
- ✅ CaveSaveData persistence
- ✅ Level snapshot capture/restore
- ⚠️ Enemy spawn per layout
- ⚠️ Loot tables and XP scaling
- ⚠️ Daily refresh for RespawnsDaily nodes
- ⚠️ KO regeneration flow

## Architecture

### Generation Pipeline
1. **CaveGenerator**: Pure procedural generation using BSP subdivision
2. **CaveGeneratedLevel**: DTO with walkable tiles, enemies, resources, exits
3. **LayoutHash**: Deterministic signature for replay validation
4. **RNG Seeding**: CaveWorldSeed + CaveRunSeed + level number

### Materialization
- **CaveRuntimeMaterializer**: Creates GameObjects from generated data
- **Floor/Walls**: Tiles with sprites or fallback colors
- **Exits**: Portal GameObjects with navigation
- **Resource Nodes**: Interactive resource objects
- **Enemies**: Spawned per encounter spec

### Runtime Components
- **CaveRunManager**: Seed tracking, checkpoint management
- **CaveLevelRuntimeController**: Level generation, capture, restore
- **ResourceNode**: Depletable resources requiring tool/tier
- **CaveExitPortal**: Navigation between levels

### Persistence
- **VisitedLevelSnapshot**: DTO capturing level state (layout, enemies, resources)
- **Snapshot Registry**: Per-run memory of visited levels
- **Deterministic Replay**: Backtrack restores identical snapshot
- **Save/Load**: Snapshots serialized in CaveSaveData

## Key Files

- `Assets/_Game/Scripts/Cave/Generation/CaveGenerator.cs` — Procedural generation
- `Assets/_Game/Scripts/Cave/Data/CaveGenerationConfigSO.cs` — Config
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` — Visuals
- `Assets/_Game/Scripts/Cave/Runtime/CaveLevelRuntimeController.cs` — Level lifecycle
- `Assets/_Game/Scripts/Cave/Runtime/ResourceNode.cs` — Resource interaction
- `Assets/_Game/Scripts/Cave/Runtime/CaveExitPortal.cs` — Navigation
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` — Seed/checkpoint management
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` — Snapshot DTO

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | CaveGenerator produces valid layouts | ✅ |
| 2 | CaveRuntimeMaterializer creates visuals | ✅ |
| 3 | Forward/back exits navigate correctly | ✅ |
| 4 | ResourceNodes spawn on level | ✅ |
| 5 | ResourceNodes deplete correctly | ✅ |
| 6 | Snapshots capture level state | ✅ |
| 7 | Backtrack restores snapshot identically | ✅ |
| 8 | Save/load preserves snapshots | ✅ |
| 9 | Multiple levels proceed forward | ✅ |
| 10 | KO resets run and clears snapshots | ✅ |

## Pending

- Enemy spawn per biome/encounter
- Loot tables for resources
- XP rewards and scaling
- Daily refresh for respawning nodes
- KO selection UI
- Boss gates (implemented in PR-193-202)

## Next Steps

Refer to Cave fixes (FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME) and boss gates (PR-193-202).



