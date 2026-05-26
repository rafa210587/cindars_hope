using System;
using UnityEngine;

namespace CindarsHope.Skills
{
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
