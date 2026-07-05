using System.Collections.Generic;
using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// spec_enemy_attack_kits_v1 — tabela de kits (Normal/Especial por criatura) embutida no
    /// generator, no padrao de CreateEnemyActionsAndSets (dados no codigo, nao parseados de
    /// Markdown em runtime). Cobre os ~121 donos de kit: 113 fichas canonicas (PARTE B do
    /// catalogo, ENEMY_ATTACK_CATALOG_DIRECTION_v1.0 §4B) + 7 NOVAS do Roster (PARTE A §4) +
    /// enemy_meteor_ooze_king (§4B.9). Os 53 IDs de VARIANCIA do Roster (§4A) NAO tem entrada
    /// aqui — ganham o ActionSetId da mae via VarianceToMotherCrosswalk (sem kit proprio).
    /// The Four (boss_*, §4B.8) tambem NAO tem entrada — permanecem DORMANTE.
    /// </summary>
    public static partial class GenerateEnemyAttackKits
    {
        /// <summary>Expoe os enemyIds dos ~121 donos de kit (fonte unica — usado pelo validator
        /// para nao duplicar a lista em 2 lugares no codigo-fonte).</summary>
        public static IEnumerable<string> GetKitOwnerIds()
        {
            foreach (var kit in BuildKitTable())
                yield return kit.EnemyId;
        }

        private static IEnumerable<KitEntry> BuildKitTable()
        {
            // ── 7 NOVAS do Roster (catalogo PARTE A §4, secoes 4.8/4.9) ────────────────────────

            yield return Simple("enemy_ash_crawler", "atk_claw", "atk_charge", statusId: "status_burn");
            yield return Simple("enemy_blackroot_sprout", "atk_whip", "atk_debuff", statusId: "status_root");
            yield return Simple("enemy_corrupted_bone_knight", "atk_cleave", "atk_nova", statusId: "status_corruption");
            yield return Simple("enemy_hollow_stagling", "atk_slam", "atk_charge", statusId: "status_fear");
            yield return Simple("enemy_stone_rat", "atk_bite", "atk_debuff", statusId: "status_durability_stress");
            yield return Simple("enemy_frost_gnawer", "atk_bite", "atk_debuff", statusId: "status_chill");
            // Salva de Espinhos (catalogo §4.9): 3 flechas rapidas em leque, Bleed 25%. Especial
            // usa atk_bow (RangedProjectile) em vez do atk_flurry citado no catalogo — ProjectileCount
            // so tem efeito em ActionType Ranged/Cast (a "salva" e literalmente o arco disparando 3x).
            yield return SalvoKit("enemy_thorn_archer", "atk_bow", "atk_bow", statusId: "status_bleed", projectileCount: 3, spreadDegrees: 30f);

            // ── Boss legado solto (§4B.9) ──────────────────────────────────────────────────────

            yield return Simple("enemy_meteor_ooze_king", "atk_slam", "atk_summon", summonEnemyId: "enemy_spore_crawler", summonCount: 2);

            // ── BAND STONE (§4B.1) ──────────────────────────────────────────────────────────────

            yield return Special("enemy_verdant_mite", "atk_bite", "atk_buff", cfg => { cfg.AllyTargetRadius = 3f; cfg.AllyBuffStatusId = "status_haste"; });
            yield return Special("enemy_pale_grub", "atk_bite", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_cave_leaper"; cfg.SummonCount = 1; });
            yield return Simple("enemy_spore_crawler", "atk_slam", "atk_debuff", statusId: "status_poison");
            yield return Simple("enemy_goblin_scrounger", "atk_slash", "atk_debuff", statusId: "status_confusion_lite");
            yield return RangedKit("enemy_kobold_sentry", "atk_throw", "atk_scream");
            yield return Simple("enemy_mushroom_puffball", "atk_nova", "atk_debuff", statusId: "status_poison");
            yield return Simple("enemy_rot_beetle", "atk_bite", "atk_debuff");
            yield return Special("enemy_grimfang_packleader", "atk_bite", "atk_buff", cfg => { cfg.AllyTargetRadius = 4f; cfg.AllyBuffStatusId = "status_haste"; });
            yield return Special("enemy_lake_lurker", "atk_bite", "atk_debuff", cfg =>
            {
                cfg.DebuffStatusId = "status_root";
                cfg.PullDistanceTiles = 1f;
                cfg.PullFromAttackerOrigin = true;
            });
            yield return Simple("enemy_glimmer_centipede", "atk_bite", "atk_scream");
            yield return Simple("enemy_stone_burrower", "atk_bite", "atk_burrow");
            yield return Simple("enemy_roost_cave_bat", "atk_bite", "atk_scream");
            yield return Simple("enemy_bandit_scavenger", "atk_slash", "atk_scream");
            yield return Simple("enemy_cracked_golem_shard", "atk_slam", "atk_nova");
            yield return Special("enemy_burrow_matron", "atk_slam", "atk_burrow", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_scrounger_king", "atk_slash", "atk_flurry", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_cave_mite_queen", "atk_bite", "atk_breath", cfg => { }, isMiniBoss: true);

            // ── BAND FUNGAL (§4B.2) ─────────────────────────────────────────────────────────────

            yield return Simple("enemy_burrowing_maggot", "atk_bite", "atk_burrow");
            yield return Special("enemy_goblin_shaman", "atk_cast", "atk_buff", cfg => { cfg.AllyTargetRadius = 4f; cfg.AllyHealPercent = 0.15f; }, rangedNormal: true);
            yield return RangedKit("enemy_kobold_trapmaster", "atk_throw", "atk_debuff", statusId: "status_root");
            yield return Simple("enemy_orc_grunt", "atk_cleave", "atk_breath");
            yield return Simple("enemy_cave_leaper", "atk_claw", "atk_leap");
            yield return RangedKit("enemy_gravedelver_crossbowman", "atk_bow", "atk_debuff");
            yield return Special("enemy_fungal_spreader", "atk_cast", "atk_nova", cfg =>
            {
                cfg.LeavesHazard = true;
                cfg.HazardRadius = 2f;
                cfg.HazardDurationSeconds = 4f;
                cfg.HazardTickSeconds = 1f;
                cfg.HazardStatusId = "status_slow";
            }, rangedNormal: true);
            yield return Simple("enemy_gloom_moth", "atk_claw", "atk_scream");
            yield return Simple("enemy_rotcap_cluster", "atk_nova", "atk_debuff", statusId: "status_poison");
            yield return Special("enemy_mycelial_warden", "atk_whip", "atk_buff", cfg => { cfg.AllyHealPercent = 0f; cfg.AllyBuffStatusId = "status_regen"; });
            yield return Simple("enemy_goblin_shredder", "atk_slash", "atk_flurry");
            yield return Special("enemy_orc_drummer", "atk_slam", "atk_buff", cfg => { cfg.AllyTargetRadius = 4f; cfg.AllyBuffStatusId = "status_frenzy"; });
            yield return Simple("enemy_cave_stalker_cat", "atk_claw", "atk_leap");
            yield return Simple("enemy_spore_amalgam", "atk_slam", "atk_debuff", statusId: "status_slow");
            yield return Special("enemy_packlord_ruvash", "atk_cleave", "atk_buff", cfg => { cfg.AllyTargetRadius = 5f; cfg.AllyBuffStatusId = "status_frenzy"; }, isMiniBoss: true);
            yield return Special("enemy_fungal_tyrant_sprout", "atk_whip", "atk_nova", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_fungal_patriarch", "atk_whip", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_spore_crawler"; cfg.SummonCount = 2; }, isBoss: true);

            // ── BAND ICE (§4B.3) ────────────────────────────────────────────────────────────────

            yield return Special("enemy_undead_shambler", "atk_claw", "atk_debuff", cfg =>
            {
                cfg.RiseOnceEnabled = true;
                cfg.RiseOnceHpPercent = 0.25f;
                cfg.RiseOnceBlockedByDamageTypes = new[] { "fire" };
                cfg.RiseOnceCollapseSeconds = 2f;
            });
            yield return RangedKit("enemy_frost_wisp", "atk_cast", "atk_nova", statusId: "status_chill");
            yield return Simple("enemy_veilkin_skirmisher", "atk_slash", "atk_blink");
            yield return Special("enemy_mirrorfin_shoal", "atk_bite", "atk_buff", cfg => { cfg.AllyTargetRadius = 3f; cfg.AllyBuffStatusId = "status_haste"; }, statusId: "status_bleed");
            yield return Simple("enemy_orc_berserker", "atk_cleave", "atk_flurry");
            yield return Special("enemy_gravedelver_warder", "atk_slam", "atk_buff", cfg => { cfg.AllyBuffStatusId = "status_guard"; });
            yield return Special("enemy_cultist_zealot", "atk_cast", "atk_buff", cfg => { cfg.AllyTargetRadius = 4f; cfg.AllyBuffStatusId = "status_shield"; }, rangedNormal: true, statusId: "status_corruption");
            yield return Simple("enemy_rime_stalker", "atk_claw", "atk_leap", statusId: "status_chill");
            yield return RangedKit("enemy_frostshard_wisp", "atk_cast", "atk_debuff", statusId: "status_chill");
            yield return Special("enemy_crystal_hound", "atk_bite", "atk_buff", cfg => { cfg.AllyTargetRadius = 3f; cfg.AllyBuffStatusId = "status_haste"; }, statusId: "status_slow");
            yield return Simple("enemy_veilkin_iceblade", "atk_slash", "atk_debuff");
            yield return RangedKit("enemy_coldcult_preacher", "atk_cast", "atk_scream", statusId: "status_chill");
            yield return Special("enemy_frostbound_revenant", "atk_cleave", "atk_debuff", cfg =>
            {
                cfg.RiseOnceEnabled = true;
                cfg.RiseOnceHpPercent = 0.5f;
                cfg.RiseOnceBlockedByDamageTypes = new[] { "fire", "radiant" };
                cfg.RiseOnceCollapseSeconds = 2f;
            });
            yield return Simple("enemy_glacier_tick", "atk_bite", "atk_debuff", statusId: "status_chill");
            yield return Special("enemy_glacier_maw", "atk_bite", "atk_charge", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_veilkin_witch", "atk_cast", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_veilkin_skirmisher"; cfg.SummonCount = 2; }, isMiniBoss: true, rangedNormal: true);
            yield return Special("enemy_rimelock_colossus", "atk_slam", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_frost_wisp"; cfg.SummonCount = 2; }, isBoss: true);

            // ── BAND FIRE (§4B.4) ───────────────────────────────────────────────────────────────

            yield return Simple("enemy_earth_elemental_minor", "atk_slam", "atk_nova");
            yield return Simple("enemy_ember_hound", "atk_bite", "atk_debuff", statusId: "status_burn");
            yield return Simple("enemy_cave_burrower_elite", "atk_bite", "atk_burrow", isFallbackLeaveHazard: true);
            yield return Special("enemy_cinder_shade", "atk_claw", "atk_debuff", cfg =>
            {
                cfg.DebuffStatusId = string.Empty;
                cfg.PullDistanceTiles = 1f;
                cfg.PullFromAttackerOrigin = true;
            });
            yield return Special("enemy_veilkin_pyromancer", "atk_cast", "atk_nova", cfg =>
            {
                cfg.LeavesHazard = true;
                cfg.HazardRadius = 1.5f;
                cfg.HazardDurationSeconds = 4f;
                cfg.HazardTickSeconds = 1f;
                cfg.HazardStatusId = "status_burn";
            }, rangedNormal: true);
            yield return Simple("enemy_corrupted_vine_horror", "atk_whip", "atk_debuff", statusId: "status_root");
            yield return Simple("enemy_abyssal_hound", "atk_bite", "atk_blink");
            yield return Special("enemy_magma_slug", "atk_slam", "atk_debuff", cfg =>
            {
                cfg.LeavesHazard = true;
                cfg.HazardRadius = 1.2f;
                cfg.HazardDurationSeconds = 8f;
                cfg.HazardTickSeconds = 1f;
                cfg.HazardStatusId = "status_burn";
            });
            yield return Simple("enemy_ember_scorpion", "atk_claw", "atk_charge", statusId: "status_burn");
            yield return RangedKit("enemy_sulfur_wyrmling", "atk_spit", "atk_breath", statusId: "status_poison");
            yield return Simple("enemy_emberroot_horror", "atk_whip", "atk_debuff", statusId: "status_burn");
            yield return RangedKit("enemy_veilkin_pyrecaller", "atk_cast", "atk_breath", statusId: "status_burn");
            yield return Simple("enemy_steam_golem_proto", "atk_slam", "atk_breath");
            yield return Special("enemy_ashwing_matriarch", "atk_claw", "atk_leap", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_forge_tyrant_vask", "atk_slam", "atk_nova", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_cindershard_wyrm", "atk_claw", "atk_breath", cfg => { }, isBoss: true);

            // ── BAND RUINS (§4B.5) ──────────────────────────────────────────────────────────────

            yield return Special("enemy_gnome_tinkerer", "atk_throw", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_construct_sentry"; cfg.SummonCount = 1; }, rangedNormal: true);
            yield return Simple("enemy_ninrorin_phantom", "atk_claw", "atk_breath", statusId: "status_fear");
            yield return Simple("enemy_hoardmaw", "atk_bite", "atk_debuff");
            yield return Simple("enemy_construct_sentry", "atk_slam", "atk_cast");
            yield return Simple("enemy_night_haunt", "atk_claw", "atk_scream", statusId: "status_fear");
            yield return Special("enemy_ruin_warden", "atk_slam", "atk_debuff", cfg =>
            {
                cfg.PullDistanceTiles = 2f;
                cfg.PullFromAttackerOrigin = true;
            });
            yield return Simple("enemy_corrupted_orc_champion", "atk_cleave", "atk_breath", statusId: "status_corruption");
            yield return RangedKit("enemy_rune_sentry_mk2", "atk_cast", "atk_cast");
            yield return Special("enemy_mirror_golem", "atk_slam", "atk_nova", cfg => { });
            yield return Special("enemy_gravedelver_runepriest", "atk_cast", "atk_buff", cfg => { cfg.AllyTargetRadius = 4f; cfg.AllyBuffStatusId = "status_shield"; }, rangedNormal: true);
            yield return Simple("enemy_ninrorin_echo_warrior", "atk_slash", "atk_flurry");
            yield return Simple("enemy_chromatic_hoardling", "atk_bite", "atk_leap");
            yield return Simple("enemy_runic_warbeast", "atk_slam", "atk_charge");
            // S2 Salva de Canhao (catalogo §4B.5 + IMPLEMENTATION §3 atk_throw): 3 projeteis em arco.
            // Miniboss so tem 1 slot de Especial nesta tabela (S1 Investida a Vapor documentada no
            // catalogo fica de fora — simplificacao ja registrada; ver "Fora de escopo" do report).
            yield return Special("enemy_gnome_wargolem", "atk_slam", "atk_throw", cfg =>
            {
                cfg.ProjectileCount = 3;
                cfg.ProjectileSpreadAngleDegrees = 30f;
            }, isMiniBoss: true, rangedNormal: false);
            yield return Special("enemy_undead_lich_acolyte", "atk_cast", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_undead_shambler"; cfg.SummonCount = 3; }, isMiniBoss: true, rangedNormal: true);
            yield return Special("enemy_gravedelver_artificer_lord", "atk_slam", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_construct_sentry"; cfg.SummonCount = 2; }, isBoss: true);
            yield return Special("enemy_draconic_guardian", "atk_thrust", "atk_buff", cfg => { cfg.AllyBuffStatusId = "status_frenzy"; }, isBoss: true);

            // ── BAND DEEP (§4B.6) ───────────────────────────────────────────────────────────────

            yield return Simple("enemy_blackstone_thrall", "atk_claw", "atk_debuff", statusId: "status_corruption");
            yield return Simple("enemy_draconic_wyrmling", "atk_bite", "atk_breath");
            yield return Special("enemy_abyssal_lurker", "atk_bite", "atk_debuff", cfg =>
            {
                cfg.DebuffStatusId = "status_root";
                cfg.PullDistanceTiles = 2f;
                cfg.PullFromAttackerOrigin = true;
            });
            yield return Simple("enemy_deep_angler", "atk_bite", "atk_leap");
            yield return Special("enemy_earth_elemental_greater", "atk_slam", "atk_nova", cfg => { }, isMiniBoss: true);
            yield return RangedKit("enemy_whisper_of_veyraath", "atk_cast", "atk_scream", statusId: "status_confusion_lite");
            yield return Special("enemy_void_brood_larva", "atk_bite", "atk_buff", cfg => { cfg.AllyTargetRadius = 3f; cfg.AllyBuffStatusId = "status_haste"; }, statusId: "status_durability_stress");
            yield return Simple("enemy_mindbound_thrall", "atk_slash", "atk_flurry");
            yield return Simple("enemy_veilkin_voidassassin", "atk_slash", "atk_blink");
            yield return Simple("enemy_gloomspine_lurker", "atk_whip", "atk_burrow");
            yield return RangedKit("enemy_corrupt_pseudowyrm", "atk_cast", "atk_breath");
            yield return Simple("enemy_nyx_shade_elemental", "atk_claw", "atk_debuff", statusId: "status_confusion_lite");
            yield return Special("enemy_orc_warlord", "atk_cleave", "atk_charge", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_goblin_warchief", "atk_slash", "atk_scream", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_abyssal_gatekeeper", "atk_whip", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_abyssal_lurker"; cfg.SummonCount = 2; }, isBoss: true);

            // ── BAND VOID (§4B.7) ───────────────────────────────────────────────────────────────

            yield return Simple("enemy_void_husk", "atk_claw", "atk_nova", statusId: "status_corruption");
            yield return Special("enemy_veilkin_blademaster", "atk_slash", "atk_flurry", cfg => { }, isMiniBoss: true);
            yield return Special("enemy_starfall_remnant", "atk_slam", "atk_debuff", cfg =>
            {
                cfg.PullDistanceTiles = 2.5f;
                cfg.PullFromAttackerOrigin = false;
            }, isMiniBoss: true, rangedNormal: true);
            yield return Simple("enemy_silence_warden", "atk_slam", "atk_nova", statusId: "status_confusion_lite");
            yield return RangedKit("enemy_dread_choir", "atk_cast", "atk_breath", statusId: "status_fear");
            yield return Special("enemy_void_tendril_watcher", "atk_whip", "atk_cast", cfg => { }, rangedNormal: false);
            yield return Simple("enemy_reality_render", "atk_claw", "atk_charge");
            yield return Simple("enemy_veilkin_voidknight", "atk_cleave", "atk_flurry", statusId: "status_corruption");
            yield return Simple("enemy_sealed_observer", "atk_bash", "atk_nova");
            yield return RangedKit("enemy_dread_chorister", "atk_cast", "atk_scream", statusId: "status_fear");
            yield return Special("enemy_heralds_hand", "atk_slam", "atk_debuff", cfg =>
            {
                cfg.PullDistanceTiles = 2f;
                cfg.PullFromAttackerOrigin = true;
            }, isMiniBoss: true);
            yield return Special("enemy_gravelborn_twins", "atk_slam", "atk_buff", cfg => { cfg.AllyTargetRadius = 6f; cfg.AllyHealPercent = 0.01f; }, isMiniBoss: true, rangedNormal: true);
            yield return Special("enemy_void_herald", "atk_thrust", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_void_husk"; cfg.SummonCount = 2; }, isBoss: true);
            yield return Special("enemy_draconic_elder", "atk_claw", "atk_summon", cfg => { cfg.SummonEnemyId = "enemy_draconic_wyrmling"; cfg.SummonCount = 2; }, isBoss: true);

            // ── The Four (§4B.8) — DORMANTE, NAO gera kit ──────────────────────────────────────
            // boss_vel_karaum, boss_cindrathel, boss_archivist_of_silence, boss_ithryndor:
            // intencionalmente SEM entrada aqui. Mecanicas de encontro adiadas para spec de endgame.
        }

        // ── Construtores compactos de KitEntry ─────────────────────────────────────────────────

        private static KitEntry Simple(string enemyId, string normalArchetype, string specialArchetype,
            string statusId = null, bool rangedNormal = false, bool isFallbackLeaveHazard = false,
            string summonEnemyId = null, int summonCount = 0)
        {
            return new KitEntry
            {
                EnemyId = enemyId,
                NormalArchetype = normalArchetype,
                SpecialArchetype = specialArchetype,
                RangedNormal = rangedNormal,
                StatusId = statusId,
                ConfigureSpecial = so =>
                {
                    if (!string.IsNullOrEmpty(statusId))
                        so.StatusApplicationIds = new[] { statusId };
                    if (!string.IsNullOrEmpty(summonEnemyId))
                    {
                        so.SummonEnemyId = summonEnemyId;
                        so.SummonCount = summonCount > 0 ? summonCount : 1;
                    }
                    if (isFallbackLeaveHazard)
                    {
                        so.LeavesHazard = true;
                        so.HazardRadius = 1.2f;
                        so.HazardDurationSeconds = 2f;
                        so.HazardTickSeconds = 1f;
                    }
                }
            };
        }

        private static KitEntry RangedKit(string enemyId, string normalArchetype, string specialArchetype, string statusId = null)
        {
            return Simple(enemyId, normalArchetype, specialArchetype, statusId, rangedNormal: true);
        }

        /// <summary>Kit de 3 (fallback melee + ranged Normal + Especial) cujo Especial e uma salva de
        /// N projeteis em leque (spec_enemy_attack_kits_v1 follow-up). O Especial deve ser um
        /// arquetipo Ranged/Cast — ProjectileCount so tem efeito nesses ActionType.</summary>
        private static KitEntry SalvoKit(string enemyId, string normalArchetype, string specialArchetype,
            string statusId, int projectileCount, float spreadDegrees)
        {
            return Special(enemyId, normalArchetype, specialArchetype, cfg =>
            {
                cfg.ProjectileCount = projectileCount;
                cfg.ProjectileSpreadAngleDegrees = spreadDegrees;
            }, rangedNormal: true, statusId: statusId);
        }

        private static KitEntry Special(string enemyId, string normalArchetype, string specialArchetype,
            System.Action<EnemyActionSO> configureSpecial, bool rangedNormal = false, bool isMiniBoss = false,
            bool isBoss = false, string statusId = null)
        {
            return new KitEntry
            {
                EnemyId = enemyId,
                NormalArchetype = normalArchetype,
                SpecialArchetype = specialArchetype,
                RangedNormal = rangedNormal,
                StatusId = statusId,
                ConfigureSpecial = so =>
                {
                    if (!string.IsNullOrEmpty(statusId))
                        so.StatusApplicationIds = new[] { statusId };
                    configureSpecial?.Invoke(so);
                }
            };
        }

        // ── Crosswalk das 53 VARIANCIAS (§4A do catalogo) — fonte unica, usada tambem pelo validator ──

        /// <summary>Roster id (sem prefixo enemy_) -> criatura-mae no catalogo (sem prefixo enemy_).</summary>
        public static readonly IReadOnlyDictionary<string, string> VarianceToMotherCrosswalk = new Dictionary<string, string>
        {
            ["abyssal_riftstalker"] = "abyssal_lurker",
            ["abyssal_void_reaver"] = "abyssal_gatekeeper",
            ["blackstone_wyvern"] = "cindershard_wyrm",
            ["cave_bat"] = "roost_cave_bat",
            ["cave_mite"] = "verdant_mite",
            ["cinder_spitter"] = "cinder_shade",
            ["clockwork_guard"] = "construct_sentry",
            ["cold_cult_acolyte"] = "coldcult_preacher",
            ["corrupted_draconic_spawn"] = "draconic_wyrmling",
            ["corrupted_lich_shard"] = "undead_lich_acolyte",
            ["cracked_bone"] = "undead_shambler",
            ["crystal_leaper"] = "crystal_hound",
            ["draconic_ashspitter"] = "sulfur_wyrmling",
            ["draconic_elder_kin"] = "draconic_elder",
            ["draconic_void_wyrm"] = "corrupt_pseudowyrm",
            ["drow_arcane_adept"] = "veilkin_witch",
            ["drow_shadow_warden"] = "veilkin_voidknight",
            ["drow_shadowblade"] = "veilkin_skirmisher",
            ["duergar_frostdelver"] = "gravedelver_warder",
            ["duergar_shieldbreaker"] = "gravedelver_warder",
            ["ember_tick"] = "ember_scorpion",
            ["frost_wailer"] = "frost_wisp",
            ["furnace_warden"] = "forge_tyrant_vask",
            ["glassbone"] = "frostbound_revenant",
            ["gnome_gem_madcap"] = "gnome_tinkerer",
            ["gnomorin_rune_tinker"] = "gnome_tinkerer",
            ["goblin_grashnaar_scavenger"] = "goblin_scrounger",
            ["goblin_urudakh_trapper"] = "goblin_shredder",
            ["icebound_sentinel"] = "rimelock_colossus",
            ["kobold_scout"] = "kobold_sentry",
            ["lava_bulwark"] = "forge_tyrant_vask",
            ["mirror_adept"] = "mirror_golem",
            ["mossling"] = "mushroom_puffball",
            ["mycobulwark"] = "mycelial_warden",
            ["ninrorin_echo_shade"] = "ninrorin_phantom",
            ["ninrorin_phasewalker"] = "ninrorin_phantom",
            ["ninrorin_void_acolyte"] = "nyx_shade_elemental",
            ["ninrorin_void_knight"] = "veilkin_voidknight",
            ["ninrorin_void_sentinel"] = "sealed_observer",
            ["nyx_moth"] = "gloom_moth",
            ["oathless_shade"] = "night_haunt",
            ["orc_kaand_ashcaller"] = "orc_drummer",
            ["orc_kaand_berserker"] = "orc_berserker",
            ["orc_nyx_stalker"] = "orc_grunt",
            ["puzzle_golem"] = "gnome_wargolem",
            ["rootsnare"] = "corrupted_vine_horror",
            ["rune_shard"] = "rune_sentry_mk2",
            ["scorched_cultist"] = "cultist_zealot",
            ["sealed_knight"] = "ruin_warden",
            ["shadow_sentinel"] = "silence_warden",
            ["spore_imp"] = "mushroom_puffball",
            ["void_spitter"] = "void_tendril_watcher",
            ["void_tick"] = "void_brood_larva",
        };
    }
}
