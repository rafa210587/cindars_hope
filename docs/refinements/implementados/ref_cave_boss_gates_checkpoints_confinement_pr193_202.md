# REF — CAVE BOSS GATES CHECKPOINTS CONFINEMENT PR193 202

> Origem: $src`n> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md`n
---

# SPEC: Cave Boss Gates, Checkpoints & Confinement (PR-193 to PR-202)

**Status**: Implementado Completo (código)  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-193 to PR-202

---

## Summary

Boss gate system blocking progression until boss defeated, checkpoint selection UI, persistent boss defeat tracking, and player path confinement to walkable tiles.

## Scope

- ✅ CaveBossGateDataSO configuration
- ✅ CaveBossGateRegistrySO lookup by level
- ✅ CaveBossDefeatState persistence
- ✅ CaveBossSpawner with visual differentiation
- ✅ CaveBossDeathReporter event integration
- ✅ CaveCheckpointSelectionUI MVP debug UI
- ✅ CaveEntryController checkpoint entry flow
- ✅ CanAdvanceToLevel() gate blocking logic
- ✅ CheckBossGate() validation
- ✅ CavePlayerPathConfinement with per-axis rollback
- ✅ DebugHud boss gate status display
- ✅ CaveBossGateValidator

## Architecture

### Boss Gate System
- **CaveBossGateDataSO**: Gate ID, cave level, boss enemy ID, checkpoint to unlock
- **CaveBossGateRegistrySO**: Registry with level → gate lookup
- **CaveBossDefeatState**: Serializable record of boss defeat
- **BossDefeatStates**: Dictionary in CaveRuntimeState, persisted in CaveSaveData

### Boss Spawning
- **CaveBossSpawner**: Spawns boss at level entry
- **Strategy Selection**: Adjacent → Diagonal → Radius → Fallback
- **Visual**: Orange color (1.0, 0.5, 0.0)
- **Validation**: Walkable, not exit/entrance, not on player

### Boss Death Flow
1. **Player defeats boss** (health ≤ 0)
2. **EnemyKilledEvent** published
3. **CaveBossDeathReporter** detects matching enemy
4. **MarkBossAsDefeated()** marks gate defeated
5. **UnlockCheckpoint()** unlocks next checkpoint
6. **CaveBossDefeatedEvent** published
7. **HUD updates** showing gate open

### Checkpoint System
- **Official Checkpoints**: 1, 15, 30, 45, 60, 75, 90
- **Selection UI**: Arrow keys (↑↓), Enter to confirm
- **Entry**: CaveEntryController awaits selection, teleports to checkpoint
- **Persistence**: UnlockedCheckpoints in save/load

### Path Confinement
- **System**: Validates player position each frame
- **Validation**: Must be in WalkableTiles, within level bounds
- **Rollback Strategy**: 
  1. Try current position
  2. Try X-only rollback (last valid X, current Y)
  3. Try Y-only rollback (current X, last valid Y)
  4. Revert to lastValidPosition
- **Per-Axis**: Allows sliding along walls, not blocking diagonals
- **Tolerance**: Lateral samples disabled (0.005 half-width), vertical enabled (0.08)
- **Rate Limiting**: Log warnings max once per second

## Key Files

- `Assets/_Game/Scripts/Cave/Data/CaveBossGateDataSO.cs` — Gate config
- `Assets/_Game/Scripts/Cave/Data/CaveBossGateRegistrySO.cs` — Gate registry
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossDefeatState.cs` — Defeat DTO
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs` — Boss spawning
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossDeathReporter.cs` — Death detection
- `Assets/_Game/Scripts/Cave/Runtime/CaveCheckpointSelectionUI.cs` — Checkpoint UI
- `Assets/_Game/Scripts/Cave/Runtime/CaveEntryController.cs` — Entry flow
- `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs` — Confinement
- `Assets/_Game/Scripts/Cave/Validation/CaveBossGateValidator.cs` — Validation
- `Assets/_Game/Scripts/UI/DebugHud.cs` — HUD display
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` — Public query methods

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Boss gate blocks 15→16 progression | ✅ |
| 2 | Boss spawns near gate (adjacent preferred) | ✅ |
| 3 | Boss has visual differentiation (orange) | ✅ |
| 4 | Boss defeat detected correctly | ✅ |
| 5 | Checkpoint unlocked on boss defeat | ✅ |
| 6 | Checkpoint selection UI shows options | ✅ |
| 7 | Checkpoint entry teleports player | ✅ |
| 8 | Boss defeat persists save/load | ✅ |
| 9 | Player confined to walkable tiles | ✅ |
| 10 | Per-axis rollback works (no diagonal blocking) | ✅ |
| 11 | HUD shows current level gate status | ✅ |
| 12 | Debug P hotkey skips levels | ✅ |

## Pending

- Full Play Mode validation (29+ test cases)
- Boss visual sprite/animation
- Checkpoint UI polish (not debug)
- Wall distance tuning (0.005 lateral, 0.08 vertical)

## Next Steps

Validate in Play Mode. Refer to audit document for test procedure.


