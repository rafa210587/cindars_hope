# WAVE_INTEGRATION_26 — Objective Type Coverage Matrix

Date: 2026-06-11

---

## Objective Type Status

| ObjectiveType | Enum Value | Event Source | Handler | Used in Chain | Available for Future |
|---------------|-----------|--------------|---------|--------------|---------------------|
| TalkToNpc | 0 | NpcInteractionStartedEvent | QuestService.OnNpcTalkedTo | No (debt) | YES — bridge wired |
| CollectItem | 3 | InventoryChangedEvent | QuestService.CheckObjectiveProgress | Q1 | YES |
| HarvestCrop | 8 | CropHarvestedEvent | QuestService.OnCropHarvested | No | YES — bridge wired |
| SellItem | 12 | EconomyTransactionCompletedEvent | QuestService.OnItemSold | Q2 | YES |
| CraftItem | 9 | ItemCraftedEvent | QuestService.OnItemCrafted | No | YES (WAVE15) |
| ReachCaveDepth | 30 | CaveLevelEnteredEvent | QuestService.OnCaveLevelEntered | Q3 | YES |
| DefeatEnemy | 20 | EnemyKilledEvent | QuestService.OnEnemyKilled | No | YES — bridge wired |

---

## Bridge Subscription Status

All subscriptions are in QuestProgressEventBridge.Subscribe():

| Event | Subscribed | Handler | Notes |
|-------|-----------|---------|-------|
| InventoryChangedEvent | YES (WAVE15) | OnInventoryChanged | CollectItem |
| ItemCraftedEvent | YES (WAVE15) | OnItemCrafted | CraftItem |
| CropHarvestedEvent | YES (WAVE26) | OnCropHarvested | HarvestCrop |
| EconomyTransactionCompletedEvent | YES (WAVE26) | OnEconomyTransactionCompleted | SellItem — filters sell + GoldDelta > 0 |
| NpcInteractionStartedEvent | YES (WAVE26) | OnNpcInteractionStarted | TalkToNpc |
| CaveLevelEnteredEvent | YES (WAVE26) | OnCaveLevelEntered | ReachCaveDepth |
| EnemyKilledEvent | YES (WAVE26) | OnEnemyKilled | DefeatEnemy (count-based) |

---

## TargetId Conventions

| Convention | ObjectiveType | Meaning |
|-----------|---------------|---------|
| "any" | SellItem, HarvestCrop, DefeatEnemy | Match any item/crop/enemy |
| "item_material_wood" | CollectItem | Specific item ID |
| "cave_level_1" | ReachCaveDepth | Level 1 entry (matches CaveLevel == 1) |
| "npc_thalindra" | TalkToNpc | Specific NPC ID |

---

## Objective Types Not Wired (Future Debt)

| ObjectiveType | Reason Not Wired |
|---------------|-----------------|
| BuyItem (11) | No buy event exists (purchase goes through shop modal, no standalone event) |
| PlantCrop (6) | No SeedPlantedEvent with item tracking |
| DeliverItem (4) | Delivery flow not implemented separately from CollectItem |
| DefeatBoss (22) | No CaveBossDefeatedEvent with DefeatBoss type — exists CaveBossDefeatedEvent but not wired |

These are documented as future quest type expansion when corresponding events are created.
