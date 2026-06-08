using System.Collections.Generic;
using CindarsHope.Farm.Crops;

namespace CindarsHope.Farm.Harvest
{
    public enum HarvestFailureReason
    {
        None = 0,
        PlotEmpty,
        CropNotReady,
        CropDead,
        InvalidCropId,
        InventoryFull,
        InvalidTool,
        AlreadyHarvested,
        SaveStateInvalid
    }

    public class HarvestedItemEntry
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public CropQualityTier Quality { get; set; }
    }

    public class HarvestResult
    {
        public bool Success { get; set; }
        public HarvestFailureReason FailureReason { get; set; }
        public List<HarvestedItemEntry> HarvestedItems { get; set; } = new List<HarvestedItemEntry>();
        public CropQualityTier Quality { get; set; }
        public int YieldAmount { get; set; }
        public bool RegrowStarted { get; set; }
        public bool InventoryOverflowed { get; set; }
        public List<string> TriggeredEvents { get; set; } = new List<string>();

        public static HarvestResult Ok(string itemId, int quantity, CropQualityTier quality, bool regrowStarted)
        {
            return new HarvestResult
            {
                Success = true,
                Quality = quality,
                YieldAmount = quantity,
                RegrowStarted = regrowStarted,
                HarvestedItems = new List<HarvestedItemEntry>
                {
                    new HarvestedItemEntry { ItemId = itemId, Quantity = quantity, Quality = quality }
                },
                TriggeredEvents = new List<string> { "CropHarvested" }
            };
        }

        public static HarvestResult Fail(HarvestFailureReason reason)
        {
            return new HarvestResult { Success = false, FailureReason = reason };
        }
    }
}
