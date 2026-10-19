using System.Collections.Generic;
using CindarsHope.Farm.Processing;
using CindarsHope.Inventory;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Farm
{
    public sealed class PurchasedOriginProcessingTests
    {
        private sealed class OriginInventory : IOriginProcessingInventory
        {
            public readonly Dictionary<string, List<ItemOriginLot>> Items =
                new Dictionary<string, List<ItemOriginLot>>();
            public bool RejectOutput;
            public bool HasItem(string itemId, int amount)
                => Items.TryGetValue(itemId, out var lots) && Count(lots) >= amount;
            public bool RemoveItem(string itemId, int amount) => false;
            public bool AddItem(string itemId, int amount) => false;
            public bool TryPrepareOriginRemoval(string itemId, int amount,
                out IReadOnlyList<ItemOriginLot> lots, out object token)
            {
                lots = null;
                token = null;
                if (!HasItem(itemId, amount)) return false;
                var selected = ItemOriginLot.CopyLots(Items[itemId]);
                if (Count(selected) != amount) return false;
                lots = selected;
                token = itemId;
                return true;
            }
            public bool TryCommitOriginRemoval(object token)
            {
                if (!(token is string itemId) || !Items.ContainsKey(itemId)) return false;
                Items.Remove(itemId);
                return true;
            }
            public int NormalUnitLiquidationGold(string itemId) => 8;
            public bool TryAddOriginLots(string itemId, int amount, IReadOnlyList<ItemOriginLot> lots)
            {
                if (RejectOutput || !ItemOriginLot.IsValid(lots, amount)) return false;
                Items.Add(itemId, ItemOriginLot.CopyLots(lots));
                return true;
            }
            public static int Count(IReadOnlyList<ItemOriginLot> lots)
            {
                var quantity = 0;
                foreach (var lot in lots) quantity += lot.Quantity;
                return quantity;
            }
        }

        [Test]
        public void PurchasedMilk_JobRoundTrip_CollectionKeepsRestrictedCap()
        {
            var inventory = new OriginInventory();
            inventory.Items.Add(ProcessingRecipeCatalog.ItemGoatMilkId,
                ItemOriginLot.Purchased(2, 10));
            var model = new FarmProcessingStationModel();
            Assert.AreEqual(FarmProcessingStationModel.StartResult.Started,
                model.StartJob(ProcessingRecipeCatalog.StationCheesePressId,
                    ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inventory));
            var saved = model.Capture();
            Assert.AreEqual(1, saved.Jobs[0].OriginSchemaVersion);
            Assert.AreEqual(2, OriginInventory.Count(
                ItemOriginLot.FromSaveData(saved.Jobs[0].InputOriginLots, 2)));

            var restored = new FarmProcessingStationModel();
            restored.Restore(saved);
            restored.AdvanceDay(2);
            inventory.RejectOutput = true;
            Assert.IsTrue(restored.Collect(ProcessingRecipeCatalog.StationCheesePressId,
                inventory).InventoryFull);
            Assert.IsTrue(restored.HasActiveJob(ProcessingRecipeCatalog.StationCheesePressId));
            inventory.RejectOutput = false;
            Assert.IsTrue(restored.Collect(ProcessingRecipeCatalog.StationCheesePressId,
                inventory).Success);
            Assert.IsFalse(restored.Collect(ProcessingRecipeCatalog.StationCheesePressId,
                inventory).Success);
            var output = inventory.Items[ProcessingRecipeCatalog.ItemGoatCheeseId];
            Assert.AreEqual(ItemOriginKind.RestrictedDerived, output[0].Kind);
            Assert.AreEqual(8, output[0].UnitResaleCapGold);
            Assert.AreEqual(10, output[0].UnitPaidBasisGold);
        }

        [Test]
        public void CorruptJobOrigin_RestoresUnsellableOutput()
        {
            var inventory = new OriginInventory();
            inventory.Items.Add(ProcessingRecipeCatalog.ItemGoatMilkId,
                ItemOriginLot.Purchased(2, 10));
            var model = new FarmProcessingStationModel();
            model.StartJob(ProcessingRecipeCatalog.StationCheesePressId,
                ProcessingRecipeCatalog.RecipeGoatCheeseId, 1, inventory);
            var saved = model.Capture();
            saved.Jobs[0].OutputOriginLots = null;
            var restored = new FarmProcessingStationModel();
            restored.Restore(saved);
            restored.AdvanceDay(2);
            Assert.IsTrue(restored.Collect(ProcessingRecipeCatalog.StationCheesePressId,
                inventory).Success);
            Assert.AreEqual(ItemOriginKind.Invalid,
                inventory.Items[ProcessingRecipeCatalog.ItemGoatCheeseId][0].Kind);
        }
    }
}
