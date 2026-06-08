using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public class AnimalDefinition
    {
        public string AnimalDataId { get; set; }
        public string DisplayName { get; set; }
        public FarmAnimalSpecies Species { get; set; }
        public string RequiredHomeBuildingType { get; set; }
        public int RequiredFarmLevel { get; set; }
        public List<string> BaseFeedItemTags { get; set; } = new List<string>();
        public List<string> FavoriteFeedItemIds { get; set; } = new List<string>();
        public string ProducesProductType { get; set; }
        public int ProductionCadenceDays { get; set; }
        public bool RequiresFedTodayToProduce { get; set; }
        public bool CareAffectsQuality { get; set; }
        public int MaxCareScore { get; set; }
        public bool IsLateGameReserved { get; set; }

        public AnimalDefinition()
        {
        }

        public AnimalDefinition(string animalDataId, string displayName, FarmAnimalSpecies species,
            string requiredHomeBuildingType, int requiredFarmLevel, string producesProductType,
            int productionCadenceDays, bool requiresFedTodayToProduce)
        {
            AnimalDataId = animalDataId;
            DisplayName = displayName;
            Species = species;
            RequiredHomeBuildingType = requiredHomeBuildingType;
            RequiredFarmLevel = requiredFarmLevel;
            ProducesProductType = producesProductType;
            ProductionCadenceDays = productionCadenceDays;
            RequiresFedTodayToProduce = requiresFedTodayToProduce;
            CareAffectsQuality = true;
            MaxCareScore = 100;
        }
    }
}
