using CindarsHope.Inventory;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Inventory
{
    public sealed class PurchasedOriginDerivationTests
    {
        [Test]
        public void PurchasedAndEarned_UsesOwnLiquidationOnceAndDistributesWholeBudget()
        {
            var inputs = new[]
            {
                new ItemOriginDerivation.Input(ItemOriginLot.Purchased(3, 17), 20),
                new ItemOriginDerivation.Input(new[] { new ItemOriginLot(ItemOriginKind.Earned, 2) }, 7)
            };
            Assert.IsTrue(ItemOriginDerivation.TryDerive(inputs, 4, out var outputs));
            long cap = 0, basis = 0, quantity = 0;
            foreach (var lot in outputs)
            {
                Assert.AreEqual(ItemOriginKind.RestrictedDerived, lot.Kind);
                cap += (long)lot.UnitResaleCapGold * lot.Quantity;
                basis += (long)lot.UnitPaidBasisGold * lot.Quantity;
                quantity += lot.Quantity;
            }
            Assert.AreEqual(28, cap); // 3 paid units cap 14 + owned 2*7.
            Assert.AreEqual(17, basis);
            Assert.AreEqual(4, quantity);

            Assert.IsTrue(ItemOriginDerivation.TryDerive(new[]
            {
                new ItemOriginDerivation.Input(outputs, 100)
            }, 8, out var second));
            long secondCap = 0;
            foreach (var lot in second) secondCap += (long)lot.UnitResaleCapGold * lot.Quantity;
            Assert.AreEqual(28, secondCap, "second recipe cannot reprice derived output");
        }

        [Test]
        public void FreePurchasedInputDoesNotEraseOwnedInputBudget()
        {
            var inputs = new[]
            {
                new ItemOriginDerivation.Input(ItemOriginLot.Purchased(1, 0), 90),
                new ItemOriginDerivation.Input(new[] { new ItemOriginLot(ItemOriginKind.Earned, 1) }, 11)
            };
            Assert.IsTrue(ItemOriginDerivation.TryDerive(inputs, 2, out var output));
            Assert.AreEqual(11, output[0].UnitResaleCapGold * output[0].Quantity
                + output[1].UnitResaleCapGold * output[1].Quantity);
        }

        [Test]
        public void InvalidAndOverflowInputsFailWithoutOutput()
        {
            Assert.IsFalse(ItemOriginDerivation.TryDerive(new[]
            {
                new ItemOriginDerivation.Input(new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }, 2)
            }, 1, out _));
            Assert.IsFalse(ItemOriginDerivation.TryDerive(new[]
            {
                new ItemOriginDerivation.Input(new[] { new ItemOriginLot(ItemOriginKind.Earned, 2) }, int.MaxValue)
            }, 1, out _));
        }

        [Test]
        public void OwnOnlyCraftKeepsEarnedButLegacyInputRemainsTraceable()
        {
            Assert.IsTrue(ItemOriginDerivation.TryDerive(new[]
            {
                new ItemOriginDerivation.Input(new[]
                { new ItemOriginLot(ItemOriginKind.Earned, 2) }, 4)
            }, 1, out var earned));
            Assert.AreEqual(ItemOriginKind.Earned, earned[0].Kind);

            Assert.IsTrue(ItemOriginDerivation.TryDerive(new[]
            {
                new ItemOriginDerivation.Input(new[]
                { new ItemOriginLot(ItemOriginKind.LegacyUntracked, 1) }, 4)
            }, 1, out var legacy));
            Assert.AreEqual(ItemOriginKind.LegacyUntracked, legacy[0].Kind);
        }
    }
}
