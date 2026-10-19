using System.Collections.Generic;

namespace CindarsHope.UI.Equipment
{
    public class EquipmentComparisonItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Rarity { get; set; }
        public string Quality { get; set; }
        public string MaterialTier { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int DurabilityMax { get; set; }
        public int DurabilityCurrent { get; set; }
        public string[] Bonuses { get; set; }
        public List<StatEntry> PrimaryStats { get; set; } = new List<StatEntry>();
        public List<ResistanceEntry> Resistances { get; set; } = new List<ResistanceEntry>();

        public bool IsDamaged => DurabilityCurrent < DurabilityMax;
        public float DurabilityPercent => DurabilityMax > 0 ? (float)DurabilityCurrent / DurabilityMax : 1f;
    }

    public class StatDiff
    {
        public string StatId { get; set; }
        public string DisplayName { get; set; }
        public int CurrentValue { get; set; }
        public int CandidateValue { get; set; }
        public int Delta => CandidateValue - CurrentValue;
        public bool IsImprovement => Delta > 0;
    }

    public class ResistanceDiff
    {
        public string DamageTypeId { get; set; }
        public float CurrentValue { get; set; }
        public float CandidateValue { get; set; }
        public float Delta => CandidateValue - CurrentValue;
    }

    public class EquipmentComparisonViewModel
    {
        public EquipmentComparisonItem Current { get; set; }
        public EquipmentComparisonItem Candidate { get; set; }
        public List<StatDiff> StatDiffs { get; set; } = new List<StatDiff>();
        public List<ResistanceDiff> ResistanceDiffs { get; set; } = new List<ResistanceDiff>();
        public int DurabilityDiff { get; set; }
        public string MaterialTierDiff { get; set; }
        public string QualityDiff { get; set; }
        public string RarityDiff { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public bool WouldBreakRequirement { get; set; }
        public bool PreviewOnly { get; set; } = true;

        public bool HasComparison => Current != null && Candidate != null;
        public int AttackDelta => (Candidate?.AttackPower ?? 0) - (Current?.AttackPower ?? 0);
        public int DefenseDelta => (Candidate?.Defense ?? 0) - (Current?.Defense ?? 0);
    }
}
