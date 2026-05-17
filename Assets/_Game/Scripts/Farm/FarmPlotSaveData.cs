using System;

namespace CindarsHope.Farm
{
    [Serializable]
    public class FarmPlotSaveData
    {
        public int PlotIndex;
        public string State;
        public string PlantedSeedId;
        public int DaysGrown;
    }
}
