# WAVE_INTEGRATION_26 — Questline Definition Matrix

Date: 2026-06-11

---

## Quest Chain: Primeiros Passos em Cindar's Hope

| Field | Quest 1 | Quest 2 | Quest 3 |
|-------|---------|---------|---------|
| QuestId | quest_first_supplies_for_cindar | quest_tools_for_the_town | quest_echo_from_the_cave |
| DisplayName | Suprimentos para Cindar | Ferramentas para a Cidade | Eco das Cavernas |
| Category | Tutorial | Side | Side |
| Giver NPC ID | npc_thalindra | npc_pip | npc_maelor |
| Prerequisite | none | quest_first_supplies_for_cindar | quest_tools_for_the_town |
| Trackable | true | true | true |
| WAVE Introduced | WAVE15 | WAVE26 | WAVE26 |

---

## Objectives Per Quest

### Quest 1: quest_first_supplies_for_cindar

| ObjectiveId | ObjectiveType | TargetId | RequiredAmount | Handler |
|-------------|---------------|----------|----------------|---------|
| obj_collect_wood_x2 | CollectItem (3) | item_material_wood | 2 | QuestService.CheckObjectiveProgress via InventoryChangedEvent |
| obj_collect_stone_x2 | CollectItem (3) | item_material_stone | 2 | QuestService.CheckObjectiveProgress via InventoryChangedEvent |

### Quest 2: quest_tools_for_the_town

| ObjectiveId | ObjectiveType | TargetId | RequiredAmount | Handler |
|-------------|---------------|----------|----------------|---------|
| obj_sell_crop_x1 | SellItem (12) | any | 1 | QuestService.OnItemSold via EconomyTransactionCompletedEvent (sell + GoldDelta > 0) |

### Quest 3: quest_echo_from_the_cave

| ObjectiveId | ObjectiveType | TargetId | RequiredAmount | Handler |
|-------------|---------------|----------|----------------|---------|
| obj_enter_cave_level1 | ReachCaveDepth (30) | cave_level_1 | 1 | QuestService.OnCaveLevelEntered via CaveLevelEnteredEvent |

---

## Rewards Per Quest

| Quest | RewardId | RewardType | Quantity | FlagId | Idempotency |
|-------|----------|------------|----------|--------|-------------|
| Q1 | reward_supply_quest_gold | Gold | 50 | — | TrackByRewardId |
| Q1 | reward_supply_quest_flag | QuestFlagGrant | — | flag_first_town_supplies_delivered | TrackByFlagId |
| Q2 | reward_tools_quest_gold | Gold | 30 | — | TrackByRewardId |
| Q2 | reward_tools_quest_flag | QuestFlagGrant | — | flag_town_tools_funded | TrackByFlagId |
| Q3 | reward_cave_quest_gold | Gold | 60 | — | TrackByRewardId |
| Q3 | reward_cave_quest_flag | QuestFlagGrant | — | flag_cave_first_explored | TrackByFlagId |

---

## Save/Load Validation

| State | Persisted | Via |
|-------|-----------|-----|
| Quest accepted (Active) | YES | QuestStateRecord.State = Active |
| Objective progress | YES | QuestStateRecord.ObjectiveStates |
| Quest ready (ReadyToComplete) | YES | QuestStateRecord.State |
| Quest completed | YES | QuestStateRecord.State = Completed |
| Reward granted | YES | QuestStateRecord.GrantedRewardIds |
| Flag granted | YES | QuestStateRecord.GrantedFlagIds |
| Prerequisite chain unlock | YES | Derived from completed QuestStates at runtime |

---

## NPC Scene Wiring Status

| NPC | QuestId Offered | QuestGiverInteractable in Scene | Status |
|-----|-----------------|----------------------------------|--------|
| npc_thalindra | quest_first_supplies_for_cindar | YES (WAVE15) | READY |
| npc_pip | quest_tools_for_the_town | NO — SCENE_WIRING_DEBT | Human must add QuestGiverInteractable with _offeredQuestIds = [quest_tools_for_the_town] to Pip NPC in TownScene |
| npc_maelor | quest_echo_from_the_cave | NO — SCENE_WIRING_DEBT | Human must add QuestGiverInteractable with _offeredQuestIds = [quest_echo_from_the_cave] to Maelor NPC in TownScene |

---

## Prerequisite Unlock Flow

Quest 2 becomes available when:
- quest_first_supplies_for_cindar.State == Completed
- QuestGiverInteractable on npc_pip evaluates PrerequisiteQuestIds and shows offer

Quest 3 becomes available when:
- quest_tools_for_the_town.State == Completed
- QuestGiverInteractable on npc_maelor evaluates PrerequisiteQuestIds and shows offer

Note: PrerequisiteQuestIds is stored in QuestDefinition but prerequisite enforcement at offer time
requires QuestGiverInteractable to check QuestService.GetQuestState for prerequisites.
Current QuestGiverInteractable checks if quest is not yet started — prerequisite UI display
depends on scene wiring. Full prerequisite enforcement is PREREQUISITE_UI_DEBT for future.
