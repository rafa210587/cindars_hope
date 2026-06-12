using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Economy
{
    /// <summary>
    /// Replaces CraftingRecipeTests (which targeted the retired parallel system in
    /// Scripts/Crafting/). Covers the equivalent deterministic rules on the live
    /// Scripts/Craft/ system: station gating (CanStartCraft), job timing,
    /// collect-idempotency at job level and save/load round-trip.
    /// </summary>
    [TestFixture]
    public class CraftingStationJobTests
    {
        private readonly List<RecipeDataSO> _createdRecipes = new List<RecipeDataSO>();

        [TearDown]
        public void TearDown()
        {
            foreach (var recipe in _createdRecipes)
            {
                if (recipe != null)
                {
                    Object.DestroyImmediate(recipe);
                }
            }

            _createdRecipes.Clear();
        }

        private RecipeDataSO MakeRecipe(
            string id = "recipe_wooden_plank",
            WorkshopType stationType = WorkshopType.Workbench,
            int requiredLevel = 1,
            bool unlocked = true,
            float craftTimeSeconds = 0f)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
            recipe.SetId(id);
            recipe.RequiredStationType = stationType;
            recipe.RequiredWorkshopLevel = requiredLevel;
            recipe.IsUnlockedByDefault = unlocked;
            recipe.OutputItemId = "item_wooden_plank";
            recipe.OutputAmount = 2;
            recipe.CraftTimeSeconds = craftTimeSeconds;
            recipe.Ingredients = new[] { new RecipeIngredient("item_log", 1) };
            _createdRecipes.Add(recipe);
            return recipe;
        }

        private static CraftingStation Workbench(int level = 1) =>
            new CraftingStation("station_workbench", WorkshopType.Workbench) { StationLevel = level };

        // ---- Station gating (CanStartCraft) ----

        [Test]
        public void CanStartCraft_HappyPath_Succeeds()
        {
            var ok = Workbench().CanStartCraft(MakeRecipe(), out var reason);
            Assert.IsTrue(ok, reason);
        }

        [Test]
        public void CanStartCraft_WrongStationType_Fails()
        {
            var forgeRecipe = MakeRecipe(id: "recipe_iron_bar", stationType: WorkshopType.Forge);
            var ok = Workbench().CanStartCraft(forgeRecipe, out var reason);
            Assert.IsFalse(ok);
            Assert.IsTrue(reason.Contains("Forge"));
        }

        [Test]
        public void CanStartCraft_StationLevelTooLow_Fails()
        {
            var advancedRecipe = MakeRecipe(requiredLevel: 3);
            var ok = Workbench(level: 1).CanStartCraft(advancedRecipe, out var reason);
            Assert.IsFalse(ok);
            Assert.IsTrue(reason.Contains("level"));
        }

        [Test]
        public void CanStartCraft_LockedRecipe_Fails()
        {
            var lockedRecipe = MakeRecipe(unlocked: false);
            var ok = Workbench().CanStartCraft(lockedRecipe, out var reason);
            Assert.IsFalse(ok);
            Assert.IsTrue(reason.Contains("locked"));
        }

        [Test]
        public void CanStartCraft_NullRecipe_Fails()
        {
            var ok = Workbench().CanStartCraft(null, out var reason);
            Assert.IsFalse(ok);
            Assert.IsNotEmpty(reason);
        }

        // ---- Job timing (processing) ----

        [Test]
        public void Job_BeforeFinish_IsNotComplete()
        {
            var job = new CraftingJob("station_workbench", MakeRecipe(craftTimeSeconds: 5f));
            job.Update(3f);
            Assert.IsFalse(job.IsComplete, "Job must not complete before its full craft time elapses.");
            Assert.AreEqual(CraftingJobStatus.InProgress, job.Status);
        }

        [Test]
        public void Job_AfterFinish_IsCompleteAndCompletes()
        {
            var job = new CraftingJob("station_workbench", MakeRecipe(craftTimeSeconds: 5f));
            job.Update(3f);
            job.Update(2f);
            Assert.IsTrue(job.IsComplete);

            job.Complete();
            Assert.AreEqual(CraftingJobStatus.Completed, job.Status);
            Assert.AreEqual(0f, job.RemainingSeconds);
        }

        [Test]
        public void Job_Complete_IsIdempotent_NoSecondCompletion()
        {
            var job = new CraftingJob("station_workbench", MakeRecipe(craftTimeSeconds: 1f));
            job.Update(1f);
            job.Complete();
            // Once completed, IsComplete must be false (status left InProgress),
            // so a station Update loop can never complete/collect it twice.
            Assert.IsFalse(job.IsComplete);
            job.Update(10f);
            Assert.AreEqual(CraftingJobStatus.Completed, job.Status);
        }

        [Test]
        public void Job_Cancel_SetsCancelledStatus()
        {
            var job = new CraftingJob("station_workbench", MakeRecipe(craftTimeSeconds: 5f));
            job.Cancel();
            Assert.AreEqual(CraftingJobStatus.Cancelled, job.Status);
        }

        // ---- Save/load round-trip ----

        [Test]
        public void Job_SaveData_RoundTrip_PreservesState()
        {
            var recipe = MakeRecipe(craftTimeSeconds: 5f);
            var job = new CraftingJob("station_workbench", recipe);
            job.Update(2f);

            var saveData = job.CaptureSaveData();
            var restored = new CraftingJob(saveData, recipe);

            Assert.AreEqual(job.JobId, restored.JobId);
            Assert.AreEqual(job.StationInstanceId, restored.StationInstanceId);
            Assert.AreEqual(job.RecipeId, restored.RecipeId);
            Assert.AreEqual(job.OutputItemId, restored.OutputItemId);
            Assert.AreEqual(job.OutputAmount, restored.OutputAmount);
            Assert.AreEqual(job.Status, restored.Status);
            Assert.AreEqual(job.RemainingSeconds, restored.RemainingSeconds);
            Assert.AreEqual(job.IngredientsConsumed.Count, restored.IngredientsConsumed.Count);
            Assert.AreEqual("item_log", restored.IngredientsConsumed[0].ItemId);
        }

        [Test]
        public void Job_SaveData_RestoreWithNullRecipe_KeepsSavedFields()
        {
            // Missing recipe id after load must not lose the saved output/ingredients
            // (CraftingStation.LoadFromSaveData relies on this fallback).
            var job = new CraftingJob("station_workbench", MakeRecipe(craftTimeSeconds: 5f));
            var saveData = job.CaptureSaveData();

            var restored = new CraftingJob(saveData, null);

            Assert.AreEqual(job.OutputItemId, restored.OutputItemId);
            Assert.AreEqual(job.OutputAmount, restored.OutputAmount);
            Assert.AreEqual(1, restored.IngredientsConsumed.Count);
        }

        [Test]
        public void Station_SaveData_CapturesIdentityLevelAndJob()
        {
            var station = Workbench(level: 2);
            var saveData = station.CaptureSaveData();

            Assert.AreEqual("station_workbench", saveData.StationInstanceId);
            Assert.AreEqual((int)WorkshopType.Workbench, saveData.StationType);
            Assert.AreEqual(2, saveData.StationLevel);
            Assert.IsNull(saveData.Job, "Fresh station must capture no job.");
        }

        [Test]
        public void Station_LoadFromSaveData_NullAndJobless_AreSafe()
        {
            var station = Workbench();

            station.LoadFromSaveData(null, null);
            Assert.AreEqual(1, station.StationLevel, "Null save data must be a no-op.");

            station.LoadFromSaveData(new CraftingStationSaveData { StationLevel = 3, Job = null }, null);
            Assert.AreEqual(3, station.StationLevel);
            Assert.IsFalse(station.IsBusy);
        }
    }
}
