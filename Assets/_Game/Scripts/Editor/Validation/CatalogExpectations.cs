using System.Collections.Generic;

namespace CindarsHope.Editor.Validation
{
    // fable_30 — Versioned expectation table for the catalog consistency validator.
    //
    // This is the SINGLE place where canonical counts / ids live in code. Every number
    // carries a source comment (doc + section) so a maintainer can trace it. When a
    // canonical catalog changes, update here; a count divergence surfaces as WARN
    // (never silent) per the spec risk-mitigation.
    //
    // IMPORTANT: count divergence is WARN, not ERROR. Only broken cross-references and
    // missing *explicitly listed* canonical ids are ERROR. The per-creature / per-item
    // nominal ids are intentionally NOT all hardcoded here (they live in the design
    // docs and will be materialized by F32/F33/F34/F29); hardcoding hundreds of ids
    // would make this table a maintenance trap. Skill-tree node ids ARE listed because
    // they already exist authoritatively in code (DefaultSkillCatalog), so they are a
    // safe, drift-resistant canonical source.

    public sealed class CatalogExpectation
    {
        // 0 = "no count expectation declared" (count check skipped for this category).
        public int ExpectedCount;
        // Canonical ids that MUST resolve to an asset when the category is generated.
        // Empty = no id-level expectation (only count is checked).
        public HashSet<string> CanonicalIds = new HashSet<string>();
    }

    public sealed class CatalogExpectationSet
    {
        private readonly Dictionary<CatalogCategory, CatalogExpectation> _byCategory
            = new Dictionary<CatalogCategory, CatalogExpectation>();

        public CatalogExpectation For(CatalogCategory category)
        {
            return _byCategory.TryGetValue(category, out var expectation) ? expectation : null;
        }

        public void Set(CatalogCategory category, CatalogExpectation expectation)
        {
            _byCategory[category] = expectation;
        }

        // Canonical expectations for v1, derived from the FABLE catalog directions.
        public static CatalogExpectationSet BuildCanonical()
        {
            var set = new CatalogExpectationSet();

            // ── ITEMS ────────────────────────────────────────────────────────────
            // Source: docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md
            //   PARTE H, decisao fechada: "~118 itens base + ~24 variantes de qualidade".
            //   Group breakdown (same doc):
            //     seeds+crops 24 (12 pares, §4) + foods 20 (§5) + potions 8 (§7)
            //     + weapon oils 4 + arrows 6 (§8) + materials 10 (§9) + essences 6 (§10)
            //     + weapons 16 (§12) + armors 6 + shields 3 (§13) + wands 3 + scrolls 4 (§14)
            //     + accessories 12 (§16) + relics 4 (§17) + animal products 4 (§18)
            //     + fish ~10 (E2.10) ~= 140 nominal lines; base canonical target = 118.
            set.Set(CatalogCategory.Items, new CatalogExpectation
            {
                ExpectedCount = 118
                // CanonicalIds intentionally empty: nominal item ids live in the doc and are
                // materialized by F32; count divergence (WARN) is the honest signal here.
            });

            // ── BESTIARY ─────────────────────────────────────────────────────────
            // Source: docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md
            //   PARTE J, decisao fechada: "Catalogo nominal: 60 criaturas de banda
            //   + 4 chefes finais (64 fichas)." 7 bands: STONE/FUNGAL/ICE/FIRE/RUINS/DEEP/VOID.
            set.Set(CatalogCategory.Bestiary, new CatalogExpectation
            {
                ExpectedCount = 64
                // CanonicalIds intentionally empty: nominal enemy ids live in the doc and are
                // materialized by F33. Duplicate BestiaryEntryId is caught by check (h).
            });

            // ── QUESTS ───────────────────────────────────────────────────────────
            // Source: docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md
            //   PARTE G §13.6: "Total v1: 20 main + 36 side + 6 moldes daily
            //   + 8 contratos + 8 secretas + 8 festivais ~= 86." 5 sources (PARTE A §1).
            set.Set(CatalogCategory.Quests, new CatalogExpectation
            {
                ExpectedCount = 86
                // CanonicalIds intentionally empty: nominal quest ids live in the doc and are
                // materialized by F34. QuestRegistry currently only holds 3 smoke-test quests,
                // so the count WARN (3/86) is expected until F34 runs — by design.
            });

            // ── SKILLS ───────────────────────────────────────────────────────────
            // Source: Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs (authoritative code)
            //   cross-checked with docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md.
            //   5 trees: melee 14 + ranged 11 + magic 13 + survival 16 + crafting 15 = 69 nodes.
            //   These ids already exist in code, so listing them here is drift-resistant.
            var skillNodeIds = new HashSet<string>
            {
                // melee (14) — DefaultSkillCatalog.BuildTree("melee", ...)
                "melee_iron_grip", "melee_guarded_stance", "melee_dual_wield_flow", "melee_offhand_cut",
                "melee_two_handed_momentum", "melee_guarded_block", "melee_battle_dash", "melee_leap_attack",
                "melee_whirl_cut", "melee_dodge_training", "melee.avanco_aco", "melee.grito_desafio",
                "melee.investida_quebra_guarda", "melee_capstone_battle_rhythm",
                // ranged (11)
                "ranged_steady_hand", "ranged_long_sight", "ranged_quick_nock", "ranged_charged_shot",
                "ranged_line_piercer", "ranged_multishot_fan", "ranged_bleeding_arrow", "ranged_kiting_steps",
                "ranged_marked_prey", "ranged_projectile_tuning", "ranged_capstone_eagle_focus",
                // magic (13)
                "magic_mana_well", "magic_quick_channel", "magic_arcane_edge", "magic_fire_spark",
                "magic_ice_bind", "magic_toxic_cloud", "magic_lightning_chain", "magic_arcane_bolt_mastery",
                "magic_elemental_ward", "magic_slowing_sigils", "magic.chama_breve", "magic.rajada_gelida",
                "magic_capstone_elemental_confluence",
                // survival (16)
                "survival_cave_lungs", "survival_hard_skin", "survival_low_rations", "survival_toxic_sense",
                "survival_cold_habit", "survival_heat_temper", "survival_status_recovery", "survival_safe_step",
                "survival_emergency_roll", "survival_last_breath", "survival.sinal_retirada", "survival.isca_improvisada",
                "survival.kit_emergencia", "survival.instinto_sobrevivencia", "survival.campo_seguro",
                "survival_capstone_caveborn",
                // crafting (15)
                "crafting_fast_hands", "crafting_repair_care", "crafting_material_eye", "crafting_field_patch",
                "crafting_station_focus", "crafting_pack_order", "crafting_quick_repair", "crafting_salvage_method",
                "crafting_durable_finish", "crafting_shop_sense", "crafting.irrigador_portatil",
                "crafting.bomba_improvisada", "crafting.mecanismo_campo", "crafting.marca_eficiencia",
                "crafting_capstone_master_artisan"
            };
            set.Set(CatalogCategory.Skills, new CatalogExpectation
            {
                ExpectedCount = 69,
                CanonicalIds = skillNodeIds
            });

            return set;
        }
    }
}
