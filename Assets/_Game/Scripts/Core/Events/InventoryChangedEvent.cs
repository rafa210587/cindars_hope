namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando a quantidade de um item no inventário muda.
    /// Usa ItemId estável; UI/Save resolvem dados visuais por registries.
    /// </summary>
    public readonly struct InventoryChangedEvent
    {
        public string ItemId { get; }
        public int Delta { get; }
        public int NewAmount { get; }

        public InventoryChangedEvent(string itemId, int delta, int newAmount)
        {
            ItemId = itemId;
            Delta = delta;
            NewAmount = newAmount;
        }
    }
}
