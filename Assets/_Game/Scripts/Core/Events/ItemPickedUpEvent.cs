namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um item do mundo entra no inventario.
    /// Usa ItemId estavel em vez de referencia Unity.
    /// </summary>
    public readonly struct ItemPickedUpEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemPickedUpEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
