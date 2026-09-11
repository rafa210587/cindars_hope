using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime.Effects;
using UnityEngine;

namespace CindarsHope.Skills
{
    /// <summary>Code fallback and generator source for the 31 stable active-skill definitions.</summary>
    public static class DefaultSkillActionCatalog
    {
        public const int CanonicalActionCount = 31;

        public static List<SkillActionSO> BuildAll()
        {
            return new List<SkillActionSO>
            {
                WithTiming(Melee("skill_melee_offhand_cut", "Corte da Mão Secundária", 8, 2, 10, 2.5f, 1.2f, 140), "M1", .10f, .10f, .20f),
                Melee("skill_melee_battle_dash", "Arrancada de Combate", 8, 2, 20, 5, 1.2f, 100, lunge: 3),
                AsLeap(Melee("skill_melee_leap_attack", "Salto Devastador", 14, 3, 25, 7, 1.4f, 360), 2.2f),
                WithTiming(Melee("skill_melee_whirl_cut", "Corte Giratório", 10, 2, 22, 6, 1.7f, 360, maxTargets: 6, fullDamageTargets: 3, falloff: .7f), "M2", .20f, .18f, .38f),
                Melee("skill_melee_avanco_aco", "Avanço de Aço", 12, 3, 22, 6, 1.3f, 110, lunge: 2.5f),
                Melee("skill_melee_grito_desafio", "Grito de Desafio", 6, 1, 18, 8, 2.2f, 360, knockback: 6),
                Melee("skill_melee_investida_quebra_guarda", "Investida Quebra-Guarda", 16, 4, 26, 9, 1.3f, 90, lunge: 2, posture: 3),

                Charged(Projectile(SkillActionEffectCatalog.RangedChargedShotActionId, "Disparo Carregado", 8, 0, 12, 0, 6, 5, 12, DamageType.Physical)),
                Projectile(SkillActionEffectCatalog.RangedLinePiercerActionId, "Linha Perfurante", 12, 3, 18, 0, 7, 10, 14, DamageType.Physical, pierce: 5),
                Projectile(SkillActionEffectCatalog.RangedMultishotFanActionId, "Tiro Triplo", 8, 2, 24, 0, 8, 7, 11, DamageType.Physical, count: 3, spread: 28),
                Projectile(SkillActionEffectCatalog.RangedBleedingArrowActionId, "Flecha Sangrante", 14, 3, 16, 0, 6, 8, 12, DamageType.Physical, status: "status_bleed"),
                MarkedPrey(Utility(SkillActionEffectCatalog.RangedMarkedPreyActionId, "Presa Marcada", 10, 0, 10, range: 8)),

                Discipline(WithTiming(Projectile("skill_magic_fire_spark", "Fagulha Ígnea", 12, 2, 0, 10, 3, 7, 10, DamageType.Fire), "MAGIC_PROJECTILE", .18f, 0f, .25f), SpellDiscipline.Offensive),
                Discipline(WithTiming(IceBind(Projectile("skill_magic_ice_bind", "Laço de Gelo", 10, 2, 0, 14, 6, 7, 9, DamageType.Ice, status: "status_chill")), "MAGIC_PROJECTILE", .25f, 0f, .25f), SpellDiscipline.Offensive),
                Discipline(WithTiming(ToxicCloud(Area("skill_magic_toxic_cloud", "Nuvem Tóxica", 24, 6, 0, 18, 8, 2f, "status_poison")), "MAGIC_FIELD", .70f, .10f, .40f), SpellDiscipline.Offensive),
                Discipline(WithTiming(LightningChain(Area("skill_magic_lightning_chain", "Corrente Relâmpago", 12, 2, 0, 20, 8, 9f)), "MAGIC_CHAIN", .35f, 0f, .30f), SpellDiscipline.Offensive),
                Discipline(WithTiming(ElementalWard(Utility("skill_magic_elemental_ward", "Guarda Elemental", 0, 18, 14)), "MAGIC_FIELD", .55f, .10f, .40f), SpellDiscipline.None),
                Discipline(WithTiming(SlowingSigils(Utility("skill_magic_slowing_sigils", "Sigilos Lentificantes", 0, 18, 8, range: 2.5f)), "MAGIC_FIELD", .70f, .10f, .40f), SpellDiscipline.None),
                Discipline(WithTiming(FlameCone(Area("skill_magic_chama_breve", "Chama Breve", 8, 2, 0, 8, 2.5f, 2.5f, "status_burn_minor")), "MAGIC_CONE", .30f, .10f, .30f), SpellDiscipline.Offensive),
                Discipline(WithTiming(IceVolley(Projectile("skill_magic_rajada_gelida", "Rajada Gélida", 6, 1, 0, 16, 6, 6, 9, DamageType.Ice, count: 3, spread: 30, status: "status_chill")), "MAGIC_PROJECTILE", .30f, 0f, .30f), SpellDiscipline.Offensive),

                LastBreath(WithTiming(Utility("skill_survival_last_breath", "Último Fôlego", 0, 0, 90), "SURVIVAL_TRIGGER", 0f, .10f, .25f)),
                RetreatSignal(WithTiming(Utility("skill_survival_sinal_retirada", "Sinal de Retirada", 15, 0, 18), "SURVIVAL_SIGNAL", .15f, .10f, .20f)),
                ImprovisedLure(WithTiming(Utility("skill_survival_isca_improvisada", "Isca Improvisada", 0, 0, 12, range: 6), "SURVIVAL_THROW", .30f, .10f, .30f)),
                EmergencyKit(WithTiming(Utility("skill_survival_kit_emergencia", "Kit de Emergência", 0, 0, 45), "SURVIVAL_CHANNEL", .80f, .10f, .25f)),
                SurvivalInstinct(WithTiming(Utility("skill_survival_instinto_sobrevivencia", "Instinto de Sobrevivência", 0, 0, 30, range: 7), "SURVIVAL_REVEAL", .25f, .10f, .20f)),
                SafeCamp(WithTiming(Utility("skill_survival_campo_seguro", "Campo Seguro", 0, 0, 60), "SURVIVAL_FIELD", .60f, .10f, .40f)),

                FieldPatch(Utility(SkillActionEffectCatalog.CraftingFieldPatchActionId, "Remendo de Campo", 0, 0, 20)),
                QuickRepair(Utility(SkillActionEffectCatalog.CraftingQuickRepairActionId, "Reparo Rápido", 0, 0, 30)),
                Irrigator(WithTiming(Utility("skill_crafting_irrigador_portatil", "Irrigador Portátil", 18, 0, 10), "CRAFT_IRRIGATION", .20f, .10f, .25f)),
                CraftBomb(WithTiming(Projectile("skill_crafting_bomba_improvisada", "Bomba Improvisada", 18, 3, 0, 0, 6, 5, 8, DamageType.Physical), "CRAFT_BOMB_THROW", .25f, .10f, .30f)),
                EfficiencyMark(WithTiming(Utility("skill_crafting_marca_eficiencia", "Marca de Eficiência", 6, 0, 30), "CRAFTING_MARK", .20f, .10f, .20f)),
            };
        }

        private static SkillActionSO Melee(string id, string name, int damage, int perRank, float stamina,
            float cooldown, float range, float arc, float lunge = 0, float knockback = 3,
            float posture = 1, int maxTargets = 0, int fullDamageTargets = 0, float falloff = 1)
        {
            var action = Create(id, name, SkillActionType.DamageSkill, damage, perRank, stamina, 0, cooldown, range);
            action.ArcDegrees = arc;
            action.DashDistance = lunge;
            action.KnockbackForce = knockback;
            action.PostureDamageMultiplier = posture;
            action.MaxTargets = maxTargets;
            action.FullDamageTargetCount = fullDamageTargets;
            action.AdditionalTargetDamageMultiplier = falloff;
            return action;
        }

        private static SkillActionSO Projectile(string id, string name, int damage, int perRank,
            float stamina, float mana, float cooldown, float range, float speed, DamageType type,
            int count = 1, float spread = 0, int pierce = 1, string status = null, bool dormant = false)
        {
            var action = Create(id, name, SkillActionType.ProjectileSkill, damage, perRank, stamina, mana, cooldown, range);
            action.ProjectileSpeed = speed;
            action.DamageType = type;
            action.ProjectileCount = count;
            action.ProjectileSpreadDegrees = spread;
            action.LinePierceCount = pierce;
            action.StatusEffectId = status ?? string.Empty;
            action.StatusApplyChance = string.IsNullOrEmpty(status) ? 0 : 1;
            action.NotYetExecutable = dormant;
            return action;
        }

        private static SkillActionSO Utility(string id, string name, float stamina, float mana,
            float cooldown, float range = 0, bool dormant = false)
        {
            var action = Create(id, name, SkillActionType.SelfBuffSkill, 0, 0, stamina, mana, cooldown, range);
            action.NotYetExecutable = dormant;
            return action;
        }

        private static SkillActionSO Area(string id, string name, int damage, int perRank,
            float stamina, float mana, float cooldown, float range, string status = null)
        {
            var action = Create(id, name, SkillActionType.AreaSkill, damage, perRank, stamina, mana, cooldown, range);
            action.StatusEffectId = status ?? string.Empty;
            action.StatusApplyChance = string.IsNullOrEmpty(status) ? 0f : 1f;
            return action;
        }

        private static SkillActionSO Create(string id, string name, SkillActionType type, int damage,
            int perRank, float stamina, float mana, float cooldown, float range)
        {
            var action = ScriptableObject.CreateInstance<SkillActionSO>();
            action.SkillActionId = id;
            action.DisplayName = name;
            action.SkillActionType = type;
            action.BaseDamage = damage;
            action.DamagePerRank = perRank;
            action.StaminaCost = stamina;
            action.ManaCost = mana;
            action.CooldownSeconds = cooldown;
            action.Range = range;
            return action;
        }

        private static SkillActionSO WithTiming(SkillActionSO action, string profileId,
            float windupSeconds, float activeSeconds, float recoverySeconds)
        {
            action.TimingProfileId = profileId;
            action.WindupSeconds = windupSeconds;
            action.ActiveSeconds = activeSeconds;
            action.RecoverySeconds = recoverySeconds;
            return action;
        }

        private static SkillActionSO Discipline(SkillActionSO action, SpellDiscipline discipline)
        {
            action.SpellDiscipline = discipline;
            return action;
        }

        private static SkillActionSO AsLeap(SkillActionSO action, float distance)
        {
            action.DashDistance = 0f;
            action.LeapDistance = Mathf.Max(0f, distance);
            action.SkillActionType = SkillActionType.LeapSkill;
            return action;
        }

        private static SkillActionSO Charged(SkillActionSO action)
        {
            action.ChargeMinimumHoldSeconds = .20f;
            action.ChargeTimeSeconds = 1.10f;
            action.PostureDamageMultiplier = .5f;
            action.ChargeMaximumDamage = 20;
            action.ChargeMaximumRange = 9f;
            action.ChargeMaximumPostureDamageMultiplier = 1.25f;
            action.ChargeMaximumStaminaCost = 26f;
            action.NotYetExecutable = false;
            return action;
        }

        private static SkillActionSO MarkedPrey(SkillActionSO action)
        {
            action.EffectDurationSeconds = 6f;
            action.EffectDurationPerRank = 2f;
            action.RangedDamageBonusFraction = .08f;
            action.RangedDamageBonusPerRank = .04f;
            return action;
        }

        private static SkillActionSO IceBind(SkillActionSO action)
        {
            action.EffectDurationSeconds = 3f;
            action.EffectDurationPerRank = 1f;
            action.ControlStrengthByRank = new[] { .50f, .50f, .50f };
            action.StrongSlowFraction = .70f;
            action.StrongSlowDurationSeconds = 1f;
            return action;
        }

        private static SkillActionSO IceVolley(SkillActionSO action)
        {
            action.EffectDurationSeconds = 3f;
            action.ControlStrengthByRank = new[] { .50f, .50f, .50f };
            return action;
        }

        private static SkillActionSO FlameCone(SkillActionSO action)
        {
            action.DamageType = DamageType.Fire;
            action.ArcDegrees = 70f;
            action.MaxTargets = 4;
            action.FullDamageTargetCount = 1;
            action.AdditionalTargetDamageMultiplier = .7f;
            return action;
        }

        private static SkillActionSO ToxicCloud(SkillActionSO action)
        {
            action.DamageType = DamageType.Toxic;
            action.EffectDurationSeconds = 4f;
            action.PulseCount = 4;
            action.MaxTargets = 4;
            action.TargetingRange = 6f;
            return action;
        }

        private static SkillActionSO LightningChain(SkillActionSO action)
        {
            action.DamageType = DamageType.Lightning;
            action.MaxTargets = 4;
            action.ChainJumpRange = 3f;
            action.TargetDamageMultipliers = new[] { 1f, .75f, .55f, .40f };
            action.PostureDamageMultiplier = .5f;
            return action;
        }

        private static SkillActionSO ElementalWard(SkillActionSO action)
        {
            action.EffectDurationSeconds = 3f;
            action.EffectDurationPerRank = 1f;
            action.ControlStrengthByRank = new[] { .25f, .32f, .40f };
            action.EffectHitCharges = 2;
            action.NotYetExecutable = false;
            return action;
        }

        private static SkillActionSO SlowingSigils(SkillActionSO action)
        {
            action.StatusEffectId = "status_slow";
            action.EffectDurationSeconds = 4f;
            action.EffectDurationPerRank = 1f;
            action.ControlStrengthByRank = new[] { .20f, .24f, .28f, .32f, .35f };
            return action;
        }

        private static SkillActionSO LastBreath(SkillActionSO action)
        {
            action.EffectDurationSeconds = 5f;
            action.EffectMagnitudeByRank = new[] { .25f, .30f, .35f, .40f, .45f };
            return action;
        }

        private static SkillActionSO RetreatSignal(SkillActionSO action)
        {
            action.EffectMagnitudeByRank = new[] { .30f, .30f, .30f };
            action.SecondaryMagnitudeByRank = new[] { .12f, .12f, .12f };
            action.SecondaryDurationByRank = new[] { 5f, 6f, 7f };
            return action;
        }

        private static SkillActionSO ImprovisedLure(SkillActionSO action)
        {
            action.EffectRadius = 5f;
            action.MaxTargets = 5;
            action.EffectMagnitudeByRank = new[] { 4f, 6f, 8f };
            action.SecondaryDurationByRank = new[] { 1f, 1.5f, 2f };
            return action;
        }

        private static SkillActionSO EmergencyKit(SkillActionSO action)
        {
            action.EffectMagnitudeByRank = new[] { .18f, .21f, .24f, .27f, .30f };
            return action;
        }

        private static SkillActionSO SurvivalInstinct(SkillActionSO action)
        {
            action.EffectRadius = 7f;
            action.SecondaryDurationByRank = new[] { 4f, 5f, 6f, 7f, 8f };
            return action;
        }

        private static SkillActionSO SafeCamp(SkillActionSO action)
        {
            action.EffectRadius = 2.5f;
            action.EffectMagnitudeByRank = new[] { .50f, .50f, .50f, .50f, .50f };
            action.SecondaryMagnitudeByRank = new[] { .50f, .50f, .50f, .50f, .50f };
            action.SecondaryDurationByRank = new[] { 8f, 10f, 12f, 14f, 16f };
            return action;
        }

        private static SkillActionSO EfficiencyMark(SkillActionSO action)
        {
            action.EffectRadius = 2.5f;
            action.EffectMagnitudeByRank = new[] { .20f, .25f, .30f };
            action.SecondaryDurationByRank = new[] { 12f, 16f, 20f };
            return action;
        }

        private static SkillActionSO FieldPatch(SkillActionSO action)
        {
            action.EffectMagnitudeByRank = new[] { .10f, .15f, .20f };
            action.SecondaryMagnitudeByRank = new[] { .60f, .60f, .60f };
            return WithTiming(action, "CRAFT_FIELD_REPAIR", 0f, .10f, .20f);
        }

        private static SkillActionSO QuickRepair(SkillActionSO action)
        {
            action.SecondaryMagnitudeByRank = new[] { .85f, .85f, .85f };
            action.SecondaryDurationByRank = new[] { 1.2f, 1.0f, .8f };
            return WithTiming(action, "CRAFT_REPAIR_CHANNEL", 1.2f, .10f, .25f);
        }

        private static SkillActionSO Irrigator(SkillActionSO action)
        {
            action.EffectMagnitudeByRank = new[] { 3f, 4f, 5f };
            action.NotYetExecutable = false;
            return action;
        }

        private static SkillActionSO CraftBomb(SkillActionSO action)
        {
            action.EffectRadius = CraftBombRules.ExplosionRadius;
            action.MaxTargets = CraftBombRules.MaximumTargets;
            action.NotYetExecutable = false;
            return action;
        }
    }
}
