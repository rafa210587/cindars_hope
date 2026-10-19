---
doc_type: game_rule
status: accepted
domain: inventory-gameplay
source_adrs:
  - ADR-0010
source_documents:
  - docs/decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Inventory/InventoryManager.cs
  - Assets/_Game/Scripts/Equipment/EquipmentSlot.cs
last_reviewed: 2026-06-13
---

# Inventory and Equipment Rules

> **Reconciled by [ADR-0010](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) (2026-06-13).**
> This rule was fully rewritten to match live runtime code and the binding FABLE v3.0
> decisions. The previous version (20 inventory slots, 1 accessory slot, 4 equipped max,
> flat 1-point durability loss) was obsolete and is superseded.

## Purpose

Defines inventory capacity, equipment slots, durability, upgrades, item pickup/drop, and item persistence for FABLE.

---

## Canonical Rules

### Rule: Inventory Slots and Capacity

- **Rule:** The player inventory holds **30 slots** (`InventoryManager` DefaultCapacity = MaxCapacity = 30):
  - Each slot holds 1-99 of the same stackable item.
  - Non-stackable items (weapons, armor) occupy 1 slot each.
  - A full inventory blocks new pickups (UI feedback shown).
- **Pouch expansion (note, Decision 2.12):** a **+6 pouch** capacity modifier is planned (F31). It is a future capacity bonus on top of the 30 base slots, not a change to the base capacity.
- **Constraint:** Capacity is enforced at pickup time; no overflow.
- **Applies to:** Item pickup, drop, and inventory UI.

### Rule: Equipment Slots

- **Rule:** Equipment has **9 slots** (`EquipmentSlot`), separate from inventory:
  - **LeftHand** — off-hand weapon / shield.
  - **RightHand** — main-hand weapon.
  - **Head**
  - **Chest**
  - **Legs**
  - **Boots**
  - **Ring1** — accessory-type.
  - **Ring2** — accessory-type.
  - **Accessory** — accessory-type (amulet / charm-style).
- **Accessory-type slots (Decision 2.12):** there are **3** accessory-type slots — two Ring slots (Ring1, Ring2) plus one Accessory slot for an amulet/charm. The earlier "1 accessory slot" was obsolete.
- **Equip requirement:** the item must be in the inventory to equip.
- **Unequip:** equipped items return to the inventory; the action fails if the inventory is full.
- **Equip eligibility (Decision 1.11 cross-reference):** items whose skill-tree tier became re-locked after a punitive respec stay in the inventory but are **non-equippable / non-usable** until the tier is re-unlocked.
- **Applies to:** Equip screen and equipment stat bonuses.

### Rule: Item Durability (derived by class × material)

- **Rule (Decision 2.11):** Maximum durability is **derived**, not flat:
  - Base durability: **80 for weapons, 150 for armor**.
  - Multiplied by material modifiers (per the §16 / §26 material bands).
  - Durability decreases with combat use (hit/defend); an item at durability 0 becomes unusable until repaired.
- **Repair:** restores durability for a gold cost (repair service).
- **Persistence:** current durability is saved per item instance.
- **Applies to:** Combat durability tracking and item state.

### Rule: Equipment Upgrade Cost (derived)

- **Rule (Decision 2.11):** Upgrade cost for tier **+N** is derived:
  - **Cost(+N) = (2N × material band) + (BV × 0.5N) gold**, where BV is the item's base value and the material band comes from §16 / §26.
- **Applies to:** Equipment progression and economy sinks.

### Rule: Item Pickup and Drop

- **Rule:**
  - **Pickup:** added to an existing matching stack first, otherwise to the first free slot; UI feedback shown. Requires space (or a non-full existing stack).
  - **Drop:** removed from inventory/equipment, placed at the player position as a lootable entity (short despawn delay allows re-pickup).
- **Applies to:** Gameplay item interaction.

### Rule: Item Acquisition Sources

- **Rule:** Items are obtained via:
  - Enemy drops (loot tables).
  - Chest / resource node rewards.
  - NPC shop purchase (gold).
  - Crafting / processing (materials + time).
- **Applies to:** Economy and item acquisition.

---

## Item Types and Properties

| Type | Stackable | Equippable | Has Durability | Max Quantity |
|---|---|---|---|---|
| Weapon | No | Yes (RightHand / LeftHand) | Yes (base 80 × material) | 1 equipped + 99 in inventory |
| Armor / Shield | No | Yes (Head/Chest/Legs/Boots/LeftHand) | Yes (base 150 × material) | 1 equipped + 99 in inventory |
| Accessory (Ring / Amulet / Charm) | No | Yes (Ring1 / Ring2 / Accessory) | No | up to 3 equipped + 99 in inventory |
| Consumable | Yes | No | No | 99 per stack |
| Material | Yes | No | No | 99 per stack |
| Quest Item | Yes | No | No | 1-99 per stack |

---

## Item Persistence

- **Rule:** Item state persists as IDs and simple values only:
  - Item ID (from the item catalog).
  - Durability remaining.
  - Stack count (if stackable).
- **No Unity refs:** item IDs only; resolved at load via the item catalog (per ADR-0006).
- **Applies to:** All save/load mechanics.

---

## Quick Reference (canonical values)

| Field | Canonical value | Source |
|---|---|---|
| Inventory slots | 30 | `InventoryManager` DefaultCapacity/MaxCapacity |
| Pouch expansion | +6 (future, F31) | Decision 2.12 |
| Equipment slots | 9 | `EquipmentSlot` |
| Accessory-type slots | 3 (Ring1, Ring2, Accessory) | Decision 2.12 |
| Weapon base durability | 80 × material | Decision 2.11 |
| Armor base durability | 150 × material | Decision 2.11 |
| Upgrade cost +N | (2N × material band) + (BV × 0.5N) gold | Decision 2.11 |

---

## Open Questions

- Final per-material band multipliers for durability (sourced from §16 / §26 tables).
- Whether broken items have a repair-count cap (current intent: unlimited repairs).
- Exact pouch-unlock condition and stacking with any future bag upgrades.

---

## Related ADRs

- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) (this rewrite)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (item persistence)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (equip/unequip events)

---

*Last Reviewed: 2026-06-13 (reconciled by ADR-0010 against FABLE v3.0 + live code)*  
*Source: ADR-0010, FABLE v3.0 (2.11, 2.12)*
