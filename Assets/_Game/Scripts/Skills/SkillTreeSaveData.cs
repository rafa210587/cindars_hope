using System;
using System.Collections.Generic;

namespace CindarsHope.Skills
{
    [Serializable]
    public class SkillTreeSaveData
    {
        // fable_29: section version (additive). 1 = legacy (flat PurchasedNodeIds only),
        // 2 = with NodeRanks + ChosenCapstoneVariants. Old saves deserialize Version = 0/1 and
        // are migrated on load (ranks default to 1; unknown ids refunded).
        public int Version = 2;

        // Back-compat flat list (rank >= 1 nodes). Always written for older readers.
        public List<string> PurchasedNodeIds = new List<string>();

        // fable_29: explicit per-node ranks (1..cap). Source of truth when present.
        public List<SkillNodeRankEntry> NodeRanks = new List<SkillNodeRankEntry>();

        // fable_29: chosen capstone variants (e.g. melee capstone -> "kanthor").
        public List<SkillCapstoneVariantEntry> ChosenCapstoneVariants = new List<SkillCapstoneVariantEntry>();

        public List<ActiveSkillSlotSaveEntry> ActiveSkillSlots = new List<ActiveSkillSlotSaveEntry>();
        public int RespecCount;
    }

    [Serializable]
    public class SkillNodeRankEntry
    {
        public string NodeId;
        public int Rank;

        public SkillNodeRankEntry() { }

        public SkillNodeRankEntry(string nodeId, int rank)
        {
            NodeId = nodeId ?? string.Empty;
            Rank = rank;
        }
    }

    [Serializable]
    public class SkillCapstoneVariantEntry
    {
        public string NodeId;
        public string Variant;

        public SkillCapstoneVariantEntry() { }

        public SkillCapstoneVariantEntry(string nodeId, string variant)
        {
            NodeId = nodeId ?? string.Empty;
            Variant = variant ?? string.Empty;
        }
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
