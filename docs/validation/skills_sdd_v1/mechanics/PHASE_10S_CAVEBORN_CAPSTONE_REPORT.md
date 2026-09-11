# Phase 10S — Caveborn / Telisandra capstone

**Spec:** `spec_skills_20_survival_crafting_capstones_v1` — slice 20S  
**Validation date:** 2026-09-10  
**Result:** PASS — independent non-regression audit approved

## Delivered behavior

- Nascido da Caverna activates once per opaque cave `RunId` when HP crosses below 25%, stamina
  crosses below 15%, or fatigue enters Exhausted. HP handling activates it before Last Breath.
- Ranks use the approved 8/9/10-second duration, 30/38/45% run and dodge cost reduction,
  20/25/30% protection for the authored environmental/mental families and +0.4 dodge tile.
- Rank three doubles natural HP regeneration after combat ends for the remaining buff duration.
  Natural HP regeneration now has a real data-driven owner at 1 HP/s outside combat.
- Save/load and revisits preserve consumption for the same run. Respec clears the active reward
  without rearming it; a different run identity can activate again.
- The runtime only reads run identity. It does not change cave seed, snapshots, generation,
  materialization, enemies or resources.
- The obsolete flat resistance and MaxStamina modifiers were removed from the canonical capstone.

## Evidence

- Canonical catalog generation: PASS, 66 nodes and 31 actions —
  `Logs/skills-phase20s-generate.log`.
- Caveborn deterministic EditMode: PASS, 9/9 —
  `TestResults/skills-phase20s-caveborn-editmode-r2.xml`.
- Caveborn composition PlayMode: PASS, 4/4 —
  `TestResults/skills-phase20s-caveborn-playmode-r3.xml`.

The PlayMode scenarios exercise the real coordinator and event flow: HP 30→24, ordering with Last
Breath, repeated HP/stamina/Exhausted triggers, rank-three modifier providers, save restore and
unchanged cave seed.

The refreshed round also measures the real `PlayerVitalsApplier` healing at 1 HP/s and 2 HP/s,
then exercises canonical defeat followed by a new run identity.

Independent review confirmed the named thresholds, one-activation-per-run contract, save/load,
respec, defeat/new-run rearm and cave stable-run boundaries.

## Balance boundary

The 1 HP/s base regeneration is the initial authored baseline needed to make the rank-three effect
functional. Phase 21 must measure its sustain against common, elite and boss encounters before the
whole skill set can be declared balanced.
