---
doc_type: adr
status: accepted
adr_id: ADR-0010
title: FABLE Skill and Inventory Rules Reconciliation
date: 2026-06-13
source_documents:
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
  - Assets/_Game/Scripts/Skills/SkillEnums.cs
  - Assets/_Game/Scripts/Inventory/InventoryManager.cs
  - Assets/_Game/Scripts/Equipment/EquipmentSlot.cs
  - Assets/_Game/Scripts/Fonte/FonteState.cs
supersedes: []
superseded_by: []
applies_to:
  - gameplay-progression
  - skill-tree
  - inventory-gameplay
  - equipment
  - save-load
relates_to:
  - docs/game_rules/skill_tree_rules.md
  - docs/game_rules/inventory_equipment_rules.md
---

# ADR-0010 — FABLE Skill and Inventory Rules Reconciliation

## Status

**accepted** (2026-06-13)

## Context

The FABLE re-audit of code (`reaudit-code-v3`, 2026-06-13, evidence per file:line) confirmed that two canonical game rules diverged severely from the live runtime code and from the v3.0 binding design decisions (`docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md`).

Both rules carry `status: accepted`, so per the project's docs governance (ADRs and game_rules are canonical; a behavior/contract change to an accepted rule requires a superseding decision, not a silent edit) the reconciliation must be recorded in an ADR before the rules are rewritten. Without this reconciliation, every skill spec (FABLE F29/F33) and item spec (F32) would hit the `code vs. game_rule` stop condition.

### Confirmed divergences — `skill_tree_rules.md` (all obsolete)

| Rule field said | Live code says |
|---|---|
| Max level 50 | `PlayerProgressionRules.MaxLevel = 100` (curve `ProgressionCurve.XpForNext`, 60·N^1.5) |
| +1 skill point per level (50 points) | `SkillPointIntervalLevels = 2` → 1 point every 2 levels = **50 base** points at cap 100 |
| 5 active skill slots | 4 active slots, keys **1-4** (`ActiveSkillExecutionController`, WAVE_INTEGRATION_11) |
| Cooldown in turns (1 turn = 1s) | Cooldown in **real time** (`Time.deltaTime`) |
| Respec 100 gold, unlimited | Respec via **Fonte de Anya** (1 free, then ~250 gold), gated by the Memory Fragment |
| Trees: Damage / Defense / Magic / Utility | `SkillTreeId`: **Melee / Ranged / Magic / Survival / Crafting** |
| Roster: Slash / Fireball / etc. | 21 real skill executors; catalog grew to ~69 nodes (patch WI-11) |

### Confirmed divergences — `inventory_equipment_rules.md` (obsolete)

| Rule field said | Live code says |
|---|---|
| 20 inventory slots | `InventoryManager` DefaultCapacity = MaxCapacity = **30** |
| 1 accessory slot; 4 equipped max | `EquipmentSlot`: 9 slots (LeftHand, RightHand, Head, Chest, Legs, Boots, **Ring1, Ring2, Accessory**) — **3 accessory-type** slots |

## Decision

**Rewrite both game rules in full to reconcile them with the live code AND with the v3.0 binding decisions, recording the reconciliation here.**

### 1. `skill_tree_rules.md` is rewritten to reflect

- Level cap 100; 50 base skill points (1 per 2 levels, `SkillPointIntervalLevels=2`).
- **Decision 1.9 (OVERRIDE):** keep +1 skill point per **main-quest act**; effective ceiling becomes ~55 (50 base + 1/act). The invariant is no longer "exactly 50" — it becomes "50 base + 1/act". F42 is amended separately to rebalance its tiers (5/11/18/26) to the new ceiling.
- 4 active skill slots (keys 1-4); cooldowns in **real time**, not turns.
- 5 trees: Melee / Ranged / Magic / Survival / Crafting.
- Respec via the **Fonte de Anya** (1 free, then ~250 gold), gated by the Memory Fragment.
- **Decision 1.1 (default):** rank cap is **dynamic** by tree depth — the rank cap of all nodes rises with the deepest unlocked tier (T1 open → cap 2 … T4/T5 → cap 5).
- **Decision 1.2 (default):** 1 skill point per rank, including the 3 capstone ranks.
- **Decision 1.3 (OVERRIDE):** per-node **prerequisites** apply in addition to tier gating.
- **Decision 1.11 (OVERRIDE):** respec is **punitive** — it invalidates the use of items belonging to tiers that became re-locked. Items stay in the inventory but become non-equippable/non-usable until the tier is re-unlocked. This requires an equipment-revalidation step on respec (amended into F29).

### 2. `inventory_equipment_rules.md` is rewritten to reflect

- 30 inventory slots (`InventoryManager`).
- 9 equipment slots, of which **3 are accessory-type** (Ring1, Ring2, Accessory) — **Decision 2.12**. Canonical naming: two Ring slots + one Amulet/Charm-style Accessory slot; a +6 pouch expansion (F31) is noted as a future capacity modifier.
- **Decision 2.11:** durability is a derived table per item class × material (base 80 weapon / 150 armor × material modifiers), and upgrade cost +N = (2N × material band) + (BV × 0.5N) gold.

### 3. Governance

- This ADR is the canonical reconciliation record for both rewrites. Each rewritten game rule is marked at the top with "Reconciled by ADR-0010 (2026-06-13)".
- Both rules keep `status: accepted` and their filenames; their `source_adrs` now reference ADR-0010.
- The v3.0 decision document is design input; this ADR + the two game rules are the canonical *what*.

## Consequences

- **Positive:** skill and item specs (F29/F32/F33) no longer hit the `code vs. game_rule` stop condition; the canonical rules now match shipping code.
- **Positive:** the binding v3.0 design decisions (dynamic rank cap, per-node prerequisites, punitive respec, accessory-slot reality, derived durability) are captured in canonical rules.
- **Cost:** Decision 1.9 forces a separate F42 amendment (ceiling ~55, tier rebalance) — tracked there, not here.
- **Cost:** Decision 1.11 introduces a new equipment-revalidation requirement on respec — tracked as an F29 amendment, not implemented by this ADR.
- **Scope:** this ADR and the two rewrites are docs-only. No runtime code, scenes, prefabs, or assets change. The code already matches the reconciled values; the divergence was in the documentation, not the code.

## Applies To

- Skill tree progression (`PlayerProgressionRules`, `SkillTreeId`, `ActiveSkillExecutionController`, Fonte de Anya respec)
- Inventory and equipment (`InventoryManager`, `EquipmentSlot`)
- Save/load (skill state and item DTOs — IDs only)

## Source Documents

- [FABLE Decisões e Respostas v3.0](../design/FABLE_DECISOES_RESPOSTAS_v3.0.md) (binding decisions 1.1, 1.2, 1.3, 1.8, 1.9, 1.11, 2.11, 2.12; re-audit acréscimos 1 and 5)
- [skill_tree_rules.md](../game_rules/skill_tree_rules.md) (rewritten by this ADR)
- [inventory_equipment_rules.md](../game_rules/inventory_equipment_rules.md) (rewritten by this ADR)
- [ADR-0006: Save Data Contracts](./ADR-0006-save-data-contracts-simple-dtos.md)
- [ADR-0007: Event Bus Gameplay Communication](./ADR-0007-event-bus-gameplay-communication.md)

---

*Created: 2026-06-13*  
*Status: accepted*  
*Reconciles: skill_tree_rules.md, inventory_equipment_rules.md*
