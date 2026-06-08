# WAVE_INTEGRATION_06A Debug Loadout — Execution Report

Date: 2026-06-08
Branch: dev
Status: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

## Preflight

- Branch: dev
- Working tree: M Assets/_Game/Scenes/FarmScene.unity (pre-existing, expected)
- WAVE_INTEGRATION_06 baseline: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED (P1 hotfix applied)
- Purpose: create debug loadout provisioner so player can receive smoke-test items in Play Mode

## Design Question Answer

"Dar todos os itens ao player já permite testar tudo?"

| Context | Answer |
|---|---|
| WAVE06 resources (Tree/Rock/Forage) | YES — FarmResourceInteractable uses InventoryManager.AddItem; no tool required |
| WAVE05 crops | PARTIALLY — needs item_seed_carrot + FarmPlot wiring; seed is provisioned |
| Fishing / LakeFishing | PARTIALLY — FishingSpot may require fishing rod tool in inventory; item_tool_fishing_rod_basic provisioned |
| Combat | NO — item must be equipped and WeaponId/SpellId must resolve in database; audit required |
| Tools | PARTIALLY — legacy EquipTool(toolId, ToolType, ToolTier) works for known tools; item IDs may differ from spec |
| Consumables | YES if consumer runtime is connected (HP potions have HungerRestore/StaminaRestore) |

## Files Created

| File | Purpose |
|---|---|
| Assets/_Game/Scripts/Editor/Validation/DebugLoadoutProvisioner.cs | Editor menu provisioner — TODO_INTEGRATION_NOT_FINAL |

## Editor Menus Added

| Menu | Action |
|---|---|
| CindarsHope/Integration/Debug/Provision Farm Smoke Loadout | Adds smoke loadout items, sets hotbar 0–5, equips first known tool |
| CindarsHope/Integration/Debug/Provision All Known Items (Debug) | Iterates ItemDatabase.All and adds 1 of each item |

## Provisioner Logic

1. Checks `Application.isPlaying` — aborts with warning if not in Play Mode.
2. Gets `GameBootstrap.Instance.InventoryManager` — aborts if null/not initialized.
3. For each entry in SmokeLoadout: calls `inv.IsKnownItem(itemId)` — skips with log if unknown.
4. Calls `inv.AddItem(itemId, amount)` — logs warning if returns false (full/max stack).
5. Sets hotbar slot via `SaveManager.HotbarState.SetSlot()` only for items successfully added.
6. Calls `EquipmentManager.EquipTool(id, ToolType, ToolTier.Basic)` for first known tool in inventory.

## Item IDs Attempted

### Confirmed exists in ItemDatabase (from static asset scan)

| ItemId | Confirmed |
|---|---|
| item_seed_carrot | YES |
| item_crop_carrot | YES |
| item_seed_wheat, item_seed_sunpepper | YES |
| item_tool_fishing_rod_basic | YES |
| item_shop_tool_hoe_basic | YES (shop alias) |
| item_material_wood | YES |
| item_material_stone | YES |
| item_material_copper_ore, item_material_iron_ore | YES |
| item_fish_common | YES |
| item_consumable_potion_hp_small | YES |
| item_consumable_food_bread | YES |
| item_weapon_bow_basic | YES |
| item_ammo_arrow_basic | YES |
| item_spell_fireball_test | YES |

### Unknown — will be skipped at runtime (logged as "Skipped/unknown")

| ItemId | Note |
|---|---|
| item_tool_hoe_basic | Not found in asset scan; may need to use item_shop_tool_hoe_basic |
| item_tool_watering_can_basic | Not found in asset scan |
| item_tool_axe_basic | Not found in asset scan |
| item_tool_pickaxe_basic | Not found in asset scan |
| item_tool_sickle_basic | Not found in asset scan |
| item_wood | Not found as primary ID; item_material_wood confirmed |
| item_stone | Not found as primary ID; item_material_stone confirmed |
| item_fiber | Not found in asset scan |
| item_forage_basic | Not found in asset scan |
| item_fish_basic | Not found; item_fish_common confirmed |

## Hotbar Configuration (intended — depends on runtime IsKnownItem)

| Slot | Item ID | Status |
|---|---|---|
| 0 | item_tool_hoe_basic | WILL SKIP if unknown; no fallback set |
| 1 | item_tool_watering_can_basic | WILL SKIP if unknown |
| 2 | item_tool_axe_basic | WILL SKIP if unknown |
| 3 | item_tool_pickaxe_basic | WILL SKIP if unknown |
| 4 | item_tool_fishing_rod_basic | EXPECTED PASS — confirmed in ItemDatabase |
| 5 | item_seed_carrot | EXPECTED PASS — confirmed in ItemDatabase |

## Equipment Configuration

Legacy `EquipTool(toolId, ToolType, ToolTier.Basic)` attempted for first known tool found in inventory order:
1. item_tool_hoe_basic (Hoe)
2. item_shop_tool_hoe_basic (Hoe)
3. item_tool_axe_basic (Axe)
4. item_tool_fishing_rod_basic (FishingRod)
5. item_tool_pickaxe_basic (Pickaxe)
6. item_tool_watering_can_basic (WateringCan)

Expected: `item_tool_fishing_rod_basic` or `item_shop_tool_hoe_basic` will be equipped if others are unknown.

## Build Validation

- Assembly-CSharp: PASS (exit code 0, 0E/0W)
- Assembly-CSharp-Editor: PASS (exit code 0, 0E/3W pre-existing — unchanged from WAVE06 P1 hotfix)
- Docs validation: EXPECTED_FAIL_LEGACY_ONLY (pre-existing)

## Testing Quality Gate

Changed runtime code: NO (DebugLoadoutProvisioner.cs is Editor-only; #if UNITY_EDITOR not needed because it's in Editor/ folder)
Changed deterministic logic: NO
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: NO
Justification: Editor-only debug utility. Core InventoryManager.AddItem already has EditMode tests. No gameplay logic changed.
Manual Play Mode scenario: docs/validation/WAVE_INTEGRATION_06A_HUMAN_PLAYMODE_CHECKLIST.md
Residual risk: Play Mode not yet executed. Item IDs for tools (hoe, axe, pickaxe, etc.) may not exist in ItemDatabase; hotbar slots 0–3 may remain empty.

## Honest Status Rationale

Status: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

Not advancing to WAVE_INTEGRATION_07 yet because:
1. Human must regenerate FarmScene via CreateMvpFarmScene generator.
2. Human must run smoke loadout provisioner in Play Mode.
3. Human must confirm WAVE05 crops (FarmPlot arar/molhar/plantar/colher) work.
4. Human must confirm WAVE06 resources (Tree/Rock/Forage) deplete and reward correctly.
5. Human must confirm hotbar and equipment are usable.
6. Debug audit report (WAVE_INTEGRATION_06A_DEBUG_LOADOUT_ITEM_USE_AUDIT.md) must be reviewed before WAVE_INTEGRATION_07 gate.

## Recommendation: WAVE_INTEGRATION_06B

See audit report for detailed binding status. If audit confirms that:
- Multiple tool IDs (hoe/axe/pickaxe/watering can) are absent from ItemDatabase
- HotbarState slots remain empty for tools
- EquipmentManager.EquipTool fails silently for all candidates

Then WAVE_INTEGRATION_06B is needed to either:
1. Register missing tool items in ItemDatabase with correct tool IDs, OR
2. Wire hotbar/equipment to new item IDs.

If tool IDs are found under different names (e.g., item_shop_tool_hoe_basic works as a Hoe), WAVE_INTEGRATION_06B may be unnecessary.
