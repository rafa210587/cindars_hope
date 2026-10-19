# WAVE_INTEGRATION_15 — Quest Catalog Smoke Test

Date: 2026-06-10

---

## Item IDs Audited

| Item ID | Found In | Status |
|---------|----------|--------|
| item_material_wood | DebugLoadoutProvisioner.cs, ItemDataInitializer.cs | REFERENCED — exists in ItemDatabase |
| item_material_stone | DebugLoadoutProvisioner.cs, ValidateShopPriceData.cs | REFERENCED — exists in ItemDatabase |
| item_basic_repair_kit | NOT FOUND in any script | NOT FOUND — quest_repair_kit_for_town blocked |
| item_seed_turnip / item_turnip_seed | NOT FOUND | NOT FOUND — not needed for fallback quest |

---

## Quest Catalog

| QuestId | Category | Giver | Objectives | Rewards | Status |
|---------|----------|-------|-----------|---------|--------|
| quest_first_supplies_for_cindar | Tutorial | npc_thalindra | CollectItem wood x2, CollectItem stone x2 | Gold 50, Flag: flag_first_town_supplies_delivered | ACTIVE (smoke test) |
| quest_repair_kit_for_town | Tutorial | npc_thalindra | CraftItem repair_kit x1, DeliverItem repair_kit x1 | Gold 100, Flag: flag_repair_kit_delivered | DEFERRED (recipe not found) |

---

## Objective Types

| Type | Status | Trigger |
|------|--------|---------|
| CollectItem | ACTIVE | InventoryChangedEvent → QuestProgressEventBridge |
| DeliverItem | DEFERRED | Requires manual turn-in; not auto-tracked |
| CraftItem | ACTIVE | ItemCraftedEvent → QuestProgressEventBridge.OnItemCrafted |
| TalkToNpc | DEFERRED | No NpcInteractionStartedEvent mapping in bridge |
| DefeatEnemy | DEFERRED | Combat runtime not ready (KILL_OBJECTIVE_DEFERRED) |
| ReachLocation | DEFERRED | No location event bridge |
| UseItem | DEFERRED | No UseItem event bridge |

---

## Recipe IDs Audited

| Recipe ID | Found In | Status |
|-----------|----------|--------|
| recipe_workbench_processed_wood | CraftingRecipeInitializer.cs | FOUND |
| recipe_forge_iron_sword | CraftingRecipeInitializer.cs | FOUND |
| recipe_basic_repair_kit | NOT FOUND | NOT FOUND — fallback quest used |
