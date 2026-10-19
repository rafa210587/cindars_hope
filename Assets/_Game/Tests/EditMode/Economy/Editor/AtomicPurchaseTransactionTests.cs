using CindarsHope.Economy.Transactions;
using CindarsHope.Foundation.Transactions;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Economy
{
    public class AtomicPurchaseTransactionTests
    {
        private sealed class InventoryStub : IInventoryTransactionPort
        {
            public bool CanAdd = true;
            public bool AddSucceeds = true;
            public int Added;

            public bool CanAddItem(string itemId, int amount) => CanAdd;
            public bool AddItem(string itemId, int amount)
            {
                if (AddSucceeds) Added += amount;
                return AddSucceeds;
            }
            public bool RemoveItem(string itemId, int amount) => true;
        }

        private sealed class WalletStub : IWalletTransactionPort
        {
            public int Balance { get; set; }
            public bool DebitSucceeds = true;
            public bool TryDebit(int amount)
            {
                if (!DebitSucceeds || Balance < amount) return false;
                Balance -= amount;
                return true;
            }
            public void Credit(int amount) => Balance += amount;
        }

        [Test]
        public void InventoryWriteFailure_RefundsEntireDebit()
        {
            var inventory = new InventoryStub { AddSucceeds = false };
            var wallet = new WalletStub { Balance = 100 };

            PurchaseTransactionResult result = AtomicPurchaseTransaction.Execute(
                inventory, wallet, "item_test", 2, 30);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Failure, Is.EqualTo(PurchaseTransactionFailure.InventoryWriteFailed));
            Assert.That(wallet.Balance, Is.EqualTo(100));
            Assert.That(inventory.Added, Is.Zero);
        }

        [Test]
        public void SuccessfulPurchase_DebitsAndAddsExactlyOnce()
        {
            var inventory = new InventoryStub();
            var wallet = new WalletStub { Balance = 100 };

            PurchaseTransactionResult result = AtomicPurchaseTransaction.Execute(
                inventory, wallet, "item_test", 2, 30);

            Assert.That(result.Success, Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(70));
            Assert.That(inventory.Added, Is.EqualTo(2));
        }
    }
}
