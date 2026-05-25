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
- [ ] Integrate CaveGeneratedLevel with VisitedLevelSnapshot
- [ ] Store enemy spawn plan in snapshot
- [ ] Store fishing spot placement in snapshot
- [ ] Store resource node state in snapshot
- [ ] Load level from snapshot without regeneration
- [ ] Validate layout hash matches snapshot
- [ ] Handle snapshot cache within same run

### Phase 10: Save/Load - Cave Runtime State

#### Save Side (SaveManager Integration)
- [ ] Create CaveRuntimeStateSaveDTO (run seed, current level, snapshots map, boss defeat states, checkpoint progress)
- [ ] Implement IDataTransfer for CaveRuntimeStateSaveDTO
- [ ] Serialize enemy spawn plans to DTOs (no Unity refs)
- [ ] Serialize boss defeat states with unique rewards claimed
- [ ] Serialize checkpoint unlock levels
- [ ] Save to persistent data path

#### Load Side (SaveManager Integration)
- [ ] Deserialize CaveRuntimeStateSaveDTO
- [ ] Restore CaveRunManager.State
- [ ] Restore BossDefeatStates with unique rewards
- [ ] Restore CheckpointUnlockedLevels
- [ ] Restore current run seed
- [ ] Restore enemy spawn plan for current level

### Phase 11: Checkpoint Menu UI & Selection

#### Checkpoint Portal Menu
- [ ] Create CaveCheckpointSelectionUI (side menu)
- [ ] List unlocked checkpoints with level/biome name
- [ ] Show locked checkpoints (greyed out)
- [ ] Teleport on selection
- [ ] Close menu on teleport or back button
- [ ] Subscribe to CaveCheckpointPortalOpenedEvent

#### Teleport Logic
- [ ] CanProgressBeyondGate check before teleport
- [ ] Load snapshot or generate level at destination
- [ ] Spawn player at entrance
- [ ] Publish CaveCheckpointTeleportCompletedEvent
- [ ] Handle teleport failures gracefully

### Phase 12: Final Testing & Integration

#### Play Mode Testing
- [ ] Test biome transitions across all 8 biomes
- [ ] Test boss gate blocking at levels 15, 30, 45, 60, 75, 90
- [ ] Test checkpoint unlock on boss defeat
- [ ] Test teleport between checkpoints (same run)
- [ ] Test enemy respawn after 2 in-game days
- [ ] Test layout hash consistency on level revisit
- [ ] Test confinement (no spawns in walls)
- [ ] Test save/load and restore state
- [ ] Test snapshot/replay (no layout changes)

#### Non-Regression
- [ ] Run tools/docs/validate_docs.ps1
- [ ] Run tools/unity/RunUnityCompileValidation.ps1
- [ ] Run tools/unity/ScanUnityLogs.ps1
- [ ] No regressions in SPEC 05-13 systems

### Phase 13: Polish & Refinement

- [ ] Cave biome visual polish (lighting, atmosphere)
- [ ] Checkpoint portal visual clarity
- [ ] Boss gate visual indicator (locked/available/completed)
- [ ] Enemy respawn visual feedback
- [ ] Teleport transition effect
- [ ] Debug skip validation (respects gates)
- [ ] Performance optimization for large level snapshots
- [ ] Documentation of cave runtime flow

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
