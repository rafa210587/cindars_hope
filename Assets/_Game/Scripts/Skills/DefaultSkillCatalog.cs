using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Skills
{
    // Code-driven catalog for all 55 skill nodes and 5 trees.
    // Used by SkillTreeManager when no SO assets are wired in the inspector.
    public static class DefaultSkillCatalog
    {
        public static List<SkillNodeDataSO> BuildAllNodes()
        {
            var nodes = new List<SkillNodeDataSO>();
            nodes.AddRange(BuildMeleeNodes());
            nodes.AddRange(BuildRangedNodes());
            nodes.AddRange(BuildMagicNodes());
            nodes.AddRange(BuildSurvivalNodes());
            nodes.AddRange(BuildCraftingNodes());
            return nodes;
        }

        public static List<SkillTreeDataSO> BuildAllTrees(List<SkillNodeDataSO> allNodes)
        {
            var index = new Dictionary<string, SkillNodeDataSO>();
            foreach (var n in allNodes)
                index[n.SkillNodeId] = n;

            return new List<SkillTreeDataSO>
            {
                BuildTree("melee", "Melee", "Combate corpo a corpo: dual wield, two-handed, block, dodge, dash e leap.", index,
                    "melee_iron_grip","melee_guarded_stance","melee_dual_wield_flow","melee_offhand_cut",
                    "melee_two_handed_momentum","melee_guarded_block","melee_battle_dash","melee_leap_attack",
                    "melee_whirl_cut","melee_dodge_training","melee.avanco_aco","melee.grito_desafio",
                    "melee.investida_quebra_guarda","melee_capstone_battle_rhythm"),
                BuildTree("ranged", "Ranged", "Arco: charge, pierce, multishot e mobilidade de arqueiro.", index,
                    "ranged_steady_hand","ranged_long_sight","ranged_quick_nock","ranged_charged_shot",
                    "ranged_line_piercer","ranged_multishot_fan","ranged_bleeding_arrow","ranged_kiting_steps",
                    "ranged_marked_prey","ranged_projectile_tuning","ranged_capstone_eagle_focus"),
                BuildTree("magic", "Magic", "Magias elementais: Fire, Ice, Toxic, Lightning e Arcane.", index,
                    "magic_mana_well","magic_quick_channel","magic_arcane_edge","magic_fire_spark",
                    "magic_ice_bind","magic_toxic_cloud","magic_lightning_chain","magic_arcane_bolt_mastery",
                    "magic_elemental_ward","magic_slowing_sigils","magic.chama_breve","magic.rajada_gelida",
                    "magic_capstone_elemental_confluence"),
                BuildTree("survival", "Survival", "Cave survival: resistencias ambientais e habilidades de sobrevivencia.", index,
                    "survival_cave_lungs","survival_hard_skin","survival_low_rations","survival_toxic_sense",
                    "survival_cold_habit","survival_heat_temper","survival_status_recovery","survival_safe_step",
                    "survival_emergency_roll","survival_last_breath","survival.sinal_retirada","survival.isca_improvisada",
                    "survival.kit_emergencia","survival.instinto_sobrevivencia","survival.campo_seguro",
                    "survival_capstone_caveborn"),
                BuildTree("crafting", "Crafting", "Crafting e reparo: melhorar bancadas, reparo e utilidade de campo.", index,
                    "crafting_fast_hands","crafting_repair_care","crafting_material_eye","crafting_field_patch",
                    "crafting_station_focus","crafting_pack_order","crafting_quick_repair","crafting_salvage_method",
                    "crafting_durable_finish","crafting_shop_sense","crafting.irrigador_portatil",
                    "crafting.bomba_improvisada","crafting.mecanismo_campo","crafting.marca_eficiencia",
                    "crafting_capstone_master_artisan")
            };
        }

        private static SkillTreeDataSO BuildTree(string id, string name, string desc,
            Dictionary<string, SkillNodeDataSO> index, params string[] nodeIds)
        {
            var tree = ScriptableObject.CreateInstance<SkillTreeDataSO>();
            tree.TreeId = id;
            tree.DisplayName = name;
            tree.Description = desc;
            foreach (var nid in nodeIds)
            {
                if (index.TryGetValue(nid, out var node))
                    tree.Nodes.Add(node);
                else
                    Debug.LogWarning($"DefaultSkillCatalog: node '{nid}' not found for tree '{id}'");
            }
            tree.CapstoneNodeId = nodeIds[nodeIds.Length - 1];
            return tree;
        }

        private static SkillNodeDataSO Node(string id, string treeId, string name, string desc,
            SkillNodeType type, SkillCategory cat, bool isCapstone = false,
            int reqNodes = 0, string prereq = null, string unlockAction = null,
            params SkillPassiveModifier[] mods)
        {
            var so = ScriptableObject.CreateInstance<SkillNodeDataSO>();
            so.SkillNodeId = id;
            so.TreeId = treeId;
            so.DisplayName = name;
            so.Description = desc;
            so.NodeType = type;
            so.SkillCategory = cat;
            so.IsCapstone = isCapstone;
            so.SkillPointCost = 1;
            so.MinimumPlayerLevel = 1;
            so.RequiredPurchasedNodesInTree = reqNodes;
            if (!string.IsNullOrEmpty(prereq))
                so.PrerequisiteNodeIds.Add(prereq);
            so.UnlockedSkillActionId = unlockAction ?? string.Empty;
            foreach (var m in mods)
                so.PassiveModifiers.Add(m);
            return so;
        }

        private static SkillPassiveModifier Mod(SkillModifierType t, float v) => new SkillPassiveModifier(t, v);

        // ── MELEE ──────────────────────────────────────────────────────────────

        private static List<SkillNodeDataSO> BuildMeleeNodes() => new List<SkillNodeDataSO>
        {
            Node("melee_iron_grip", "melee", "Pegada de Ferro",
                "Attack +1 com armas melee.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                mods: Mod(SkillModifierType.AttackFlat, 1f)),

            Node("melee_guarded_stance", "melee", "Postura Guardada",
                "Defense +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "melee_iron_grip",
                mods: Mod(SkillModifierType.DefenseFlat, 1f)),

            Node("melee_dual_wield_flow", "melee", "Fluxo de Duas Lâminas",
                "Bonus de AttackSpeed com dual wield.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "melee_iron_grip",
                mods: Mod(SkillModifierType.DualWieldAttackSpeedBonus, 0.1f)),

            Node("melee_offhand_cut", "melee", "Corte da Mão Secundária",
                "Ataque curto com offhand.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_dual_wield_flow",
                unlockAction: "skill_melee_offhand_cut"),

            Node("melee_two_handed_momentum", "melee", "Ímpeto de Duas Mãos",
                "Bonus de dano para armas two-handed.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "melee_guarded_stance",
                mods: Mod(SkillModifierType.TwoHandedDamageBonus, 1f)),

            Node("melee_guarded_block", "melee", "Bloqueio Guardado",
                "Block temporário: reduz próximo dano.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_guarded_stance",
                unlockAction: "skill_melee_guarded_block"),

            Node("melee_battle_dash", "melee", "Arrancada de Combate",
                "Dash curto na direção do facing.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_two_handed_momentum",
                unlockAction: "skill_melee_battle_dash"),

            Node("melee_leap_attack", "melee", "Salto Devastador",
                "Leap attack com dano em arco.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_battle_dash",
                unlockAction: "skill_melee_leap_attack"),

            Node("melee_whirl_cut", "melee", "Corte Giratório",
                "Ataque circular ao redor do player.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_guarded_block",
                unlockAction: "skill_melee_whirl_cut"),

            Node("melee_dodge_training", "melee", "Treino de Esquiva",
                "Reduz custo/cooldown de dodge.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "melee_battle_dash",
                mods: Mod(SkillModifierType.DodgeCostReduction, 0.1f)),

            Node("melee_capstone_battle_rhythm", "melee", "Ritmo de Batalha",
                "Reduz cooldown melee após hit/kill.",
                SkillNodeType.Capstone, SkillCategory.CapstonePassive,
                isCapstone: true, reqNodes: 8,
                prereq: "melee_dodge_training"),

            // ── MELEE: Action Skill Balance Patch (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH) ──
            // Adicionar sem remover. Nao altera IDs existentes.

            Node("melee.avanco_aco", "melee", "Avanço de Aço",
                "Avanço curto ofensivo até inimigo à frente seguido de golpe. Tier 2. Respeita colisão.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee_iron_grip",
                unlockAction: "skill_melee_avanco_aco"),

            Node("melee.grito_desafio", "melee", "Grito de Desafio",
                "Provoca inimigos próximos por curta duração; aumenta estabilidade contra stagger. Tier 3.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee.avanco_aco",
                unlockAction: "skill_melee_grito_desafio"),

            Node("melee.investida_quebra_guarda", "melee", "Investida Quebra-Guarda",
                "Avanço frontal curto com alto posture damage. Stagger maior se alvo estiver bloqueando. Tier 4.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "melee.grito_desafio",
                unlockAction: "skill_melee_investida_quebra_guarda"),
        };

        // ── RANGED ─────────────────────────────────────────────────────────────

        private static List<SkillNodeDataSO> BuildRangedNodes() => new List<SkillNodeDataSO>
        {
            Node("ranged_steady_hand", "ranged", "Mão Firme",
                "BowDamage +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                mods: Mod(SkillModifierType.BowDamageFlat, 1f)),

            Node("ranged_long_sight", "ranged", "Mira Longa",
                "BowRange +0.5.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "ranged_steady_hand",
                mods: Mod(SkillModifierType.BowRangeFlat, 0.5f)),

            Node("ranged_quick_nock", "ranged", "Encaixe Rápido",
                "Melhora cooldown de arco.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "ranged_steady_hand",
                mods: Mod(SkillModifierType.AttackSpeedBonus, 0.1f)),

            Node("ranged_charged_shot", "ranged", "Disparo Carregado",
                "Carrega antes de soltar para dano/range maior.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "ranged_long_sight",
                unlockAction: "skill_ranged_charged_shot"),

            Node("ranged_line_piercer", "ranged", "Linha Perfurante",
                "Disparo reto que perfura todos em linha.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "ranged_quick_nock",
                unlockAction: "skill_ranged_line_piercer"),

            Node("ranged_multishot_fan", "ranged", "Tiro Triplo",
                "Dispara 3 flechas: centro + duas diagonais.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "ranged_charged_shot",
                unlockAction: "skill_ranged_multishot_fan"),

            Node("ranged_bleeding_arrow", "ranged", "Flecha Sangrante",
                "Aplica Bleed no alvo.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "ranged_line_piercer",
                unlockAction: "skill_ranged_bleeding_arrow"),

            Node("ranged_kiting_steps", "ranged", "Passos de Kiting",
                "Pequeno bonus de MoveSpeed após disparo.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "ranged_multishot_fan",
                mods: Mod(SkillModifierType.MoveSpeedBonus, 0.05f)),

            Node("ranged_marked_prey", "ranged", "Presa Marcada",
                "Marca alvo; próximos disparos causam bonus.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "ranged_bleeding_arrow",
                unlockAction: "skill_ranged_marked_prey"),

            Node("ranged_projectile_tuning", "ranged", "Afinação de Projétil",
                "ProjectileSpeed +1.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "ranged_kiting_steps",
                mods: Mod(SkillModifierType.BowProjectileSpeedFlat, 1f)),

            Node("ranged_capstone_eagle_focus", "ranged", "Foco da Águia",
                "BowRange +1, ProjectileSpeed +1 e bonus em skills ranged.",
                SkillNodeType.Capstone, SkillCategory.CapstonePassive,
                isCapstone: true, reqNodes: 8,
                prereq: "ranged_projectile_tuning",
                mods: new[] { Mod(SkillModifierType.BowRangeFlat, 1f), Mod(SkillModifierType.BowProjectileSpeedFlat, 1f) }),
        };

        // ── MAGIC ──────────────────────────────────────────────────────────────

        private static List<SkillNodeDataSO> BuildMagicNodes() => new List<SkillNodeDataSO>
        {
            Node("magic_mana_well", "magic", "Poço de Mana",
                "MaxMana +10.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                mods: Mod(SkillModifierType.MaxManaFlat, 10f)),

            Node("magic_quick_channel", "magic", "Canalização Rápida",
                "ManaRegen +1/s.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "magic_mana_well",
                mods: Mod(SkillModifierType.ManaRegenFlat, 1f)),

            Node("magic_arcane_edge", "magic", "Fio Arcano",
                "ArcaneDamage +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "magic_mana_well",
                mods: Mod(SkillModifierType.AttackFlat, 1f)),

            Node("magic_fire_spark", "magic", "Fagulha Ígnea",
                "Spell Fire projectile.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic_arcane_edge",
                unlockAction: "skill_magic_fire_spark"),

            Node("magic_ice_bind", "magic", "Laço de Gelo",
                "Spell Ice: dano leve e Slow.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic_quick_channel",
                unlockAction: "skill_magic_ice_bind"),

            Node("magic_toxic_cloud", "magic", "Nuvem Tóxica",
                "Área Toxic/Poison por tick.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic_fire_spark",
                unlockAction: "skill_magic_toxic_cloud"),

            Node("magic_lightning_chain", "magic", "Corrente Relâmpago",
                "Lightning que salta para alvo próximo.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic_ice_bind",
                unlockAction: "skill_magic_lightning_chain"),

            Node("magic_arcane_bolt_mastery", "magic", "Domínio do Raio Arcano",
                "Melhora ArcaneBolt.",
                SkillNodeType.UpgradeSkillAction, SkillCategory.PassiveSkill,
                prereq: "magic_arcane_edge",
                mods: Mod(SkillModifierType.AttackFlat, 1f)),

            Node("magic_elemental_ward", "magic", "Guarda Elemental",
                "Buff temporário de resistência elemental.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "magic_toxic_cloud",
                unlockAction: "skill_magic_elemental_ward"),

            Node("magic_slowing_sigils", "magic", "Sigilos Lentificantes",
                "Aplica Slow em pequena área.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "magic_lightning_chain",
                unlockAction: "skill_magic_slowing_sigils"),

            Node("magic_capstone_elemental_confluence", "magic", "Confluência Elemental",
                "Bonus em dano elemental e mana regen.",
                SkillNodeType.Capstone, SkillCategory.CapstonePassive,
                isCapstone: true, reqNodes: 8,
                prereq: "magic_slowing_sigils",
                mods: new[] { Mod(SkillModifierType.AttackFlat, 1f), Mod(SkillModifierType.ManaRegenFlat, 1f) }),

            // ── MAGIC: Action Skill Balance Patch (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH) ──
            // Adicionar sem remover. Nao altera IDs existentes.

            Node("magic.chama_breve", "magic", "Chama Breve",
                "Cone ou projétil curto de fogo. Dano moderado e chance de Burn. Tier 2.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic_fire_spark",
                unlockAction: "skill_magic_chama_breve"),

            Node("magic.rajada_gelida", "magic", "Rajada Gélida",
                "Rajada curta de gelo. Aplica Chill/slow leve por curta duração. Tier 3.",
                SkillNodeType.UnlockSpell, SkillCategory.EquippableSkill,
                prereq: "magic.chama_breve",
                unlockAction: "skill_magic_rajada_gelida"),
        };

        // ── SURVIVAL ───────────────────────────────────────────────────────────

        private static List<SkillNodeDataSO> BuildSurvivalNodes() => new List<SkillNodeDataSO>
        {
            Node("survival_cave_lungs", "survival", "Pulmões da Caverna",
                "MaxStamina +10.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                mods: Mod(SkillModifierType.MaxStaminaFlat, 10f)),

            Node("survival_hard_skin", "survival", "Pele Dura",
                "MaxHP +5.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "survival_cave_lungs",
                mods: Mod(SkillModifierType.MaxHPFlat, 5f)),

            Node("survival_low_rations", "survival", "Rações Curtas",
                "Reduz HungerDrain.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "survival_cave_lungs",
                mods: Mod(SkillModifierType.HungerDrainReduction, 0.1f)),

            Node("survival_toxic_sense", "survival", "Senso Tóxico",
                "ToxicResistance +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "survival_hard_skin",
                mods: Mod(SkillModifierType.ToxicResistanceBonus, 1f)),

            Node("survival_cold_habit", "survival", "Hábito do Frio",
                "ColdResistance +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "survival_hard_skin",
                mods: Mod(SkillModifierType.ColdResistanceBonus, 1f)),

            Node("survival_heat_temper", "survival", "Têmpera do Calor",
                "HeatResistance +1.",
                SkillNodeType.PassiveStat, SkillCategory.PassiveSkill,
                prereq: "survival_hard_skin",
                mods: Mod(SkillModifierType.HeatResistanceBonus, 1f)),

            Node("survival_status_recovery", "survival", "Recuperação Instintiva",
                "Reduz duração de Poison/Burn/Slow.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "survival_toxic_sense",
                mods: Mod(SkillModifierType.StatusDurationReduction, 0.1f)),

            Node("survival_safe_step", "survival", "Passo Seguro",
                "Reduz penalidade de terreno.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "survival_low_rations",
                mods: Mod(SkillModifierType.MoveSpeedBonus, 0.05f)),

            Node("survival_emergency_roll", "survival", "Rolamento de Emergência",
                "Dodge/utility especial.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival_safe_step",
                unlockAction: "skill_survival_emergency_roll"),

            Node("survival_last_breath", "survival", "Último Fôlego",
                "Cura/escudo emergencial com cooldown alto.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival_status_recovery",
                unlockAction: "skill_survival_last_breath"),

            Node("survival_capstone_caveborn", "survival", "Nascido da Caverna",
                "Bonus em todas as resistências e MaxStamina.",
                SkillNodeType.Capstone, SkillCategory.CapstonePassive,
                isCapstone: true, reqNodes: 8,
                prereq: "survival_last_breath",
                mods: new[] { Mod(SkillModifierType.ToxicResistanceBonus, 1f), Mod(SkillModifierType.ColdResistanceBonus, 1f), Mod(SkillModifierType.HeatResistanceBonus, 1f), Mod(SkillModifierType.MaxStaminaFlat, 5f) }),

            // ── SURVIVAL: Action Skill Balance Patch (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH) ──
            // Adicionar sem remover. Nao altera IDs existentes.

            Node("survival.sinal_retirada", "survival", "Sinal de Retirada",
                "Buff curto de evasão: reduz custo de Stamina de movimento/Dodge e cansaço gerado. Tier 2.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival_safe_step",
                unlockAction: "skill_survival_sinal_retirada"),

            Node("survival.isca_improvisada", "survival", "Isca Improvisada",
                "Lança isca que distrai criaturas simples por segundos. Em boss: sem distração total. Tier 2.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival_safe_step",
                unlockAction: "skill_survival_isca_improvisada"),

            Node("survival.kit_emergencia", "survival", "Kit de Emergência",
                "Usa kit para recuperar pequena Stamina ou reduzir cansaço fora de combate. Tier 3.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival.sinal_retirada",
                unlockAction: "skill_survival_kit_emergencia"),

            Node("survival.instinto_sobrevivencia", "survival", "Instinto de Sobrevivência",
                "Revela brevemente recursos, perigos leves e interagíveis próximos. Tier 3.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival.isca_improvisada",
                unlockAction: "skill_survival_instinto_sobrevivencia"),

            Node("survival.campo_seguro", "survival", "Campo Seguro",
                "Zona fora de combate que reduz ganho de cansaço/fome e melhora recovery leve. Tier 4.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "survival.kit_emergencia",
                unlockAction: "skill_survival_campo_seguro"),
        };

        // ── CRAFTING ───────────────────────────────────────────────────────────

        private static List<SkillNodeDataSO> BuildCraftingNodes() => new List<SkillNodeDataSO>
        {
            Node("crafting_fast_hands", "crafting", "Mãos Ágeis",
                "CraftTime -10%.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                mods: Mod(SkillModifierType.CraftTimeReductionPercent, 0.1f)),

            Node("crafting_repair_care", "crafting", "Cuidado no Reparo",
                "RepairKit recupera mais.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_fast_hands",
                mods: Mod(SkillModifierType.RepairEfficiencyBonus, 0.1f)),

            Node("crafting_material_eye", "crafting", "Olho de Material",
                "Chance futura de bonus de recurso.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_fast_hands"),

            Node("crafting_field_patch", "crafting", "Remendo de Campo",
                "Reparo rápido de campo.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting_repair_care",
                unlockAction: "skill_crafting_field_patch"),

            Node("crafting_station_focus", "crafting", "Foco de Bancada",
                "Bonus de craft em workstation.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_material_eye",
                mods: Mod(SkillModifierType.CraftTimeReductionPercent, 0.05f)),

            Node("crafting_pack_order", "crafting", "Mochila Ordenada",
                "Hook para futura organização de inventário.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_fast_hands"),

            Node("crafting_quick_repair", "crafting", "Reparo Rápido",
                "Ação de reparo com custo de material.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting_field_patch",
                unlockAction: "skill_crafting_quick_repair"),

            Node("crafting_salvage_method", "crafting", "Método de Salvage",
                "Melhor retorno ao salvage de item.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_station_focus"),

            Node("crafting_durable_finish", "crafting", "Acabamento Durável",
                "Bonus de DurabilityMax para crafted gear.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_salvage_method"),

            Node("crafting_shop_sense", "crafting", "Senso de Mercado",
                "Hook para bonus de venda/compra futuro.",
                SkillNodeType.PassiveModifier, SkillCategory.PassiveSkill,
                prereq: "crafting_pack_order"),

            Node("crafting_capstone_master_artisan", "crafting", "Mestre Artesão",
                "Craft time menor e repair efficiency maior.",
                SkillNodeType.Capstone, SkillCategory.CapstonePassive,
                isCapstone: true, reqNodes: 8,
                prereq: "crafting_durable_finish",
                mods: new[] { Mod(SkillModifierType.CraftTimeReductionPercent, 0.15f), Mod(SkillModifierType.RepairEfficiencyBonus, 0.15f) }),

            // ── CRAFTING: Action Skill Balance Patch (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH) ──
            // Adicionar sem remover. crafting_quick_repair (ID existente "Reparo Rápido") nao duplicado.
            // Nao altera IDs existentes.

            Node("crafting.irrigador_portatil", "crafting", "Irrigador Portátil",
                "Rega grupo de crop plots próximos ou alvo selecionado. Consome Stamina/carga. Tier 3.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting_quick_repair",
                unlockAction: "skill_crafting_irrigador_portatil"),

            Node("crafting.bomba_improvisada", "crafting", "Bomba Improvisada",
                "Arremessa bomba leve craftada. Dano baixo; útil para stagger e swarms. Consome carga. Tier 3.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting_quick_repair",
                unlockAction: "skill_crafting_bomba_improvisada"),

            Node("crafting.mecanismo_campo", "crafting", "Mecanismo de Campo",
                "Dispositivo temporário: puxa item próximo ou ativa mecanismo leve. Tier 4.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting.irrigador_portatil",
                unlockAction: "skill_crafting_mecanismo_campo"),

            Node("crafting.marca_eficiencia", "crafting", "Marca de Eficiência",
                "Buff curto: reduz custo Stamina de ações agrícolas/crafting próximas. Não acumula. Tier 4.",
                SkillNodeType.UnlockSkillAction, SkillCategory.EquippableSkill,
                prereq: "crafting.bomba_improvisada",
                unlockAction: "skill_crafting_marca_eficiencia"),
        };
    }
}
