---
doc_type: adr
status: accepted
adr_id: ADR-0018
title: Cave Inter-Monster Conflict Carve-out from Stable-Run "Scene Identical"
date: 2026-06-23
source_documents:
  - .specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md
  - docs/decisions/ADR-0005-cave-stable-run-and-replay.md
  - docs/game_rules/cave_rules.md
supersedes:
  - ADR-0005 (only the §"Scene must be identical on revisit" clause, scoped to non-player-caused enemy HP/death state)
superseded_by: []
applies_to:
  - cave-procedural-generation
  - cave-gameplay
  - combat
relates_to:
  - docs/game_rules/cave_rules.md
  - docs/game_rules/save_rules.md
---

# ADR-0018 — Cave Inter-Monster Conflict Carve-out from Stable-Run "Scene Identical"

## Status

**accepted** (2026-06-23)

## Context

`fable_78` ("Caverna Viva") introduces a deterministic **inter-monster conflict** system:
on each entry into a cave level there is a small chance (5%, dropping to 0.5% after the
first conflict on that level in the run) that two **different** species already present in
the level become hostile to each other, deal reduced damage between them, and can kill one
another.

`docs/game_rules/cave_rules.md` (Rule: Revisit Loading Snapshot) currently states the
scene "**must be identical to first visit**", sourced from ADR-0005 (FASE9F stable-run
invariant). Taken literally, that clause forbids any enemy HP or death state changing for a
non-player cause — which is exactly what a monster killing another monster does.

This is a documentation conflict, not a design contradiction:

- ADR-0005's intent is that **composition** does not re-roll on revisit: which enemies
  exist, their count, positions, IDs, and types must be stable within a `CaveRunSeed`.
- The conflict system never touches composition. It only changes **behavior during a
  visit** (who targets whom) and the resulting HP/death state — and per-instance enemy HP
  is *already* persisted outside the `LayoutHash` by the F13 (`fable_13`) enemy-HP-per-
  instance contract. A player killing an enemy and revisiting already produces a "not
  identical" scene under F13; that was accepted.

Per `docs-governance` (#3: ADRs/game_rules are canonical; a behavior change requires a
*superseding* decision, not a silent edit), the "scene identical" clause needs a named,
scoped carve-out before `fable_78` can be implemented without contradicting the canonical
game rule.

## Decision

**Inter-monster conflict consequences are a named exception to the stable-run "scene must
be identical on revisit" clause. Composition stays deterministic and stable; only
behavior-per-visit and the resulting per-instance HP/death state may differ on revisit,
already covered by the F13 enemy-HP contract.**

- **Composition remains stable (ADR-0005 unchanged):** which enemies exist, their count,
  spawn positions, IDs, and types are deterministic from
  `(CaveWorldSeed, CaveRunSeed, CaveLevel)` and must not re-roll on revisit.
- **Conflict is behavior, not composition:** the conflict roll is seeded by
  `(worldSeed, runSeed, caveLevel, entryIndex)`. It is reproducible for a given entry index
  and intentionally re-rolls **per entry** (the entry counter advances). This re-roll is of
  *behavior* (hostility between two already-present species during that visit), never of
  composition, and is therefore compatible with ADR-0005.
- **HP/death consequences are F13's domain:** damage dealt monster→monster routes through
  the normal `EnemyHealth.TakeDamage` path and persists via the existing per-instance
  `EnemyHpRecords` (outside the `LayoutHash`). A monster wounded or killed by a rival is the
  same kind of HP-state delta that a player kill already produces — no new persistence
  channel is created.
- **The "scene identical" clause is read as "composition identical":** layout, entrance/
  exit, enemy/resource composition (count/positions/IDs/types), and node depletion remain
  the validation target of the replay validator. Non-player-caused HP/death state is
  explicitly carved out, identical in spirit to the existing F13 carve-out for player kills.

## Consequences

### Positive

- `fable_78` can implement deterministic conflict without contradicting `cave_rules.md`.
- The replay validator keeps a clear, narrow contract: it validates *composition* stability,
  which conflict never violates.
- The carve-out is named and scoped, so a future agent will not "fix" the planner toward a
  literal scene-identical reading and regress the feature.

### Negative

- A revisited level can show different live HP / dead-enemy state than first visit when a
  conflict occurred. This is intended and consistent with F13 (player kills already do this).
- Reversible by a future ADR if the human decides conflict consequences should not persist.

### Operational

- `docs/game_rules/cave_rules.md` (Rule: Revisit Loading Snapshot) updated with the carve-out,
  citing this ADR; the "must preserve" list is clarified to mean composition.
- The conflict planner (`CaveEcosystemConflictPlanner`) must remain pure and seeded by
  `(worldSeed, runSeed, caveLevel, entryIndex)` via `CaveLayoutStableHash` (FNV-1a); no
  `UnityEngine.Random`, GUID, or timestamp.
- Validation: docs validation exit 0; replay validator continues to assert composition
  stability (unchanged target).

## Applies To

- Cave inter-monster conflict (`CaveEcosystemConflictPlanner`, `fable_78`)
- Per-instance enemy HP/death persistence interaction (F13 / `fable_13`)
- Cave replay validator scope (composition only)

## NOT Applicable To

- Enemy composition (count/positions/IDs/types) — unchanged and canonical under ADR-0005.
- Layout / entrance / exit / node depletion stability — unchanged.
- Player-caused kills and loot — unchanged (existing F13 + `EnemyKilledEvent` path).

## Source Documents

- [`fable_78` spec](../../.specs/a_implementar/fable/fable_78_spec_cave_ecosystem_population_runtime.md) — §14.5, §14.10 (G1), §16.4 (conflict snapshot)
- [ADR-0005: Cave Stable Run and Replay](./ADR-0005-cave-stable-run-and-replay.md) — partially carved out (scene-identical clause only)
- [cave_rules.md](../game_rules/cave_rules.md) — updated by this ADR

## Migration Notes

- `cave_rules.md` "Revisit Loading Snapshot" rule annotated with the conflict carve-out and
  clarified to mean composition; "Caves are Procedural by Run" `Must preserve` list scoped to
  composition + node depletion.
- No amendment retired; no document deleted.

---

*Created: 2026-06-23 (fable_78 SLICE 1)*
*Status: accepted*
*Partially supersedes: ADR-0005 (scene-identical clause, scoped to non-player-caused HP/death)*
