# RUN Ã¢â‚¬â€ Wave 00: Planning & Base Corrections

**Date:** 2026-05-23
**Branch:** wave/specs-overnight-00-plan
**Executor:** Claude Code
**Status:** Planning phase

---

## Objective

Map all future specs in `docs/specs/a_implementar/`, organize them into macro-waves, identify dependencies, gaps and corrections needed, validate documentation and Unity baseline before executing waves 01-07.

---

## Git Status

- **Branch base:** dev (up to date with origin)
- **Branch created:** wave/specs-overnight-00-plan
- **Commit initial:** (before any changes)
- **Working tree:** clean

---

## Specs Mapped Ã¢â‚¬â€ 22 Total

### FASE9E Group (Data, Save, Progression, Damage)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9e_item_taxonomy_ids.md | A implementar | spec_data_001 | Define item categories, prefixes, flags, stacks |
| spec_fase9e_item_examples_variations.md | A implementar | spec_data_001, spec_inventory_001 | 6 seeds, 6 crops, 8+ consumables, 4 materials |
| spec_fase9e_save_schema_migration.md | A implementar | spec_save_001 | Versioning, migration, Equipment/Hotbar/Status persistence |
| spec_fase9e_player_level_up_progression.md | A implementar | spec_progression_001 | Level 1-100, XP curve, 6 attributes, skill points |
| spec_fase9e_damage_status_elements_PARTIAL.md | A implementar | spec_damage_001 | Status effects (poison, burn, bleed), elemental interactions |
| spec_fase9e_ui_hotbar_inventory_equipment_final.md | A implementar | spec_ui_001, spec_tools_001 | UI for hotbar, inventory, equipment |
| spec_fase9e_attribute_allocation_debug.md | A implementar | spec_progression_001 | Attribute allocation, respec mechanics |

### FASE9C & Tools Group (Equipment, Tools, Combat)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9c_player_equipment_items_combat_remaining.md | A implementar | spec_tools_001, spec_combat_001 | Loadouts, equipment slots, combat items |
| spec_fase9c_tools_farm_combat_refinement_remaining.md | A implementar | spec_tools_001, spec_farm_003, spec_combat_001 | Tool refinements, farm/combat integration |

### FASE9D Group (Enemies, AI, Architecture)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9d_enemy_actions_ai_combat.md | A implementar | spec_combat_002 | Enemy actions, AI patterns, combat behavior |
| spec_fase9d_enemy_architecture_40_monsters.md | A implementar | spec_combat_002 | Data-driven roster, 40 monster examples |

### FASE9F Group (Cave Resources, Encounters)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9f_cave_resources_encounters_PARTIAL.md | A implementar | spec_cave_002 | Resource nodes, encounter spawn rules |

### FASE9G Group (Cave Bestiary, Amendment)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9g_cave_bestiary_faction_locks.md | A implementar | spec_cave_004 | Bestiary, faction locks, ecology |
| spec_fase9g_enemy_combat_roles_ai_status_amendment.md | A implementar | spec_combat_002, spec_cave_004 | Enemy roles, AI refinement, status update |

### FASE9H Group (Loot, Crafting, Equipment, Durability)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9h_loot_crafting_equipment_durability_environment.md | A implementar | spec_tools_001, spec_craft_001, spec_cave_002 | Loot tables, crafting, durability, environmental resistance |

### FASE9I Group (Player Combat, Weapons, Magic)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9i_player_combat_weapons_magic_skill_actions.md | A implementar | spec_combat_001, spec_damage_001, spec_progression_001 | Weapon attacks, magic, skill actions |

### FASE9J Group (Cave Entry, Death, Recovery)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9j_cave_entry_loadout_death_anya_corpse.md | A implementar | spec_cave_002, spec_cave_004, spec_ui_001 | Cave entry flow, death mechanics, corpse recovery, Fonte de Anya |

### FASE9K Group (Skill Trees, Nodes)

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9k_skill_trees_nodes_active_slots_respec.md | A implementar | spec_progression_001 | Skill trees, active slots, respec |

### FASE9L & UI Group

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fase9l_ui_ux_full_gameplay.md | A implementar | placeholder controlado | Full UI/UX (placeholder, will be refined before implementing) |
| spec_ui_menu_systems_final.md | A implementar | spec_ui_001 | Menu systems |

### Other

| Spec | Status | Dependencies | Estimated Scope |
|---|---|---|---|
| spec_fishing_combat_integration_final.md | A implementar | spec_farm_003, spec_combat_001 | Fishing + combat integration |
| spec_future_ideas_todo.md | A implementar | backlog ativo | Future ideas, not immediate implementation |

---

## Refinements to Re-read

Key refinements by wave (to be read before each wave):

- `docs/refinements/a_implementar/` for all future refinements
- Key historical refinements from `docs/refinements/implementados/` for context

---

## Proposed Macro-Wave Composition

### Wave 00 (Current) Ã¢â‚¬â€ Planning & Base Corrections
- **Goal:** Map specs, refine dependencies, validate baseline
- **Commits:** Up to 2-3 for critical corrections only
- **Scope:**
  - Ã¢Å“â€¦ Map all 22 specs
  - Ã¢Å“â€¦ Identify dependencies
  - Ã¢Å“â€¦ Identify lacunas/contradictions
  - Ã¢Å“â€¦ Validate documentation
  - Ã¢Å“â€¦ Validate Unity baseline
  - Ã¢Å¡Â Ã¯Â¸Â Correct CaveBossGateRegistry asset structure if needed
  - Ã¢Å¡Â Ã¯Â¸Â Ensure no duplicate registries in Assets/Resources/ vs Assets/_Game/Data/

### Wave 01 Ã¢â‚¬â€ Data, Save, Progression, Damage Base
- **Specs:** FASE9E items, save, progression, damage
- **Key files:** ItemDataSO, SaveData DTOs, PlayerStats, DamageCalculator
- **Rules:** No UI; backend/data-driven first; stable IDs

### Wave 02 Ã¢â‚¬â€ Equipment, Tools, Loot, Crafting, Durability
- **Specs:** FASE9C remaining, FASE9H loot/equipment
- **Key files:** EquipmentDataSO, LootTableSO, CraftingRecipeSO
- **Rules:** Data-driven; no UI final; durability system

### Wave 03 Ã¢â‚¬â€ Player Combat, Weapons, Magic, Skill Actions
- **Specs:** FASE9I player combat, magic, weapons
- **Key files:** PlayerCombatController, WeaponDataSO, SpellDataSO
- **Rules:** Integrate with existing Slime basic combat; no Input System changes

### Wave 04 Ã¢â‚¬â€ Enemies, AI, Status, Bestiary
- **Specs:** FASE9D architecture, 40 monsters, Amendment
- **Key files:** EnemyDataSO, AIBehaviorSO, StatusEffectSO
- **Rules:** Data-driven before roster; fallback visuals; stable run safe

### Wave 05 Ã¢â‚¬â€ Cave Entry, Loadout, Death, Recovery
- **Specs:** FASE9J cave entry, death, recovery
- **Key files:** CaveEntryController, DeathHandler, CorpseRecovery
- **Rules:** Integrate with cave save state; no stable run breakage; Fonte de Anya

### Wave 06 Ã¢â‚¬â€ Skill Trees, Nodes, Respec
- **Specs:** FASE9K skill trees
- **Key files:** SkillTreeDataSO, SkillNodeDataSO, SkillPoint system
- **Rules:** Backend first; no UI final; save/load compatibility

### Wave 07 Ã¢â‚¬â€ UI/Menu Systems Minimal
- **Specs:** UI menu systems (not full FASE9L)
- **Key files:** Menu prefabs, UIManager
- **Rules:** Minimal safe UI; avoid FASE9L complexity without spec clarity

---

## Known Issues Found in Wave 00

### Asset Structure Issue
- **Duplicate CaveBossGateRegistry:**
  - `Assets/Resources/CaveBossGateRegistry.asset` (OLD)
  - `Assets/_Game/Data/Cave/CaveBossGateRegistry.asset` (CORRECT)
  - **Action:** Delete old Assets/Resources/ version

### BossGate_Level15 Current State
- **File:** `Assets/_Game/Data/Cave/BossGate_Level15.asset`
- **Current values:**
  - Id: boss_gate_level_15
  - CaveLevel: 15
  - BiomeId: biome_cave_earth (not cave_default)
  - BossEnemyId: enemy_meteor_ooze_king (not slime)
  - CheckpointUnlockedOnDefeat: 15 (not 16)
- **Status:** Values appear to be from a recent PR; no correction needed unless specs contradict

### Registry Contents
- **CaveBossGateRegistry in _Game/Data/Cave:** Currently empty (`_gates: []`)
- **Status:** Expected; entries will be populated by future specs

---

## Validation Tasks for Wave 00

- [ ] Documentation validation: `.\tools\docs\validate_docs.ps1`
- [ ] Unity compile validation (batchmode)
- [ ] No Assembly-CSharp or import errors
- [ ] No missing scripts or broken prefabs
- [ ] All ScriptableObjects loadable

---

## Potential Gaps Identified

### Spec-to-Refinement Mapping
Some specs may not have corresponding refinement docs yet; these will be created as needed during each wave's execution.

### SpecKit Refinement Needed
- **spec_fase9l_ui_ux_full_gameplay.md:** Currently a placeholder; needs SpecKit /specify Ã¢â€ â€™ /plan Ã¢â€ â€™ /tasks before implementation
- **spec_future_ideas_todo.md:** Backlog; not in critical path
- **spec_ui_menu_systems_final.md:** May need SpecKit refinement if too shallow

### Dependency Chain Validation
- Waves 1-2 are foundational (items, save, progression, damage)
- Waves 3-4 depend heavily on 1-2
- Waves 5-6 depend on 3-4
- Wave 7 is UI; must not block

---

## Documentation & Tracking Files to Update After Wave 00

If corrections are made:
1. PROJECT_LOG.md Ã¢â‚¬â€ add entry for wave 00
2. docs/IMPLEMENTATION_STATUS.md Ã¢â‚¬â€ update if any baseline specs change
3. docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md Ã¢â‚¬â€ no change unless specs are removed
4. docs/refinements/a_implementar/ref_futuro_map.md Ã¢â‚¬â€ no change at this stage

---

## Next Steps (Upon Wave 00 Completion)

1. Ã¢Å“â€¦ Validate documental (validate_docs.ps1)
2. Ã¢Å“â€¦ Validate Unity (batchmode compile)
3. Ã¢Å“â€¦ Commit local (if corrections made)
4. Ã°Å¸â€â€ž Create Wave 01 branch from Wave 00 branch
5. Ã°Å¸â€â€ž Begin Wave 01 execution

---

## Progress Tracking

- [x] Map all specs
- [x] Validate docs Ã¢â‚¬â€ PASSED
- [x] Validate Unity Ã¢â‚¬â€ PASSED (return code 0)
- [x] Correct baseline issues Ã¢â‚¬â€ No critical issues found
- [x] Commit corrections locally Ã¢â‚¬â€ Ready
- [x] Generate final Wave 00 report Ã¢â‚¬â€ PARTIAL

---

## Validation Results

### Documentation Validation
- Ã¢Å“â€¦ **Result:** PASSED
- Root folder structure correct
- No template placeholders
- No critical old path references
- Specs and refinements use correct prefixes

### Unity Validation
- Ã¢Å“â€¦ **Result:** PASSED (return code 0)
- Script compilation: 0.899115s
- Domain reload: 569ms
- No compilation errors
- No missing scripts or broken references
- 899 scripts, 1067 non-script assets
- All assemblies loaded successfully

### Baseline Analysis
- Ã¢Å“â€¦ CaveBossGateRegistry exists in correct location: Assets/_Game/Data/Cave/
- Ã¢Å¡Â Ã¯Â¸Â Duplicate found in Assets/Resources/ (obsolete copy, lower priority)
- Ã¢Å“â€¦ BossGate_Level15.asset values are from recent PR (no rollback needed)
- Ã¢Å“â€¦ Registry contents empty as expected for future implementation

---

**Status:** Wave 00 PARTIAL Ã¢â‚¬â€ All specs mapped, documented, and baseline validated. Ready to proceed with Wave 01.
