# SPEC: Tools, Equipment, Hotbar, Progression & Damage (PR-101 to PR-130)

**Status**: Implementado Parcial  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-101 to PR-130

---

## Summary

Comprehensive system overhaul: tools (axe, fishing rod) with tiers, equipment hotbar, player progression (XP/level), damage formula, and item taxonomy refactor.

## Scope

- ✅ ToolDataSO with tier system
- ✅ EquipmentManager (equip/unequip tools)
- ✅ Hotbar with 6 slots (F1-F6)
- ✅ Item taxonomy and ID system (item_*, seed_*, ore_*, etc.)
- ✅ Inventory overhaul with ItemStackSaveData
- ✅ PlayerProgression (XP, level, stat points)
- ✅ DamageCalculator (base formula)
- ✅ Tool gating (TreeNode requires axe, FishingSpot requires rod)
- ✅ Hotbar seed gating (FarmPlot uses selected seed)
- ⚠️ Attribute allocation UI (not implemented)
- ⚠️ Status/element system (contracts only)

## Architecture

### Tools & Equipment
- **ToolDataSO**: ID, display name, tier (Basic/Standard/Fine), durability, stat bonuses
- **EquipmentManager**: Current equipped tool, swap via hotbar
- **Tool Tiers**: Basic → Standard → Fine progression
- **Gating**: Systems check tool tier before allowing actions

### Hotbar System
- **6 Slots**: F1-F6 keybinds
- **Contents**: Tools, consumables, seeds
- **Equip**: F key swaps equipped tool
- **Persistence**: Hotbar state saved/loaded

### Item Taxonomy
- **Format**: `category_name` (e.g., `item_wood`, `seed_wheat`, `ore_copper`)
- **ItemDataSO**: Base contract with ID, display name, icon
- **Subclasses**: SeedDataSO, ToolDataSO, WeaponDataSO, ConsumableDataSO
- **Registry**: Central database for all items

### Player Progression
- **XP System**: Gained from actions (farming, fishing, combat)
- **Levels**: 1-99, XP threshold per level
- **Stat Points**: 5 base points per level
- **Attributes**: Strength, Vitality, Dexterity, Intelligence
- **Distribution**: Points allocated to attributes (UI pending)

### Damage Formula
- **DamageCalculator**: Static damage calculation
- **Formula**: Base damage + (str × 0.5) + weapon_bonus
- **DamageResult**: Damage value, hit, crit, element
- **Status Effects**: Contracts for poison, burn, etc. (implementation pending)

## Key Files

- `Assets/_Game/Scripts/Tools/ToolDataSO.cs` — Tool contracts
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs` — Tool equipping
- `Assets/_Game/Scripts/UI/Hotbar/HotbarSlot.cs` — Slot data
- `Assets/_Game/Scripts/UI/Hotbar/HotbarManager.cs` — Hotbar system
- `Assets/_Game/Scripts/Core/Data/ItemDataSO.cs` — Item base contracts
- `Assets/_Game/Scripts/Player/Progression/PlayerProgression.cs` — XP/level/stats
- `Assets/_Game/Scripts/Combat/DamageCalculator.cs` — Damage formula
- `Assets/_Game/Scripts/Core/Events/PlayerActionFeedbackEvent.cs` — Action feedback

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | ToolDataSO defines tools with tiers | ✅ |
| 2 | EquipmentManager equips/unequips | ✅ |
| 3 | Hotbar shows 6 slots | ✅ |
| 4 | F key swaps equipped tool | ✅ |
| 5 | Hotbar persists save/load | ✅ |
| 6 | Item taxonomy IDs consistent | ✅ |
| 7 | TreeNode requires axe | ✅ |
| 8 | FishingSpot requires rod | ✅ |
| 9 | FarmPlot uses selected seed | ✅ |
| 10 | XP/level system works | ✅ |
| 11 | Stat points awarded | ✅ |
| 12 | DamageCalculator formula applied | ✅ |

## Pending

- Attribute allocation UI and input
- Full status/element implementation
- Item variations (e.g., Axe Basic vs. Axe Standard)
- Damage formula balancing
- Combat integration (melee attacks using formula)
- Play Mode validation

## Next Steps

Continue to Cave Procedural Runtime (PR-141 to PR-153).
