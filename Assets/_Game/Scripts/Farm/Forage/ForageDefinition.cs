using System.Collections.Generic;

namespace CindarsHope.Farm.Forage
{
    public enum ForageRarity { Common = 0, Uncommon = 1, Rare = 2, LunarFuture = 10, ArcaneFuture = 11 }

    public class ForageDefinition
    {
        public string ForageId { get; set; }
        public string DisplayName { get; set; }
        public string ItemId { get; set; }
        public List<string> AllowedSeasons { get; set; } = new List<string>();
        public List<string> AllowedWeather { get; set; } = new List<string>();
        public List<string> AllowedZones { get; set; } = new List<string>();
        public int RequiredFarmLevel { get; set; } = 0;
        public ForageRarity Rarity { get; set; } = ForageRarity.Common;
        public bool CanRespawnSameSeason { get; set; } = true;
        public bool IsLoreProtected { get; set; } = false;
        public bool IsEndgameReserved => Rarity == ForageRarity.LunarFuture || Rarity == ForageRarity.ArcaneFuture;
        public int RespawnAfterDays { get; set; } = 1;
        public float SpawnChance { get; set; } = 1.0f;

        public bool IsCollectable => !IsLoreProtected && !IsEndgameReserved;
    }
}
