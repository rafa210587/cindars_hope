namespace CindarsHope.Farm.Crops
{
    public enum CropQualityTier
    {
        Normal = 0,
        Good = 1,
        Excellent = 2,
        Rare = 3,
        Arcane = 4
    }

    public class CropQualityInput
    {
        public int WateringConsistencyScore { get; set; }
        public bool SeasonMatch { get; set; } = true;
        public bool FertilizerApplied { get; set; }
        public bool IsQualityEnabled { get; set; } = true;
    }

    public static class CropQualityResolver
    {
        private const int ExcellentThreshold = 80;
        private const int GoodThreshold = 50;
        private const int SeasonBonus = 10;
        private const int FertilizerBonus = 15;

        public static CropQualityTier Resolve(CropQualityInput input)
        {
            if (input == null || !input.IsQualityEnabled)
                return CropQualityTier.Normal;

            int score = input.WateringConsistencyScore;
            if (input.SeasonMatch) score += SeasonBonus;
            if (input.FertilizerApplied) score += FertilizerBonus;

            if (score >= ExcellentThreshold) return CropQualityTier.Excellent;
            if (score >= GoodThreshold) return CropQualityTier.Good;
            return CropQualityTier.Normal;
        }
    }
}
