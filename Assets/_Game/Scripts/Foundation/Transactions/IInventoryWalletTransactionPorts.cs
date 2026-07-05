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
}
