namespace CindarsHope.Quest
{
    public class QuestStartedEvent
    {
        public string QuestId { get; }

        public QuestStartedEvent(string questId)
        {
            QuestId = questId;
        }
    }
}
