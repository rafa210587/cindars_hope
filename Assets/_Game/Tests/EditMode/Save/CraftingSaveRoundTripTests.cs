using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Save
{
    public sealed class CraftingSaveRoundTripTests
    {
        private GameObject _host;
        private RecipeDatabaseSO _recipes;
        private RecipeDataSO _recipe;
        private CraftingRuntime _runtime;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("craft-save-roundtrip");
            _host.SetActive(false);
            var inventory = _host.AddComponent<InventoryManager>();
            inventory.Initialize(AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(
                "Assets/_Game/Data/Registries/ItemDatabase.asset"));
            _recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
            _recipe.SetId("recipe_test_saved_job");
            _recipe.OutputItemId = "item_material_wood";
            _recipe.OutputAmount = 2;
            _recipe.CraftTimeSeconds = 10;
            _recipe.Ingredients = new RecipeIngredient[0];
            _recipes = ScriptableObject.CreateInstance<RecipeDatabaseSO>();
            typeof(DataRegistrySO<RecipeDataSO>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_recipes, new[] { _recipe });
            typeof(DataRegistrySO<RecipeDataSO>).GetMethod("RebuildIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_recipes, null);
            _runtime = _host.AddComponent<CraftingRuntime>();
            typeof(CraftingRuntime).GetField("_recipeDatabase", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_runtime, _recipes);
            _runtime.RebindInventoryManager(inventory);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_recipes);
            Object.DestroyImmediate(_recipe);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoadIdleSnapshot_OverActiveJob_DiscardsFutureWork(bool omitStation)
        {
            var station = _runtime.GetOrCreateStation("workbench", WorkshopType.Workbench);
            var idle = JsonUtility.FromJson<CraftingRuntimeSaveData>(JsonUtility.ToJson(_runtime.CaptureSaveData()));
            var job = new CraftingJob("workbench", _recipe);
            station.LoadFromSaveData(new CraftingStationSaveData { StationLevel = 2, Job = job.CaptureSaveData() }, _recipes);
            Assert.That(station.HasActiveJob, Is.True);
            if (omitStation) idle.Stations.Clear();
            _runtime.LoadFromSaveData(idle);
            Assert.That(_runtime.GetOrCreateStation("workbench", WorkshopType.Workbench), Is.SameAs(station));
            Assert.That(station.Job, Is.Null);
            Assert.That(station.StationLevel, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoadActiveOrCompletedSnapshot_PreservesOutputAndTiming(bool completed)
        {
            var station = _runtime.GetOrCreateStation("workbench", WorkshopType.Workbench);
            var job = new CraftingJob("workbench", _recipe);
            job.Update(completed ? 10 : 3);
            if (completed) job.Complete();
            station.LoadFromSaveData(new CraftingStationSaveData { StationLevel = 2, Job = job.CaptureSaveData() }, _recipes);
            var roundtrip = JsonUtility.FromJson<CraftingRuntimeSaveData>(JsonUtility.ToJson(_runtime.CaptureSaveData()));
            _runtime.LoadFromSaveData(roundtrip);
            Assert.That(station.Job.JobId, Is.EqualTo(job.JobId));
            Assert.That(station.Job.OutputAmount, Is.EqualTo(2));
            Assert.That(station.Job.RemainingSeconds, Is.EqualTo(completed ? 0 : 7));
            Assert.That(station.HasCompletedOutput, Is.EqualTo(completed));
        }

        [Test]
        public void EmptyPlaceholder_WithPayload_IsNotDiscardable()
        {
            Assert.That(new CraftingJobSaveData().IsEmptySerializedJob, Is.True);
            Assert.That(new CraftingJobSaveData { LivingForgeReservationToken = "reservation" }.IsEmptySerializedJob, Is.False);
            Assert.That(new CraftingJobSaveData { IngredientsConsumed = new List<CraftingIngredientSaveData>
                { new CraftingIngredientSaveData { ItemId = "wood", Amount = 1 } } }.IsEmptySerializedJob, Is.False);
            Assert.That(new CraftingJobSaveData { OutputItemId = " " }.IsEmptySerializedJob, Is.False);
        }

        [Test]
        public void MissingLegacySection_ClearsJobFromDiscardedSession()
        {
            var station = _runtime.GetOrCreateStation("workbench", WorkshopType.Workbench);
            station.LoadFromSaveData(new CraftingStationSaveData
            { StationLevel = 2, Job = new CraftingJob("workbench", _recipe).CaptureSaveData() }, _recipes);
            Assert.That(station.HasActiveJob, Is.True);
            new CraftingSectionProvider(_runtime).Restore(null, 5);
            Assert.That(station.Job, Is.Null);
            Assert.That(station.StationLevel, Is.EqualTo(1));
        }

        [Test]
        public void ObservedPlaytestSave_ValidatesWithRealCatalog_WithoutRewritingFixture()
        {
            var path = Path.GetFullPath("TestResults/exploratory-playtest/crafting-unloadable-slot.json");
            if (!File.Exists(path)) Assert.Ignore("Local exploratory reproduction fixture is not distributed with the project.");
            var original = File.ReadAllText(path);
            var manager = _host.AddComponent<SaveManager>();
            typeof(SaveManager).GetField("_inventoryManager", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(manager, _host.GetComponent<InventoryManager>());
            var args = new object[] { path, false, null, null };
            var method = typeof(SaveManager).GetMethod("TryReadSaveWithMigration", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method.Invoke(manager, args), Is.True, args[3]?.ToString());
            Assert.That(((GameSaveData)args[2]).Crafting.Stations[0].Job, Is.Null);
            Assert.That(File.ReadAllText(path), Is.EqualTo(original));
        }
    }
}
