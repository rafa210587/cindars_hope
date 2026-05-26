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

    public enum SkillModifierType
    {
        AttackFlat,
        DefenseFlat,
        MaxHPFlat,
        MaxStaminaFlat,
        MaxManaFlat,
        ManaRegenFlat,
        BowRangeFlat,
        BowDamageFlat,
        BowProjectileSpeedFlat,
        CraftTimeReductionPercent,
        RepairEfficiencyBonus,
        HungerDrainReduction,
        ToxicResistanceBonus,
        ColdResistanceBonus,
        HeatResistanceBonus,
        DualWieldAttackSpeedBonus,
        TwoHandedDamageBonus,
        DodgeCostReduction,
        StatusDurationReduction,
        MoveSpeedBonus,
        AttackSpeedBonus
    }

    public enum SkillTreeId
    {
        Melee,
        Ranged,
        Magic,
        Survival,
        Crafting
    }
}
