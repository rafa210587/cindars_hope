# Phase 09B — Melee combat capstones

**Spec:** `spec_skills_19_combat_capstones_v1` — T19B  
**Validation date:** 2026-09-10  
**Result:** PASS

## Delivered behavior

- Kanthor activates once for a perfect block/posture-break pair sharing one resolution id,
  refreshes to eight seconds on a distinct trigger and maintains at most one heal charge.
- The charge is consumed only by confirmed primary player melee damage above zero. It heals
  `ceil(MaxHP × 2/3/3%)`; rank three restores 10 stamina when HP was already full.
- Kanthor applies the authored melee damage, player knockback and Stun-duration reductions.
- Kaand activates only from a player-caused posture break and applies the authored direct melee
  and critical-damage bonuses for eight seconds without healing.
- Normal melee attacks and active melee skills carry source/target identity, one action token,
  primary/capstone gates and player posture-break causality.
- Save preserves remaining time, the single charge and a bounded causal ledger. Restore is silent;
  respec clears transient rewards.
- Cross-system HP/stamina rewards use Foundation runtime ports, and combat reads modifier providers
  from Foundation without a Combat-to-Skills dependency.

## Evidence

- Unity compile: PASS — `Logs/skills-phase19bc-compile-ports.log`.
- Melee capstone EditMode: PASS, 9/9 —
  `Logs/skills-phase19b-editmode-results.xml`.
- Melee capstone PlayMode: PASS, 4/4 —
  `Logs/skills-phase19b-playmode-results.xml`.
- Generalized encounter EditMode foundation: PASS, 9/9 —
  `Logs/skills-phase19c-encounter-editmode-results.xml`.
- Generalized encounter PlayMode foundation: PASS, 3/3 —
  `Logs/skills-phase19c-encounter-playmode-results.xml`.
- Existing survival PlayMode regression: PASS, 5/5 —
  `Logs/skills-phase19c-survival-regression-results.xml`.
- Independent non-regression reaudits: functional PASS; final documentation warning corrected.

## Phase boundary

Encounter identity and lifecycle are validated infrastructure for T19C. Lunar target ownership,
isolation, impact bonuses and elemental secondary damage remain part of the ranged subphase.
