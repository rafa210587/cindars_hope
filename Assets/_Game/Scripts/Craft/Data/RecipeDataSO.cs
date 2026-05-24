using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Craft.Data
{
    [CreateAssetMenu(fileName = "RecipeData", menuName = "CindarsHope/Craft/Recipe")]
    public class RecipeDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;

        public string Id => _id;
        public string DisplayName;
        [TextArea] public string Description;
        public WorkshopType WorkshopType;
        public int RequiredWorkshopLevel = 1;
        public RecipeIngredient[] Ingredients;
        public string OutputItemId;
        public int OutputAmount = 1;

        // Spec 07 - Crafting queue and timing
        public float CraftTimeSeconds = 0f;
        public int StaminaCost = 0;
        public bool IsUnlocked = true;

        public void SetId(string id)
        {
            _id = id;
        }

        public bool IsInstantaneous => CraftTimeSeconds <= 0f;

        private void OnValidate()
        {
            RequiredWorkshopLevel = Mathf.Max(1, RequiredWorkshopLevel);
            OutputAmount = Mathf.Max(1, OutputAmount);
            CraftTimeSeconds = Mathf.Max(0f, CraftTimeSeconds);
            StaminaCost = Mathf.Max(0, StaminaCost);

            if (Ingredients == null)
            {
                return;
            }

            for (var index = 0; index < Ingredients.Length; index++)
            {
                Ingredients[index].Amount = Mathf.Max(1, Ingredients[index].Amount);
            }
        }
    }
}