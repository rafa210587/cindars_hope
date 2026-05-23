# RUN — Wave 01: Data, Save, Progression, Damage Base

**Date:** 2026-05-23  
**Branch base:** wave/specs-overnight-00-plan  
**Branch created:** wave/specs-overnight-01-data-save-progression  
**Executor:** Claude Code  
**Status:** Planning phase

---

## Objective

Implement foundational data layer, save schema migration, player progression (level/XP/attributes), and complete damage/status system. This wave establishes the backend groundwork for all subsequent combat and equipment waves.

---

## Specs Targeted for Wave 01

| Spec | Status | Priority | Est. Size |
|---|---|---|---|
| spec_fase9e_item_taxonomy_ids.md | Reading | HIGH | M |
| spec_fase9e_item_examples_variations.md | Reading | HIGH | S |
| spec_fase9e_save_schema_migration.md | Reading | HIGH | L |
| spec_fase9e_player_level_up_progression.md | Reading | HIGH | M |
| spec_fase9e_damage_status_elements_complete.md | Reading | MEDIUM | M |

**Total estimated scope:** Large (1-2 day implementation)

---

## Refinements to Review

- `docs/refinements/a_implementar/` — check for corresponding refinements
- Historical refs: `spec_data_001`, `spec_save_001`, `spec_progression_001`, `spec_damage_001`

---

## Pre-Implementation Analysis

### Spec: Item Taxonomy IDs

**Source:** `docs_old/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md` (preserved)

**Key decisions already closed:**
- ItemCategory enum: Seed, Crop, Consumable, Material, Tool, Weapon, Magic, Ammo, Fish, Ore, Gem, MonsterDrop, Quest, KeyItem, Furniture, Misc
- ConsumableSubtype: Potion, Food, BuffFood
- ID prefixes: item_seed_, item_crop_, item_consumable_, item_material_, item_tool_, item_weapon_, item_magic_, item_ammo_, item_fish_, item_ore_, item_gem_, item_monster_drop_, item_quest_, item_key_, item_furniture_, item_misc_
- MaxStack rules: Seeds 99, Crops 99, Consumables 99, Ammo 99, Most others 1

**Dependencies:** spec_data_001 (IDs, registries, ScriptableObjects)

**Questions to clarify:**
- Will taxonomy apply to existing items (seeds, crops, fish) or only new items?
- Should existing ItemDataSO assets be refactored or wrapped?

### Spec: Item Examples & Variations

**Source:** Complements Item Taxonomy spec

**Key content:**
- 6 seed types + growth days + crop output
- 6 crop types + crafting recipes
- 8+ consumable/food types with effects
- 4 material types

**Dependencies:** spec_data_001, spec_inventory_001 (existing)

**Note:** Examples should be created as ItemDataSO assets in Assets/_Game/Data/Items/

### Spec: Save Schema Migration

**Source:** `docs_old/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md` (preserved)

**Key decisions:**
- SchemaVersion required in all saves (v1, v2, v3, v4, v5 planned)
- Tolerant loading with defaults for missing fields
- Incremental migration (version by version)
- No serialization of GameObject, Transform, MonoBehaviour, ScriptableObject, Sprite, Collider, Rigidbody

**Versions defined:**
- v1: current baseline (player, inventory, pickups)
- v2: Equipment, Hotbar, LeftHand, RightHand, ActiveSeed, selected consumable
- v3: Attributes, Level, XP, unspent points, ActiveStatuses
- v4: Farm/world state
- v5: Cave state MVP

**Critical:** Must not break existing saves

**Dependencies:** spec_save_001 (existing JSON save/load)

### Spec: Player Level Up Progression

**Source:** `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md` (preserved)

**Key decisions:**
- MaxLevel: 100
- AttributePoint: +1 per level
- SkillPoint: +1 per 3 levels
- XP curve: linear with 10-level multiplier blocks
- 6 attributes: Strength, Dexterity, Intelligence, Willpower, Constitution, Breath

**Formulas:**
- MeleeDamage = BaseDamage + Strength
- RangedDamage = BaseDamage + Dexterity
- MagicDamage = BaseDamage + Intelligence
- MaxMana = 10 + (Willpower * 4)
- MaxHP = 12 + (Constitution * 5)
- MaxStamina = 10 + (Breath * 2)

**MVP values:** All attributes start at 1, max 100

**Dependencies:** spec_progression_001 (existing partial implementation)

**Note:** Must integrate with existing player level system in partial spec

### Spec: Damage, Status & Elements Complete

**Source:** Newer spec (partially filled template)

**Pending work:**
- Status effect application logic (poison, burn, bleed)
- Poison: 3 turns, 2 HP/turn
- Burn: 2 turns, 3 HP/turn + fire immunity
- Bleed: 4 turns, 1 HP/turn
- Elemental weakness matrix
- Visual feedback (color overlay, particle FX)
- Status removal (cure potions, etc.)

**Key files to extend:**
- DamageCalculator.cs
- DamageResult.cs
- StatusEffect.cs (new)
- ElementalInteraction.cs (new)

**Dependencies:** spec_damage_001 (existing MVP formula)

---

## Dependency Chain

```
spec_data_001 (implemented)
  ↓
spec_fase9e_item_taxonomy_ids.md → spec_fase9e_item_examples_variations.md
  ↓
spec_inventory_001 (implemented)
  ↓
spec_save_001 (implemented, needs migration upgrade)
  ↓
spec_fase9e_save_schema_migration.md
  ↓
spec_progression_001 (implemented, partial)
  ↓
spec_fase9e_player_level_up_progression.md
  ↓
spec_damage_001 (implemented, MVP)
  ↓
spec_fase9e_damage_status_elements_complete.md
```

---

## Key Lacunas / Contradictions Found

1. **Item Taxonomy vs. Existing Items:** Spec assumes clean slate; reality has existing seeds, crops, fish already in code/assets. Need strategy to:
   - Either extend/refactor existing to match taxonomy
   - Or isolate new items under taxonomy only (risk of inconsistency)

2. **Save Migration v2 scope in Wave 01:** Save schema mentions Equipment, Hotbar but those aren't fully defined yet. Should we:
   - Implement v2 now (partial, forward-compatible)?
   - Implement v1 only + skeleton for v2?
   - Wait for Equipment spec (Wave 02)?

3. **Status Effects in Wave 01:** Damage/status spec lists poison, burn, bleed but no UI, no cures defined. Should we:
   - Implement backend only (data structures, application logic)?
   - Add placeholder UI?
   - Defer visual/cure until Wave 07?

4. **AttributePoint allocation:** Spec defines +1 per level but no UI for allocation. Should we:
   - Implement save/load of allocated points?
   - Add debug allocation UI?
   - Defer UI to Wave 07?

---

## Proposed Wave 01 Scope (SpecKit .plan)

### Architecture
1. **ItemDataSO Extension:** Extend existing ItemDataSO with:
   - Category enum
   - Subtype field
   - ID prefix validation
   - MaxStack per category
   - Flags (Consumable, Equipment, etc.)

2. **SaveData Evolution:** Implement SaveData v2 struct with:
   - SchemaVersion field (1-2 range)
   - Migration handler (v1→v2)
   - Equipment slots (DTO, no GameObject refs)
   - Hotbar state (DTO)
   - Selected consumable/seed

3. **PlayerStats & Progression:**
   - PlayerStats class with 6 attributes
   - LevelUpManager for XP→Level curve
   - AttributePoint allocation storage (save/load)
   - SkillPoint accumulation (for Wave 06)

4. **DamageCalculator Enhancement:**
   - Integrate attributes into damage formulas
   - StatusEffect system (apply/tick/remove)
   - Elemental interaction matrix (data-driven)

### Data & ScriptableObjects
- ItemDataSO (refactored, new fields)
- ItemCategorySO (registry for categories?)
- ItemExamplesSO (inventory of 20+ examples to create)
- SaveSchemaSO (version info, migration functions)
- PlayerStatsSO (base definitions)
- StatusEffectSO (poison, burn, bleed templates)

### Entities / Managers
- SaveDataMigration (static utility)
- LevelUpManager (calculation service)
- StatusEffectManager (apply/tick/track)
- DamageCalculator (enhanced with attrs + status)

### Events
- LevelUpEvent
- AttributeAllocatedEvent
- StatusAppliedEvent
- StatusRemovedEvent

### Save/Load
- PlayerSaveData.Level, XP, UnspentAttributePoints
- EquipmentSaveData (v2)
- HotbarSaveData (v2)
- StatusEffectData (active effects)

### Validation
- No breaking save v1 loads
- All saves accept SchemaVersion
- Attribute ranges validated (1-100)
- Status duration ticks down properly
- XP formula produces expected level at known XP

---

## Files Likely to Change/Create

### New Files
- Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs
- Assets/_Game/Scripts/Core/Progression/AttributeAllocationManager.cs
- Assets/_Game/Scripts/Combat/StatusEffectManager.cs
- Assets/_Game/Scripts/Combat/ElementalInteractionMatrix.cs
- Assets/_Game/Data/Items/ItemExamples_Seeds.asset (6 instances)
- Assets/_Game/Data/Items/ItemExamples_Consumables.asset (8+ instances)
- Assets/_Game/Data/Items/StatusEffects/*.asset (poison, burn, bleed templates)

### Modified Files
- Assets/_Game/Scripts/Core/Data/ItemDataSO.cs (add Category, MaxStack, etc.)
- Assets/_Game/Scripts/Save/SaveData.cs (add SchemaVersion, EquipmentData, HotbarData)
- Assets/_Game/Scripts/Save/SaveManager.cs (migration logic)
- Assets/_Game/Scripts/Player/PlayerData.cs (add Level, XP, AttributePoints)
- Assets/_Game/Scripts/Combat/DamageCalculator.cs (integrate attributes + status)
- Assets/_Game/Scripts/Combat/DamageResult.cs (add status field)

---

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Breaking existing saves | Implement migration first; test backward compat |
| Item taxonomy duplication | Create new ItemDataSO instances; don't refactor old |
| Status effects without UI | Implement backend only; mark UI as TODO |
| Attribute allocation without UI | Save/load mechanism only; debug UI minimal |
| XP formula not validated | Create XP test cases for known levels |

---

## Success Criteria

- [x] Item taxonomy defined in code (ItemCategory enum, ID prefixes)
- [ ] 20+ item examples created as ItemDataSO assets
- [ ] SaveData v1→v2 migration works, backward compatible
- [ ] Player level/XP system integrated, formula validated
- [ ] Attributes saved/loaded correctly
- [ ] Status effects apply, tick, and remove
- [ ] Damage formula uses attributes
- [ ] Unity validates (no errors)
- [ ] All tracking docs updated

---

## Status

**Phase:** Pre-implementation planning  
**Next step:** Rereading refinements, then begin implementation

