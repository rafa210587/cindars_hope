namespace CindarsHope.UI.Equipment
{
    public class RepairUpgradeViewModel
    {
        public enum ServiceType { Repair, Upgrade }

        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int DurabilityCurrent { get; set; }
        public int DurabilityMax { get; set; }
        public int UpgradeLevel { get; set; }

        public string RepairCost { get; set; }
        public string UpgradeCost { get; set; }
        public string UpgradeBonus { get; set; }

        public bool CanRepair => DurabilityCurrent < DurabilityMax;
        public bool CanUpgrade => UpgradeLevel < 5;
    }
}
