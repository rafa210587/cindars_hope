namespace CindarsHope.Core.Events
{
    public readonly struct EconomyTransactionCompletedEvent
    {
        public readonly bool WasSuccessful;
        public readonly string TransactionType;
        public readonly string ItemId;
        public readonly int Amount;
        public readonly int GoldDelta;
        public readonly string Message;

        public EconomyTransactionCompletedEvent(
            bool wasSuccessful,
            string transactionType,
            string itemId,
            int amount,
            int goldDelta,
            string message)
        {
            WasSuccessful = wasSuccessful;
            TransactionType = transactionType ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Amount = amount;
            GoldDelta = goldDelta;
            Message = message ?? string.Empty;
        }
    }
}
