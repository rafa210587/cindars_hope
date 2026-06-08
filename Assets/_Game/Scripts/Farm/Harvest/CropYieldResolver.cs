using CindarsHope.Farm.Crops;

namespace CindarsHope.Farm.Harvest
{
    public class CropYieldInput
    {
        public CropDefinitionData Definition { get; set; }
        public CropQualityTier Quality { get; set; }
        public bool FertilizerApplied { get; set; }
        public int Seed { get; set; }
    }

    public static class CropYieldResolver
    {
        public static int Resolve(CropYieldInput input)
        {
            if (input?.Definition == null)
                return 0;

            int min = input.Definition.ExpectedYieldMin;
            int max = input.Definition.ExpectedYieldMax;

            if (min >= max)
                return min;

            // Deterministic yield: use seed to pick within range
            int range = max - min + 1;
            int yieldOffset = ((input.Seed < 0 ? -input.Seed : input.Seed) % range);
            int yield = min + yieldOffset;

            // Quality bonus: Excellent adds 1 extra
            if (input.Quality >= CropQualityTier.Excellent)
                yield = System.Math.Min(yield + 1, max + 1);

            return yield;
        }
    }
}
