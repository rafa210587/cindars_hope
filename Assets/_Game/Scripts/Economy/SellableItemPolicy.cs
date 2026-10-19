using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Economy
{
    public static class SellableItemPolicy
    {
        public static bool IsSellable(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            var bootstrap = GameBootstrap.Instance;
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — cast local para o tipo concreto
            // (GameBootstrap.ItemDatabase agora retorna ScriptableObject).
            var itemDatabase = bootstrap?.ItemDatabase as ItemDatabaseSO;
            if (itemDatabase == null)
                return false;

            if (!itemDatabase.TryGetById(itemId, out var itemData))
                return false;

            if (itemData == null || itemData.BaseValue <= 0)
                return false;

            if (IsKeyItem(itemData) || IsQuestItem(itemData))
                return false;

            if (IsEssentialTool(itemId))
                return false;

            return true;
        }

        private static bool IsKeyItem(ItemDataSO itemData)
        {
            return itemData.Category == ItemCategory.KeyItem;
        }

        private static bool IsQuestItem(ItemDataSO itemData)
        {
            return itemData.Category == ItemCategory.Quest;
        }

        private static bool IsEssentialTool(string itemId)
        {
            return itemId.Contains("tool_pickaxe") ||
                   itemId.Contains("tool_hoe") ||
                   itemId.Contains("tool_watering");
        }
    }
}
