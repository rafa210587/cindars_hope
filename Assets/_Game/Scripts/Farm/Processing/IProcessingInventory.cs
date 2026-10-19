using System.Collections.Generic;
using CindarsHope.Inventory;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — porta de inventário usada pelo motor puro de processamento
    /// (<see cref="FarmProcessingStationModel"/>). Permite testar o consumo de insumos e a
    /// entrega de output em EditMode sem um InventoryManager de cena. InventoryManager satisfaz
    /// esta porta por adaptador no <see cref="FarmProcessingStationService"/>.
    /// </summary>
    public interface IProcessingInventory
    {
        bool HasItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
        bool AddItem(string itemId, int amount);
    }

    /// <summary>Origin-aware extension used by the runtime adapter; legacy test ports remain compatible.</summary>
    public interface IOriginProcessingInventory : IProcessingInventory
    {
        bool TryPrepareOriginRemoval(string itemId, int amount,
            out IReadOnlyList<ItemOriginLot> lots, out object token);
        bool TryCommitOriginRemoval(object token);
        int NormalUnitLiquidationGold(string itemId);
        bool TryAddOriginLots(string itemId, int amount, IReadOnlyList<ItemOriginLot> lots);
    }
}
