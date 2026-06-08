using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Watering
{
    public class FarmPlotWaterState
    {
        public string PlotId { get; set; }
        public Vector2Int TilePosition { get; set; }
        public bool IsWateredToday { get; set; }
        public List<WaterSource> WateredBySources { get; set; } = new List<WaterSource>();
        public int LastWateredDay { get; set; }
        public string IrrigationCoverageId { get; set; }
        public string GreenhouseId { get; set; }
        public bool IsExternal { get; set; } = true;
        public bool IsInterior { get; set; }
        public bool IsGreenhouse { get; set; }

        public void MarkWatered(WaterSource source, int dayNumber)
        {
            IsWateredToday = true;
            LastWateredDay = dayNumber;
            if (!WateredBySources.Contains(source))
                WateredBySources.Add(source);
        }

        public void ResetDayWaterState()
        {
            IsWateredToday = false;
            WateredBySources.Clear();
        }
    }
}
