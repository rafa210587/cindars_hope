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

    public enum QuestGiverInteractionMode
    {
        Offer = 0,
        TurnIn = 1,
        NoQuest = 2
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
