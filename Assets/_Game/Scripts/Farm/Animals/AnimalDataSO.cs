using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — definição de um animal de fazenda (ScriptableObject), ponte entre o catálogo
    /// canônico de itens/economia e a lógica pura de WAVE 05 (<see cref="AnimalDefinition"/>).
    ///
    /// REUSO: a regra de produto/qualidade/cuidado continua em AnimalCareService / AnimalDailyProcessor /
    /// AnimalProductCollectionService; este SO só carrega os metadados de aquisição, abrigo, ração e
    /// produto (IDs estáveis do ITEM_CATALOG §18). Não duplica capacidade — isso é
    /// <see cref="AnimalHousingCapacityState"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalData", menuName = "CindarsHope/Farm/Animal Data", order = 0)]
    public class AnimalDataSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identidade")]
        [SerializeField] private string _animalId;
        [SerializeField] private string _displayName;
        [SerializeField] private FarmAnimalSpecies _species = FarmAnimalSpecies.Chicken;

        [Header("Abrigo")]
        [SerializeField] private AnimalHousingBuildingType _housingType = AnimalHousingBuildingType.Coop;

        [Header("Economia / itens (ITEM_CATALOG §18)")]
        [Tooltip("Item filhote vendido pelo Eiran; usado para soltar o animal no abrigo.")]
        [SerializeField] private string _purchaseItemId;
        [Tooltip("Item de ração consumido do inventário ao alimentar o animal.")]
        [SerializeField] private string _feedItemId;
        [Tooltip("Item-base de produto (Normal). Variantes _silver/_gold derivadas por qualidade.")]
        [SerializeField] private string _productItemId;

        [Header("Produção")]
        [Tooltip("Cadência de produção em dias (1 = produz a cada dia se alimentado).")]
        [SerializeField] private int _productIntervalDays = 1;
        [Tooltip("Quantidade base de produto por coleta.")]
        [SerializeField] private int _productQuantity = 1;

        [Header("Negligência (EMENDA 5.1-A)")]
        [Tooltip("Dias consecutivos sem alimentação até a MORTE permanente. Decisão Fase 0: N=7.")]
        [SerializeField] private int _neglectDeathDays = 7;

        [Header("Visual placeholder")]
        [SerializeField] private Color _placeholderColor = new Color(0.95f, 0.9f, 0.7f, 1f);

        public string Id => _animalId;
        public string AnimalId => _animalId;
        public string DisplayName => _displayName;
        public FarmAnimalSpecies Species => _species;
        public AnimalHousingBuildingType HousingType => _housingType;
        public string PurchaseItemId => _purchaseItemId;
        public string FeedItemId => _feedItemId;
        public string ProductItemId => _productItemId;
        public int ProductIntervalDays => Mathf.Max(1, _productIntervalDays);
        public int ProductQuantity => Mathf.Max(1, _productQuantity);
        public int NeglectDeathDays => Mathf.Max(1, _neglectDeathDays);
        public Color PlaceholderColor => _placeholderColor;

        /// <summary>
        /// Constrói a <see cref="AnimalDefinition"/> pura (WAVE 05) correspondente a este SO,
        /// para alimentar AnimalCareService/AnimalDailyProcessor sem duplicar a regra de produto.
        /// </summary>
        public AnimalDefinition ToAnimalDefinition()
        {
            var def = new AnimalDefinition(
                _animalId,
                _displayName,
                _species,
                _housingType.ToString(),
                requiredFarmLevel: 1,
                producesProductType: _productItemId,
                productionCadenceDays: ProductIntervalDays,
                requiresFedTodayToProduce: true);
            if (!string.IsNullOrWhiteSpace(_feedItemId))
            {
                def.BaseFeedItemTags.Add(_feedItemId);
            }
            return def;
        }

        /// <summary>Editor/asset-generator hook to populate fields deterministically.</summary>
        public void Configure(
            string animalId,
            string displayName,
            FarmAnimalSpecies species,
            AnimalHousingBuildingType housingType,
            string purchaseItemId,
            string feedItemId,
            string productItemId,
            int productIntervalDays,
            int neglectDeathDays,
            Color placeholderColor)
        {
            _animalId = animalId;
            _displayName = displayName;
            _species = species;
            _housingType = housingType;
            _purchaseItemId = purchaseItemId;
            _feedItemId = feedItemId;
            _productItemId = productItemId;
            _productIntervalDays = productIntervalDays;
            _productQuantity = 1;
            _neglectDeathDays = neglectDeathDays;
            _placeholderColor = placeholderColor;
        }
    }
}
