using System.Collections.Generic;

namespace CindarsHope.UI.SkillTree
{
    public class ActiveSkillSlot
    {
        public int SlotIndex { get; set; }
        public string SkillId { get; set; }
        public string SkillName { get; set; }
        public float Cooldown { get; set; }
        public float CooldownMax { get; set; }
        public bool IsReady => Cooldown <= 0;
    }

    public class SkillActiveSlotsViewModel
    {
        public List<ActiveSkillSlot> Slots { get; set; } = new();
        public int MaxSlots { get; set; } = 4;

        public bool CanAddSkill => Slots.Count < MaxSlots;
    }
}
