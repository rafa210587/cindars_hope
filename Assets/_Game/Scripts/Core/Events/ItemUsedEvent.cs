namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um item é consumido/usado pelo jogador.
    /// </summary>
    public readonly struct ItemUsedEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemUsedEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
