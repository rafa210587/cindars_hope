namespace CindarsHope.Quest
{
    public class QuestCompletedEvent
    {
        public string QuestId { get; }
        public int RewardXp { get; }
        public int RewardGold { get; }

        public QuestCompletedEvent(string questId, int rewardXp, int rewardGold)
        {
            QuestId = questId;
            RewardXp = rewardXp;
            RewardGold = rewardGold;
        }
    }
}
