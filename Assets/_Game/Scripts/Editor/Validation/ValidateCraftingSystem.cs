#if UNITY_EDITOR
using CindarsHope.Core.Data;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.UI.Modal;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateCraftingSystem
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";

        public static void ValidateSpec07()
        {
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            var recipeDatabase = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath);
            Require(itemDatabase != null && recipeDatabase != null, "Crafting registries must exist.");

            var pocket = GetRecipe(recipeDatabase, "recipe_pocket_processed_wood");
            var workbenchRecipe = GetRecipe(recipeDatabase, "recipe_workbench_processed_wood");
            var forgeRecipe = GetRecipe(recipeDatabase, "recipe_forge_iron_sword");
            var cookingRecipe = GetRecipe(recipeDatabase, "recipe_cooking_bread");
            Require(!recipeDatabase.TryGetById("recipe_processed_wood", out _), "Legacy recipe with unknown ingredient must not remain in registry.");
            Require(pocket.RequiredStationType == WorkshopType.None, "Pocket recipe must require no station.");
            Require(workbenchRecipe.RequiredStationType == WorkshopType.Workbench, "Workbench recipe station mismatch.");
            Require(forgeRecipe.RequiredStationType == WorkshopType.Forge && forgeRecipe.CraftTimeSeconds > 0f, "Forge recipe must be timed.");
            Require(cookingRecipe.RequiredStationType == WorkshopType.CookingStation && cookingRecipe.CraftTimeSeconds > 0f, "Cooking recipe must be timed.");

            var gameObject = new GameObject("SPEC07_CraftingValidation");
            try
            {
                var inventory = gameObject.AddComponent<InventoryManager>();
                inventory.Initialize(itemDatabase);

                Require(inventory.AddItem("item_material_wood", 2), "Could not seed wood for instant craft.");
                var workbench = new CraftingStation("farm_workbench_01", WorkshopType.Workbench);
                Require(workbench.TryStartCraft(workbenchRecipe, inventory, out var failure), failure);
                Require(inventory.GetAmount(workbenchRecipe.OutputItemId) == workbenchRecipe.OutputAmount, "Instant output was not delivered directly.");

                inventory.Clear();
                Require(inventory.AddItem("item_material_iron_ore", 4), "Could not seed iron for timed tests.");
                var forge = new CraftingStation("farm_forge_01", WorkshopType.Forge);
                Require(forge.TryStartCraft(forgeRecipe, inventory, out failure), failure);
                Require(forge.HasActiveJob, "Timed job was not started.");
                Require(forge.TryCancelJob(inventory, out failure), failure);
                Require(inventory.GetAmount("item_material_iron_ore") == 4, "Cancel did not restore all ingredients.");

                Require(forge.TryStartCraft(forgeRecipe, inventory, out failure), failure);
                forge.Update(10f);
                Require(forge.HasCompletedOutput, "Timed job did not reach completed state.");
                var saved = forge.CaptureSaveData();
                var loadedForge = new CraftingStation("farm_forge_01", WorkshopType.Forge);
                loadedForge.LoadFromSaveData(saved, recipeDatabase);
                Require(loadedForge.HasCompletedOutput, "Completed job was not restored from save.");
                Require(loadedForge.TryCollectOutput(inventory, out failure), failure);

                inventory.Clear();
                Require(inventory.AddItem("item_material_iron_ore", 2), "Could not seed iron for full inventory test.");
                var blockedForge = new CraftingStation("farm_forge_blocked", WorkshopType.Forge);
                Require(blockedForge.TryStartCraft(forgeRecipe, inventory, out failure), failure);
                Require(inventory.AddItem("item_material_processed_wood", InventoryManager.DefaultCapacity * 99), "Could not fill inventory for collection test.");
                blockedForge.Update(10f);
                Require(!blockedForge.TryCollectOutput(inventory, out _), "Collection should fail with a full inventory.");
                Require(blockedForge.HasCompletedOutput, "Output must remain at station after failed collection.");

                var modalManager = gameObject.AddComponent<ModalManager>();
                Require(modalManager.PushModal(ModalType.Crafting), "Crafting modal should open on an empty modal stack.");
                Require(!modalManager.PushModal(ModalType.Inventory), "Inventory modal must not overlap crafting.");
                Require(modalManager.TryPopModal(ModalType.Crafting, out _), "Crafting modal could not close.");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }

            Debug.Log("SPEC 07 crafting validation passed: assets, instant craft, timed job, cancel, collect, full inventory retention and save/load.");
        }

        private static RecipeDataSO GetRecipe(RecipeDatabaseSO database, string id)
        {
            Require(database.TryGetById(id, out var recipe) && recipe != null, $"Recipe '{id}' is missing from registry.");
            return recipe;
        }

        private static void Require(bool condition, string failureMessage)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException(failureMessage);
            }
        }
    }
}
#endif
