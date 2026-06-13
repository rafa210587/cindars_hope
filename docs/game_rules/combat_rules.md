---
doc_type: game_rule
status: accepted
domain: combat-gameplay
source_adrs:
  - ADR-0005
source_documents:
  - docs/amendments/FASE9G_COMBAT_SYSTEM_AMENDMENT_v1.0.md
  - .specs/implementados/SPEC_09_COMBAT_STATUS_EFFECTS.md
last_reviewed: 2026-06-01
---

# Combat Rules

## Purpose

Defines enemy roles, combat interactions, status effects, and damage systems.

---

## Canonical Rules

### Rule: Enemy Level Distribution

- **Rule:** Enemy composition on first cave level visit follows distribution:
  - 70-80%: EnemyLevel = CaveLevel (same level as cave)
  - Remainder: Scaled based on difficulty and cave progression (lower levels possible)

- **Applies to:** Cave level enemy generation
- **Constraint:** Distribution must be preserved on revisit (use snapshot from cave_rules.md)
- **Source:** FASE9G Amendment

### Rule: Status Effect Duration in Turns

- **Rule:** Status effect `DurationTurns` field represents tick count in the MVP runtime:
  - 1 tick = 1 second (gameplay time, not real time)
  - Duration persists across turns; decrements each tick
  - Effect expires when DurationTurns <= 0

- **Example:**
  - StatusEffect with DurationTurns=5 lasts approximately 5 seconds
  - If re-applied before expiry, duration is reset to new DurationTurns value
  - Persists in save (snapshot includes active status effects)

- **Applies to:** All status effects (burn, poison, stun, etc.)
- **Source:** SPEC_09, FASE9G Amendment

### Rule: Status Effect Stacking

- **Rule:** Multiple instances of same status effect on same target:
  - Do not stack (replace with new application, resetting duration)
  - Single active instance per status per target
  - Re-application resets duration to new DurationTurns

- **Example:**
  - Player has Burn (3 turns remaining)
  - Enemy casts Burn again (5 turns)
  - Result: Player has Burn (5 turns), not stacked

- **Applies to:** All status effects

### Rule: Damage Calculation

- **Rule:** Damage = Base Damage ± random variance + status effects
  - Base Damage: from weapon/spell
  - Random variance: ±10% (or configurable range)
  - Status effects: can amplify or reduce damage
  - Critical hits: double damage on RNG roll (if implemented)

- **Constraint:** Minimum damage = 1; no 0 damage hits
- **Applies to:** All attack calculations

### Rule: Enemy AI Behavior

- **Rule:** Enemies select actions based on:
  - Current health (low health → healing or escape priority)
  - Target distance (melee vs. ranged selection)
  - Status effects (stunned → skip turn; poisoned → wait longer)
  - Ability cooldowns

- **Constraint:** AI must be deterministic within a cave run (use seeded RNG from CaveRunSeed)
- **Applies to:** All enemy AI decision-making

### Rule: Combat Turn Order

- **Rule:** Combat turn order determined by:
  - Submitted before execution (not real-time decision)
  - Speed stat + RNG modifier
  - Turn order recalculated after status effects

- **Applies to:** Multi-unit combat systems

---

## Status Effects (MVP Roster)

| Status | Source | Duration | Effect | Removal |
|---|---|---|---|---|
| Burn | Spell: Fireball | 3-5 turns | DoT (damage per turn) | Duration expiry or cure |
| Poison | Spell: Venom | 4-6 turns | DoT (slower than burn) | Duration expiry or cure |
| Stun | Spell: Shockwave | 1-2 turns | Skip next turn | Duration expiry |
| Slow | Spell: Freeze | 2-4 turns | Speed reduction | Duration expiry |

---

## Open Questions

- What happens if an enemy is stunned? (Current: skip entire turn, turn order recalculates)
- Do status effects stack with themselves or different statuses? (Current: single instance per status, no stacking)
- Can status effects be cleansed? (Current: expire on duration; cleanse not implemented in MVP)
- What is the critical hit rate? (Current: configurable; MVP uses 10% base)

---

## Related ADRs

- [ADR-0005: Cave Stable Run and Replay](../decisions/ADR-0005-cave-stable-run-and-replay.md) (enemy snapshots)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (damage/status events)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Migrated from: FASE9G Amendment, SPEC_09*
