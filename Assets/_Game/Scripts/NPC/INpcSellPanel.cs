using System;
using CindarsHope.Economy;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Porta local do módulo NPC (arch: corte do par mútuo NPC|UI, 2026-07-15 — precedente
    /// Craft/ICraftingStationModal, World/ICorpseRecoveryPresenter) para o painel de venda
    /// consumido por NpcShopController/NpcShopTransactionFacade, sem depender do tipo concreto
    /// CindarsHope.UI.Shop.SellPanel.
    /// </summary>
    public interface INpcSellPanel
    {
        event Action OnBackPressed;

        void Initialize(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, IModalRuntime modalManager);
        bool IsInitializedWith(ShopManager shopManager, PlayerManager playerManager, InventoryManager inventoryManager, ItemDatabaseSO itemDatabase, IModalRuntime modalManager);
        void Show(string shopId);
        void Hide();
        void HideVisualOnly();
    }
}
