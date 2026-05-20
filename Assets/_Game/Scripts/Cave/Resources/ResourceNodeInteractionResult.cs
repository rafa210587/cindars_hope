namespace CindarsHope.Cave.Resources
{
    public readonly struct ResourceNodeInteractionResult
    {
        public readonly bool DeliveredItem;
        public readonly bool DepletedNode;
        public readonly string ItemId;
        public readonly int Amount;
        public readonly string Message;

        public ResourceNodeInteractionResult(bool deliveredItem, bool depletedNode, string itemId, int amount, string message)
        {
            DeliveredItem = deliveredItem;
            DepletedNode = depletedNode;
            ItemId = itemId ?? string.Empty;
            Amount = amount;
            Message = message ?? string.Empty;
        }
    }
}
