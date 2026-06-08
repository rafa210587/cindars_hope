using System.Collections.Generic;
using CindarsHope.City.Layout;
using CindarsHope.City.Schedule;

namespace CindarsHope.City.Validation
{
    public class CityValidationIssue
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class CityLayoutScheduleValidator
    {
        public List<CityValidationIssue> ValidateDoor(DoorTriggerDefinition door)
        {
            var issues = new List<CityValidationIssue>();
            if (door == null) { issues.Add(new CityValidationIssue { Code = "DOOR_NULL", Message = "Door is null", IsBlocker = true }); return issues; }
            if (!door.HasValidTarget())
                issues.Add(new CityValidationIssue { Code = "DOOR_INVALID_TARGET", Message = $"Door '{door.DoorTriggerId}' has no valid TargetSceneId/SpawnPointId", IsBlocker = true });
            if (door.IsHiddenDoor && string.IsNullOrEmpty(door.RequiredStateFlag) && string.IsNullOrEmpty(door.RequiresQuestFlag))
                issues.Add(new CityValidationIssue { Code = "HIDDEN_DOOR_NO_GATE", Message = $"Hidden door '{door.DoorTriggerId}' has no state/quest gate", IsBlocker = false });
            return issues;
        }

        public List<CityValidationIssue> ValidateBed(BedDefinition bed)
        {
            var issues = new List<CityValidationIssue>();
            if (bed == null) { issues.Add(new CityValidationIssue { Code = "BED_NULL", Message = "Bed is null", IsBlocker = true }); return issues; }
            if (string.IsNullOrEmpty(bed.BedId))
                issues.Add(new CityValidationIssue { Code = "BED_NO_ID", Message = "Bed has no BedId", IsBlocker = true });
            // Player must not use NPC beds except explicitly marked guest beds
            if (bed.CanPlayerUse && bed.OwnerNpcIds.Count > 0 && bed.BedType != BedType.Guest)
                issues.Add(new CityValidationIssue { Code = "BED_PLAYER_ON_NPC_BED", Message = $"Bed '{bed.BedId}' allows player but is owned by an NPC and is not Guest type", IsBlocker = true });
            return issues;
        }

        public List<CityValidationIssue> ValidateSchedule(NpcScheduleDefinition schedule)
        {
            var issues = new List<CityValidationIssue>();
            if (schedule == null) { issues.Add(new CityValidationIssue { Code = "SCHEDULE_NULL", Message = "Schedule is null", IsBlocker = true }); return issues; }
            if (string.IsNullOrEmpty(schedule.NpcId))
                issues.Add(new CityValidationIssue { Code = "SCHEDULE_NO_NPC_ID", Message = $"Schedule '{schedule.ScheduleId}' has no NpcId", IsBlocker = true });
            if (schedule.DefaultPeriodBlocks.Count == 0 && string.IsNullOrEmpty(schedule.FallbackWaypointId))
                issues.Add(new CityValidationIssue { Code = "SCHEDULE_NO_BLOCKS_NO_FALLBACK", Message = $"Schedule '{schedule.ScheduleId}' has no period blocks and no fallback waypoint", IsBlocker = false });
            return issues;
        }

        public List<CityValidationIssue> ValidateBuilding(CityBuildingDefinition building,
            Dictionary<string, DoorTriggerDefinition> doorRegistry,
            Dictionary<string, BedDefinition> bedRegistry)
        {
            var issues = new List<CityValidationIssue>();
            if (building == null) { issues.Add(new CityValidationIssue { Code = "BUILDING_NULL", Message = "Building is null", IsBlocker = true }); return issues; }
            foreach (var doorId in building.DoorTriggerIds)
            {
                if (doorRegistry == null || !doorRegistry.ContainsKey(doorId))
                    issues.Add(new CityValidationIssue { Code = "BUILDING_DOOR_MISSING", Message = $"Building '{building.BuildingId}' references unknown door '{doorId}'", IsBlocker = false });
            }
            foreach (var bedId in building.BedIds)
            {
                if (bedRegistry == null || !bedRegistry.ContainsKey(bedId))
                    issues.Add(new CityValidationIssue { Code = "BUILDING_BED_MISSING", Message = $"Building '{building.BuildingId}' references unknown bed '{bedId}'", IsBlocker = false });
            }
            return issues;
        }
    }
}
