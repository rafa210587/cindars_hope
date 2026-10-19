using System.Collections.Generic;

namespace CindarsHope.Farm.Mining
{
    public enum RockKind
    {
        SmallRock = 0,
        Boulder = 1,
        LightOrePatch = 2,
        ClaySoil = 3,
        QuarryFuture = 10
    }

    public class RockDefinition
    {
        public string RockId { get; set; }
        public RockKind Kind { get; set; } = RockKind.SmallRock;
        public string DisplayName { get; set; }
        public int MaxHits { get; set; } = 2;
        public string RequiredPickaxeTier { get; set; } = "Basic";
        public string DropTableId { get; set; }
        public int RefreshAfterDays { get; set; } = 3;
        public bool CanRefresh { get; set; } = true;
        public bool IsQuarryReserved => Kind == RockKind.QuarryFuture;
        public int RequiredFarmLevel { get; set; } = 0;

        public static List<RockDefinition> GetDefaults()
        {
            return new List<RockDefinition>
            {
                new RockDefinition { RockId = "rock_small", Kind = RockKind.SmallRock, DisplayName = "Pedra Pequena", MaxHits = 1, DropTableId = "drop_stone_small", RefreshAfterDays = 3 },
                new RockDefinition { RockId = "rock_boulder", Kind = RockKind.Boulder, DisplayName = "Pedra Grande", MaxHits = 3, DropTableId = "drop_stone_large", RefreshAfterDays = 5 },
                new RockDefinition { RockId = "rock_clay", Kind = RockKind.ClaySoil, DisplayName = "Argila", MaxHits = 2, DropTableId = "drop_clay", RefreshAfterDays = 2 },
                new RockDefinition { RockId = "rock_quarry_reserved", Kind = RockKind.QuarryFuture, DisplayName = "Pedreira (Reservado)", MaxHits = 0, CanRefresh = false },
            };
        }
    }

    public class RockInstanceState
    {
        public string RockInstanceId { get; set; }
        public string RockId { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string ZoneId { get; set; }
        public bool IsDepleted { get; set; }
        public int RemainingHits { get; set; }
        public int DepletedOnDay { get; set; }
        public int NextRefreshEligibleDay { get; set; }
        public bool IsAvailable => !IsDepleted;
    }
}
