using System;
using System.Linq;

namespace CindarsHope.City.Schedule
{
    // fable_19 (CA-1): motor de resolução do schedule DUPLICADO (WAVE 08), absorvido por NPC/Schedule
    // (canônico). Marcado [Obsolete]; nenhum consumidor novo. #pragma: o resolver referencia as
    // classes-motor obsoletas deste mesmo subsistema dentro deste arquivo.
#pragma warning disable CS0618

    [Obsolete("Absorvido por NPC/Schedule — fable_19. Não usar em código novo.")]
    public class ScheduleResolveContext
    {
        public int CurrentHour { get; set; }
        public bool IsRaining { get; set; }
        public string ActiveLunarPhase { get; set; }
        public bool IsFestivalDay { get; set; }
        public string ActiveQuestFlag { get; set; }
        public int RelationshipLevelWithNpc { get; set; }
    }

    [Obsolete("Absorvido por NPC/Schedule — fable_19. Não usar em código novo.")]
    public class ScheduleResolveResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public SchedulePeriod Period { get; set; }
        public string LocationId { get; set; }
        public string WaypointId { get; set; }
        public string ActivityTag { get; set; }
        public NpcMovementPolicy MovementPolicy { get; set; }
        public bool WasModified { get; set; }
        public string ModifierApplied { get; set; }

        public static ScheduleResolveResult Fail(string reason) =>
            new ScheduleResolveResult { Success = false, FailureReason = reason };
    }

    [Obsolete("Absorvido por NPC/Schedule (NpcScheduleService) — fable_19. Não usar em código novo.")]
    public class NpcScheduleResolver
    {
        // Resolve where an NPC should be right now given current world state
        public ScheduleResolveResult Resolve(NpcScheduleDefinition schedule, ScheduleResolveContext ctx)
        {
            if (schedule == null) return ScheduleResolveResult.Fail("schedule is null");
            if (ctx == null) return ScheduleResolveResult.Fail("context is null");

            var period = SchedulePeriodHelper.FromHour(ctx.CurrentHour);

            // Festival overrides take highest priority
            if (ctx.IsFestivalDay)
            {
                var festivalBlock = schedule.FestivalOverrides
                    .FirstOrDefault(m => m.AffectedPeriod == period);
                if (festivalBlock != null)
                    return FromModifier(festivalBlock, period, schedule, "Festival");
            }

            // Lunar phase modifiers
            if (!string.IsNullOrEmpty(ctx.ActiveLunarPhase))
            {
                var lunarBlock = schedule.LunarModifiers
                    .FirstOrDefault(m => m.ConditionId == ctx.ActiveLunarPhase && m.AffectedPeriod == period);
                if (lunarBlock != null)
                    return FromModifier(lunarBlock, period, schedule, $"Lunar:{ctx.ActiveLunarPhase}");
            }

            // Rain modifiers
            if (ctx.IsRaining)
            {
                var rainBlock = schedule.WeatherModifiers
                    .FirstOrDefault(m => m.ModifierType == ScheduleModifierType.Rain && m.AffectedPeriod == period);
                if (rainBlock != null)
                    return FromModifier(rainBlock, period, schedule, "Rain");
            }

            // Quest overrides
            if (!string.IsNullOrEmpty(ctx.ActiveQuestFlag))
            {
                var questBlock = schedule.QuestOverrides
                    .FirstOrDefault(m => m.ConditionId == ctx.ActiveQuestFlag && m.AffectedPeriod == period);
                if (questBlock != null)
                    return FromModifier(questBlock, period, schedule, $"Quest:{ctx.ActiveQuestFlag}");
            }

            // Default period block
            var defaultBlock = schedule.DefaultPeriodBlocks.FirstOrDefault(b => b.Period == period);
            if (defaultBlock != null)
            {
                return new ScheduleResolveResult
                {
                    Success = true,
                    Period = period,
                    LocationId = defaultBlock.LocationId,
                    WaypointId = defaultBlock.WaypointId ?? schedule.FallbackWaypointId,
                    ActivityTag = defaultBlock.ActivityTag,
                    MovementPolicy = schedule.VisibleMovementPolicy,
                    WasModified = false
                };
            }

            // Fallback
            return new ScheduleResolveResult
            {
                Success = true,
                Period = period,
                LocationId = null,
                WaypointId = schedule.FallbackWaypointId,
                MovementPolicy = NpcMovementPolicy.Static,
                WasModified = false,
                ModifierApplied = "FallbackWaypoint"
            };
        }

        private static ScheduleResolveResult FromModifier(ScheduleModifierBlock block, SchedulePeriod period,
            NpcScheduleDefinition schedule, string modifierLabel)
        {
            return new ScheduleResolveResult
            {
                Success = true,
                Period = period,
                LocationId = block.OverrideLocationId,
                WaypointId = block.OverrideWaypointId ?? schedule.FallbackWaypointId,
                ActivityTag = block.OverrideActivityTag,
                MovementPolicy = schedule.OffscreenTickPolicy,
                WasModified = true,
                ModifierApplied = modifierLabel
            };
        }
    }
#pragma warning restore CS0618
}
