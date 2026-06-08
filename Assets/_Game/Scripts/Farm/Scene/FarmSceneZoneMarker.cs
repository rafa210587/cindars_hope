using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    public enum FarmSceneZoneType
    {
        Unknown = 0,
        PlayerSpawn = 1,
        CropField = 2,
        ResourceTrees = 3,
        ResourceRocks = 4,
        Forage = 5,
        LakeFishing = 6,
        ShippingSellpoint = 7,
        Construction = 8,
        HouseEntrance = 9,
        TownExit = 10,
        CaveEntrance = 11
    }

    public sealed class FarmSceneZoneMarker : MonoBehaviour
    {
        [SerializeField] private FarmSceneZoneType zoneType = FarmSceneZoneType.Unknown;
        [SerializeField] private string stableId = "";

        public FarmSceneZoneType ZoneType => zoneType;
        public string StableId => stableId;
    }
}
