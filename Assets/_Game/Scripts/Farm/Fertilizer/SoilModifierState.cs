namespace CindarsHope.Farm.Fertilizer
{
    public class SoilModifierState
    {
        public string PlotId { get; set; }
        public string FertilizerId { get; set; }
        public int AppliedDay { get; set; }
        public int ExpiresDay { get; set; } = -1;
        public string AppliesToCropInstanceId { get; set; }
        public bool ConsumedOnHarvest { get; set; } = true;
        public int RemainingUses { get; set; } = 1;
        public float QualityModifierSnapshot { get; set; }
        public float YieldModifierSnapshot { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsExpired(int currentDay)
        {
            return !IsActive || (ExpiresDay > 0 && currentDay >= ExpiresDay);
        }

        public void Consume()
        {
            RemainingUses = System.Math.Max(0, RemainingUses - 1);
            if (RemainingUses <= 0)
                IsActive = false;
        }
    }
}
