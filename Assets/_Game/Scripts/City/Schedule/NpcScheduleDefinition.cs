using System;
using System.Collections.Generic;

namespace CindarsHope.City.Schedule
{
    // fable_19 (CA-1): sistema de schedule DUPLICADO (WAVE 08) absorvido por NPC/Schedule (WI-25,
    // canônico). Estas classes-MOTOR (definição/resolver/policy) são marcadas [Obsolete]; nenhum
    // consumidor NOVO deve referenciá-las. A semântica de períodos nomeados foi absorvida em
    // NPC/Schedule/NpcSchedulePeriod (crosswalk 1:1) e a cama/home na âncora npc_<id>_home.
    // Remoção física via delete candidates pós-F11. O enum semântico SchedulePeriod permanece
    // (não-obsoleto) pois é vocabulário consumido por Dialogue/* (fora do escopo desta spec).
    // #pragma abaixo: as classes-motor referenciam umas às outras dentro deste arquivo.
#pragma warning disable CS0618

    [Obsolete("Absorvido por NPC/Schedule (NpcTimeBlock/Anchor) — fable_19. Não usar em código novo.")]
    public enum NpcMovementPolicy
    {
        WaypointWalk = 0,   // Visible NPCs walk between nearby waypoints
        OffscreenTick,      // Offscreen/different scene: tick state, teleport to waypoint on entry
        Static              // Doesn't move
    }

    [Obsolete("Absorvido por NPC/Schedule — fable_19. Não usar em código novo.")]
    public class SchedulePeriodBlock
    {
        public SchedulePeriod Period { get; set; }
        public string LocationId { get; set; }
        public string WaypointId { get; set; }
        public string ActivityTag { get; set; }
    }

    [Obsolete("Absorvido por NPC/Schedule — fable_19. Não usar em código novo.")]
    public class ScheduleModifierBlock
    {
        public ScheduleModifierType ModifierType { get; set; }
        public string ConditionId { get; set; }
        public SchedulePeriod AffectedPeriod { get; set; }
        public string OverrideLocationId { get; set; }
        public string OverrideWaypointId { get; set; }
        public string OverrideActivityTag { get; set; }
    }

    [Obsolete("Absorvido por NPC/Schedule (canônico) — fable_19. Não usar em código novo.")]
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
#pragma warning restore CS0618
}
