using CindarsHope.Player.Data;
using UnityEngine;

namespace CindarsHope.Core.Bootstrap
{
    public readonly struct GameBootstrapRuntimeContext
    {
        // arch: quebra do par mutuo Core|Inventory (2026-07-15) — tipado como ScriptableObject (nao
        // mais CindarsHope.Inventory.Data.ItemDatabaseSO) para que Core pare de nomear
        // CindarsHope.Inventory; consumidores fora de Core (SaveManager, ShopManager,
        // InventoryManager) castam para o tipo concreto localmente (molde MonoBehaviour usado por
        // ModalManager/SaveManager no cort do par Core|UI/Core|Save).
        public GameBootstrapRuntimeContext(ScriptableObject itemDatabase, PlayerDataSO playerData = null)
        {
            ItemDatabase = itemDatabase;
            PlayerData = playerData;
        }

        public ScriptableObject ItemDatabase { get; }

        // arch: quebra do ciclo Core|Save (2026-07-15) — adicionado para que SaveManager receba a
        // PlayerDataSO canonica via IGameBootstrapRuntimeService.InitializeFromBootstrap em vez de
        // GameBootstrap chamar SaveManager.RebindStarterInventoryData diretamente (o que nomearia
        // CindarsHope.Save no arquivo de Core).
        public PlayerDataSO PlayerData { get; }
    }

    public interface IGameBootstrapRuntimeService
    {
        string BootstrapServiceId { get; }
        bool IsInitialized { get; }
        void InitializeFromBootstrap(GameBootstrapRuntimeContext context);
        void ShutdownFromBootstrap();
    }
}
