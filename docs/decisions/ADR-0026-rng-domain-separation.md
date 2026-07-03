---
doc_type: adr
status: proposed
adr_id: ADR-0026
title: RNG Domain Separation (Gameplay / World / Visual)
date: 2026-07-03
source_documents:
  - .claude/skills/rng-and-determinism/SKILL.md
  - .claude/rules/cave-stable-run.md
supersedes: []
superseded_by: []
applies_to:
  - determinism
  - save-load
  - rng-and-determinism
---

# ADR-0026 — RNG Domain Separation (Gameplay / World / Visual)

## Status

**proposed** (draft — requires human decision before any implementation; extends the existing
`rng-and-determinism` skill, does not replace it)

## Context

The project already has a documented determinism precedent for the cave: the `cave-stable-run`
rule and the `rng-and-determinism` skill establish that RNG touching saves, loot, weather, spawns,
or generation must be seeded per-system rather than pulled from the single global
`UnityEngine.Random` stream, exactly to avoid the class of bug the FASE9F stable-run contract was
built to prevent (a revisited `CaveLevel` re-rolling its layout/enemies/resources because some
unrelated code path also called into the shared global RNG state first).

Reconfirmed 2026-07-03 via `Select-String -Pattern "UnityEngine\.Random" -List` across
`Assets/_Game/Scripts/**`: **13 files** call `UnityEngine.Random` directly today (the prior audit
prompt cited 14; the one-file delta is not material and does not change the qualitative finding).
Confirmed files include gameplay-adjacent systems (`EnemyBrain`, `EnemyMovementExecutor`,
`EnemyActionRunner`, `EnemyEvasionDecision`, `PlayerCombatStatsProvider`) alongside
world/save-adjacent systems (`CaveWanderingMerchant`, `CaveEnemySpawnPlanner`,
`CaveEcosystemConflictPlanner`, `FishingCatchResolver`, `FishingTableSO`, `TrapDisarmResolver`,
`EnemyLootResolver`, `LootTableSO`) — i.e. the same shared global `UnityEngine.Random` stream is
consumed by both "doesn't need to be deterministic" gameplay feel (e.g. an enemy's evasion jitter)
and "must be deterministic/seeded" world generation and loot (already partially seeded per-system
in the cave code, but not architecturally separated by domain).

## Decision (proposed, not accepted)

Generalize the cave's per-system-seeded pattern into three explicit RNG domains, each with its
own seeded stream, so that consuming one domain's randomness can never perturb another's sequence:

1. **World RNG** — anything that must be deterministic/reproducible and touches saves: cave
   layout/spawn/loot generation (already following this in spirit via `CaveRunSeed` +
   `cave-stable-run`), weather generation, world events. Seeded from
   `CaveWorldSeed`/`CaveRunSeed`-class stable seeds, never from wall-clock time or a shared global
   stream.
2. **Gameplay RNG** — combat/AI randomness that affects outcomes but is not required to replay
   identically across a session revisit (e.g. enemy evasion jitter, hit variance) — still ideally
   seeded per-encounter for testability, but does not carry the strict revisit-stability contract
   that World RNG does.
3. **Visual RNG** — pure cosmetic variance (particle jitter, idle-animation timing variety) that
   never needs to be deterministic or reproducible and can safely keep using the shared
   `UnityEngine.Random` stream or an unseeded local `System.Random` with no save/determinism
   contract at all.

Each of the 13 confirmed call sites must be classified into one of these three domains and
migrated to use that domain's seeded RNG source instead of `UnityEngine.Random` directly.

### Alternatives considered

- **Do nothing (status quo):** the cave already has its own carve-out (`cave-stable-run`) so the
  worst-case failure mode (cave reroll on revisit) has some protection; but the other 12 files
  outside the cave module have no equivalent protection today, and any future save/replay feature
  touching combat or fishing/loot would inherit the same class of bug the cave rule was written to
  prevent.
- **One single seeded RNG for everything:** simpler than three domains, but conflates "must be
  bit-identical on replay" (World) with "just needs to feel varied" (Visual) — forcing Visual
  jitter through the same seeded stream as World generation makes the World stream's sequence
  sensitive to how many times a purely cosmetic effect fired, defeating the purpose.
- **Three-domain separation (this proposal):** matches the precedent already established for the
  cave and generalizes it project-wide with the least conceptual novelty — no new pattern
  invented, just applying the existing one broadly and explicitly naming the domain boundary.

## Cost

- Medium: 13 confirmed call sites to reclassify and migrate (some already partially seeded per the
  cave precedent and may need only the domain label applied, not new seeding logic); requires a
  new small `WorldRng`/`GameplayRng`/`VisualRng` (or equivalent) static/service surface if one does
  not already exist in a general-purpose form outside the cave-specific seeding utilities.

## Risk

- Misclassifying a call site (e.g. treating a save-relevant roll as Visual) would reintroduce
  exactly the bug class `cave-stable-run` exists to prevent, just in a new system; classification
  must be done file-by-file with the actual usage context, not by file name alone.
- If World RNG migration touches loot/fishing tables, existing balance tuning (seeded rolls used
  for drop-rate tuning) could shift observed outcomes even with the same underlying probabilities,
  since seed derivation changes — should be validated against existing loot/economy balance docs
  before shipping.

## Requer decisão humana

**SIM** — motivo: this changes RNG consumption across 13 files spanning combat, cave, fishing, and
loot systems, some of which have already-tuned balance behavior; the domain boundaries and the
migration order (world-critical first vs. gameplay-feel first) should be a human call, and any
seed-derivation change to already-seeded cave code must be checked against the `cave-stable-run`
invariant before being touched.

## Source Documents

- [rng-and-determinism SKILL.md](../../.claude/skills/rng-and-determinism/SKILL.md)
- [cave-stable-run rule](../../.claude/rules/cave-stable-run.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
