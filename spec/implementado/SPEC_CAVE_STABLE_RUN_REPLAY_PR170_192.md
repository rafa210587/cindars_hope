# SPEC: Cave Stable Run & Replay (PR-170 to PR-192)

**Status**: Implementado Completo  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-170 to PR-192

---

## Summary

Deterministic cave generation with snapshot replay: revisit levels within same run to restore identical layout, enemies, and resources. KO resets run but preserves checkpoints.

## Scope

- ✅ LayoutHash computation for generation validation
- ✅ VisitedLevelSnapshot DTO (layout, enemies, resources)
- ✅ Snapshot capture after level materialization
- ✅ Snapshot restore on backtrack (identical replay)
- ✅ CaveRunSeed lifecycle (persists during run, resets on KO)
- ✅ CaveWorldSeed (persists indefinitely)
- ✅ Checkpoint preservation on KO
- ✅ Daily refresh for RespawnsDaily=true nodes
- ✅ Save/load persistence of snapshots

## Architecture

### Seeding Strategy
- **CaveWorldSeed**: Long-lived seed, defines world "identity"
- **CaveRunSeed**: Per-run seed, regenerates on KO
- **Combination**: CaveWorldSeed + CaveRunSeed + level number determines level layout

### Snapshot Capture/Restore
1. **Generate Level**: Create layout using current seeds
2. **Materialize**: Create GameObjects (floors, walls, exits, resources, enemies)
3. **Capture**: Record VisitedLevelSnapshot with exact state
4. **Store**: Keep in CaveRuntimeState.VisitedLevelSnapshots
5. **Backtrack**: Load snapshot, restore identical layout
6. **Forward**: Generate new level if snapshot missing

### KO Flow
- **Player Defeated**: Health ≤ 0
- **Generate New RunSeed**: CaveRunSeed = new seed
- **Clear Snapshots**: Start fresh within same CheckpointLevel
- **Preserve Checkpoints**: UnlockedCheckpoints unchanged
- **Restore Position**: Teleport to checkpoint spawn point

### Daily Refresh
- **RespawnsDaily=true**: Nodes reset each new game day
- **RefreshForNewDay()**: Decrement usage count, reset if threshold met
- **Persistence**: Daily state tracked per snapshot

## Key Files

- `Assets/_Game/Scripts/Cave/Generation/CaveGenerator.cs` — Generation with seed
- `Assets/_Game/Scripts/Cave/Runtime/CaveGeneratedLevel.cs` — DTO with LayoutHash
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` — Snapshot contracts
- `Assets/_Game/Scripts/Cave/Runtime/CaveLevelRuntimeController.cs` — Capture/restore
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` — Seed/KO lifecycle
- `Assets/_Game/Scripts/Cave/Runtime/ResourceNode.cs` — Daily refresh logic
- `Assets/_Game/Scripts/Save/CaveSaveData.cs` — Snapshot serialization

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Same layout regenerated within run | ✅ |
| 2 | LayoutHash matches on replay | ✅ |
| 3 | Snapshot captures all level data | ✅ |
| 4 | Backtrack restores identical snapshot | ✅ |
| 5 | Forward generates new level | ✅ |
| 6 | KO resets CaveRunSeed | ✅ |
| 7 | KO clears snapshots | ✅ |
| 8 | KO preserves checkpoints | ✅ |
| 9 | Daily refresh works | ✅ |
| 10 | Save/load round-trips snapshots | ✅ |

## Pending

- Full Play Mode validation (all test cases)
- Edge cases (KO at deep levels, cross-day scenarios)
- Performance validation (snapshot memory)

## Next Steps

Refer to Cave boss gates and checkpoints (PR-193-202).
