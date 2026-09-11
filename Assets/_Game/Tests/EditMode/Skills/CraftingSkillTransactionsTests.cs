using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class CraftingSkillTransactionsTests
    {
        [TearDown]
        public void TearDown() => SkillModifierHooks.ResetAll();

        [Test]
        public void FieldPatch_ConsumesOneBasicKit_AndStopsAtSixtyPercent()
        {
            var inventory = new FakeInventory(CraftingRepairItemIds.BasicKit, 1);
            var equipment = new FakeEquipment(new FieldRepairTarget("sword", 55, 100, 0, false, false));

            var result = new CraftingRepairTransaction(inventory, equipment)
                .TryCommit(CraftingRepairItemIds.BasicKit, .20f, .60f);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Restored, Is.EqualTo(5));
            Assert.That(inventory.Amount, Is.Zero);
            Assert.That(equipment.Current, Is.EqualTo(60));
        }

        [TestCase(true, false, FieldRepairFailure.BrokenEquipment)]
        [TestCase(false, true, FieldRepairFailure.ArtifactEquipment)]
        public void ForbiddenTarget_DoesNotConsumeKit(bool broken, bool artifact, FieldRepairFailure expected)
        {
            var inventory = new FakeInventory(CraftingRepairItemIds.StandardKit, 1);
            var equipment = new FakeEquipment(new FieldRepairTarget("gear", broken ? 0 : 20, 100, 0, broken, artifact));

            var result = new CraftingRepairTransaction(inventory, equipment)
                .TryCommit(CraftingRepairItemIds.StandardKit, 0f, .85f);

            Assert.That(result.Failure, Is.EqualTo(expected));
            Assert.That(inventory.Amount, Is.EqualTo(1));
        }

        [Test]
        public void EquipmentChangeAfterMaterialRemoval_RollsKitBack()
        {
            var inventory = new FakeInventory(CraftingRepairItemIds.BasicKit, 1);
            var equipment = new FakeEquipment(new FieldRepairTarget("axe", 20, 100, 0, false, false))
                { FailCommit = true };

            var result = new CraftingRepairTransaction(inventory, equipment)
                .TryCommit(CraftingRepairItemIds.BasicKit, .10f, .60f);

            Assert.That(result.Failure, Is.EqualTo(FieldRepairFailure.EquipmentChanged));
            Assert.That(inventory.Amount, Is.EqualTo(1));
            Assert.That(equipment.Current, Is.EqualTo(20));
        }

        [Test]
        public void Catalog_AuthorsRepairRanksCooldownsCapsAndChannels()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var patch = actions.Find(a => a.SkillActionId == SkillActionEffectCatalog.CraftingFieldPatchActionId);
                var quick = actions.Find(a => a.SkillActionId == SkillActionEffectCatalog.CraftingQuickRepairActionId);
                Assert.That(patch.NotYetExecutable, Is.False);
                Assert.That(patch.CooldownSeconds, Is.EqualTo(20f));
                Assert.That(patch.EffectMagnitudeByRank, Is.EqualTo(new[] { .10f, .15f, .20f }));
                Assert.That(patch.SecondaryMagnitudeByRank[0], Is.EqualTo(.60f));
                Assert.That(quick.NotYetExecutable, Is.False);
                Assert.That(quick.CooldownSeconds, Is.EqualTo(30f));
                Assert.That(quick.SecondaryMagnitudeByRank[0], Is.EqualTo(.85f));
                Assert.That(quick.SecondaryDurationByRank, Is.EqualTo(new[] { 1.2f, 1f, .8f }));
            }
            finally
            {
                foreach (var action in actions) Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void CraftingPassives_AuthorsTypedChannelsRanksAndExecutionFlags()
        {
            var nodes = DefaultSkillCatalog.BuildAllNodes();
            try
            {
                var fast = nodes.Find(n => n.SkillNodeId == "crafting_fast_hands");
                var repair = nodes.Find(n => n.SkillNodeId == "crafting_repair_care");
                var station = nodes.Find(n => n.SkillNodeId == "crafting_station_focus");
                var salvage = nodes.Find(n => n.SkillNodeId == "crafting_salvage_method");
                var pack = nodes.Find(n => n.SkillNodeId == "crafting_pack_order");
                var shop = nodes.Find(n => n.SkillNodeId == "crafting_shop_sense");

                Assert.That(fast.AuthoredMaxRank, Is.EqualTo(5));
                Assert.That(fast.PassiveModifiers.Single().ModifierType,
                    Is.EqualTo(SkillModifierType.CraftTimeReductionPercent));
                Assert.That(fast.PassiveModifiers.Single().Value, Is.EqualTo(.10f));
                Assert.That(repair.AuthoredMaxRank, Is.EqualTo(3));
                Assert.That(repair.PassiveModifiers.Single().ModifierType,
                    Is.EqualTo(SkillModifierType.RepairEfficiencyBonus));
                Assert.That(station.AuthoredMaxRank, Is.EqualTo(5));
                Assert.That(station.PassiveModifiers.Single().ModifierType,
                    Is.EqualTo(SkillModifierType.StationCommonMaterialReduction));
                Assert.That(station.EffectPending, Is.False);
                Assert.That(salvage.AuthoredMaxRank, Is.EqualTo(3));
                Assert.That(salvage.EffectRoute, Is.EqualTo(SkillEffectRoute.None));
                Assert.That(salvage.EffectPending, Is.False);
                Assert.That(pack.AuthoredMaxRank, Is.EqualTo(1));
                Assert.That(pack.NotYetExecutable, Is.True);
                Assert.That(shop.AuthoredMaxRank, Is.EqualTo(3));
                Assert.That(shop.CapstoneVariants, Is.EqualTo(new[] { "buy", "sell" }));
                Assert.That(shop.EffectRoute, Is.EqualTo(SkillEffectRoute.None));
                Assert.That(shop.PassiveModifiers, Is.Empty);
                Assert.That(shop.EffectPending, Is.False);
            }
            finally
            {
                foreach (var node in nodes) Object.DestroyImmediate(node);
            }
        }

        [Test]
        public void FastHandsAndRepairCare_RespectCombinedCaps()
        {
            Assert.That(DerivedFollowupFormulas.CraftTimeMultiplier(.80f),
                Is.EqualTo(.40f).Within(.0001f));
            Assert.That(DerivedFollowupFormulas.EffectiveRepairAmount(10, .80f),
                Is.EqualTo(13));
        }

        [Test]
        public void DurableFinish_RankAndDurabilityCapsAreDeterministic()
        {
            Assert.That(CraftedItemDurabilityProvider.ResolveBonus(1), Is.EqualTo(.08f));
            Assert.That(CraftedItemDurabilityProvider.ResolveBonus(3), Is.EqualTo(.24f));
            Assert.That(CraftedItemDurabilityProvider.ResolveBonus(9), Is.EqualTo(.24f));
            Assert.That(CraftedItemDurabilityProvider.ResolveMaxDurability(100, .24f),
                Is.EqualTo(124));
        }

        [Test]
        public void StationFocus_DerivesMaterialChannelWithoutReducingCraftTime()
        {
            var stats = DerivedStatsCalculator.Calculate(
                100, 0, 0, 0f, 100, 0f, 1f, null,
                new List<SkillPassiveModifier>
                {
                    new SkillPassiveModifier(
                        SkillModifierType.StationCommonMaterialReduction, .25f)
                });

            Assert.That(stats.StationCommonMaterialReduction, Is.EqualTo(.25f));
            Assert.That(stats.CraftTimeReduction, Is.Zero);
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(5, 4)]
        [TestCase(10, 8)]
        public void StationFocus_CommonIngredientFixturesUseCeilAndMinimumOne(
            int baseAmount, int expected)
        {
            Assert.That(CraftingPassiveConsumers.ResolveIngredientAmount(
                baseAmount, .25f, true), Is.EqualTo(expected));
        }

        [Test]
        public void StationFocus_WrongStationAndRareIngredientReceiveNoReduction()
        {
            Assert.That(CraftingPassiveConsumers.ResolveStationMaterialReduction(
                .25f, WorkshopType.Forge, WorkshopType.Workbench), Is.Zero);
            Assert.That(CraftingPassiveConsumers.ResolveIngredientAmount(10, .25f, false),
                Is.EqualTo(10));
        }

        [Test]
        public void StationFocus_ConsumptionAndCancelUseSameReducedRequirements()
        {
            var fixture = new StationFixture(includeOutput: true, instantaneous: false);
            try
            {
                SkillModifierHooks.PublishCraftCostReduction(.25f);

                Assert.That(fixture.Station.TryStartCraft(
                    fixture.Recipe, fixture.Inventory, out var reason), Is.True, reason);
                Assert.That(fixture.Inventory.GetAmount("item_common"), Is.EqualTo(2));
                Assert.That(fixture.Inventory.GetAmount("item_rare"), Is.Zero);
                Assert.That(fixture.Station.Job.IngredientsConsumed[0].Amount, Is.EqualTo(8));
                Assert.That(fixture.Station.Job.IngredientsConsumed[1].Amount, Is.EqualTo(10));

                Assert.That(fixture.Station.TryCancelJob(fixture.Inventory, out reason),
                    Is.True, reason);
                Assert.That(fixture.Inventory.GetAmount("item_common"), Is.EqualTo(10));
                Assert.That(fixture.Inventory.GetAmount("item_rare"), Is.EqualTo(10));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void StationFocus_OutputFailureRollsBackReducedConsumption()
        {
            var fixture = new StationFixture(includeOutput: false, instantaneous: true);
            try
            {
                SkillModifierHooks.PublishCraftCostReduction(.25f);

                Assert.That(fixture.Station.TryStartCraft(
                    fixture.Recipe, fixture.Inventory, out _), Is.False);
                Assert.That(fixture.Inventory.GetAmount("item_common"), Is.EqualTo(10));
                Assert.That(fixture.Inventory.GetAmount("item_rare"), Is.EqualTo(10));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void DurableFinish_DelayedJobSnapshotsIdentityAndMaxDurability()
        {
            var fixture = new DurableFinishFixture();
            try
            {
                long sequence = 7;
                Assert.That(fixture.Station.TryStartCraft(
                    fixture.Recipe, fixture.Inventory, out var reason,
                    reserveOutputInstanceId: id => $"{id}#crafted-{sequence++}",
                    resolveBaseDurability: _ => 100,
                    craftedDurabilityBonus: .24f), Is.True, reason);

                Assert.That(fixture.Station.Job.OutputInstanceId,
                    Is.EqualTo("item_crafted_gear#crafted-7"));
                Assert.That(fixture.Station.Job.OutputDurabilityMax, Is.EqualTo(124));

                var restored = new CraftingJob(
                    fixture.Station.Job.CaptureSaveData(), fixture.Recipe);
                Assert.That(restored.OutputInstanceId,
                    Is.EqualTo("item_crafted_gear#crafted-7"));
                Assert.That(restored.OutputDurabilityMax, Is.EqualTo(124));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void DurableFinish_CollectAddsInstanceAndInitializesSnapshottedDurability()
        {
            var fixture = new DurableFinishFixture();
            try
            {
                var tracker = new EquipmentDurabilityTracker();
                Assert.That(fixture.Station.TryStartCraft(
                    fixture.Recipe, fixture.Inventory, out var reason,
                    reserveOutputInstanceId: id => id + "#crafted-1",
                    resolveBaseDurability: _ => 100,
                    craftedDurabilityBonus: .16f), Is.True, reason);

                fixture.Station.Update(10f);
                Assert.That(fixture.Station.TryCollectOutput(
                    fixture.Inventory, out reason, tracker.InitializeEquipment), Is.True, reason);

                var output = fixture.Inventory.GetAllItems()
                    .Single(item => item.ItemId == "item_crafted_gear");
                Assert.That(output.ItemInstanceId, Is.EqualTo("item_crafted_gear#crafted-1"));
                Assert.That(tracker.GetDurability(output.ItemInstanceId).MaxDurability,
                    Is.EqualTo(116));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void DurableFinish_FullInventoryLeavesCompletedOutputAtStation()
        {
            var fixture = new DurableFinishFixture();
            try
            {
                Assert.That(fixture.Station.TryStartCraft(
                    fixture.Recipe, fixture.Inventory, out var reason,
                    reserveOutputInstanceId: id => id + "#crafted-1",
                    resolveBaseDurability: _ => 100,
                    craftedDurabilityBonus: .08f), Is.True, reason);

                for (var i = 0; i < InventoryManager.MaxCapacity; i++)
                {
                    Assert.That(fixture.Inventory.TryAddItemInstance(
                        "item_filler", $"item_filler#fill-{i}").Success, Is.True);
                }

                fixture.Station.Update(10f);
                Assert.That(fixture.Station.TryCollectOutput(
                    fixture.Inventory, out reason, (_, __) => Assert.Fail("Durability initialized before collection.")),
                    Is.False);
                Assert.That(fixture.Station.HasCompletedOutput, Is.True);
                Assert.That(fixture.Station.Job.OutputInstanceId,
                    Is.EqualTo("item_crafted_gear#crafted-1"));
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void CraftedItemSequence_RoundTripIsMonotonicAndLegacySafe()
        {
            var first = new RuntimeFixture();
            var second = new RuntimeFixture();
            try
            {
                Assert.That(first.Runtime.ReserveCraftedItemInstanceId("item_sword"),
                    Is.EqualTo("item_sword#crafted-1"));
                Assert.That(first.Runtime.ReserveCraftedItemInstanceId("item_sword"),
                    Is.EqualTo("item_sword#crafted-2"));

                second.Runtime.LoadFromSaveData(first.Runtime.CaptureSaveData());
                Assert.That(second.Runtime.ReserveCraftedItemInstanceId("item_sword"),
                    Is.EqualTo("item_sword#crafted-3"));

                second.Runtime.LoadFromSaveData(new CraftingRuntimeSaveData
                {
                    NextCraftedItemSequence = 0
                });
                Assert.That(second.Runtime.ReserveCraftedItemInstanceId("item_sword"),
                    Is.EqualTo("item_sword#crafted-1"));

                var legacyCounterWithNewJob = new CraftingRuntimeSaveData
                {
                    NextCraftedItemSequence = 0
                };
                legacyCounterWithNewJob.Stations.Add(new CraftingStationSaveData
                {
                    StationInstanceId = "forge_saved",
                    StationType = (int)WorkshopType.Forge,
                    StationLevel = 1,
                    Job = new CraftingJobSaveData
                    {
                        RecipeId = RuntimeFixture.RecipeId,
                        OutputItemId = "item_sword",
                        OutputAmount = 1,
                        OutputInstanceId = "item_sword#crafted-9"
                    }
                });
                second.Runtime.LoadFromSaveData(legacyCounterWithNewJob);
                Assert.That(second.Runtime.ReserveCraftedItemInstanceId("item_sword"),
                    Is.EqualTo("item_sword#crafted-10"));
            }
            finally
            {
                first.Dispose();
                second.Dispose();
            }
        }

        private sealed class FakeInventory : IReversibleSkillItemInventory
        {
            private readonly string _itemId;
            public int Amount { get; private set; }
            public FakeInventory(string itemId, int amount) { _itemId = itemId; Amount = amount; }
            public int GetAmount(string itemId) => itemId == _itemId ? Amount : 0;
            public bool RemoveItem(string itemId, int amount)
            {
                if (itemId != _itemId || amount <= 0 || Amount < amount) return false;
                Amount -= amount;
                return true;
            }
            public bool AddItem(string itemId, int amount)
            {
                if (itemId != _itemId || amount <= 0) return false;
                Amount += amount;
                return true;
            }
        }

        private sealed class StationFixture : System.IDisposable
        {
            private readonly GameObject _host;
            private readonly ItemDatabaseSO _database;
            private readonly List<Object> _assets = new List<Object>();

            public InventoryManager Inventory { get; }
            public CraftingStation Station { get; }
            public RecipeDataSO Recipe { get; }

            public StationFixture(bool includeOutput, bool instantaneous)
            {
                var common = Item("item_common", ItemCategory.Material, commonMaterial: true);
                var rare = Item("item_rare", ItemCategory.Essence);
                var items = new List<ItemDataSO> { common, rare };
                if (includeOutput) items.Add(Item("item_output", ItemCategory.Material));

                _database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                _assets.Add(_database);
                typeof(DataRegistrySO<ItemDataSO>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_database, items.ToArray());
                typeof(DataRegistrySO<ItemDataSO>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_database, false);

                _host = new GameObject("crafting-passive-fixture");
                _host.SetActive(false);
                Inventory = _host.AddComponent<InventoryManager>();
                Inventory.Initialize(_database);
                Assert.That(Inventory.AddItem(common.Id, 10), Is.True);
                Assert.That(Inventory.AddItem(rare.Id, 10), Is.True);

                Recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                _assets.Add(Recipe);
                Recipe.SetId("recipe_station_focus_fixture");
                Recipe.RequiredStationType = WorkshopType.Workbench;
                Recipe.OutputItemId = "item_output";
                Recipe.OutputAmount = 1;
                Recipe.CraftTimeSeconds = instantaneous ? 0f : 10f;
                Recipe.Ingredients = new[]
                {
                    new RecipeIngredient(common.Id, 10),
                    new RecipeIngredient(rare.Id, 10)
                };
                Station = new CraftingStation("workbench_fixture", WorkshopType.Workbench);
            }

            private ItemDataSO Item(string id, ItemCategory category, bool commonMaterial = false)
            {
                var item = ScriptableObject.CreateInstance<ItemDataSO>();
                item.Id = id;
                item.Category = category;
                item.MaxStack = 99;
                item.IsCommonMaterialBonusEligible = commonMaterial;
                _assets.Add(item);
                return item;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_host);
                foreach (var asset in _assets)
                    if (asset != null) Object.DestroyImmediate(asset);
            }
        }

        private sealed class DurableFinishFixture : System.IDisposable
        {
            private readonly GameObject _host;
            private readonly List<Object> _assets = new List<Object>();

            public InventoryManager Inventory { get; }
            public CraftingStation Station { get; }
            public RecipeDataSO Recipe { get; }

            public DurableFinishFixture()
            {
                var input = Item("item_input", 99);
                var output = Item("item_crafted_gear", 1);
                var filler = Item("item_filler", 1);
                var database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                _assets.Add(database);
                typeof(DataRegistrySO<ItemDataSO>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                    database, new[] { input, output, filler });
                typeof(DataRegistrySO<ItemDataSO>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, false);

                _host = new GameObject("durable-finish-fixture");
                _host.SetActive(false);
                Inventory = _host.AddComponent<InventoryManager>();
                Inventory.Initialize(database);
                Assert.That(Inventory.AddItem(input.Id, 1), Is.True);

                Recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                _assets.Add(Recipe);
                Recipe.SetId("recipe_durable_finish_fixture");
                Recipe.RequiredStationType = WorkshopType.Forge;
                Recipe.OutputItemId = output.Id;
                Recipe.OutputAmount = 1;
                Recipe.CraftTimeSeconds = 10f;
                Recipe.Ingredients = new[] { new RecipeIngredient(input.Id, 1) };
                Station = new CraftingStation("forge_fixture", WorkshopType.Forge);
            }

            private ItemDataSO Item(string id, int maxStack)
            {
                var item = ScriptableObject.CreateInstance<ItemDataSO>();
                item.Id = id;
                item.Category = ItemCategory.Weapon;
                item.MaxStack = maxStack;
                _assets.Add(item);
                return item;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_host);
                foreach (var asset in _assets)
                    if (asset != null) Object.DestroyImmediate(asset);
            }
        }

        private sealed class RuntimeFixture : System.IDisposable
        {
            public const string RecipeId = "recipe_runtime_fixture";
            private readonly GameObject _host;
            private readonly ItemDatabaseSO _itemDatabase;
            private readonly RecipeDatabaseSO _recipeDatabase;
            private readonly RecipeDataSO _recipe;
            public CraftingRuntime Runtime { get; }

            public RuntimeFixture()
            {
                _itemDatabase = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                _recipeDatabase = ScriptableObject.CreateInstance<RecipeDatabaseSO>();
                _recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                _recipe.SetId(RecipeId);
                typeof(DataRegistrySO<RecipeDataSO>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                    _recipeDatabase, new[] { _recipe });
                typeof(DataRegistrySO<RecipeDataSO>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_recipeDatabase, false);
                _host = new GameObject("crafting-runtime-sequence-fixture");
                _host.SetActive(false);
                var inventory = _host.AddComponent<InventoryManager>();
                inventory.Initialize(_itemDatabase);
                Runtime = _host.AddComponent<CraftingRuntime>();
                typeof(CraftingRuntime).GetField("_recipeDatabase",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(Runtime, _recipeDatabase);
                Runtime.RebindInventoryManager(inventory);
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_host);
                Object.DestroyImmediate(_itemDatabase);
                Object.DestroyImmediate(_recipeDatabase);
                Object.DestroyImmediate(_recipe);
            }
        }

        private sealed class FakeEquipment : IFieldRepairEquipment
        {
            private FieldRepairTarget _target;
            public bool FailCommit { get; set; }
            public int Current => _target.CurrentDurability;
            public FakeEquipment(FieldRepairTarget target) { _target = target; }
            public IReadOnlyList<FieldRepairTarget> GetEquippedTargets() => new[] { _target };
            public bool TryApplyExactRepair(FieldRepairTarget expected, int amount, int capDurability, out int restored)
            {
                restored = 0;
                if (FailCommit || expected.CurrentDurability != _target.CurrentDurability) return false;
                restored = System.Math.Min(amount, capDurability - _target.CurrentDurability);
                if (restored <= 0) return false;
                _target = new FieldRepairTarget(_target.ItemInstanceId, _target.CurrentDurability + restored,
                    _target.MaxDurability, _target.SlotIndex, _target.IsBroken, _target.IsArtifact);
                return true;
            }
        }
    }
}
