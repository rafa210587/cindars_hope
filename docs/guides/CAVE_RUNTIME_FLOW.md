# Cave Runtime Flow - Complete Architecture

**Document:** SPEC 14 Complete Runtime Architecture
**Date:** 2026-05-25
**Scope:** Phases 1-13 Implementation

## Overview

The cave system in Cindar's Hope implements a **stable run with snapshot/replay** architecture. Players enter a 100-level procedural cave divided into 8 biomes. Once a level is visited, it's cached via snapshot and never regenerated within the same run.

## Key Systems

### 1. Cave Run Management (CaveRunManager)

**Responsibility:** Maintain run state across level transitions

**State Stored:**
- `CurrentCaveLevel`: Current level (1-100)
- `DeepestLayerReached`: Furthest level reached
- `CaveWorldSeed`: Fixed seed for world (usually "cindars_world_seed_001")
- `CaveRunSeed`: Per-run seed (changes on new run/player death)
- `UnlockedCheckpoints`: Set<int> of accessible checkpoints (1, 15, 30, 45, 60, 75, 90)
- `DepletedNodeIds`: Set<string> of harvested resources (persist across runs)
- `VisitedLevelSnapshots`: Dictionary<int, VisitedLevelSnapshot> (per-run cache)
- `BossDefeatStates`: Dictionary<string, CaveBossDefeatState> (which bosses beaten)

**Key Methods:**
```csharp
EnterLevel(int caveLevel)           // Navigate to level
GenerateNewRunSeed(string reason)   // New run (death/debug)
CanAdvanceToLevel(...)              // Check boss gate blocking
IsBossDefeated(string bossGateId)   // Query boss state
UnlockCheckpoint(int level)         // On boss defeat
CaptureSaveData()                   // For SaveManager
RestoreFromSaveData(CaveSaveData)   // Load game
```

### 2. Level Generation (CaveProceduralGenerator)

**Responsibility:** Generate fresh level layout if not in cache

**Inputs:**
- `CaveLevel`: Which level to generate (1-100)
- `CaveWorldSeed`: Deterministic seed
- `CaveRunSeed`: Per-run variation
- `BiomeId`: Biome-specific generation params (tile palette, enemy profiles, etc.)

**Output:**
- `CaveGeneratedLevel`: Non-serializable runtime object with:
  - Rooms, walkable/wall tiles
  - Enemy spawn points (CaveGenerationPoint[])
  - Resource spawn points
  - Layout hash (SHA256)

**Generation Steps:**
1. Generate rooms via `GenerateRooms()`
2. Connect rooms with corridors
3. Place entrance & exit
4. Place generation points (enemy/resource)
5. Carve walls
6. Compute layout hash

### 3. Snapshot & Replay (VisitedLevelSnapshot)

**Responsibility:** Cache generated levels to avoid regeneration

**Serialized Data:**
- Layout: dimensions, tiles, entrance/exit, rooms
- Spawn points: enemy and resource generation points
- Enemy spawn plan: `EnemySpawnPlanEntry[]` (who spawns where, respawn state)
- Resource state: `DepletedResourceNodeIds`
- Layout hash: SHA256 for validation

**Flow:**
1. **Generate:** `CaveLevelRuntimeController.GenerateCurrentLevel()`
   - Check `VisitedLevelSnapshots[level]`
   - If missing → generate fresh → capture snapshot → cache

2. **Load from Cache:** `CaveLevelRuntimeController.RestoreFromSnapshot()`
   - Reconstruct `CaveGeneratedLevel` from snapshot data
   - Validate layout hash matches
   - Restore enemy spawn plan state
   - No regeneration happens

### 4. Biome Resolution (CaveBiomeResolver)

**Responsibility:** Map cave level → biome

**Level Ranges:**
- Levels 1-10: Stone Cavern (biome_cave_earth)
- Levels 11-25: Forest (biome_cave_forest)
- Levels 26-40: Ice (biome_cave_ice)
- Levels 41-55: Fire (biome_cave_fire)
- Levels 56-70: Ruins (biome_cave_ruins)
- Levels 71-85: Abyss (biome_cave_abyss)
- Levels 86-99: Core (biome_cave_core)
- Level 100: Final (biome_cave_final)

**Method:**
```csharp
ResolveBiome(int caveLevel) → BiomeId
```

Uses `CaveBiomeRegistrySO` to look up biome data (enemy profiles, resource spawns, etc.)

### 5. Boss Gates (CaveBossGateService)

**Responsibility:** Block progression until boss defeated

**Gate Locations:**
- Level 15: Gate 1
- Level 30: Gate 2
- Level 45: Gate 3
- Level 60: Gate 4
- Level 75: Gate 5
- Level 90: Gate 6

**Flow:**
```
Player tries to advance level N → Check CanAdvanceToLevel()
  ├─ Is N past a boss gate level? 
  ├─ If yes: Is boss defeated? 
  │  ├─ Yes → Allow
  │  └─ No → Block + LogWarning
  └─ No → Allow
```

**State Tracking:**
- `CaveBossDefeatState` stores: `IsDefeated`, `UniqueRewardsClaimed[]`
- Boss defeated → `UnlockCheckpoint(gateLevel)`
- Unique rewards tracked per boss (can only claim once per run)

### 6. Enemy Spawn Planning (CaveEnemySpawnPlanService)

**Responsibility:** Track enemy instances for respawn/redistribution

**Data Structure:**
```csharp
EnemySpawnPlan {
  EnemyPlans: EnemySpawnPlanEntry[] {
    PlannedEnemyInstanceId: "enemy_0_1_2_3"
    EnemyId: "enemy_goblin_basic"
    CurrentAnchorId: "anchor_5_7"
    IsDefeated: false
    RespawnAvailableAtGameDay: -1  // After 2-day delay
    IsBoss: false
  }
  RedistributionState { RedistributionCount, Reason, SeedOffset }
  RespawnState { RespawnDelayGameDays = 2, LastEvaluationDay = -1 }
}
```

**Captured in Snapshot:**
- Each level's enemy plan serialized in `VisitedLevelSnapshot.EnemySpawnPlan`
- On level load, plan restored and re-engaged

### 7. Respawn Mechanics (CaveEnemyRespawnService)

**2-Day Delay Logic:**
```
Day 0: Defeat enemy → RespawnAvailableAtGameDay = 2
Day 1: Check respawn → Current day 1 < 2 → No respawn
Day 2: Check respawn → Current day 2 >= 2 → Respawn available
       → Call RespawnEnemy() → Spawn at original anchor
```

**Respawn Redistribution:**
- If spawn anchor becomes unreachable → `RedistributeEnemies()`
- New anchors selected deterministically
- Respawn count tracked

### 8. Checkpoint Portal System

**Interaction Flow:**
```
Player interacts with CaveCheckpointPortal
  ↓
Publish CaveCheckpointSelectionRequestedEvent
  ↓
CaveCheckpointSelectionUI shows menu (OnGUI)
  ↓
Player selects checkpoint level
  ↓
Publish CaveCheckpointSelectedEvent
  ↓
CaveEntryController.OnCheckpointSelected()
  ├─ EnterLevel(selectedLevel)
  ├─ SetSpawnAnchorForNextGeneration(Entrance)
  ├─ GenerateCurrentLevel()
  │   ├─ Check snapshot cache
  │   ├─ Load or generate
  │   └─ Materialize
  └─ Publish CaveLevelEnteredEvent
```

**Validation:**
- `CanInteract()` checks: no active modal
- `CanAdvanceToLevel()` blocks teleport if gate not passed
- Layout hash validation on load

### 9. Save/Load Integration (SaveManager ↔ CaveRunManager)

**Save Flow:**
```
SaveManager.SaveGame()
  ↓
CaptureCaveSaveData()
  ├─ CaveRunManager.CaptureSaveData()
  │   ├─ Current level, deepest reached
  │   ├─ Both seeds
  │   ├─ Unlocked checkpoints
  │   ├─ Depleted nodes
  │   ├─ All snapshots → SerializedVisitedLevelSnapshot[]
  │   └─ Boss defeat states
  └─ Serialize to JSON via JsonUtility
```

**Load Flow:**
```
SaveManager.LoadGame()
  ↓
Deserialize JSON → CaveSaveData
  ↓
CaveRunManager.RestoreFromSaveData()
  ├─ Restore current level, seeds
  ├─ Restore unlocked checkpoints
  ├─ Restore all snapshots (Dictionary)
  └─ Restore boss defeat states
```

**Snapshot Serialization:**
- `VisitedLevelSnapshot` → `SerializedVisitedLevelSnapshot` (no Unity refs)
- Includes: EnemySpawnPlan, layout hash, tile lists, resource state

### 10. Layout Hash Validation (CaveLayoutHashGenerator)

**Purpose:** Detect snapshot corruption or layout changes

**Algorithm:**
```csharp
GenerateHash(CaveGeneratedLevel level) {
  StringBuilder sb = new()
  sb.Append($"{level.CaveLevel}_{width}_{height}_{entrance}_{exit}")
  sb.Append("_rooms:" + rooms)
  sb.Append("_walkable:" + walkableTiles_sorted)
  return SHA256(sb.ToString()).ToString().SubString(0, 16)
}
```

**Validation:**
- On snapshot load: reconstruct hash and compare
- If mismatch → LogWarning + regenerate level
- If match → "Layout hash validated" in debug

### 11. Confinement Validation (CaveConfinementValidator)

**Purpose:** Prevent enemy spawns in walls

**Methods:**
```csharp
IsPositionSafe(Vector2Int pos, CaveGeneratedLevel level)
  ├─ Check if on walkable tile
  ├─ Check distance to walls (MinDistanceFromWall = 0.5f)
  └─ Return safe or unsafe

FindSafeSpawnPosition(Vector2Int preferred, CaveGeneratedLevel level)
  ├─ If preferred safe → return preferred
  ├─ Else → find nearest safe position
  └─ Fallback: return room center
```

## Data Flow Diagram

```
Player enters CaveScene
  ↓
CaveLevelRuntimeController.GenerateCurrentLevel()
  ├─ Load from snapshot cache?
  │  ├─ YES → RestoreFromSnapshot() → Materialize
  │  └─ NO → Generate fresh
  │     ├─ CaveProceduralGenerator.Generate()
  │     ├─ Materialize (tiles, objects)
  │     ├─ CaveLevelRuntimeController.CaptureSnapshot()
  │     ├─ CaveEnemySpawner.SpawnEnemiesForLevel()
  │     ├─ CaveEnemySpawnPlanService.CreatePlanForLevel()
  │     └─ CaveLevelRuntimeController.RegisterEnemySpawnPlan()
  └─ Publish CaveLevelEnteredEvent

Player progresses through levels
  ├─ Advances via ForwardExit → level N+1
  │  ├─ Check CanAdvanceToLevel(N, N+1)
  │  ├─ If blocked by gate → Cannot proceed
  │  └─ If clear → Load/generate N+1
  │
  ├─ Defeats boss → Checkpoint unlocks
  │  ├─ Publish CaveBossGateCompletedEvent
  │  ├─ CaveRunManager.MarkBossAsDefeated()
  │  ├─ CaveRunManager.UnlockCheckpoint(gateLevel)
  │  └─ Boss state persists in snapshots
  │
  └─ Uses checkpoint portal → Teleports
     ├─ Select from unlocked checkpoints
     ├─ Load snapshot or generate
     ├─ Spawn at entrance
     └─ Continue from there

Player defeats or dies
  ├─ Defeat: SaveManager.SaveGame()
  │  ├─ All snapshots persisted
  │  ├─ Boss states persisted
  │  ├─ Checkpoint list persisted
  │  └─ Can reload same run later
  │
  └─ Death: CaveRunManager.HandlePlayerDefeated()
     ├─ Generate new run seed
     ├─ Clear snapshots (restart fresh)
     ├─ Keep checkpoints unlocked
     └─ Start from level 1
```

## Performance Considerations

### Snapshot Memory
- **Per-level snapshot:** ~100-200 KB (tiles, spawn points, enemy plan)
- **100 levels:** ~10-20 MB in-memory cache (via CaveSnapshotCacheManager)
- **Optimization:** Compress snapshot data or implement LRU eviction if needed

### Generation Performance
- **Fresh level generation:** ~50-100ms (procedural with deterministic random)
- **Snapshot load:** ~10-20ms (deserialization + validation)
- **Caching benefit:** ~5x speedup on revisits

### Recommended Optimizations
1. **Lazy-load snapshots:** Only deserialize when needed (not all 100 at startup)
2. **Enemy plan streaming:** Only keep active enemies in memory
3. **Tile compression:** Use bit-packing for large tile grids
4. **Layout hash caching:** Cache hash with snapshot to avoid recompute

## Debug Features

### Debug Level Skip (Respects Gates)
```csharp
// Shift+R to regenerate current run
CaveLevelRuntimeController.RegenerateCurrentRunDebug()

// Can still be blocked by boss gates
// CaveRunManager.CanAdvanceToLevel() always checks
```

### Debug Logging
- Enable in CaveLevelRuntimeController: `_logGeneratedLayout = true`
- ASCII visualization of layout in console
- Snapshot validation messages
- Enemy spawn plan details

## Known Limitations & Future Work

1. **Fishing spots:** Not yet captured in snapshots (feature pending)
2. **Unique rewards:** Tracked but distribution logic in Phase 15
3. **Boss arena persistence:** Boss state resets on level reload (temporary)
4. **Visual feedback:** Checkpoint menu is basic OnGUI (Phase 13 polish pending)
5. **Performance:** Unoptimized for 100 levels (Phase 13 optimization pending)

## Testing Checklist (Phase 12)

See: `docs/validation/SPEC14_PHASE12_TESTING_CHECKLIST.md`

## Related Specifications

- **FASE9F Amendment:** Stable run rules (Phase 9-13 adherence)
- **SPEC 05-13:** All prerequisite systems (enemy AI, inventory, combat, etc.)
- **SPEC 15:** Death/Corpse Recovery (uses cave runtime as foundation)
