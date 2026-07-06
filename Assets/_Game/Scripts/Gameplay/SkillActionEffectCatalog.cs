using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>Canonical, Unity-free mapping from equipped action ids to runtime effect ids.</summary>
    public static class SkillActionEffectCatalog
    {
        private static readonly IReadOnlyDictionary<string, string> Mappings =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { "skill_crafting_field_patch", "crafting.field_patch" },
                { "skill_melee_offhand_cut", "combat.melee.offhand_cut" },
                { "skill_melee_battle_dash", "combat.melee.battle_dash" },
                { "skill_melee_leap_attack", "combat.melee.leap_attack" },
                { "skill_melee_whirl_cut", "combat.melee.whirl_cut" },
                { "skill_ranged_charged_shot", "combat.ranged.charged_shot" },
                { "skill_ranged_line_piercer", "combat.ranged.line_piercer" },
                { "skill_ranged_multishot_fan", "combat.ranged.multishot_fan" },
                { "skill_ranged_bleeding_arrow", "combat.ranged.bleeding_arrow" },
                { "skill_ranged_marked_prey", "combat.ranged.marked_prey" },
                { "skill_magic_fire_spark", "combat.magic.fire_spark" },
                { "skill_magic_ice_bind", "combat.magic.ice_bind" },
                { "skill_magic_toxic_cloud", "combat.magic.toxic_cloud" },
                { "skill_magic_lightning_chain", "combat.magic.lightning_chain" },
                { "skill_magic_elemental_ward", "combat.magic.elemental_ward" },
                { "skill_magic_slowing_sigils", "combat.magic.slowing_sigils" },
                { "skill_melee_avanco_aco", "melee.avanco_aco" },
                { "skill_melee_grito_desafio", "melee.grito_desafio" },
                { "skill_melee_investida_quebra_guarda", "melee.investida_quebra_guarda" },
                { "skill_magic_chama_breve", "magic.chama_breve" },
                { "skill_magic_rajada_gelida", "magic.rajada_gelida" },
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
