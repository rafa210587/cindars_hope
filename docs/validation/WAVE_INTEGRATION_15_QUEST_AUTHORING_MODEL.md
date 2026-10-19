# WAVE_INTEGRATION_15 — Quest Authoring Model

Date: 2026-06-10

---

## How to Add a New Quest

1. Open `Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs`
2. In the `RegisterSmokeTestQuests()` method (or create a new registration method), add:

```csharp
var myObjectives = new List<QuestObjective>
{
    new QuestObjective
    {
        ObjectiveId = "obj_my_collect_x3",
        ObjectiveType = QuestObjectiveType.CollectItem,
        TargetId = "item_material_wood",
        RequiredAmount = 3
    }
};

var myQuest = new QuestDefinition
{
    QuestId = "quest_my_new_quest",
    Category = QuestCategory.Side,
    DisplayName = "Nome da Missão",
    Description = "Descrição da missão.",
    Trackable = true
};

var myRewards = new List<QuestRewardDefinition>
{
    new QuestRewardDefinition
    {
        RewardId = "reward_my_quest_gold",
        RewardType = QuestRewardType.Gold,
        Quantity = 100,
        IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
    }
};

Register(myQuest, myObjectives, myRewards, "npc_my_giver");
```

---

## How to Add a Quest Giver NPC

1. In TownScene, find the NPC's root GameObject
2. Add component `QuestGiverInteractable`
3. Set `npcId` = stable NPC ID (e.g., "npc_thalindra")
4. Set `offeredQuestIds` = array of quest IDs the NPC offers

---

## How to Add a Quest Board

1. Create empty GameObject in TownScene
2. Add component `QuestBoardInteractable`
3. Set `boardId` = stable board ID (e.g., "board_first_quest_01")
4. Set `postedQuestIds` = array of quest IDs posted on the board

---

## Known Debts

| Debt | Tag | Next Step |
|------|-----|-----------|
| Hard-coded quests in QuestRegistry | TEMPORARY_QUEST_SMOKE_TEST | Create QuestDatabaseSO / ScriptableObject pipeline |
| No SaveManager integration | QUEST_SAVE_LOAD_IN_MEMORY | Wire QuestStateSection to SaveManager sections |
| DefeatEnemy objectives not wired | KILL_OBJECTIVE_DEFERRED | Wire EnemyKilledEvent in QuestProgressEventBridge when combat ready |
| TalkToNpc objectives not wired | TALK_OBJECTIVE_DEFERRED | Wire NpcInteractionStartedEvent in bridge |
| Canvas UI panels not wired | HUMAN_UNITY_ACTION_REQUIRED | Human must add Canvas panels and wire to controllers |

---

## SO Pipeline (Future)

When ScriptableObject pipeline is ready:
1. Create `QuestDefinitionSO : ScriptableObject` with QuestId, DisplayName, Description, GiverId
2. Create `QuestObjectiveSO : ScriptableObject` with ObjectiveId, ObjectiveType, TargetId, RequiredAmount
3. Create `QuestDatabaseSO : ScriptableObject` with List<QuestDefinitionSO>
4. Modify `QuestRegistry` to load from `QuestDatabaseSO` instead of hard-coded registration
5. Keep `QuestService`, adapters, and event bridge unchanged (they operate on plain C# models)
