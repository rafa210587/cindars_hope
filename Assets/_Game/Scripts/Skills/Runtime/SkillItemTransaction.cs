using CindarsHope.Inventory;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>
    /// Narrow inventory port for a material cost. RemoveItem implementations must either remove
    /// the complete requested amount or leave inventory unchanged.
    /// </summary>
    public interface ISkillItemInventory
    {
        int GetAmount(string itemId);
        bool RemoveItem(string itemId, int amount);
    }

    /// <summary>Inventory port used when a compound skill transaction may need compensation.</summary>
    public interface IReversibleSkillItemInventory : ISkillItemInventory
    {
        bool AddItem(string itemId, int amount);
    }

    public enum SkillItemCommitFailure
    {
        None = 0,
        InvalidRequest = 1,
        MissingItem = 2,
        RemoveFailed = 3
    }

    public readonly struct SkillItemCommitResult
    {
        public bool Success { get; }
        public SkillItemCommitFailure Failure { get; }
        public string ItemId { get; }
        public int Amount { get; }

        public SkillItemCommitResult(
            bool success,
            SkillItemCommitFailure failure,
            string itemId,
            int amount)
        {
            Success = success;
            Failure = failure;
            ItemId = itemId ?? string.Empty;
            Amount = amount;
        }
    }

    /// <summary>
    /// Pure commit transaction used after every action-specific precondition has been validated.
    /// It performs one all-or-nothing removal and never reserves or consumes during a channel.
    /// </summary>
    public sealed class SkillItemTransaction
    {
        private readonly ISkillItemInventory _inventory;

        public SkillItemTransaction(ISkillItemInventory inventory)
        {
            _inventory = inventory;
        }

        public SkillItemCommitResult TryCommit(string itemId, int amount = 1)
        {
            if (_inventory == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return Failure(SkillItemCommitFailure.InvalidRequest, itemId, amount);
            }

            if (_inventory.GetAmount(itemId) < amount)
            {
                return Failure(SkillItemCommitFailure.MissingItem, itemId, amount);
            }

            if (!_inventory.RemoveItem(itemId, amount))
            {
                return Failure(SkillItemCommitFailure.RemoveFailed, itemId, amount);
            }

            return new SkillItemCommitResult(
                true,
                SkillItemCommitFailure.None,
                itemId,
                amount);
        }

        private static SkillItemCommitResult Failure(
            SkillItemCommitFailure failure,
            string itemId,
            int amount)
        {
            return new SkillItemCommitResult(false, failure, itemId, amount);
        }
    }

    /// <summary>Composition adapter over the existing inventory aggregate.</summary>
    public sealed class SkillItemInventoryAdapter : IReversibleSkillItemInventory
    {
        private readonly InventoryManager _inventory;

        public SkillItemInventoryAdapter(InventoryManager inventory)
        {
            _inventory = inventory;
        }

        public int GetAmount(string itemId)
        {
            return _inventory != null ? _inventory.GetAmount(itemId) : 0;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            return _inventory != null && _inventory.RemoveItem(itemId, amount);
        }

        public bool AddItem(string itemId, int amount)
        {
            return _inventory != null && _inventory.AddItem(itemId, amount);
        }
    }
}
