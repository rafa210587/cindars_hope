---
doc_type: game_rule
status: accepted
domain: cave-gameplay
source_adrs:
  - ADR-0005
source_documents:
  - docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md
  - .specs/a_implementar/closeout_mvp/SPEC_24_CAVE_RUNTIME_CHECKPOINTS_BOSS_GATES_CLOSEOUT.md
last_reviewed: 2026-06-01
---

# Cave Gameplay Rules

## Purpose

Defines cave procedural generation, stable run behavior, snapshot persistence, and level revisit mechanics.

---

## Canonical Rules

### Rule: Caves are Procedural by Run, Not by Entry

- **Rule:** Within the same `CaveRunSeed`, a `CaveLevel` visited before must preserve all generated content
- **Applies to:** CaveLevel scene generation, enemy placement, resource placement, boss/miniboss state
- **Must preserve:**
  - Layout/structure
  - Entrance and exit positions
  - Enemy composition, count, positions, IDs, types
  - Resource node composition, count, positions, IDs, types
  - Depletion state of nodes
  - Boss/miniboss state
- **Validation:** Snapshot hash must match previous visit
- **Source:** ADR-0005, FASE9F Amendment, SPEC_24

### Rule: Procedural Content Changes Only On

- **Rule:** Cave level content is only regenerated on:
  1. New game (new CaveWorldSeed)
  2. KO/death/defeat of player
  3. Explicit debug regeneration command
- **Applies to:** Level generation, enemy spawning, resource placement
- **Must NOT:** Regenerate on ForwardExit, BackExit, scene reload
- **Validation:** CaveRunSeed must not change on forward/backward exit
- **Source:** ADR-0005, FASE9F Amendment

### Rule: Enemy Count Range per Level per Run

- **Rule:** First visit to a CaveLevel in a run: generate 12-20 enemies (random)
- **Applies to:** Enemy composition, spawn counts
- **Must NOT:** Re-roll on revisit (load snapshot instead)
- **Validation:** Enemy count stable within run; only changes on new game/death/debug
- **Source:** FASE9F Amendment, SPEC_24

### Rule: Resource Node Range per Level per Run

- **Rule:** First visit to a CaveLevel in a run: generate 4-10 resource nodes (random)
- **Applies to:** Resource placement, depletion tracking
- **Must NOT:** Re-roll on revisit
- **Validation:** Node count stable within run
- **Source:** FASE9F Amendment, SPEC_24

### Rule: First Visit Snapshot Creation

- **Rule:** On first visit to a CaveLevel in a run:
  1. Generate layout (procedural)
  2. Generate enemy composition (procedural)
  3. Generate resource composition (procedural)
  4. Create `CaveVisitedLevelSnapshot` with LayoutHash, ContentHash
  5. Save snapshot to runtime state
  6. Save snapshot in game save when player saves
  7. Materialize scene from snapshot
- **Applies to:** Level generation, save/load
- **Validation:** Snapshot must be persisted in save data
- **Source:** FASE9F Amendment, SPEC_24

### Rule: Revisit Loading Snapshot

- **Rule:** On revisit to a CaveLevel in the same run:
  1. Load `CaveVisitedLevelSnapshot` from runtime state
  2. Do NOT regenerate layout, enemies, or resources
  3. Do NOT alter quantities, types, positions, composition
  4. Materialize scene from loaded snapshot
- **Applies to:** Level loading, revisit behavior
- **Validation:** Scene must be identical to first visit
- **Source:** FASE9F Amendment, SPEC_24

### Rule: Enemy Distribution on First Generation

- **Rule:** Enemy composition distribution on first visit:
  - 70-80%: EnemyLevel = CaveLevel (same level as cave level)
  - Remainder: Scaled based on difficulty and cave progression
- **Applies to:** Enemy generation algorithm
- **Must NOT:** Violate distribution on revisit (use snapshot)
- **Source:** FASE9F Amendment

### Rule: Exit Actions Never Change CaveRunSeed

- **Rule:** `ForwardExit` and `BackExit` must never alter `CaveRunSeed`
- **Applies to:** Exit triggers, seed management
- **Validation:** CaveRunSeed remains constant throughout a run
- **Source:** ADR-0005, FASE9F Amendment

---

## Open Questions

- What happens if a boss is defeated in a level, then the player revisits? (Current: state is preserved in snapshot, boss remains defeated)
- What constitutes "KO/death/defeat" that allows regeneration? (Current: player character KO, run reset, explicit debug command)

---

## Related ADRs

- [ADR-0005: Cave Stable Run and Replay](../decisions/ADR-0005-cave-stable-run-and-replay.md)
- [ADR-0006: Save Data Contracts Simple DTOs](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (snapshot persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (cave events if needed)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Implemented by: SPEC_24*  
*Migrated from: FASE9F Amendment*
