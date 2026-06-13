using NUnit.Framework;
using CindarsHope.Cave.Runtime;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// Stable-run guarantees for the cave wandering merchant: appearance and stock are
    /// pure functions of (worldSeed, runSeed, caveLevel) — no time, no UnityEngine.Random.
    /// </summary>
    [TestFixture]
    public class CaveWanderingMerchantTests
    {
        [Test]
        public void ShouldAppear_IsDeterministic_ForSameSeedsAndLevel()
        {
            for (int level = 1; level <= 101; level++)
            {
                bool first = CaveWanderingMerchant.ShouldAppear("world_a", "run_b", level);
                bool second = CaveWanderingMerchant.ShouldAppear("world_a", "run_b", level);
                Assert.AreEqual(first, second, $"Appearance flipped between calls on level {level}.");
            }
        }

        [Test]
        public void ShouldAppear_ChangesWithRunSeed()
        {
            // A new run (death/new game) must be able to reroll merchant levels.
            int differences = 0;
            for (int level = 1; level <= 101; level++)
            {
                if (CaveWanderingMerchant.ShouldAppear("world_a", "run_1", level)
                    != CaveWanderingMerchant.ShouldAppear("world_a", "run_2", level))
                {
                    differences++;
                }
            }

            Assert.Greater(differences, 0, "Different run seeds should produce different merchant levels.");
        }

        [Test]
        public void ShouldAppear_RateIsNearConfiguredChance()
        {
            int appearances = 0;
            const int samples = 2000;
            for (int level = 0; level < samples; level++)
            {
                if (CaveWanderingMerchant.ShouldAppear("world_x", "run_y", level))
                {
                    appearances++;
                }
            }

            float rate = appearances * 100f / samples;
            Assert.That(rate, Is.InRange(CaveWanderingMerchant.AppearanceChancePercent - 8f,
                                         CaveWanderingMerchant.AppearanceChancePercent + 8f),
                $"Observed appearance rate {rate:F1}% too far from configured {CaveWanderingMerchant.AppearanceChancePercent}%.");
        }

        [Test]
        public void ResolveOfferIndices_AreDeterministicAndDistinct()
        {
            for (int level = 1; level <= 101; level++)
            {
                var (firstA, secondA) = CaveWanderingMerchant.ResolveOfferIndices("world_a", "run_b", level);
                var (firstB, secondB) = CaveWanderingMerchant.ResolveOfferIndices("world_a", "run_b", level);

                Assert.AreEqual(firstA, firstB, $"First offer changed between calls on level {level}.");
                Assert.AreEqual(secondA, secondB, $"Second offer changed between calls on level {level}.");
                Assert.AreNotEqual(firstA, secondA, $"Offers must be distinct on level {level}.");
                Assert.That(firstA, Is.InRange(0, CaveWanderingMerchant.OfferCatalog.Length - 1));
                Assert.That(secondA, Is.InRange(0, CaveWanderingMerchant.OfferCatalog.Length - 1));
            }
        }

        [Test]
        public void OfferCatalog_HasValidEntries()
        {
            Assert.GreaterOrEqual(CaveWanderingMerchant.OfferCatalog.Length, 2);
            foreach (var offer in CaveWanderingMerchant.OfferCatalog)
            {
                Assert.IsNotEmpty(offer.ItemId);
                Assert.Greater(offer.Amount, 0);
                Assert.GreaterOrEqual(offer.TotalCost, 0);
                Assert.IsNotEmpty(offer.Prompt);
            }
        }
    }
}
