---
doc_type: game_rule
status: accepted
domain: gameplay-progression
source_adrs:
  - ADR-0010
source_documents:
  - docs/decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
  - Assets/_Game/Scripts/Skills/SkillEnums.cs
  - Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
  - Assets/_Game/Scripts/Fonte/FonteState.cs
last_reviewed: 2026-06-13
---

# Skill Tree Rules

> **Reconciled by [ADR-0010](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) (2026-06-13).**
> This rule was fully rewritten to match live runtime code and the binding FABLE v3.0
> decisions. The previous version (cap 50, 5 slots, turn-based cooldowns, 100-gold respec,
> Damage/Defense/Magic/Utility trees) was obsolete and is superseded.

## Purpose

Defines skill progression, point acquisition, rank caps, prerequisites, active skill slots, cooldowns, respec, and persistence for the FABLE skill tree.

---

## Canonical Rules

### Rule: Skill Points Acquisition

- **Rule:** The player gains skill points from:
  - **Leveling:** +1 skill point every 2 levels (`PlayerProgressionRules.SkillPointIntervalLevels = 2`).
  - **Main-quest acts:** +1 skill point per completed act of the main quest (Decision 1.9, OVERRIDE).
- **Max level:** 100 (`PlayerProgressionRules.MaxLevel = 100`; XP curve `ProgressionCurve.XpForNext`, ≈ 60·N^1.5).
- **Base skill points at cap:** **50** (level 100 ÷ 2).
- **Effective ceiling:** ~**55** = 50 base + 1 per main-quest act. The progression invariant is **"50 base + 1/act"**, not "exactly 50". F42 is amended separately to rebalance its tier thresholds (5/11/18/26) to this ceiling.
- **Starting points:** 0 at level 1 (first point is granted on reaching level 2).
- **Applies to:** Progression and skill-tree unlocking.

### Rule: Skill Tree Structure

- **Rule:** The skill tree has **5 trees** (`SkillTreeId`):
  - **Melee** — close-combat attack and weapon skills.
  - **Ranged** — bow/projectile skills.
  - **Magic** — spell-focused skills.
  - **Survival** — non-combat resilience, resistances, hunger/stamina utility.
  - **Crafting** — crafting/repair efficiency and economy hooks.
- **Node types** (`SkillNodeType`): PassiveStat, PassiveModifier, UnlockSkillAction, UpgradeSkillAction, UnlockSpell, Capstone.
- **Roster:** ~69 nodes total (catalog grown via patch WI-11); 21 real skill executors are live. The full numeric roster (damage / cooldown / cost per skill × rank) is the adendo annexed to `SKILL_ACTION_MOVEMENT_TABLE` and is the executor contract — not duplicated here.
- **Applies to:** Skill acquisition and tree navigation.

### Rule: Rank Cap (dynamic by tree depth)

- **Rule (Decision 1.1, default):** Rank caps are **dynamic by the deepest unlocked tier** of the tree. As deeper tiers unlock, the rank cap of **all** nodes in that tree rises:
  - T1 unlocked → rank cap 2
  - T2 unlocked → rank cap 3
  - T3 unlocked → rank cap 4
  - T4 / T5 unlocked → rank cap 5
- **Rule (Decision 1.2, default):** 1 skill point per rank, **including** the 3 capstone ranks.
- **Applies to:** Point allocation and build pacing.

### Rule: Node Prerequisites

- **Rule (Decision 1.3, OVERRIDE):** Per-node **prerequisites apply in addition to tier gating**. A node may require specific upstream node(s) to be unlocked, not merely the tier being open.
- **Constraint:** A node cannot be unlocked while any of its declared prerequisite nodes are locked.
- **Authoring:** Prerequisite chains for all nodes are authored as a dedicated column in `PLAYER_SKILL_TREES_DIRECTION`.
- **Applies to:** Tree navigation and build validity.

### Rule: Active Skill Slots

- **Rule:** The player has **4 active skill slots** (`ActiveSkillExecutionController`, WAVE_INTEGRATION_11):
  - Slots 0-3 are bound to keyboard keys **1, 2, 3, 4**.
  - Only an equipped active skill is usable in combat; an empty slot does nothing.
  - Skills are assigned to slots from the skill-tree menu (key U).
  - Swapping a slot assignment outside combat carries no cooldown penalty.
- **Note (re-audit acréscimo 1):** the legacy `ActiveSkillSlots.cs` path (keys R/T/Y/G) is **superseded** by the keys-1-4 controller; the legacy path is to be disambiguated/retired by an F29 amendment and must not be treated as canonical.
- **Applies to:** Combat readiness and skill selection.

### Rule: Skill Cooldowns

- **Rule:** Active skills have cooldowns measured in **real time**, not turns:
  - Cooldown decremented with `Time.deltaTime` (`ActiveSkillExecutionController`).
  - Cooldown starts immediately on use; the slot is blocked until it elapses (UI shows remaining seconds).
  - Each slot tracks its own cooldown independently.
  - Default cooldown is 1.5s when a skill defines no explicit value.
- **Applies to:** Combat balance and feel.

### Rule: Respec System (via Fonte de Anya)

- **Rule:** The player resets the skill tree at the **Fonte de Anya**, not via a town NPC dialogue:
  - Respec is **gated by the Memory Fragment** (Fonte function `Respec`, available from Fonte state `MemoryEchoing`).
  - **First respec is free; subsequent respecs cost ~250 gold** (cost policy carried on `RespecState.CostPolicy`).
  - Respec requires explicit confirmation (`RespecState.RequiresConfirmation = true`) and cannot be done mid-combat.
  - Respec refunds all spent skill points and re-locks tiers down to the player's current depth eligibility.
- **Rule (Decision 1.11, OVERRIDE — punitive respec):** Respec **invalidates the use of items whose tier became re-locked**. Such items remain in the inventory but become **non-equippable / non-usable** until the corresponding tier is re-unlocked. This requires an equipment-revalidation step at respec time (amended into F29).
- **Applies to:** Build flexibility and the cost of experimentation.

### Rule: Skill Save Persistence

- **Rule:** Skill-tree state persists in the save as IDs and counts only:
  - Unlocked node IDs and their ranks.
  - Active slot assignments (slots 0-3 → skill action ID, or empty).
  - Remaining unallocated skill points and total earned.
- **No Unity refs:** node/skill IDs only; resolved at load via the skill catalog (per ADR-0006).
- **Applies to:** All save/load mechanics.

---

## Quick Reference (canonical values)

| Field | Canonical value | Source |
|---|---|---|
| Max level | 100 | `PlayerProgressionRules.MaxLevel` |
| Skill points from leveling | 1 per 2 levels (50 at cap) | `SkillPointIntervalLevels = 2` |
| Extra skill points | +1 per main-quest act | Decision 1.9 |
| Effective point ceiling | ~55 (50 + 1/act) | Decision 1.9 / ADR-0010 |
| Trees | Melee, Ranged, Magic, Survival, Crafting | `SkillTreeId` |
| Rank cap | Dynamic by deepest unlocked tier (2→5) | Decision 1.1 |
| Point per rank | 1, including capstones | Decision 1.2 |
| Node prerequisites | Yes, in addition to tier gating | Decision 1.3 |
| Active slots | 4 (keys 1-4) | `ActiveSkillExecutionController` |
| Cooldown unit | Real time (`Time.deltaTime`) | `ActiveSkillExecutionController` |
| Respec location | Fonte de Anya (Memory Fragment gated) | `FonteFunction.Respec` |
| Respec cost | 1 free, then ~250 gold | Decision 1.8 / `RespecState.CostPolicy` |
| Respec penalty | Punitive: re-locked tier items become non-usable | Decision 1.11 |

---

## Open Questions

- Exact per-act gold/point scaling once F42 tier rebalance (50 + 1/act) lands.
- Whether re-unlocking a tier auto-restores previously-equipped items or requires manual re-equip after a punitive respec (current intent: manual re-equip).
- Final numeric cooldown/cost values per skill × rank live in the `SKILL_ACTION_MOVEMENT_TABLE` adendo, not here.

---

## Related ADRs

- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) (this rewrite)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (skill persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (skill use events)

---

*Last Reviewed: 2026-06-13 (reconciled by ADR-0010 against FABLE v3.0 + live code)*  
*Source: ADR-0010, FABLE v3.0 (1.1, 1.2, 1.3, 1.8, 1.9, 1.11)*
