using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Crafting
{
    /// <summary>
    /// Legacy compatibility wrapper kept only to avoid breaking existing Unity asset references during stabilization.
    /// The official runtime recipe model is CindarsHope.Craft.Data.RecipeDataSO.
    /// Do not create new assets with this type.
    /// </summary>
    [Obsolete("Use CindarsHope.Craft.Data.RecipeDataSO as the official crafting recipe model.")]
    public class CraftingRecipeSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string RecipeName;
        public string OutputItemId;
        public int OutputQuantity = 1;
        public int CraftingTimeSeconds = 5;
        public CraftingIngredient[] Ingredients;
        public int RequiredLevel = 1;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            CraftingTimeSeconds = Mathf.Max(1, CraftingTimeSeconds);
            OutputQuantity = Mathf.Max(1, OutputQuantity);
            RequiredLevel = Mathf.Max(1, RequiredLevel);
        }
    }

    [Serializable]
    public class CraftingIngredient
    {
        public string ItemId;
        public int Quantity = 1;
    }
}