---
doc_type: game_rule
status: accepted
domain: cave-gameplay
source_adrs:
  - ADR-0005
  - ADR-0016
  - ADR-0018
  - ADR-0019
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
- **Must preserve (composition):**
  - Layout/structure
  - Entrance and exit positions
  - Enemy composition, count, positions, IDs, types
  - Resource node composition, count, positions, IDs, types
  - Depletion state of nodes
  - Boss/miniboss state
- **Carve-out (ADR-0018):** "preserve" means **composition**. Per-instance enemy HP/death
  state caused by inter-monster conflict (a monster killing/wounding another) may differ on
  revisit — this is behavior-per-visit, persisted by the F13 enemy-HP-per-instance contract
  (outside the `LayoutHash`), exactly like a player kill. Composition (which enemies exist,
  count, positions, IDs, types) is never re-rolled.
- **Validation:** Snapshot hash must match previous visit (composition target)
- **Source:** ADR-0005, ADR-0018 (conflict carve-out), FASE9F Amendment, SPEC_24

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

- **Rule:** First visit to a CaveLevel in a run: generate **16-32 enemies** (deterministic
  from the level seed), with **depth scaling** (min +1 every 12 levels, max +1 every 8
  levels) and a **hard cap of 44** enemies per level. This reflects the live
  `CaveEnemySpawnPlanner` (constants `MinEnemiesPerLevel=16`, `MaxEnemiesPerLevel=32`,
  `DepthScalingHardCap=44`; `ResolveTargetEnemyCount`).
- **Applies to:** Enemy composition, spawn counts
- **Must NOT:** Re-roll on revisit (load snapshot instead)
- **Validation:** Enemy count stable within run; only changes on new game/death/debug
- **Source:** ADR-0016 (canonical density), ADR-0005 (stable-run invariant), FASE9F Amendment, SPEC_24
- **Note:** The earlier 12-20 value was the SPEC_24 plan; superseded by ADR-0016 to match
  shipping code accepted in the 2026-06-12 gameplay-expansion slice.

### Rule: Resource Node Range per Level per Run

- **Rule:** First visit to a CaveLevel in a run: generate a **per-biome resource/mineable
  node budget** (deterministic from the level seed), themed by band. This **supersedes** the
  former flat "4-10 resource nodes" range (ADR-0019). Every band guarantees a minimum
  mineable presence (stone + the band's base ore tier); deeper/richer bands carry a higher
  ceiling and rarer veins. Mineables are the **expansion of the resource-node budget**, not a
  parallel pool — they reuse `ResourceNode` + `ResourceNodeDatabaseSO` +
  `CaveLootSnapshotService`. **Final signed per-band numbers are deferred to `fable_59`
  tuning**; per-band density is authored in `CaveEnvironmentElementProfileSO` /
  `CaveEcosystemBalanceSO` (no magic values in code).
- **Applies to:** Resource placement, depletion tracking
- **Must NOT:** Re-roll on revisit; introduce a second mineable/loot system
- **Validation:** Node count stable within run (deterministic per level seed)
- **Source:** ADR-0019 (per-biome budget, supersedes 4-10), ADR-0005 (stable-run invariant),
  FASE9F Amendment, SPEC_24

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
- **Validation:** Scene **composition** must be identical to first visit. Per ADR-0018, the
  "identical" target is composition; per-instance enemy HP/death state (player kills under
  F13, and inter-monster conflict consequences) may differ on revisit and is persisted
  separately outside the `LayoutHash`.
- **Source:** FASE9F Amendment, SPEC_24, ADR-0018 (conflict/HP carve-out)

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
- [ADR-0016: Cave Enemy Density and Depth Scaling](../decisions/ADR-0016-cave-enemy-density-depth-scaling.md) (canonical enemy count: 16-32, cap 44, depth scaling)
- [ADR-0018: Cave Inter-Monster Conflict Carve-out](../decisions/ADR-0018-cave-conflict-stable-run-carveout.md) (scene-identical = composition; conflict HP/death carve-out)
- [ADR-0019: Cave Biome Mineable Budget](../decisions/ADR-0019-cave-biome-mineable-budget-supersedes-resource-node-range.md) (per-biome node budget supersedes flat 4-10)
- [ADR-0006: Save Data Contracts Simple DTOs](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (snapshot persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (cave events if needed)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Implemented by: SPEC_24*  
*Migrated from: FASE9F Amendment*
