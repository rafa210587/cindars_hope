# WAVE_INTEGRATION_06A — Item Use / Action Binding Audit

Date: 2026-06-08
Source: Static asset scan + source code audit (pre-Play Mode)
Status: STATIC_AUDIT — Play Mode verification pending

## Methodology

Items were identified by scanning `Assets/_Game/Data/Items/` .asset files.
`ItemDataSO` fields inspected: Id, Category, UseKind, WeaponId, SpellId, IsEquippable, AllowedEquipmentSlots.
"Usable now" classification is based on static code analysis; some require Play Mode to confirm.

## Usability Classification Key

| Code | Meaning |
|---|---|
| YES_INVENTORY_REWARD_ONLY | Item adds to inventory as reward/material; no active use handler required |
| YES_TOOL_LEGACY_INFERRED | Tool wired via legacy EquipTool(id, ToolType, ToolTier); behavior inferred from EquipmentManager |
| YES_WEAPON_COMBAT | Has WeaponId or Weapon category; CombatManager/WeaponDatabase expected to resolve |
| YES_MAGIC_COMBAT | Has SpellId or Magic category; SpellDatabase expected to resolve |
| YES_CONSUMABLE | Has ConsumeFood or ConsumePotion UseKind; ConsumableHandler expected to resolve |
| NO_BINDING | No active use handler found in static scan |
| UNKNOWN_NEEDS_PLAYMODE | Exists in database but use behavior unclear without Play Mode |

---

## Audit Table

| ItemId | Exists in ItemDB | Category | UseKind | WeaponId | SpellId | Added to Inv | Equipable | Usable Now | Notes |
|---|---|---|---|---|---|---|---|---|---|
| item_seed_carrot | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | FarmPlot planting consumes from inventory |
| item_crop_carrot | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | FarmPlot harvest reward |
| item_seed_wheat | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same as carrot seed |
| item_seed_sunpepper | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same as carrot seed |
| item_seed_crystal_berry | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_seed_moonbean | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_seed_starroot | YES | Seed | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_crop_wheat | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | FarmPlot harvest reward |
| item_crop_sunpepper | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_crop_crystal_berry | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_crop_moonbean | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_crop_starroot | YES | Crop | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Same |
| item_tool_hoe_basic | UNKNOWN | — | — | — | — | SKIP | — | NO_BINDING | Not found in asset scan; check if item_shop_tool_hoe_basic is the canonical ID |
| item_tool_watering_can_basic | UNKNOWN | — | — | — | — | SKIP | — | NO_BINDING | Not found in asset scan; WateringCan tool type exists in ToolType enum |
| item_tool_axe_basic | UNKNOWN | — | — | — | — | SKIP | — | NO_BINDING | Not found in asset scan; Axe ToolType exists |
| item_tool_pickaxe_basic | UNKNOWN | — | — | — | — | SKIP | — | NO_BINDING | Not found in asset scan; Pickaxe ToolType exists |
| item_tool_sickle_basic | UNKNOWN | — | — | — | — | SKIP | — | NO_BINDING | Not found in asset scan; Sickle ToolType exists |
| item_tool_fishing_rod_basic | YES | Tool | UseTool (inferred) | — | — | YES | YES | YES_TOOL_LEGACY_INFERRED | EquipTool(FishingRod, Basic) expected to work; FishingSpot checks EquipmentManager.HasTool |
| item_shop_tool_hoe_basic | YES | Tool (shop) | — | — | — | YES | MAYBE | UNKNOWN_NEEDS_PLAYMODE | Confirmed in scan as shop item; may function as Hoe if Category=Tool and EquipTool resolves it |
| item_wood | UNKNOWN | — | — | — | — | SKIP | NO | NO_BINDING | Legacy alias; item_material_wood is the confirmed ID |
| item_stone | UNKNOWN | — | — | — | — | SKIP | NO | NO_BINDING | Legacy alias; item_material_stone is the confirmed ID |
| item_material_wood | YES | Material | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Tree/chop reward material |
| item_material_stone | YES | Material | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Rock/mine reward material |
| item_material_copper_ore | YES | Material | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Mining reward |
| item_material_iron_ore | YES | Material | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Mining reward |
| item_material_processed_wood | YES | Material | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | Crafting output |
| item_fiber | UNKNOWN | — | — | — | — | SKIP | NO | NO_BINDING | Not found in asset scan |
| item_forage_basic | UNKNOWN | — | — | — | — | SKIP | NO | NO_BINDING | Not found in asset scan |
| item_fish_basic | UNKNOWN | — | — | — | — | SKIP | NO | NO_BINDING | Legacy alias; item_fish_common is confirmed |
| item_fish_common | YES | Fish | None | — | — | YES | NO | YES_INVENTORY_REWARD_ONLY | FishingSpot reward; no active use handler |
| item_consumable_potion_hp_small | YES | Consumable | ConsumePotion | — | — | YES | NO | YES_CONSUMABLE | HungerRestore/StaminaRestore fields set; requires ConsumableHandler at runtime |
| item_consumable_food_bread | YES | Consumable | ConsumeFood | — | — | YES | NO | YES_CONSUMABLE | Same; check HungerManager wiring |
| item_consumable_food_carrot_stew | YES | Consumable | ConsumeFood | — | — | YES | NO | YES_CONSUMABLE | Same |
| item_weapon_bow_basic | YES | Weapon | EquipWeapon | bow_basic (inferred) | — | YES | YES | YES_WEAPON_COMBAT | WeaponDatabase must have matching entry; EquipItem(RightHand, instanceId) |
| item_ammo_arrow_basic | YES | Ammo | EquipAmmo | — | — | YES | YES | YES_WEAPON_COMBAT | Paired with bow; AmmoType binding |
| item_spell_fireball_test | YES | Magic | EquipSpell | — | fireball_test (inferred) | YES | YES | YES_MAGIC_COMBAT | SpellDatabase must have matching entry; test spell only |
| item_consumable_repair_kit_basic | YES | Consumable | — | — | — | YES | NO | YES_CONSUMABLE | DurabilityRestoreAmount set; EquipmentManager.RepairItem |

---

## Summary

| Category | Count | Ready for Smoke Test |
|---|---|---|
| Seeds (planting) | 7 | YES — FarmPlot wiring needed |
| Crops (harvest) | 6 | YES — FarmPlot reward |
| Tools (confirmed) | 2 (fishing rod + shop hoe) | PARTIAL — basic EquipTool only |
| Tools (missing IDs) | 5 (hoe, axe, pickaxe, watering can, sickle) | NO — IDs not in ItemDatabase |
| Materials | 5 | YES — inventory reward only |
| Fish | 1 | YES — inventory reward only |
| Consumables | 5+ | YES if consumer runtime wired |
| Weapon/Ammo | 2 | PARTIAL — requires WeaponDatabase binding |
| Magic | 1 | PARTIAL — requires SpellDatabase binding |

---

## Critical Gap: Tool Item IDs

5 tool IDs from the spec (`item_tool_hoe_basic`, `item_tool_watering_can_basic`, `item_tool_axe_basic`, `item_tool_pickaxe_basic`, `item_tool_sickle_basic`) were **not found** in the static asset scan.

`ToolType` enum has all 6 tool types (Hoe, Axe, Pickaxe, Sickle, FishingRod, WateringCan).
`EquipmentManager.EquipTool(string toolId, ToolType, ToolTier)` exists as a legacy method.

Possible causes:
1. Tool items use different IDs in ItemDatabase (e.g., `item_shop_tool_hoe_basic` for hoe).
2. Tool items exist as ScriptableObjects but with different file names than expected.
3. Tool items were never created in the ItemDatabase asset.

**Play Mode verification required**: open Unity, run ProvisionSmokeLoadout, check Console for "Skipped/unknown" list. If hoe/axe/pickaxe/watering can are all skipped, WAVE_INTEGRATION_06B is needed to register them.

---

## Recommendation

### If tools are confirmed missing (all 5 skipped in Play Mode):

Create `WAVE_INTEGRATION_06B_ITEM_USE_ACTION_BINDING_SPEC` to:
1. Register `item_tool_hoe_basic`, `item_tool_watering_can_basic`, `item_tool_axe_basic`, `item_tool_pickaxe_basic`, `item_tool_sickle_basic` in `ItemDatabase.asset` (via Unity Editor, not YAML).
2. Wire `AllowedEquipmentSlots` and `UseKind = UseTool` for each.
3. Confirm hotbar slots 0–4 are bindable.
4. Confirm FarmPlot.Interact() checks EquipmentManager.HasTool(ToolType.Hoe) correctly.

### If item_shop_tool_hoe_basic works as hoe (Play Mode confirms):

No new spec needed for hoe. Document canonical tool ID mapping in WAVE_INTEGRATION_06B only if other tools are missing.

### Combat and magic items:

`item_weapon_bow_basic` and `item_spell_fireball_test` are in the database but require WeaponDatabase/SpellDatabase resolution. These are NOT required for WAVE_INTEGRATION_06 smoke test (which covers farm/resource only). Defer to future combat integration wave.

---

## Item IDs for WAVE06 Smoke Test (minimum required)

These are the minimum IDs that must be provisioned for WAVE_INTEGRATION_06 acceptance:

| Item | Required for |
|---|---|
| item_seed_carrot | WAVE05 FarmPlot planting |
| item_tool_fishing_rod_basic OR equivalent | FishingSpot interaction (EquipmentManager.HasTool check) |
| Any material (item_material_wood) | WAVE06 TreeResource_01 reward confirmation |
| Any material (item_material_stone) | WAVE06 RockResource_01 reward confirmation |
| item_fish_common | WAVE06 LakeFishing/FishingSpot reward |

All 5 are confirmed in ItemDatabase and will be provisioned. WAVE06 smoke test can proceed.
