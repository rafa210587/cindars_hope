using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;

namespace CindarsHope.Core.Bootstrap
{
    public readonly struct GameBootstrapRuntimeContext
    {
        public GameBootstrapRuntimeContext(ItemDatabaseSO itemDatabase, PlayerDataSO playerData = null)
        {
            ItemDatabase = itemDatabase;
            PlayerData = playerData;
        }

        public ItemDatabaseSO ItemDatabase { get; }

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
