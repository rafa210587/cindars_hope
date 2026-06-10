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
}
