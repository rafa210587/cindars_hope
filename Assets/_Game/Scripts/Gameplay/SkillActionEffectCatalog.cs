using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>Canonical, Unity-free mapping from equipped action ids to runtime effect ids.</summary>
    public static class SkillActionEffectCatalog
    {
        public const string RangedChargedShotActionId = "skill_ranged_charged_shot";
        public const string RangedLinePiercerActionId = "skill_ranged_line_piercer";
        public const string RangedMultishotFanActionId = "skill_ranged_multishot_fan";
        public const string RangedBleedingArrowActionId = "skill_ranged_bleeding_arrow";
        public const string RangedMarkedPreyActionId = "skill_ranged_marked_prey";
        public const string MagicFireSparkActionId = "skill_magic_fire_spark";
        public const string MagicIceBindActionId = "skill_magic_ice_bind";
        public const string MagicToxicCloudActionId = "skill_magic_toxic_cloud";
        public const string MagicLightningChainActionId = "skill_magic_lightning_chain";
        public const string MagicElementalWardActionId = "skill_magic_elemental_ward";
        public const string MagicSlowingSigilsActionId = "skill_magic_slowing_sigils";
        public const string MagicBriefFlameActionId = "skill_magic_chama_breve";
        public const string MagicIceVolleyActionId = "skill_magic_rajada_gelida";
        public const string CraftingFieldPatchActionId = "skill_crafting_field_patch";
        public const string CraftingQuickRepairActionId = "skill_crafting_quick_repair";
        public const string CraftingFieldPatchEffectId = "crafting.field_patch";
        public const string CraftingQuickRepairEffectId = "crafting.quick_repair";

        public const string RangedChargedShotEffectId = "combat.ranged.charged_shot";
        public const string RangedLinePiercerEffectId = "combat.ranged.line_piercer";
        public const string RangedMultishotFanEffectId = "combat.ranged.multishot_fan";
        public const string RangedBleedingArrowEffectId = "combat.ranged.bleeding_arrow";
        public const string RangedMarkedPreyEffectId = "combat.ranged.marked_prey";

        private static readonly IReadOnlyDictionary<string, string> Mappings =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { CraftingFieldPatchActionId, CraftingFieldPatchEffectId },
                { CraftingQuickRepairActionId, CraftingQuickRepairEffectId },
                { "skill_melee_offhand_cut", "combat.melee.offhand_cut" },
                { "skill_melee_battle_dash", "combat.melee.battle_dash" },
                { "skill_melee_leap_attack", "combat.melee.leap_attack" },
                { "skill_melee_whirl_cut", "combat.melee.whirl_cut" },
                { RangedChargedShotActionId, RangedChargedShotEffectId },
                { RangedLinePiercerActionId, RangedLinePiercerEffectId },
                { RangedMultishotFanActionId, RangedMultishotFanEffectId },
                { RangedBleedingArrowActionId, RangedBleedingArrowEffectId },
                { RangedMarkedPreyActionId, RangedMarkedPreyEffectId },
                { MagicFireSparkActionId, "combat.magic.fire_spark" },
                { MagicIceBindActionId, "combat.magic.ice_bind" },
                { MagicToxicCloudActionId, "combat.magic.toxic_cloud" },
                { MagicLightningChainActionId, "combat.magic.lightning_chain" },
                { MagicElementalWardActionId, "combat.magic.elemental_ward" },
                { MagicSlowingSigilsActionId, "combat.magic.slowing_sigils" },
                { "skill_melee_avanco_aco", "melee.avanco_aco" },
                { "skill_melee_grito_desafio", "melee.grito_desafio" },
                { "skill_melee_investida_quebra_guarda", "melee.investida_quebra_guarda" },
                { MagicBriefFlameActionId, "magic.chama_breve" },
                { MagicIceVolleyActionId, "magic.rajada_gelida" },
                { "skill_survival_sinal_retirada", "survival.sinal_retirada" },
                { "skill_survival_isca_improvisada", "survival.isca_improvisada" },
                { "skill_survival_kit_emergencia", "survival.kit_emergencia" },
                { "skill_survival_instinto_sobrevivencia", "survival.instinto_sobrevivencia" },
                { "skill_survival_campo_seguro", "survival.campo_seguro" },
                { "skill_survival_last_breath", "survival.last_breath" },
                { "skill_crafting_irrigador_portatil", "farm.crop.water_skill" },
                { "skill_crafting_bomba_improvisada", "crafting.bomba_improvisada" },
                { "skill_crafting_marca_eficiencia", "crafting.marca_eficiencia" }
            });

        public static IReadOnlyDictionary<string, string> All => Mappings;

        public static bool TryGetEffectId(string skillActionId, out string effectId)
        {
            if (string.IsNullOrWhiteSpace(skillActionId))
            {
                effectId = null;
                return false;
            }

            return Mappings.TryGetValue(skillActionId, out effectId);
        }
    }
}
