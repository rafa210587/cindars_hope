using System;

namespace CindarsHope.Foundation
{
    // arch: quebra dos pares mutuos Combat|Skills e Player|Skills (2026-07-15) — tipo puro
    // (sem engine nativa) movido de CindarsHope.Skills para Foundation; Combat/Player consomem
    // via this namespace em vez de nomear CindarsHope.Skills.
    [Serializable]
    public class SkillPassiveModifier
    {
        public SkillModifierType ModifierType;
        public float Value;
        public string SourceNodeId = string.Empty;
        public string SourceTreeId = string.Empty;

        public SkillPassiveModifier() { }

        public SkillPassiveModifier(SkillModifierType type, float value)
        {
            ModifierType = type;
            Value = value;
        }

        public SkillPassiveModifier(SkillModifierType type, float value, string sourceNodeId, string sourceTreeId)
        {
            ModifierType = type;
            Value = value;
            SourceNodeId = sourceNodeId ?? string.Empty;
            SourceTreeId = sourceTreeId ?? string.Empty;
        }
    }
}
