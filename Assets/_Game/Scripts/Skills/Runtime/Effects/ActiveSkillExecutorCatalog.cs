using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Canonical executor composition and tuning. Values are intentionally identical to the former
    /// controller-local registrations; changing them is a balance change requiring a dedicated spec.
    /// </summary>
    public static class ActiveSkillExecutorCatalog
    {
        public static SkillEffectRegistry CreateRegistry()
            => CreateRegistry(null, null, null, null);

        public static SkillEffectRegistry CreateRegistry(EquipmentManager equipmentManager,
            ItemDatabaseSO itemDatabase, WeaponDatabaseSO weaponDatabase, SpellDatabaseSO spellDatabase)
        {
            var registry = new SkillEffectRegistry();
            RegisterAll(registry, new MeleeEquipmentGate(equipmentManager,
                new EquippedItemResolver(itemDatabase, weaponDatabase, spellDatabase)), equipmentManager);
            return registry;
        }

        public static void RegisterAll(SkillEffectRegistry registry, MeleeEquipmentGate meleeEquipmentGate = null,
            EquipmentManager equipmentManager = null)
        {
            if (registry == null) return;
            var defaults = BuildDefaultsByEffectId();

            registry.Register(new IrrigationLineSkillEffectExecutor(
                Get(defaults, IrrigationLineSkillEffectExecutor.EffectIdValue), equipmentManager));

            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.offhand_cut", "Corte com a Mao Inversa", Get(defaults, "combat.melee.offhand_cut"), meleeEquipmentGate ?? new MeleeEquipmentGate(null, null)));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.whirl_cut", "Corte Giratorio", Get(defaults, "combat.melee.whirl_cut")));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.leap_attack", "Ataque Saltante", Get(defaults, "combat.melee.leap_attack")));
            registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.battle_dash", "Avanco de Batalha", Get(defaults, "combat.melee.battle_dash")));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.avanco_aco", "Avanco de Aco", Get(defaults, "melee.avanco_aco")));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.grito_desafio", "Grito de Desafio", Get(defaults, "melee.grito_desafio")));
            registry.Register(new MeleeStrikeSkillEffectExecutor("melee.investida_quebra_guarda", "Investida Quebra-Guarda", Get(defaults, "melee.investida_quebra_guarda")));

            registry.Register(new ProjectileSkillEffectExecutor(SkillActionEffectCatalog.RangedChargedShotEffectId, "Tiro Carregado", Get(defaults, SkillActionEffectCatalog.RangedChargedShotEffectId)));
            registry.Register(new ProjectileSkillEffectExecutor(SkillActionEffectCatalog.RangedLinePiercerEffectId, "Perfurador em Linha", Get(defaults, SkillActionEffectCatalog.RangedLinePiercerEffectId)));
            registry.Register(new ProjectileSkillEffectExecutor(SkillActionEffectCatalog.RangedMultishotFanEffectId, "Leque de Flechas", Get(defaults, SkillActionEffectCatalog.RangedMultishotFanEffectId)));
            registry.Register(new ProjectileSkillEffectExecutor(SkillActionEffectCatalog.RangedBleedingArrowEffectId, "Flecha Lacerante", Get(defaults, SkillActionEffectCatalog.RangedBleedingArrowEffectId)));

            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.fire_spark", "Faisca de Fogo", Get(defaults, "combat.magic.fire_spark")));
            registry.Register(new ProjectileSkillEffectExecutor("combat.magic.ice_bind", "Prisao de Gelo", Get(defaults, "combat.magic.ice_bind")));
            registry.Register(new PersistentZoneSkillEffectExecutor("combat.magic.toxic_cloud", "Nuvem Toxica", Get(defaults, "combat.magic.toxic_cloud")));
            registry.Register(new ChainSkillEffectExecutor("combat.magic.lightning_chain", "Corrente Eletrica", Get(defaults, "combat.magic.lightning_chain")));
            registry.Register(new ConeSkillEffectExecutor("magic.chama_breve", "Chama Breve", Get(defaults, "magic.chama_breve")));
            registry.Register(new ProjectileSkillEffectExecutor("magic.rajada_gelida", "Rajada Gelida", Get(defaults, "magic.rajada_gelida")));
            registry.Register(new CraftBombSkillEffectExecutor(
                Get(defaults, CraftBombSkillEffectExecutor.EffectIdValue)));

            registry.Register(new SlowFieldSkillEffectExecutor("combat.magic.slowing_sigils", "Sigilos Lentificantes", Get(defaults, "combat.magic.slowing_sigils")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.kit_emergencia", "Kit de Emergência", Get(defaults, "survival.kit_emergencia")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.instinto_sobrevivencia", "Instinto de Sobrevivência", Get(defaults, "survival.instinto_sobrevivencia")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.campo_seguro", "Campo Seguro", Get(defaults, "survival.campo_seguro")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.last_breath", "Último Fôlego", Get(defaults, "survival.last_breath")));

            registry.Register(new MarkedPreySkillEffectExecutor(Get(defaults, SkillActionEffectCatalog.RangedMarkedPreyEffectId)));
            registry.Register(new ElementalWardSkillEffectExecutor("combat.magic.elemental_ward", "Guarda Elemental", Get(defaults, "combat.magic.elemental_ward")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.sinal_retirada", "Sinal de Retirada", Get(defaults, "survival.sinal_retirada")));
            registry.Register(new SurvivalSkillEffectExecutor("survival.isca_improvisada", "Isca Improvisada", Get(defaults, "survival.isca_improvisada")));
            registry.Register(new FieldRepairSkillEffectExecutor(SkillActionEffectCatalog.CraftingFieldPatchEffectId,
                "Remendo de Campo", Get(defaults, SkillActionEffectCatalog.CraftingFieldPatchEffectId), equipmentManager));
            registry.Register(new FieldRepairSkillEffectExecutor(SkillActionEffectCatalog.CraftingQuickRepairEffectId,
                "Reparo Rápido", Get(defaults, SkillActionEffectCatalog.CraftingQuickRepairEffectId), equipmentManager));
            registry.Register(new EfficiencyMarkSkillEffectExecutor(Get(defaults, "crafting.marca_eficiencia")));
        }

        private static Dictionary<string, SkillActionSO> BuildDefaultsByEffectId()
        {
            var result = new Dictionary<string, SkillActionSO>(System.StringComparer.Ordinal);
            foreach (var action in DefaultSkillActionCatalog.BuildAll())
                if (SkillActionEffectCatalog.TryGetEffectId(action.SkillActionId, out var effectId))
                    result[effectId] = action;
            return result;
        }

        private static SkillActionSO Get(Dictionary<string, SkillActionSO> defaults, string effectId)
            => defaults.TryGetValue(effectId, out var action) ? action : null;
    }
}
