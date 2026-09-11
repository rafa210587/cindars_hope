using System;
using System.Collections.Generic;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    public readonly struct CraftingIngredientRequirement
    {
        public string ItemId { get; }
        public int Amount { get; }

        public CraftingIngredientRequirement(string itemId, int amount)
        {
            ItemId = itemId ?? string.Empty;
            Amount = Mathf.Max(1, amount);
        }
    }

    /// <summary>Consumidores puros das passivas determinísticas de crafting da fase 18.</summary>
    public static class CraftingPassiveConsumers
    {
        public const float StationMaterialReductionPerRank = .05f;
        public const float StationMaterialReductionCap = .25f;

        public static float ResolveStationMaterialReduction(float aggregatedReduction,
            WorkshopType stationType, WorkshopType requiredStationType)
        {
            if (stationType == WorkshopType.None || stationType != requiredStationType)
                return 0f;
            return Mathf.Clamp(aggregatedReduction, 0f, StationMaterialReductionCap);
        }

        public static int ResolveIngredientAmount(int baseAmount, float reduction,
            bool isCommonIngredient)
        {
            int safeBase = Mathf.Max(1, baseAmount);
            if (!isCommonIngredient) return safeBase;
            return Mathf.Max(1, Mathf.CeilToInt(safeBase * (1f - Mathf.Clamp01(reduction))));
        }

        public static List<CraftingIngredientRequirement> BuildRequirements(
            RecipeIngredient[] ingredients,
            float reduction,
            Func<string, bool> isCommonIngredient)
        {
            var requirements = new List<CraftingIngredientRequirement>();
            if (ingredients == null) return requirements;

            var indicesByItemId = new Dictionary<string, int>();
            foreach (var ingredient in ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 0)
                    continue;

                bool isCommon = isCommonIngredient?.Invoke(ingredient.ItemId) == true;
                int amount = ResolveIngredientAmount(ingredient.Amount, reduction, isCommon);
                if (indicesByItemId.TryGetValue(ingredient.ItemId, out int index))
                {
                    var existing = requirements[index];
                    requirements[index] = new CraftingIngredientRequirement(
                        existing.ItemId, existing.Amount + amount);
                }
                else
                {
                    indicesByItemId.Add(ingredient.ItemId, requirements.Count);
                    requirements.Add(new CraftingIngredientRequirement(ingredient.ItemId, amount));
                }
            }

            return requirements;
        }

        public static bool IsCommonIngredient(ItemDataSO item)
        {
            return item != null && !item.IsEquippable && item.IsCommonMaterialBonusEligible;
        }
    }
}
