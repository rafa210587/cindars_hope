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

        public SkillPassiveModifier() { }

        public SkillPassiveModifier(SkillModifierType type, float value)
        {
            ModifierType = type;
            Value = value;
        }
    }
}
