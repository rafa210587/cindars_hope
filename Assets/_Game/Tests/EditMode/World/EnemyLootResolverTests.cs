using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Loot;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_06 CA-1 — EnemyLootResolver rola uma LootTableSO de forma DETERMINÍSTICA por seed
    /// (ADR-0005 / cave-stable-run): mesma instância na mesma run => mesmo loot; runs diferentes =>
    /// distribuições diferentes. Distribuição ~pesos em N rolagens. Tabela vazia/nula => sem drop.
    /// </summary>
    [TestFixture]
    public class EnemyLootResolverTests
    {
        private static LootTableSO MakeTable(
            LootTableEntry[] entries,
            LootTableEntry[] guaranteed = null,
            string essence = null,
            float essenceCommon = 0.08f,
            float essenceEliteBonus = 0.25f)
        {
            var t = ScriptableObject.CreateInstance<LootTableSO>();
            t.TableId = "loot_test";
            t.FamilyId = "test";
            t.Entries = entries ?? new LootTableEntry[0];
            t.GuaranteedEntries = guaranteed ?? new LootTableEntry[0];
            t.EssenceItemId = essence ?? string.Empty;
            t.EssenceCommonChance = essenceCommon;
            t.EssenceEliteBonusChance = essenceEliteBonus;
            return t;
        }

        private static LootTableEntry W(string id, int weight, int min = 1, int max = 1, float chance = 1f) =>
            new LootTableEntry { ItemId = id, Weight = weight, MinAmount = min, MaxAmount = max, DropChance = chance };

        // ── Determinism (CA-1) ───────────────────────────────────────────────────────────────

        [Test]
        public void Roll_SameSeed_IsDeterministic()
        {
            var table = MakeTable(new[] { W("item_a", 3), W("item_b", 1) }, essence: "item_essence_fire");
            int seed = EnemyLootResolver.BuildLootSeed("run_001", "enemy_1_room_0_3_slime_abcd");

            var first = EnemyLootResolver.Roll(table, seed, isElite: true);
            var second = EnemyLootResolver.Roll(table, seed, isElite: true);

            Assert.AreEqual(first.Count, second.Count, "Same seed must produce same number of drops.");
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].ItemId, second[i].ItemId, $"Drop {i} itemId must match.");
                Assert.AreEqual(first[i].Amount, second[i].Amount, $"Drop {i} amount must match.");
            }
        }

        [Test]
        public void BuildLootSeed_SameRunAndInstance_SameSeed_DifferentRun_DifferentSeed()
        {
            int a = EnemyLootResolver.BuildLootSeed("run_001", "enemy_X");
            int aAgain = EnemyLootResolver.BuildLootSeed("run_001", "enemy_X");
            int b = EnemyLootResolver.BuildLootSeed("run_002", "enemy_X");

            Assert.AreEqual(a, aAgain, "Same run + instance must yield the same loot seed (stable run).");
            Assert.AreNotEqual(a, b, "Different run seed should yield a different loot seed.");
        }

        [Test]
        public void Roll_DifferentRuns_ProduceDifferentDistributions()
        {
            var table = MakeTable(new[] { W("item_a", 1), W("item_b", 1), W("item_c", 1), W("item_d", 1) });

            // Two different runs over the same instance index should not be forced identical.
            int diffs = 0;
            for (int i = 0; i < 200; i++)
            {
                int seedA = EnemyLootResolver.BuildLootSeed("run_A", $"enemy_{i}");
                int seedB = EnemyLootResolver.BuildLootSeed("run_B", $"enemy_{i}");
                var a = EnemyLootResolver.Roll(table, seedA);
                var b = EnemyLootResolver.Roll(table, seedB);
                if (a.Count == 0 || b.Count == 0) continue;
                if (a[0].ItemId != b[0].ItemId) diffs++;
            }

            Assert.Greater(diffs, 0, "Different runs should produce at least some different weighted picks.");
        }

        // ── Distribution ~ weights (CA-1) ──────────────────────────────────────────────────────

        [Test]
        public void Roll_WeightedDistribution_ApproximatesWeights()
        {
            // item_a weight 3, item_b weight 1 → ~75% / ~25% over many seeds.
            var table = MakeTable(new[] { W("item_a", 3), W("item_b", 1) });

            int countA = 0, countB = 0;
            const int rolls = 4000;
            for (int i = 0; i < rolls; i++)
            {
                var drops = EnemyLootResolver.Roll(table, i);
                foreach (var d in drops)
                {
                    if (d.ItemId == "item_a") countA++;
                    else if (d.ItemId == "item_b") countB++;
                }
            }

            float ratioA = (float)countA / rolls;
            // Expect ~0.75 for item_a; allow generous tolerance for RNG variance.
            Assert.That(ratioA, Is.InRange(0.68f, 0.82f), $"item_a share {ratioA:F3} should be near 0.75 (countA={countA}, countB={countB}).");
            Assert.Greater(countA, countB, "Higher weight must win more often.");
        }

        // ── Guaranteed + essence ────────────────────────────────────────────────────────────────

        [Test]
        public void Roll_GuaranteedEntry_AlwaysDrops()
        {
            var table = MakeTable(
                entries: new[] { W("item_weighted", 1) },
                guaranteed: new[] { new LootTableEntry { ItemId = "item_guaranteed", Weight = 1, MinAmount = 1, MaxAmount = 2, DropChance = 1f } });

            for (int seed = 0; seed < 50; seed++)
            {
                var drops = EnemyLootResolver.Roll(table, seed);
                Assert.IsTrue(drops.Exists(d => d.ItemId == "item_guaranteed"), $"Guaranteed drop missing at seed {seed}.");
            }
        }

        [Test]
        public void Roll_Essence_BossAlwaysGetsEssence_CommonRarelyDoes()
        {
            var table = MakeTable(new[] { W("item_a", 1) }, essence: "item_essence_void");

            // Boss/miniboss: essence guaranteed (100%).
            for (int seed = 0; seed < 30; seed++)
            {
                var bossDrops = EnemyLootResolver.Roll(table, seed, isElite: false, isMinibossOrBoss: true);
                Assert.IsTrue(bossDrops.Exists(d => d.ItemId == "item_essence_void"), $"Boss must always drop essence (seed {seed}).");
            }

            // Common: essence ~8% — should be the minority over many rolls.
            int essenceCount = 0;
            const int rolls = 2000;
            for (int seed = 0; seed < rolls; seed++)
            {
                var drops = EnemyLootResolver.Roll(table, seed, isElite: false, isMinibossOrBoss: false);
                if (drops.Exists(d => d.ItemId == "item_essence_void")) essenceCount++;
            }
            float ratio = (float)essenceCount / rolls;
            Assert.That(ratio, Is.InRange(0.03f, 0.14f), $"Common essence rate {ratio:F3} should be near 0.08.");
        }

        [Test]
        public void Roll_EliteHasHigherEssenceRateThanCommon()
        {
            var table = MakeTable(new[] { W("item_a", 1) }, essence: "item_essence_fire", essenceCommon: 0.08f, essenceEliteBonus: 0.25f);

            int common = 0, elite = 0;
            const int rolls = 3000;
            for (int seed = 0; seed < rolls; seed++)
            {
                if (EnemyLootResolver.Roll(table, seed, isElite: false).Exists(d => d.ItemId == "item_essence_fire")) common++;
                if (EnemyLootResolver.Roll(table, seed, isElite: true).Exists(d => d.ItemId == "item_essence_fire")) elite++;
            }

            Assert.Greater(elite, common, $"Elite essence rate ({elite}) must exceed common ({common}).");
        }

        // ── Invalid entries / empty fallback (CA-1 legacy) ──────────────────────────────────────

        [Test]
        public void Roll_NullTable_ReturnsEmpty_WithWarning()
        {
            var warnings = new List<string>();
            var drops = EnemyLootResolver.Roll(null, 123, warnings: warnings);
            Assert.AreEqual(0, drops.Count);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void Roll_EmptyTable_ReturnsEmpty()
        {
            var table = MakeTable(new LootTableEntry[0]);
            var drops = EnemyLootResolver.Roll(table, 42);
            Assert.AreEqual(0, drops.Count, "Empty table must drop nothing (legacy fallback handled by spawner).");
        }

        [Test]
        public void Roll_InvalidEntries_AreIgnoredWithWarnings()
        {
            var table = MakeTable(new[]
            {
                new LootTableEntry { ItemId = "", Weight = 5, MinAmount = 1, MaxAmount = 1 },        // empty id
                new LootTableEntry { ItemId = "item_zero_weight", Weight = 0, MinAmount = 1, MaxAmount = 1 }, // weight 0
                W("item_valid", 1)
            });

            var warnings = new List<string>();
            var drops = EnemyLootResolver.Roll(table, 7, warnings: warnings);

            foreach (var d in drops)
            {
                Assert.AreEqual("item_valid", d.ItemId, "Only the valid entry may drop.");
            }
            Assert.GreaterOrEqual(warnings.Count, 2, "Both invalid entries should warn.");
        }

        [Test]
        public void StableHash_MatchesFnv1aReference()
        {
            // Parity guard with CaveEnemySpawnPlanner.StableHash (FNV-1a). If the cave planner ever
            // changes its hash, this reference must change too or stable-run loot would diverge.
            Assert.AreEqual(Fnv1a("run_001|enemy_X|loot"), EnemyLootResolver.StableHash("run_001|enemy_X|loot"));
            Assert.AreEqual(Fnv1a(""), EnemyLootResolver.StableHash(""));
            Assert.AreEqual(Fnv1a("a"), EnemyLootResolver.StableHash("a"));
        }

        private static int Fnv1a(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }
                return hash == int.MinValue ? 0 : hash;
            }
        }
    }
}
