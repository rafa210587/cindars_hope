namespace CindarsHope.City.Layout
{
    public class DoorTriggerDefinition
    {
        public string DoorTriggerId { get; set; }
        public string SourceLocationId { get; set; }
        public string TargetSceneId { get; set; }
        public string TargetSpawnPointId { get; set; }
        // Optional gate: state flag, relationship, quest flag
        public string RequiredStateFlag { get; set; }
        public string LockedMessage { get; set; }
        public OpenHoursRule OpenHoursRule { get; set; }
        // Door visual size (default 2x2 tiles / 64x64px; temple/guild can be 3x3 / 96x96px)
        public int DoorWidthTiles { get; set; } = 2;
        public int DoorHeightTiles { get; set; } = 2;
        // Optional relationship/quest gates
        public string RequiresRelationshipNpcId { get; set; }
        public int RequiresRelationshipLevel { get; set; } = 0;
        public string RequiresQuestFlag { get; set; }
        public bool IsHiddenDoor { get; set; } = false;

        public bool HasValidTarget() =>
            !string.IsNullOrEmpty(TargetSceneId) && !string.IsNullOrEmpty(TargetSpawnPointId);
    }
}
