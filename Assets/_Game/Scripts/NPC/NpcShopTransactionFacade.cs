using System;
using CindarsHope.Economy;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Encapsula a transação de abrir o BuyPanel/SellPanel de uma sessão de loja: garante que o
    /// controller e os panels estão prontos (delegando a decisão pura a
    /// <see cref="NpcShopTransactionReadinessPolicy"/>), religa o handler de "voltar" do panel e
    /// exibe o panel. Extraído de <see cref="NpcShopController"/> para isolar a sequência de
    /// "ensure ready + wire back + show" que se repetia em quatro pontos do fluxo de diálogo/menu
    /// (Thalindra, root shop dialogue, dialogue tree e o menu de loja). Todas as referências mutáveis
    /// (rebindáveis pelo GameBootstrap) são lidas via provider para nunca operar sobre um snapshot
    /// obsoleto — o mesmo comportamento "sempre live" que o controller já tinha inline.
    /// </summary>
    public sealed class NpcShopTransactionFacade
    {
        private const int MaxReadinessPasses = 2;

        private readonly Func<ShopDataSO> _shopDataProvider;
        private readonly Func<ShopManager> _shopManagerProvider;
        private readonly Func<PlayerManager> _playerManagerProvider;
        private readonly Func<InventoryManager> _inventoryManagerProvider;
        private readonly Func<ItemDatabaseSO> _itemDatabaseProvider;
        private readonly Func<IModalRuntime> _modalManagerProvider;
        private readonly Func<INpcBuyPanel> _buyPanelProvider;
        private readonly Func<INpcSellPanel> _sellPanelProvider;
        private readonly Func<bool> _isControllerReadyProvider;
        private readonly Func<string, bool> _tryEnsureShopInitialized;
        private readonly Action _onPanelBack;
        private readonly Action<ShopMenuOption, string, string> _logTransactionError;

        public NpcShopTransactionFacade(
            Func<ShopDataSO> shopDataProvider,
            Func<ShopManager> shopManagerProvider,
            Func<PlayerManager> playerManagerProvider,
            Func<InventoryManager> inventoryManagerProvider,
            Func<ItemDatabaseSO> itemDatabaseProvider,
            Func<IModalRuntime> modalManagerProvider,
            Func<INpcBuyPanel> buyPanelProvider,
            Func<INpcSellPanel> sellPanelProvider,
            Func<bool> isControllerReadyProvider,
            Func<string, bool> tryEnsureShopInitialized,
            Action onPanelBack,
            Action<ShopMenuOption, string, string> logTransactionError)
        {
            _shopDataProvider = shopDataProvider;
            _shopManagerProvider = shopManagerProvider;
            _playerManagerProvider = playerManagerProvider;
            _inventoryManagerProvider = inventoryManagerProvider;
            _itemDatabaseProvider = itemDatabaseProvider;
            _modalManagerProvider = modalManagerProvider;
            _buyPanelProvider = buyPanelProvider;
            _sellPanelProvider = sellPanelProvider;
            _isControllerReadyProvider = isControllerReadyProvider;
            _tryEnsureShopInitialized = tryEnsureShopInitialized;
            _onPanelBack = onPanelBack;
            _logTransactionError = logTransactionError;
        }

        /// <summary>Garante prontidão, religa o back-handler e mostra o BuyPanel para o shopId atual.</summary>
        public bool TryOpenBuyPanel() => TryOpen(ShopMenuOption.Buy);

        /// <summary>Garante prontidão, religa o back-handler e mostra o SellPanel para o shopId atual.</summary>
        public bool TryOpenSellPanel() => TryOpen(ShopMenuOption.Sell);

        private bool TryOpen(ShopMenuOption option)
        {
            if (!EnsureReady(option))
            {
                return false;
            }

            var shopId = _shopDataProvider().Id;
            if (option == ShopMenuOption.Buy)
            {
                var buyPanel = _buyPanelProvider();
                buyPanel.OnBackPressed -= _onPanelBack;
                buyPanel.OnBackPressed += _onPanelBack;
                buyPanel.Show(shopId);
            }
            else
            {
                var sellPanel = _sellPanelProvider();
                sellPanel.OnBackPressed -= _onPanelBack;
                sellPanel.OnBackPressed += _onPanelBack;
                sellPanel.Show(shopId);
            }

            return true;
        }

        private bool EnsureReady(ShopMenuOption option)
        {
            var fieldName = option == ShopMenuOption.Buy ? "_buyPanel" : "_sellPanel";
            for (var pass = 0; pass < MaxReadinessPasses; pass++)
            {
                var shopManager = _shopManagerProvider();
                var shopData = _shopDataProvider();
                bool hasSession = shopManager != null
                    && shopData != null
                    && !string.IsNullOrWhiteSpace(shopData.Id)
                    && shopManager.TryGetSession(shopData.Id, out _);

                var playerManager = _playerManagerProvider();
                var inventoryManager = _inventoryManagerProvider();
                var itemDatabase = _itemDatabaseProvider();
                var modalManager = _modalManagerProvider();
                var buyPanel = _buyPanelProvider();
                var sellPanel = _sellPanelProvider();
                bool panelReady = option == ShopMenuOption.Buy
                    ? buyPanel != null && buyPanel.IsInitializedWith(
                        shopManager, playerManager, inventoryManager, itemDatabase, modalManager)
                    : sellPanel != null && sellPanel.IsInitializedWith(
                        shopManager, playerManager, inventoryManager, itemDatabase, modalManager);

                var snapshot = new NpcShopTransactionReadinessSnapshot(
                    shopData != null,
                    shopData != null && !string.IsNullOrWhiteSpace(shopData.Id),
                    shopManager != null,
                    itemDatabase != null,
                    _isControllerReadyProvider(),
                    shopManager != null && shopManager.IsInitialized,
                    hasSession,
                    panelReady);
                var decision = NpcShopTransactionReadinessPolicy.Evaluate(snapshot, fieldName);

                switch (decision.Action)
                {
                    case NpcShopTransactionReadinessAction.Ready:
                        return true;
                    case NpcShopTransactionReadinessAction.InitializeController:
                        if (_tryEnsureShopInitialized($"Before{option}")) continue;
                        _logTransactionError(option, "_isReady",
                            "controller is not ready after initialization attempt.");
                        return false;
                    case NpcShopTransactionReadinessAction.RecoverSession:
                        if (_tryEnsureShopInitialized($"MissingSessionBefore{option}")
                            && _shopManagerProvider().TryGetSession(_shopDataProvider().Id, out _))
                            continue;
                        _logTransactionError(option, "_shopManager",
                            $"ShopManager exists but has no session for this shopId. {_shopManagerProvider().GetDiagnosticSummary()}");
                        return false;
                    default:
                        _logTransactionError(option, decision.FieldName, decision.Cause);
                        return false;
                }
            }

            _logTransactionError(option, "_isReady",
                "transaction readiness did not converge after bounded initialization/recovery attempts.");
            return false;
        }
    }
}
