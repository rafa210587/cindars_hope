using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.UI.Crafting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class LivingForgeCapstonePlayModeTests
    {
        private GameObject _skillHost;
        private SkillTreeManager _previousSkillTree;
        private SkillTreeManager _skillTree;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousSkillTree = SkillTreeManager.Instance;
            _skillHost = new GameObject("Living Forge PlayMode skill fixture");
            _skillHost.SetActive(false);
            _skillTree = _skillHost.AddComponent<SkillTreeManager>();
            SetSkillTreeInstance(_skillTree);
            SetRank(2);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SetSkillTreeInstance(_previousSkillTree);
            if (_skillHost != null)
                Object.Destroy(_skillHost);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CraftingModalOptOut_RoutesBaseCraftWithoutConsumingDailyCharge()
        {
            using var fixture = new Fixture(instantaneous: true, OutputKind.Food, 1, 2);

            Assert.That(CraftingModal.TryBuildLivingForgeSelection(
                2, true, LivingForgeBenefitChoice.None, 0, fixture.Recipe,
                fixture.Runtime.IsLivingForgeCommonIngredient,
                out var selection, out var reason), Is.True, reason);
            Assert.That(selection.IsSelected, Is.False);

            Assert.That(fixture.Runtime.TryStartCraft(
                fixture.Station, fixture.Recipe, selection, out reason), Is.True, reason);

            Assert.That(fixture.Inventory.GetAmount(fixture.BaseOutputId), Is.EqualTo(1));
            Assert.That(fixture.State.IsChargeAvailable(1), Is.True);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RankTwoQuality_CommitsQ2ExactlyOnce_AndDayRolloverRearms()
        {
            using var fixture = new Fixture(instantaneous: true, OutputKind.Food, 1, 2);
            var quality = new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality);

            Assert.That(fixture.Runtime.TryStartCraft(
                fixture.Station, fixture.Recipe, quality, out var reason), Is.True, reason);
            Assert.That(fixture.Inventory.GetAmount(fixture.Q2OutputId), Is.EqualTo(1));
            Assert.That(fixture.State.IsChargeConsumed(1), Is.True);
            Assert.That(fixture.State.HasPendingReservation(1), Is.False);

            Assert.That(fixture.Inventory.AddItem(Fixture.CommonId, 2), Is.True);
            var sameDayStation = fixture.Runtime.GetOrCreateStation(
                "same-day-workbench", WorkshopType.Workbench);
            Assert.That(fixture.Runtime.TryStartCraft(
                sameDayStation, fixture.Recipe, quality, out reason), Is.False);
            Assert.That(reason, Does.Contain("unavailable"));
            Assert.That(fixture.Inventory.GetAmount(fixture.Q2OutputId), Is.EqualTo(1));

            GameEventBus.Publish(new DayStartedEvent(2));
            var nextDayStation = fixture.Runtime.GetOrCreateStation(
                "next-day-workbench", WorkshopType.Workbench);
            Assert.That(fixture.Runtime.TryStartCraft(
                nextDayStation, fixture.Recipe, quality, out reason), Is.True, reason);
            Assert.That(fixture.State.IsChargeConsumed(2), Is.True);
            Assert.That(fixture.Inventory.GetAmount(fixture.Q2OutputId), Is.EqualTo(2));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TimedQuality_SaveLoadInventoryFullPreservesReservation_AndRetryCommits()
        {
            using var fixture = new Fixture(instantaneous: false, OutputKind.Food, 1, 2);
            var quality = new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality);

            Assert.That(fixture.Runtime.TryStartCraft(
                fixture.Station, fixture.Recipe, quality, out var reason), Is.True, reason);
            var runtimeSave = fixture.Runtime.CaptureSaveData();
            var stateSave = fixture.State.CaptureSaveData();
            Assert.That(runtimeSave.Stations[0].Job.LivingForgeReservationToken, Is.Not.Empty);

            fixture.DeactivateRuntime();
            var restoredState = new CraftingSkillState();
            new CraftingSkillSectionProvider(restoredState).Restore(stateSave);
            var restoredRuntime = fixture.CreateAdditionalRuntime(restoredState);
            restoredRuntime.LoadFromSaveData(runtimeSave);
            var restoredStation = restoredRuntime.GetOrCreateStation(
                Fixture.StationId, WorkshopType.Workbench);

            restoredStation.Update(.1f);
            yield return null;
            Assert.That(restoredStation.HasCompletedOutput, Is.True);
            fixture.FillInventory();

            Assert.That(restoredRuntime.TryCollect(restoredStation, out reason), Is.False);
            Assert.That(reason, Does.Contain("Inventory is full"));
            Assert.That(restoredStation.HasCompletedOutput, Is.True);
            Assert.That(restoredState.HasPendingReservation(1), Is.True);
            Assert.That(restoredState.IsChargeConsumed(1), Is.False);

            Assert.That(fixture.Inventory.RemoveItem(Fixture.FillerId, 1), Is.True);
            Assert.That(restoredRuntime.TryCollect(restoredStation, out reason), Is.True, reason);
            Assert.That(restoredState.IsChargeConsumed(1), Is.True);
            Assert.That(restoredState.HasPendingReservation(1), Is.False);
            Assert.That(fixture.Inventory.GetAmount(fixture.Q2OutputId), Is.EqualTo(1));
            Assert.That(restoredRuntime.TryCollect(restoredStation, out _), Is.False);
        }

        [UnityTest]
        public IEnumerator TimedQuality_CancelRestoresIngredientsAndReleasesReservation()
        {
            using var fixture = new Fixture(instantaneous: false, OutputKind.Food, 1, 2);

            Assert.That(fixture.Runtime.TryStartCraft(
                fixture.Station, fixture.Recipe,
                new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                out var reason), Is.True, reason);
            Assert.That(fixture.State.HasPendingReservation(1), Is.True);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.Zero);

            Assert.That(fixture.Runtime.TryCancel(fixture.Station, out reason), Is.True, reason);
            Assert.That(fixture.Inventory.GetAmount(Fixture.CommonId), Is.EqualTo(2));
            Assert.That(fixture.State.IsChargeAvailable(1), Is.True);
            Assert.That(fixture.Station.Job, Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RankThreeConsumable_ProducesCombinedQualityAndPotencyBatch()
        {
            SetRank(3);
            using var fixture = new Fixture(instantaneous: true, OutputKind.Food, 5, 2);

            Assert.That(fixture.Runtime.TryStartCraft(
                fixture.Station, fixture.Recipe,
                new LivingForgeCraftSelection(LivingForgeBenefitChoice.Quality),
                out var reason), Is.True, reason);

            Assert.That(fixture.Inventory.GetAmount(fixture.Q2PotencyOutputId), Is.EqualTo(5));
            Assert.That(fixture.GetHungerRestore(fixture.Q2PotencyOutputId), Is.EqualTo(44));
            Assert.That(fixture.Inventory.GetAmount(fixture.Q2OutputId), Is.Zero);
            Assert.That(fixture.State.IsChargeConsumed(1), Is.True);
            yield return null;
        }

        private void SetRank(int rank)
        {
            var state = new SkillTreeState(rank);
            state.Purchase(LivingForgeCapstoneResolver.NodeId, 1, "crafting");
            for (var currentRank = 1; currentRank < rank; currentRank++)
                state.RankUp(LivingForgeCapstoneResolver.NodeId, "crafting");
            typeof(SkillTreeManager).GetField("_state",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(_skillTree, state);
        }

        private static void SetSkillTreeInstance(SkillTreeManager value)
        {
            typeof(SkillTreeManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, value);
        }

        private enum OutputKind
        {
            Material,
            Food
        }

        private sealed class Fixture : System.IDisposable
        {
            public const string CommonId = "play_living_forge_common";
            public const string FillerId = "play_living_forge_filler";
            public const string StationId = "play_living_forge_workbench";

            private readonly List<Object> _owned = new List<Object>();
            private readonly GameObject _runtimeHost;
            private readonly RecipeDatabaseSO _recipeDatabase;

            public Fixture(bool instantaneous, OutputKind outputKind, int outputAmount,
                int commonAmount)
            {
                var common = Own(Item(CommonId, 99, ItemCategory.Material, common: true));
                var filler = Own(Item(FillerId, 1, ItemCategory.Material));
                BaseOutputId = outputKind == OutputKind.Food
                    ? "play_living_forge_food"
                    : "play_living_forge_output";
                var category = outputKind == OutputKind.Food
                    ? ItemCategory.Food
                    : ItemCategory.Material;
                var duration = outputKind == OutputKind.Food ? 20f : 0f;
                var output = Own(Item(BaseOutputId, 99, category, duration: duration));
                if (outputKind == OutputKind.Food)
                    output.HungerRestore = 30;
                var q1 = Own(Item(LivingForgeOutputVariantCatalog.Quality1Id(BaseOutputId),
                    99, category));
                q1.HungerRestore = outputKind == OutputKind.Food ? 35 : 0;
                var q2 = Own(Item(LivingForgeOutputVariantCatalog.Quality2Id(BaseOutputId),
                    99, category));
                q2.HungerRestore = outputKind == OutputKind.Food ? 41 : 0;
                var potencyOutput = Own(Item(
                    LivingForgeOutputVariantCatalog.PotencyId(BaseOutputId),
                    99, category, duration: duration));
                potencyOutput.HungerRestore = outputKind == OutputKind.Food ? 32 : 0;
                var q2Potency = Own(Item(
                    LivingForgeOutputVariantCatalog.Quality2PotencyId(BaseOutputId),
                    99, category, duration: duration));
                q2Potency.HungerRestore = outputKind == OutputKind.Food ? 44 : 0;

                var itemDatabase = Own(ScriptableObject.CreateInstance<ItemDatabaseSO>());
                SetItems(itemDatabase,
                    new[] { common, filler, output, q1, q2, potencyOutput, q2Potency });
                _recipeDatabase = Own(ScriptableObject.CreateInstance<RecipeDatabaseSO>());
                Recipe = Own(ScriptableObject.CreateInstance<RecipeDataSO>());
                Recipe.SetId("play_living_forge_recipe");
                Recipe.RequiredStationType = WorkshopType.Workbench;
                Recipe.IsUnlockedByDefault = true;
                Recipe.OutputItemId = BaseOutputId;
                Recipe.OutputAmount = outputAmount;
                Recipe.CraftTimeSeconds = instantaneous ? 0f : .05f;
                Recipe.Ingredients = new[] { new RecipeIngredient(CommonId, commonAmount) };
                SetItems(_recipeDatabase, new[] { Recipe });

                _runtimeHost = Own(new GameObject("Living Forge PlayMode runtime fixture"));
                _runtimeHost.SetActive(false);
                Inventory = _runtimeHost.AddComponent<InventoryManager>();
                Inventory.Initialize(itemDatabase);
                Assert.That(Inventory.AddItem(CommonId, commonAmount), Is.True);

                State = new CraftingSkillState();
                Runtime = AddRuntime(_runtimeHost, State);
                _runtimeHost.SetActive(true);
                Station = Runtime.GetOrCreateStation(StationId, WorkshopType.Workbench);
            }

            public InventoryManager Inventory { get; }
            public CraftingRuntime Runtime { get; }
            public CraftingStation Station { get; }
            public RecipeDataSO Recipe { get; }
            public CraftingSkillState State { get; }
            public string BaseOutputId { get; }
            public string Q2OutputId => LivingForgeOutputVariantCatalog.Quality2Id(BaseOutputId);
            public string Q2PotencyOutputId =>
                LivingForgeOutputVariantCatalog.Quality2PotencyId(BaseOutputId);

            public int GetHungerRestore(string itemId) =>
                Inventory.TryGetItemData(itemId, out var item) ? item.HungerRestore : -1;

            public void DeactivateRuntime() => _runtimeHost.SetActive(false);

            public CraftingRuntime CreateAdditionalRuntime(CraftingSkillState state)
            {
                var host = Own(new GameObject("Living Forge restored runtime fixture"));
                host.SetActive(false);
                var runtime = AddRuntime(host, state);
                host.SetActive(true);
                return runtime;
            }

            public void FillInventory()
            {
                var sequence = 0;
                while (Inventory.TryAddItemInstance(
                           FillerId, FillerId + "#" + sequence++).Success)
                {
                }
            }

            public void Dispose()
            {
                foreach (var owned in _owned)
                    if (owned != null) Object.DestroyImmediate(owned);
                _owned.Clear();
            }

            private CraftingRuntime AddRuntime(GameObject host, CraftingSkillState state)
            {
                var runtime = host.AddComponent<CraftingRuntime>();
                SetField(runtime, "_inventoryManager", Inventory);
                SetField(runtime, "_recipeDatabase", _recipeDatabase);
                SetField(runtime, "_craftingSkillState", state);
                runtime.Initialize();
                return runtime;
            }

            private T Own<T>(T value) where T : Object
            {
                _owned.Add(value);
                return value;
            }

            private static ItemDataSO Item(string id, int maxStack, ItemCategory category,
                bool common = false, float duration = 0f)
            {
                var item = ScriptableObject.CreateInstance<ItemDataSO>();
                item.Id = id;
                item.MaxStack = maxStack;
                item.Category = category;
                item.IsCommonMaterialBonusEligible = common;
                item.BuffDurationSeconds = duration;
                return item;
            }

            private static void SetItems<T>(DataRegistrySO<T> database, T[] items)
                where T : ScriptableObject, IIdentifiedData
            {
                typeof(DataRegistrySO<T>).GetField("_items",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, items);
                typeof(DataRegistrySO<T>).GetField("_indexBuilt",
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(database, false);
            }

            private static void SetField(object target, string fieldName, object value)
            {
                target.GetType().GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
            }
        }
    }
}
