using CindarsHope.Inventory;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>Characterizes stack mutations without creating Unity objects or publishing events.</summary>
    public sealed class InventorySlotOperationsTests
    {
        [Test]
        public void Add_FillsPartialBeforeEmptyAndPreservesBinding()
        {
            var slots = new[] { Slot(0, "", 0), Slot(1, "wood", 8), Slot(2, "", 0) };
            slots[1].IsEquipped = true;
            slots[1].EquipmentBindingId = "equipped";
            Assert.That(InventorySlotOperations.GetAvailableCapacityFor(slots, "wood", 10), Is.EqualTo(22));
            Assert.That(InventorySlotOperations.TryAddItem(slots, "wood", 15, 10).Success, Is.True);
            Assert.That(slots[0].Amount, Is.EqualTo(10));
            Assert.That(slots[1].Amount, Is.EqualTo(10));
            Assert.That(slots[2].Amount, Is.EqualTo(3));
            Assert.That(slots[1].IsEquipped, Is.True);
            Assert.That(slots[1].EquipmentBindingId, Is.EqualTo("equipped"));
        }

        [Test]
        public void Add_InsufficientCapacityDoesNotPartiallyFillOrChangeMetadata()
        {
            var slots = new[] { Slot(0, "wood", 8), Slot(1, "stone", 3) };
            slots[0].EquipmentBindingId = "binding";
            var result = InventorySlotOperations.TryAddItem(slots, "wood", 3, 10);
            Assert.That(result.Success, Is.False);
            Assert.That(result.AddedAmount, Is.Zero);
            Assert.That(slots[0].Amount, Is.EqualTo(8));
            Assert.That(slots[0].EquipmentBindingId, Is.EqualTo("binding"));
            Assert.That(slots[1].ItemId, Is.EqualTo("stone"));
            Assert.That(slots[1].Amount, Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Add_InvalidStackLimitNormalizesToOne(int maxStack)
        {
            var slots = new[] { Slot(0, "", 0), Slot(1, "", 0) };
            Assert.That(InventorySlotOperations.TryAddItem(slots, "wood", 2, maxStack).Success, Is.True);
            Assert.That(slots[0].Amount, Is.EqualTo(1));
            Assert.That(slots[1].Amount, Is.EqualTo(1));
        }

        [Test]
        public void Remove_UsesLastStackFirstAndClearsOnlyExhaustedBinding()
        {
            var slots = new[] { Slot(0, "wood", 5), Slot(1, "stone", 3), Slot(2, "wood", 2) };
            slots[0].EquipmentBindingId = "partial";
            slots[2].EquipmentBindingId = "exhausted";
            slots[2].IsEquipped = true;
            Assert.That(InventorySlotOperations.TryRemoveItem(slots, "wood", 3), Is.True);
            Assert.That(slots[0].Amount, Is.EqualTo(4));
            Assert.That(slots[0].EquipmentBindingId, Is.EqualTo("partial"));
            Assert.That(slots[1].Amount, Is.EqualTo(3));
            Assert.That(slots[2].IsEmpty, Is.True);
            Assert.That(slots[2].IsEquipped, Is.False);
            Assert.That(slots[2].EquipmentBindingId, Is.Empty);
        }

        [Test]
        public void Remove_InsufficientContentsRejectsBeforeMutation()
        {
            var slots = new[] { Slot(0, "wood", 2), Slot(1, "wood", 3) };
            slots[1].EquipmentBindingId = "binding";
            Assert.That(InventorySlotOperations.TryRemoveItem(slots, "wood", 6), Is.False);
            Assert.That(slots[0].Amount, Is.EqualTo(2));
            Assert.That(slots[1].Amount, Is.EqualTo(3));
            Assert.That(slots[1].EquipmentBindingId, Is.EqualTo("binding"));
        }

        [TestCase(null, 1)]
        [TestCase("", 1)]
        [TestCase("wood", 0)]
        [TestCase("wood", -1)]
        public void AddRemove_InvalidRequestPreservesContents(string itemId, int amount)
        {
            var slots = new[] { Slot(0, "wood", 3) };
            Assert.That(InventorySlotOperations.TryAddItem(slots, itemId, amount, 10).Success, Is.False);
            Assert.That(InventorySlotOperations.TryRemoveItem(slots, itemId, amount), Is.False);
            Assert.That(slots[0].Amount, Is.EqualTo(3));
        }

        [Test]
        public void Move_PreservesSlotIdentityAndMovesBindingWithContents()
        {
            var source = Slot(2, "wood", 5);
            source.EquipmentBindingId = "retained-binding";
            var target = Slot(7, string.Empty, 0);

            Assert.That(InventorySlotOperations.TryMoveOrMerge(source, target, 10, out _), Is.True);
            Assert.That(source.IsEmpty, Is.True);
            Assert.That(source.EquipmentBindingId, Is.Empty);
            Assert.That(target.EquipmentBindingId, Is.EqualTo("retained-binding"));
            Assert.That(target.Amount, Is.EqualTo(5));
            Assert.That(source.SlotIndex, Is.EqualTo(2));
            Assert.That(target.SlotIndex, Is.EqualTo(7));
        }

        [TestCase(6, 8, 10, 4, 10)]
        [TestCase(2, 3, 10, 0, 5)]
        [TestCase(1, 1, 0, 1, 1)]
        public void Merge_ConservesTotalAndHonorsStackLimit(int sourceAmount, int targetAmount,
            int maxStack, int expectedSource, int expectedTarget)
        {
            var source = Slot(0, "wood", sourceAmount);
            var target = Slot(1, "wood", targetAmount);
            var moved = InventorySlotOperations.TryMoveOrMerge(source, target, maxStack, out var reason);
            Assert.That(moved, Is.EqualTo(expectedSource != sourceAmount));
            Assert.That(string.IsNullOrEmpty(reason), Is.EqualTo(moved));
            Assert.That(source.Amount, Is.EqualTo(expectedSource));
            Assert.That(target.Amount, Is.EqualTo(expectedTarget));
            Assert.That(source.Amount + target.Amount, Is.EqualTo(sourceAmount + targetAmount));
        }

        [Test]
        public void Swap_KeepsPositionIdentityAndExchangesAllContentMetadata()
        {
            var source = Slot(3, "wood", 4);
            var target = Slot(8, "stone", 7);
            source.EquipmentBindingId = "source-binding";
            target.EquipmentBindingId = "target-binding";
            Assert.That(InventorySlotOperations.TryMoveOrMerge(source, target, 10, out _), Is.True);
            Assert.That(source.ItemId, Is.EqualTo("stone"));
            Assert.That(source.Amount, Is.EqualTo(7));
            Assert.That(source.EquipmentBindingId, Is.EqualTo("target-binding"));
            Assert.That(target.ItemId, Is.EqualTo("wood"));
            Assert.That(target.Amount, Is.EqualTo(4));
            Assert.That(target.EquipmentBindingId, Is.EqualTo("source-binding"));
            Assert.That(source.SlotIndex, Is.EqualTo(3));
            Assert.That(target.SlotIndex, Is.EqualTo(8));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EquippedSlot_RejectsMoveWithoutMutation(bool equipSource)
        {
            var source = Slot(0, "wood", 4);
            var target = Slot(1, "stone", 7);
            source.IsEquipped = equipSource;
            target.IsEquipped = !equipSource;
            Assert.That(InventorySlotOperations.TryMoveOrMerge(source, target, 10, out _), Is.False);
            Assert.That(source.ItemId, Is.EqualTo("wood"));
            Assert.That(source.Amount, Is.EqualTo(4));
            Assert.That(target.ItemId, Is.EqualTo("stone"));
            Assert.That(target.Amount, Is.EqualTo(7));
        }

        [Test]
        public void SameSlotOrMissingDestination_RejectsWithoutMutation()
        {
            var source = Slot(0, "wood", 5);
            Assert.That(InventorySlotOperations.TryMoveOrMerge(source, source, 10, out _), Is.False);
            Assert.That(InventorySlotOperations.TryMoveOrMerge(source, null, 10, out _), Is.False);
            Assert.That(InventorySlotOperations.TrySplit(source, source), Is.False);
            Assert.That(InventorySlotOperations.TrySplit(source, null), Is.False);
            Assert.That(source.Amount, Is.EqualTo(5));
        }

        [Test]
        public void SplitOddEquippedStack_PreservesLegacySourceBindingAndTotal()
        {
            var source = Slot(2, "wood", 5);
            source.IsEquipped = true;
            source.EquipmentBindingId = "equipment-slot:Hand";
            var target = Slot(9, string.Empty, 0);
            Assert.That(InventorySlotOperations.TrySplit(source, target), Is.True);
            Assert.That(source.Amount, Is.EqualTo(3));
            Assert.That(target.Amount, Is.EqualTo(2));
            Assert.That(source.IsEquipped, Is.True);
            Assert.That(source.EquipmentBindingId, Is.EqualTo("equipment-slot:Hand"));
            Assert.That(target.IsEquipped, Is.False);
            Assert.That(target.EquipmentBindingId, Is.Empty);
            Assert.That(source.SlotIndex, Is.EqualTo(2));
            Assert.That(target.SlotIndex, Is.EqualTo(9));
        }

        [Test]
        public void SplitIntoOccupiedSlotOrSingleItem_RejectsWithoutMutation()
        {
            var source = Slot(0, "wood", 1);
            var target = Slot(1, string.Empty, 0);
            Assert.That(InventorySlotOperations.TrySplit(source, target), Is.False);
            source.Amount = 5;
            target.ItemId = "stone";
            target.Amount = 2;
            Assert.That(InventorySlotOperations.TrySplit(source, target), Is.False);
            Assert.That(source.Amount, Is.EqualTo(5));
            Assert.That(target.ItemId, Is.EqualTo("stone"));
            Assert.That(target.Amount, Is.EqualTo(2));
        }

        private static InventorySlot Slot(int index, string id, int amount)
        {
            return new InventorySlot { SlotIndex = index, ItemId = id, Amount = amount };
        }
    }
}
