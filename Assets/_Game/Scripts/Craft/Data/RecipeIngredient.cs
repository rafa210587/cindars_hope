using System;
using UnityEngine;

namespace CindarsHope.Craft.Data
{
    [Serializable]
    public struct RecipeIngredient
    {
        public string ItemId;
        public int Amount;

        public RecipeIngredient(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = Mathf.Max(1, amount);
        }
    }
}
