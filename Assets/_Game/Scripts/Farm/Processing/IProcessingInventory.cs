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
}
