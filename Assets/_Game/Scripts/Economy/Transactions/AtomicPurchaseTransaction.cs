using CindarsHope.Foundation.Transactions;
using CindarsHope.Inventory;
using CindarsHope.Player;

namespace CindarsHope.Economy.Transactions
{
    public enum PurchaseTransactionFailure
    {
        None = 0,
        InvalidRequest = 1,
        InsufficientFunds = 2,
        InventoryFull = 3,
        DebitFailed = 4,
        InventoryWriteFailed = 5,
        StockUnavailable = 6
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
        public static PurchaseTransactionResult ExecutePurchased(
            InventoryManager inventory, PlayerManager wallet, string itemId, int amount,
            int totalCost, IShopPurchaseStockPort stock = null)
        {
            if (inventory == null || wallet == null)
                return Failed(PurchaseTransactionFailure.InvalidRequest);
            return ExecutePurchased(new PurchasedInventoryAdapter(inventory),
                new StagedWalletAdapter(wallet), itemId, amount, totalCost, stock);
        }

        public static PurchaseTransactionResult ExecutePurchased(
            IPurchasedInventoryTransactionPort inventory, IStagedPurchaseWalletPort wallet,
            string itemId, int amount, int totalCost, IShopPurchaseStockPort stock = null)
        {
            if (inventory == null || wallet == null || string.IsNullOrWhiteSpace(itemId)
                || amount <= 0 || totalCost < 0)
                return Failed(PurchaseTransactionFailure.InvalidRequest);
            if (wallet.Balance < totalCost)
                return Failed(PurchaseTransactionFailure.InsufficientFunds);
            if (!inventory.CanAddItem(itemId, amount))
                return Failed(PurchaseTransactionFailure.InventoryFull);
            if (stock != null && !stock.CanReserve())
                return Failed(PurchaseTransactionFailure.StockUnavailable);

            if (!wallet.TryStageDebit(totalCost))
                return Failed(PurchaseTransactionFailure.DebitFailed);

            var stockReserved = false;
            var inventoryAdded = false;
            var committed = false;
            try
            {
                if (stock != null)
                {
                    if (!stock.TryReserve())
                        return Failed(PurchaseTransactionFailure.StockUnavailable);
                    stockReserved = true;
                }

                if (!inventory.TryAddPurchasedItemSilently(itemId, amount, totalCost))
                    return Failed(PurchaseTransactionFailure.InventoryWriteFailed);
                inventoryAdded = true;

                // TryCommitDebit publishes GoldChanged. At this point save listeners see
                // the charged wallet, purchased lots, and reduced stock together.
                if (!wallet.TryCommitDebit())
                    return Failed(PurchaseTransactionFailure.DebitFailed);
                committed = true;
            }
            finally
            {
                if (!committed)
                {
                    var inventoryRestored = !inventoryAdded || inventory.TryRollbackPurchasedAdd();
                    var stockRestored = !stockReserved || stock.Rollback();
                    var walletRestored = wallet.TryRollbackDebit();
                    if (!inventoryRestored || !stockRestored || !walletRestored)
                        throw new System.InvalidOperationException("Purchase rollback did not restore every staged domain.");
                }
            }

            inventory.PublishCommittedInventoryChange(itemId, amount);
            stock?.PublishCommittedChange();
            return new PurchaseTransactionResult(true, PurchaseTransactionFailure.None);
        }

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

    public interface IShopPurchaseStockPort
    {
        bool CanReserve();
        bool TryReserve();
        bool Rollback();
        void PublishCommittedChange();
    }

    internal sealed class PurchasedInventoryAdapter : IPurchasedInventoryTransactionPort
    {
        private readonly InventoryManager _inventory;
        private InventoryAdditionToken _token;

        public PurchasedInventoryAdapter(InventoryManager inventory) => _inventory = inventory;
        public bool CanAddItem(string itemId, int amount) => _inventory.CanAddItem(itemId, amount);
        public bool TryAddPurchasedItemSilently(string itemId, int amount, int totalCost)
            => _inventory.TryStagePurchasedAddition(itemId, amount, totalCost, out _token);
        public bool TryRollbackPurchasedAdd() => _inventory.TryRollbackPurchasedAdd(_token);
        public void PublishCommittedInventoryChange(string itemId, int amount)
            => _inventory.PublishCommittedInventoryChange(itemId, amount);
    }

    internal sealed class StagedWalletAdapter : IStagedPurchaseWalletPort
    {
        private readonly PlayerManager _wallet;
        private PlayerManager.GoldMutationToken _token;
        public StagedWalletAdapter(PlayerManager wallet) => _wallet = wallet;
        public int Balance => _wallet.CurrentGold;
        public bool TryStageDebit(int amount) => _wallet.TryStageGoldMutation(-amount, out _token);
        public bool TryRollbackDebit() => _wallet.TryRollbackGoldMutation(_token);
        public bool TryCommitDebit() => _wallet.TryCommitGoldMutation(_token);
    }
}
