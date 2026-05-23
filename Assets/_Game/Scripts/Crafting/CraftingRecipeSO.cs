using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Crafting
{
    [CreateAssetMenu(fileName = "Recipe_", menuName = "CindarsHope/Crafting/CraftingRecipe")]
    public class CraftingRecipeSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string RecipeName;
        public string OutputItemId;
        public int OutputQuantity = 1;
        public int CraftingTimeSeconds = 5;
        public CraftingIngredient[] Ingredients;
        public int RequiredLevel;

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
