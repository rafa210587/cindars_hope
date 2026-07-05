using CindarsHope.Foundation.Transactions;

namespace CindarsHope.Economy.Transactions
{
    public enum PurchaseTransactionFailure
    {
        None = 0,
        InvalidRequest = 1,
        InsufficientFunds = 2,
        InventoryFull = 3,
        DebitFailed = 4,
        InventoryWriteFailed = 5
    }

    public readonly struct PurchaseTransactionResult
    {
        public PurchaseTransactionResult(bool success, PurchaseTransactionFailure failure)
        {
            Success = success;
            Failure = failure;
        }

        public bool Success { get; }
        public PurchaseTransactionFailure Failure { get; }
    }

    /// <summary>Compra atômica: débito é compensado se a escrita no inventário falhar.</summary>
    public static class AtomicPurchaseTransaction
    {
        public static PurchaseTransactionResult Execute(
            IInventoryTransactionPort inventory,
            IWalletTransactionPort wallet,
            string itemId,
            int amount,
            int totalCost)
        {
            if (inventory == null || wallet == null || string.IsNullOrWhiteSpace(itemId)
                || amount <= 0 || totalCost < 0)
            {
                return Failed(PurchaseTransactionFailure.InvalidRequest);
            }

            if (wallet.Balance < totalCost)
            {
                return Failed(PurchaseTransactionFailure.InsufficientFunds);
            }

            if (!inventory.CanAddItem(itemId, amount))
            {
                return Failed(PurchaseTransactionFailure.InventoryFull);
            }

            if (totalCost > 0 && !wallet.TryDebit(totalCost))
            {
                return Failed(PurchaseTransactionFailure.DebitFailed);
            }

            if (inventory.AddItem(itemId, amount))
            {
                return new PurchaseTransactionResult(true, PurchaseTransactionFailure.None);
            }

            if (totalCost > 0)
            {
                wallet.Credit(totalCost);
            }

            return Failed(PurchaseTransactionFailure.InventoryWriteFailed);
        }

        private static PurchaseTransactionResult Failed(PurchaseTransactionFailure failure) =>
            new PurchaseTransactionResult(false, failure);
    }
}
