using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do inventário do jogador. Fonte do estado: <see cref="InventoryManager"/>
    /// injetado via constructor. Após restaurar, aplica reparo de itens iniciais ausentes e limpa
    /// bindings do hotbar para itens inexistentes.
    /// </summary>
    public class InventorySectionProvider : ISaveSectionProvider
    {
        private readonly InventoryManager _inventoryManager;
        private readonly PlayerDataSO _playerData;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly HotbarState _hotbarState;

        /// <param name="inventoryManager">Manager de inventário — pode ser null (warn no log).</param>
        /// <param name="playerData">Dados iniciais do jogador para reparo de itens starter.</param>
        /// <param name="itemDatabase">Database de itens para resolução de IDs.</param>
        /// <param name="hotbarState">Estado do hotbar para limpeza de bindings pós-restore.</param>
        public InventorySectionProvider(
            InventoryManager inventoryManager,
            PlayerDataSO playerData,
            ItemDatabaseSO itemDatabase,
            HotbarState hotbarState)
        {
            _inventoryManager = inventoryManager;
            _playerData = playerData;
            _itemDatabase = itemDatabase;
            _hotbarState = hotbarState;
        }

        public string ProviderId => "inventory";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_inventoryManager != null)
            {
                return _inventoryManager.CaptureSaveData();
            }

            Debug.LogWarning("InventorySectionProvider: InventoryManager não injetado. Seção de inventário vazia.");
            return new InventorySaveData();
        }

        public void Restore(object sectionData)
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning("InventorySectionProvider: InventoryManager não injetado; restore de inventário ignorado.");
                return;
            }

            var data = sectionData as InventorySaveData ?? new InventorySaveData();
            _inventoryManager.RestoreFromSaveData(data);
            _inventoryManager.EnsureStarterItemsPresent(_playerData, _itemDatabase, "RepairMissingItems");

            if (_hotbarState != null)
            {
                _inventoryManager.ClearHotbarBindingsForMissingItems(
                    _hotbarState.GetSlotItemId,
                    (slot, id) => _hotbarState.SetSlot(slot, id),
                    HotbarState.SlotCount);
            }
        }
    }
}
