namespace CindarsHope.Foundation.Transactions
{
    public interface IInventoryTransactionPort
    {
        bool CanAddItem(string itemId, int amount);
        bool AddItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
    }

    public interface IWalletTransactionPort
    {
        int Balance { get; }
        bool TryDebit(int amount);
        void Credit(int amount);
    }

    // A purchase keeps its inventory grant silent until the wallet and optional stock
    // are committed. Implementations must restore the exact slots/lots on rollback.
    public interface IPurchasedInventoryTransactionPort
    {
        bool CanAddItem(string itemId, int amount);
        bool TryAddPurchasedItemSilently(string itemId, int amount, int totalCost);
        bool TryRollbackPurchasedAdd();
        void PublishCommittedInventoryChange(string itemId, int amount);
    }

    public interface IStagedPurchaseWalletPort
    {
        int Balance { get; }
        bool TryStageDebit(int amount);
        bool TryRollbackDebit();
        bool TryCommitDebit();
    }
}
