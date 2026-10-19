using System.Collections.Generic;

namespace CindarsHope.UI.Tooltips
{
    public class TooltipEquipmentBlock
    {
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int DurabilityCurrent { get; set; }
        public int DurabilityMax { get; set; }
        public string MaterialTier { get; set; }
        public bool HasComparison { get; set; }
    }

    public class TooltipSkillMagicBlock
    {
        public int ManaCost { get; set; }
        public int StaminaCost { get; set; }
        public float Cooldown { get; set; }
        public float CastTime { get; set; }
        public string Shape { get; set; }
        public string Range { get; set; }
        public List<string> StatusEffects { get; set; } = new List<string>();
        public string UnlockSource { get; set; }
    }

    public class TooltipAdvancedBlock
    {
        public List<string> AdditionalEffects { get; set; } = new List<string>();
        public List<string> RequirementsVerbose { get; set; } = new List<string>();
        public string LoreText { get; set; }
        public bool IsDebugOnly { get; set; }
    }

    public class ItemTooltipViewModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Quality { get; set; }
        public string Rarity { get; set; }
        public int? ContextualValue { get; set; }
        public string MainUse { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> PrimaryEffects { get; set; } = new List<string>();
        public List<string> Requirements { get; set; } = new List<string>();
        public List<string> QuestKeyWarnings { get; set; } = new List<string>();
        public bool IsQuestItem { get; set; }
        public bool IsKeyItem { get; set; }
        public TooltipEquipmentBlock EquipmentBlock { get; set; }
        public TooltipSkillMagicBlock SkillMagicBlock { get; set; }
        public TooltipAdvancedBlock AdvancedBlock { get; set; }
        public bool SpoilerSafe { get; set; } = true;
    }
}
