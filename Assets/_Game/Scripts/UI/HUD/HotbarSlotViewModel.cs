namespace CindarsHope.UI.HUD
{
    public class HotbarSlotViewModel
    {
        public int SlotIndex { get; set; }
        public string ItemId { get; set; }
        public string ItemIconId { get; set; }
        public int Quantity { get; set; }
        public float? Durability { get; set; }
        public float? Cooldown { get; set; }
        public bool IsSelected { get; set; }
        public bool IsUsableInContext { get; set; }
        public string BlockedReason { get; set; }
    }

    public enum ActiveSlotBlockedReason
    {
        None = 0, Empty = 1, Locked = 2, Cooldown = 3,
        NoMp = 4, NoStamina = 5, InvalidTarget = 6, WrongContext = 7
    }

    public class ActiveSkillSlotViewModel
    {
        public int SlotIndex { get; set; }
        public string SkillId { get; set; }
        public string IconId { get; set; }
        public float Cooldown { get; set; }
        public int CostMp { get; set; }
        public int CostStamina { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsUsableInContext { get; set; }
        public ActiveSlotBlockedReason BlockedReason { get; set; }
    }

    public class StatusBuffProjection
    {
        public string EffectId { get; set; }
        public string IconId { get; set; }
        public float RemainingDuration { get; set; }
        public int Stacks { get; set; }
        public bool IsNegative { get; set; }
        public string DescriptionKey { get; set; }
    }

    public class ContextPromptProjection
    {
        public string ActionKey { get; set; }
        public string DescriptionKey { get; set; }
        public bool IsVisible { get; set; }
        public string TargetObjectId { get; set; }
    }
}
