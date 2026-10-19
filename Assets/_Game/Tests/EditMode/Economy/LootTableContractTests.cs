using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Loot;

namespace CindarsHope.Tests.EditMode.Economy
{
    [TestFixture]
    public class LootTableContractTests
    {
        private LootTableDefinition BasicTable(string id = "table_test") => new LootTableDefinition
        {
            LootTableId = id,
            SourceType = LootSourceType.CaveEnemies,
            WeightedDrops = new List<LootEntry>
            {
                new LootEntry { EntryId = "e1", ItemId = "item_bone", Weight = 8, QuantityMin = 1, QuantityMax = 2, Repeatable = true },
                new LootEntry { EntryId = "e2", ItemId = "item_iron_ore", Weight = 2, QuantityMin = 1, QuantityMax = 1, Repeatable = true }
            }
        };

        [Test]
        public void Validator_ValidTable_NoErrors()
        {
            var errors = LootTableValidator.Validate(BasicTable());
            Assert.AreEqual(0, errors.Count, string.Join(", ", errors));
        }

        [Test]
        public void Validator_MissingSourceType_Error()
        {
            var table = BasicTable();
            table.SourceType = LootSourceType.Unknown;
            var errors = LootTableValidator.Validate(table);
            Assert.Greater(errors.Count, 0);
        }

        [Test]
        public void Validator_ProtectedItemInRepeatableLoot_Error()
        {
            var table = BasicTable();
            table.WeightedDrops.Add(new LootEntry { ItemId = "item_fruto_mana", Weight = 1, Repeatable = true });
            var errors = LootTableValidator.Validate(table);
            Assert.IsTrue(errors.Exists(e => e.Contains("protected item")));
        }

        [Test]
        public void Validator_UniqueDropNotMarkedUnique_Error()
        {
            var table = BasicTable();
            table.UniqueDrops.Add(new LootEntry { ItemId = "item_boss_trophy", Repeatable = false });
            var errors = LootTableValidator.Validate(table);
            Assert.IsTrue(errors.Exists(e => e.Contains("IsUniqueReward")));
        }

        [Test]
        public void Validator_ProgressionCriticalRareWithoutPity_Error()
        {
            var table = BasicTable();
            table.RareDrops.Add(new LootEntry
            {
                ItemId = "item_rare_essence",
                IsProgressionCritical = true,
                DropChance = 0.05f,
                Repeatable = true
            });
            var errors = LootTableValidator.Validate(table);
            Assert.IsTrue(errors.Exists(e => e.Contains("PityRules")));
        }

        [Test]
        public void Validator_ProgressionCriticalRareWithPity_NoError()
        {
            var table = BasicTable();
            table.RareDrops.Add(new LootEntry
            {
                ItemId = "item_rare_essence",
                IsProgressionCritical = true,
                DropChance = 0.05f,
                Repeatable = true
            });
            table.PityRules = new PityRules { Enabled = true, PityThreshold = 20, PityGrantItemId = "item_rare_essence" };
            var errors = LootTableValidator.Validate(table);
            Assert.IsFalse(errors.Exists(e => e.Contains("PityRules")));
        }

        [Test]
        public void Resolver_GuaranteedDrop_AlwaysGranted()
        {
            var table = BasicTable();
            table.GuaranteedDrops.Add(new LootEntry { EntryId = "g1", ItemId = "item_gold_coin", QuantityMin = 1, QuantityMax = 1 });
            var ctx = new LootRollContext { TableId = table.LootTableId, Seed = 42, CurrentDay = 1 };
            var resolver = new LootTableResolver();
            var result = resolver.Resolve(table, ctx);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ItemIds.Contains("item_gold_coin"));
        }

        [Test]
        public void Resolver_UniqueDrop_NotGrantedTwice()
        {
            var table = new LootTableDefinition
            {
                LootTableId = "table_boss",
                SourceType = LootSourceType.CaveBosses,
                UniqueDrops = new List<LootEntry>
                {
                    new LootEntry { ItemId = "item_boss_key", IsUniqueReward = true, Repeatable = false, QuantityMin = 1, QuantityMax = 1 }
                }
            };
            var ctx = new LootRollContext { TableId = table.LootTableId, Seed = 1, CurrentDay = 1 };
            var resolver = new LootTableResolver();

            var r1 = resolver.Resolve(table, ctx);
            Assert.IsTrue(r1.ItemIds.Contains("item_boss_key"), "First grant should include boss key");

            // Second resolve — unique already granted
            var r2 = resolver.Resolve(table, ctx);
            Assert.IsFalse(r2.ItemIds.Contains("item_boss_key"), "Second resolve must not re-grant unique");
        }

        [Test]
        public void Resolver_WeightedDrop_SeededDeterministic()
        {
            var table = BasicTable();
            var ctx1 = new LootRollContext { TableId = table.LootTableId, Seed = 100, CurrentDay = 1 };
            var ctx2 = new LootRollContext { TableId = table.LootTableId, Seed = 100, CurrentDay = 1 };
            var resolver = new LootTableResolver();

            var r1 = resolver.Resolve(table, ctx1);
            var r2 = resolver.Resolve(table, ctx2);

            Assert.AreEqual(string.Join(",", r1.ItemIds), string.Join(",", r2.ItemIds), "Same seed must produce same result");
        }

        [Test]
        public void Resolver_FirstTimeBonus_OnlyAppliedOnce()
        {
            var table = BasicTable();
            table.FirstTimeBonus = new FirstTimeBonus { BonusItemIds = new List<string> { "item_tutorial_reward" }, BonusGold = 50, IsConsumed = false };
            var ctx = new LootRollContext { TableId = table.LootTableId, Seed = 7, CurrentDay = 1 };
            var resolver = new LootTableResolver();

            var r1 = resolver.Resolve(table, ctx);
            Assert.IsTrue(r1.FirstTimeBonusApplied);
            Assert.IsTrue(r1.ItemIds.Contains("item_tutorial_reward"));

            var r2 = resolver.Resolve(table, ctx);
            Assert.IsFalse(r2.FirstTimeBonusApplied);
        }

        [Test]
        public void RewardGrantResult_Fail_HasReason()
        {
            var r = RewardGrantResult.Fail("blocked");
            Assert.IsFalse(r.Success);
            Assert.AreEqual("blocked", r.FailureReason);
        }

        [Test]
        public void ProtectedItemIds_FrutoMana_IsProtected()
        {
            Assert.IsTrue(ProtectedItemIds.IsProtected("item_fruto_mana"));
            Assert.IsTrue(ProtectedItemIds.IsProtected("item_agua_viva"));
            Assert.IsFalse(ProtectedItemIds.IsProtected("item_bone"));
        }
    }
}
