#if UNITY_EDITOR
using CindarsHope.Crafting;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class CraftingRecipeInitializer
    {
        private const string RecipePath = "Assets/_Game/Data/Crafting/Recipes/";
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
            var recipeDir = "Assets/_Game/Data/Crafting/Recipes";
            if (!AssetDatabase.IsValidFolder(recipeDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Crafting"))
                    AssetDatabase.CreateFolder("Assets/_Game/Data", "Crafting");
                AssetDatabase.CreateFolder("Assets/_Game/Data/Crafting", "Recipes");
            }

            CreateFoodRecipes();
            CreateToolRecipes();

            AssetDatabase.SaveAssets();
        }

        private static void CreateFoodRecipes()
        {
            var recipes = new[]
            {
                ("recipe_bread", "Bread", "item_consumable_food_bread", 1, 3, 1, new[] { ("item_crop_wheat", 2) }),
                ("recipe_carrot_stew", "Carrot Stew", "item_consumable_food_carrot_stew", 1, 5, 2, new[] { ("item_crop_carrot", 3) }),
                ("recipe_moonbean_soup", "Moonbean Soup", "item_consumable_food_moonbean_soup", 1, 8, 3, new[] { ("item_crop_moonbean", 2) }),
            };

            foreach (var (id, name, output, qty, time, level, ingredients) in recipes)
            {
                CreateRecipe(id, name, output, qty, time, level, ingredients);
            }
        }

        private static void CreateToolRecipes()
        {
            var recipes = new[]
            {
                ("recipe_processed_wood", "Processed Wood", "item_processed_wood", 2, 5, 1, new[] { ("item_wood", 3) }),
            };

            foreach (var (id, name, output, qty, time, level, ingredients) in recipes)
            {
                CreateRecipe(id, name, output, qty, time, level, ingredients);
            }
        }

        private static void CreateRecipe(
            string id,
            string name,
            string outputItemId,
            int outputQty,
            int craftingTime,
            int requiredLevel,
            (string itemId, int quantity)[] ingredients)
        {
            var path = $"{RecipePath}{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CraftingRecipeSO>(path);
            if (existing != null)
                return;

            var asset = ScriptableObject.CreateInstance<CraftingRecipeSO>();
            asset.Id = id;
            asset.RecipeName = name;
            asset.OutputItemId = outputItemId;
            asset.OutputQuantity = outputQty;
            asset.CraftingTimeSeconds = craftingTime;
            asset.RequiredLevel = requiredLevel;

            asset.Ingredients = new CraftingIngredient[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                asset.Ingredients[i] = new CraftingIngredient
                {
                    ItemId = ingredients[i].itemId,
                    Quantity = ingredients[i].quantity
                };
            }

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
