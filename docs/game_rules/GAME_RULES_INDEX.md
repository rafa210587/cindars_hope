# Game Rules Index — Cindar's Hope

> Canonical current rules for gameplay, architecture, data, and validation.  
> ADRs explain why. Game rules state what is true now.

---

## Game Rules by Domain

| Domain | File | Key ADRs | Purpose |
|---|---|---|---|
| Documentation | [documentation_rules.md](documentation_rules.md) | ADR-0001 | Canonical folders, file organization, source locations |
| Agent Execution | [agent_execution_rules.md](agent_execution_rules.md) | ADR-0002 | What agents read/don't read by default |
| Validation/Acceptance | [validation_acceptance_rules.md](validation_acceptance_rules.md) | ADR-0004, ADR-0009 | Phase gates (0/1/2/3), acceptance criteria, NOT_RUN tracking |
| Cave Gameplay | [cave_rules.md](cave_rules.md) | ADR-0005 | Stable run invariant, snapshots, enemy/resource ranges, boss gates |
| Event/Architecture | [event_rules.md](event_rules.md) | ADR-0007 | Event bus, gameplay communication, avoid global lookup |
| Save/Data | [save_rules.md](save_rules.md) | ADR-0006 | DTO contracts, no Unity refs, versioning, migration |
| Combat | [combat_rules.md](combat_rules.md) | ADR-0005 (enemy factions) | Enemy roles, AI, status effects, damage, factions |
| Inventory/Equipment | [inventory_equipment_rules.md](inventory_equipment_rules.md) | — | Slots, capacity, durability, equip rules, shop/crafting |
| Skill Trees | [skill_tree_rules.md](skill_tree_rules.md) | — | Points, active slots, respec via Anya, save/load |
| UI/Modal | [ui_modal_rules.md](ui_modal_rules.md) | — | Modal stack, Esc behavior, input blocking, hud/inventory/shop |
| Death/Respawn | [death_anya_corpse_rules.md](death_anya_corpse_rules.md) | — | Death flow, corpse recovery, Anya respawn/respec, constraints |

---

## How to Use Game Rules

### For Implementing a Spec

A spec lists `required_game_rules: [cave_rules.md, event_rules.md]`.

Read only those documents. Don't read this entire index.

If your implementation contradicts a game rule, stop and report.

### For Creating a Game Rule

Document currently-active behavior, not desired future behavior.

If a feature is in Batch 2 and not yet validated, don't codify the rule until Phase 2-3.

### For Updating a Rule

If a rule changes, create a superseding ADR explaining why.

Update the game rule document with current behavior.

Link the ADR in this INDEX.

---

## Protected Rules (Batch 2 Pending Phase 2-3)

The following are **BLOCKED** until human Phase 2-3 validation:

- Batch 2 specs (spec_14a*, spec_14b*, spec_enemy_ai*, spec_cave_runtime*, spec_ui_ux_full_gameplay*, spec_combat_movement*)
- Related game rules will not be created until Phase 2-3 completion

---

## Not Applicable

Rules in `.claude/rules/` define agent behavior, not game behavior:
- Context reading policy (operational)
- Git safety (operational)
- YAML editing policy (tooling)

These are **not** game rules; they are agent constraints.

---

*Last Updated: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/game_rules/*.md*
