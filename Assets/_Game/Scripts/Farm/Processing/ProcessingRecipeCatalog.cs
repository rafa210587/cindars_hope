using System.Collections.Generic;

namespace CindarsHope.Farm.Processing
{
    /// <summary>
    /// fable_55 — catálogo canônico (em código, v1) das 2 receitas de processamento físico da
    /// fazenda. Números do ITEM_CATALOG §5: goat_milk ×2 → goat_cheese (1 dia) na queijaria;
    /// grape ×5 → vale_wine (2 dias) no barril. IDs de item confirmados no CanonicalItemCatalog
    /// (F32): item_animal_goat_milk, item_consumable_food_goat_cheese, item_crop_brigandini_grape,
    /// item_consumable_food_vale_wine.
    ///
    /// Estas receitas pertencem à superfície de job POR DIAS (FarmProcessingJob). NÃO confundir
    /// com recipe_goat_cheese / recipe_vale_wine do RecipeDatabaseSO (cook instantâneo do banco de
    /// crafting WI-14) — conceitos e superfícies distintos (decisão Fase 0 da spec).
    /// </summary>
    public static class ProcessingRecipeCatalog
    {
        public const string StationCheesePressId = "station_cheese_press_01";
        public const string StationWineBarrelId = "station_wine_barrel_01";

        public const string RecipeGoatCheeseId = "processing_goat_cheese";
        public const string RecipeValeWineId = "processing_vale_wine";

        public const string ItemGoatMilkId = "item_animal_goat_milk";
        public const string ItemGoatCheeseId = "item_consumable_food_goat_cheese";
        public const string ItemGrapeId = "item_crop_brigandini_grape";
        public const string ItemValeWineId = "item_consumable_food_vale_wine";

        private static readonly Dictionary<string, ProcessingRecipe> RecipesById = BuildRecipesById();
        private static readonly Dictionary<string, ProcessingRecipe> RecipesByStation = BuildRecipesByStation();

        public static IReadOnlyDictionary<string, ProcessingRecipe> All => RecipesById;

        public static bool TryGetById(string recipeId, out ProcessingRecipe recipe)
        {
            recipe = null;
            return !string.IsNullOrEmpty(recipeId) && RecipesById.TryGetValue(recipeId, out recipe);
        }

        /// <summary>Receita única associada à estação (cada estação processa uma receita no v1).</summary>
        public static bool TryGetByStation(string stationId, out ProcessingRecipe recipe)
        {
            recipe = null;
            return !string.IsNullOrEmpty(stationId) && RecipesByStation.TryGetValue(stationId, out recipe);
        }

        private static List<ProcessingRecipe> BuildDefinitions()
        {
            return new List<ProcessingRecipe>
            {
                new ProcessingRecipe(
                    RecipeGoatCheeseId, StationCheesePressId,
                    ItemGoatMilkId, 2,
                    ItemGoatCheeseId, 1,
                    1),
                new ProcessingRecipe(
                    RecipeValeWineId, StationWineBarrelId,
                    ItemGrapeId, 5,
                    ItemValeWineId, 1,
                    2),
            };
        }

        private static Dictionary<string, ProcessingRecipe> BuildRecipesById()
        {
            var map = new Dictionary<string, ProcessingRecipe>();
            foreach (var recipe in BuildDefinitions())
            {
                map[recipe.RecipeId] = recipe;
            }

            return map;
        }

        private static Dictionary<string, ProcessingRecipe> BuildRecipesByStation()
        {
            var map = new Dictionary<string, ProcessingRecipe>();
            foreach (var recipe in BuildDefinitions())
            {
                map[recipe.StationId] = recipe;
            }

            return map;
        }
    }
}
