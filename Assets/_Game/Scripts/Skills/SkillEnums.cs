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
        // fable_29 (emenda V3 item 8): the five formerly-dead modifiers below are now ROUTED into
        // the DerivedStats provider (F02) via DerivedStatsCalculator — none is retired, none is
        // left dangling unaggregated on a node. Two reach gameplay TODAY (AttackSpeed/Attack are
        // read by combat); three aggregate into a derived field whose downstream READER is pending
        // (same "efeito pendente" discipline as the named hooks — the value is computed/exposed,
        // the consumer reads it when its system lands). Marked [TODAY] / [reader pending] below.
        BowProjectileSpeedFlat,       // → DerivedStats.BowProjectileSpeed   [reader pending: bow projectile speed]
        CraftTimeReductionPercent,
        RepairEfficiencyBonus,
        HungerDrainReduction,
        ToxicResistanceBonus,
        ColdResistanceBonus,
        HeatResistanceBonus,
        DualWieldAttackSpeedBonus,    // → DerivedStats.AttackSpeed          [TODAY: combat reads AttackSpeed]
        TwoHandedDamageBonus,         // → DerivedStats.Attack               [TODAY: combat reads Attack]
        DodgeCostReduction,           // → DerivedStats.DodgeCostReduction   [reader pending: dodge stamina cost]
        StatusDurationReduction,      // → DerivedStats.StatusDurationReduction [reader pending: player status duration]
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
