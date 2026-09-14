using CindarsHope.Economy.Pricing;
using CindarsHope.Inventory;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Economy
{
    public sealed class OriginAwareSaleQuoteTests
    {
        [Test]
        public void MixedBread_CapsOnlyPurchasedUnitAfterNormalBonus()
        {
            var lots = new[]
            {
                new ItemOriginLot(ItemOriginKind.Purchased, 1, 17, 16),
                new ItemOriginLot(ItemOriginKind.Earned, 1)
            };

            var quote = OriginAwareSaleQuote.Calculate(lots, new[] { 30, 20 });

            Assert.That(quote.Success, Is.True);
            Assert.That(quote.NormalGold, Is.EqualTo(50));
            Assert.That(quote.FinalGold, Is.EqualTo(36));
            Assert.That(quote.LimitedUnits, Is.EqualTo(1));
            Assert.That(quote.LimitedGold, Is.EqualTo(14));
        }

        [Test]
        public void ZeroCostPurchasedUnit_ReturnsZeroWithoutTouchingLegacyPrice()
        {
            var lots = new[]
            {
                new ItemOriginLot(ItemOriginKind.Purchased, 1, 0, 0),
                new ItemOriginLot(ItemOriginKind.LegacyUntracked, 1)
            };

            var quote = OriginAwareSaleQuote.Calculate(lots, new[] { 20, 20 });

            Assert.That(quote.Success, Is.True);
            Assert.That(quote.FinalGold, Is.EqualTo(20));
        }

        [Test]
        public void CorruptOrOverflowQuote_IsRejected()
        {
            Assert.That(OriginAwareSaleQuote.Calculate(
                new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }, new[] { 20 }).Success,
                Is.False);
            Assert.That(OriginAwareSaleQuote.Calculate(
                new[] { new ItemOriginLot(ItemOriginKind.Purchased, 1, 17, 20) },
                new[] { 20 }).Success, Is.False);
            Assert.That(OriginAwareSaleQuote.Calculate(
                new[] { new ItemOriginLot(ItemOriginKind.Purchased, 1, 17, 0) },
                new[] { 20 }).Success, Is.False);
            Assert.That(OriginAwareSaleQuote.Calculate(
                new[] { new ItemOriginLot(ItemOriginKind.Earned, 2) },
                new[] { int.MaxValue, 1 }).Success, Is.False);
        }

        [Test]
        public void AggregateBonus_UsesStableLargestRemainderBeforeCaps()
        {
            Assert.That(OriginAwareSaleQuote.TryAllocateAggregateBonus(
                new[] { 10, 10, 10 }, 32, out var units), Is.True);
            Assert.That(units, Is.EqualTo(new[] { 11, 11, 10 }));

            var lots = new[]
            {
                new ItemOriginLot(ItemOriginKind.Purchased, 1, 10, 9),
                new ItemOriginLot(ItemOriginKind.Earned, 2)
            };
            var quote = OriginAwareSaleQuote.Calculate(lots, units);
            Assert.That(quote.Success, Is.True);
            Assert.That(quote.NormalGold, Is.EqualTo(32));
            Assert.That(quote.FinalGold, Is.EqualTo(30));
        }
    }
}
