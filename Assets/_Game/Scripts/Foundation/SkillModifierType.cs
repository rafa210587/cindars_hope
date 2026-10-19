// arch: quebra dos pares mutuos Combat|Skills e Player|Skills (2026-07-15) — enum puro (sem
// engine nativa) movido de CindarsHope.Skills.SkillEnums para Foundation. Serializa como int, entao
// e transparente para os assets existentes (mesmo molde de EquipmentSlot/HazardType).
namespace CindarsHope.Foundation
{
    public enum SkillModifierType
    {
        AttackFlat = 0,
        DefenseFlat = 1,
        MaxHPFlat = 2,
        MaxStaminaFlat = 3,
        MaxManaFlat = 4,
        ManaRegenFlat = 5,
        BowRangeFlat = 6,
        BowDamageFlat = 7,
        // Legacy ordinals remain pinned because existing ScriptableObjects serialize this enum as int.
        // Phase 17 catalogs no longer use the generic attack/recovery entries for effects whose
        // weapon or action identity matters.
        BowProjectileSpeedFlat = 8,
        CraftTimeReductionPercent = 9,
        RepairEfficiencyBonus = 10,
        HungerDrainReduction = 11,
        ToxicResistanceBonus = 12,
        ColdResistanceBonus = 13,
        HeatResistanceBonus = 14,
        DualWieldAttackSpeedBonus = 15,
        TwoHandedDamageBonus = 16,
        DodgeCostReduction = 17,
        StatusDurationReduction = 18,
        MoveSpeedBonus = 19,
        AttackSpeedBonus = 20,

        // Phase 17: typed channels retain gameplay identity through DerivedStats. Values are
        // per-rank payloads; equipment/action gates belong to their downstream consumer.
        MeleeAttackFlat = 21,
        MagicAttackFlat = 22,
        ArcaneBoltDamageFlat = 23,
        DualWieldRecoverySpeed = 24,
        BowRecoverySpeed = 25,
        ManaRegenBasePercent = 26,
        TerrainPenaltyRecovery = 27,
        KitingMoveSpeedBonus = 28,
        GuardedDefenseFlat = 29,
        StationCommonMaterialReduction = 30
    }
}
