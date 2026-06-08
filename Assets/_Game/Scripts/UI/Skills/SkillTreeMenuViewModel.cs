using System.Collections.Generic;

namespace CindarsHope.UI.Skills
{
    public enum SkillNodeState { Locked = 0, Available = 1, Purchased = 2, MaxRank = 3, CapstoneExclusive = 4 }

    public class SkillNodeViewModel
    {
        public string NodeId { get; set; }
        public string DisplayName { get; set; }
        public SkillNodeState State { get; set; }
        public int CurrentRank { get; set; }
        public int MaxRank { get; set; }
        public int CostPerRank { get; set; }
        public List<string> PrerequisiteNodeIds { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool IsCapstone { get; set; }
        public string LockReason { get; set; }
        public bool IsEquipped { get; set; }
        public bool CanPurchase { get; set; }
    }

    public class SkillTreeTabViewModel
    {
        public string TreeId { get; set; }
        public string TreeName { get; set; }
        public List<SkillNodeViewModel> Nodes { get; set; } = new List<SkillNodeViewModel>();
    }

    public class ActiveSlotSummary
    {
        public int UsedSlots { get; set; }
        public int MaxSlots { get; set; } = 4;
        public List<string> EquippedSkillIds { get; set; } = new List<string>();
        public bool IsFull => UsedSlots >= MaxSlots;
    }

    public class SkillTreeMenuViewModel
    {
        public int AvailableSkillPoints { get; set; }
        public List<SkillTreeTabViewModel> TreeTabs { get; set; } = new List<SkillTreeTabViewModel>();
        public SkillNodeViewModel SelectedNode { get; set; }
        public string PrerequisiteSummary { get; set; }
        public string RankInfo { get; set; }
        public string CapstoneInfo { get; set; }
        public ActiveSlotSummary ActiveSlotSummary { get; set; } = new ActiveSlotSummary();
        public bool RespecAvailable { get; set; }
        public int RespecCost { get; set; }
        public string RespecBlockedReason { get; set; }
        public bool CanConfirmSpend { get; set; }
    }
}
