using System;
using CindarsHope.Foundation;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Porta local do módulo NPC (arch: corte do par mútuo NPC|UI, 2026-07-15 — precedente
    /// Craft/ICraftingStationModal, World/ICorpseRecoveryPresenter) para o menu de loja (Buy/Sell/
    /// Exit) consumido por NpcShopController, sem depender do tipo concreto
    /// CindarsHope.UI.Shop.ShopMenuModal.
    /// </summary>
    public interface INpcShopMenuPresenter
    {
        event Action<ShopMenuOption> OnOptionSelected;

        void Initialize(IModalRuntime modalManager);
        void Show();
        void Hide();
        void HideVisualOnly();
    }
}
