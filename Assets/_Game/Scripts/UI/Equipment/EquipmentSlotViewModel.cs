using System.Collections.Generic;

namespace CindarsHope.UI.Equipment
{
    public class StatEntry
    {
        public string StatId { get; set; }
        public string DisplayName { get; set; }
        public int Value { get; set; }
    }

    public class ResistanceEntry
    {
        public string DamageTypeId { get; set; }
        public float Value { get; set; }
    }

    public enum SlotWarningState { None = 0, Damaged = 1, Broken = 2, UnderRepair = 3, RequirementNotMet = 4 }

    public class EquipmentSlotViewModel
    {
        public string SlotType { get; set; }
        public string EquippedItemId { get; set; }
        public string IconId { get; set; }
        public int DurabilityCurrent { get; set; }
        public int DurabilityMax { get; set; }
        public string Quality { get; set; }
        public string Rarity { get; set; }
        public string MaterialTier { get; set; }
        public List<StatEntry> PrimaryStats { get; set; } = new List<StatEntry>();
        public List<ResistanceEntry> Resistances { get; set; } = new List<ResistanceEntry>();
        public SlotWarningState WarningState { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(EquippedItemId);
        public float DurabilityPercent => DurabilityMax > 0 ? (float)DurabilityCurrent / DurabilityMax : 1f;
    }
}
