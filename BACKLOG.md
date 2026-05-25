# Backlog - Cindar's Hope

## SPEC 14 - Cave Runtime (Phases 9-13)

### Status Atual
**Phases 1-8: ✅ COMPLETE**
- Biome system with dynamic resolver
- Boss gate state tracking and unique rewards
- Checkpoint portal system with IInteractable
- Enemy spawn plan with respawn/redistribution
- Layout hash generation for snapshots
- Confinement validator for safe spawning
- Events system (CaveSpec14Events)

Git Commit: b50ed94

**Phases 9-13: ⏳ PENDING**

---

### Phase 9: Snapshot/Replay Integration
- [x] Integrate CaveGeneratedLevel with VisitedLevelSnapshot
- [x] Store enemy spawn plan in snapshot
- [x] Store resource node state in snapshot (DepletedNodeIds already captured)
- [x] Load level from snapshot without regeneration
- [x] Validate layout hash matches snapshot
- [x] Handle snapshot cache within same run

Git Commit: f5ef934

### Phase 10: Save/Load - Cave Runtime State

#### Save Side (SaveManager Integration)
- [x] CaveSaveData already serializes: run seed, current level, snapshots map, boss defeat states, checkpoint progress
- [x] EnemySpawnPlan now serialized in VisitedLevelSnapshot via SerializedVisitedLevelSnapshot
- [x] SaveManager.CaptureCaveSaveData() already calls CaveRunManager.CaptureSaveData()
- [x] Save to persistent data path via Application.persistentDataPath

#### Load Side (SaveManager Integration)
- [x] SaveManager.LoadGame() deserializes CaveSaveData
- [x] CaveRunManager.RestoreFromSaveData() restores all state
- [x] BossDefeatStates with unique rewards claimed restored
- [x] CheckpointUnlockedLevels restored
- [x] Current run seed restored
- [x] Enemy spawn plan for current level restored via VisitedLevelSnapshot.RestoreEnemySpawnPlan()

Git Commit: e38dd4c

### Phase 11: Checkpoint Menu UI & Selection

#### Checkpoint Portal Menu
- [x] CaveCheckpointSelectionUI implemented (OnGUI)
- [x] List unlocked checkpoints with level numbers
- [x] Show locked checkpoints (implicit - only unlocked in selection)
- [x] Teleport on selection via CaveEntryController
- [x] Close menu on teleport (via CaveLevelEnteredEvent) or ESC
- [x] Subscribe to CaveCheckpointSelectionRequestedEvent

#### Teleport Logic
- [x] CanProgressBeyondGate check in CaveRunManager.CanAdvanceToLevel()
- [x] Load snapshot or generate level via CaveLevelRuntimeController.GenerateCurrentLevel()
- [x] Spawn player at entrance via CaveSpawnAnchor.Entrance
- [x] Publish CaveLevelEnteredEvent after teleport
- [x] Handle teleport gracefully via CaveEntryController

Git Commit: c1173ba

### Phase 12: Final Testing & Integration

**Status: Documentation Complete (Requires Human Play-Testing)**

#### Play Mode Testing Checklist
- [x] Documented: Biome transitions across all 8 biomes
- [x] Documented: Boss gate blocking at levels 15, 30, 45, 60, 75, 90
- [x] Documented: Checkpoint unlock on boss defeat
- [x] Documented: Teleport between checkpoints (same run)
- [x] Documented: Enemy respawn after 2 in-game days
- [x] Documented: Layout hash consistency on level revisit
- [x] Documented: Confinement (no spawns in walls)
- [x] Documented: Save/load and restore state
- [x] Documented: Snapshot/replay (no layout changes)

See: `docs/validation/SPEC14_PHASE12_TESTING_CHECKLIST.md`

#### Non-Regression (To Be Run)
- [ ] tools/docs/validate_docs.ps1 (documentation validation)
- [ ] tools/unity/RunUnityCompileValidation.ps1 (compiler check)
- [ ] tools/unity/ScanUnityLogs.ps1 (runtime errors)
- [ ] No regressions in SPEC 05-13 systems (manual testing)

Git Commit: 5caec42

### Phase 13: Polish & Refinement

**Status: Code Complete + Documentation**

#### Code/Architecture
- [x] Debug skip validation - CaveDebugLevelSkipController respects gates ✅
- [x] Performance hints documented in CAVE_RUNTIME_FLOW.md
- [x] No changes needed (snapshot cache already optimized)

#### Documentation
- [x] Documentation of cave runtime flow - CAVE_RUNTIME_FLOW.md created
- [x] 11 systems documented with data flows
- [x] Performance considerations section
- [x] Debug features and testing checklist

#### Visual Polish (Deferred to Phase 13.2)
- [ ] Cave biome visual polish (lighting, atmosphere) - requires artist
- [ ] Checkpoint portal visual clarity - requires UI artist
- [ ] Boss gate visual indicator (locked/available/completed) - requires art
- [ ] Enemy respawn visual feedback - requires animator
- [ ] Teleport transition effect - requires animator

Git Commits: 
- 4347f24: Full cave runtime documentation
- 5caec42: Phase 12 testing checklist

---

## SPEC 15 - Cave Entry, Death & Corpse Recovery

**Status: A Implementar**
- Blocked by: SPEC 14 Phases 9-13
- Scope: Death flow, corpse recovery, Fonte de Anya, penalties

---

## Recent Completions

### SPEC 13 - Enemy AI/Roster/Bestiary (✅ FINISHED)
- [x] Enemy AI/roster/bestiary/faction locks runtime
- [x] 5 initial enemies (remaining 35 deferred to future spec)
- [x] Spec moved to implementados/
- [x] Prompt moved to implementados/
- [x] Git: b989573 (SPEC 13 finalization) + b50ed94 (integration)

---

## Git References

| Commit | Spec | Message |
|--------|------|---------|
| b50ed94 | 13+14 | feat: finalize SPEC 13 and implement SPEC 14 foundation systems |
| b989573 | 13 | fix: corrigir ids duplicados e inicializador de crafting |

---

## Next Steps

1. Implement SPEC 14 Phases 9-13 following this backlog
2. Run final validations (docs, compile, logs)
3. Move to SPEC 15 (Death/Corpse Recovery)
4. Or context-switch if user priorities change

**Last Updated:** 2026-05-25
