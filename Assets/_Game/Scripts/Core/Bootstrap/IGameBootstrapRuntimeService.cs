using CindarsHope.Inventory.Data;

namespace CindarsHope.Core.Bootstrap
{
    public readonly struct GameBootstrapRuntimeContext
    {
        public GameBootstrapRuntimeContext(ItemDatabaseSO itemDatabase)
        {
            ItemDatabase = itemDatabase;
        }

        public ItemDatabaseSO ItemDatabase { get; }
    }

    public interface IGameBootstrapRuntimeService
    {
        string BootstrapServiceId { get; }
        bool IsInitialized { get; }
        void InitializeFromBootstrap(GameBootstrapRuntimeContext context);
        void ShutdownFromBootstrap();
    }
}
