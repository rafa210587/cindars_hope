namespace CindarsHope.UI.Equipment
{
    /// <summary>
    /// SPEC 04: Equipment comparison view model for side-by-side display.
    /// </summary>
    public class EquipmentComparisonItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Rarity { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int DurabilityMax { get; set; }
        public int DurabilityCurrent { get; set; }
        public string[] Bonuses { get; set; }

        public bool IsDamaged => DurabilityCurrent < DurabilityMax;
        public float DurabilityPercent => DurabilityMax > 0 ? (float)DurabilityCurrent / DurabilityMax : 1f;
    }

    public class EquipmentComparisonViewModel
    {
        public EquipmentComparisonItem Current { get; set; }
        public EquipmentComparisonItem Candidate { get; set; }

        public bool HasComparison => Current != null && Candidate != null;
        public int AttackDelta => (Candidate?.AttackPower ?? 0) - (Current?.AttackPower ?? 0);
        public int DefenseDelta => (Candidate?.Defense ?? 0) - (Current?.Defense ?? 0);
    }
}
