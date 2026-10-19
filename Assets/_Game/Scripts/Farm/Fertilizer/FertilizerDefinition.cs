using System.Collections.Generic;

namespace CindarsHope.Farm.Fertilizer
{
    public enum FertilizerTier
    {
        Simple = 0,
        Improved = 1,
        Advanced = 2,
        LunarFuture = 10,
        ArcaneFuture = 11,
        LivingWaterFuture = 12
    }

    public enum DurationPolicy
    {
        OneCrop = 0,
        NDays = 1,
        UntilHarvest = 2,
        PermanentUntilTilled = 3
    }

    public enum ApplicationTiming
    {
        BeforePlanting = 0,
        AfterPlanting = 1,
        AnyBeforeReady = 2
    }

    public enum FertilizerStackingPolicy
    {
        ReplaceSameTier = 0,
        ReplaceLowerTier = 1,
        RejectIfAnyFertilizer = 2,
        AllowAdditiveCapped = 3,
        RecipeDefined = 4
    }

    public class FertilizerDefinition
    {
        public string FertilizerId { get; set; }
        public string DisplayName { get; set; }
        public FertilizerTier Tier { get; set; } = FertilizerTier.Simple;
        public ApplicationTiming Timing { get; set; } = ApplicationTiming.AnyBeforeReady;
        public DurationPolicy Duration { get; set; } = DurationPolicy.UntilHarvest;
        public int DurationDays { get; set; }
        public float QualityModifier { get; set; } = 0f;
        public float YieldModifier { get; set; } = 0f;
        public float GrowthModifier { get; set; } = 0f;
        public FertilizerStackingPolicy StackingPolicy { get; set; } = FertilizerStackingPolicy.ReplaceSameTier;
        public int RequiredFarmLevel { get; set; } = 0;
        public bool IsEndgameReserved { get; set; } = false;
        public List<string> AllowedCropTags { get; set; } = new List<string>();

        public static List<FertilizerDefinition> GetDefaults()
        {
            return new List<FertilizerDefinition>
            {
                new FertilizerDefinition
                {
                    FertilizerId = "fertilizer_simple",
                    DisplayName = "Fertilizante Simples",
                    Tier = FertilizerTier.Simple,
                    QualityModifier = 10f,
                    YieldModifier = 0f,
                    Duration = DurationPolicy.UntilHarvest,
                    StackingPolicy = FertilizerStackingPolicy.ReplaceSameTier,
                    IsEndgameReserved = false
                },
                new FertilizerDefinition
                {
                    FertilizerId = "fertilizer_improved",
                    DisplayName = "Fertilizante Melhorado",
                    Tier = FertilizerTier.Improved,
                    QualityModifier = 20f,
                    YieldModifier = 0.2f,
                    Duration = DurationPolicy.UntilHarvest,
                    StackingPolicy = FertilizerStackingPolicy.ReplaceLowerTier,
                    RequiredFarmLevel = 2,
                    IsEndgameReserved = false
                },
                new FertilizerDefinition
                {
                    FertilizerId = "fertilizer_lunar",
                    DisplayName = "Fertilizante Lunar",
                    Tier = FertilizerTier.LunarFuture,
                    IsEndgameReserved = true
                }
            };
        }
    }
}
