using CindarsHope.Combat;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Canonical executor composition and tuning. Values are intentionally identical to the former
    /// controller-local registrations; changing them is a balance change requiring a dedicated spec.
    /// </summary>
    public static class ActiveSkillExecutorCatalog
    {
        public static SkillEffectRegistry CreateRegistry()
        {
            var registry = new SkillEffectRegistry();
            RegisterAll(registry);
            return registry;
        }

        public static void RegisterAll(SkillEffectRegistry registry)
        {
            if (registry == null) return;

            registry.Register(new FarmCropSkillEffectExecutor());

            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.offhand_cut", "Corte com a Mao Inversa", baseDamage: 8, range: 1.2f, arcDegrees: 140f, staminaCost: 10, cooldownSeconds: 2.5f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.whirl_cut", "Corte Giratorio", baseDamage: 10, range: 1.7f, arcDegrees: 360f, staminaCost: 22, cooldownSeconds: 6f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.leap_attack", "Ataque Saltante", baseDamage: 14, range: 1.4f, arcDegrees: 120f, staminaCost: 25, lungeDistance: 2.2f, cooldownSeconds: 7f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.battle_dash", "Avanco de Batalha", baseDamage: 8, range: 1.2f, arcDegrees: 100f, staminaCost: 20, lungeDistance: 3f, cooldownSeconds: 5f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.avanco_aco", "Avanco de Aco", baseDamage: 12, range: 1.3f, arcDegrees: 110f, staminaCost: 22, lungeDistance: 2.5f, cooldownSeconds: 6f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.grito_desafio", "Grito de Desafio", baseDamage: 6, range: 2.2f, arcDegrees: 360f, staminaCost: 18, knockbackForce: 6f, cooldownSeconds: 8f));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.investida_quebra_guarda", "Investida Quebra-Guarda", baseDamage: 16, range: 1.3f, arcDegrees: 90f, staminaCost: 26, lungeDistance: 2f, knockbackForce: 3f, cooldownSeconds: 9f, postureDamageMultiplier: 3f));

            registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.charged_shot", "Tiro Carregado", baseDamage: 20, speed: 12f, range: 9f, damageType: DamageType.Physical, resourceCost: 20, cooldownSeconds: 6f));
            registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.line_piercer", "Perfurador em Linha", baseDamage: 12, speed: 14f, range: 10f, damageType: DamageType.Physical, resourceCost: 18, maxHitsPerProjectile: 5, cooldownSeconds: 7f));
            registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.multishot_fan", "Leque de Flechas", baseDamage: 8, speed: 11f, range: 7f, damageType: DamageType.Physical, resourceCost: 24, projectileCount: 3, spreadDegrees: 28f, cooldownSeconds: 8f));
            registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.bleeding_arrow", "Flecha Lacerante", baseDamage: 14, speed: 12f, range: 8f, damageType: DamageType.Physical, resourceCost: 16, cooldownSeconds: 6f, statusEffectId: "status_bleed"));

            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.fire_spark", "Faisca de Fogo", baseDamage: 12, speed: 10f, range: 7f, damageType: DamageType.Fire, resourceCost: 10, cooldownSeconds: 3f));
            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.ice_bind", "Prisao de Gelo", baseDamage: 10, speed: 9f, range: 7f, damageType: DamageType.Ice, resourceCost: 14, cooldownSeconds: 6f, statusEffectId: "status_chill"));
            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.toxic_cloud", "Nuvem Toxica", baseDamage: 8, speed: 7f, range: 6f, damageType: DamageType.Toxic, resourceCost: 18, projectileCount: 3, spreadDegrees: 40f, cooldownSeconds: 8f, statusEffectId: "status_poison"));
            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.lightning_chain", "Corrente Eletrica", baseDamage: 12, speed: 16f, range: 9f, damageType: DamageType.Lightning, resourceCost: 20, maxHitsPerProjectile: 4, cooldownSeconds: 8f));
            registry.Register(new ProjectileSkillEffectExecutor("magic.chama_breve", "Chama Breve", baseDamage: 8, speed: 10f, range: 6f, damageType: DamageType.Fire, resourceCost: 8, cooldownSeconds: 2.5f));
            registry.Register(new ProjectileSkillEffectExecutor("magic.rajada_gelida", "Rajada Gelida", baseDamage: 6, speed: 9f, range: 6f, damageType: DamageType.Ice, resourceCost: 16, projectileCount: 3, spreadDegrees: 30f, cooldownSeconds: 6f, statusEffectId: "status_chill"));
            registry.Register(new ProjectileSkillEffectExecutor("crafting.bomba_improvisada", "Bomba Improvisada", baseDamage: 18, speed: 8f, range: 5f, damageType: DamageType.Toxic, resourceCost: 20, maxHitsPerProjectile: 3, cooldownSeconds: 12f));

            registry.Register(new SlowFieldSkillEffectExecutor("combat.magic.slowing_sigils", "Sigilos Lentificantes", statusEffectId: "status_slow", radius: 2.5f, manaCost: 18, cooldownSeconds: 8f));
            registry.Register(new SelfRestoreSkillEffectExecutor("survival.kit_emergencia", "Kit de Emergencia", restoreHp: 30, restoreStamina: 0, restoreMana: 0, cooldownSeconds: 45f));
            registry.Register(new SelfRestoreSkillEffectExecutor("survival.instinto_sobrevivencia", "Instinto de Sobrevivencia", restoreHp: 0, restoreStamina: 50, restoreMana: 0, cooldownSeconds: 30f));
            registry.Register(new SelfRestoreSkillEffectExecutor("survival.campo_seguro", "Campo Seguro", restoreHp: 15, restoreStamina: 25, restoreMana: 15, cooldownSeconds: 60f));
            registry.Register(new SelfRestoreSkillEffectExecutor("survival.last_breath", "Ultimo Folego", restoreHp: 40, restoreStamina: 0, restoreMana: 0, cooldownSeconds: 90f));

            registry.Register(new FeedbackOnlySkillEffectExecutor("combat.ranged.marked_prey", "Presa Marcada. (Sistema de marcacao pendente.)", SkillEffectCategory.Combat));
            registry.Register(new FeedbackOnlySkillEffectExecutor("combat.magic.elemental_ward", "Barreira Elemental ativada. (Sistema de ward pendente.)", SkillEffectCategory.Combat));
            registry.Register(new FeedbackOnlySkillEffectExecutor("survival.sinal_retirada", "Sinal de Retirada ativado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            registry.Register(new FeedbackOnlySkillEffectExecutor("survival.isca_improvisada", "Isca Improvisada lanÃ§ada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.field_patch", "Reparo de Campo aplicado. (Efeito de reparo pendente.)", SkillEffectCategory.Utility));
            registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.marca_eficiencia", "Marca de EficiÃªncia aplicada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
        }
    }
}
