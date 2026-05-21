# FASE9F — Cave Replay System Contracts v1.0

## Overview

The Cave Replay System enables stable level state persistence, allowing players to:
- Backtrack through visited levels with identical layout/enemies/resources
- Experience consistent cave generation via deterministic seeding
- Recover checkpoint-locked progression after KO/defeat
- Refresh only `RespawnsDaily=true` resource nodes daily

---

## Core Concepts

### CaveWorldSeed
- Persistent seed for the entire save file
- Generated once at new game creation
- Used with `CaveRunSeed + CaveLevel` to deterministically generate level layout
- Preserved across save/load

### CaveRunSeed
- Per-run seed; changes when:
  - Player starts a new run (first entry to cave)
  - Player suffers KO/defeat in cave
- Preserved across save/load within the same run
- Regenerated on KO, clearing visited snapshots but preserving checkpoints

### VisitedLevelSnapshot
- DTO containing captured state of a generated level
- Captured after successful generation + materialization
- Stored in `CaveRuntimeState.VisitedLevelSnapshots[CaveLevel]`
- Persisted in `CaveSaveData.VisitedLevelSnapshots`
- Cleared on KO/defeat to force fresh generation on re-entry

### LayoutHash
- SHA256 hash of level layout (rooms, walls, entrance, exit, walkable tiles)
- Computed after generation to verify determinism
- Compared on snapshot restoration for validation

---

## Snapshot Lifecycle

### Generation → Capture
1. `CaveLevelRuntimeController.GenerateCurrentLevel()` checks for existing snapshot
2. If snapshot exists and is valid, restore from snapshot (skip generation)
3. If not, generate level using `CaveProceduralGenerator`
4. Compute `LayoutHash` after generation
5. Materialize level (create visual tiles, enemies, nodes)
6. Call `CaptureSnapshot()` to save level state
7. Publish `CaveLevelEnteredEvent`

### Backtrack → Restore
1. Player interacts with `CaveExitPortal.BackExit`
2. `CaveRunManager.EnterLevel(previousLevel)`
3. `CaveLevelRuntimeController.GenerateCurrentLevel()` checks snapshot
4. If valid snapshot exists, call `RestoreFromSnapshot()` instead of regenerating
5. Materialization occurs identically to fresh generation
6. Player enters level in previous state (same enemies, same resources, same layout)

### KO/Defeat → Reset
1. `PlayerManager.CurrentHP` reaches 0
2. `PlayerManager` publishes `HPChangedEvent` (0)
3. Cave listener (TBD: CaveLevelRuntimeController or dedicated handler) calls `CaveRunManager.HandlePlayerDefeated()`
4. `HandlePlayerDefeated()`:
   - Calls `GenerateNewRunSeed("PlayerDefeated")`
   - Clears `VisitedLevelSnapshots`
   - Clears `DepletedNodeIds`
   - Preserves `UnlockedCheckpoints`
   - Publishes `CavePlayerDefeatedEvent`
5. Player respawns/reloads, checkpoints still available
6. Next entry to cave uses new `CaveRunSeed` → different layout

### Daily Refresh
1. `TimeManager` publishes `DayStartedEvent`
2. `CaveLevelRuntimeController` listens and calls `RefreshDailyResourceNodes()`
3. For each `ResourceNode` where `nodeData.RespawnsDaily == true`:
   - If depleted, reset `_isDepleted = false` and `_hitsTaken = 0`
   - Remove from `CaveRunManager.State.DepletedNodeIds`
   - Republish node visually

---

## Contracts (Types)

### IVisitedLevelSnapshot
```csharp
public interface IVisitedLevelSnapshot
{
    int CaveLevel { get; }
    string SnapshotId { get; }
    bool IsValid();
}
```

### VisitedLevelSnapshot
```csharp
[Serializable]
public sealed class VisitedLevelSnapshot : IVisitedLevelSnapshot
{
    public int CaveLevel;
    public string SnapshotId;
    public string BiomeId;
    public string LayoutHash;
    public Vector2 EntrancePosition;
    public Vector2 ExitPosition;
    public List<SerializedEnemySpawn> EnemySpawns;
    public List<SerializedResourceNode> ResourceNodes;
    public List<string> DepletedResourceNodeIds;

    public bool IsValid() => CaveLevel > 0 && !string.IsNullOrWhiteSpace(SnapshotId);
}
```

### SerializedEnemySpawn
```csharp
[Serializable]
public sealed class SerializedEnemySpawn
{
    public string EnemyId;
    public Vector2 Position;
    public int Level;
}
```

### SerializedResourceNode
```csharp
[Serializable]
public sealed class SerializedResourceNode
{
    public string NodeInstanceId;
    public Vector2 Position;
    public string ResourceDataId;
}
```

---

## CaveSaveData Updates

```csharp
[Serializable]
public sealed class CaveSaveData
{
    // existing
    public int CurrentCaveLevel = 1;
    public int DeepestLayerReached = 1;
    public string CaveWorldSeed = string.Empty;
    public string CaveRunSeed = string.Empty;
    public List<int> UnlockedCheckpoints = new List<int>();
    public List<string> DepletedNodeIds = new List<string>();

    // new
    public List<SerializedVisitedLevelSnapshot> VisitedLevelSnapshots = new();

    // helpers
    public void PopulateSnapshots(Dictionary<int, VisitedLevelSnapshot> snapshots);
    public Dictionary<int, VisitedLevelSnapshot> RestoreSnapshots();
}
```

---

## Events

### CavePlayerDefeatedEvent
```csharp
public readonly struct CavePlayerDefeatedEvent
{
    public readonly int CaveLevel;
    public readonly DateTime DefeatedAt;
}
```

---

## Key Implementation Points

1. **Determinism**: Generator must produce identical layouts for same `(WorldSeed, RunSeed, CaveLevel)`
2. **No Unity Refs in Snapshots**: Only IDs, positions, enums, strings, bools
3. **Checkpoint Persistence**: KO clears snapshots but preserves unlocked checkpoints
4. **Daily Refresh Guard**: Only `RespawnsDaily=true` nodes are refreshed; others stay depleted until next run
5. **Snapshot Validation**: Always check `snapshot.IsValid()` before restore
6. **LayoutHash Mismatch**: Log warning if restored snapshot hash differs from freshly generated (indicates non-determinism)

---

## Testing Checklist

- [ ] New run generates level, captures snapshot, displays in DebugHud
- [ ] BackExit from level 2+ loads snapshot (same layout, same enemies at same positions)
- [ ] ForwardExit advances level
- [ ] ForwardExit blocked at level 15 (boss gate)
- [ ] KO in cave clears snapshots but preserves checkpoints
- [ ] Save/load preserves `CaveWorldSeed`, `CaveRunSeed`, snapshots, checkpoints
- [ ] New day refreshes only `RespawnsDaily=true` nodes
- [ ] Depleted node stays depleted until daily refresh if `RespawnsDaily=false`
- [ ] LayoutHash computed and stored for each snapshot
- [ ] DebugHud displays visited snapshots, checkpoint list, and current state

---

## Future Enhancements

- Snapshot diff visualization (changed enemies, depleted nodes vs. original)
- Checkpoint selection UI (choose which checkpoint to enter)
- Persistent miniboss state per run
- Boss defeat checkpoint unlock
- Snapshot integrity check on load (LayoutHash validation)
