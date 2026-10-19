# WAVE_INTEGRATION_26 — Decision: Questline Expansion + Objective Variety

Date: 2026-06-11
Status: BUILD_VALIDATED

---

## Sources Read

- CLAUDE.md
- docs/project/CURRENT_STATE.md
- Assets/_Game/Scripts/Quests/Runtime/QuestService.cs
- Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs
- Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs
- Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs
- Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs
- Assets/_Game/Scripts/Quests/QuestDefinition.cs
- Assets/_Game/Scripts/Quests/QuestStepDefinition.cs (QuestObjective class)
- Assets/_Game/Scripts/Quests/QuestCategoryType.cs (QuestObjectiveType enum)
- Assets/_Game/Scripts/Quests/Rewards/QuestRewardDefinition.cs
- Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs
- Assets/_Game/Scripts/Core/Events/QuestRuntimeEvents.cs
- Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs
- Assets/_Game/Scripts/Core/Events/CaveLevelEnteredEvent.cs
- Assets/_Game/Scripts/Core/Events/EnemyKilledEvent.cs
- Assets/_Game/Scripts/Core/Events/CropHarvestedEvent.cs
- Assets/_Game/Scripts/Core/Events/EconomyTransactionCompletedEvent.cs
- Assets/_Game/Scripts/Core/Events/CraftingEvents.cs
- Assets/_Game/Scripts/NPC/NpcTownRosterRegistry.cs
- Assets/_Game/Scripts/UI/Quests/Runtime/QuestLogPanelController.cs
- Assets/_Game/Scripts/UI/Quests/Runtime/QuestOfferPanelController.cs

---

## Gates

| Gate | Status |
|------|--------|
| WAVE20 gate | SATISFIED |
| WAVE21 gate | SATISFIED |
| WAVE22 gate | SATISFIED |
| WAVE23 gate | SATISFIED (GameplayFeedbackService available) |
| WAVE24 gate | SATISFIED (FarmDailyGoalService available) |
| WAVE25 gate | SATISFIED (NpcScheduleService + 23 NPCs canonicos) |
| P0/P1 abertos | 0 |

---

## Existing Quest System Audit Summary

| Component | Status | Notes |
|-----------|--------|-------|
| QuestService | EXISTS — reused | accept/progress/turn-in/idempotent rewards |
| QuestRegistry | EXISTS — extended | +2 new quests registered |
| QuestStateSection | EXISTS — reused | persists quest records in memory |
| QuestProgressEventBridge | EXISTS — extended | +5 new event subscriptions |
| QuestRuntimeBootstrap | EXISTS — reused | RuntimeInitializeOnLoadMethod singleton |
| QuestGiverInteractable | EXISTS — reused | npc_thalindra wired; npc_pip/maelor need scene wiring |
| QuestOfferPanelController | EXISTS — reused | IMGUI headless |
| QuestLogPanelController | EXISTS — reused | IMGUI, J key |
| Save/Load | EXISTS — reused | WAVE18 wired quest state to SaveManager |
| Reward idempotency | EXISTS — GrantedRewardIds guard in QuestStateRecord |

---

## Questline Decisions

Mini-questline de 3 quests com chain de prerequisitos:

| Order | Quest ID | Giver | Objective Type | Prerequisite |
|-------|----------|-------|----------------|-------------|
| 1 | quest_first_supplies_for_cindar | npc_thalindra | CollectItem (wood x2, stone x2) | none |
| 2 | quest_tools_for_the_town | npc_pip | SellItem (any x1) | quest_first_supplies_for_cindar |
| 3 | quest_echo_from_the_cave | npc_maelor | ReachCaveDepth (level 1) | quest_tools_for_the_town |

Quest 1 ja existia (WAVE15). Quests 2 e 3 criadas no WAVE26.

---

## Objective Type Decisions

| ObjectiveType | Handler | Event Source | Decision |
|---------------|---------|--------------|----------|
| CollectItem | QuestService.CheckObjectiveProgress | InventoryChangedEvent | EXISTS (WAVE15) |
| CraftItem | QuestService.OnItemCrafted | ItemCraftedEvent | EXISTS (WAVE15) |
| HarvestCrop | QuestService.OnCropHarvested | CropHarvestedEvent | NEW (WAVE26) — event exists (WAVE24) |
| SellItem | QuestService.OnItemSold | EconomyTransactionCompletedEvent | NEW (WAVE26) — event exists (WAVE24) |
| TalkToNpc | QuestService.OnNpcTalkedTo | NpcInteractionStartedEvent | NEW (WAVE26) — event exists (WAVE25) |
| ReachCaveDepth | QuestService.OnCaveLevelEntered | CaveLevelEnteredEvent | NEW (WAVE26) — event exists (WAVE16) |
| DefeatEnemy | QuestService.OnEnemyKilled | EnemyKilledEvent | NEW (WAVE26) — event exists (WAVE17) |

HarvestCrop, TalkToNpc, DefeatEnemy: adicionados ao bridge mas NAO usados nas 3 quests da chain MVP.
Ficam disponiveis para quests futuras sem novo codigo.

---

## NPC Hook Strategy

- npc_thalindra: QuestGiverInteractable ja wired com quest_first_supplies_for_cindar (WAVE15)
- npc_pip: registrado como giver de quest_tools_for_the_town; precisa de QuestGiverInteractable no TownScene (SCENE_WIRING_DEBT)
- npc_maelor: registrado como giver de quest_echo_from_the_cave; precisa de QuestGiverInteractable no TownScene (SCENE_WIRING_DEBT)

SCENE_WIRING_DEBT: npc_pip e npc_maelor precisam de QuestGiverInteractable em TownScene — requer wiring humano no Unity Editor.

---

## Reward Strategy

Todas as recompensas sao idempotentes via GrantedRewardIds:
- Q1: Gold 50 + flag_first_town_supplies_delivered (WAVE15)
- Q2: Gold 30 + flag_town_tools_funded (WAVE26 new)
- Q3: Gold 60 + flag_cave_first_explored (WAVE26 new)

Recompensa nao duplica apos save/load (GrantedRewardIds persiste na QuestStateSection).

---

## Save/Load Strategy

- QuestStateSection persiste estado das quests (Active/ReadyToComplete/Completed)
- QuestStateRecord persiste: ObjectiveStates, GrantedRewardIds, GrantedFlagIds
- WAVE18 ja wireou CaptureSaveData/RestoreFromSaveData no SaveManager
- Prerequisitos sao verificados ao runtime via QuestStateSection (completed quests)

---

## Design/Direction Compliance Matrix

| Requirement | Status |
|-------------|--------|
| NAO recriar QuestService | COMPLIANT — extended only |
| NAO recriar QuestRegistry | COMPLIANT — extended only |
| NAO recriar QuestLog/Offer | COMPLIANT — reused |
| NAO criar nova main quest/cutscene | COMPLIANT |
| Prerequisito chain funciona | COMPLIANT — PrerequisiteQuestIds field |
| Save/load preserva progresso | COMPLIANT — WAVE18 |
| Reward idempotente | COMPLIANT — GrantedRewardIds |
| Minimo 3 quests | COMPLIANT — 3 quests |
| Minimo 3 objective types | COMPLIANT — CollectItem + SellItem + ReachCaveDepth |
| NPC turn-in Thalindra | COMPLIANT — WAVE15 |
| using CindarsHope.Core nos novos handlers | COMPLIANT |
| IDs reais do repo | COMPLIANT — todos IDs verificados |
