using CindarsHope.Core.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace CindarsHope.Craft.Data
{
    [CreateAssetMenu(fileName = "RecipeData", menuName = "CindarsHope/Craft/Recipe")]
    public class RecipeDataSO : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private string _id;

        public string Id => _id;
        public string DisplayName;
        [TextArea] public string Description;
        [FormerlySerializedAs("WorkshopType")] public WorkshopType RequiredStationType;
        public int RequiredWorkshopLevel = 1;
        public RecipeIngredient[] Ingredients;
        public string OutputItemId;
        public int OutputAmount = 1;

        public float CraftTimeSeconds = 0f;
        public int StaminaCost = 0;
        [FormerlySerializedAs("IsUnlocked")] public bool IsUnlockedByDefault = true;
        public int RequiredPlayerLevel;
        public string RequiredSkillNodeId;
        public string[] UnlockConditionIds;

        // fable_49 (aditivo): slug de receita aprendida exigido para craftar (ex.: recipe_unlock_mithril_work).
        // Vazio = sem gating de unlock (receitas atuais inalteradas). Quando preenchido, o craft só é aceito
        // se RecipeUnlockService.IsUnlocked(este slug) for true — receitas de tier alto começam bloqueadas.
        public string RequiredRecipeUnlockId;

        public WorkshopType WorkshopType
        {
            get => RequiredStationType;
            set => RequiredStationType = value;
        }

        public bool IsUnlocked
        {
            get => IsUnlockedByDefault;
            set => IsUnlockedByDefault = value;
        }

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
            RequiredPlayerLevel = Mathf.Max(0, RequiredPlayerLevel);

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
