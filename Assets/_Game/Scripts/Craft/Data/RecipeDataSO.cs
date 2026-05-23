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

        public void SetId(string id)
        {
            _id = id;
        }

        private void OnValidate()
        {
            RequiredWorkshopLevel = Mathf.Max(1, RequiredWorkshopLevel);
            OutputAmount = Mathf.Max(1, OutputAmount);

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