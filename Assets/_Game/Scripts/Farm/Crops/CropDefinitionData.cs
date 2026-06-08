using System.Collections.Generic;

namespace CindarsHope.Farm.Crops
{
    public class CropDefinitionData
    {
        public string CropId { get; set; }
        public string SeedItemId { get; set; }
        public string HarvestItemId { get; set; }
        public string DisplayName { get; set; }
        public int GrowthDays { get; set; } = 1;
        public int RegrowDays { get; set; } = 0;
        public bool RequiresWater { get; set; } = true;
        public int DiesAfterDaysWithoutWater { get; set; } = 3;
        public int ExpectedYieldMin { get; set; } = 1;
        public int ExpectedYieldMax { get; set; } = 1;
        public bool QualityEnabled { get; set; } = true;
        public bool CanBeProcessed { get; set; } = true;
        public bool CanBeShipped { get; set; } = true;
        public bool CanRegrow => RegrowDays > 0;
        public List<string> AllowedSeasonTags { get; set; } = new List<string>();

        public CropDefinitionData() { }

        public CropDefinitionData(string cropId, string seedItemId, string harvestItemId, int growthDays)
        {
            CropId = cropId;
            SeedItemId = seedItemId;
            HarvestItemId = harvestItemId;
            GrowthDays = growthDays > 0 ? growthDays : 1;
        }

        public static List<CropDefinitionData> GetDefaults()
        {
            return new List<CropDefinitionData>
            {
                new CropDefinitionData("crop_turnip", "seed_turnip", "item_turnip", 5) { DisplayName = "Nababo" },
                new CropDefinitionData("crop_pumpkin", "seed_pumpkin", "item_pumpkin", 14) { DisplayName = "Abobora" },
                new CropDefinitionData("crop_herbs", "seed_herbs", "item_herbs", 6) { DisplayName = "Ervas", RegrowDays = 2 },
                new CropDefinitionData("crop_moon_wheat", "seed_moon_wheat", "item_moon_wheat", 10) { DisplayName = "Trigo Lunar", AllowedSeasonTags = new List<string> { "Lunar", "Any" }, QualityEnabled = true },
            };
        }
    }
}
