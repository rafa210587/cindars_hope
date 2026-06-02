---
doc_type: game_rule
status: accepted
domain: inventory-gameplay
source_adrs: []
source_documents:
  - docs/specs/implementados/SPEC_19_INVENTORY_MANAGEMENT.md
  - docs/specs/implementados/SPEC_20_EQUIPMENT_SYSTEM.md
last_reviewed: 2026-06-01
---

# Inventory and Equipment Rules

## Purpose

Defines inventory slots, capacity, equipment mechanics, and item persistence.

---

## Canonical Rules

### Rule: Inventory Slots and Capacity

- **Rule:** Player inventory has:
  - Max 20 slots total
  - Each slot can hold 1-99 of same item (stackable)
  - Non-stackable items (weapons, armor) occupy 1 slot each
  - Full inventory prevents new item pickup (shows UI message)

- **Applies to:** Item pickup, item drop, inventory UI
- **Constraint:** Capacity must be enforced at pickup time; no overflow allowed

### Rule: Equipment Slots

- **Rule:** Equipment slots are separate from inventory:
  - 1 Main Hand weapon slot
  - 1 Off Hand weapon/shield slot (optional)
  - 1 Armor slot (full body)
  - 1 Accessory slot (ring, amulet, etc.)

- **Total equipped at once:** 4 items max
- **Equip requirement:** Item must be in inventory to equip
- **Unequip:** Equipped items return to inventory; fails if inventory full

- **Applies to:** Equip screen, equipment stat bonuses

### Rule: Item Durability

- **Rule:** Equipment items have durability:
  - Max durability varies by item type (weapon 50-100, armor 100-200)
  - Durability decreases on hit/defend (1 point per attack/defense)
  - Durability decreases on equip change (no penalty; only during combat)
  - Item breaks at durability 0 (becomes unusable until repaired)

- **Repair:** Repair NPC restores to max durability for gold cost
- **Persistence:** Durability saved in save data

- **Applies to:** Combat durability tracking, item state

### Rule: Item Pickup and Drop

- **Rule:**
  - Pickup: Item added to first available inventory slot; shows UI feedback
  - Drop: Item removed from inventory or equipment; placed at player position; becomes lootable entity
  - Pickup requires inventory space (unless item is stackable and stack exists)

- **Pickup priority:** Stackable items added to existing stack first; new slot if no stack
- **Drop location:** Dropped at player center; 0.5 second despawn delay (allows re-pickup)

- **Applies to:** Gameplay item interaction

### Rule: Crafting and Shop Interaction

- **Rule:** Items can be obtained via:
  - Enemy drops (loot table RNG)
  - Chest/resource rewards
  - NPC shop purchase (costs gold)
  - Crafting (requires materials + gold, if implemented)

- **Shop:** Limited inventory; refreshes daily or manually
- **Crafting:** Not in MVP; planned for Batch 2

- **Applies to:** Economy, item acquisition

---

## Item Types and Properties

| Type | Stackable | Equippable | Durability | Max Quantity |
|---|---|---|---|---|
| Weapon | No | Yes | Yes | 1 (equipped) + 99 (inventory) |
| Armor | No | Yes | Yes | 1 (equipped) + 99 (inventory) |
| Accessory | No | Yes | No | 1 (equipped) + 99 (inventory) |
| Consumable | Yes | No | No | 99 per stack |
| Material | Yes | No | No | 99 per stack |
| Quest Item | Yes | No | No | 1-99 per stack |

---

## Item Persistence

- **Rule:** Item state persists in save:
  - Item ID (from ItemDatabaseSO)
  - Durability remaining
  - Stack count (if stackable)
  - Enchantments (if implemented)

- **Save format:** ItemDTO with itemId + durability + count
- **No Unity refs:** Item IDs only; resolved at load time via ItemDatabaseSO

- **Applies to:** All save/load mechanics

---

## Open Questions

- Can items be sold back to shop? (Current: not in MVP; planned for Batch 2)
- What is the durability loss rate in combat? (Current: 1 point per attack/defense)
- Can broken items be repaired multiple times? (Current: infinite repairs; no durability cap)
- Are there item rarity tiers? (Current: no tiers; all items same rarity; Batch 2 may add)

---

## Related ADRs

- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (item persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (equip/unequip events)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: SPEC_19, SPEC_20*
