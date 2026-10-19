namespace CindarsHope.Skills
{
    public enum SkillNodeType
    {
        PassiveStat,
        PassiveModifier,
        UnlockSkillAction,
        UpgradeSkillAction,
        UnlockSpell,
        Capstone
    }

    public enum SkillCategory
    {
        PassiveSkill,
        EquippableSkill,
        CapstonePassive
    }

    // arch: SkillModifierType e SkillTreeId (2026-07-15) movidos para CindarsHope.Foundation —
    // ver Foundation/SkillModifierType.cs e Foundation/SkillTreeId.cs. Quebra dos pares mutuos
    // Combat|Skills e Player|Skills (Combat/Player consumiam estes enums puros).
}
