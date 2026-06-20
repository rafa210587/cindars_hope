namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Minimal inventory access interface for the quest service.
    /// Decouples QuestService from InventoryManager MonoBehaviour.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    /// </summary>
    public interface IQuestInventoryAccess
    {
        int GetItemCount(string itemId);
        bool TryAddItem(string itemId, int count);
        bool TryRemoveItems(string itemId, int count);
    }

    /// <summary>
    /// Minimal gold access interface for the quest service reward system.
    /// </summary>
    public interface IQuestGoldAccess
    {
        void AddGold(int amount);
    }

    /// <summary>
    /// fable_34 — minimal player-progression access for scaled quest rewards (XP) and the
    /// main-act +1 skill point hook. Decouples QuestService from PlayerProgressionManager.
    /// </summary>
    public interface IQuestProgressionAccess
    {
        void AddXp(int amount);
        void GrantSkillPoints(int amount);
    }
}
