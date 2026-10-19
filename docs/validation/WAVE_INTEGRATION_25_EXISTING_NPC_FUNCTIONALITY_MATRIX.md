# WAVE INTEGRATION 25 - Existing NPC Functionality Matrix

Status: AUDIT_COMPLETE

| System | File | Status | Notes |
|---|---|---|---|
| NpcManager | Assets/_Game/Scripts/NPC/NpcManager.cs | EXISTS | Manages NpcController + NpcShopController lists; CaptureSaveData/RestoreFromSaveData implemented |
| NpcController | Assets/_Game/Scripts/NPC/NpcController.cs | EXISTS | IInteractable; HasMet; DialogueTree; quest offer; wanderer pause; events |
| NpcShopController | Assets/_Game/Scripts/NPC/NpcShopController.cs | EXISTS | IInteractable; HasMet; DialogueModal; ShopMenu; BuyPanel; SellPanel; Thalindra quest+buy+sell+adeus fully implemented |
| NpcWanderer | Assets/_Game/Scripts/NPC/NpcWanderer.cs | EXISTS | RandomWander mode; SetInteractionPaused(bool); wander bounds; respects NpcMovementMode |
| NpcDataSO | Assets/_Game/Scripts/NPC/NpcDataSO.cs | EXISTS | NpcId, DisplayName, OpeningLine, ClosingLine, DialogueTree, ShopId, DefaultSceneId, DefaultPosition, MovementMode |
| DialogueTreeSO | Assets/_Game/Scripts/NPC/DialogueTreeSO.cs | EXISTS | Nodes, StartNodeId, GetNodeById(); supports RandomLinePool |
| DialogueNode | Assets/_Game/Scripts/NPC/DialogueNode.cs | EXISTS | NodeId, Text, Choices, RandomLinePool |
| DialogueChoice | Assets/_Game/Scripts/NPC/DialogueChoice.cs | EXISTS | Label, NextNodeId, ActionType, ActionPayload |
| DialogueActionType | Assets/_Game/Scripts/NPC/DialogueChoice.cs | EXISTS | None, CloseDialogue, OpenShop, OfferQuest |
| NpcScenePlacementMarker | Assets/_Game/Scripts/NPC/Runtime/NpcScenePlacementMarker.cs | EXISTS | NpcId, SceneId, PlacementId, MovementProfile, Reachable; Configure() method |
| NpcInteractionStartedEvent | Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs | EXISTS | Published on interaction start |
| NpcInteractionEndedEvent | Assets/_Game/Scripts/Core/Events/NpcInteractionEvents.cs | EXISTS | Published on interaction end |
| DayStartedEvent | Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs | EXISTS | DayNumber; triggers daily logic |
| GameTimeTickEvent | Assets/_Game/Scripts/Core/Events/GameTimeTickEvent.cs | EXISTS | Per-second tick; no TimeBlock |
| NpcManagerSaveData | Assets/_Game/Scripts/Save/SaveData.cs | EXISTS | List<NpcSaveData> Npcs |
| NpcSaveData | Assets/_Game/Scripts/Save/SaveData.cs | EXISTS | NpcId, SceneId, Vector2 Position, bool HasMet |
| SaveManager NPC | Assets/_Game/Scripts/Save/SaveManager.cs | EXISTS | Calls NpcManager.CaptureSaveData/RestoreFromSaveData |
| NpcScheduleDefinition | Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs | EXISTS | Pure data; NpcId, DefaultPeriodBlocks, WeatherModifiers, etc. |
| NpcScheduleResolver (pure) | Assets/_Game/Scripts/City/Schedule/NpcScheduleResolver.cs | EXISTS | Pure resolver (no MonoBehaviour); resolves period+waypoint from context |
| SchedulePeriod | Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs | EXISTS | Morning/WorkStart/Midday/WorkAfternoon/Evening/Night/SleepLateNight; SchedulePeriodHelper.FromHour() |
| GameTimeManager | Assets/_Game/Scripts/Core/GameTimeManager.cs | EXISTS | Publishes DayStartedEvent; NO TimeBlockChangedEvent |

## What Does NOT Exist (WAVE25 delta)

| System | Namespace | Notes |
|---|---|---|
| NpcScheduleService | CindarsHope.NPC.Schedule | Runtime MonoBehaviour; bridges DayStartedEvent → resolve schedule → set NPC position |
| NpcScheduleAnchor | CindarsHope.NPC.Schedule | Lightweight MonoBehaviour marking named anchor position for schedule |
| NpcScheduleProfile | CindarsHope.NPC.Schedule | Simple serializable wrapper linking NpcId → schedule blocks |
| NpcScheduleRuntimeState | CindarsHope.NPC.Schedule | Transient in-memory record of NPC's current schedule state |
| NpcScheduleRuntimeBootstrap | CindarsHope.NPC.Schedule | RuntimeInitializeOnLoadMethod ensuring NpcScheduleService singleton |
| NpcTownRosterRegistry | CindarsHope.NPC | Static canonical roster with real NPC IDs for validator use |
| NpcDialogueSetRegistry | CindarsHope.NPC | Documents dialogue coverage per NPC (maps NpcId → line count) |
| NpcDialogueExpansionBootstrap | CindarsHope.NPC | RuntimeInitializeOnLoadMethod registering dialogue expansion |
| ValidateWave25TownNpcSchedulesDialogue | Editor | Editor validator for WAVE25 checks |

## TIME_BLOCK_DEBT

GameTimeManager publishes DayStartedEvent (not TimeBlockChangedEvent).
Schedule service resolves positions only on day boundary, not intra-day period changes.
Intra-day schedule transitions are TIME_BLOCK_DEBT — deferred.
