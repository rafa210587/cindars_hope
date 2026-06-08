using System.Collections.Generic;

namespace CindarsHope.City.Schedule
{
    public enum NpcMovementPolicy
    {
        WaypointWalk = 0,   // Visible NPCs walk between nearby waypoints
        OffscreenTick,      // Offscreen/different scene: tick state, teleport to waypoint on entry
        Static              // Doesn't move
    }

    public class SchedulePeriodBlock
    {
        public SchedulePeriod Period { get; set; }
        public string LocationId { get; set; }
        public string WaypointId { get; set; }
        public string ActivityTag { get; set; }
    }

    public class ScheduleModifierBlock
    {
        public ScheduleModifierType ModifierType { get; set; }
        public string ConditionId { get; set; }
        public SchedulePeriod AffectedPeriod { get; set; }
        public string OverrideLocationId { get; set; }
        public string OverrideWaypointId { get; set; }
        public string OverrideActivityTag { get; set; }
    }

    public class NpcScheduleDefinition
    {
        public string ScheduleId { get; set; }
        public string NpcId { get; set; }
        // Default location blocks per period
        public List<SchedulePeriodBlock> DefaultPeriodBlocks { get; set; } = new List<SchedulePeriodBlock>();
        // Overrides by weather/lunar/festival/quest/relationship
        public List<ScheduleModifierBlock> WeatherModifiers { get; set; } = new List<ScheduleModifierBlock>();
        public List<ScheduleModifierBlock> LunarModifiers { get; set; } = new List<ScheduleModifierBlock>();
        public List<ScheduleModifierBlock> FestivalOverrides { get; set; } = new List<ScheduleModifierBlock>();
        public List<ScheduleModifierBlock> QuestOverrides { get; set; } = new List<ScheduleModifierBlock>();
        public List<ScheduleModifierBlock> RelationshipOverrides { get; set; } = new List<ScheduleModifierBlock>();
        public string FallbackWaypointId { get; set; }
        public NpcMovementPolicy VisibleMovementPolicy { get; set; } = NpcMovementPolicy.WaypointWalk;
        public NpcMovementPolicy OffscreenTickPolicy { get; set; } = NpcMovementPolicy.OffscreenTick;
    }
}
