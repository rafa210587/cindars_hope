namespace CindarsHope.Farm.Crops
{
    public class CropGrowthState
    {
        public int DaysGrown { get; set; }
        public int DaysWithoutWater { get; set; }
        public bool IsDead { get; set; }
        public bool IsReadyToHarvest { get; set; }
        public int LastProcessedDay { get; set; }

        public CropGrowthState Clone()
        {
            return new CropGrowthState
            {
                DaysGrown = DaysGrown,
                DaysWithoutWater = DaysWithoutWater,
                IsDead = IsDead,
                IsReadyToHarvest = IsReadyToHarvest,
                LastProcessedDay = LastProcessedDay
            };
        }
    }

    public class CropGrowthInput
    {
        public bool IsWateredToday { get; set; }
        public bool IsValidSeason { get; set; } = true;
        public int CurrentDay { get; set; }
        public CropDefinitionData Definition { get; set; }
    }
}
