# WAVE_INTEGRATION_26 — NPC Quest Hook Matrix

Date: 2026-06-11

---

## NPC Quest Hook Status

| NpcId | NPC Name | Quest Offered | Turn-In NPC | QuestGiverInteractable | Scene Status | Dialogue Option |
|-------|----------|---------------|-------------|------------------------|-------------|-----------------|
| npc_thalindra | Thalindra Veu-de-Lua | quest_first_supplies_for_cindar | npc_thalindra | YES (WAVE15) | WIRED in TownScene | Offer + TurnIn via QuestGiverInteractedEvent |
| npc_pip | Pip Semente-Solta | quest_tools_for_the_town | npc_pip | SCENE_WIRING_DEBT | NOT wired — human must add | Add QuestGiverInteractable; _npcId = "npc_pip"; _offeredQuestIds = ["quest_tools_for_the_town"] |
| npc_maelor | Maelor Cinza | quest_echo_from_the_cave | npc_maelor | SCENE_WIRING_DEBT | NOT wired — human must add | Add QuestGiverInteractable; _npcId = "npc_maelor"; _offeredQuestIds = ["quest_echo_from_the_cave"] |

---

## How QuestGiverInteractable Works

1. Player enters NPC interaction range
2. QuestGiverInteractable.Interact() is called (IInteractable)
3. Publishes QuestGiverInteractedEvent(npcId, questId, mode)
   - Mode = Offer: quest not yet started
   - Mode = TurnIn: quest ReadyToComplete
   - Mode = NoQuest: quest completed or no quest
4. QuestOfferPanelController receives event:
   - Offer mode: shows quest offer dialog, Accept button calls QuestService.AcceptQuest
   - TurnIn mode: calls QuestService.TurnIn and applies rewards

---

## Runtime Event Flow

```
NPC Interaction -> QuestGiverInteractable.Interact()
  -> GameEventBus.Publish(QuestGiverInteractedEvent)
    -> QuestOfferPanelController.OnQuestGiverInteracted()
      -> QuestService.AcceptQuest(questId) [if Offer]
        -> GameEventBus.Publish(QuestAcceptedEvent)
        -> QuestLogPanelController updates display
      -> QuestService.TurnIn(questId) [if TurnIn]
        -> GameEventBus.Publish(QuestCompletedEvent)
        -> GameEventBus.Publish(QuestRewardClaimedEvent)
        -> Rewards applied via QuestGoldAdapter + QuestInventoryAdapter
```

---

## Prerequisite Enforcement Status

| Enforcement Point | Status | Notes |
|-------------------|--------|-------|
| QuestDefinition.PrerequisiteQuestIds | POPULATED | Q2 prereq = Q1; Q3 prereq = Q2 |
| QuestGiverInteractable prerequisite check | PARTIAL — shows offer if quest not started | Does not enforce PrerequisiteQuestIds at interaction level; relies on QuestRegistry ordering |
| QuestService.AcceptQuest prerequisite check | NOT ENFORCED | AcceptQuest does not validate PrerequisiteQuestIds |
| Full prerequisite UI | PREREQUISITE_UI_DEBT | Future: QuestGiverInteractable should check if prerequisites completed before offering |

The prerequisite chain is documented and the data is correct. Runtime enforcement of
"show only if prerequisite completed" is a known debt — player can technically accept Q2
before completing Q1 in current build, but the QuestLog and flow still work correctly.
