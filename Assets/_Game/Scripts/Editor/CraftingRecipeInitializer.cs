#if UNITY_EDITOR
using CindarsHope.Craft.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class CraftingRecipeInitializer
    {
        private const string RecipePath = "Assets/_Game/Data/Craft/Recipes/";
        private const string InitKey = "CraftingRecipeInitialized";

        static CraftingRecipeInitializer()
        {
            if (!SessionState.GetBool(InitKey, false))
            {
                SessionState.SetBool(InitKey, true);
                GenerateDefaultRecipes();
            }
        }

        private static void GenerateDefaultRecipes()
        {
            var recipeDir = "Assets/_Game/Data/Craft/Recipes";
            if (!AssetDatabase.IsValidFolder(recipeDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Craft"))
                {
                    AssetDatabase.CreateFolder("Assets/_Game/Data", "Craft");
                }

                AssetDatabase.CreateFolder("Assets/_Game/Data/Craft", "Recipes");
            }

            CreateFoodRecipes();
            CreateToolRecipes();

            AssetDatabase.SaveAssets();
        }

        private static void CreateFoodRecipes()
        {
            var recipes = new[]
            {
                ("recipe_bread", "Bread", "item_consumable_food_bread", 1, 1, WorkshopType.Carpentry, new[] { ("item_crop_wheat", 2) }),
                ("recipe_carrot_stew", "Carrot Stew", "item_consumable_food_carrot_stew", 1, 2, WorkshopType.Carpentry, new[] { ("item_crop_carrot", 3) }),
                ("recipe_moonbean_soup", "Moonbean Soup", "item_consumable_food_moonbean_soup", 1, 3, WorkshopType.Alchemy, new[] { ("item_crop_moonbean", 2) }),
            };

            foreach (var (id, name, output, qty, level, workshop, ingredients) in recipes)
            {
                CreateRecipe(id, name, output, qty, level, workshop, ingredients);
            }
        }

        private static void CreateToolRecipes()
        {
            var recipes = new[]
            {
                ("recipe_processed_wood", "Processed Wood", "item_processed_wood", 2, 1, WorkshopType.Carpentry, new[] { ("item_wood", 3) }),
            };

            foreach (var (id, name, output, qty, level, workshop, ingredients) in recipes)
            {
                CreateRecipe(id, name, output, qty, level, workshop, ingredients);
            }
        }

        private static void CreateRecipe(
            string id,
            string name,
            string outputItemId,
            int outputQty,
            int requiredWorkshopLevel,
            WorkshopType workshopType,
            (string itemId, int quantity)[] ingredients)
        {
            var path = $"{RecipePath}{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<RecipeDataSO>(path);
            if (existing != null)
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<RecipeDataSO>();
            asset.SetId(id);
            asset.DisplayName = name;
            asset.Description = $"Recipe: {name}";
            asset.OutputItemId = outputItemId;
            asset.OutputAmount = outputQty;
            asset.RequiredWorkshopLevel = requiredWorkshopLevel;
            asset.WorkshopType = workshopType;

            asset.Ingredients = new RecipeIngredient[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                asset.Ingredients[i] = new RecipeIngredient(ingredients[i].itemId, ingredients[i].quantity);
            }

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif