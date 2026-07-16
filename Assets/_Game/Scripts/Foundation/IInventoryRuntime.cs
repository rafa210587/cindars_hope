// arch: quebra do par mutuo Core|Inventory (2026-07-15) — porta pura (C# puro, sem dependencia de
// engine) que permite a GameBootstrap (Core) e a CorpseRecoveryManager (Player.Death) consumirem o
// InventoryManager sem nomear CindarsHope.Inventory. InventoryManager se anuncia via
// DomainManagerRegistry.Register<IInventoryRuntime>(this) (molde IEquipmentRuntime/ISkillTreeRuntime).
// Cobre apenas os membros efetivamente chamados por esses dois consumidores fora do modulo Inventory;
// consumidores dentro de outros modulos que precisem da API completa resolvem o tipo concreto por cast
// local (ex.: bootstrap.InventoryManager as InventoryManager), permitido pois esses modulos ja podem
// nomear CindarsHope.Inventory.
namespace CindarsHope.Foundation
{
    public interface IInventoryRuntime
    {
        bool HasItem(string itemId, int amount = 1);
        bool AddItem(string itemId, int amount);
        void Shutdown();
        int ClearHotbarBindingsForMissingItems(System.Func<int, string> getSlotItemId, System.Action<int, string> setSlot, int slotCount);
    }
}
