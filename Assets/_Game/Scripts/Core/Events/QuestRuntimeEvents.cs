using System.Collections.Generic;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Quest lifecycle events for WAVE_INTEGRATION_15.
    /// Published by QuestService; consumed by QuestLogPanelController, QuestLogRuntimeBinder, UI.
    ///
    /// Rule: no GameObject/MonoBehaviour/SO refs in event structs (event-bus-only-gameplay-communication rule).
    /// </summary>

    public readonly struct QuestAcceptedEvent
    {
        public string QuestId { get; }
        public QuestAcceptedEvent(string questId) => QuestId = questId;
    }

    public readonly struct QuestObjectiveProgressedEvent
    {
        public string QuestId { get; }
        public string ObjectiveId { get; }
        public int CurrentProgress { get; }
        public int RequiredProgress { get; }
        public QuestObjectiveProgressedEvent(string questId, string objectiveId, int current, int required)
        {
            QuestId = questId;
            ObjectiveId = objectiveId;
            CurrentProgress = current;
            RequiredProgress = required;
        }
    }

    public readonly struct QuestReadyToCompleteEvent
    {
        public string QuestId { get; }
        public QuestReadyToCompleteEvent(string questId) => QuestId = questId;
    }

    public readonly struct QuestCompletedEvent
    {
        public string QuestId { get; }
        public QuestCompletedEvent(string questId) => QuestId = questId;
    }

    public readonly struct QuestRewardClaimedEvent
    {
        public string QuestId { get; }
        public int GoldGiven { get; }
        public IReadOnlyList<string> ItemsGiven { get; }
        public QuestRewardClaimedEvent(string questId, int goldGiven, List<string> itemsGiven)
        {
            QuestId = questId;
            GoldGiven = goldGiven;
            ItemsGiven = itemsGiven ?? new List<string>();
        }
    }

    /// <summary>
    /// fable_34 — published when the notice board rotates its daily contracts (DayStartedEvent).
    /// UI/board listen to refresh the offered list. No Unity refs (event-bus rule).
    /// </summary>
    public readonly struct QuestBoardRefreshedEvent
    {
        public int Day { get; }
        public IReadOnlyList<string> ContractQuestIds { get; }
        public QuestBoardRefreshedEvent(int day, IReadOnlyList<string> contractQuestIds)
        {
            Day = day;
            ContractQuestIds = contractQuestIds ?? new List<string>();
        }
    }

    /// <summary>
    /// fable_34 — published when a cave-secret quest is discovered (offered by a peaceful creature
    /// or wandering merchant). Until this fires, the secret is absent from the Quest Log.
    /// </summary>
    public readonly struct SecretQuestDiscoveredEvent
    {
        public string QuestId { get; }
        public SecretQuestDiscoveredEvent(string questId) => QuestId = questId;
    }

    /// <summary>
    /// fable_36 — published when a main-quest act is fully completed (its final quest turned in).
    /// Consumed by toast/feedback to announce the milestone. Carries only simple ids (event-bus rule):
    /// the act number (1-4), the act-done milestone flag id, and the lore record id unlocked.
    /// </summary>
    public readonly struct ActCompletedEvent
    {
        public int ActNumber { get; }
        public string ActDoneFlagId { get; }
        public string LoreRecordId { get; }
        public ActCompletedEvent(int actNumber, string actDoneFlagId, string loreRecordId)
        {
            ActNumber = actNumber;
            ActDoneFlagId = actDoneFlagId;
            LoreRecordId = loreRecordId;
        }
    }

    public readonly struct QuestGiverInteractedEvent
    {
        public string NpcId { get; }
        public string QuestId { get; }
        public QuestGiverInteractionMode Mode { get; }
        public QuestGiverInteractedEvent(string npcId, string questId, QuestGiverInteractionMode mode)
        {
            NpcId = npcId;
            QuestId = questId;
            Mode = mode;
        }
    }
}
