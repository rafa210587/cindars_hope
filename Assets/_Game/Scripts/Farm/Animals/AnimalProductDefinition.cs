using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public enum AnimalProductQuality
    {
        Normal = 0,
        Boa = 1,
        Excelente = 2,
        Rara = 3,
        Lunar = 4
    }

    public class AnimalProductDefinition
    {
        public string ProductDefinitionId { get; set; }
        public FarmAnimalSpecies SourceAnimalSpecies { get; set; }
        public string OutputItemId { get; set; }
        public int BaseQuantity { get; set; } = 1;
        public int ProductionCadenceDays { get; set; } = 1;
        public bool RequiresFedToday { get; set; } = true;
        public bool RequiresProductReady { get; set; } = true;
        public bool QualityEnabled { get; set; } = true;
        public bool CanBeProcessed { get; set; } = true;
        public bool CanBeUsedAsFertilizerBase { get; set; } = false;
        public bool IsLateGameReserved { get; set; } = false;

        public AnimalProductDefinition()
        {
        }

        public AnimalProductDefinition(string definitionId, FarmAnimalSpecies species, string outputItemId,
            int baseCadenceDays)
        {
            ProductDefinitionId = definitionId;
            SourceAnimalSpecies = species;
            OutputItemId = outputItemId;
            ProductionCadenceDays = baseCadenceDays;
        }

        public static List<AnimalProductDefinition> GetDefaults()
        {
            return new List<AnimalProductDefinition>
            {
                new AnimalProductDefinition("milk", FarmAnimalSpecies.Cow, "item_milk", 1)
                {
                    BaseQuantity = 1,
                    CanBeProcessed = true,
                    CanBeUsedAsFertilizerBase = false
                },
                new AnimalProductDefinition("fertilizer_base", FarmAnimalSpecies.Cow, "item_fertilizer_base", 1)
                {
                    BaseQuantity = 1,
                    CanBeProcessed = false,
                    CanBeUsedAsFertilizerBase = true
                },
                new AnimalProductDefinition("egg", FarmAnimalSpecies.Chicken, "item_egg", 1)
                {
                    BaseQuantity = 1,
                    CanBeProcessed = true
                },
                new AnimalProductDefinition("wool", FarmAnimalSpecies.Sheep, "item_wool", 3)
                {
                    BaseQuantity = 1,
                    CanBeProcessed = true
                },
                new AnimalProductDefinition("rare_resource", FarmAnimalSpecies.FantasySmallFuture, "item_rare_fantasy", 7)
                {
                    BaseQuantity = 1,
                    IsLateGameReserved = true
                }
            };
        }
    }
}
