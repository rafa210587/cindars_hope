namespace CindarsHope.Farm
{
    public enum FarmPlotState
    {
        Blocked = -1,
        Raw = 0,
        TilledDry = 1,
        TilledWet = 2,
        PlantedDry = 3,
        PlantedWet = 4,
        ReadyToHarvest = 5,
        Dead = 6,

        Empty = Raw,
        Growing = PlantedDry,
        Ready = ReadyToHarvest
    }
}
