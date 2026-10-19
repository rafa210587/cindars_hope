using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.UI.Crafting;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class LivingForgeCapstoneTests
    {
        [Test]
        public void Resolver_RankChoicesGrantExactlyTheirAuthoredBenefit()
        {
            var food = Item("item_food", 20, ItemCategory.Food, duration: 10f);
            var gear = Item("item_gear", 1, ItemCategory.Weapon, equippable: true);
            try
            {
                Assert.That(LivingForgeCapstoneResolver.TryPrepare(1,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    food, 1, 0, out var rankOne, out _), Is.True);
                Assert.That(rankOne.OutputItemId,
                    Is.EqualTo(LivingForgeOutputVariantCatalog.Quality1Id(food.Id)));

                Assert.That(LivingForgeCapstoneResolver.TryPrepare(2,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    food, 1, 0, out var rankTwoQuality, out _), Is.True);
                Assert.That(rankTwoQuality.OutputItemId,
                    Is.EqualTo(LivingForgeOutputVariantCatalog.Quality2Id(food.Id)));

                Assert.That(LivingForgeCapstoneResolver.TryPrepare(2,
                    new LivingForgeCraftSelection(
                        LivingForgeBenefitChoice.SaveCommonMaterial, "item_common"),
                    food, 1, 0, out var rankTwo, out _), Is.True);
                Assert.That(rankTwo.OutputItemId, Is.EqualTo(food.Id));
                Assert.That(rankTwo.SavedCommonIngredientItemId, Is.EqualTo("item_common"));

                Assert.That(LivingForgeCapstoneResolver.TryPrepare(3,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    gear, 1, 100, out var rankThreeGear, out _), Is.True);
                Assert.That(rankThreeGear.OutputItemId,
                    Is.EqualTo(LivingForgeOutputVariantCatalog.Quality2Id(gear.Id)));
                Assert.That(rankThreeGear.DurabilityMultiplier, Is.EqualTo(1.08f));

                Assert.That(LivingForgeCapstoneResolver.TryPrepare(3,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    food, 5, 0, out var rankThreeFood, out _), Is.True);
                Assert.That(rankThreeFood.OutputItemId,
                    Is.EqualTo(LivingForgeOutputVariantCatalog.Quality2PotencyId(food.Id)));

                Assert.That(LivingForgeCapstoneResolver.TryPrepare(3,
                    new LivingForgeCraftSelection(
                        LivingForgeBenefitChoice.SaveCommonMaterial, "item_common"),
                    food, 5, 0, out var rankThreeSave, out _), Is.True);
                Assert.That(rankThreeSave.OutputItemId,
                    Is.EqualTo(LivingForgeOutputVariantCatalog.PotencyId(food.Id)));
                Assert.That(rankThreeSave.SavedCommonIngredientItemId,
                    Is.EqualTo("item_common"));

                var ineligible = Item("item_plain", 20, ItemCategory.Material);
                try
                {
                    Assert.That(LivingForgeCapstoneResolver.TryPrepare(3,
                        new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                        ineligible, 1, 0, out _, out _), Is.False);
                    Assert.That(LivingForgeCapstoneResolver.TryPrepare(3,
                        new LivingForgeCraftSelection(
                            LivingForgeBenefitChoice.SaveCommonMaterial, "item_common"),
                        ineligible, 1, 0, out var savedWithoutExtra, out _), Is.True);
                    Assert.That(savedWithoutExtra.OutputItemId, Is.EqualTo(ineligible.Id));
                }
                finally
                {
                    Object.DestroyImmediate(ineligible);
                }
            }
            finally
            {
                Object.DestroyImmediate(food);
                Object.DestroyImmediate(gear);
            }
        }

        [Test]
        public void RankTwoMaterialSaving_ConsumesExactlyOneLessAndCommitsAfterOutput()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Material,
                outputAmount: 1, commonAmount: 3);
            var state = new CraftingSkillState();

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.SaveCommonMaterial, Fixture.CommonId),
                craftingSkillState: state, livingForgeRank: 2, dayIndex: 7),
                Is.True, reason);

            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.EqualTo(1));
            Assert.That(fixture.Inventory.GetAmount(fixture.Recipe.OutputItemId), Is.EqualTo(1));
            Assert.That(state.IsChargeConsumed(7), Is.True);
            Assert.That(state.HasPendingReservation(7), Is.False);

            var another = new CraftingStation("other_station", WorkshopType.Workbench);
            Assert.That(another.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.SaveCommonMaterial, Fixture.CommonId),
                craftingSkillState: state, livingForgeRank: 2, dayIndex: 7), Is.False);
            Assert.That(reason, Does.Contain("unavailable"));
        }

        [Test]
        public void MaterialSavingThatWouldConsumeZero_ReleasesDailyReservation()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Material,
                outputAmount: 1, commonAmount: 1);
            var state = new CraftingSkillState();

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out _, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.SaveCommonMaterial, Fixture.CommonId),
                craftingSkillState: state, livingForgeRank: 2, dayIndex: 4), Is.False);

            Assert.That(state.IsChargeAvailable(4), Is.True);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.EqualTo(1));
        }

        [Test]
        public void RankTwoQuality_SaveLoadInventoryFullAndRetry_CommitsQ2Once()
        {
            using var fixture = new Fixture(instantaneous: false, outputKind: OutputKind.Food,
                outputAmount: 1, commonAmount: 2);
            var state = new CraftingSkillState();
            var selection = new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality);

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: selection, craftingSkillState: state,
                livingForgeRank: 2, dayIndex: 9), Is.True, reason);

            var stationSave = fixture.Station.CaptureSaveData();
            var stateSave = state.CaptureSaveData();
            var restoredState = new CraftingSkillState();
            restoredState.RestoreFromSaveData(stateSave);
            var restoredStation = new CraftingStation("workbench", WorkshopType.Workbench);
            restoredStation.LoadFromSaveData(stationSave, fixture.RecipeDatabase);
            restoredStation.Update(20f);

            fixture.FillInventory();
            Assert.That(restoredStation.TryCollectOutput(
                fixture.Inventory, out reason, craftingSkillState: restoredState), Is.False);
            Assert.That(restoredStation.HasCompletedOutput, Is.True);
            Assert.That(restoredState.HasPendingReservation(9), Is.True);
            Assert.That(restoredState.IsChargeConsumed(9), Is.False);

            Assert.That(fixture.Inventory.RemoveItem(Fixture.FillerId, 1), Is.True);
            Assert.That(restoredStation.TryCollectOutput(
                fixture.Inventory, out reason, craftingSkillState: restoredState), Is.True, reason);
            Assert.That(restoredState.IsChargeConsumed(9), Is.True);
            Assert.That(fixture.Inventory.GetAmount(
                LivingForgeOutputVariantCatalog.Quality2Id(fixture.Recipe.OutputItemId)),
                Is.EqualTo(1));
            Assert.That(restoredStation.TryCollectOutput(
                fixture.Inventory, out _, craftingSkillState: restoredState), Is.False);
        }

        [Test]
        public void TimedCancel_RestoresIngredientsAndReleasesSharedCharge()
        {
            using var fixture = new Fixture(instantaneous: false, outputKind: OutputKind.Food,
                outputAmount: 1, commonAmount: 2);
            var state = new CraftingSkillState();

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.Quality),
                craftingSkillState: state, livingForgeRank: 1, dayIndex: 3),
                Is.True, reason);
            Assert.That(state.HasPendingReservation(3), Is.True);

            Assert.That(fixture.Station.TryCancelJob(
                fixture.Inventory, out reason, state), Is.True, reason);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.EqualTo(2));
            Assert.That(state.IsChargeAvailable(3), Is.True);
        }

        [Test]
        public void QualityGear_AppliesVariantDurabilityBeforeRankThreeExtra()
        {
            using (var rankOne = new Fixture(instantaneous: true,
                       outputKind: OutputKind.Gear, outputAmount: 1, commonAmount: 2))
            {
                int rankOneInitializedMax = 0;
                Assert.That(rankOne.Station.TryStartCraft(rankOne.Recipe, rankOne.Inventory,
                    out var rankOneReason, isCommonIngredient: rankOne.IsCommon,
                    reserveOutputInstanceId: id => id + "#q1",
                    resolveBaseDurability: _ => 100,
                    initializeDurability: (_, maximum) => rankOneInitializedMax = maximum,
                    livingForgeSelection: new LivingForgeCraftSelection(
                        LivingForgeBenefitChoice.Quality),
                    craftingSkillState: new CraftingSkillState(), livingForgeRank: 1,
                    dayIndex: 4), Is.True, rankOneReason);
                Assert.That(rankOneInitializedMax, Is.EqualTo(105));
            }

            using (var rankTwo = new Fixture(instantaneous: true,
                       outputKind: OutputKind.Gear, outputAmount: 1, commonAmount: 2))
            {
                int rankTwoInitializedMax = 0;
                Assert.That(rankTwo.Station.TryStartCraft(rankTwo.Recipe, rankTwo.Inventory,
                    out var rankTwoReason, isCommonIngredient: rankTwo.IsCommon,
                    reserveOutputInstanceId: id => id + "#q2",
                    resolveBaseDurability: _ => 100,
                    initializeDurability: (_, maximum) => rankTwoInitializedMax = maximum,
                    livingForgeSelection: new LivingForgeCraftSelection(
                        LivingForgeBenefitChoice.Quality),
                    craftingSkillState: new CraftingSkillState(), livingForgeRank: 2,
                    dayIndex: 4), Is.True, rankTwoReason);
                Assert.That(rankTwoInitializedMax, Is.EqualTo(110));
            }

            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Gear,
                outputAmount: 1, commonAmount: 2);
            var state = new CraftingSkillState();
            string initializedId = null;
            int initializedMax = 0;

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                reserveOutputInstanceId: id => id + "#crafted-1",
                resolveBaseDurability: _ => 100,
                initializeDurability: (id, maximum) =>
                {
                    initializedId = id;
                    initializedMax = maximum;
                },
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.Quality),
                craftingSkillState: state, livingForgeRank: 3, dayIndex: 5),
                Is.True, reason);

            string expectedOutputId = LivingForgeOutputVariantCatalog.Quality2Id(
                fixture.Recipe.OutputItemId);
            Assert.That(initializedId, Is.EqualTo(expectedOutputId + "#crafted-1"));
            Assert.That(initializedMax, Is.EqualTo(119));
            Assert.That(state.IsChargeConsumed(5), Is.True);
            Assert.That(fixture.Inventory.GetAllItems().Single(item =>
                item.ItemInstanceId == initializedId).ItemId,
                Is.EqualTo(expectedOutputId));
        }

        [Test]
        public void RankThreeSaveGear_AppliesOnlyAutomaticEightPercentDurability()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Gear,
                outputAmount: 1, commonAmount: 2);
            int initializedMax = 0;

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                reserveOutputInstanceId: id => id + "#saved-common",
                resolveBaseDurability: _ => 100,
                initializeDurability: (_, maximum) => initializedMax = maximum,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.SaveCommonMaterial, Fixture.CommonId),
                craftingSkillState: new CraftingSkillState(), livingForgeRank: 3,
                dayIndex: 6), Is.True, reason);

            Assert.That(initializedMax, Is.EqualTo(108));
        }

        [Test]
        public void RankThreeQualityConsumable_ProducesCombinedQ2PotencyVariant()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Food,
                outputAmount: 5, commonAmount: 2);
            var state = new CraftingSkillState();

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.Quality), craftingSkillState: state,
                livingForgeRank: 3, dayIndex: 15), Is.True, reason);
            Assert.That(fixture.Inventory.GetAmount(
                LivingForgeOutputVariantCatalog.Quality2PotencyId(
                    fixture.Recipe.OutputItemId)), Is.EqualTo(5));
        }

        [Test]
        public void RankThreeSaveConsumable_SavesOneAndAddsPotencyOnly()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Food,
                outputAmount: 5, commonAmount: 3);
            var state = new CraftingSkillState();

            Assert.That(fixture.Station.TryStartCraft(fixture.Recipe, fixture.Inventory,
                out var reason, isCommonIngredient: fixture.IsCommon,
                livingForgeSelection: new LivingForgeCraftSelection(
                    LivingForgeBenefitChoice.SaveCommonMaterial, Fixture.CommonId),
                craftingSkillState: state, livingForgeRank: 3, dayIndex: 16),
                Is.True, reason);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.EqualTo(1));
            Assert.That(fixture.Inventory.GetAmount(
                LivingForgeOutputVariantCatalog.PotencyId(fixture.Recipe.OutputItemId)),
                Is.EqualTo(5));
        }

        [Test]
        public void CraftingModalHelper_AllowsExplicitOptOutAndResolvesSelectedCommonMaterial()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Food,
                outputAmount: 1, commonAmount: 3);

            Assert.That(CraftingModal.TryBuildLivingForgeSelection(2, true,
                LivingForgeBenefitChoice.None, 0, fixture.Recipe, fixture.IsCommon,
                out var optOut, out var reason), Is.True, reason);
            Assert.That(optOut.IsSelected, Is.False);

            Assert.That(CraftingModal.TryBuildLivingForgeSelection(2, true,
                LivingForgeBenefitChoice.SaveCommonMaterial, 0, fixture.Recipe,
                fixture.IsCommon, out var selection, out reason), Is.True, reason);
            Assert.That(selection.Choice,
                Is.EqualTo(LivingForgeBenefitChoice.SaveCommonMaterial));
            Assert.That(selection.CommonIngredientItemId, Is.EqualTo(Fixture.CommonId));
        }

        [Test]
        public void CraftingRuntime_ExplicitSelectionOverloadRoutesQ2ToCanonicalStation()
        {
            using var fixture = new Fixture(instantaneous: true, outputKind: OutputKind.Food,
                outputAmount: 1, commonAmount: 2);
            var skillHost = new GameObject("living-forge-skill-tree");
            var skillManager = skillHost.AddComponent<SkillTreeManager>();
            var state = new SkillTreeState(3);
            state.Purchase(LivingForgeCapstoneResolver.NodeId, 1, "crafting");
            state.RankUp(LivingForgeCapstoneResolver.NodeId, "crafting");
            SetField(skillManager, "_state", state);
            SetSkillTreeInstance(skillManager);
            try
            {
                var runtime = fixture.CreateRuntime();
                Assert.That(runtime.TryStartCraft(fixture.Station, fixture.Recipe,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    out var reason), Is.True, reason);
                Assert.That(fixture.Inventory.GetAmount(
                    LivingForgeOutputVariantCatalog.Quality2Id(
                        fixture.Recipe.OutputItemId)), Is.EqualTo(1));
            }
            finally
            {
                SetSkillTreeInstance(null);
                Object.DestroyImmediate(skillHost);
            }
        }

        [Test]
        public void QualityConsumable_RejectsBatchAboveFiveAndPayloadThatDoesNotRoundUp()
        {
            var bread = Item("item_bread", 20, ItemCategory.Food);
            var nominal = Item("item_nominal", 20, ItemCategory.Food);
            nominal.HungerRestore = 1;
            try
            {
                Assert.That(LivingForgeCapstoneResolver.TryPrepare(1,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    bread, 6, 0, out _, out _), Is.False);
                Assert.That(LivingForgeCapstoneResolver.TryPrepare(2,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    bread, 6, 0, out _, out _), Is.False);
                Assert.That(LivingForgeCapstoneResolver.TryPrepare(1,
                    new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                    nominal, 1, 0, out _, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(bread);
                Object.DestroyImmediate(nominal);
            }
        }

        [Test]
        public void StateProvider_RoundTripKeepsConsumptionAndDayAdvanceRearms()
        {
            var source = new CraftingSkillState();
            Assert.That(source.TryReserve(11, "committed"), Is.True);
            Assert.That(source.Commit(11, "committed"), Is.True);
            Assert.That(source.TryReserve(12, "pending"), Is.True);

            var provider = new CraftingSkillSectionProvider(source);
            var save = (CraftingSkillSaveData)provider.Capture(new GameSaveData());
            var restored = new CraftingSkillState();
            new CraftingSkillSectionProvider(restored).Restore(save);

            Assert.That(restored.IsChargeConsumed(11), Is.True);
            Assert.That(restored.HasPendingReservation(12), Is.True);
            restored.ObserveDay(13);
            Assert.That(restored.IsChargeAvailable(13), Is.True);
            Assert.That(restored.IsChargeAvailable(11), Is.False);

            restored.RestoreFromSaveData(new CraftingSkillSaveData
            {
                CurrentDayIndex = 11
            });
            Assert.That(restored.IsChargeConsumed(11), Is.True,
                "Loading an older section must not rearm a consumed daily charge.");

            // Respec mutates only the tree ledger; the independent daily ledger stays consumed.
            var skillTree = new SkillTreeState(6);
            skillTree.Purchase(LivingForgeCapstoneResolver.NodeId, 1, "crafting");
            skillTree.RankUp(LivingForgeCapstoneResolver.NodeId, "crafting");
            skillTree.RankUp(LivingForgeCapstoneResolver.NodeId, "crafting");
            Assert.That(skillTree.GetRank(LivingForgeCapstoneResolver.NodeId), Is.EqualTo(3));
            skillTree.RemoveAndRefundNode(LivingForgeCapstoneResolver.NodeId);
            Assert.That(skillTree.GetRank(LivingForgeCapstoneResolver.NodeId), Is.Zero);
            Assert.That(restored.IsChargeConsumed(11), Is.True);
        }

        private static ItemDataSO Item(
            string id,
            int maxStack,
            ItemCategory category,
            bool common = false,
            bool equippable = false,
            float duration = 0f)
        {
            var item = ScriptableObject.CreateInstance<ItemDataSO>();
            item.Id = id;
            item.MaxStack = maxStack;
            item.Category = category;
            item.IsCommonMaterialBonusEligible = common;
            item.IsEquippable = equippable;
            item.BuffDurationSeconds = duration;
            item.HungerRestore = category == ItemCategory.Food ? 30 : 0;
            return item;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            typeof(SkillTreeManager).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
        }

        private static void SetSkillTreeInstance(SkillTreeManager value)
        {
            typeof(SkillTreeManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, value);
        }

        private enum OutputKind
        {
            Material,
            Gear,
            Food
        }

        private sealed class Fixture : System.IDisposable
        {
            public const string CommonId = "item_common";
            public const string FillerId = "item_filler";
            private readonly GameObject _host;
            private readonly List<Object> _assets = new List<Object>();

            public Fixture(bool instantaneous, OutputKind outputKind, int outputAmount,
                int commonAmount)
            {
                var common = Add(Item(CommonId, 99, ItemCategory.Material, common: true));
                var filler = Add(Item(FillerId, 1, ItemCategory.Material));
                var outputId = outputKind == OutputKind.Gear
                    ? "item_gear"
                    : outputKind == OutputKind.Food ? "item_food" : "item_output";
                var output = Add(Item(outputId,
                    outputKind == OutputKind.Gear ? 1 : 99,
                    outputKind == OutputKind.Gear ? ItemCategory.Weapon :
                    outputKind == OutputKind.Food ? ItemCategory.Food : ItemCategory.Material,
                    equippable: outputKind == OutputKind.Gear,
                    duration: outputKind == OutputKind.Food ? 20f : 0f));
                var q1 = Add(Item(LivingForgeOutputVariantCatalog.Quality1Id(outputId),
                    output.MaxStack, output.Category, equippable: output.IsEquippable));
                var q2 = Add(Item(LivingForgeOutputVariantCatalog.Quality2Id(outputId),
                    output.MaxStack, output.Category, equippable: output.IsEquippable));
                var potency = Add(Item(LivingForgeOutputVariantCatalog.PotencyId(outputId),
                    output.MaxStack, output.Category, duration: output.BuffDurationSeconds));
                potency.HungerRestore = LivingForgeOutputVariantCatalog.ScaleInteger(
                    output.HungerRestore, LivingForgeOutputVariantCatalog.PotencyMultiplier);
                var q2Potency = Add(Item(
                    LivingForgeOutputVariantCatalog.Quality2PotencyId(outputId),
                    output.MaxStack, output.Category, duration: output.BuffDurationSeconds));
                q2Potency.HungerRestore = LivingForgeOutputVariantCatalog.ScaleInteger(
                    output.HungerRestore,
                    LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier *
                    LivingForgeOutputVariantCatalog.PotencyMultiplier);

                var itemDatabase = Add(ScriptableObject.CreateInstance<ItemDatabaseSO>());
                SetItems(itemDatabase,
                    new[] { common, filler, output, q1, q2, potency, q2Potency });
                RecipeDatabase = Add(ScriptableObject.CreateInstance<RecipeDatabaseSO>());
                Recipe = Add(ScriptableObject.CreateInstance<RecipeDataSO>());
                Recipe.SetId("recipe_living_forge_fixture");
                Recipe.RequiredStationType = WorkshopType.Workbench;
                Recipe.IsUnlockedByDefault = true;
                Recipe.OutputItemId = output.Id;
                Recipe.OutputAmount = outputAmount;
                Recipe.CraftTimeSeconds = instantaneous ? 0f : 10f;
                Recipe.Ingredients = new[] { new RecipeIngredient(CommonId, commonAmount) };
                SetItems(RecipeDatabase, new[] { Recipe });

                _host = new GameObject("living-forge-fixture");
                _host.SetActive(false);
                Inventory = _host.AddComponent<InventoryManager>();
                Inventory.Initialize(itemDatabase);
                Assert.That(Inventory.AddItem(CommonId, commonAmount), Is.True);
                Station = new CraftingStation("workbench", WorkshopType.Workbench);
            }

            public InventoryManager Inventory { get; }
            public CraftingStation Station { get; }
            public RecipeDataSO Recipe { get; }
            public RecipeDatabaseSO RecipeDatabase { get; }

            public bool IsCommon(string itemId) => itemId == CommonId;

            public void FillInventory()
            {
                int sequence = 0;
                while (Inventory.TryAddItemInstance(
                           FillerId, FillerId + "#" + sequence++).Success)
                {
                }
            }

            public CraftingRuntime CreateRuntime()
            {
                var runtime = _host.AddComponent<CraftingRuntime>();
                typeof(CraftingRuntime).GetField("_inventoryManager",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(runtime, Inventory);
                typeof(CraftingRuntime).GetField("_recipeDatabase",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(runtime, RecipeDatabase);
                typeof(CraftingRuntime).GetField("_craftingSkillState",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                    runtime, new CraftingSkillState());
                runtime.Initialize();
                return runtime;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_host);
                foreach (var asset in _assets)
                    if (asset != null) Object.DestroyImmediate(asset);
            }

            private T Add<T>(T asset) where T : Object
            {
                _assets.Add(asset);
                return asset;
            }

            private static void SetItems<T>(DataRegistrySO<T> database, T[] items)
                where T : ScriptableObject, CindarsHope.Core.Data.IIdentifiedData
            {
                typeof(DataRegistrySO<T>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, items);
                typeof(DataRegistrySO<T>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, false);
            }
        }
    }
}
