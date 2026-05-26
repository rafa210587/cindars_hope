using System;
using System.Collections.Generic;

namespace CindarsHope.Skills
{
    [Serializable]
    public class SkillTreeSaveData
    {
        public List<string> PurchasedNodeIds = new List<string>();
        public List<ActiveSkillSlotSaveEntry> ActiveSkillSlots = new List<ActiveSkillSlotSaveEntry>();
        public int RespecCount;
    }

    [Serializable]
    public class ActiveSkillSlotSaveEntry
    {
        public int SlotIndex;
        public string InputKey;
        public string SkillActionId;

        public ActiveSkillSlotSaveEntry() { }

        public ActiveSkillSlotSaveEntry(int slotIndex, string inputKey, string skillActionId)
        {
            SlotIndex = slotIndex;
            InputKey = inputKey;
            SkillActionId = skillActionId ?? string.Empty;
        }
    }
}
