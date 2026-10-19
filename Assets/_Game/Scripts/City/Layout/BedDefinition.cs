using System.Collections.Generic;

namespace CindarsHope.City.Layout
{
    public enum BedType
    {
        Simple = 0,
        Couple,
        Guest,
        Hidden,
        ScheduleOnly
    }

    public class BedDefinition
    {
        public string BedId { get; set; }
        public List<string> OwnerNpcIds { get; set; } = new List<string>();
        public string LocationId { get; set; }
        public BedType BedType { get; set; } = BedType.Simple;
        // ScheduleOnly beds are markers for NPC schedule; cannot be slept in
        public bool ScheduleOnly { get; set; } = false;
        // Player can use only guest beds IF inn/lodging system is implemented
        public bool CanPlayerUse { get; set; } = false;
        public int WidthTiles { get; set; } = 1;
        public int HeightTiles { get; set; } = 1;

        public bool IsNpcBed() => !CanPlayerUse || ScheduleOnly;
    }
}
