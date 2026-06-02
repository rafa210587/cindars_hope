---
doc_type: game_rule
status: accepted
domain: gameplay-progression
source_adrs: []
source_documents:
  - docs/specs/implementados/SPEC_26_SKILL_TREE_CLOSEOUT.md
last_reviewed: 2026-06-01
---

# Skill Tree Rules

## Purpose

Defines skill progression, point allocation, respecs, and active skill slots.

---

## Canonical Rules

### Rule: Skill Points Acquisition

- **Rule:** Player gains skill points from:
  - Leveling up: +1 skill point per level
  - Quest completion: variable skill points as rewards
  - Milestone achievements: variable skill points

- **Max level:** 50 (MVP cap; extensible beyond)
- **Max skill points:** 50 (if acquired only from leveling)
- **Starting points:** 1 point at game start (for first skill)

- **Applies to:** Progression and skill tree unlocking

### Rule: Skill Tree Structure

- **Rule:** Skill tree has branches/trees:
  - Damage tree: attack-focused skills
  - Defense tree: protection-focused skills
  - Magic tree: spell-focused skills
  - Utility tree: non-combat skills (if applicable)

- **Prerequisites:** Some skills require other skills unlocked first
- **Constraint:** Cannot unlearn a skill if other skills depend on it (unless respec)

- **Applies to:** Skill acquisition and tree navigation

### Rule: Active Skill Slots

- **Rule:** Player has limited active skill slots:
  - Max 5 active skills at once
  - Can drag/drop skills into slots or use skill bar
  - Active skills are the only ones usable in combat
  - Inactive skills can be swapped out (no cooldown on swap)

- **Applies to:** Combat readiness and skill selection

### Rule: Skill Cooldowns

- **Rule:** Skills have cooldowns after use:
  - Cooldown in turns (1 turn = 1 second in MVP runtime)
  - Cooldown starts immediately after skill is used
  - Multiple skills can have separate cooldowns
  - Cooldowns persist across turns/combat rounds

- **Cooldown range:** 0-30 turns (depending on skill power)
- **Applies to:** Combat balance

### Rule: Respec System (via Anya)

- **Rule:** Player can reset skill tree with NPC Anya (costs gold):
  - Refunds all skill points
  - Resets all skills to unlearned
  - Can immediately re-allocate points into different skills
  - Anya NPC located in starting area (farm/village)

- **Cost:** 100 gold per respec (configurable)
- **Cooldown:** None (immediate respec possible)
- **Constraint:** Cannot respec mid-combat

- **Applies to:** Skill flexibility and experimentation

### Rule: Skill Save Persistence

- **Rule:** Skill tree state persists in save:
  - List of learned skills (skill IDs)
  - Skill point allocation breakdown
  - Active skill slot assignments
  - Remaining unallocated skill points

- **Save format:** SkillTreeSaveData with list of skillIds + remainingPoints + activeSlots
- **No Unity refs:** Skill IDs only; resolved at load time via SkillDatabaseSO

- **Applies to:** All save/load mechanics

---

## Skill Roster (MVP)

| Skill | Tree | Cost | Cooldown | Effect | Prereq |
|---|---|---|---|---|---|
| Slash | Damage | 1 | 0 | Basic attack | None |
| Power Strike | Damage | 2 | 3 | High damage hit | Slash |
| Whirlwind | Damage | 3 | 5 | AoE attack | Power Strike |
| Block | Defense | 1 | 0 | Reduce next damage | None |
| Parry | Defense | 2 | 2 | Negate next attack | Block |
| Shield Bash | Defense | 2 | 3 | Stun + damage | Block |
| Fireball | Magic | 2 | 4 | Fire damage + burn | None |
| Ice Spike | Magic | 2 | 4 | Ice damage + slow | None |
| Lightning | Magic | 3 | 6 | Chain damage | Fireball OR Ice Spike |

---

## Open Questions

- Can players swap active skills during rest? (Current: yes, any time except mid-combat)
- Do skill points carry over if max level is increased? (Current: yes; new levels still grant points)
- Can a skill be learned twice for bonus? (Current: no; each skill learned once)
- Are there hidden/secret skills? (Current: no; all skills visible in tree)

---

## Related ADRs

- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (skill persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (skill use events)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: SPEC_26*
